using EnglishCenter.Application.Features.Classes;
using EnglishCenter.Application.Features.Courses;
using EnglishCenter.Application.Features.Enrollments;
using EnglishCenter.Application.Features.Students;
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
        return services;
    }
}