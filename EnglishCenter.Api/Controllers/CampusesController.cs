using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Campus;
using EnglishCenter.Application.Features.Campus.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireSuperAdmin")]
public class CampusesController : ControllerBase
{
    private readonly CampusService _campusService;

    public CampusesController(CampusService campusService)
    {
        _campusService = campusService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetCampusesPagingRequestDto request)
    {
        var result = await _campusService.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<CampusDto>>.SuccessResponse(result, "Get campuses successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _campusService.GetByIdAsync(id);
        return Ok(ApiResponse<CampusDetailDto>.SuccessResponse(result, "Get campus successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampusRequestDto request)
    {
        var id = await _campusService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Campus created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCampusRequestDto request)
    {
        await _campusService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Campus updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _campusService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Campus deleted successfully"));
    }
}
