using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Exams;
using EnglishCenter.Application.Features.Exams.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamsController : ControllerBase
{
    private readonly ExamService _examService;

    public ExamsController(ExamService examService)
    {
        _examService = examService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetExamsPagingRequestDto request)
    {
        var result = await _examService.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<ExamDto>>.SuccessResponse(result, "Get exams successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _examService.GetByIdAsync(id);
        return Ok(ApiResponse<ExamDetailDto>.SuccessResponse(result, "Get exam successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExamRequestDto request)
    {
        // attach current user id from token if available
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value
                          ?? User.FindFirst("id")?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var uid))
        {
            request.CreatedByUserId = uid;
        }

        // create with validation to avoid student schedule conflicts
        var id = await _examService.CreateWithValidationAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Exam created successfully"));
    }

    [HttpGet("available-slots")]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] long classId, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int durationMinutes, [FromQuery] int stepMinutes = 30)
    {
        var slots = await _examService.GetAvailableSlotsAsync(classId, from, to, durationMinutes, stepMinutes);
        return Ok(ApiResponse<List<AvailableSlotDto>>.SuccessResponse(slots, "Available slots"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateExamRequestDto request)
    {
        await _examService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Exam updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _examService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Exam deleted successfully"));
    }
}
