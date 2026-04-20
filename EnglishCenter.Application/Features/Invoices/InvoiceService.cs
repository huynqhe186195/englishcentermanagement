using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Extensions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Invoices.Dtos;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnglishCenter.Application.Features.Invoices;

public class InvoiceService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public InvoiceService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<InvoiceDto>> GetPagedAsync(GetInvoicesPagingRequestDto request)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.InvoiceNo))
        {
            var invoiceNo = request.InvoiceNo.Trim().ToLower();
            query = query.Where(x => x.InvoiceNo.ToLower().Contains(invoiceNo));
        }

        if (request.StudentId.HasValue)
        {
            query = query.Where(x => x.StudentId == request.StudentId.Value);
        }

        if (request.ClassId.HasValue)
        {
            query = query.Where(x => x.ClassId == request.ClassId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var sortMappings = new Dictionary<string, Expression<Func<Invoice, object>>>
        {
            { "Id", x => x.Id },
            { "InvoiceNo", x => x.InvoiceNo },
            { "StudentId", x => x.StudentId },
            { "ClassId", x => x.ClassId ?? 0 },
            { "FinalAmount", x => x.FinalAmount },
            { "PaidAmount", x => x.PaidAmount },
            { "DueDate", x => x.DueDate },
            { "Status", x => x.Status },
            { "CreatedAt", x => x.CreatedAt }
        };

        query = query.ApplySorting(
            request.SortBy,
            request.SortDirection,
            sortMappings,
            x => x.Id);

        var totalRecords = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<InvoiceDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<InvoiceDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<InvoiceDetailDto> GetByIdAsync(long id)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Class)
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<InvoiceDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (invoice == null)
        {
            throw new NotFoundException("Invoice not found.");
        }

        return invoice;
    }

    public async Task<long> CreateAsync(CreateInvoiceRequestDto request)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(x => x.Id == request.StudentId && !x.IsDeleted);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == request.CourseId && !x.IsDeleted);

        if (course == null)
        {
            throw new NotFoundException("Course not found.");
        }

        var unpaidOrPaidInvoiceExists = await _context.Invoices.AnyAsync(x =>
            x.StudentId == request.StudentId &&
            !x.IsDeleted &&
            x.Status != InvoiceStatusConstants.Cancelled &&
            x.Note != null &&
            x.Note.Contains($"[COURSE_ID={request.CourseId}]"));

        if (unpaidOrPaidInvoiceExists)
        {
            throw new BusinessException("An active invoice for this course already exists for the student.");
        }

        var totalAmount = course.DefaultFee;
        var discountAmount = 0m;
        var finalAmount = totalAmount - discountAmount;

        var entity = new Invoice
        {
            InvoiceNo = await GenerateInvoiceNoAsync(),
            StudentId = request.StudentId,
            ClassId = null,
            TotalAmount = totalAmount,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            PaidAmount = 0,
            RefundedAmount = 0,
            DueDate = request.DueDate,
            Status = InvoiceStatusConstants.Unpaid,
            Note = BuildInvoiceNote(request.CourseId, course.Name, request.Note),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };

        _context.Invoices.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateAsync(long id, UpdateInvoiceRequestDto request)
    {
        var entity = await _context.Invoices
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Invoice not found.");
        }

        if (entity.Status == InvoiceStatusConstants.Paid)
        {
            throw new BusinessException("Paid invoice cannot be updated.");
        }

        if (entity.Status == InvoiceStatusConstants.Cancelled)
        {
            throw new BusinessException("Cancelled invoice cannot be updated.");
        }

        entity.DueDate = request.DueDate;
        entity.Note = MergeInvoiceNote(entity.Note, request.Note);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task CancelAsync(long id)
    {
        var entity = await _context.Invoices
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Invoice not found.");
        }

        if (entity.Status == InvoiceStatusConstants.Paid)
        {
            throw new BusinessException("Paid invoice cannot be cancelled.");
        }

        entity.Status = InvoiceStatusConstants.Cancelled;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<long> SelectClassAsync(long invoiceId, SelectClassForInvoiceRequestDto request)
    {
        await using var transaction = await _context.BeginTransactionAsync();

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDeleted);

        if (invoice == null)
        {
            throw new NotFoundException("Invoice not found.");
        }

        if (invoice.Status != InvoiceStatusConstants.Paid)
        {
            throw new BusinessException("You must complete full payment before selecting a class.");
        }

        if (invoice.PaidAmount != invoice.FinalAmount)
        {
            throw new BusinessException("Invoice is not fully paid.");
        }

        if (invoice.ClassId.HasValue)
        {
            throw new BusinessException("Class has already been selected for this invoice.");
        }

        var hasPayment = await _context.Payments.AnyAsync(x => x.InvoiceId == invoice.Id);
        if (!hasPayment)
        {
            throw new BusinessException("No payment record was found for this invoice.");
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(x => x.Id == invoice.StudentId && !x.IsDeleted);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        var @class = await _context.Classes
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == request.ClassId && !x.IsDeleted);

        if (@class == null)
        {
            throw new NotFoundException("Class not found.");
        }

        if (@class.Status != 1)
        {
            throw new BusinessException("Selected class is not available.");
        }

        var courseIdFromInvoice = ExtractCourseIdFromNote(invoice.Note);
        if (!courseIdFromInvoice.HasValue)
        {
            throw new BusinessException("Invoice does not contain course information.");
        }

        if (@class.CourseId != courseIdFromInvoice.Value)
        {
            throw new BusinessException("Selected class does not belong to the paid course.");
        }

        var activeCount = await _context.Enrollments.CountAsync(x =>
            x.ClassId == request.ClassId &&
            !x.IsDeleted &&
            x.Status == EnrollmentStatusConstants.Active);

        if (activeCount >= 10)
        {
            throw new BusinessException("This class already has the maximum of 10 students.");
        }

        var existed = await _context.Enrollments.AnyAsync(x =>
            x.StudentId == invoice.StudentId &&
            x.ClassId == request.ClassId &&
            !x.IsDeleted &&
            x.Status == EnrollmentStatusConstants.Active);

        if (existed)
        {
            throw new BusinessException("Student is already enrolled in this class.");
        }

        invoice.ClassId = request.ClassId;
        invoice.UpdatedAt = DateTime.UtcNow;

        var enrollment = new Enrollment
        {
            StudentId = invoice.StudentId,
            ClassId = request.ClassId,
            EnrollDate = DateOnly.FromDateTime(DateTime.Today),
            Status = EnrollmentStatusConstants.Active,
            Note = $"Created from paid invoice {invoice.InvoiceNo}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return enrollment.Id;
    }

    private async Task<string> GenerateInvoiceNoAsync()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"INV-{datePart}-";

        var countToday = await _context.Invoices.CountAsync(x =>
            x.InvoiceNo.StartsWith(prefix));

        return $"{prefix}{(countToday + 1):D4}";
    }

    private static string BuildInvoiceNote(long courseId, string courseName, string? userNote)
    {
        var systemPart = $"[COURSE_ID={courseId}] [COURSE_NAME={courseName}]";
        return string.IsNullOrWhiteSpace(userNote)
            ? systemPart
            : $"{systemPart} {userNote}".Trim();
    }

    private static string MergeInvoiceNote(string? oldNote, string? newNote)
    {
        if (string.IsNullOrWhiteSpace(newNote))
        {
            return oldNote ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(oldNote))
        {
            return newNote.Trim();
        }

        return $"{oldNote} {newNote.Trim()}";
    }

    private static long? ExtractCourseIdFromNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;

        const string prefix = "[COURSE_ID=";
        var start = note.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            return null;

        start += prefix.Length;
        var end = note.IndexOf(']', start);
        if (end < 0)
            return null;

        var value = note[start..end];
        return long.TryParse(value, out var courseId) ? courseId : null;
    }
}
