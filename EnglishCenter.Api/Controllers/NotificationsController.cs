using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Notifications;
using EnglishCenter.Application.Features.Notifications.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetNotificationsPagingRequestDto request)
    {
        var result = await _notificationService.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<NotificationDto>>.SuccessResponse(result, "Get notifications successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _notificationService.GetByIdAsync(id);
        return Ok(ApiResponse<NotificationDetailDto>.SuccessResponse(result, "Get notification successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequestDto request)
    {
        var id = await _notificationService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Notification created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateNotificationRequestDto request)
    {
        await _notificationService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Notification updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _notificationService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Notification deleted successfully"));
    }
}
