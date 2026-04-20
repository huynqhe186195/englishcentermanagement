using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Features.FinancialDashboards.Dtos;
using EnglishCenter.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.FinancialDashboards;

public class FinancialDashboardService
{
    private readonly IApplicationDbContext _context;

    public FinancialDashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(GetRevenueDashboardRequestDto request)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= request.ToDate.Value);
        }

        return new RevenueSummaryDto
        {
            TotalInvoices = await query.CountAsync(),
            PaidInvoices = await query.CountAsync(x => x.Status == InvoiceStatusConstants.Paid),
            UnpaidInvoices = await query.CountAsync(x => x.Status == InvoiceStatusConstants.Unpaid),
            CancelledInvoices = await query.CountAsync(x => x.Status == InvoiceStatusConstants.Cancelled),

            TotalExpectedRevenue = await query.SumAsync(x => x.FinalAmount),
            TotalCollectedRevenue = await query.SumAsync(x => x.PaidAmount),
            TotalDiscountAmount = await query.SumAsync(x => x.DiscountAmount),
            TotalRefundedAmount = await query.SumAsync(x => x.RefundedAmount)
        };
    }

    public async Task<List<RevenueByMonthItemDto>> GetRevenueByMonthAsync(GetRevenueDashboardRequestDto request)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= request.ToDate.Value);
        }

        var result = await query
            .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
            .Select(g => new RevenueByMonthItemDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                InvoiceCount = g.Count(),
                PaidInvoiceCount = g.Count(x => x.Status == InvoiceStatusConstants.Paid),
                ExpectedRevenue = g.Sum(x => x.FinalAmount),
                CollectedRevenue = g.Sum(x => x.PaidAmount)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();

        return result;
    }

    public async Task<List<RevenueByCourseItemDto>> GetRevenueByCourseAsync(GetRevenueDashboardRequestDto request)
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ToListAsync();

        if (request.FromDate.HasValue)
        {
            invoices = invoices.Where(x => x.CreatedAt >= request.FromDate.Value).ToList();
        }

        if (request.ToDate.HasValue)
        {
            invoices = invoices.Where(x => x.CreatedAt <= request.ToDate.Value).ToList();
        }

        var invoiceCourseMap = invoices
            .Select(x => new
            {
                Invoice = x,
                CourseId = ExtractCourseIdFromNote(x.Note)
            })
            .Where(x => x.CourseId.HasValue)
            .ToList();

        var courseIds = invoiceCourseMap
            .Select(x => x.CourseId!.Value)
            .Distinct()
            .ToList();

        var courses = await _context.Courses
            .AsNoTracking()
            .Where(x => courseIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        var result = invoiceCourseMap
            .GroupBy(x => x.CourseId!.Value)
            .Select(g => new RevenueByCourseItemDto
            {
                CourseId = g.Key,
                CourseName = courses.ContainsKey(g.Key) ? courses[g.Key] : "Unknown",
                InvoiceCount = g.Count(),
                PaidInvoiceCount = g.Count(x => x.Invoice.Status == InvoiceStatusConstants.Paid),
                ExpectedRevenue = g.Sum(x => x.Invoice.FinalAmount),
                CollectedRevenue = g.Sum(x => x.Invoice.PaidAmount)
            })
            .OrderByDescending(x => x.CollectedRevenue)
            .ToList();

        return result;
    }

    public async Task<List<RevenueByCampusItemDto>> GetRevenueByCampusAsync(GetRevenueDashboardRequestDto request)
    {
        var campuses = await _context.Campuses
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ToListAsync();

        var invoiceQuery =
            from i in _context.Invoices
            join c in _context.Classes on i.ClassId equals c.Id
            where !i.IsDeleted
                  && !c.IsDeleted
                  && i.ClassId != null
            select new
            {
                Invoice = i,
                c.CampusId
            };

        if (request.FromDate.HasValue)
        {
            invoiceQuery = invoiceQuery.Where(x => x.Invoice.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            invoiceQuery = invoiceQuery.Where(x => x.Invoice.CreatedAt <= request.ToDate.Value);
        }

        var invoiceGroups = await invoiceQuery
            .GroupBy(x => x.CampusId)
            .Select(g => new
            {
                CampusId = g.Key,
                InvoiceCount = g.Count(),
                PaidInvoiceCount = g.Count(x => x.Invoice.Status == InvoiceStatusConstants.Paid),
                UnpaidInvoiceCount = g.Count(x => x.Invoice.Status == InvoiceStatusConstants.Unpaid),
                CancelledInvoiceCount = g.Count(x => x.Invoice.Status == InvoiceStatusConstants.Cancelled),
                ExpectedRevenue = g.Sum(x => x.Invoice.FinalAmount),
                CollectedRevenue = g.Sum(x => x.Invoice.PaidAmount),
                DiscountAmount = g.Sum(x => x.Invoice.DiscountAmount),
                RefundedAmount = g.Sum(x => x.Invoice.RefundedAmount)
            })
            .ToListAsync();

        var result = campuses
            .Select(campus =>
            {
                var revenue = invoiceGroups.FirstOrDefault(x => x.CampusId == campus.Id);

                return new RevenueByCampusItemDto
                {
                    CampusId = campus.Id,
                    CampusCode = campus.CampusCode,
                    CampusName = campus.Name,
                    InvoiceCount = revenue?.InvoiceCount ?? 0,
                    PaidInvoiceCount = revenue?.PaidInvoiceCount ?? 0,
                    UnpaidInvoiceCount = revenue?.UnpaidInvoiceCount ?? 0,
                    CancelledInvoiceCount = revenue?.CancelledInvoiceCount ?? 0,
                    ExpectedRevenue = revenue?.ExpectedRevenue ?? 0,
                    CollectedRevenue = revenue?.CollectedRevenue ?? 0,
                    DiscountAmount = revenue?.DiscountAmount ?? 0,
                    RefundedAmount = revenue?.RefundedAmount ?? 0
                };
            })
            .OrderByDescending(x => x.CollectedRevenue)
            .ToList();

        return result;
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

    public async Task<List<TeacherWorkloadByCampusItemDto>> GetTeacherWorkloadByCampusAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var campuses = await _context.Campuses
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ToListAsync();

        var teacherQuery = _context.Teachers
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.CampusId != null);

        var activeClassTeacherQuery =
            from ct in _context.ClassTeachers
            join c in _context.Classes on ct.ClassId equals c.Id
            where !c.IsDeleted
                  && ct.AssignedFrom <= DateTime.Today
                  && (ct.AssignedTo == null || ct.AssignedTo >= DateTime.Today)
            select new
            {
                ct.TeacherId,
                ct.ClassId,
                ct.IsMainTeacher,
                c.CampusId
            };

        var classSessionQuery =
            from cs in _context.ClassSessions
            join c in _context.Classes on cs.ClassId equals c.Id
            where !c.IsDeleted
            select new
            {
                Session = cs,
                c.CampusId
            };

        var result = campuses
            .Select(campus =>
            {
                var campusTeachers = teacherQuery.Where(x => x.CampusId == campus.Id);
                var campusClassTeachers = activeClassTeacherQuery.Where(x => x.CampusId == campus.Id);
                var campusSessions = classSessionQuery.Where(x => x.CampusId == campus.Id);

                return new TeacherWorkloadByCampusItemDto
                {
                    CampusId = campus.Id,
                    CampusCode = campus.CampusCode,
                    CampusName = campus.Name,

                    TeacherCount = campusTeachers.Count(),
                    ActiveTeacherCount = campusTeachers.Count(x => x.Status == 1),

                    TotalAssignedClasses = campusClassTeachers
                        .Select(x => x.ClassId)
                        .Distinct()
                        .Count(),

                    TotalSessions = campusSessions.Count(),
                    PlannedSessions = campusSessions.Count(x => x.Session.Status == ClassSessionStatusConstants.Planned),
                    CompletedSessions = campusSessions.Count(x => x.Session.Status == ClassSessionStatusConstants.Completed),
                    CancelledSessions = campusSessions.Count(x => x.Session.Status == ClassSessionStatusConstants.Cancelled),
                    UpcomingSessions = campusSessions.Count(x =>
                        x.Session.SessionDate > today &&
                        x.Session.Status == ClassSessionStatusConstants.Planned),
                    TodaySessions = campusSessions.Count(x =>
                        x.Session.SessionDate == today &&
                        x.Session.Status != ClassSessionStatusConstants.Cancelled)
                };
            })
            .OrderByDescending(x => x.TotalSessions)
            .ToList();

        return result;
    }

    public async Task<List<RoomUtilizationByCampusItemDto>> GetRoomUtilizationByCampusAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var campuses = await _context.Campuses
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ToListAsync();

        var classQuery = _context.Classes
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        var roomQuery = _context.Rooms
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        var roomSessionQuery =
            from cs in _context.ClassSessions
            join c in _context.Classes on cs.ClassId equals c.Id
            join r in _context.Rooms on cs.RoomId equals r.Id
            where !c.IsDeleted && !r.IsDeleted
            select new
            {
                Session = cs,
                c.CampusId,
                RoomCampusId = r.CampusId
            };

        var result = campuses
            .Select(campus =>
            {
                var campusRooms = roomQuery.Where(x => x.CampusId == campus.Id);
                var campusClasses = classQuery.Where(x => x.CampusId == campus.Id);
                var campusSessions = roomSessionQuery.Where(x => x.CampusId == campus.Id);

                return new RoomUtilizationByCampusItemDto
                {
                    CampusId = campus.Id,
                    CampusCode = campus.CampusCode,
                    CampusName = campus.Name,

                    RoomCount = campusRooms.Count(),
                    ActiveRoomCount = campusRooms.Count(x => x.Status == 1),

                    TotalAssignedClasses = campusClasses.Count(x => x.RoomId != null),
                    TotalSessions = campusSessions.Count(),
                    PlannedSessions = campusSessions.Count(x => x.Session.Status == ClassSessionStatusConstants.Planned),
                    CompletedSessions = campusSessions.Count(x => x.Session.Status == ClassSessionStatusConstants.Completed),
                    CancelledSessions = campusSessions.Count(x => x.Session.Status == ClassSessionStatusConstants.Cancelled),
                    UpcomingSessions = campusSessions.Count(x =>
                        x.Session.SessionDate > today &&
                        x.Session.Status == ClassSessionStatusConstants.Planned),
                    TodaySessions = campusSessions.Count(x =>
                        x.Session.SessionDate == today &&
                        x.Session.Status != ClassSessionStatusConstants.Cancelled)
                };
            })
            .OrderByDescending(x => x.TotalSessions)
            .ToList();

        return result;
    }

    public async Task<List<ClassDashboardByCampusItemDto>> GetClassDashboardByCampusAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var campuses = await _context.Campuses
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ToListAsync();

        var classQuery = _context.Classes
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        var enrollmentQuery =
            from e in _context.Enrollments
            join c in _context.Classes on e.ClassId equals c.Id
            where !e.IsDeleted && !c.IsDeleted
            select new
            {
                Enrollment = e,
                c.CampusId
            };

        var sessionQuery =
            from cs in _context.ClassSessions
            join c in _context.Classes on cs.ClassId equals c.Id
            where !c.IsDeleted
            select new
            {
                Session = cs,
                c.CampusId,
                cs.ClassId
            };

        var attendanceQuery =
            from a in _context.AttendanceRecords
            join cs in _context.ClassSessions on a.SessionId equals cs.Id
            join c in _context.Classes on cs.ClassId equals c.Id
            where !c.IsDeleted
            select new
            {
                Attendance = a,
                c.CampusId
            };

        var result = campuses
            .Select(campus =>
            {
                var campusClasses = classQuery.Where(x => x.CampusId == campus.Id);
                var campusEnrollments = enrollmentQuery.Where(x => x.CampusId == campus.Id);
                var campusSessions = sessionQuery.Where(x => x.CampusId == campus.Id);
                var campusAttendance = attendanceQuery.Where(x => x.CampusId == campus.Id);

                var totalAttendance = campusAttendance.Count();
                var presentAttendance = campusAttendance.Count(x => x.Attendance.Status == AttendanceStatusConstants.Present);

                var attendanceRate = totalAttendance == 0
                    ? 0
                    : Math.Round((decimal)presentAttendance * 100 / totalAttendance, 2);

                return new ClassDashboardByCampusItemDto
                {
                    CampusId = campus.Id,
                    CampusCode = campus.CampusCode,
                    CampusName = campus.Name,

                    ClassCount = campusClasses.Count(),
                    ActiveClassCount = campusClasses.Count(x => x.Status == 1),

                    ActiveEnrollments = campusEnrollments.Count(x => x.Enrollment.Status == EnrollmentStatusConstants.Active),
                    SuspendedEnrollments = campusEnrollments.Count(x => x.Enrollment.Status == EnrollmentStatusConstants.Suspended),
                    CompletedEnrollments = campusEnrollments.Count(x => x.Enrollment.Status == EnrollmentStatusConstants.Completed),
                    TransferredEnrollments = campusEnrollments.Count(x => x.Enrollment.Status == EnrollmentStatusConstants.Transferred),
                    CancelledEnrollments = campusEnrollments.Count(x => x.Enrollment.Status == EnrollmentStatusConstants.Cancelled),

                    TotalSessions = campusSessions.Count(),
                    PlannedSessions = campusSessions.Count(x => x.Session.Status == ClassSessionStatusConstants.Planned),
                    CompletedSessions = campusSessions.Count(x => x.Session.Status == ClassSessionStatusConstants.Completed),
                    CancelledSessions = campusSessions.Count(x => x.Session.Status == ClassSessionStatusConstants.Cancelled),
                    UpcomingSessions = campusSessions.Count(x =>
                        x.Session.SessionDate > today &&
                        x.Session.Status == ClassSessionStatusConstants.Planned),

                    AttendanceRate = attendanceRate
                };
            })
            .OrderByDescending(x => x.ClassCount)
            .ToList();

        return result;
    }
}