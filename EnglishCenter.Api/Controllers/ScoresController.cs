using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Scores;
using EnglishCenter.Application.Features.Scores.Dtos;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using System.IO;
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

    [HttpGet("template/{examId:long}")]
    public IActionResult DownloadTemplate(long examId)
    {
        // generate template and pre-fill student info for the exam's class
        var students = _scoreService.GetTemplateStudentsAsync(examId).GetAwaiter().GetResult();

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Scores");
        ws.Cell(1, 1).Value = "StudentId";
        ws.Cell(1, 2).Value = "StudentCode";
        ws.Cell(1, 3).Value = "StudentName";
        ws.Cell(1, 4).Value = "ScoreValue";
        ws.Cell(1, 5).Value = "Remark";

        var row = 2;
        foreach (var s in students)
        {
            ws.Cell(row, 1).Value = s.StudentId;
            ws.Cell(row, 2).Value = s.StudentCode;
            ws.Cell(row, 3).Value = s.StudentName;
            // leave ScoreValue and Remark empty
            row++;
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"scores_template_exam_{examId}.xlsx");
    }

    [HttpPost("import/{examId:long}")]
    public async Task<IActionResult> Import(long examId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.FailResponse("No file uploaded."));

        var items = new List<ImportScoreItemDto>();

        using (var stream = file.OpenReadStream())
        using (var workbook = new XLWorkbook(stream))
        {
            var ws = workbook.Worksheets.First();
            var row = 2;
            while (true)
            {
                var idCell = ws.Cell(row, 1);
                var codeCell = ws.Cell(row, 2);
                var nameCell = ws.Cell(row, 3);
                var scoreCell = ws.Cell(row, 4);
                var remarkCell = ws.Cell(row, 5);

                if (idCell.IsEmpty() && codeCell.IsEmpty() && scoreCell.IsEmpty()) break;

                long? studentId = null;
                if (!idCell.IsEmpty())
                {
                    if (long.TryParse(idCell.GetString(), out var sid)) studentId = sid;
                }

                var code = codeCell.GetString();
                decimal scoreValue = 0;
                if (!scoreCell.IsEmpty())
                {
                    var s = scoreCell.GetString();
                    decimal.TryParse(s, out scoreValue);
                }

                items.Add(new ImportScoreItemDto
                {
                    StudentId = studentId,
                    StudentCode = string.IsNullOrWhiteSpace(code) ? null : code,
                    ScoreValue = scoreValue,
                    Remark = remarkCell.GetString()
                });

                row++;
            }
        }

        await _scoreService.ImportScoresAsync(examId, items);

        var passFail = await _scoreService.GetPassFailListAsync(examId);
        return Ok(ApiResponse<List<PassFailDto>>.SuccessResponse(passFail, "Import completed"));
    }

    [HttpGet("export/{examId:long}")]
    public async Task<IActionResult> Export(long examId, [FromQuery] bool passed = true)
    {
        var list = await _scoreService.GetPassFailListAsync(examId);
        var filtered = list.Where(x => x.IsPassed == passed).ToList();

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Result");
        ws.Cell(1, 1).Value = "StudentId";
        ws.Cell(1, 2).Value = "StudentCode";
        ws.Cell(1, 3).Value = "StudentName";
        ws.Cell(1, 4).Value = "AverageScore";
        ws.Cell(1, 5).Value = "PresentCount";
        ws.Cell(1, 6).Value = "TotalSessions";
        ws.Cell(1, 7).Value = "AttendancePercent";
        ws.Cell(1, 8).Value = "IsPassed";

        var row = 2;
        foreach (var it in filtered)
        {
            ws.Cell(row, 1).Value = it.StudentId;
            ws.Cell(row, 2).Value = it.StudentCode;
            ws.Cell(row, 3).Value = it.StudentName;
            ws.Cell(row, 4).Value = it.AverageScore;
            ws.Cell(row, 5).Value = it.PresentCount;
            ws.Cell(row, 6).Value = it.TotalSessions;
            ws.Cell(row, 7).Value = it.AttendancePercent;
            ws.Cell(row, 8).Value = it.IsPassed ? "Pass" : "Not Pass";
            row++;
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"scores_export_exam_{examId}_{(passed ? "passed" : "notpassed")}.xlsx");
    }

    [HttpGet("export-all/{examId:long}")]
    public async Task<IActionResult> ExportAll(long examId)
    {
        var list = await _scoreService.GetPassFailListAsync(examId);

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Result");
        ws.Cell(1, 1).Value = "StudentId";
        ws.Cell(1, 2).Value = "StudentCode";
        ws.Cell(1, 3).Value = "StudentName";
        ws.Cell(1, 4).Value = "AverageScore";
        ws.Cell(1, 5).Value = "PresentCount";
        ws.Cell(1, 6).Value = "TotalSessions";
        ws.Cell(1, 7).Value = "AttendancePercent";
        ws.Cell(1, 8).Value = "IsPassed";

        var row = 2;
        foreach (var it in list)
        {
            ws.Cell(row, 1).Value = it.StudentId;
            ws.Cell(row, 2).Value = it.StudentCode;
            ws.Cell(row, 3).Value = it.StudentName;
            ws.Cell(row, 4).Value = it.AverageScore;
            ws.Cell(row, 5).Value = it.PresentCount;
            ws.Cell(row, 6).Value = it.TotalSessions;
            ws.Cell(row, 7).Value = it.AttendancePercent;
            ws.Cell(row, 8).Value = it.IsPassed ? "Pass" : "Not Pass";
            row++;
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"scores_export_exam_{examId}_all.xlsx");
    }
}
