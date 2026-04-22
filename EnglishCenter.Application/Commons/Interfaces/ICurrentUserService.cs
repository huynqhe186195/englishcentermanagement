using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Common.Interfaces;

public interface ICurrentUserService
{
    long? UserId { get; }
    long? CampusId { get; }
    string? UserName { get; }
    string? IpAddress { get; }
    List<string> Roles { get; }
    List<string> Permissions { get; }
    bool IsInRole(string role);
}
