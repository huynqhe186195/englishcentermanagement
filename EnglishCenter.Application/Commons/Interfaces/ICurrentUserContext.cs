namespace EnglishCenter.Application.Common.Interfaces;

public interface ICurrentUserContext
{
    long UserId { get; }
    long CampusId { get; }
    bool IsSuperAdmin { get; }
    bool IsCenterAdmin { get; }
    bool IsInRole(string roleCode);
}
