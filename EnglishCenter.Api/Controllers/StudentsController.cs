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
    public async Task<IActionResult> GetAll()
    {
        var result = await _studentService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _studentService.GetByIdAsync(id);

        if (result == null)
            return NotFound(new { message = "Student not found" });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequestDto request)
    {
        var id = await _studentService.CreateAsync(request);
        return Ok(new { message = "Student created successfully", id });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateStudentRequestDto request)
    {
        var updated = await _studentService.UpdateAsync(id, request);

        if (!updated)
            return NotFound(new { message = "Student not found" });

        return Ok(new { message = "Student updated successfully" });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _studentService.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Student not found" });

        return Ok(new { message = "Student deleted successfully" });
    }
}