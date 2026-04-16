using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Students.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Students;

public class StudentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public StudentService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

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

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<StudentDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
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
        var studentCode = request.StudentCode.Trim();
        var fullName = request.FullName.Trim();

        var exists = await _context.Students
            .AnyAsync(x => x.StudentCode == studentCode && !x.IsDeleted);

        if (exists)
        {
            throw new BusinessException("StudentCode already exists.");
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