using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Extensions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.ClassSessions.Dtos;
using EnglishCenter.Application.Features.Enrollments;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static EnglishCenter.Domain.Constants.PermissionConstants;

namespace EnglishCenter.Application.Features.ClassSessions;

public class ClassSessionService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly SessionConflictService _sessionConflictService;
    private readonly ICurrentUserService _currentUserService;
    private readonly EnrollmentService _enrollmentService;

    public ClassSessionService(
    IApplicationDbContext context,
    IMapper mapper,
    SessionConflictService sessionConflictService,
    ICurrentUserService currentUserService,
    EnrollmentService enrollmentService)
    {
        _context = context;
        _mapper = mapper;
        _sessionConflictService = sessionConflictService;
        _currentUserService = currentUserService;
        _enrollmentService = enrollmentService;
    }
    // Cho phép giáo viên lên lịch lại một buổi học cụ thể,
    public async Task RescheduleAsync(long sessionId, RescheduleClassSessionRequestDto request)
    {
        var session = await _context.ClassSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            throw new NotFoundException("Class session not found.");
        }

        await ValidateTeacherCanManageSessionAsync(session);

        if (session.Status == ClassSessionStatusConstants.Completed)
        {
            throw new BusinessException("Completed session cannot be rescheduled.");
        }

        if (session.Status == ClassSessionStatusConstants.Cancelled)
        {
            throw new BusinessException("Cancelled session cannot be rescheduled.");
        }

        var @class = await _context.Classes
            .FirstOrDefaultAsync(x => x.Id == session.ClassId && !x.IsDeleted);

        if (@class == null)
        {
            throw new NotFoundException("Class not found.");
        }

        if (request.SessionDate < @class.StartDate || request.SessionDate > @class.EndDate)
        {
            throw new BusinessException("Rescheduled date must be within the class date range.");
        }

        await _sessionConflictService.ValidateSessionConflictsAsync(
            request.TeacherId,
            request.RoomId,
            request.SessionDate,
            request.StartTime,
            request.EndTime,
            sessionId);

        session.SessionDate = request.SessionDate;
        session.StartTime = request.StartTime;
        session.EndTime = request.EndTime;
        session.RoomId = request.RoomId;
        session.TeacherId = request.TeacherId;
        session.Note = request.Note;
        session.Status = ClassSessionStatusConstants.Rescheduled;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
    // Cho phép giáo viên hủy một buổi học cụ thể,
    public async Task CancelAsync(long sessionId, CancelClassSessionRequestDto request)
    {
        var session = await _context.ClassSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            throw new NotFoundException("Class session not found.");
        }

        await ValidateTeacherCanManageSessionAsync(session);

        if (session.Status == ClassSessionStatusConstants.Completed)
        {
            throw new BusinessException("Completed session cannot be cancelled.");
        }

        if (session.Status == ClassSessionStatusConstants.Cancelled)
        {
            throw new BusinessException("Session is already cancelled.");
        }

        session.Status = ClassSessionStatusConstants.Cancelled;
        session.Note = request.Reason.Trim();
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
    // Cho phép giáo viên hoàn thành một buổi học cụ thể,
    // cập nhật trạng thái của buổi học thành "Completed" và lưu lại ghi chú nếu có.
    public async Task CompleteAsync(long sessionId, CompleteClassSessionRequestDto request)
    {
        var session = await _context.ClassSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            throw new NotFoundException("Class session not found.");
        }

        await ValidateTeacherCanManageSessionAsync(session);

        if (session.Status == ClassSessionStatusConstants.Completed)
        {
            throw new BusinessException("Session is already completed.");
        }

        if (session.Status == ClassSessionStatusConstants.Cancelled)
        {
            throw new BusinessException("Cancelled session cannot be completed.");
        }

        session.Status = ClassSessionStatusConstants.Completed;

        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            session.Note = request.Note.Trim();
        }

        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _enrollmentService.EvaluateAttendancePolicyByClassAsync(session.ClassId);
    }
    // Cho phép giáo viên quản lý buổi học (chỉ những buổi học mà họ được phân công giảng dạy),
    private async Task ValidateTeacherCanManageSessionAsync(ClassSession session)
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

    // Tự động sinh các buổi học cho một lớp học dựa trên lịch học đã được thiết lập trước đó,
    public async Task<int> GenerateSessionsAsync(GenerateClassSessionsRequestDto request)
    {
        var @class = await _context.Classes
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == request.ClassId && !x.IsDeleted);

        if (@class == null)
        {
            throw new NotFoundException("Class not found.");
        }

        if (@class.Course == null)
        {
            throw new BusinessException("Class must belong to a course.");
        }

        var totalSessions = @class.Course.TotalSessions;

        if (totalSessions <= 0)
        {
            throw new BusinessException("Course.TotalSessions must be greater than 0.");
        }

        var schedules = await _context.ClassSchedules
            .AsNoTracking()
            .Where(x => x.ClassId == request.ClassId)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToListAsync();

        if (!schedules.Any())
        {
            throw new BusinessException("No class schedules found.");
        }

        var existingSessions = await _context.ClassSessions
            .Where(x => x.ClassId == request.ClassId)
            .ToListAsync();

        if (existingSessions.Count >= totalSessions)
        {
            throw new BusinessException(
                $"All sessions have already been generated. Target is {totalSessions} sessions.");
        }

        var existingSet = existingSessions
            .Select(x => $"{x.SessionDate:yyyy-MM-dd}_{x.StartTime}")
            .ToHashSet();

        var currentMaxSessionNo = existingSessions.Any()
            ? existingSessions.Max(x => x.SessionNo)
            : 0;

        // 1. Sinh tất cả candidate slots hợp lệ trong khoảng StartDate -> EndDate
        var candidateSlots = new List<(DateOnly SessionDate, TimeOnly StartTime, TimeOnly EndTime, long? RoomId)>();
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

                candidateSlots.Add((
                    currentDate,
                    schedule.StartTime,
                    schedule.EndTime,
                    schedule.RoomId
                ));
            }

            currentDate = currentDate.AddDays(1);
        }

        // 2. Sort candidate slots theo ngày và giờ
        candidateSlots = candidateSlots
            .OrderBy(x => x.SessionDate)
            .ThenBy(x => x.StartTime)
            .ToList();

        // 3. Tính số session còn thiếu cần generate
        var remainingSessionsToGenerate = totalSessions - existingSessions.Count;

        // 4. Nếu không đủ slot thì báo lỗi
        if (candidateSlots.Count < remainingSessionsToGenerate)
        {
            throw new BusinessException(
                $"Schedule is not sufficient to generate {totalSessions} sessions within the selected date range. " +
                $"Existing sessions: {existingSessions.Count}, remaining required: {remainingSessionsToGenerate}, available slots: {candidateSlots.Count}.");
        }

        // 5. Chỉ lấy đúng số slot cần thiết
        var selectedSlots = candidateSlots
            .Take(remainingSessionsToGenerate)
            .ToList();

        // 6. Tạo session từ selected slots
        var createdSessions = new List<ClassSession>();

        foreach (var slot in selectedSlots)
        {
            await _sessionConflictService.ValidateRoomConflictAsync(
                slot.RoomId,
                slot.SessionDate,
                slot.StartTime,
                slot.EndTime);

            currentMaxSessionNo++;

            createdSessions.Add(new ClassSession
            {
                ClassId = request.ClassId,
                SessionNo = currentMaxSessionNo,
                SessionDate = slot.SessionDate,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                RoomId = slot.RoomId,
                TeacherId = null,
                Topic = null,
                Note = null,
                Status = ClassSessionStatusConstants.Planned,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });
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