using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Common.Security;
using EnglishCenter.Application.Features.Auth.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace EnglishCenter.Application.Features.Auth;

public class AuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly JwtSettings _jwtSettings;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IPasswordHasherService passwordHasherService,
        IOptions<JwtSettings> jwtSettings,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _passwordHasherService = passwordHasherService;
        _jwtSettings = jwtSettings.Value;
        _currentUserService = currentUserService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.UserName == request.UserName &&
                !x.IsDeleted &&
                x.Status == 1);

        if (user == null)
        {
            throw new BusinessException("Invalid username or password.");
        }

        var isValidPassword = _passwordHasherService.VerifyPassword(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            throw new BusinessException("Invalid username or password.");
        }

        var roles = await (
            from ur in _context.UserRoles
            join r in _context.Roles on ur.RoleId equals r.Id
            where ur.UserId == user.Id && !r.IsDeleted
            select r.Code
        ).ToListAsync();

        var permissions = RolePermissionMapping.GetPermissionsByRoles(roles);

        var (accessToken, expiresAtUtc) = _jwtTokenService.GenerateToken(
            user.Id,
            user.UserName,
            user.FullName,
            roles,
            permissions);

        var refreshToken = GenerateRefreshToken();

        var refreshTokenEntity = new Domain.Models.RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAtUtc = DateTime.UtcNow,
            IsUsed = false,
            CreatedByIp = ipAddress
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = expiresAtUtc,
            Roles = roles
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null)
    {
        var refreshTokenEntity = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (refreshTokenEntity == null)
        {
            throw new BusinessException("Invalid refresh token.");
        }

        if (refreshTokenEntity.RevokedAtUtc.HasValue || refreshTokenEntity.IsUsed)
        {
            throw new BusinessException("Refresh token is no longer valid.");
        }

        if (refreshTokenEntity.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new BusinessException("Refresh token has expired.");
        }

        var user = refreshTokenEntity.User;

        if (user == null || user.IsDeleted || user.Status != 1)
        {
            throw new BusinessException("User is no longer active.");
        }

        var roles = await (
            from ur in _context.UserRoles
            join r in _context.Roles on ur.RoleId equals r.Id
            where ur.UserId == user.Id && !r.IsDeleted
            select r.Code
        ).ToListAsync();

        var permissions = RolePermissionMapping.GetPermissionsByRoles(roles);

        var (accessToken, expiresAtUtc) = _jwtTokenService.GenerateToken(
            user.Id,
            user.UserName,
            user.FullName,
            roles,
            permissions);

        var newRefreshToken = GenerateRefreshToken();

        refreshTokenEntity.IsUsed = true;
        refreshTokenEntity.RevokedAtUtc = DateTime.UtcNow;
        refreshTokenEntity.ReplacedByToken = newRefreshToken;
        refreshTokenEntity.RevokedByIp = ipAddress;

        _context.RefreshTokens.Add(new Domain.Models.RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAtUtc = DateTime.UtcNow,
            IsUsed = false,
            CreatedByIp = ipAddress
        });

        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAtUtc = expiresAtUtc,
            Roles = roles
        };
    }

    public async Task LogoutAsync(LogoutRequestDto request, string? ipAddress = null)
    {
        var refreshTokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (refreshTokenEntity == null)
        {
            throw new BusinessException("Refresh token not found.");
        }

        if (refreshTokenEntity.RevokedAtUtc.HasValue)
        {
            return;
        }

        refreshTokenEntity.RevokedAtUtc = DateTime.UtcNow;
        refreshTokenEntity.RevokedByIp = ipAddress;
        refreshTokenEntity.IsUsed = true;

        await _context.SaveChangesAsync();
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync()
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new BusinessException("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted && x.Status == 1);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return new CurrentUserDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            Email = user.Email,
            Roles = _currentUserService.Roles,
            Permissions = _currentUserService.Permissions
        };
    }

    public async Task ChangePasswordAsync(ChangePasswordRequestDto request)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new BusinessException("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted && x.Status == 1);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        var isValidCurrentPassword = _passwordHasherService.VerifyPassword(request.CurrentPassword, user.PasswordHash);
        if (!isValidCurrentPassword)
        {
            throw new BusinessException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId && !x.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}