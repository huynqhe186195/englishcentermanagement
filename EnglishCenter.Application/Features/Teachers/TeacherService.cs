using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Teachers.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Teachers;

public class TeacherService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public TeacherService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TeacherSummaryDto> GetSummaryAsync(long teacherId)
    {
        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == teacherId && !x.IsDeleted);

        if (teacher == null)
        {
            throw new NotFoundException("Teacher not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        var sessionQuery = _context.ClassSessions
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId);

        var totalAssignedClasses = await sessionQuery
            .Select(x => x.ClassId)
            .Distinct()
            .CountAsync();

        var totalSessions = await sessionQuery.CountAsync();
        var plannedSessions = await sessionQuery.CountAsync(x => x.Status == 1);
        var completedSessions = await sessionQuery.CountAsync(x => x.Status == 2);
        var cancelledSessions = await sessionQuery.CountAsync(x => x.Status == 3);
        var upcomingSessions = await sessionQuery.CountAsync(x => x.SessionDate > today && x.Status == 1);
        var todaySessions = await sessionQuery.CountAsync(x => x.SessionDate == today && x.Status != 3);

        return new TeacherSummaryDto
        {
            TeacherId = teacher.Id,
            TeacherCode = teacher.TeacherCode,
            FullName = teacher.FullName,
            Status = teacher.Status,
            TotalAssignedClasses = totalAssignedClasses,
            TotalSessions = totalSessions,
            PlannedSessions = plannedSessions,
            CompletedSessions = completedSessions,
            CancelledSessions = cancelledSessions,
            UpcomingSessions = upcomingSessions,
            TodaySessions = todaySessions
        };
    }

    public async Task<List<TeacherDto>> GetAllAsync()
    {
        return await _context.Teachers
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ProjectTo<TeacherDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<TeacherDto>> GetPagedAsync(GetTeachersPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Teachers
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            query = query.Where(x => x.TeacherCode.ToLower().Contains(keyword) || x.FullName.ToLower().Contains(keyword));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TeacherDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<TeacherDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
        };
    }

    public async Task<TeacherDetailDto> GetByIdAsync(long id)
    {
        var teacher = await _context.Teachers
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<TeacherDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (teacher == null) throw new NotFoundException("Teacher not found.");
        return teacher;
    }

    public async Task<long> CreateAsync(CreateTeacherRequestDto request)
    {
        var code = request.TeacherCode?.Trim() ?? string.Empty;
        var exists = await _context.Teachers.AnyAsync(x => x.TeacherCode == code && !x.IsDeleted);
        if (exists) throw new BusinessException("TeacherCode already exists.");

        var entity = _mapper.Map<Teacher>(request);
        entity.TeacherCode = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Teachers.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateTeacherRequestDto request)
    {
        var entity = await _context.Teachers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("Teacher not found.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Teachers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("Teacher not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
