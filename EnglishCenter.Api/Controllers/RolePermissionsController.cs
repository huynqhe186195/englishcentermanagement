using EnglishCenter.Application.Features.RolePermissions;
using EnglishCenter.Application.Features.RolePermissions.Dtos;
using EnglishCenter.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PermissionConstants.Roles.ManagePermissions)]
public class RolePermissionsController : ControllerBase
{
    private readonly RolePermissionService _rolePermissionService;

    public RolePermissionsController(RolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet("{roleId:long}")]
    public async Task<IActionResult> GetByRoleId(long roleId)
    {
        var result = await _rolePermissionService.GetPermissionsByRoleIdAsync(roleId);
        return Ok(result);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignPermissionToRoleRequestDto request)
    {
        await _rolePermissionService.AssignPermissionAsync(request);
        return Ok();
    }

    [HttpDelete("{roleId:long}/{permissionId:long}")]
    public async Task<IActionResult> Remove(long roleId, long permissionId)
    {
        await _rolePermissionService.RemovePermissionAsync(roleId, permissionId);
        return Ok();
    }

    [HttpPut("replace")]
    public async Task<IActionResult> Replace([FromBody] ReplaceRolePermissionsRequestDto request)
    {
        await _rolePermissionService.ReplacePermissionsAsync(request);
        return Ok();
    }
}