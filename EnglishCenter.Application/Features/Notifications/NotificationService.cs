using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Notifications.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Notifications;

public class NotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public NotificationService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<NotificationDto>> GetAllAsync()
    {
        return await _context.Notifications
            .AsNoTracking()
            .ProjectTo<NotificationDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<NotificationDto>> GetPagedAsync(GetNotificationsPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Notifications
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(kw) || x.Content.ToLower().Contains(kw));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<NotificationDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<NotificationDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<NotificationDetailDto> GetByIdAsync(long id)
    {
        var entity = await _context.Notifications
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<NotificationDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (entity == null) throw new NotFoundException("Notification not found.");
        return entity;
    }

    public async Task<long> CreateAsync(CreateNotificationRequestDto request)
    {
        var entity = _mapper.Map<Notification>(request);
        entity.CreatedAt = DateTime.UtcNow;

        _context.Notifications.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateNotificationRequestDto request)
    {
        var entity = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) throw new NotFoundException("Notification not found.");

        _mapper.Map(request, entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) throw new NotFoundException("Notification not found.");

        _context.Notifications.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
