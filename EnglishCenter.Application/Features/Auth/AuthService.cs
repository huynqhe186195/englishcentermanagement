using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Features.Auth.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Auth;

public class AuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserName == request.UserName &&
                !x.IsDeleted &&
                x.Status == 1);

        if (user == null)
        {
            throw new BusinessException("Invalid username or password.");
        }

        // Tạm thời so sánh thẳng.
        // Sau này thay bằng password hasher.
        if (user.PasswordHash != request.Password)
        {
            throw new BusinessException("Invalid username or password.");
        }

        var roles = await (
            from ur in _context.UserRoles
            join r in _context.Roles on ur.RoleId equals r.Id
            where ur.UserId == user.Id && !r.IsDeleted
            select r.Code
        ).ToListAsync();

        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(
            user.Id,
            user.UserName,
            user.FullName,
            roles);

        return new LoginResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            Roles = roles
        };
    }
}
