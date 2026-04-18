using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Teachers;
using EnglishCenter.Application.Features.Teachers.Dtos;
using EnglishCenter.Application.Features.Timetables;
using EnglishCenter.Application.Features.Timetables.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeachersController : ControllerBase
{
    private readonly TeacherService _teacherService;
    private readonly TimetableService _timetableService;

    public TeachersController(
    TeacherService teacherService,
    TimetableService timetableService)
    {
        _teacherService = teacherService;
        _timetableService = timetableService;
    }
    // xem thông tin tổng quan của giáo viên
    [HttpGet("{teacherId:long}/summary")]
    public async Task<IActionResult> GetSummary(long teacherId)
    {
        var result = await _teacherService.GetSummaryAsync(teacherId);
        return Ok(result);
    }

    [HttpGet("{teacherId:long}/timetable")]
    public async Task<IActionResult> GetTimetable(long teacherId, [FromQuery] GetTimetableRequestDto request)
    {
        var result = await _timetableService.GetTeacherTimetableAsync(teacherId, request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetTeachersPagingRequestDto request)
    {
        var result = await _teacherService.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<TeacherDto>>.SuccessResponse(result, "Get teachers successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _teacherService.GetByIdAsync(id);
        return Ok(ApiResponse<TeacherDetailDto>.SuccessResponse(result, "Get teacher successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeacherRequestDto request)
    {
        var id = await _teacherService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Teacher created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateTeacherRequestDto request)
    {
        await _teacherService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Teacher updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _teacherService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Teacher deleted successfully"));
    }
}
