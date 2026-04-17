using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Features.ClassSchedules.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.ClassSchedules;

public class ClassScheduleService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ClassScheduleService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ClassScheduleDto>> GetAllAsync(long? classId)
    {
        var query = _context.ClassSchedules
            .AsNoTracking()
            .AsQueryable();

        if (classId.HasValue)
        {
            query = query.Where(x => x.ClassId == classId.Value);
        }

        return await query
            .OrderBy(x => x.ClassId)
            .ThenBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ProjectTo<ClassScheduleDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ClassScheduleDto> GetByIdAsync(long id)
    {
        var schedule = await _context.ClassSchedules
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<ClassScheduleDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (schedule == null)
        {
            throw new NotFoundException("Class schedule not found.");
        }

        return schedule;
    }

    public async Task<long> CreateAsync(CreateClassScheduleRequestDto request)
    {
        var @class = await _context.Classes
            .FirstOrDefaultAsync(x => x.Id == request.ClassId && !x.IsDeleted);

        if (@class == null)
        {
            throw new NotFoundException("Class not found.");
        }

        if (request.RoomId.HasValue)
        {
            var roomExists = await _context.Rooms
                .AnyAsync(x => x.Id == request.RoomId.Value && !x.IsDeleted);

            if (!roomExists)
            {
                throw new NotFoundException("Room not found.");
            }
        }

        var duplicated = await _context.ClassSchedules.AnyAsync(x =>
            x.ClassId == request.ClassId &&
            x.DayOfWeek == request.DayOfWeek &&
            x.StartTime == request.StartTime &&
            x.EndTime == request.EndTime);

        if (duplicated)
        {
            throw new BusinessException("This class schedule already exists.");
        }

        var entity = _mapper.Map<ClassSchedule>(request);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;

        _context.ClassSchedules.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateAsync(long id, UpdateClassScheduleRequestDto request)
    {
        var entity = await _context.ClassSchedules
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new NotFoundException("Class schedule not found.");
        }

        if (request.RoomId.HasValue)
        {
            var roomExists = await _context.Rooms
                .AnyAsync(x => x.Id == request.RoomId.Value && !x.IsDeleted);

            if (!roomExists)
            {
                throw new NotFoundException("Room not found.");
            }
        }

        var duplicated = await _context.ClassSchedules.AnyAsync(x =>
            x.Id != id &&
            x.ClassId == entity.ClassId &&
            x.DayOfWeek == request.DayOfWeek &&
            x.StartTime == request.StartTime &&
            x.EndTime == request.EndTime);

        if (duplicated)
        {
            throw new BusinessException("This class schedule already exists.");
        }

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.ClassSchedules
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new NotFoundException("Class schedule not found.");
        }

        _context.ClassSchedules.Remove(entity);
        await _context.SaveChangesAsync();
    }
}