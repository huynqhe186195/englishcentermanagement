using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
    DbSet<Parent> Parents { get; }
    DbSet<StudentParent> StudentParents { get; }
    DbSet<AssignmentSubmission> AssignmentSubmissions { get; }
    DbSet<ProgressReport> ProgressReports { get; }
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
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Payment> Payments { get; }
    DbSet<ClassTeacher> ClassTeachers { get; }
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}