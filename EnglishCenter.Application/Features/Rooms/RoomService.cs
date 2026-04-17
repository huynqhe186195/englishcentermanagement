using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Rooms.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Rooms;

public class RoomService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RoomService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<RoomDto>> GetAllAsync()
    {
        return await _context.Rooms
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ProjectTo<RoomDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<RoomDto>> GetPagedAsync(GetRoomsPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Rooms
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            query = query.Where(x => x.RoomCode.ToLower().Contains(keyword) || x.Name.ToLower().Contains(keyword));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<RoomDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<RoomDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
        };
    }

    public async Task<RoomDetailDto> GetByIdAsync(long id)
    {
        var room = await _context.Rooms
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<RoomDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (room == null) throw new NotFoundException("Room not found.");
        return room;
    }

    public async Task<long> CreateAsync(CreateRoomRequestDto request)
    {
        var code = request.RoomCode?.Trim() ?? string.Empty;

        var campus = await _context.Campuses.FirstOrDefaultAsync(c => c.Id == request.CampusId && !c.IsDeleted);
        if (campus == null) throw new NotFoundException("Campus not found.");

        var exists = await _context.Rooms.AnyAsync(x => x.RoomCode == code && !x.IsDeleted);
        if (exists) throw new BusinessException("RoomCode already exists.");

        var entity = _mapper.Map<Room>(request);
        entity.RoomCode = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Rooms.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateRoomRequestDto request)
    {
        var entity = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("Room not found.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("Room not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
