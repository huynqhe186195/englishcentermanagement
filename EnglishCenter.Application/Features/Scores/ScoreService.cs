using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Scores.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

    // Import scores (create or update) for a specific exam
    public async Task ImportScoresAsync(long examId, List<ImportScoreItemDto> items)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == examId);
        if (exam == null) throw new NotFoundException("Exam not found.");

        foreach (var it in items)
        {
            long? studentId = it.StudentId;

            if (!studentId.HasValue && !string.IsNullOrWhiteSpace(it.StudentCode))
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == it.StudentCode && !s.IsDeleted);
                if (student != null) studentId = student.Id;
            }

            if (!studentId.HasValue)
            {
                // skip unknown student rows
                continue;
            }

            var score = await _context.Scores.FirstOrDefaultAsync(x => x.ExamId == examId && x.StudentId == studentId.Value);
            if (score == null)
            {
                score = new Score
                {
                    ExamId = examId,
                    StudentId = studentId.Value,
                    ScoreValue = it.ScoreValue,
                    Remark = it.Remark,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Scores.Add(score);
            }
            else
            {
                score.ScoreValue = it.ScoreValue;
                score.Remark = it.Remark;
                score.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    // Get pass/fail list for students related to an exam's class. Criteria:
    // - attendance percentage > 80%
    // - average score across all exams in the class > 4
    public async Task<List<PassFailDto>> GetPassFailListAsync(long examId)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == examId);
        if (exam == null) throw new NotFoundException("Exam not found.");

        var classId = exam.ClassId;

        // total sessions for the class (exclude cancelled)
        var totalSessions = await _context.ClassSessions.CountAsync(cs => cs.ClassId == classId && cs.Status != 3);

        // students actively enrolled in class
        var students = await (from e in _context.Enrollments
                              join s in _context.Students on e.StudentId equals s.Id
                              where e.ClassId == classId && !e.IsDeleted && e.Status == 1 && !s.IsDeleted
                              select new { s.Id, s.StudentCode, s.FullName }).ToListAsync();

        var result = new List<PassFailDto>();

        foreach (var s in students)
        {
            // average score across exams in this class
            // EF Core cannot translate DefaultIfEmpty().AverageAsync() over a join in some cases,
            // so materialize the score values first then compute average in memory.
            var scoreValues = await (from sc in _context.Scores
                                     join ex in _context.Exams on sc.ExamId equals ex.Id
                                     where sc.StudentId == s.Id && ex.ClassId == classId
                                     select sc.ScoreValue).ToListAsync();

            decimal avg = 0m;
            if (scoreValues != null && scoreValues.Count > 0)
            {
                avg = scoreValues.Average();
            }

            // attendance present count for this student and class
            var presentCount = await (from ar in _context.AttendanceRecords
                                      join cs in _context.ClassSessions on ar.SessionId equals cs.Id
                                      where ar.StudentId == s.Id && cs.ClassId == classId && ar.Status == 1
                                      select ar).CountAsync();

            decimal attendancePercent = 0;
            if (totalSessions > 0)
            {
                attendancePercent = Math.Round((decimal)presentCount / (decimal)totalSessions * 100m, 2);
            }

            var isPassed = attendancePercent > 80m && avg > 4m;

            result.Add(new PassFailDto
            {
                StudentId = s.Id,
                StudentCode = s.StudentCode,
                StudentName = s.FullName,
                AverageScore = Math.Round(avg, 2),
                PresentCount = presentCount,
                TotalSessions = totalSessions,
                AttendancePercent = attendancePercent,
                IsPassed = isPassed
            });
        }

        return result;
    }

    public async Task<List<TemplateStudentDto>> GetTemplateStudentsAsync(long examId)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == examId);
        if (exam == null) throw new NotFoundException("Exam not found.");

        var classId = exam.ClassId;

        var students = await (from e in _context.Enrollments
                              join s in _context.Students on e.StudentId equals s.Id
                              where e.ClassId == classId && !e.IsDeleted && e.Status == 1 && !s.IsDeleted
                              orderby s.FullName
                              select new TemplateStudentDto
                              {
                                  StudentId = s.Id,
                                  StudentCode = s.StudentCode,
                                  StudentName = s.FullName
                              }).ToListAsync();

        return students;
    }
}
