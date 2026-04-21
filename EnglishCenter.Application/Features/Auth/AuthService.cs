using BCrypt.Net;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Auth.Dtos;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;


namespace EnglishCenter.Application.Features.Auth;

public class AuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly JwtSettings _jwtSettings;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionCacheService _permissionCacheService;
    private readonly ResetPasswordSettings _resetPasswordSettings;
    private readonly IEmailService _emailService;


    public AuthService(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IPasswordHasherService passwordHasherService,
        IOptions<JwtSettings> jwtSettings,
        IOptions<ResetPasswordSettings> resetPasswordSettings,
        ICurrentUserService currentUserService,
        IPermissionCacheService permissionCacheService,
        IEmailService emailService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _passwordHasherService = passwordHasherService;
        _jwtSettings = jwtSettings.Value;
        _resetPasswordSettings = resetPasswordSettings.Value;
        _currentUserService = currentUserService;
        _permissionCacheService = permissionCacheService;
        _emailService = emailService;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var principal = ValidateResetPasswordToken(request.Token);

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
        {
            throw new BusinessException("Invalid reset password token.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var keyword = request.EmailOrUserName.Trim().ToLower();

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                (
                    x.UserName.ToLower() == keyword ||
                    (x.Email != null && x.Email.ToLower() == keyword)
                ));

        // Không để lộ user có tồn tại hay không
        if (user == null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        var token = GenerateResetPasswordToken(user);
        var resetLink = $"{_resetPasswordSettings.ResetUrl}?token={Uri.EscapeDataString(token)}";

        var subject = "Reset your password";
        var body =
            $@"Hello {user.UserName},

            We received a request to reset your password.

            Please use the link below to reset your password:
            {resetLink}

            This link will expire in {_resetPasswordSettings.TokenExpirationMinutes} minutes.

            If you did not request this, please ignore this email.

            Best regards,
            English Center";

        await _emailService.SendAsync(user.Email, subject, body);
    }

    private ClaimsPrincipal ValidateResetPasswordToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        }, out _);

        var tokenType = principal.Claims.FirstOrDefault(x => x.Type == "token_type")?.Value;
        if (tokenType != "reset_password")
        {
            throw new BusinessException("Invalid reset password token.");
        }

        return principal;
    }

    private string GenerateResetPasswordToken(User user)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim("token_type", "reset_password")
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_resetPasswordSettings.TokenExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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

        if (user.CampusId.HasValue && user.CampusId.Value != request.CampusId)
        {
            throw new BusinessException("Invalid campus for this account.");
        }

        var isStudent = roles.Contains(RoleConstants.Student);
        if (isStudent)
        {
            var eligibleEnrollments = _context.Enrollments
                .Where(e =>
                    !e.IsDeleted &&
                    e.Student.UserId == user.Id &&
                    (e.Status == EnrollmentStatusConstants.Active
                     || e.Status == EnrollmentStatusConstants.Suspended
                     || e.Status == EnrollmentStatusConstants.Completed));

            var hasAnyEligibleEnrollment = await eligibleEnrollments.AnyAsync();
            if (hasAnyEligibleEnrollment)
            {
                var isValidStudentCampus = await eligibleEnrollments
                    .AnyAsync(e =>
                        e.Class.CampusId == request.CampusId);

                if (!isValidStudentCampus)
                {
                    throw new BusinessException("Invalid campus for this student account.");
                }
            }
        }

        var tokenCampusId = user.CampusId ?? request.CampusId;

        var permissions = await _permissionCacheService.GetPermissionsAsync(user.Id);

        var (accessToken, expiresAtUtc) = _jwtTokenService.GenerateToken(
            user.Id,
            user.UserName,
            user.FullName,
            roles,
            permissions,
            tokenCampusId);

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
            CampusId = tokenCampusId,
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

        var permissions = await _permissionCacheService.GetPermissionsAsync(user.Id);

        var tokenCampusId = user.CampusId;
        if (!tokenCampusId.HasValue && roles.Contains(RoleConstants.Student))
        {
            var studentCampusId = await (
                from e in _context.Enrollments
                where !e.IsDeleted
                      && e.Student.UserId == user.Id
                      && e.Class.CampusId.HasValue
                      && (e.Status == EnrollmentStatusConstants.Active
                          || e.Status == EnrollmentStatusConstants.Suspended
                          || e.Status == EnrollmentStatusConstants.Completed)
                select e.Class.CampusId!.Value
            ).FirstOrDefaultAsync();

            if (studentCampusId > 0)
            {
                tokenCampusId = studentCampusId;
            }
            else if (request.CampusId.HasValue && request.CampusId.Value > 0)
            {
                tokenCampusId = request.CampusId.Value;
            }
        }

        var (accessToken, expiresAtUtc) = _jwtTokenService.GenerateToken(
            user.Id,
            user.UserName,
            user.FullName,
            roles,
            permissions,
            tokenCampusId);

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
            CampusId = tokenCampusId,
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
}
