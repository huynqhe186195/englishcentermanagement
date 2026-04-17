using AutoMapper;
using EnglishCenter.Application.Features.ClassSchedules.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.ClassSchedules.Mappings;

public class ClassScheduleProfile : Profile
{
    public ClassScheduleProfile()
    {
        CreateMap<ClassSchedule, ClassScheduleDto>();

        CreateMap<CreateClassScheduleRequestDto, ClassSchedule>();

        CreateMap<UpdateClassScheduleRequestDto, ClassSchedule>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ClassId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}