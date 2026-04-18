using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EnglishCenter.Application.Features.Timetables.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Timetables.Mappings;

public class TimetableProfile : Profile
{
    public TimetableProfile()
    {
        CreateMap<ClassSession, TimetableItemDto>()
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.Id));
    }
}
