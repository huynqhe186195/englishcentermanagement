using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Features.UserRoles.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.UserRoles;

public class UserRoleService
{
    private readonly IApplicationDbContext _context;
    private readonly IPermissionCacheService _permissionCacheService;

    public UserRoleService(
        IApplicationDbContext context,
        IPermissionCacheService permissionCacheService)
    {
        _context = context;
        _permissionCacheService = permissionCacheService;
    }

    public async Task<List<UserRoleDto>> GetRolesByUserIdAsync(long userId)
    {
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
}
