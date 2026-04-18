using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.ClassSessions;

public class SessionConflictService
{
    private readonly IApplicationDbContext _context;

    public SessionConflictService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ValidateTeacherConflictAsync(
        long? teacherId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        long? excludeSessionId = null)
    {
        if (!teacherId.HasValue)
            return;

        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == teacherId.Value && !x.IsDeleted);

        if (teacher == null)
            throw new NotFoundException("Teacher not found.");

        if (teacher.Status != 1)
            throw new BusinessException("Teacher is inactive.");

        var hasConflict = await _context.ClassSessions
            .AsNoTracking()
            .AnyAsync(x =>
                x.TeacherId == teacherId.Value &&
                x.SessionDate == sessionDate &&
                x.Status != ClassSessionStatusConstants.Cancelled &&
                (!excludeSessionId.HasValue || x.Id != excludeSessionId.Value) &&
                startTime < x.EndTime &&
                endTime > x.StartTime);

        if (hasConflict)
            throw new BusinessException("Teacher has a schedule conflict.");
    }

    public async Task ValidateRoomConflictAsync(
        long? roomId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        long? excludeSessionId = null)
    {
        if (!roomId.HasValue)
            return;

        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == roomId.Value && !x.IsDeleted);

        if (room == null)
            throw new NotFoundException("Room not found.");

        if (room.Status != 1)
            throw new BusinessException("Room is inactive.");

        var hasConflict = await _context.ClassSessions
            .AsNoTracking()
            .AnyAsync(x =>
                x.RoomId == roomId.Value &&
                x.SessionDate == sessionDate &&
                x.Status != ClassSessionStatusConstants.Cancelled &&
                (!excludeSessionId.HasValue || x.Id != excludeSessionId.Value) &&
                startTime < x.EndTime &&
                endTime > x.StartTime);

        if (hasConflict)
            throw new BusinessException("Room has a schedule conflict.");
    }

    public async Task ValidateSessionConflictsAsync(
        long? teacherId,
        long? roomId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        long? excludeSessionId = null)
    {
        if (startTime >= endTime)
            throw new BusinessException("StartTime must be less than EndTime.");

        await ValidateTeacherConflictAsync(
            teacherId,
            sessionDate,
            startTime,
            endTime,
            excludeSessionId);

        await ValidateRoomConflictAsync(
            roomId,
            sessionDate,
            startTime,
            endTime,
            excludeSessionId);
    }
}
