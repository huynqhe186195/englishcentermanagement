using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Roles;
using EnglishCenter.Application.Features.Roles.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireSuperAdmin")]
public class RolesController : ControllerBase
{
    private readonly RoleService _roleService;

    public RolesController(RoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _roleService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResult<RoleDto>>.SuccessResponse(result, "Get roles successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _roleService.GetByIdAsync(id);
        return Ok(ApiResponse<RoleDetailDto>.SuccessResponse(result, "Get role successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequestDto request)
    {
        var id = await _roleService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Role created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRoleRequestDto request)
    {
        await _roleService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Role updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _roleService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Role deleted successfully"));
    }
}
