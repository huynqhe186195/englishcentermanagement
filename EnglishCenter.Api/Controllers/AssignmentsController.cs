using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Assignments;
using EnglishCenter.Application.Features.Assignments.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly AssignmentService _assignmentService;

    public AssignmentsController(AssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _assignmentService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResult<AssignmentDto>>.SuccessResponse(result, "Get assignments successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _assignmentService.GetByIdAsync(id);
        return Ok(ApiResponse<AssignmentDetailDto>.SuccessResponse(result, "Get assignment successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentRequestDto request)
    {
        var id = await _assignmentService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Assignment created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateAssignmentRequestDto request)
    {
        await _assignmentService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Assignment updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _assignmentService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Assignment deleted successfully"));
    }
}
