using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EnglishCenter.Application.Features.UserRoles;
using EnglishCenter.Application.Features.UserRoles.Dtos;
using EnglishCenter.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PermissionConstants.Users.ManageRoles)]
public class UserRolesController : ControllerBase
{
    private readonly UserRoleService _userRoleService;

    public UserRolesController(UserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetByUserId(long userId)
    {
        var result = await _userRoleService.GetRolesByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignRoleToUserRequestDto request)
    {
        await _userRoleService.AssignRoleAsync(request);
        return Ok();
    }

    [HttpDelete("{userId:long}/{roleId:long}")]
    public async Task<IActionResult> Remove(long userId, long roleId)
    {
        await _userRoleService.RemoveRoleAsync(userId, roleId);
        return Ok();
    }

    [HttpPut("replace")]
    public async Task<IActionResult> Replace([FromBody] ReplaceUserRolesRequestDto request)
    {
        await _userRoleService.ReplaceRolesAsync(request);
        return Ok();
    }
}
