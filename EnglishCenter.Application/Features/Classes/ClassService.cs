using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Classes.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Classes;

public class ClassService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ClassService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ClassDto>> GetAllAsync()
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ProjectTo<ClassDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<ClassDto>> GetPagedAsync(GetClassesPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Classes
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();

            query = query.Where(x =>
                x.ClassCode.ToLower().Contains(keyword) ||
                x.Name.ToLower().Contains(keyword));
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
            .ProjectTo<ClassDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<ClassDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
        };
    }

    public async Task<ClassDetailDto> GetByIdAsync(long id)
    {
        var cls = await _context.Classes
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<ClassDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (cls == null)
        {
            throw new NotFoundException("Class not found.");
        }

        return cls;
    }

    public async Task<long> CreateAsync(CreateClassRequestDto request)
    {
        var classCode = request.ClassCode.Trim();
        var name = request.Name.Trim();

        var exists = await _context.Classes
            .AnyAsync(x => x.ClassCode == classCode && !x.IsDeleted);

        if (exists)
        {
            throw new BusinessException("ClassCode already exists.");
        }

        var entity = _mapper.Map<Class>(request);

        entity.ClassCode = classCode;
        entity.Name = name;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Classes.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateClassRequestDto request)
    {
        var entity = await _context.Classes
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Class not found.");
        }

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Classes
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Class not found.");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
