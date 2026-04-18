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
using EnglishCenter.Application.Features.Timetables.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnglishCenter.Application.Features.Timetables;

public class TimetableService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public TimetableService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<TimetableItemDto>> GetStudentTimetableAsync(long studentId, GetTimetableRequestDto request)
    {
        var studentExists = await _context.Students
            .AnyAsync(x => x.Id == studentId && !x.IsDeleted);

        if (!studentExists)
        {
            throw new NotFoundException("Student not found.");
        }

        var activeClassIds = await _context.Enrollments
            .Where(x => x.StudentId == studentId && !x.IsDeleted && x.Status == 1)
            .Select(x => x.ClassId)
            .Distinct()
            .ToListAsync();

        var query = _context.ClassSessions
            .AsNoTracking()
            .Where(x => activeClassIds.Contains(x.ClassId))
            .AsQueryable();

        query = ApplyTimetableFilters(query, request);
        query = ApplyTimetableSorting(query, request);

        var totalRecords = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<TimetableItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<TimetableItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<PagedResult<TimetableItemDto>> GetTeacherTimetableAsync(long teacherId, GetTimetableRequestDto request)
    {
        var teacherExists = await _context.Teachers
            .AnyAsync(x => x.Id == teacherId && !x.IsDeleted);

        if (!teacherExists)
        {
            throw new NotFoundException("Teacher not found.");
        }

        var query = _context.ClassSessions
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId)
            .AsQueryable();

        query = ApplyTimetableFilters(query, request);
        query = ApplyTimetableSorting(query, request);

        var totalRecords = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<TimetableItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<TimetableItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<PagedResult<TimetableItemDto>> GetRoomTimetableAsync(long roomId, GetTimetableRequestDto request)
    {
        var roomExists = await _context.Rooms
            .AnyAsync(x => x.Id == roomId && !x.IsDeleted);

        if (!roomExists)
        {
            throw new NotFoundException("Room not found.");
        }

        var query = _context.ClassSessions
            .AsNoTracking()
            .Where(x => x.RoomId == roomId)
            .AsQueryable();

        query = ApplyTimetableFilters(query, request);
        query = ApplyTimetableSorting(query, request);

        var totalRecords = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<TimetableItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<TimetableItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<PagedResult<TimetableItemDto>> GetClassTimetableAsync(long classId, GetTimetableRequestDto request)
    {
        var classExists = await _context.Classes
            .AnyAsync(x => x.Id == classId && !x.IsDeleted);

        if (!classExists)
        {
            throw new NotFoundException("Class not found.");
        }

        var query = _context.ClassSessions
            .AsNoTracking()
            .Where(x => x.ClassId == classId)
            .AsQueryable();

        query = ApplyTimetableFilters(query, request);
        query = ApplyTimetableSorting(query, request);

        var totalRecords = await query.CountAsync();

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<TimetableItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<TimetableItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    private static IQueryable<ClassSession> ApplyTimetableFilters(
        IQueryable<ClassSession> query,
        GetTimetableRequestDto request)
    {
        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.SessionDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.SessionDate <= request.ToDate.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        return query;
    }

    private static IQueryable<ClassSession> ApplyTimetableSorting(
        IQueryable<ClassSession> query,
        GetTimetableRequestDto request)
    {
        var sortMappings = new Dictionary<string, Expression<Func<ClassSession, object>>>
        {
            { "SessionDate", x => x.SessionDate },
            { "StartTime", x => x.StartTime },
            { "EndTime", x => x.EndTime },
            { "SessionNo", x => x.SessionNo },
            { "Status", x => x.Status },
            { "CreatedAt", x => x.CreatedAt }
        };

        return query.ApplySorting(
            request.SortBy,
            request.SortDirection,
            sortMappings,
            x => x.SessionDate);
    }
}
