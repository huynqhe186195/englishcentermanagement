using AutoMapper;
using EnglishCenter.Application.Features.Attendance.Dtos;
using EnglishCenter.Domain.Models;

namespace EnglishCenter.Application.Features.Attendance.Mappings;

public class AttendanceProfile : Profile
{
    public AttendanceProfile()
    {
        CreateMap<AttendanceRecord, AttendanceRecordDto>();
    }
}