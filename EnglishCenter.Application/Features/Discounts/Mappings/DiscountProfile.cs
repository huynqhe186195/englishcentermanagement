using AutoMapper;
using EnglishCenter.Application.Features.Discounts.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Discounts.Mappings;

public class DiscountProfile : Profile
{
    public DiscountProfile()
    {
        CreateMap<Discount, DiscountDto>();
        CreateMap<Discount, DiscountDetailDto>();
        CreateMap<CreateDiscountRequestDto, Discount>();
        CreateMap<UpdateDiscountRequestDto, Discount>();
    }
}
