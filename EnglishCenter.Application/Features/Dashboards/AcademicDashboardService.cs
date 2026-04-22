using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Dashboards.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Dashboards;

public class AcademicDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;

    public AcademicDashboardService(IApplicationDbContext context, ICurrentUserContext currentUserContext)
    {
        _context = context;
        _currentUserContext = currentUserContext;
    }

    public async Task<PagedResult<StudentAtRiskDto>> GetStudentsAtRiskAsync(GetStudentsAtRiskRequestDto request)
    {
        var studentsQuery = _context.Students
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!_currentUserContext.IsSuperAdmin)
        {
            var campusId = _currentUserContext.CampusId;
            studentsQuery = studentsQuery.Where(student =>
                _context.Enrollments.Any(e =>
                    e.StudentId == student.Id &&
                    !e.IsDeleted &&
                    e.Class.CampusId == campusId));
        }

        var students = await studentsQuery
            .OrderBy(x => x.FullName)
            .ToListAsync();

        var items = new List<StudentAtRiskDto>();

        foreach (var student in students)
        {
            var suspendedEnrollmentQuery = _context.Enrollments
                .Where(x => x.StudentId == student.Id && !x.IsDeleted && x.Status == 2);

            var attendanceQuery =
                from attendance in _context.AttendanceRecords.AsNoTracking()
                join session in _context.ClassSessions.AsNoTracking() on attendance.SessionId equals session.Id
                join @class in _context.Classes.AsNoTracking() on session.ClassId equals @class.Id
                where attendance.StudentId == student.Id && !@class.IsDeleted
                select attendance;

            if (!_currentUserContext.IsSuperAdmin)
            {
                var campusId = _currentUserContext.CampusId;
                suspendedEnrollmentQuery = suspendedEnrollmentQuery.Where(x => x.Class.CampusId == campusId);
                attendanceQuery = attendanceQuery.Where(x => x.Session.Class.CampusId == campusId);
            }

            var suspendedEnrollments = await suspendedEnrollmentQuery.CountAsync();

            var totalAttendanceRecords = await attendanceQuery.CountAsync();
            var presentCount = await attendanceQuery.CountAsync(x => x.Status == 1);
            var absentCount = await attendanceQuery.CountAsync(x => x.Status == 2);

            var attendanceRate = totalAttendanceRecords == 0
                ? 0
                : Math.Round((decimal)presentCount * 100 / totalAttendanceRecords, 2);

            var isAtRiskByAttendance = totalAttendanceRecords > 0 && attendanceRate < request.AttendanceThreshold;
            var isAtRiskBySuspension = suspendedEnrollments > 0;

            if (!isAtRiskByAttendance && !isAtRiskBySuspension)
                continue;

            items.Add(new StudentAtRiskDto
            {
                StudentId = student.Id,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                SuspendedEnrollments = suspendedEnrollments,
                TotalAttendanceRecords = totalAttendanceRecords,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                AttendanceRate = attendanceRate,
                IsAtRiskByAttendance = isAtRiskByAttendance,
                IsAtRiskBySuspension = isAtRiskBySuspension
            });
        }

        var totalRecords = items.Count;

        var pagedItems = items
            .OrderByDescending(x => x.IsAtRiskBySuspension)
            .ThenBy(x => x.AttendanceRate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<StudentAtRiskDto>
        {
            Items = pagedItems,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<PagedResult<TeacherWorkloadDto>> GetTeacherWorkloadAsync(GetTeacherWorkloadRequestDto request)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var teachersQuery = _context.Teachers
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!_currentUserContext.IsSuperAdmin)
        {
            var campusId = _currentUserContext.CampusId;
            teachersQuery = teachersQuery.Where(x => x.CampusId == campusId);
        }

        var totalRecords = await teachersQuery.CountAsync();

        var teachers = await teachersQuery
            .OrderBy(x => x.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var items = new List<TeacherWorkloadDto>();

        foreach (var teacher in teachers)
        {
            var sessionQuery = _context.ClassSessions
                .AsNoTracking()
                .Where(x => x.TeacherId == teacher.Id)
                .AsQueryable();

            if (!_currentUserContext.IsSuperAdmin)
            {
                var campusId = _currentUserContext.CampusId;
                sessionQuery = sessionQuery.Where(x => x.Class.CampusId == campusId);
            }

            items.Add(new TeacherWorkloadDto
            {
                TeacherId = teacher.Id,
                TeacherCode = teacher.TeacherCode,
                FullName = teacher.FullName,
                TotalAssignedClasses = await sessionQuery.Select(x => x.ClassId).Distinct().CountAsync(),
                TotalSessions = await sessionQuery.CountAsync(),
                PlannedSessions = await sessionQuery.CountAsync(x => x.Status == 1),
                CompletedSessions = await sessionQuery.CountAsync(x => x.Status == 2),
                CancelledSessions = await sessionQuery.CountAsync(x => x.Status == 3),
                UpcomingSessions = await sessionQuery.CountAsync(x => x.SessionDate > today && x.Status == 1),
                TodaySessions = await sessionQuery.CountAsync(x => x.SessionDate == today && x.Status != 3)
            });
        }

        return new PagedResult<TeacherWorkloadDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<PagedResult<RoomUtilizationDto>> GetRoomUtilizationAsync(GetRoomUtilizationRequestDto request)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var roomsQuery = _context.Rooms
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!_currentUserContext.IsSuperAdmin)
        {
            var campusId = _currentUserContext.CampusId;
            roomsQuery = roomsQuery.Where(x => x.CampusId == campusId);
        }

        var totalRecords = await roomsQuery.CountAsync();

        var rooms = await roomsQuery
            .OrderBy(x => x.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var items = new List<RoomUtilizationDto>();

        foreach (var room in rooms)
        {
            var sessionQuery = _context.ClassSessions
                .AsNoTracking()
                .Where(x => x.RoomId == room.Id)
                .AsQueryable();

            if (!_currentUserContext.IsSuperAdmin)
            {
                var campusId = _currentUserContext.CampusId;
                sessionQuery = sessionQuery.Where(x => x.Class.CampusId == campusId);
            }

            items.Add(new RoomUtilizationDto
            {
                RoomId = room.Id,
                RoomCode = room.RoomCode,
                Name = room.Name,
                Capacity = room.Capacity,
                TotalAssignedClasses = await sessionQuery.Select(x => x.ClassId).Distinct().CountAsync(),
                TotalSessions = await sessionQuery.CountAsync(),
                PlannedSessions = await sessionQuery.CountAsync(x => x.Status == 1),
                CompletedSessions = await sessionQuery.CountAsync(x => x.Status == 2),
                CancelledSessions = await sessionQuery.CountAsync(x => x.Status == 3),
                UpcomingSessions = await sessionQuery.CountAsync(x => x.SessionDate > today && x.Status == 1),
                TodaySessions = await sessionQuery.CountAsync(x => x.SessionDate == today && x.Status != 3)
            });
        }

        return new PagedResult<RoomUtilizationDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<PagedResult<ClassDashboardDto>> GetClassDashboardAsync(GetClassDashboardRequestDto request)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var classesQuery = _context.Classes
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!_currentUserContext.IsSuperAdmin)
        {
            var campusId = _currentUserContext.CampusId;
            classesQuery = classesQuery.Where(x => x.CampusId == campusId);
        }

        var totalRecords = await classesQuery.CountAsync();

        var classes = await classesQuery
            .OrderBy(x => x.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var items = new List<ClassDashboardDto>();

        foreach (var @class in classes)
        {
            var attendanceQuery =
                from a in _context.AttendanceRecords
                join cs in _context.ClassSessions on a.SessionId equals cs.Id
                where cs.ClassId == @class.Id
                select a;

            var totalAttendance = await attendanceQuery.CountAsync();
            var presentAttendance = await attendanceQuery.CountAsync(x => x.Status == 1);

            var attendanceRate = totalAttendance == 0
                ? 0
                : Math.Round((decimal)presentAttendance * 100 / totalAttendance, 2);

            items.Add(new ClassDashboardDto
            {
                ClassId = @class.Id,
                ClassCode = @class.ClassCode,
                ClassName = @class.Name,
                MaxStudents = @class.MaxStudents,
                ActiveEnrollments = await _context.Enrollments.CountAsync(x => x.ClassId == @class.Id && !x.IsDeleted && x.Status == 1),
                SuspendedEnrollments = await _context.Enrollments.CountAsync(x => x.ClassId == @class.Id && !x.IsDeleted && x.Status == 2),
                CompletedEnrollments = await _context.Enrollments.CountAsync(x => x.ClassId == @class.Id && !x.IsDeleted && x.Status == 3),
                TransferredEnrollments = await _context.Enrollments.CountAsync(x => x.ClassId == @class.Id && !x.IsDeleted && x.Status == 4),
                CancelledEnrollments = await _context.Enrollments.CountAsync(x => x.ClassId == @class.Id && !x.IsDeleted && x.Status == 5),
                TotalSessions = await _context.ClassSessions.CountAsync(x => x.ClassId == @class.Id),
                PlannedSessions = await _context.ClassSessions.CountAsync(x => x.ClassId == @class.Id && x.Status == 1),
                CompletedSessions = await _context.ClassSessions.CountAsync(x => x.ClassId == @class.Id && x.Status == 2),
                CancelledSessions = await _context.ClassSessions.CountAsync(x => x.ClassId == @class.Id && x.Status == 3),
                UpcomingSessions = await _context.ClassSessions.CountAsync(x => x.ClassId == @class.Id && x.SessionDate > today && x.Status == 1),
                AttendanceRate = attendanceRate
            });
        }

        return new PagedResult<ClassDashboardDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }
}
