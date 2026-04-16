using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Students;
using EnglishCenter.Application.Features.Students.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly StudentService _studentService;

    public StudentsController(StudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetStudentsPagingRequestDto request)
    {
        var result = await _studentService.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<StudentDto>>.SuccessResponse(result, "Get students successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _studentService.GetByIdAsync(id);
        return Ok(ApiResponse<StudentDetailDto>.SuccessResponse(result, "Get student successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequestDto request)
    {
        var id = await _studentService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Student created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateStudentRequestDto request)
    {
        await _studentService.UpdateAsync(id, request);
        return Ok(ApiResponse<string>.SuccessResponse(null!, "Student updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _studentService.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse(null!, "Student deleted successfully"));
    }
}