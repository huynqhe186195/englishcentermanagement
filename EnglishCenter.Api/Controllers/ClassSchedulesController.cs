using EnglishCenter.Application.Features.ClassSchedules;
using EnglishCenter.Application.Features.ClassSchedules.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassSchedulesController : ControllerBase
{
    private readonly ClassScheduleService _classScheduleService;

    public ClassSchedulesController(ClassScheduleService classScheduleService)
    {
        _classScheduleService = classScheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long? classId)
    {
        var result = await _classScheduleService.GetAllAsync(classId);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _classScheduleService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassScheduleRequestDto request)
    {
        var id = await _classScheduleService.CreateAsync(request);
        return Ok(new { Id = id });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateClassScheduleRequestDto request)
    {
        await _classScheduleService.UpdateAsync(id, request);
        return Ok();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _classScheduleService.DeleteAsync(id);
        return Ok();
    }
}