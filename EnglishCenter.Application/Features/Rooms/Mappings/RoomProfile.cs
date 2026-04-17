using AutoMapper;
using EnglishCenter.Application.Features.Rooms.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Rooms.Mappings;

public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<Room, RoomDto>();
        CreateMap<Room, RoomDetailDto>();
        CreateMap<CreateRoomRequestDto, Room>();
        CreateMap<UpdateRoomRequestDto, Room>();
        CreateMap<RoomDto, Room>();
    }
}
