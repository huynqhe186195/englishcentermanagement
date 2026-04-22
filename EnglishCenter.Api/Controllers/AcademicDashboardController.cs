using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnglishCenter.Application.Features.Dashboards;
using EnglishCenter.Application.Features.Dashboards.Dtos;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireCenterAdmin")]
public class AcademicDashboardController : ControllerBase
{
    private readonly AcademicDashboardService _academicDashboardService;

    public AcademicDashboardController(AcademicDashboardService academicDashboardService)
    {
        _academicDashboardService = academicDashboardService;
    }
    // endpoint để lấy thông tin tổng quan về học sinh có nguy cơ cao, khối lượng công việc của giáo viên, tình trạng sử dụng phòng học,
    // và các chỉ số khác liên quan đến hoạt động học tập và giảng dạy tại trung tâm.
    [HttpGet("students-at-risk")]
    public async Task<IActionResult> GetStudentsAtRisk([FromQuery] GetStudentsAtRiskRequestDto request)
    {
        var result = await _academicDashboardService.GetStudentsAtRiskAsync(request);
        return Ok(result);
    }
    // endpoint để lấy thông tin về khối lượng công việc của giáo viên, bao gồm số lượng lớp học,
    // số giờ giảng dạy, và các nhiệm vụ khác mà giáo viên đang đảm nhận.
    [HttpGet("teacher-workload")]
    public async Task<IActionResult> GetTeacherWorkload([FromQuery] GetTeacherWorkloadRequestDto request)
    {
        var result = await _academicDashboardService.GetTeacherWorkloadAsync(request);
        return Ok(result);
    }
    // endpoint để lấy thông tin về tình trạng sử dụng phòng học, bao gồm số lượng phòng học đang được sử dụng,
    [HttpGet("room-utilization")]
    public async Task<IActionResult> GetRoomUtilization([FromQuery] GetRoomUtilizationRequestDto request)
    {
        var result = await _academicDashboardService.GetRoomUtilizationAsync(request);
        return Ok(result);
    }
    // endpoint để lấy thông tin tổng quan về hoạt động học tập và giảng dạy tại trung tâm, bao gồm các chỉ số như số lượng lớp học,
    // số lượng học sinh,
    // tỷ lệ hoàn thành bài tập, và các chỉ số khác liên quan đến hiệu quả giảng dạy và học tập.
    [HttpGet("class-dashboard")]
    public async Task<IActionResult> GetClassDashboard([FromQuery] GetClassDashboardRequestDto request)
    {
        var result = await _academicDashboardService.GetClassDashboardAsync(request);
        return Ok(result);
    }
}
