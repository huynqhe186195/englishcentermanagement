using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Scores;
using EnglishCenter.Application.Features.Scores.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoresController : ControllerBase
{
    private readonly ScoreService _scoreService;

    public ScoresController(ScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _scoreService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResult<ScoreDto>>.SuccessResponse(result, "Get scores successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _scoreService.GetByIdAsync(id);
        return Ok(ApiResponse<ScoreDetailDto>.SuccessResponse(result, "Get score successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateScoreRequestDto request)
    {
        var id = await _scoreService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Score created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateScoreRequestDto request)
    {
        await _scoreService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Score updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _scoreService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Score deleted successfully"));
    }
}
