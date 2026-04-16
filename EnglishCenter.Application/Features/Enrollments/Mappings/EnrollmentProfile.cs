using AutoMapper;
using EnglishCenter.Application.Features.Enrollments.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Enrollments.Mappings;

public class EnrollmentProfile : Profile
{
    public EnrollmentProfile()
    {
        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student.FullName))
            .ForMember(d => d.ClassName, o => o.MapFrom(s => s.Class.Name));

        CreateMap<Enrollment, EnrollmentDetailDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student.FullName))
            .ForMember(d => d.ClassName, o => o.MapFrom(s => s.Class.Name));

        CreateMap<CreateEnrollmentRequestDto, Enrollment>();
        CreateMap<UpdateEnrollmentRequestDto, Enrollment>();
    }
}
