using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Students.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Students.Validators;

public class GetStudentAttendanceReportRequestDtoValidator : AbstractValidator<GetStudentAttendanceReportRequestDto>
{
    public GetStudentAttendanceReportRequestDtoValidator()
    {
        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("ClassId must be greater than 0.");
    }
}
