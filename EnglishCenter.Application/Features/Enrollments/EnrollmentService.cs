using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Enrollments.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Enrollments;

public class EnrollmentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public EnrollmentService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task SuspendAsync(long enrollmentId, SuspendEnrollmentRequestDto request)
    {
        var entity = await _context.Enrollments
            .FirstOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDeleted);

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

    public async Task CompleteAsync(long enrollmentId, CompleteEnrollmentRequestDto request)
    {
        var entity = await _context.Enrollments
            .FirstOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDeleted);

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

    public async Task<long> TransferAsync(long enrollmentId, TransferEnrollmentRequestDto request)
    {
        var sourceEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDeleted);

        if (sourceEnrollment == null)
        {
            throw new NotFoundException("Enrollment not found.");
        }

        if (sourceEnrollment.Status != 1)
        {
            throw new BusinessException("Only active enrollment can be transferred.");
        }

        var targetClass = await _context.Classes
            .FirstOrDefaultAsync(x => x.Id == request.TargetClassId && !x.IsDeleted);

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

    public async Task<List<EnrollmentDto>> GetAllAsync()
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
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
        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
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
        // Business rules checks
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId && !s.IsDeleted);
        if (student == null) throw new NotFoundException("Student not found.");

        var cls = await _context.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId && !c.IsDeleted);
        if (cls == null) throw new NotFoundException("Class not found.");

        var activeCount = await _context.Enrollments.CountAsync(x =>
            x.ClassId == request.ClassId &&
            !x.IsDeleted &&
            x.Status == 1);

        if (activeCount >= 10)
        {
            throw new BusinessException("This class already has the maximum of 10 students.");
        }

        // Check class open for registration (assume Status == 1 means open)
        if (cls.Status != 1)
            throw new BusinessException("Class is not open for enrollment.");

        // Check capacity
        var enrolledCount = await _context.Enrollments
            .CountAsync(e => e.ClassId == request.ClassId && !e.IsDeleted);
        if (enrolledCount >= cls.MaxStudents)
            throw new BusinessException("Class has reached maximum number of students.");

        var entity = _mapper.Map<Enrollment>(request);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Enrollments.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateEnrollmentRequestDto request)
    {
        var entity = await _context.Enrollments
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

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
        var entity = await _context.Enrollments
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Enrollment not found.");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
