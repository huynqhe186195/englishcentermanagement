using AutoMapper;
using EnglishCenter.Application.Features.Students.Dtos;
using EnglishCenter.Infrastructure.Persistence.Models;

namespace EnglishCenter.Application.Features.Students.Mappings;

public class StudentProfile : Profile
{
    public StudentProfile()
    {
        CreateMap<Student, StudentDto>();
        CreateMap<Student, StudentDetailDto>();
        CreateMap<CreateStudentRequestDto, Student>();
        CreateMap<UpdateStudentRequestDto, Student>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StudentCode, opt => opt.Ignore());
    }
}