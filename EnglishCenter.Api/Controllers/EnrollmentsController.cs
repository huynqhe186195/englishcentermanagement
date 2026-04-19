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

    // đánh giá chính sách điểm danh của lớp học dựa trên số buổi học đã tham gia và
    // số buổi học đã vắng mặt của học viên trong quá trình đăng ký lớp học
    [HttpPost("classes/{classId:long}/evaluate-attendance-policy")]
    public async Task<IActionResult> EvaluateAttendancePolicy(long classId)
    {
        var result = await _enrollmentService.EvaluateAttendancePolicyByClassAsync(classId);
        return Ok(result);
    }

    // đình chỉ học viên khỏi lớp học nếu có bất kỳ active nào không đúng trong quá trình đăng ký lớp học
    [HttpPut("{enrollmentId:long}/suspend")]
    public async Task<IActionResult> Suspend(long enrollmentId, [FromBody] SuspendEnrollmentRequestDto request)
    {
        await _enrollmentService.SuspendAsync(enrollmentId, request);
        return Ok();
    }
    // hoàn thành lớp học nếu học viên đã hoàn thành tất cả các active trong quá trình đăng ký lớp học
    [HttpPut("{enrollmentId:long}/complete")]
    public async Task<IActionResult> Complete(long enrollmentId, [FromBody] CompleteEnrollmentRequestDto request)
    {
        await _enrollmentService.CompleteAsync(enrollmentId, request);
        return Ok();
    }
    // chuyển lớp học nếu học viên muốn chuyển sang lớp học khác trong quá trình đăng ký lớp học
    [HttpPost("{enrollmentId:long}/transfer")]
    public async Task<IActionResult> Transfer(long enrollmentId, [FromBody] TransferEnrollmentRequestDto request)
    {
        var newEnrollmentId = await _enrollmentService.TransferAsync(enrollmentId, request);
        return Ok(new { NewEnrollmentId = newEnrollmentId });
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
