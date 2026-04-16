using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Classes;
using EnglishCenter.Application.Features.Classes.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly ClassService _classService;

    public ClassesController(ClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetClassesPagingRequestDto request)
    {
        var result = await _classService.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<ClassDto>>.SuccessResponse(result, "Get classes successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _classService.GetByIdAsync(id);
        return Ok(ApiResponse<ClassDetailDto>.SuccessResponse(result, "Get class successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequestDto request)
    {
        var id = await _classService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Class created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateClassRequestDto request)
    {
        await _classService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Class updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _classService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Class deleted successfully"));
    }
}