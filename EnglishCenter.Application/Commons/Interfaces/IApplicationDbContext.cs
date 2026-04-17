using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Course> Courses { get; }
    DbSet<Class> Classes { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<Campus> Campuses { get; }
    DbSet<Exam> Exams { get; }
    DbSet<Assignment> Assignments { get; }
    DbSet<Score> Scores { get; }
    DbSet<Student> Students { get; }
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Discount> Discounts { get; }
    DbSet<Room> Rooms { get; }
    DbSet<ClassSession> ClassSessions { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<ClassSchedule> ClassSchedules { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}