using EnglishCenter.Application.Features.Students;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EnglishCenter.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddScoped<StudentService>();
        return services;
    }
}