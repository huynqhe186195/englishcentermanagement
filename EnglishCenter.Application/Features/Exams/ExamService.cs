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

    private int GetDurationMinutes(int examType) => examType == 1 ? 30 : 90;

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

        // only allow update if exam has not started yet
        var start = DateTime.SpecifyKind(entity.ExamDate, DateTimeKind.Utc);
        if (DateTime.UtcNow >= start)
            throw new BusinessException("Cannot edit exam that has already started or completed.");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Exams.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) throw new NotFoundException("Exam not found.");

        var start = DateTime.SpecifyKind(entity.ExamDate, DateTimeKind.Utc);
        if (DateTime.UtcNow >= start)
            throw new BusinessException("Cannot delete exam that has already started or completed.");

        _context.Exams.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ExamDto>> GetByClassIdWithStatusAsync(long classId)
    {
        var exams = await _context.Exams
            .AsNoTracking()
            .Where(x => x.ClassId == classId)
            .OrderBy(x => x.ExamDate)
            .ProjectTo<ExamDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var ex in exams)
        {
            var dur = GetDurationMinutes(ex.ExamType);
            var start = DateTime.SpecifyKind(ex.ExamDate, DateTimeKind.Utc);
            var end = start.AddMinutes(dur);
            if (now < start) ex.Status = 0; // Upcoming
            else if (now >= start && now < end) ex.Status = 1; // Ongoing
            else ex.Status = 2; // Completed
        }

        return exams;
    }

    public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(long classId, DateTime from, DateTime to, int durationMinutes, int stepMinutes = 30)
    {
        if (from >= to) throw new BusinessException("Invalid time range.");

        // get active student ids in class
        var studentIds = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.ClassId == classId && !e.IsDeleted && e.Status == 1)
            .Select(e => e.StudentId)
            .Distinct()
            .ToListAsync();

        // preload possible conflicting exams and sessions in the extended window
        var windowStart = from.AddMinutes(-90);
        var windowEnd = to.AddMinutes(90);

        var exams = await (from ex in _context.Exams
                           join en in _context.Enrollments on ex.ClassId equals en.ClassId
                           where studentIds.Contains(en.StudentId)
                                 && ex.ExamDate >= windowStart && ex.ExamDate <= windowEnd
                           select new { ex.Id, ex.ClassId, ex.ExamType, ex.ExamDate, StudentId = en.StudentId })
            .AsNoTracking()
            .ToListAsync();

        // filter sessions by session date range first (compare DateOnly) then evaluate precise DateTime overlaps in memory
        var windowStartDate = DateOnly.FromDateTime(windowStart);
        var windowEndDate = DateOnly.FromDateTime(windowEnd);

        var sessions = await (from cs in _context.ClassSessions
                              join en in _context.Enrollments on cs.ClassId equals en.ClassId
                              where studentIds.Contains(en.StudentId)
                                    && cs.SessionDate >= windowStartDate
                                    && cs.SessionDate <= windowEndDate
                              select new { cs.Id, cs.ClassId, cs.SessionDate, cs.StartTime, cs.EndTime, StudentId = en.StudentId })
            .AsNoTracking()
            .ToListAsync();

        var result = new List<AvailableSlotDto>();

        // preload student basic info for mapping
        var studentInfos = await _context.Students
            .AsNoTracking()
            .Where(s => studentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => new { s.StudentCode, s.FullName });

        for (var slotStart = from; slotStart.AddMinutes(durationMinutes) <= to; slotStart = slotStart.AddMinutes(stepMinutes))
        {
            var slotEnd = slotStart.AddMinutes(durationMinutes);

            var conflictingStudents = new HashSet<long>();

            // check exams
            foreach (var e in exams)
            {
                var eStart = DateTime.SpecifyKind(e.ExamDate, DateTimeKind.Utc);
                var eEnd = eStart.AddMinutes(GetDurationMinutes(e.ExamType));
                if (slotStart < eEnd && slotEnd > eStart)
                {
                    conflictingStudents.Add(e.StudentId);
                }
            }

            // check sessions
            foreach (var s in sessions)
            {
                var sStart = DateTime.SpecifyKind(s.SessionDate.ToDateTime(s.StartTime), DateTimeKind.Utc);
                var sEnd = DateTime.SpecifyKind(s.SessionDate.ToDateTime(s.EndTime), DateTimeKind.Utc);
                if (slotStart < sEnd && slotEnd > sStart)
                {
                    conflictingStudents.Add(s.StudentId);
                }
            }

            var slot = new AvailableSlotDto
            {
                Start = slotStart,
                End = slotEnd,
                ConflictingStudentCount = conflictingStudents.Count
            };

            if (conflictingStudents.Any())
            {
                foreach (var sid in conflictingStudents)
                {
                    if (studentInfos.TryGetValue(sid, out var info))
                    {
                        slot.ConflictingStudents.Add(new ConflictingStudentDto
                        {
                            StudentId = sid,
                            StudentCode = info.StudentCode,
                            StudentName = info.FullName
                        });
                    }
                    else
                    {
                        slot.ConflictingStudents.Add(new ConflictingStudentDto
                        {
                            StudentId = sid,
                            StudentCode = string.Empty,
                            StudentName = string.Empty
                        });
                    }
                }
            }

            result.Add(slot);
        }

        return result;
    }

    public async Task<long> CreateWithValidationAsync(CreateExamRequestDto request)
    {
        var start = DateTime.SpecifyKind(request.ExamDate, DateTimeKind.Utc);
        var dur = GetDurationMinutes(request.ExamType);
        var end = start.AddMinutes(dur);

        // load student ids in target class
        var studentIds = await _context.Enrollments
            .Where(e => e.ClassId == request.ClassId && !e.IsDeleted && e.Status == 1)
            .Select(e => e.StudentId)
            .Distinct()
            .ToListAsync();

        // find potential conflicting exams for these students
        var windowStart = start.AddMinutes(-90);
        var windowEnd = end.AddMinutes(90);

        var exams = await (from ex in _context.Exams
                           join en in _context.Enrollments on ex.ClassId equals en.ClassId
                           where studentIds.Contains(en.StudentId)
                                 && ex.ExamDate >= windowStart && ex.ExamDate <= windowEnd
                           select new { ex.Id, ex.ExamType, ex.ExamDate, StudentId = en.StudentId })
            .AsNoTracking()
            .ToListAsync();

        var windowStartDate2 = DateOnly.FromDateTime(windowStart);
        var windowEndDate2 = DateOnly.FromDateTime(windowEnd);

        var sessions = await (from cs in _context.ClassSessions
                              join en in _context.Enrollments on cs.ClassId equals en.ClassId
                              where studentIds.Contains(en.StudentId)
                                    && cs.SessionDate >= windowStartDate2
                                    && cs.SessionDate <= windowEndDate2
                              select new { cs.Id, cs.SessionDate, cs.StartTime, cs.EndTime, StudentId = en.StudentId })
            .AsNoTracking()
            .ToListAsync();

        var conflictingStudentIds = new HashSet<long>();

        foreach (var e in exams)
        {
            var eStart = DateTime.SpecifyKind(e.ExamDate, DateTimeKind.Utc);
            var eEnd = eStart.AddMinutes(GetDurationMinutes(e.ExamType));
            if (start < eEnd && end > eStart)
            {
                conflictingStudentIds.Add(e.StudentId);
            }
        }

        foreach (var s in sessions)
        {
            var sStart = DateTime.SpecifyKind(s.SessionDate.ToDateTime(s.StartTime), DateTimeKind.Utc);
            var sEnd = DateTime.SpecifyKind(s.SessionDate.ToDateTime(s.EndTime), DateTimeKind.Utc);
            if (start < sEnd && end > sStart)
            {
                conflictingStudentIds.Add(s.StudentId);
            }
        }

        if (conflictingStudentIds.Any())
        {
            // load student codes for message
            var students = await _context.Students.Where(s => conflictingStudentIds.Contains(s.Id)).ToListAsync();
            var codes = students.Select(s => s.StudentCode).ToArray();
            throw new BusinessException($"Conflicting schedule for students: {string.Join(',', codes)}");
        }

        // create exam
        var entity = _mapper.Map<Exam>(request);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;

        _context.Exams.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }
}
