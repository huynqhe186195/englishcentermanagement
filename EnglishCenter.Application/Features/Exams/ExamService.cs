using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Exams.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Exams;

public class ExamService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ExamService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ExamDto>> GetAllAsync()
    {
        return await _context.Exams
            .AsNoTracking()
            .ProjectTo<ExamDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<ExamDto>> GetPagedAsync(GetExamsPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Exams
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(keyword) || x.Description != null && x.Description.ToLower().Contains(keyword));
        }

        

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ExamDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<ExamDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
        };
    }

    public async Task<ExamDetailDto> GetByIdAsync(long id)
    {
        var exam = await _context.Exams
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<ExamDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (exam == null)
            throw new NotFoundException("Exam not found.");

        return exam;
    }

    public async Task<long> CreateAsync(CreateExamRequestDto request)
    {
        var entity = _mapper.Map<Exam>(request);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;

        _context.Exams.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateExamRequestDto request)
    {
        var entity = await _context.Exams.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) throw new NotFoundException("Exam not found.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Exams.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) throw new NotFoundException("Exam not found.");

        _context.Exams.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
