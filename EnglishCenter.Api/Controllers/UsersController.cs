using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Users;
using EnglishCenter.Application.Features.Users.Dtos;
using EnglishCenter.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.CenterAdmin}")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(UserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetUsersPagingRequestDto request)
    {
        if (_currentUserService.IsInRole(RoleConstants.SuperAdmin))
        {
            var result = await _userService.GetPagedAsync(request);
            return Ok(ApiResponse<PagedResult<UserDto>>.SuccessResponse(result, "Get users successfully"));
        }

        if (_currentUserService.IsInRole(RoleConstants.CenterAdmin))
        {
            if (!_currentUserService.CampusId.HasValue)
            {
                return BadRequest(ApiResponse<object>.FailResponse("Current admin does not have a campus assigned."));
            }

            var result = await _userService.GetPagedByCampusAsync(request, _currentUserService.CampusId.Value);
            return Ok(ApiResponse<PagedResult<UserDto>>.SuccessResponse(result, "Get campus users successfully"));
        }

        return Forbid();
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        if (_currentUserService.IsInRole(RoleConstants.SuperAdmin))
        {
            var result = await _userService.GetByIdAsync(id);
            return Ok(ApiResponse<UserDetailDto>.SuccessResponse(result, "Get user successfully"));
        }

        if (_currentUserService.IsInRole(RoleConstants.CenterAdmin))
        {
            var result = await _userService.GetByIdInCampusAsync(id);
            return Ok(ApiResponse<UserDetailDto>.SuccessResponse(result, "Get campus user successfully"));
        }

        return Forbid();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
    {
        if (_currentUserService.IsInRole(RoleConstants.SuperAdmin))
        {
            var id = await _userService.CreateAdminAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Admin user created successfully"));
        }

        if (_currentUserService.IsInRole(RoleConstants.CenterAdmin))
        {
            if (!_currentUserService.CampusId.HasValue)
            {
                return BadRequest(ApiResponse<object>.FailResponse("Current admin does not have a campus assigned."));
            }

            var id = await _userService.CreateInCampusAsync(request, _currentUserService.CampusId.Value);
            return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Campus user created successfully"));
        }

        return Forbid();
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequestDto request)
    {
        if (_currentUserService.IsInRole(RoleConstants.SuperAdmin))
        {
            await _userService.UpdateAdminAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Admin user updated successfully"));
        }

        if (_currentUserService.IsInRole(RoleConstants.CenterAdmin))
        {
            await _userService.UpdateInCampusAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Campus user updated successfully"));
        }

        return Forbid();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        if (_currentUserService.IsInRole(RoleConstants.SuperAdmin))
        {
            await _userService.DeleteAdminAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Admin user deleted successfully"));
        }

        if (_currentUserService.IsInRole(RoleConstants.CenterAdmin))
        {
            await _userService.DeleteInCampusAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Campus user deleted successfully"));
        }

        return Forbid();
    }
}