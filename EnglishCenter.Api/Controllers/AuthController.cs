using EnglishCenter.Application.Features.Auth;
using EnglishCenter.Application.Features.Auth.Dtos;
using EnglishCenter.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok("If the account exists, a password reset email has been sent.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok("Password has been reset successfully.");
    }

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request, ipAddress);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshTokenAsync(request, ipAddress);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _authService.LogoutAsync(request, ipAddress);
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var result = await _authService.GetCurrentUserAsync();
        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        await _authService.ChangePasswordAsync(request);
        return Ok();
    }
}