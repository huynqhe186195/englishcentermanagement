using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Course> Courses { get; }
    DbSet<Student> Students { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}