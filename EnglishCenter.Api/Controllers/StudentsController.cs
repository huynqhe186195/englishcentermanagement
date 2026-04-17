using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Students;
using EnglishCenter.Application.Features.Students.Dtos;
using EnglishCenter.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Policy = PermissionConstants.Students.View)]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetStudentsPagingRequestDto request)
    {
        var result = await _studentService.GetPagedAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _studentService.GetByIdAsync(id);
        return Ok(result);
    }

    [Authorize(Policy = PermissionConstants.Students.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequestDto request)
    {
        var id = await _studentService.CreateAsync(request);
        return Ok(new { Id = id });
    }

    [Authorize(Policy = PermissionConstants.Students.Update)]
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateStudentRequestDto request)
    {
        await _studentService.UpdateAsync(id, request);
        return Ok();
    }

    [Authorize(Policy = PermissionConstants.Students.Delete)]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _studentService.DeleteAsync(id);
        return Ok();
    }
}