using AutoMapper;
using EnglishCenter.Application.Features.Courses.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Courses.Mappings;

public class CourseProfile : Profile
{
    public CourseProfile()
    {
        CreateMap<Course, CourseDto>();
        CreateMap<Course, CourseDetailDto>();
        CreateMap<CreateCourseRequestDto, Course>();

        CreateMap<UpdateCourseRequestDto, Course>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CourseCode, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }
}