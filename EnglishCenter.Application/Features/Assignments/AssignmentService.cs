using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Assignments.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Assignments;

public class AssignmentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AssignmentService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AssignmentDto>> GetAllAsync()
    {
        return await _context.Assignments
            .AsNoTracking()
            .ProjectTo<AssignmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<AssignmentDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var query = _context.Assignments.AsNoTracking().AsQueryable();
        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<AssignmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<AssignmentDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<AssignmentDetailDto> GetByIdAsync(long id)
    {
        var entity = await _context.Assignments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<AssignmentDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (entity == null) throw new NotFoundException("Assignment not found.");
        return entity;
    }

    public async Task<long> CreateAsync(CreateAssignmentRequestDto request)
    {
        var entity = _mapper.Map<Assignment>(request);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;

        _context.Assignments.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateAssignmentRequestDto request)
    {
        var entity = await _context.Assignments.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) throw new NotFoundException("Assignment not found.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Assignments.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) throw new NotFoundException("Assignment not found.");

        _context.Assignments.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
