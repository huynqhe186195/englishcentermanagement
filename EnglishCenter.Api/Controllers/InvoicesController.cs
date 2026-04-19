using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Invoices;
using EnglishCenter.Application.Features.Invoices.Dtos;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public InvoicesController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetInvoicesPagingRequestDto request)
    {
        var result = await _invoiceService.GetPagedAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _invoiceService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequestDto request)
    {
        var id = await _invoiceService.CreateAsync(request);
        return Ok(new { Id = id });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateInvoiceRequestDto request)
    {
        await _invoiceService.UpdateAsync(id, request);
        return Ok();
    }

    [HttpPut("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        await _invoiceService.CancelAsync(id);
        return Ok();
    }

    [HttpPost("{invoiceId:long}/select-class")]
    public async Task<IActionResult> SelectClass(long invoiceId, [FromBody] SelectClassForInvoiceRequestDto request)
    {
        var enrollmentId = await _invoiceService.SelectClassAsync(invoiceId, request);
        return Ok(new { EnrollmentId = enrollmentId });
    }
}
