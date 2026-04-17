using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Common.Models;

namespace EnglishCenter.Application.Features.AuditLogs.Dtos;

public class GetAuditLogsPagingRequestDto : SortablePaginationRequest
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public long? UserId { get; set; }
}
