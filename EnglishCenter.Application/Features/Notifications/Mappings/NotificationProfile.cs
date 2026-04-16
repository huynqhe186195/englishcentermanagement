using AutoMapper;
using EnglishCenter.Application.Features.Notifications.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Notifications.Mappings;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<Notification, NotificationDto>();
        CreateMap<Notification, NotificationDetailDto>()
            .ForMember(d => d.CreatedByUserName, o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FullName : null));
        CreateMap<CreateNotificationRequestDto, Notification>();
        CreateMap<UpdateNotificationRequestDto, Notification>();
    }
}
