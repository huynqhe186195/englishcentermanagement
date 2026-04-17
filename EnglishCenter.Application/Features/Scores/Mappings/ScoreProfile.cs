using AutoMapper;
using EnglishCenter.Application.Features.Scores.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Scores.Mappings;

public class ScoreProfile : Profile
{
    public ScoreProfile()
    {
        CreateMap<Score, ScoreDto>()
            .ForMember(d => d.ExamTitle, o => o.MapFrom(s => s.Exam.Title))
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student.FullName));

        CreateMap<Score, ScoreDetailDto>()
            .ForMember(d => d.ExamTitle, o => o.MapFrom(s => s.Exam.Title))
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student.FullName));

        CreateMap<CreateScoreRequestDto, Score>();
        CreateMap<UpdateScoreRequestDto, Score>();
    }
}
