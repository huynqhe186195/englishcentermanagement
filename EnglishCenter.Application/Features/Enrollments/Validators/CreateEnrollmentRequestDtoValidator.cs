using EnglishCenter.Application.Features.Enrollments.Dtos;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

// Validator uses the application DB context to perform existence and business-rule checks.

namespace EnglishCenter.Application.Features.Enrollments.Validators;

public class CreateEnrollmentRequestDtoValidator : AbstractValidator<CreateEnrollmentRequestDto>
{
    private readonly IApplicationDbContext _context;

    public CreateEnrollmentRequestDtoValidator(IApplicationDbContext context)
    {
        _context = context;
        RuleFor(x => x.ClassId)
            .GreaterThan(0)
            .WithMessage("ClassId must be greater than 0.");

        RuleFor(x => x.EnrollDate)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("EnrollDate cannot be in the future.");

        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .WithMessage("Note must not exceed 1000 characters.");

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 1)
            .WithMessage("Status must be 0 or 1.");
        // Student must exist and not soft-deleted
        RuleFor(x => x.StudentId)
            .GreaterThan(0)
            .MustAsync(async (studentId, cancellation) =>
            {
                var student = await _context.Students
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted, cancellation);
                return student != null;
            })
            .WithMessage("Student does not exist or has been deleted.");

        // Class must exist and not soft-deleted
        RuleFor(x => x.ClassId)
            .GreaterThan(0)
            .MustAsync(async (classId, cancellation) =>
            {
                var cls = await _context.Classes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == classId && !c.IsDeleted, cancellation);
                return cls != null;
            })
            .WithMessage("Class does not exist or has been deleted.");

        // Check class capacity and registration status
        RuleFor(x => x)
            .MustAsync(async (dto, cancellation) =>
            {
                // ensure class exists
                var cls = await _context.Classes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == dto.ClassId && !c.IsDeleted, cancellation);
                if (cls == null) return false;

                // check registration open - assume Status == 1 means open for registration
                if (cls.Status != 1) return false;

                // check capacity
                var enrolledCount = await _context.Enrollments
                    .AsNoTracking()
                    .CountAsync(e => e.ClassId == dto.ClassId && !e.IsDeleted, cancellation);

                if (enrolledCount >= cls.MaxStudents) return false;

                // check student not already enrolled
                var already = await _context.Enrollments
                    .AsNoTracking()
                    .AnyAsync(e => e.ClassId == dto.ClassId && e.StudentId == dto.StudentId && !e.IsDeleted, cancellation);
                if (already) return false;

                return true;
            })
            .WithMessage("Class is not available for enrollment (closed, full, or student already enrolled)." );


    }
}
