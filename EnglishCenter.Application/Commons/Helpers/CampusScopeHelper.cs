using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Commons.Helpers;

public class CampusScopeHelper
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IApplicationDbContext _context;

    public CampusScopeHelper(ICurrentUserContext currentUserContext, IApplicationDbContext context)
    {
        _currentUserContext = currentUserContext;
        _context = context;
    }

    public long GetManagedCampusId()
    {
        return _currentUserContext.IsSuperAdmin ? 0 : _currentUserContext.CampusId;
    }

    public void EnsureCampusAllowed(long? targetCampusId)
    {
        if (_currentUserContext.IsSuperAdmin)
        {
            return;
        }

        if (!targetCampusId.HasValue || targetCampusId.Value != _currentUserContext.CampusId)
        {
            throw new BusinessException("You can only manage data in your campus.");
        }
    }

    public async Task EnsureUserInScopeAsync(long userId)
    {
        if (_currentUserContext.IsSuperAdmin)
        {
            return;
        }

        var userCampusId = await _context.Users
            .Where(x => x.Id == userId && !x.IsDeleted)
            .Select(x => x.CampusId)
            .FirstOrDefaultAsync();

        if (!userCampusId.HasValue || userCampusId.Value != _currentUserContext.CampusId)
        {
            throw new BusinessException("You can only manage users in your campus.");
        }
    }
}
