using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Scores.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Scores;

public class ScoreService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ScoreService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ScoreDto>> GetAllAsync()
    {
        return await _context.Scores
            .AsNoTracking()
            .ProjectTo<ScoreDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<ScoreDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var query = _context.Scores.AsNoTracking().AsQueryable();
        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ScoreDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<ScoreDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<ScoreDetailDto> GetByIdAsync(long id)
    {
        var entity = await _context.Scores
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<ScoreDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (entity == null) throw new NotFoundException("Score not found.");
        return entity;
    }

    public async Task<long> CreateAsync(CreateScoreRequestDto request)
    {
        var entity = _mapper.Map<Score>(request);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;

        _context.Scores.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateScoreRequestDto request)
    {
        var entity = await _context.Scores.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) throw new NotFoundException("Score not found.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Scores.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) throw new NotFoundException("Score not found.");

        _context.Scores.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
