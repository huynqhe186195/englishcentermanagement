using AutoMapper;
using EnglishCenter.Application.Features.Assignments.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Assignments.Mappings;

public class AssignmentProfile : Profile
{
    public AssignmentProfile()
    {
        CreateMap<Assignment, AssignmentDto>()
            .ForMember(d => d.ClassName, o => o.MapFrom(s => s.Class.Name));

        CreateMap<Assignment, AssignmentDetailDto>()
            .ForMember(d => d.ClassName, o => o.MapFrom(s => s.Class.Name))
            .ForMember(d => d.CreatedByUserName, o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FullName : null));

        CreateMap<CreateAssignmentRequestDto, Assignment>();
        CreateMap<UpdateAssignmentRequestDto, Assignment>();
    }
}
