using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(
        long userId,
        string userName,
        string fullName,
        IEnumerable<string> roles);
}
