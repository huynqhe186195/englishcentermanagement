using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EnglishCenter.Application.Features.AuditLogs;
using EnglishCenter.Application.Features.AuditLogs.Dtos;
using EnglishCenter.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PermissionConstants.Roles.ManagePermissions)]
public class AuditLogsController : ControllerBase
{
    private readonly AuditLogService _auditLogService;

    public AuditLogsController(AuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetAuditLogsPagingRequestDto request)
    {
        var result = await _auditLogService.GetPagedAsync(request);
        return Ok(result);
    }
}
