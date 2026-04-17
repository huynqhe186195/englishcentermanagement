using AutoMapper;
using EnglishCenter.Application.Features.Exams.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Exams.Mappings;

public class ExamProfile : Profile
{
    public ExamProfile()
    {
        CreateMap<Exam, ExamDto>()
            .ForMember(d => d.ClassName, o => o.MapFrom(s => s.Class.Name));

        CreateMap<Exam, ExamDetailDto>()
            .ForMember(d => d.ClassName, o => o.MapFrom(s => s.Class.Name))
            .ForMember(d => d.CreatedByUserName, o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FullName : null));

        CreateMap<CreateExamRequestDto, Exam>();
        CreateMap<UpdateExamRequestDto, Exam>();
    }
}
