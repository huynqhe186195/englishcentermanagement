using AutoMapper;
using EnglishCenter.Application.Features.AuditLogs.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.AuditLogs.Mappings;

public class AuditLogProfile : Profile
{
    public AuditLogProfile()
    {
        CreateMap<AuditLog, AuditLogDto>();
    }
}
