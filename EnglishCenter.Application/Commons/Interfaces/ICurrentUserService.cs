using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Common.Interfaces;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? UserName { get; }
    List<string> Roles { get; }
}
