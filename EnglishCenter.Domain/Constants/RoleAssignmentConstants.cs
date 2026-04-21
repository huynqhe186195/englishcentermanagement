namespace EnglishCenter.Domain.Constants;

public static class RoleAssignmentConstants
{
    public static readonly HashSet<string> CampusAdminAssignableRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        RoleConstants.Staff,
        RoleConstants.Teacher,
        RoleConstants.Parent,
        RoleConstants.Student
    };
}
