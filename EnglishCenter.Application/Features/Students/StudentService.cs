using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Extensions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Students.Dtos;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EnglishCenter.Application.Features.Students;

public class StudentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;

    public StudentService(
    IApplicationDbContext context,
    IMapper mapper,
    IEmailService emailService,
    ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _emailService = emailService;
        _currentUserService = currentUserService;
    }
    // Helper methods to convert status codes to text
    private static string GetAttendanceStatusText(int status)
    {
        return status switch
        {
            AttendanceStatusConstants.Present => "Present",
            AttendanceStatusConstants.Absent => "Absent",
            _ => "Unknown"
        };
    }
    // Helper method to convert class session status code to text
    private static string GetSessionStatusText(int status)
    {
        return status switch
        {
            ClassSessionStatusConstants.Planned => "Planned",
            ClassSessionStatusConstants.Completed => "Completed",
            ClassSessionStatusConstants.Cancelled => "Cancelled",
            ClassSessionStatusConstants.Rescheduled => "Rescheduled",
            _ => "Unknown"
        };
    }
    // Helper method to build the email body for attendance warning
    private static string BuildAttendanceWarningEmailBody(StudentAttendanceReportDto report)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Hello {report.FullName},");
        sb.AppendLine();
        sb.AppendLine("This is an attendance warning from the English Center.");
        sb.AppendLine();
        sb.AppendLine($"Class: {report.ClassName} ({report.ClassCode})");
        sb.AppendLine($"Total valid sessions: {report.TotalValidSessions}");
        sb.AppendLine($"Present count: {report.PresentCount}");
        sb.AppendLine($"Absent count: {report.AbsentCount}");
        sb.AppendLine($"Absent rate: {report.AbsentRate}%");
        sb.AppendLine();
        sb.AppendLine("Your absence rate has exceeded the allowed threshold of 10%.");
        sb.AppendLine("Please contact the academic office if you need support.");
        sb.AppendLine();
        sb.AppendLine("Best regards,");
        sb.AppendLine("English Center");

        return sb.ToString();
    }
    // Lấy báo cáo điểm danh chi tiết của một sinh viên trong một lớp học cụ thể, bao gồm thông tin về từng buổi học,
    // trạng thái điểm danh, và tỷ lệ vắng mặt. Nếu tỷ lệ vắng mặt vượt quá 10%, gửi email cảnh báo đến sinh viên.
    public async Task<StudentAttendanceReportDto> GetAttendanceReportAsync(
    long studentId,
    GetStudentAttendanceReportRequestDto request)
    {
        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == studentId && !x.IsDeleted);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.StudentId == studentId &&
                x.ClassId == request.ClassId &&
                !x.IsDeleted);

        if (enrollment == null)
        {
            throw new NotFoundException("Enrollment not found for this student and class.");
        }

        var @class = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ClassId && !x.IsDeleted);

        if (@class == null)
        {
            throw new NotFoundException("Class not found.");
        }

        var sessions = await _context.ClassSessions
            .AsNoTracking()
            .Where(x =>
                x.ClassId == request.ClassId &&
                x.Status != ClassSessionStatusConstants.Cancelled)
            .OrderBy(x => x.SessionDate)
            .ThenBy(x => x.StartTime)
            .ToListAsync();

        var sessionIds = sessions.Select(x => x.Id).ToList();

        var attendanceRecords = await _context.AttendanceRecords
            .AsNoTracking()
            .Where(x =>
                x.StudentId == studentId &&
                sessionIds.Contains(x.SessionId))
            .ToListAsync();

        var sessionItems = sessions.Select(session =>
        {
            var attendance = attendanceRecords.FirstOrDefault(x => x.SessionId == session.Id);

            var attendanceStatus = attendance?.Status;
            var attendanceStatusText = attendance != null
                ? GetAttendanceStatusText(attendance.Status)
                : "NotMarked";

            // Business rule matrix:
            // - Planned/Future session + no record => NotMarked
            // - Completed session + no record => Absent
            if (attendance == null && session.Status == ClassSessionStatusConstants.Completed)
            {
                attendanceStatus = AttendanceStatusConstants.Absent;
                attendanceStatusText = "Absent";
            }

            return new StudentAttendanceReportSessionItemDto
            {
                SessionId = session.Id,
                SessionNo = session.SessionNo,
                SessionDate = session.SessionDate,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                SessionStatus = session.Status,
                SessionStatusText = GetSessionStatusText(session.Status),
                AttendanceStatus = attendanceStatus,
                AttendanceStatusText = attendanceStatusText,
                Note = attendance?.Note,
                CheckedAt = attendance?.CheckedAt
            };
        }).ToList();

        var totalValidSessions = sessions.Count;
        var presentCount = sessionItems.Count(x => x.AttendanceStatus == AttendanceStatusConstants.Present);
        var absentCount = sessionItems.Count(x => x.AttendanceStatus == AttendanceStatusConstants.Absent);
        var notMarkedCount = sessionItems.Count(x => x.AttendanceStatus == null);

        var absentRate = totalValidSessions == 0
            ? 0
            : Math.Round((decimal)absentCount * 100 / totalValidSessions, 2);

        var isWarning = absentRate > 10;

        var result = new StudentAttendanceReportDto
        {
            StudentId = student.Id,
            StudentCode = student.StudentCode,
            FullName = student.FullName,
            Email = student.Email,
            EnrollmentId = enrollment.Id,
            EnrollmentStatus = enrollment.Status,
            ClassId = @class.Id,
            ClassCode = @class.ClassCode,
            ClassName = @class.Name,
            TotalValidSessions = totalValidSessions,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            NotMarkedCount = notMarkedCount,
            AbsentRate = absentRate,
            IsWarning = isWarning,
            WarningMessage = isWarning
                ? $"Attendance warning: your absence rate is {absentRate}% which exceeds the 10% threshold."
                : null,
            Sessions = sessionItems
        };

        if (request.SendWarningEmail && isWarning && !string.IsNullOrWhiteSpace(student.Email))
        {
            var subject = $"[Attendance Warning] {result.ClassName}";
            var body = BuildAttendanceWarningEmailBody(result);

            await _emailService.SendAsync(student.Email!, subject, body);
        }

        return result;
    }

    public async Task<StudentAcademicSummaryDto> GetAcademicSummaryAsync(long studentId)
    {
        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == studentId && !x.IsDeleted);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        var activeEnrollments = await _context.Enrollments
            .CountAsync(x => x.StudentId == studentId && !x.IsDeleted && x.Status == 1);

        var suspendedEnrollments = await _context.Enrollments
            .CountAsync(x => x.StudentId == studentId && !x.IsDeleted && x.Status == 2);

        var completedEnrollments = await _context.Enrollments
            .CountAsync(x => x.StudentId == studentId && !x.IsDeleted && x.Status == 3);

        var transferredEnrollments = await _context.Enrollments
            .CountAsync(x => x.StudentId == studentId && !x.IsDeleted && x.Status == 4);

        var cancelledEnrollments = await _context.Enrollments
            .CountAsync(x => x.StudentId == studentId && !x.IsDeleted && x.Status == 5);

        var attendanceQuery = _context.AttendanceRecords
            .AsNoTracking()
            .Where(x => x.StudentId == studentId);

        var totalAttendanceRecords = await attendanceQuery.CountAsync();
        var presentCount = await attendanceQuery.CountAsync(x => x.Status == 1);
        var absentCount = await attendanceQuery.CountAsync(x => x.Status == 2);

        var attendanceRate = totalAttendanceRecords == 0
            ? 0
            : Math.Round((decimal)presentCount * 100 / totalAttendanceRecords, 2);

        var activeClassIds = await _context.Enrollments
            .Where(x => x.StudentId == studentId && !x.IsDeleted && x.Status == 1)
            .Select(x => x.ClassId)
            .Distinct()
            .ToListAsync();

        var upcomingSessions = await _context.ClassSessions
            .CountAsync(x =>
                activeClassIds.Contains(x.ClassId) &&
                x.SessionDate > today &&
                x.Status == 1);

        return new StudentAcademicSummaryDto
        {
            StudentId = student.Id,
            StudentCode = student.StudentCode,
            FullName = student.FullName,
            Status = student.Status,
            ActiveEnrollments = activeEnrollments,
            SuspendedEnrollments = suspendedEnrollments,
            CompletedEnrollments = completedEnrollments,
            TransferredEnrollments = transferredEnrollments,
            CancelledEnrollments = cancelledEnrollments,
            TotalAttendanceRecords = totalAttendanceRecords,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            AttendanceRate = attendanceRate,
            UpcomingSessions = upcomingSessions
        };
    }

    

    public async Task<List<StudentDto>> GetAllAsync()
    {
        return await _context.Students
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<StudentDto>> GetPagedAsync(GetStudentsPagingRequestDto request)
    {
        var query = _context.Students
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();

            query = query.Where(x =>
                x.StudentCode.ToLower().Contains(keyword) ||
                x.FullName.ToLower().Contains(keyword) ||
                (x.Phone != null && x.Phone.ToLower().Contains(keyword)) ||
                (x.Email != null && x.Email.ToLower().Contains(keyword)));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var sortMappings = new Dictionary<string, System.Linq.Expressions.Expression<Func<Student, object>>>
    {
        { "Id", x => x.Id },
        { "StudentCode", x => x.StudentCode },
        { "FullName", x => x.FullName },
        { "Email", x => x.Email! },
        { "Phone", x => x.Phone! },
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
            .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<StudentDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<StudentDetailDto> GetByIdAsync(long id)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<StudentDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        return student;
    }

    public async Task<long> CreateAsync(CreateStudentRequestDto request)
    {
        var fullName = request.FullName.Trim();
        var requestedCode = request.StudentCode?.Trim();
        var studentCode = string.IsNullOrWhiteSpace(requestedCode)
            ? await GenerateStudentCodeAsync(request.UserId)
            : requestedCode;

        var exists = await _context.Students
            .AnyAsync(x => x.StudentCode == studentCode && !x.IsDeleted);

        if (exists)
        {
            throw new BusinessException("StudentCode already exists.");
        }

        if (request.UserId.HasValue && request.UserId.Value > 0)
        {
            var userExists = await _context.Users
                .AnyAsync(x => x.Id == request.UserId.Value && !x.IsDeleted);

            if (!userExists)
            {
                throw new BusinessException("User not found.");
            }

            var studentProfileExists = await _context.Students
                .AnyAsync(x => x.UserId == request.UserId.Value && !x.IsDeleted);

            if (studentProfileExists)
            {
                throw new BusinessException("User already has a student profile.");
            }
        }

        var entity = _mapper.Map<Student>(request);

        entity.StudentCode = studentCode;
        entity.FullName = fullName;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Students.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    private async Task<string> GenerateStudentCodeAsync(long? userId)
    {
        var baseCode = userId.HasValue && userId.Value > 0
            ? $"STU{userId.Value:D6}"
            : $"STU{DateTime.UtcNow:yyyyMMddHHmmss}";

        var candidate = baseCode;
        var suffix = 1;

        while (await _context.Students.AnyAsync(x => x.StudentCode == candidate && !x.IsDeleted))
        {
            candidate = $"{baseCode}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    public async Task<bool> UpdateAsync(long id, UpdateStudentRequestDto request)
    {
        var entity = await _context.Students
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Student not found.");
        }

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCurrentStudentProfileAsync(UpdateStudentRequestDto request)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new BusinessException("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var entity = await _context.Students
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Student profile not found.");
        }

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Students
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Student not found.");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
