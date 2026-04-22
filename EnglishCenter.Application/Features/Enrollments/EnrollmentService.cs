using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Enrollments.Dtos;
using EnglishCenter.Domain.Constants;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using EnglishCenter.Application.Commons.Helpers;

namespace EnglishCenter.Application.Features.Enrollments;

public class EnrollmentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly HelperMethodEnrollments _helperMethodEnrollments;
    private readonly ICurrentUserService _currentUserService;

    public EnrollmentService(
    IApplicationDbContext context,
    IMapper mapper,
    IEmailService emailService,
    HelperMethodEnrollments helperMethodEnrollments,
    ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _emailService = emailService;
        _helperMethodEnrollments = helperMethodEnrollments;
        _currentUserService = currentUserService;
    }
    // Đánh giá chính sách điểm danh cho tất cả sinh viên trong một lớp học cụ thể,
    // tính toán tỷ lệ vắng mặt của từng sinh viên dựa trên số buổi học đã lên lịch và số buổi vắng mặt,
    // sau đó cập nhật trạng thái của bản ghi enrollment tương ứng (ví dụ: cảnh báo nếu vắng mặt trên 10%,
    // đình chỉ nếu vắng mặt trên 20%) và trả về kết quả đánh giá cho từng sinh viên.
    public async Task<List<EnrollmentAttendancePolicyResultDto>> EvaluateAttendancePolicyByClassAsync(long classId)
    {
        var @class = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == classId && !x.IsDeleted);

        if (@class == null)
        {
            throw new NotFoundException("Class not found.");
        }

        var validSessionCount = await _context.ClassSessions.CountAsync(x =>
            x.ClassId == classId &&
            x.Status != ClassSessionStatusConstants.Cancelled);

        if (validSessionCount == 0)
        {
            return [];
        }

        var enrollments = await (
            from e in _context.Enrollments
            join s in _context.Students on e.StudentId equals s.Id
            where e.ClassId == classId
                  && !e.IsDeleted
                  && !s.IsDeleted
                  && (e.Status == EnrollmentStatusConstants.Active
                      || e.Status == EnrollmentStatusConstants.Suspended)
            select new
            {
                Enrollment = e,
                Student = s
            }
        ).ToListAsync();

        var results = new List<EnrollmentAttendancePolicyResultDto>();

        foreach (var item in enrollments)
        {
            var absentCount = await (
                from a in _context.AttendanceRecords
                join cs in _context.ClassSessions on a.SessionId equals cs.Id
                where cs.ClassId == classId
                      && cs.Status != ClassSessionStatusConstants.Cancelled
                      && a.StudentId == item.Enrollment.StudentId
                      && a.Status == AttendanceStatusConstants.Absent
                select a.Id
            ).CountAsync();

            var absentRate = Math.Round((decimal)absentCount * 100 / validSessionCount, 2);

            var isWarning = absentRate > 10 && absentRate <= 20;
            var isSuspended = absentRate > 20;
            var warningEmailSentNow = false;

            // Warning mail: chi gui 1 lan dau tien
            if (absentRate > 10 &&
                    !_helperMethodEnrollments.HasAttendanceWarningSent(item.Enrollment.Note) &&
                    !string.IsNullOrWhiteSpace(item.Student.Email))
            {
                var subject = $"[Attendance Warning] {@class.Name}";
                var body = _helperMethodEnrollments.BuildAttendanceWarningEmailBody(
                    item.Student.FullName,
                    @class.Name,
                    @class.ClassCode,
                    absentRate,
                    absentCount,
                    validSessionCount);

                try
                {
                    await _emailService.SendAsync(item.Student.Email!, subject, body);

                    item.Enrollment.Note = _helperMethodEnrollments.AppendAttendanceWarningMarker(item.Enrollment.Note);
                    item.Enrollment.UpdatedAt = DateTime.UtcNow;
                    warningEmailSentNow = true;
                }
                catch
                {
                    warningEmailSentNow = false;
                    // Có thể log warning ở đây nếu bạn có ILogger
                    // Không throw lại để tránh fail toàn bộ flow complete session
                }
            }

            // Suspend neu > 20%
            if (item.Enrollment.Status == EnrollmentStatusConstants.Active && isSuspended)
            {
                var baseNote = item.Enrollment.Note ?? string.Empty;

                var suspendMessage =
                    $"Suspended because absent {absentCount}/{validSessionCount} sessions ({absentRate}%). Exceeded 20% threshold.";

                if (!baseNote.Contains("Suspended because absent", StringComparison.OrdinalIgnoreCase))
                {
                    item.Enrollment.Note = string.IsNullOrWhiteSpace(baseNote)
                        ? suspendMessage
                        : $"{baseNote} {suspendMessage}";
                }

                item.Enrollment.Status = EnrollmentStatusConstants.Suspended;
                item.Enrollment.UpdatedAt = DateTime.UtcNow;
            }
            else if (item.Enrollment.Status == EnrollmentStatusConstants.Active && isWarning)
            {
                var baseNote = item.Enrollment.Note ?? string.Empty;
                var warningMessage =
                    $"Warning: absent {absentCount}/{validSessionCount} sessions ({absentRate}%). Exceeded 10% threshold.";

                if (!baseNote.Contains("Warning: absent", StringComparison.OrdinalIgnoreCase))
                {
                    item.Enrollment.Note = string.IsNullOrWhiteSpace(baseNote)
                        ? warningMessage
                        : $"{baseNote} {warningMessage}";
                    item.Enrollment.UpdatedAt = DateTime.UtcNow;
                }
            }

            results.Add(new EnrollmentAttendancePolicyResultDto
            {
                EnrollmentId = item.Enrollment.Id,
                StudentId = item.Student.Id,
                StudentCode = item.Student.StudentCode,
                FullName = item.Student.FullName,
                ValidSessionCount = validSessionCount,
                AbsentCount = absentCount,
                AbsentRate = absentRate,
                IsWarning = isWarning,
                IsSuspended = isSuspended,
                WarningEmailSentNow = warningEmailSentNow,
                Message = isSuspended
                     ? "Student exceeded 20% absence rate and has been suspended."
                    : absentRate > 10
                        ? warningEmailSentNow
                             ? "Student exceeded 10% absence rate and warning email was sent."
                         : "Student exceeded 10% absence rate, but warning email could not be sent."
                        : "Attendance is within allowed threshold."
            });
        }

        await _context.SaveChangesAsync();

        return results;
    }

    public async Task SuspendAsync(long enrollmentId, SuspendEnrollmentRequestDto request)
    {
        var query = _context.Enrollments
            .Where(x => x.Id == enrollmentId && !x.IsDeleted)
            .AsQueryable();
        query = ApplyCampusScope(query);

        var entity = await query.FirstOrDefaultAsync();

        if (entity == null)
        {
            throw new NotFoundException("Enrollment not found.");
        }

        if (entity.Status != 1)
        {
            throw new BusinessException("Only active enrollment can be suspended.");
        }

        entity.Status = 2;
        entity.Note = request.Reason.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
    public async Task<long> TransferAsync(long enrollmentId, TransferEnrollmentRequestDto request)
    {
        var sourceQuery = _context.Enrollments
            .Where(x => x.Id == enrollmentId && !x.IsDeleted)
            .AsQueryable();
        sourceQuery = ApplyCampusScope(sourceQuery);

        var sourceEnrollment = await sourceQuery.FirstOrDefaultAsync();

        if (sourceEnrollment == null)
        {
            throw new NotFoundException("Enrollment not found.");
        }

        if (sourceEnrollment.Status != 1)
        {
            throw new BusinessException("Only active enrollment can be transferred.");
        }

        var targetClass = await _context.Classes
            .FirstOrDefaultAsync(x =>
                x.Id == request.TargetClassId
                && !x.IsDeleted
                && (!IsCampusScoped() || x.CampusId == GetRequiredCampusId()));

        if (targetClass == null)
        {
            throw new NotFoundException("Target class not found.");
        }

        if (sourceEnrollment.ClassId == request.TargetClassId)
        {
            throw new BusinessException("Target class must be different from current class.");
        }

        var alreadyExists = await _context.Enrollments.AnyAsync(x =>
            x.StudentId == sourceEnrollment.StudentId &&
            x.ClassId == request.TargetClassId &&
            !x.IsDeleted &&
            x.Status == 1);

        if (alreadyExists)
        {
            throw new BusinessException("Student is already actively enrolled in target class.");
        }

        var activeCount = await _context.Enrollments.CountAsync(x =>
                x.ClassId == request.TargetClassId &&
                !x.IsDeleted &&
                x.Status == 1);

        if (activeCount >= 10)
        {
            throw new BusinessException("Target class already has the maximum of 10 students.");
        }

        sourceEnrollment.Status = 4;
        sourceEnrollment.Note = request.Note;
        sourceEnrollment.UpdatedAt = DateTime.UtcNow;

        var newEnrollment = new Enrollment
        {
            StudentId = sourceEnrollment.StudentId,
            ClassId = request.TargetClassId,
            EnrollDate = DateOnly.FromDateTime(DateTime.Today),
            Status = 1,
            Note = $"Transferred from enrollment {sourceEnrollment.Id}. {request.Note}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };

        _context.Enrollments.Add(newEnrollment);
        await _context.SaveChangesAsync();

        return newEnrollment.Id;
    }

    public async Task CompleteAsync(long enrollmentId, CompleteEnrollmentRequestDto request)
    {
        var query = _context.Enrollments
            .Where(x => x.Id == enrollmentId && !x.IsDeleted)
            .AsQueryable();
        query = ApplyCampusScope(query);

        var entity = await query.FirstOrDefaultAsync();

        if (entity == null)
        {
            throw new NotFoundException("Enrollment not found.");
        }

        if (entity.Status != 1)
        {
            throw new BusinessException("Only active enrollment can be completed.");
        }

        entity.Status = 3;
        entity.Note = request.Note;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }



    public async Task<List<EnrollmentDto>> GetAllAsync()
    {
        var query = _context.Enrollments
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        query = ApplyCampusScope(query);

        return await query
            .ProjectTo<EnrollmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<EnrollmentDto>> GetPagedAsync(GetEnrollmentsPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Enrollments
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        query = ApplyCampusScope(query);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();

            query = query.Where(x =>
                (x.Student != null && x.Student.FullName.ToLower().Contains(keyword)) ||
                (x.Class != null && x.Class.Name.ToLower().Contains(keyword)) ||
                (x.Note != null && x.Note.ToLower().Contains(keyword)));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<EnrollmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<EnrollmentDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
        };
    }

    public async Task<EnrollmentDetailDto> GetByIdAsync(long id)
    {
        var query = _context.Enrollments
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .AsQueryable();

        query = ApplyCampusScope(query);

        var enrollment = await query
            .ProjectTo<EnrollmentDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (enrollment == null)
        {
            throw new NotFoundException("Enrollment not found.");
        }

        return enrollment;
    }

    public async Task<long> CreateAsync(CreateEnrollmentRequestDto request)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && !s.IsDeleted);

        if (student == null)
        {
            throw new BusinessException("Student does not exist or has been deleted.");
        }

        var cls = await _context.Classes.FirstOrDefaultAsync(c =>
            c.Id == request.ClassId
            && !c.IsDeleted
            && (!IsCampusScoped() || c.CampusId == GetRequiredCampusId()));
        if (cls == null) throw new NotFoundException("Class not found.");

        if (cls == null)
        {
            throw new BusinessException("Class does not exist or has been deleted.");
        }

        // assume Status == 1 means open for registration
        if (cls.Status != 1)
        {
            throw new BusinessException("Class is not open for enrollment.");
        }

        var enrolledCount = await _context.Enrollments
            .CountAsync(e => e.ClassId == request.ClassId && !e.IsDeleted);

        if (enrolledCount >= cls.MaxStudents)
        {
            throw new BusinessException("Class is already full.");
        }

        var already = await _context.Enrollments
            .AnyAsync(e => e.ClassId == request.ClassId
                        && e.StudentId == request.StudentId
                        && !e.IsDeleted);

        if (already)
        {
            throw new BusinessException("Student is already enrolled in this class.");
        }

        var entity = new Enrollment
        {
            StudentId = request.StudentId,
            ClassId = request.ClassId,
            EnrollDate = request.EnrollDate,
            Note = request.Note,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };

        _context.Enrollments.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateEnrollmentRequestDto request)
    {
        var query = _context.Enrollments
            .Where(x => x.Id == id && !x.IsDeleted)
            .AsQueryable();

        query = ApplyCampusScope(query);

        var entity = await query.FirstOrDefaultAsync();

        if (entity == null)
        {
            throw new NotFoundException("Enrollment not found.");
        }

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var query = _context.Enrollments
            .Where(x => x.Id == id && !x.IsDeleted)
            .AsQueryable();

        query = ApplyCampusScope(query);

        var entity = await query.FirstOrDefaultAsync();

        if (entity == null)
        {
            throw new NotFoundException("Enrollment not found.");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private IQueryable<Enrollment> ApplyCampusScope(IQueryable<Enrollment> query)
    {
        if (IsCampusScoped())
        {
            var campusId = GetRequiredCampusId();
            query = query.Where(x => x.Class != null && x.Class.CampusId == campusId);
        }

        return query;
    }

    private bool IsCampusScoped()
    {
        if (_currentUserService.IsInRole(RoleConstants.SuperAdmin))
        {
            return false;
        }

        return _currentUserService.IsInRole(RoleConstants.CenterAdmin)
            || _currentUserService.IsInRole(RoleConstants.Manager)
            || _currentUserService.IsInRole(RoleConstants.Admin);
    }

    private long GetRequiredCampusId()
    {
        if (!_currentUserService.CampusId.HasValue)
        {
            throw new BusinessException("Current admin does not have a campus assigned.");
        }

        return _currentUserService.CampusId.Value;
    }
}
