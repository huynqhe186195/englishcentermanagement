using EnglishCenter.Application.Features.Overrides;
using EnglishCenter.Application.Features.Overrides.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireSuperAdmin")]
public class OverridesController : ControllerBase
{
    private readonly OverrideWorkflowService _overrideWorkflowService;

    public OverridesController(OverrideWorkflowService overrideWorkflowService)
    {
        _overrideWorkflowService = overrideWorkflowService;
    }

    [HttpGet("supported-actions")]
    public IActionResult GetSupportedActions()
    {
        return Ok(new[]
        {
            "INVOICE_CANCEL",
            "ENROLLMENT_SUSPEND",
            "CLASSSESSION_CANCEL"
        });
    }

    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] ExecuteOverrideRequestDto request)
    {
        await _overrideWorkflowService.ExecuteAsync(request);
        return Ok(new { Message = "Override executed successfully." });
    }
}
