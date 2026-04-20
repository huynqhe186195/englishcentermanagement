using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EnglishCenter.Application.Features.Payments.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Payments.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Payment, PaymentDto>();

        CreateMap<Payment, PaymentDetailDto>()
            .ForMember(dest => dest.InvoiceNo, opt => opt.MapFrom(src => src.Invoice.InvoiceNo))
            .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.Invoice.StudentId))
            .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.Invoice.Student.StudentCode))
            .ForMember(dest => dest.StudentFullName, opt => opt.MapFrom(src => src.Invoice.Student.FullName));
    }
}
