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