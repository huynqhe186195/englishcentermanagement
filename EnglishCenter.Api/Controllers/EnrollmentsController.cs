using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Enrollments;
using EnglishCenter.Application.Features.Enrollments.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly EnrollmentService _enrollmentService;

    public EnrollmentsController(EnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetEnrollmentsPagingRequestDto request)
    {
        var result = await _enrollmentService.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<EnrollmentDto>>.SuccessResponse(result, "Get enrollments successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _enrollmentService.GetByIdAsync(id);
        return Ok(ApiResponse<EnrollmentDetailDto>.SuccessResponse(result, "Get enrollment successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequestDto request)
    {
        var id = await _enrollmentService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Enrollment created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateEnrollmentRequestDto request)
    {
        await _enrollmentService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Enrollment updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _enrollmentService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Enrollment deleted successfully"));
    }
}
