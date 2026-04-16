using AutoMapper;
using EnglishCenter.Application.Features.Roles.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Roles.Mappings;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Role, RoleDto>();
        CreateMap<Role, RoleDetailDto>();
        CreateMap<CreateRoleRequestDto, Role>();
        CreateMap<UpdateRoleRequestDto, Role>();
    }
}
