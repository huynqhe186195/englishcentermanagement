using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Invoices.Dtos;

public class CreateInvoiceRequestDto
{
    public long StudentId { get; set; }
    public long CourseId { get; set; }
    public DateTime DueDate { get; set; }
    public string? Note { get; set; }
}
