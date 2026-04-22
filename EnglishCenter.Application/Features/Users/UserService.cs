using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Helpers;
using EnglishCenter.Application.Features.Users.Dtos;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Users;

public class UserService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly CampusScopeHelper _campusScopeHelper;

    public UserService(
        IApplicationDbContext context,
        IMapper mapper,
        CampusScopeHelper campusScopeHelper)
    {
        _context = context;
        _mapper = mapper;
        _campusScopeHelper = campusScopeHelper;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(GetUsersPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        query = ApplyUserFilters(query, request);

        var total = await query.CountAsync();

        var items = await query
    .OrderBy(x => x.Id)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .Select(x => new UserDto
    {
        Id = x.Id,
        UserName = x.UserName,
        Email = x.Email,
        FullName = x.FullName,
        Status = x.Status,
        RoleNames = x.UserRoles.Select(ur => ur.Role.Name).ToList(),
        RoleDisplay = string.Join(", ", x.UserRoles.Select(ur => ur.Role.Name))
    })
    .ToListAsync();

        return BuildPagedResult(items, pageNumber, pageSize, total);
    }

    public async Task<PagedResult<UserDto>> GetPagedByCampusAsync(GetUsersPagingRequestDto request, long campusId)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.CampusId == campusId)
            .Where(x => !x.UserRoles.Any(ur =>
                ur.Role.Name == RoleConstants.SuperAdmin ||
                ur.Role.Name == RoleConstants.CenterAdmin))
            .AsQueryable();

        query = ApplyUserFilters(query, request);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserDto
            {
                Id = x.Id,
                UserName = x.UserName,
                Email = x.Email,
                FullName = x.FullName,
                Status = x.Status,
                RoleNames = x.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .ToListAsync();

        foreach (var item in items)
        {
            item.RoleDisplay = string.Join(", ", item.RoleNames);
        }

        return new PagedResult<UserDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<PagedResult<UserDto>> GetPagedAdminsAsync(GetUsersPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Where(x => x.UserRoles.Any(ur => ur.Role.Name == RoleConstants.CenterAdmin))
            .AsQueryable();

        query = ApplyUserFilters(query, request);

        var total = await query.CountAsync();

        var items = await query
    .OrderBy(x => x.Id)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .Select(x => new UserDto
    {
        Id = x.Id,
        UserName = x.UserName,
        Email = x.Email,
        FullName = x.FullName,
        Status = x.Status,
        RoleNames = x.UserRoles.Select(ur => ur.Role.Name).ToList(),
        RoleDisplay = string.Join(", ", x.UserRoles.Select(ur => ur.Role.Name))
    })
    .ToListAsync();

        return BuildPagedResult(items, pageNumber, pageSize, total);
    }

    public async Task<UserDetailDto> GetByIdAsync(long id)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<UserDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (entity == null)
            throw new NotFoundException("User not found.");

        return entity;
    }

    public async Task<UserDetailDto> GetByIdInCampusAsync(long id)
    {
        await _campusScopeHelper.EnsureUserInScopeAsync(id);
        await EnsureUserIsNotAdminAsync(id);
        return await GetByIdAsync(id);
    }

    public async Task<UserDetailDto> GetAdminByIdAsync(long id)
    {
        await EnsureUserIsCenterAdminAsync(id);
        return await GetByIdAsync(id);
    }

    public async Task<long> CreateAsync(CreateUserRequestDto request)
    {
        var userName = request.UserName.Trim();
        var exists = await _context.Users.AnyAsync(x => x.UserName == userName && !x.IsDeleted);
        if (exists)
            throw new BusinessException("UserName already exists.");

        var entity = _mapper.Map<User>(request);
        entity.UserName = userName;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();

        await AssignRolesIfProvidedAsync(entity.Id, request);
        return entity.Id;
    }

    public async Task<long> CreateInCampusAsync(CreateUserRequestDto request, long campusId)
    {
        await ValidateNoAdminRoleAssignmentAsync(request);

        var userName = request.UserName.Trim();
        var exists = await _context.Users.AnyAsync(x => x.UserName == userName && !x.IsDeleted);
        if (exists)
            throw new BusinessException("UserName already exists.");

        var entity = _mapper.Map<User>(request);
        entity.UserName = userName;
        entity.CampusId = campusId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();

        await AssignRolesIfProvidedAsync(entity.Id, request);
        return entity.Id;
    }

    public async Task<long> CreateAdminAsync(CreateUserRequestDto request)
    {
        await ValidateOnlyCenterAdminRoleAssignmentAsync(request);

        var userName = request.UserName.Trim();
        var exists = await _context.Users.AnyAsync(x => x.UserName == userName && !x.IsDeleted);
        if (exists)
            throw new BusinessException("UserName already exists.");

        var entity = _mapper.Map<User>(request);
        entity.UserName = userName;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();

        await AssignRolesIfProvidedAsync(entity.Id, request);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateUserRequestDto request)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null)
            throw new NotFoundException("User not found.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await ReplaceRolesIfProvidedAsync(id, request);
        return true;
    }

    public async Task<bool> UpdateInCampusAsync(long id, UpdateUserRequestDto request)
    {
        await _campusScopeHelper.EnsureUserInScopeAsync(id);
        await EnsureUserIsNotAdminAsync(id);
        await ValidateNoAdminRoleAssignmentAsync(request);

        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null)
            throw new NotFoundException("User not found.");

        // CenterAdmin không được đổi user sang campus khác
        if (request.CampusId.HasValue && request.CampusId.Value != entity.CampusId)
            throw new BusinessException("Center admin cannot move users to another campus.");

        _mapper.Map(request, entity);
        entity.CampusId = entity.CampusId; // giữ campus cũ
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await ReplaceRolesIfProvidedAsync(id, request);
        return true;
    }

    public async Task<bool> UpdateAdminAsync(long id, UpdateUserRequestDto request)
    {
        await EnsureUserIsCenterAdminAsync(id);
        await ValidateOnlyCenterAdminRoleAssignmentAsync(request);

        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null)
            throw new NotFoundException("User not found.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await ReplaceRolesIfProvidedAsync(id, request);
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null)
            throw new NotFoundException("User not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteInCampusAsync(long id)
    {
        await _campusScopeHelper.EnsureUserInScopeAsync(id);
        await EnsureUserIsNotAdminAsync(id);

        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null)
            throw new NotFoundException("User not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAdminAsync(long id)
    {
        await EnsureUserIsCenterAdminAsync(id);

        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null)
            throw new NotFoundException("User not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private IQueryable<User> ApplyUserFilters(IQueryable<User> query, GetUsersPagingRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            query = query.Where(x =>
                x.UserName.ToLower().Contains(kw) ||
                x.FullName.ToLower().Contains(kw) ||
                (x.Email != null && x.Email.ToLower().Contains(kw)));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        return query;
    }

    private PagedResult<UserDto> BuildPagedResult(
        List<UserDto> items,
        int pageNumber,
        int pageSize,
        int total)
    {
        return new PagedResult<UserDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    private async Task EnsureUserIsNotAdminAsync(long userId)
    {
        var isAdmin = await _context.Users
            .AnyAsync(x => x.Id == userId &&
                           x.UserRoles.Any(ur =>
                               ur.Role.Name == RoleConstants.SuperAdmin ||
                               ur.Role.Name == RoleConstants.CenterAdmin));

        if (isAdmin)
            throw new BusinessException("Center admin cannot manage admin accounts.");
    }

    private async Task EnsureUserIsCenterAdminAsync(long userId)
    {
        var isCenterAdmin = await _context.Users
            .AnyAsync(x => x.Id == userId &&
                           !x.IsDeleted &&
                           x.UserRoles.Any(ur => ur.Role.Name == RoleConstants.CenterAdmin));

        if (!isCenterAdmin)
            throw new BusinessException("Target user is not a center admin.");
    }

    private async Task ValidateNoAdminRoleAssignmentAsync(CreateUserRequestDto request)
    {
        var roleNames = await ResolveRoleNamesFromCreateRequestAsync(request);
        if (roleNames.Contains(RoleConstants.SuperAdmin) || roleNames.Contains(RoleConstants.CenterAdmin))
            throw new BusinessException("Center admin cannot assign admin roles.");
    }

    private async Task ValidateNoAdminRoleAssignmentAsync(UpdateUserRequestDto request)
    {
        var roleNames = await ResolveRoleNamesFromUpdateRequestAsync(request);
        if (roleNames.Contains(RoleConstants.SuperAdmin) || roleNames.Contains(RoleConstants.CenterAdmin))
            throw new BusinessException("Center admin cannot assign admin roles.");
    }

    private async Task ValidateOnlyCenterAdminRoleAssignmentAsync(CreateUserRequestDto request)
    {
        var roleNames = await ResolveRoleNamesFromCreateRequestAsync(request);
        if (roleNames.Count == 0 || roleNames.Any(x => x != RoleConstants.CenterAdmin))
            throw new BusinessException("Super admin can only create users with role CenterAdmin in this endpoint.");
    }

    private async Task ValidateOnlyCenterAdminRoleAssignmentAsync(UpdateUserRequestDto request)
    {
        var roleNames = await ResolveRoleNamesFromUpdateRequestAsync(request);
        if (roleNames.Count > 0 && roleNames.Any(x => x != RoleConstants.CenterAdmin))
            throw new BusinessException("Super admin can only manage users with role CenterAdmin in this endpoint.");
    }

    private async Task<HashSet<string>> ResolveRoleNamesFromCreateRequestAsync(CreateUserRequestDto request)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (request.RoleIds != null && request.RoleIds.Any())
        {
            var names = await _context.Roles
                .Where(x => request.RoleIds.Contains(x.Id))
                .Select(x => x.Name)
                .ToListAsync();

            foreach (var name in names)
                result.Add(name);
        }

        return result;
    }

    private async Task<HashSet<string>> ResolveRoleNamesFromUpdateRequestAsync(UpdateUserRequestDto request)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (request.RoleIds != null && request.RoleIds.Any())
        {
            var names = await _context.Roles
                .Where(x => request.RoleIds.Contains(x.Id))
                .Select(x => x.Name)
                .ToListAsync();

            foreach (var name in names)
                result.Add(name);
        }

        return result;
    }

    private async Task AssignRolesIfProvidedAsync(long userId, CreateUserRequestDto request)
    {
        if (request.RoleIds == null || !request.RoleIds.Any())
            return;

        var existingRoleIds = await _context.Roles
            .Where(x => request.RoleIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var roleId in existingRoleIds.Distinct())
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task ReplaceRolesIfProvidedAsync(long userId, UpdateUserRequestDto request)
    {
        if (request.RoleIds == null)
            return;

        var currentRoles = await _context.UserRoles
            .Where(x => x.UserId == userId)
            .ToListAsync();

        _context.UserRoles.RemoveRange(currentRoles);

        var existingRoleIds = await _context.Roles
            .Where(x => request.RoleIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var roleId in existingRoleIds.Distinct())
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
}