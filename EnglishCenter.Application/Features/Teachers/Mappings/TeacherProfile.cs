using AutoMapper;
using EnglishCenter.Application.Features.Teachers.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Teachers.Mappings;

public class TeacherProfile : Profile
{
    public TeacherProfile()
    {
        CreateMap<Teacher, TeacherDto>();
        CreateMap<Teacher, TeacherDetailDto>();
        CreateMap<CreateTeacherRequestDto, Teacher>();
        CreateMap<UpdateTeacherRequestDto, Teacher>();
        CreateMap<TeacherDto, Teacher>();
    }
}
