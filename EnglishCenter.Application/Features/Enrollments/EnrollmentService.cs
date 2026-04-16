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
        var exists = await _context.Enrollments
            .AnyAsync(x => x.StudentId == request.StudentId && x.ClassId == request.ClassId && !x.IsDeleted);

        if (exists)
        {
            throw new BusinessException("Enrollment already exists for this student and class.");
        }

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
