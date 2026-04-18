using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Classes.Dtos;

public class ClassRosterItemDto
{
    public long EnrollmentId { get; set; }
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int EnrollmentStatus { get; set; }   
    public DateOnly EnrollDate { get; set; }
}
