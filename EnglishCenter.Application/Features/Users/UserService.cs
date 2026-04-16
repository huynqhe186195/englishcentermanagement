using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Users.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Users;

public class UserService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UserService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(GetUsersPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Users.AsNoTracking().Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            query = query.Where(x => x.UserName.ToLower().Contains(kw) || x.FullName.ToLower().Contains(kw) || (x.Email != null && x.Email.ToLower().Contains(kw)));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<UserDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<UserDetailDto> GetByIdAsync(long id)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<UserDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (entity == null) throw new NotFoundException("User not found.");
        return entity;
    }

    public async Task<long> CreateAsync(CreateUserRequestDto request)
    {
        var userName = request.UserName.Trim();
        var exists = await _context.Users.AnyAsync(x => x.UserName == userName && !x.IsDeleted);
        if (exists) throw new BusinessException("UserName already exists.");

        var entity = _mapper.Map<User>(request);
        entity.UserName = userName;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateUserRequestDto request)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("User not found.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("User not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
