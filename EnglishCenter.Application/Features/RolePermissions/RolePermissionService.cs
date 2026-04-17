using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Features.RolePermissions.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.RolePermissions;

public class RolePermissionService
{
    private readonly IApplicationDbContext _context;
    private readonly IPermissionCacheService _permissionCacheService;

    public RolePermissionService(
        IApplicationDbContext context,
        IPermissionCacheService permissionCacheService)
    {
        _context = context;
        _permissionCacheService = permissionCacheService;
    }

    public async Task<List<RolePermissionDto>> GetPermissionsByRoleIdAsync(long roleId)
    {
        var roleExists = await _context.Roles
            .AnyAsync(x => x.Id == roleId && !x.IsDeleted);

        if (!roleExists)
        {
            throw new NotFoundException("Role not found.");
        }

        return await (
            from rp in _context.RolePermissions
            join p in _context.Permissions on rp.PermissionId equals p.Id
            where rp.RoleId == roleId && !p.IsDeleted
            select new RolePermissionDto
            {
                RoleId = rp.RoleId,
                PermissionId = rp.PermissionId,
                PermissionCode = p.Code,
                PermissionName = p.Name
            }
        ).ToListAsync();
    }

    public async Task AssignPermissionAsync(AssignPermissionToRoleRequestDto request)
    {
        var roleExists = await _context.Roles
            .AnyAsync(x => x.Id == request.RoleId && !x.IsDeleted);

        if (!roleExists)
        {
            throw new NotFoundException("Role not found.");
        }

        var permissionExists = await _context.Permissions
            .AnyAsync(x => x.Id == request.PermissionId && !x.IsDeleted);

        if (!permissionExists)
        {
            throw new NotFoundException("Permission not found.");
        }

        var exists = await _context.RolePermissions
            .AnyAsync(x => x.RoleId == request.RoleId && x.PermissionId == request.PermissionId);

        if (exists)
        {
            throw new BusinessException("This permission is already assigned to the role.");
        }

        _context.RolePermissions.Add(new RolePermission
        {
            RoleId = request.RoleId,
            PermissionId = request.PermissionId
        });

        await _context.SaveChangesAsync();

        await InvalidateUsersByRoleAsync(request.RoleId);
    }

    public async Task RemovePermissionAsync(long roleId, long permissionId)
    {
        var entity = await _context.RolePermissions
            .FirstOrDefaultAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);

        if (entity == null)
        {
            throw new NotFoundException("Role permission mapping not found.");
        }

        _context.RolePermissions.Remove(entity);
        await _context.SaveChangesAsync();

        await InvalidateUsersByRoleAsync(roleId);
    }

    public async Task ReplacePermissionsAsync(ReplaceRolePermissionsRequestDto request)
    {
        var roleExists = await _context.Roles
            .AnyAsync(x => x.Id == request.RoleId && !x.IsDeleted);

        if (!roleExists)
        {
            throw new NotFoundException("Role not found.");
        }

        var distinctPermissionIds = request.PermissionIds.Distinct().ToList();

        if (distinctPermissionIds.Any())
        {
            var validPermissionCount = await _context.Permissions
                .CountAsync(x => distinctPermissionIds.Contains(x.Id) && !x.IsDeleted);

            if (validPermissionCount != distinctPermissionIds.Count)
            {
                throw new BusinessException("One or more PermissionIds are invalid.");
            }
        }

        var existingMappings = await _context.RolePermissions
            .Where(x => x.RoleId == request.RoleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existingMappings);

        var newMappings = distinctPermissionIds.Select(permissionId => new RolePermission
        {
            RoleId = request.RoleId,
            PermissionId = permissionId
        });

        _context.RolePermissions.AddRange(newMappings);

        await _context.SaveChangesAsync();

        await InvalidateUsersByRoleAsync(request.RoleId);
    }

    private async Task InvalidateUsersByRoleAsync(long roleId)
    {
        var userIds = await _context.UserRoles
            .Where(x => x.RoleId == roleId)
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var userId in userIds)
        {
            _permissionCacheService.RemovePermissions(userId);
        }
    }
}