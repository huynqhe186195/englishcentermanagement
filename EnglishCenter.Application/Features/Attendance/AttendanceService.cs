using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Extensions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Attendance.Dtos;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnglishCenter.Application.Features.Attendance;

public class AttendanceService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceService(
    IApplicationDbContext context,
    IMapper mapper,
    ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }
    // Cho phép giáo viên điểm danh cho một buổi học cụ thể,
    // cập nhật hoặc tạo mới bản ghi điểm danh cho từng sinh viên dựa trên thông tin được cung cấp.
    public async Task MarkAttendanceAsync(MarkAttendanceRequestDto request, long? checkedByUserId = null)
    {
        var session = await _context.ClassSessions
            .FirstOrDefaultAsync(x => x.Id == request.SessionId);

        if (session == null)
        {
            throw new NotFoundException("Class session not found.");
        }

        await ValidateTeacherCanAccessSessionAsync(session);

        if (session.Status == ClassSessionStatusConstants.Cancelled)
        {
            throw new BusinessException("Cannot mark attendance for a cancelled session.");
        }

        if (session.Status == ClassSessionStatusConstants.Completed)
        {
            throw new BusinessException("Cannot modify attendance because the session is already completed.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (today != session.SessionDate)
        {
            throw new BusinessException("Attendance can only be marked or updated on the session date.");
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
                    CheckedByUserId = checkedByUserId ?? _currentUserService.UserId
                };

                _context.AttendanceRecords.Add(attendance);
            }
            else
            {
                attendance.Status = item.Status;
                attendance.Note = item.Note;
                attendance.CheckedAt = DateTime.UtcNow;
                attendance.CheckedByUserId = checkedByUserId ?? _currentUserService.UserId;
            }
        }

        await _context.SaveChangesAsync();
    }
    // Truy xuất danh sách điểm danh cho một buổi học cụ thể, bao gồm thông tin sinh viên và tình trạng điểm danh của họ.
    public async Task<List<SessionAttendanceRosterItemDto>> GetSessionAttendanceRosterAsync(long sessionId)
    {
        var session = await _context.ClassSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            throw new NotFoundException("Class session not found.");
        }

        await ValidateTeacherCanAccessSessionAsync(session);

        var result = await (
            from e in _context.Enrollments
            join s in _context.Students on e.StudentId equals s.Id
            join a in _context.AttendanceRecords.Where(x => x.SessionId == sessionId)
                on s.Id equals a.StudentId into attendanceGroup
            from attendance in attendanceGroup.DefaultIfEmpty()
            where e.ClassId == session.ClassId
                  && !e.IsDeleted
                  && e.Status == 1
                  && !s.IsDeleted
            orderby s.FullName
            select new SessionAttendanceRosterItemDto
            {
                StudentId = s.Id,
                StudentCode = s.StudentCode,
                FullName = s.FullName,
                EnrollmentId = e.Id,
                EnrollmentStatus = e.Status,
                AttendanceStatus = attendance != null ? attendance.Status : null,
                Note = attendance != null ? attendance.Note : null,
                CheckedAt = attendance != null ? attendance.CheckedAt : null
            }
        ).ToListAsync();

        return result;
    }
    // Validates that the current teacher can access the specified class session.
    private async Task ValidateTeacherCanAccessSessionAsync(ClassSession session)
    {
        if (!_currentUserService.IsInRole(RoleConstants.Teacher))
            return;

        if (!_currentUserService.UserId.HasValue)
            throw new BusinessException("User is not authenticated.");

        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _currentUserService.UserId.Value && !x.IsDeleted);

        if (teacher == null)
            throw new BusinessException("Teacher profile not found for current user.");

        if (session.TeacherId != teacher.Id)
            throw new BusinessException("You are not assigned to this session.");
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