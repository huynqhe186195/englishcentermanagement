using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Persistence.Seed;

public class IdentitySeeder
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasherService _passwordHasherService;

    public IdentitySeeder(
        IApplicationDbContext context,
        IPasswordHasherService passwordHasherService)
    {
        _context = context;
        _passwordHasherService = passwordHasherService;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedPermissionsAsync();
        await SeedUsersAsync();
        await SeedUserRolesAsync();
        await SeedRolePermissionsAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new List<Role>
        {
            new() { Code = RoleConstants.SuperAdmin, Name = "Super Admin", IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new() { Code = RoleConstants.CenterAdmin, Name = "Center Admin", IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new() { Code = RoleConstants.Staff, Name = "Staff", IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new() { Code = RoleConstants.Teacher, Name = "Teacher", IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new() { Code = RoleConstants.Parent, Name = "Parent", IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new() { Code = RoleConstants.Student, Name = "Student", IsDeleted = false, CreatedAt = DateTime.UtcNow }
        };

        foreach (var role in roles)
        {
            var exists = await _context.Roles.AnyAsync(x => x.Code == role.Code);
            if (!exists)
            {
                _context.Roles.Add(role);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        var users = new List<User>
        {
            new()
            {
                UserName = "superadmin",
                FullName = "System Super Admin",
                Email = "superadmin@englishcenter.local",
                PasswordHash = _passwordHasherService.HashPassword("123456"),
                Status = 1,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                UserName = "admin02",
                FullName = "Center Admin Two",
                Email = "admin02@englishcenter.local",
                PasswordHash = _passwordHasherService.HashPassword("123456"),
                Status = 1,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                UserName = "staff02",
                FullName = "Center Staff Two",
                Email = "staff02@englishcenter.local",
                PasswordHash = _passwordHasherService.HashPassword("123456"),
                Status = 1,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                UserName = "teacher02",
                FullName = "Teacher Two",
                Email = "teacher02@englishcenter.local",
                PasswordHash = _passwordHasherService.HashPassword("123456"),
                Status = 1,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                UserName = "student02",
                FullName = "Student Two",
                Email = "student02@englishcenter.local",
                PasswordHash = _passwordHasherService.HashPassword("123456"),
                Status = 1,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var user in users)
        {
            var exists = await _context.Users.AnyAsync(x => x.UserName == user.UserName);
            if (!exists)
            {
                _context.Users.Add(user);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedUserRolesAsync()
    {
        var roleMap = await _context.Roles.ToDictionaryAsync(x => x.Code, x => x.Id);
        var userMap = await _context.Users.ToDictionaryAsync(x => x.UserName, x => x.Id);

        var mappings = new List<(string UserName, string RoleCode)>
        {
            ("superadmin", RoleConstants.SuperAdmin),
            ("admin", RoleConstants.CenterAdmin),
            ("staff01", RoleConstants.Staff),
            ("teacher01", RoleConstants.Teacher)
        };

        foreach (var mapping in mappings)
        {
            if (!userMap.ContainsKey(mapping.UserName) || !roleMap.ContainsKey(mapping.RoleCode))
                continue;

            var userId = userMap[mapping.UserName];
            var roleId = roleMap[mapping.RoleCode];

            var exists = await _context.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == roleId);
            if (!exists)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedRolePermissionsAsync()
    {
        var roleMap = await _context.Roles.ToDictionaryAsync(x => x.Code, x => x.Id);
        var permissionMap = await _context.Permissions.ToDictionaryAsync(x => x.Code, x => x.Id);

        var mappings = new Dictionary<string, List<string>>
        {
            [RoleConstants.SuperAdmin] = permissionMap.Keys.ToList(),
            [RoleConstants.CenterAdmin] = permissionMap.Keys.ToList(),

            [RoleConstants.Staff] =
            [
                PermissionConstants.Students.View,
            PermissionConstants.Students.Create,
            PermissionConstants.Students.Update,
            PermissionConstants.Courses.View,
            PermissionConstants.Attendance.View
            ],

            [RoleConstants.Teacher] =
            [
                PermissionConstants.Attendance.View,
            PermissionConstants.Attendance.Mark
            ]
        };

        foreach (var roleEntry in mappings)
        {
            if (!roleMap.TryGetValue(roleEntry.Key, out var roleId))
                continue;

            foreach (var permissionCode in roleEntry.Value)
            {
                if (!permissionMap.TryGetValue(permissionCode, out var permissionId))
                    continue;

                var exists = await _context.RolePermissions
                    .AnyAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);

                if (!exists)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedPermissionsAsync()
    {
        var permissions = new List<Permission>
    {
        new() { Code = PermissionConstants.Students.View, Name = "View students", GroupName = "Students", IsDeleted = false, CreatedAt = DateTime.UtcNow },
        new() { Code = PermissionConstants.Students.Create, Name = "Create students", GroupName = "Students", IsDeleted = false, CreatedAt = DateTime.UtcNow },
        new() { Code = PermissionConstants.Students.Update, Name = "Update students", GroupName = "Students", IsDeleted = false, CreatedAt = DateTime.UtcNow },
        new() { Code = PermissionConstants.Students.Delete, Name = "Delete students", GroupName = "Students", IsDeleted = false, CreatedAt = DateTime.UtcNow },

        new() { Code = PermissionConstants.Courses.View, Name = "View courses", GroupName = "Courses", IsDeleted = false, CreatedAt = DateTime.UtcNow },
        new() { Code = PermissionConstants.Courses.Create, Name = "Create courses", GroupName = "Courses", IsDeleted = false, CreatedAt = DateTime.UtcNow },
        new() { Code = PermissionConstants.Courses.Update, Name = "Update courses", GroupName = "Courses", IsDeleted = false, CreatedAt = DateTime.UtcNow },
        new() { Code = PermissionConstants.Courses.Delete, Name = "Delete courses", GroupName = "Courses", IsDeleted = false, CreatedAt = DateTime.UtcNow },

        new() { Code = PermissionConstants.Attendance.View, Name = "View attendance", GroupName = "Attendance", IsDeleted = false, CreatedAt = DateTime.UtcNow },
        new() { Code = PermissionConstants.Attendance.Mark, Name = "Mark attendance", GroupName = "Attendance", IsDeleted = false, CreatedAt = DateTime.UtcNow },

        new() { Code = PermissionConstants.Users.ResetPassword, Name = "Reset password", GroupName = "Users", IsDeleted = false, CreatedAt = DateTime.UtcNow },
        new() { Code = PermissionConstants.Users.ManageRoles, Name = "Manage user roles", GroupName = "Users", IsDeleted = false, CreatedAt = DateTime.UtcNow },

        new() { Code = PermissionConstants.Roles.ManagePermissions, Name = "Manage role permissions", GroupName = "Roles", IsDeleted = false, CreatedAt = DateTime.UtcNow }
    };

        foreach (var permission in permissions)
        {
            var exists = await _context.Permissions.AnyAsync(x => x.Code == permission.Code);
            if (!exists)
            {
                _context.Permissions.Add(permission);
            }
        }

        await _context.SaveChangesAsync();
    }
}