using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Extensions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Attendance.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnglishCenter.Application.Features.Attendance;

public class AttendanceService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AttendanceService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<AttendanceRecordDto>> GetPagedAsync(GetAttendancePagingRequestDto request)
    {
        var query = _context.AttendanceRecords
            .AsNoTracking()
            .AsQueryable();

        if (request.SessionId.HasValue)
        {
            query = query.Where(x => x.SessionId == request.SessionId.Value);
        }

        if (request.StudentId.HasValue)
        {
            query = query.Where(x => x.StudentId == request.StudentId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var sortMappings = new Dictionary<string, Expression<Func<AttendanceRecord, object>>>
        {
            { "Id", x => x.Id },
            { "SessionId", x => x.SessionId },
            { "StudentId", x => x.StudentId },
            { "Status", x => x.Status },
            { "CheckedAt", x => x.CheckedAt }
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
            .ProjectTo<AttendanceRecordDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<AttendanceRecordDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<List<AttendanceRecordDto>> GetBySessionIdAsync(long sessionId)
    {
        var sessionExists = await _context.ClassSessions.AnyAsync(x => x.Id == sessionId);
        if (!sessionExists)
        {
            throw new NotFoundException("Class session not found.");
        }

        return await _context.AttendanceRecords
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.StudentId)
            .ProjectTo<AttendanceRecordDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<List<AttendanceRecordDto>> GetByStudentIdAsync(long studentId)
    {
        var studentExists = await _context.Students.AnyAsync(x => x.Id == studentId && !x.IsDeleted);
        if (!studentExists)
        {
            throw new NotFoundException("Student not found.");
        }

        return await _context.AttendanceRecords
            .AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.CheckedAt)
            .ProjectTo<AttendanceRecordDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task MarkAttendanceAsync(MarkAttendanceRequestDto request, long? checkedByUserId = null)
    {
        var session = await _context.ClassSessions
            .FirstOrDefaultAsync(x => x.Id == request.SessionId);

        if (session == null)
        {
            throw new NotFoundException("Class session not found.");
        }

        foreach (var item in request.Items)
        {
            var isEnrolled = await _context.Enrollments.AnyAsync(x =>
                x.StudentId == item.StudentId &&
                x.ClassId == session.ClassId &&
                !x.IsDeleted &&
                x.Status == 1);

            if (!isEnrolled)
            {
                throw new BusinessException($"Student {item.StudentId} is not actively enrolled in this class.");
            }

            var attendance = await _context.AttendanceRecords
                .FirstOrDefaultAsync(x => x.SessionId == request.SessionId && x.StudentId == item.StudentId);

            if (attendance == null)
            {
                attendance = new AttendanceRecord
                {
                    SessionId = request.SessionId,
                    StudentId = item.StudentId,
                    Status = item.Status,
                    Note = item.Note,
                    CheckedAt = DateTime.UtcNow,
                    CheckedByUserId = checkedByUserId
                };

                _context.AttendanceRecords.Add(attendance);
            }
            else
            {
                attendance.Status = item.Status;
                attendance.Note = item.Note;
                attendance.CheckedAt = DateTime.UtcNow;
                attendance.CheckedByUserId = checkedByUserId;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<AttendanceSummaryDto> GetStudentSummaryAsync(long studentId)
    {
        var studentExists = await _context.Students.AnyAsync(x => x.Id == studentId && !x.IsDeleted);
        if (!studentExists)
        {
            throw new NotFoundException("Student not found.");
        }

        var query = _context.AttendanceRecords.Where(x => x.StudentId == studentId);

        var presentCount = await query.CountAsync(x => x.Status == 1);
        var absentCount = await query.CountAsync(x => x.Status == 2);
        var totalSessions = await query.CountAsync();

        return new AttendanceSummaryDto
        {
            StudentId = studentId,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            TotalSessions = totalSessions
        };
    }
}