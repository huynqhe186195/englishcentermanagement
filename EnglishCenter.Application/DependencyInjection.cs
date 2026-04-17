using EnglishCenter.Application.Features.Assignments;
using EnglishCenter.Application.Features.Attendance;
using EnglishCenter.Application.Features.AuditLogs;
using EnglishCenter.Application.Features.Auth;
using EnglishCenter.Application.Features.Campus;
using EnglishCenter.Application.Features.Classes;
using EnglishCenter.Application.Features.ClassSchedules;
using EnglishCenter.Application.Features.ClassSessions;
using EnglishCenter.Application.Features.Courses;
using EnglishCenter.Application.Features.Enrollments;
using EnglishCenter.Application.Features.Exams;
using EnglishCenter.Application.Features.Notifications;
using EnglishCenter.Application.Features.RolePermissions;
using EnglishCenter.Application.Features.Roles;
using EnglishCenter.Application.Features.Rooms;
using EnglishCenter.Application.Features.Scores;
using EnglishCenter.Application.Features.Students;
using EnglishCenter.Application.Features.Teachers;
using EnglishCenter.Application.Features.UserRoles;
using EnglishCenter.Application.Features.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EnglishCenter.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<StudentService>();
        services.AddScoped<ClassService>();
        services.AddScoped<CourseService>();
        services.AddScoped<EnrollmentService>();
        services.AddScoped<RoleService>();
        services.AddScoped<AssignmentService>();
        services.AddScoped<CampusService>();
        services.AddScoped<ExamService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<ScoreService>();
        services.AddScoped<UserService>();
        services.AddScoped<TeacherService>();
        services.AddScoped<RoomService>();
        services.AddScoped<ClassScheduleService>();
        services.AddScoped<ClassSessionService>();
        services.AddScoped<AttendanceService>();
        services.AddScoped<AuthService>();
        services.AddScoped<UserRoleService>();
        services.AddScoped<RolePermissionService>();
        services.AddScoped<AuditLogService>();
        return services;
    }
}