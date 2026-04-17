using AutoMapper;
using EnglishCenter.Application.Features.ClassSessions.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.ClassSessions.Mappings;

public class ClassSessionProfile : Profile
{
    public ClassSessionProfile()
    {
        CreateMap<ClassSession, ClassSessionDto>();
        CreateMap<ClassSession, ClassSessionDetailDto>();
        CreateMap<CreateClassSessionRequestDto, ClassSession>();

        CreateMap<UpdateClassSessionRequestDto, ClassSession>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ClassId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}