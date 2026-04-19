using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EnglishCenter.Application.Features.Invoices.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Invoices.Mappings;

public class InvoiceProfile : Profile
{
    public InvoiceProfile()
    {
        CreateMap<Invoice, InvoiceDto>();

        CreateMap<Invoice, InvoiceDetailDto>()
            .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.Student.StudentCode))
            .ForMember(dest => dest.StudentFullName, opt => opt.MapFrom(src => src.Student.FullName))
            .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassCode : null))
            .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class != null ? src.Class.Name : null));
    }
}