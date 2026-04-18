using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Extensions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.ClassSessions.Dtos;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnglishCenter.Application.Features.ClassSessions;

public class ClassSessionService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly SessionConflictService _sessionConflictService;

    public ClassSessionService(
    IApplicationDbContext context,
    IMapper mapper,
    SessionConflictService sessionConflictService)
    {
        _context = context;
        _mapper = mapper;
        _sessionConflictService = sessionConflictService;
    }

    public async Task<PagedResult<ClassSessionDto>> GetPagedAsync(GetClassSessionsPagingRequestDto request)
    {
        var query = _context.ClassSessions
            .AsNoTracking()
            .AsQueryable();

        if (request.ClassId.HasValue)
        {
            query = query.Where(x => x.ClassId == request.ClassId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.SessionDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.SessionDate <= request.ToDate.Value);
        }

        var sortMappings = new Dictionary<string, Expression<Func<ClassSession, object>>>
        {
            { "Id", x => x.Id },
            { "SessionNo", x => x.SessionNo },
            { "SessionDate", x => x.SessionDate },
            { "StartTime", x => x.StartTime },
            { "EndTime", x => x.EndTime },
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
            .ProjectTo<ClassSessionDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<ClassSessionDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<ClassSessionDetailDto> GetByIdAsync(long id)
    {
        var session = await _context.ClassSessions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<ClassSessionDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (session == null)
        {
            throw new NotFoundException("Class session not found.");
        }

        return session;
    }

    public async Task<long> CreateAsync(CreateClassSessionRequestDto request)
    {
        var @class = await _context.Classes
            .FirstOrDefaultAsync(x => x.Id == request.ClassId && !x.IsDeleted);

        if (@class == null)
            throw new NotFoundException("Class not found.");

        if (request.SessionDate < @class.StartDate || request.SessionDate > @class.EndDate)
            throw new BusinessException("SessionDate must be within the class date range.");

        var duplicate = await _context.ClassSessions.AnyAsync(x =>
            x.ClassId == request.ClassId &&
            x.SessionDate == request.SessionDate &&
            x.StartTime == request.StartTime);

        if (duplicate)
            throw new BusinessException("This class session already exists.");

        var sessionNoExists = await _context.ClassSessions.AnyAsync(x =>
            x.ClassId == request.ClassId &&
            x.SessionNo == request.SessionNo);

        if (sessionNoExists)
            throw new BusinessException("SessionNo already exists in this class.");

        await _sessionConflictService.ValidateSessionConflictsAsync(
            request.TeacherId,
            request.RoomId,
            request.SessionDate,
            request.StartTime,
            request.EndTime);

        var entity = _mapper.Map<ClassSession>(request);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;

        _context.ClassSessions.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateAsync(long id, UpdateClassSessionRequestDto request)
    {
        var entity = await _context.ClassSessions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            throw new NotFoundException("Class session not found.");

        var @class = await _context.Classes
            .FirstOrDefaultAsync(x => x.Id == entity.ClassId && !x.IsDeleted);

        if (@class == null)
            throw new NotFoundException("Class not found.");

        if (request.SessionDate < @class.StartDate || request.SessionDate > @class.EndDate)
            throw new BusinessException("SessionDate must be within the class date range.");

        var duplicate = await _context.ClassSessions.AnyAsync(x =>
            x.Id != id &&
            x.ClassId == entity.ClassId &&
            x.SessionDate == request.SessionDate &&
            x.StartTime == request.StartTime);

        if (duplicate)
            throw new BusinessException("This class session already exists.");

        var sessionNoExists = await _context.ClassSessions.AnyAsync(x =>
            x.Id != id &&
            x.ClassId == entity.ClassId &&
            x.SessionNo == request.SessionNo);

        if (sessionNoExists)
            throw new BusinessException("SessionNo already exists in this class.");

        await _sessionConflictService.ValidateSessionConflictsAsync(
            request.TeacherId,
            request.RoomId,
            request.SessionDate,
            request.StartTime,
            request.EndTime,
            id);

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.ClassSessions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new NotFoundException("Class session not found.");
        }

        _context.ClassSessions.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GenerateSessionsAsync(GenerateClassSessionsRequestDto request)
    {
        var @class = await _context.Classes
            .FirstOrDefaultAsync(x => x.Id == request.ClassId && !x.IsDeleted);

        if (@class == null)
        {
            throw new NotFoundException("Class not found.");
        }

        var schedules = await _context.ClassSchedules
            .Where(x => x.ClassId == request.ClassId)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToListAsync();

        if (!schedules.Any())
        {
            throw new BusinessException("Class has no schedules to generate sessions.");
        }

        var existingSessions = await _context.ClassSessions
            .Where(x => x.ClassId == request.ClassId)
            .Select(x => new { x.SessionDate, x.StartTime })
            .ToListAsync();

        var existingSet = existingSessions
            .Select(x => $"{x.SessionDate:yyyy-MM-dd}_{x.StartTime}")
            .ToHashSet();

        var currentMaxSessionNo = await _context.ClassSessions
            .Where(x => x.ClassId == request.ClassId)
            .Select(x => (int?)x.SessionNo)
            .MaxAsync() ?? 0;

        var createdSessions = new List<ClassSession>();
        var currentDate = @class.StartDate;
        var endDate = @class.EndDate;

        while (currentDate <= endDate)
        {
            var dayOfWeek = ConvertToCustomDayOfWeek(currentDate.DayOfWeek);

            var matchedSchedules = schedules
                .Where(x => x.DayOfWeek == dayOfWeek)
                .ToList();

            foreach (var schedule in matchedSchedules)
            {
                var key = $"{currentDate:yyyy-MM-dd}_{schedule.StartTime}";

                if (existingSet.Contains(key))
                    continue;

                await _sessionConflictService.ValidateRoomConflictAsync(
                    schedule.RoomId,
                    currentDate,
                    schedule.StartTime,
                    schedule.EndTime);

                currentMaxSessionNo++;

                createdSessions.Add(new ClassSession
                {
                    ClassId = request.ClassId,
                    SessionNo = currentMaxSessionNo,
                    SessionDate = currentDate,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    RoomId = schedule.RoomId,
                    TeacherId = null,
                    Topic = null,
                    Note = null,
                    Status = ClassSessionStatusConstants.Planned,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                });

                existingSet.Add(key);
            }

            currentDate = currentDate.AddDays(1);
        }

        if (createdSessions.Any())
        {
            _context.ClassSessions.AddRange(createdSessions);
            await _context.SaveChangesAsync();
        }

        return createdSessions.Count;
    }

    private static int ConvertToCustomDayOfWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            DayOfWeek.Saturday => 6,
            DayOfWeek.Sunday => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek))
        };
    }
}