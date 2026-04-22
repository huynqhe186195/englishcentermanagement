using EnglishCenter.Application.Features.ClassSessions;
using EnglishCenter.Application.Features.ClassSessions.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassSessionsController : ControllerBase
{
    private readonly ClassSessionService _classSessionService;

    public ClassSessionsController(ClassSessionService classSessionService)
    {
        _classSessionService = classSessionService;
    }
    // Cho phép giáo viên thay đổi lịch trình của một buổi học đã lên lịch,
    [HttpPut("{sessionId:long}/reschedule")]
    public async Task<IActionResult> Reschedule(long sessionId, [FromBody] RescheduleClassSessionRequestDto request)
    {
        await _classSessionService.RescheduleAsync(sessionId, request);
        return Ok();
    }
    // Cho phép giảng viên có thể cancel session đang dạy để có thể điểm danh cho sinh viên
    [HttpPut("{id:long}/reopen")]
    public async Task<IActionResult> Reopen(long id, [FromBody] CompleteClassSessionRequestDto request)
    {
        await _classSessionService.ReopenAsync(id, request);
        return Ok();
    }
    // Cho phép giáo viên hủy một buổi học đã lên lịch,
    // cập nhật trạng thái của buổi học và thực hiện các hành động liên quan như gửi thông báo đến sinh viên hoặc cập nhật điểm danh.
    [HttpPut("{sessionId:long}/cancel")]
    public async Task<IActionResult> Cancel(long sessionId, [FromBody] CancelClassSessionRequestDto request)
    {
        await _classSessionService.CancelAsync(sessionId, request);
        return Ok();
    }
    // Cho phép giáo viên đánh dấu một buổi học là đã hoàn thành,
    // cập nhật trạng thái của buổi học và thực hiện các hành động liên quan như gửi thông báo hoặc cập nhật điểm danh.
    [HttpPut("{sessionId:long}/complete")]
    public async Task<IActionResult> Complete(long sessionId, [FromBody] CompleteClassSessionRequestDto request)
    {
        await _classSessionService.CompleteAsync(sessionId, request);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetClassSessionsPagingRequestDto request)
    {
        var result = await _classSessionService.GetPagedAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _classSessionService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassSessionRequestDto request)
    {
        var id = await _classSessionService.CreateAsync(request);
        return Ok(new { Id = id });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateClassSessionRequestDto request)
    {
        await _classSessionService.UpdateAsync(id, request);
        return Ok();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _classSessionService.DeleteAsync(id);
        return Ok();
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateClassSessionsRequestDto request)
    {
        var count = await _classSessionService.GenerateSessionsAsync(request);
        return Ok(new { GeneratedCount = count });
    }
}