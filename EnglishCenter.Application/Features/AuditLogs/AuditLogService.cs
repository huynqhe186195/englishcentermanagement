using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Extensions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.AuditLogs.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnglishCenter.Application.Features.AuditLogs;

public class AuditLogService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AuditLogService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(GetAuditLogsPagingRequestDto request)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            var entityName = request.EntityName.Trim().ToLower();
            query = query.Where(x => x.EntityName.ToLower().Contains(entityName));
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim().ToLower();
            query = query.Where(x => x.Action.ToLower().Contains(action));
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId.Value);
        }

        var sortMappings = new Dictionary<string, Expression<Func<AuditLog, object>>>
        {
            { "Id", x => x.Id },
            { "UserId", x => x.UserId ?? 0 },
            { "Action", x => x.Action },
            { "EntityName", x => x.EntityName },
            { "EntityId", x => x.EntityId ?? string.Empty },
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
            .ProjectTo<AuditLogDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }
}
