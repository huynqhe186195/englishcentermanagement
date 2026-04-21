using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Users;
using EnglishCenter.Application.Features.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/campus-admin/users")]
[Authorize(Policy = "RequireCenterAdmin")]
public class CampusAdminUsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ICurrentUserContext _currentUserContext;

    public CampusAdminUsersController(UserService userService, ICurrentUserContext currentUserContext)
    {
        _userService = userService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetUsersPagingRequestDto request)
    {
        var result = await _userService.GetPagedByCampusAsync(request, _currentUserContext.CampusId);
        return Ok(ApiResponse<PagedResult<UserDto>>.SuccessResponse(result, "Get campus users successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _userService.GetByIdInCampusAsync(id);
        return Ok(ApiResponse<UserDetailDto>.SuccessResponse(result, "Get campus user successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
    {
        var id = await _userService.CreateInCampusAsync(request, _currentUserContext.CampusId);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Campus user created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequestDto request)
    {
        await _userService.UpdateInCampusAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Campus user updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _userService.DeleteInCampusAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Campus user deleted successfully"));
    }
}
