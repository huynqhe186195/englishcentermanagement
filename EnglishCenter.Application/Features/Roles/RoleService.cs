using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Roles.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Roles;

public class RoleService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RoleService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<RoleDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var query = _context.Roles.AsNoTracking().Where(x => !x.IsDeleted).AsQueryable();
        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<RoleDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<RoleDetailDto> GetByIdAsync(long id)
    {
        var entity = await _context.Roles
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<RoleDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (entity == null) throw new NotFoundException("Role not found.");
        return entity;
    }

    public async Task<long> CreateAsync(CreateRoleRequestDto request)
    {
        var code = request.Code.Trim();
        var exists = await _context.Roles.AnyAsync(x => x.Code == code && !x.IsDeleted);
        if (exists) throw new BusinessException("Role code already exists.");

        var entity = _mapper.Map<Role>(request);
        entity.Code = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Roles.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateRoleRequestDto request)
    {
        var entity = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("Role not found.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("Role not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
