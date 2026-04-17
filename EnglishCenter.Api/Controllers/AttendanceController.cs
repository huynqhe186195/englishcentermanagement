using EnglishCenter.Application.Features.Attendance;
using EnglishCenter.Application.Features.Attendance.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public AttendanceController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetAttendancePagingRequestDto request)
    {
        var result = await _attendanceService.GetPagedAsync(request);
        return Ok(result);
    }

    [HttpGet("session/{sessionId:long}")]
    public async Task<IActionResult> GetBySessionId(long sessionId)
    {
        var result = await _attendanceService.GetBySessionIdAsync(sessionId);
        return Ok(result);
    }

    [HttpGet("student/{studentId:long}")]
    public async Task<IActionResult> GetByStudentId(long studentId)
    {
        var result = await _attendanceService.GetByStudentIdAsync(studentId);
        return Ok(result);
    }

    [HttpGet("student/{studentId:long}/summary")]
    public async Task<IActionResult> GetStudentSummary(long studentId)
    {
        var result = await _attendanceService.GetStudentSummaryAsync(studentId);
        return Ok(result);
    }

    [HttpPost("mark")]
    public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceRequestDto request)
    {
        await _attendanceService.MarkAttendanceAsync(request);
        return Ok();
    }
}