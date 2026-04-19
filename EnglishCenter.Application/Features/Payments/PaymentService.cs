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
using EnglishCenter.Application.Features.Payments.Dtos;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnglishCenter.Application.Features.Payments;

public class PaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public PaymentService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<PaymentDto>> GetPagedAsync(GetPaymentsPagingRequestDto request)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.InvoiceId.HasValue)
        {
            query = query.Where(x => x.InvoiceId == request.InvoiceId.Value);
        }

        if (request.PaymentMethod.HasValue)
        {
            query = query.Where(x => x.PaymentMethod == request.PaymentMethod.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.TransactionCode))
        {
            var transactionCode = request.TransactionCode.Trim().ToLower();
            query = query.Where(x => x.TransactionCode != null && x.TransactionCode.ToLower().Contains(transactionCode));
        }

        var sortMappings = new Dictionary<string, Expression<Func<Payment, object>>>
        {
            { "Id", x => x.Id },
            { "InvoiceId", x => x.InvoiceId },
            { "Amount", x => x.Amount },
            { "PaymentMethod", x => x.PaymentMethod },
            { "PaymentDate", x => x.PaymentDate },
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
            .ProjectTo<PaymentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<PaymentDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<PaymentDetailDto> GetByIdAsync(long id)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(x => x.Invoice)
                .ThenInclude(x => x.Student)
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<PaymentDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (payment == null)
        {
            throw new NotFoundException("Payment not found.");
        }

        return payment;
    }

    public async Task<long> CreateAsync(CreatePaymentRequestDto request)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(x => x.Id == request.InvoiceId && !x.IsDeleted);

        if (invoice == null)
        {
            throw new NotFoundException("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatusConstants.Cancelled)
        {
            throw new BusinessException("Cannot create payment for a cancelled invoice.");
        }

        if (invoice.Status == InvoiceStatusConstants.Paid)
        {
            throw new BusinessException("Invoice is already fully paid.");
        }

        var existingActivePayment = await _context.Payments.AnyAsync(x =>
            x.InvoiceId == request.InvoiceId &&
            !x.IsDeleted &&
            x.Status != PaymentStatusConstants.Cancelled);

        if (existingActivePayment)
        {
            throw new BusinessException("This invoice already has an active payment record.");
        }

        if (request.Amount != invoice.FinalAmount)
        {
            throw new BusinessException("Full payment is required. Partial payment is not allowed.");
        }

        if (request.ReceivedByUserId.HasValue)
        {
            var receivedByExists = await _context.Users.AnyAsync(x =>
                x.Id == request.ReceivedByUserId.Value &&
                !x.IsDeleted);

            if (!receivedByExists)
            {
                throw new NotFoundException("ReceivedByUserId not found.");
            }
        }

        var entity = new Payment
        {
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            PaymentDate = request.PaymentDate,
            TransactionCode = request.TransactionCode,
            ReceivedByUserId = request.ReceivedByUserId,
            Note = request.Note,
            Status = PaymentStatusConstants.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };

        _context.Payments.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task ConfirmAsync(long paymentId, ConfirmPaymentRequestDto request)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(x => x.Id == paymentId && !x.IsDeleted);

        if (payment == null)
        {
            throw new NotFoundException("Payment not found.");
        }

        if (payment.Status == PaymentStatusConstants.Confirmed)
        {
            throw new BusinessException("Payment is already confirmed.");
        }

        if (payment.Status == PaymentStatusConstants.Cancelled)
        {
            throw new BusinessException("Cancelled payment cannot be confirmed.");
        }

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(x => x.Id == payment.InvoiceId && !x.IsDeleted);

        if (invoice == null)
        {
            throw new NotFoundException("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatusConstants.Cancelled)
        {
            throw new BusinessException("Cannot confirm payment for a cancelled invoice.");
        }

        if (invoice.Status == InvoiceStatusConstants.Paid)
        {
            throw new BusinessException("Invoice is already fully paid.");
        }

        if (payment.Amount != invoice.FinalAmount)
        {
            throw new BusinessException("Full payment is required. Partial payment is not allowed.");
        }

        payment.Status = PaymentStatusConstants.Confirmed;
        payment.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            payment.Note = string.IsNullOrWhiteSpace(payment.Note)
                ? request.Note.Trim()
                : $"{payment.Note} {request.Note.Trim()}";
        }

        invoice.PaidAmount = invoice.FinalAmount;
        invoice.Status = InvoiceStatusConstants.Paid;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task CancelAsync(long paymentId, CancelPaymentRequestDto request)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(x => x.Id == paymentId && !x.IsDeleted);

        if (payment == null)
        {
            throw new NotFoundException("Payment not found.");
        }

        if (payment.Status == PaymentStatusConstants.Confirmed)
        {
            throw new BusinessException("Confirmed payment cannot be cancelled.");
        }

        if (payment.Status == PaymentStatusConstants.Cancelled)
        {
            throw new BusinessException("Payment is already cancelled.");
        }

        payment.Status = PaymentStatusConstants.Cancelled;
        payment.Note = string.IsNullOrWhiteSpace(payment.Note)
            ? request.Reason.Trim()
            : $"{payment.Note} {request.Reason.Trim()}";
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
