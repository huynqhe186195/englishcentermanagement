using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Courses;
using EnglishCenter.Application.Features.Courses.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly CourseService _courseService;

    public CoursesController(CourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetCoursesPagingRequestDto request)
    {
        var result = await _courseService.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<CourseDto>>.SuccessResponse(result, "Get courses successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _courseService.GetByIdAsync(id);
        return Ok(ApiResponse<CourseDetailDto>.SuccessResponse(result, "Get course successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequestDto request)
    {
        var id = await _courseService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Course created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCourseRequestDto request)
    {
        await _courseService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Course updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _courseService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Course deleted successfully"));
    }
}