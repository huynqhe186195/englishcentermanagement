using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Features.Students.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Students;

public class StudentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public StudentService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<StudentDto>> GetAllAsync()
    {
        return await _context.Students
               .AsNoTracking()
               .Where(x => !x.IsDeleted)
               .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
               .ToListAsync();
    }

    public async Task<StudentDetailDto?> GetByIdAsync(long id)
    {
        return await _context.Students
               .AsNoTracking()
               .Where(x => x.Id == id && !x.IsDeleted)
               .ProjectTo<StudentDetailDto>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync();
    }

    public async Task<long> CreateAsync(CreateStudentRequestDto request)
    {
        var entity = _mapper.Map<Student>(request);

        entity.CreatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        _context.Students.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateStudentRequestDto request)
    {
        var entity = await _context.Students.FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Students.FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}