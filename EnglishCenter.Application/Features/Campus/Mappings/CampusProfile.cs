using AutoMapper;
using EnglishCenter.Application.Features.Campus.Dtos;
using EnglishCenter.Domain.Models;
namespace EnglishCenter.Application.Features.Campus.Mappings;

public class CampusProfile : Profile
{
    public CampusProfile()
    {
        CreateMap<EnglishCenter.Domain.Models.Campus, CampusDto>();
        CreateMap<CampusDto, EnglishCenter.Domain.Models.Campus>();

        CreateMap<EnglishCenter.Domain.Models.Campus, CampusDetailDto>();
        CreateMap<CreateCampusRequestDto, EnglishCenter.Domain.Models.Campus>();
        CreateMap<UpdateCampusRequestDto, EnglishCenter.Domain.Models.Campus>();
    }
}
