using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Students;
using EnglishCenter.Application.Features.Students.Dtos;
using EnglishCenter.Application.Features.Timetables;
using EnglishCenter.Application.Features.Timetables.Dtos;
using EnglishCenter.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly StudentService _studentService;
    private readonly TimetableService _timetableService;

    public StudentsController(StudentService studentService, TimetableService timetableService)
    {
        _studentService = studentService;
        _timetableService = timetableService;
    }
    // xem báo cáo điểm danh của học viên, bao gồm số buổi học đã tham gia, số buổi học vắng mặt, và các ghi chú liên quan.
    [HttpGet("{studentId:long}/attendance-report")]
    public async Task<IActionResult> GetAttendanceReport(
    long studentId,
    [FromQuery] GetStudentAttendanceReportRequestDto request)
    {
        var result = await _studentService.GetAttendanceReportAsync(studentId, request);
        return Ok(result);
    }
    // xem thông tin tổng quan của học viên
    [HttpGet("{studentId:long}/academic-summary")]
    public async Task<IActionResult> GetAcademicSummary(long studentId)
    {
        var result = await _studentService.GetAcademicSummaryAsync(studentId);
        return Ok(result);
    }

    //xem lich học của học viên theo cac enrollment con hieu luc của học viên đó
    [HttpGet("{studentId:long}/timetable")]
    public async Task<IActionResult> GetTimetable(long studentId, [FromQuery] GetTimetableRequestDto request)
    {
        var result = await _timetableService.GetStudentTimetableAsync(studentId, request);
        return Ok(result);
    }

    [Authorize(Policy = PermissionConstants.Students.View)]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetStudentsPagingRequestDto request)
    {
        var result = await _studentService.GetPagedAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _studentService.GetByIdAsync(id);
        return Ok(result);
    }

    [Authorize(Policy = PermissionConstants.Students.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequestDto request)
    {
        var id = await _studentService.CreateAsync(request);
        return Ok(new { Id = id });
    }

    [Authorize(Policy = PermissionConstants.Students.Update)]
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateStudentRequestDto request)
    {
        await _studentService.UpdateAsync(id, request);
        return Ok();
    }

    [Authorize(Policy = PermissionConstants.Students.Delete)]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _studentService.DeleteAsync(id);
        return Ok();
    }
}