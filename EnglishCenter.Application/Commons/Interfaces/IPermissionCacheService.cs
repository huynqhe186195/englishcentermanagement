using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Common.Interfaces;

public interface IPermissionCacheService
{
    Task<List<string>> GetPermissionsAsync(long userId);
    void RemovePermissions(long userId);
}
