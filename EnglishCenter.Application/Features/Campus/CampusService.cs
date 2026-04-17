using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Campus.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Campus;

public class CampusService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CampusService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CampusDto>> GetAllAsync()
    {
        return await _context.Campuses
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ProjectTo<CampusDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<CampusDto>> GetPagedAsync(GetCampusesPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Campuses
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            query = query.Where(x => x.CampusCode.ToLower().Contains(keyword) || x.Name.ToLower().Contains(keyword));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<CampusDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<CampusDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
        };
    }

    public async Task<CampusDetailDto> GetByIdAsync(long id)
    {
        var campus = await _context.Campuses
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<CampusDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (campus == null)
            throw new NotFoundException("Campus not found.");

        return campus;
    }

    public async Task<long> CreateAsync(CreateCampusRequestDto request)
    {
        var code = request.CampusCode.Trim();
        var exists = await _context.Campuses.AnyAsync(x => x.CampusCode == code && !x.IsDeleted);
        if (exists) throw new BusinessException("CampusCode already exists.");

        var entity = _mapper.Map<EnglishCenter.Domain.Models.Campus>(request);
        entity.CampusCode = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Campuses.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateCampusRequestDto request)
    {
        var entity = await _context.Campuses.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("Campus not found.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Campuses.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("Campus not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
