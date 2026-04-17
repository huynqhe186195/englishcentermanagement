using EnglishCenter.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EnglishCenter.Infrastructure.Identity;

public class PermissionCacheService : IPermissionCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PermissionCacheService> _logger;

    public PermissionCacheService(
        IMemoryCache memoryCache,
        IApplicationDbContext context,
        ILogger<PermissionCacheService> logger)
    {
        _memoryCache = memoryCache;
        _context = context;
        _logger = logger;
    }

    public async Task<List<string>> GetPermissionsAsync(long userId)
    {
        var cacheKey = GetCacheKey(userId);

        if (_memoryCache.TryGetValue(cacheKey, out List<string>? cachedPermissions) && cachedPermissions is not null)
        {
            _logger.LogInformation("Permission cache hit for userId {UserId}", userId);
            return cachedPermissions;
        }

        _logger.LogInformation("Permission cache miss for userId {UserId}", userId);

        var permissions = await (
            from ur in _context.UserRoles
            join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
            join p in _context.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId && !p.IsDeleted
            select p.Code
        ).Distinct().ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        _memoryCache.Set(cacheKey, permissions, cacheOptions);

        return permissions;
    }


    public void RemovePermissions(long userId)
    {
        _logger.LogInformation("Removing permission cache for userId {UserId}", userId);
        _memoryCache.Remove(GetCacheKey(userId));
    }

    private static string GetCacheKey(long userId) => $"permissions:user:{userId}";
}