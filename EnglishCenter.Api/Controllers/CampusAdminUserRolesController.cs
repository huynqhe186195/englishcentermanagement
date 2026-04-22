using EnglishCenter.Application.Features.UserRoles;
using EnglishCenter.Application.Features.UserRoles.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/campus-admin/user-roles")]
[Authorize(Policy = "RequireCenterAdmin")]
public class CampusAdminUserRolesController : ControllerBase
{
    private readonly UserRoleService _userRoleService;

    public CampusAdminUserRolesController(UserRoleService userRoleService)
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
