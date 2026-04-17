using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Infrastructure.Identity;
using EnglishCenter.Infrastructure.Persistence.Context;
using EnglishCenter.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishCenter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<EnglishCenterDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MyCnn")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<EnglishCenterDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddHttpContextAccessor();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IPasswordHasherService, PasswordHasherService>();

        services.AddMemoryCache();
        services.AddScoped<IPermissionCacheService, PermissionCacheService>();

        services.AddScoped<IdentitySeeder>();
        return services;
    }
}