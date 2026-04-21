using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Users;
using EnglishCenter.Application.Features.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireSuperAdmin")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetUsersPagingRequestDto request)
    {
        var result = await _userService.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<UserDto>>.SuccessResponse(result, "Get users successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(ApiResponse<UserDetailDto>.SuccessResponse(result, "Get user successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
    {
        var id = await _userService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "User created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequestDto request)
    {
        await _userService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "User updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _userService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "User deleted successfully"));
    }
}
