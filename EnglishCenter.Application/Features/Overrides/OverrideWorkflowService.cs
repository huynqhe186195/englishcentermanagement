using System.Text.Json;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Features.ClassSessions;
using EnglishCenter.Application.Features.ClassSessions.Dtos;
using EnglishCenter.Application.Features.Enrollments;
using EnglishCenter.Application.Features.Enrollments.Dtos;
using EnglishCenter.Application.Features.Invoices;
using EnglishCenter.Application.Features.Overrides.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Overrides;

public class OverrideWorkflowService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly InvoiceService _invoiceService;
    private readonly EnrollmentService _enrollmentService;
    private readonly ClassSessionService _classSessionService;

    public OverrideWorkflowService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        InvoiceService invoiceService,
        EnrollmentService enrollmentService,
        ClassSessionService classSessionService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _invoiceService = invoiceService;
        _enrollmentService = enrollmentService;
        _classSessionService = classSessionService;
    }

    public async Task ExecuteAsync(ExecuteOverrideRequestDto request)
    {
        var actionCode = request.ActionCode.Trim().ToUpperInvariant();
        var reason = request.Reason.Trim();

        await WriteAuditAsync("OverrideRequested", actionCode, request.TargetId, new
        {
            request.ActionCode,
            request.TargetId,
            request.Reason,
            request.Note
        });

        try
        {
            switch (actionCode)
            {
                case "INVOICE_CANCEL":
                    await _invoiceService.CancelAsync(request.TargetId);
                    break;

                case "ENROLLMENT_SUSPEND":
                    await _enrollmentService.SuspendAsync(request.TargetId, new SuspendEnrollmentRequestDto
                    {
                        Reason = reason
                    });
                    break;

                case "CLASSSESSION_CANCEL":
                    await _classSessionService.CancelAsync(request.TargetId, new CancelClassSessionRequestDto
                    {
                        Reason = reason
                    });
                    break;

                default:
                    throw new BusinessException("Unsupported override action.");
            }

            await WriteAuditAsync("OverrideExecuted", actionCode, request.TargetId, new
            {
                request.ActionCode,
                request.TargetId,
                request.Reason,
                request.Note
            });
        }
        catch (Exception ex)
        {
            await WriteAuditAsync("OverrideFailed", actionCode, request.TargetId, new
            {
                request.ActionCode,
                request.TargetId,
                request.Reason,
                request.Note,
                Error = ex.Message
            });
            throw;
        }
    }

    private async Task WriteAuditAsync(string action, string entityName, long targetId, object payload)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = _currentUserService.UserId,
            Action = action,
            EntityName = entityName,
            EntityId = targetId.ToString(),
            NewValues = JsonSerializer.Serialize(payload),
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}
