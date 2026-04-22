using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Domain.Constants;

namespace EnglishCenter.Application.Commons.Helpers;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly ICurrentUserService _currentUserService;

    public CurrentUserContext(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public long UserId => _currentUserService.UserId ?? throw new BusinessException("User is not authenticated.");

    public long CampusId => _currentUserService.CampusId ?? throw new BusinessException("Current user is not bound to a campus.");

    public bool IsSuperAdmin => _currentUserService.IsInRole(RoleConstants.SuperAdmin);

    public bool IsCenterAdmin =>
        _currentUserService.IsInRole(RoleConstants.CenterAdmin)
        || _currentUserService.IsInRole(RoleConstants.Manager)
        || _currentUserService.IsInRole(RoleConstants.Admin);

    public bool IsInRole(string roleCode) => _currentUserService.IsInRole(roleCode);
}
