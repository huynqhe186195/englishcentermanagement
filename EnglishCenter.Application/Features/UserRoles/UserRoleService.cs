using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Commons.Helpers;
using EnglishCenter.Application.Features.UserRoles.Dtos;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.UserRoles;

public class UserRoleService
{
    private readonly IApplicationDbContext _context;
    private readonly IPermissionCacheService _permissionCacheService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly CampusScopeHelper _campusScopeHelper;

    public UserRoleService(
        IApplicationDbContext context,
        IPermissionCacheService permissionCacheService,
        ICurrentUserContext currentUserContext,
        CampusScopeHelper campusScopeHelper)
    {
        _context = context;
        _permissionCacheService = permissionCacheService;
        _currentUserContext = currentUserContext;
        _campusScopeHelper = campusScopeHelper;
    }

    public async Task<List<UserRoleDto>> GetRolesByUserIdAsync(long userId)
    {
        if (!_currentUserContext.IsSuperAdmin)
        {
            await _campusScopeHelper.EnsureUserInScopeAsync(userId);
        }

        var userExists = await _context.Users
            .AnyAsync(x => x.Id == userId && !x.IsDeleted);

        if (!userExists)
        {
            throw new NotFoundException("User not found.");
        }

        return await (
            from ur in _context.UserRoles
            join r in _context.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId && !r.IsDeleted
            select new UserRoleDto
            {
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                RoleCode = r.Code,
                RoleName = r.Name
            }
        ).ToListAsync();
    }

    public async Task AssignRoleAsync(AssignRoleToUserRequestDto request)
    {
        await EnsureRoleAssignmentAllowedAsync(request.UserId, new List<long> { request.RoleId });

        var userExists = await _context.Users
            .AnyAsync(x => x.Id == request.UserId && !x.IsDeleted);

        if (!userExists)
        {
            throw new NotFoundException("User not found.");
        }

        var roleExists = await _context.Roles
            .AnyAsync(x => x.Id == request.RoleId && !x.IsDeleted);

        if (!roleExists)
        {
            throw new NotFoundException("Role not found.");
        }

        var exists = await _context.UserRoles
            .AnyAsync(x => x.UserId == request.UserId && x.RoleId == request.RoleId);

        if (exists)
        {
            throw new BusinessException("This role is already assigned to the user.");
        }

        _context.UserRoles.Add(new UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId
        });

        await _context.SaveChangesAsync();

        _permissionCacheService.RemovePermissions(request.UserId);
    }

    public async Task RemoveRoleAsync(long userId, long roleId)
    {
        if (!_currentUserContext.IsSuperAdmin)
        {
            await _campusScopeHelper.EnsureUserInScopeAsync(userId);
            await EnsureRoleAssignmentAllowedAsync(userId, new List<long> { roleId });
        }

        var entity = await _context.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);

        if (entity == null)
        {
            throw new NotFoundException("User role mapping not found.");
        }

        _context.UserRoles.Remove(entity);
        await _context.SaveChangesAsync();

        _permissionCacheService.RemovePermissions(userId);
    }

    public async Task ReplaceRolesAsync(ReplaceUserRolesRequestDto request)
    {
        await EnsureRoleAssignmentAllowedAsync(request.UserId, request.RoleIds);

        var userExists = await _context.Users
            .AnyAsync(x => x.Id == request.UserId && !x.IsDeleted);

        if (!userExists)
        {
            throw new NotFoundException("User not found.");
        }

        var distinctRoleIds = request.RoleIds.Distinct().ToList();

        if (distinctRoleIds.Any())
        {
            var validRoleCount = await _context.Roles
                .CountAsync(x => distinctRoleIds.Contains(x.Id) && !x.IsDeleted);

            if (validRoleCount != distinctRoleIds.Count)
            {
                throw new BusinessException("One or more RoleIds are invalid.");
            }
        }

        var existingMappings = await _context.UserRoles
            .Where(x => x.UserId == request.UserId)
            .ToListAsync();

        _context.UserRoles.RemoveRange(existingMappings);

        var newMappings = distinctRoleIds.Select(roleId => new UserRole
        {
            UserId = request.UserId,
            RoleId = roleId
        });

        _context.UserRoles.AddRange(newMappings);

        await _context.SaveChangesAsync();

        _permissionCacheService.RemovePermissions(request.UserId);
    }

    public async Task<RoleUserImpactResultDto> GetUsersByRoleAsync(long roleId, int pageNumber = 1, int pageSize = 10)
    {
        var roleExists = await _context.Roles
            .AnyAsync(x => x.Id == roleId && !x.IsDeleted);

        if (!roleExists)
        {
            throw new NotFoundException("Role not found.");
        }

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var query = from ur in _context.UserRoles
                    join u in _context.Users on ur.UserId equals u.Id
                    where ur.RoleId == roleId && !u.IsDeleted
                    orderby u.FullName
                    select new RoleUserImpactDto
                    {
                        UserId = u.Id,
                        UserName = u.UserName,
                        FullName = u.FullName
                    };

        var totalUsers = await query.CountAsync();
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new RoleUserImpactResultDto
        {
            RoleId = roleId,
            TotalUsers = totalUsers,
            Users = users
        };
    }

    private async Task EnsureRoleAssignmentAllowedAsync(long targetUserId, List<long> roleIds)
    {
        if (_currentUserContext.IsSuperAdmin)
        {
            return;
        }

        await _campusScopeHelper.EnsureUserInScopeAsync(targetUserId);

        if (_currentUserContext.IsCenterAdmin)
        {
            var distinctRoleIds = roleIds.Distinct().ToList();
            if (!distinctRoleIds.Any())
            {
                return;
            }

            var requestedRoleCodes = await _context.Roles
                .Where(x => distinctRoleIds.Contains(x.Id) && !x.IsDeleted)
                .Select(x => x.Code)
                .ToListAsync();

            var hasUnallowedRole = requestedRoleCodes.Any(roleCode => !RoleAssignmentConstants.CampusAdminAssignableRoles.Contains(roleCode));
            if (hasUnallowedRole)
            {
                throw new BusinessException(
                    $"Center admin can only assign roles: {string.Join(", ", RoleAssignmentConstants.CampusAdminAssignableRoles)}.");
            }
        }
    }
}
