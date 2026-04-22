using AutoMapper;
using EnglishCenter.Application.Features.Users.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Users.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, UserDetailDto>();
        CreateMap<CreateUserRequestDto, User>();
        CreateMap<UpdateUserRequestDto, User>();
    }
}