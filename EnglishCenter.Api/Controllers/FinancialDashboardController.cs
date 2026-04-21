using EnglishCenter.Application.Features.FinancialDashboards;
using EnglishCenter.Application.Features.FinancialDashboards.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireSuperAdmin")]
public class FinancialDashboardController : ControllerBase
{
    private readonly FinancialDashboardService _financialDashboardService;

    public FinancialDashboardController(FinancialDashboardService financialDashboardService)
    {
        _financialDashboardService = financialDashboardService;
    }
    // Lấy tổng quan doanh thu
    [HttpGet("revenue-summary")]
    public async Task<IActionResult> GetRevenueSummary([FromQuery] GetRevenueDashboardRequestDto request)
    {
        var result = await _financialDashboardService.GetRevenueSummaryAsync(request);
        return Ok(result);
    }
    // Lấy doanh thu theo tháng
    [HttpGet("revenue-by-month")]
    public async Task<IActionResult> GetRevenueByMonth([FromQuery] GetRevenueDashboardRequestDto request)
    {
        var result = await _financialDashboardService.GetRevenueByMonthAsync(request);
        return Ok(result);
    }
    // Lấy doanh thu theo khóa học
    [HttpGet("revenue-by-course")]
    public async Task<IActionResult> GetRevenueByCourse([FromQuery] GetRevenueDashboardRequestDto request)
    {
        var result = await _financialDashboardService.GetRevenueByCourseAsync(request);
        return Ok(result);
    }
    // Lấy doanh thu theo cơ sở
    [HttpGet("revenue-by-campus")]
    public async Task<IActionResult> GetRevenueByCampus([FromQuery] GetRevenueDashboardRequestDto request)
    {
        var result = await _financialDashboardService.GetRevenueByCampusAsync(request);
        return Ok(result);
    }
    // Lấy tổng quan khối lượng giảng dạy của giáo viên theo cơ sở
    [HttpGet("teacher-workload-by-campus")]
    public async Task<IActionResult> GetTeacherWorkloadByCampus()
    {
        var result = await _financialDashboardService.GetTeacherWorkloadByCampusAsync();
        return Ok(result);
    }
    // Lấy tỷ lệ sử dụng phòng học theo cơ sở
    [HttpGet("room-utilization-by-campus")]
    public async Task<IActionResult> GetRoomUtilizationByCampus()
    {
        var result = await _financialDashboardService.GetRoomUtilizationByCampusAsync();
        return Ok(result);
    }
    // Lấy tổng quan số lượng học viên theo lớp học và cơ sở
    [HttpGet("class-dashboard-by-campus")]
    public async Task<IActionResult> GetClassDashboardByCampus()
    {
        var result = await _financialDashboardService.GetClassDashboardByCampusAsync();
        return Ok(result);
    }
}
