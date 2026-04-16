using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Courses.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Courses;

public class CourseService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CourseService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<CourseDto>> GetPagedAsync(GetCoursesPagingRequestDto request)
    {
        var query = _context.Courses
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();

            query = query.Where(x =>
                x.CourseCode.ToLower().Contains(keyword) ||
                x.Name.ToLower().Contains(keyword) ||
                (x.Level != null && x.Level.ToLower().Contains(keyword)));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<CourseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<CourseDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
        };
    }

    public async Task<CourseDetailDto> GetByIdAsync(long id)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<CourseDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (course == null)
        {
            throw new NotFoundException("Course not found.");
        }

        return course;
    }

    public async Task<long> CreateAsync(CreateCourseRequestDto request)
    {
        var courseCode = request.CourseCode.Trim();
        var name = request.Name.Trim();

        var exists = await _context.Courses
            .AnyAsync(x => x.CourseCode == courseCode && !x.IsDeleted);

        if (exists)
        {
            throw new BusinessException("CourseCode already exists.");
        }

        var entity = _mapper.Map<Course>(request);

        entity.CourseCode = courseCode;
        entity.Name = name;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Courses.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateAsync(long id, UpdateCourseRequestDto request)
    {
        var entity = await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Course not found.");
        }

        entity.Name = request.Name.Trim();

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Course not found.");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}