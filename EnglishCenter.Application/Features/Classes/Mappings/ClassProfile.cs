using AutoMapper;
using EnglishCenter.Application.Features.Classes.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Classes.Mappings;

public class ClassProfile : Profile
{
    public ClassProfile()
    {
        CreateMap<Class, ClassDto>();
        CreateMap<Class, ClassDetailDto>();
        CreateMap<CreateClassRequestDto, Class>();
        CreateMap<UpdateClassRequestDto, Class>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ClassCode, opt => opt.Ignore());
    }
}
