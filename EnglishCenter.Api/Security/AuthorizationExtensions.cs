using EnglishCenter.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace EnglishCenter.Api.Security;

public static class AuthorizationExtensions
{
    public static void AddPermissionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PermissionConstants.Students.View,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Students.View)));

        options.AddPolicy(PermissionConstants.Students.Create,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Students.Create)));

        options.AddPolicy(PermissionConstants.Students.Update,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Students.Update)));

        options.AddPolicy(PermissionConstants.Students.Delete,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Students.Delete)));

        options.AddPolicy(PermissionConstants.Courses.View,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Courses.View)));

        options.AddPolicy(PermissionConstants.Courses.Create,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Courses.Create)));

        options.AddPolicy(PermissionConstants.Courses.Update,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Courses.Update)));

        options.AddPolicy(PermissionConstants.Courses.Delete,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Courses.Delete)));

        options.AddPolicy(PermissionConstants.Attendance.View,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Attendance.View)));

        options.AddPolicy(PermissionConstants.Attendance.Mark,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Attendance.Mark)));

        options.AddPolicy(PermissionConstants.ClassSessions.Generate,
            policy => policy.Requirements.Add(new PermissionRequirement(PermissionConstants.ClassSessions.Generate)));
    }
}