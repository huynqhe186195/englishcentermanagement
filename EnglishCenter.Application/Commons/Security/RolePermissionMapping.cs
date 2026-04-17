using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Domain.Constants;

namespace EnglishCenter.Application.Common.Security;

public static class RolePermissionMapping
{
    public static List<string> GetPermissionsByRoles(IEnumerable<string> roles)
    {
        var permissions = new HashSet<string>();

        foreach (var role in roles)
        {
            switch (role)
            {
                case RoleConstants.SuperAdmin:
                    AddAllPermissions(permissions);
                    break;

                case RoleConstants.CenterAdmin:
                    AddAllPermissions(permissions);
                    break;

                case RoleConstants.Staff:
                    permissions.Add(PermissionConstants.Students.View);
                    permissions.Add(PermissionConstants.Students.Create);
                    permissions.Add(PermissionConstants.Students.Update);
                    permissions.Add(PermissionConstants.Courses.View);
                    permissions.Add(PermissionConstants.Classes.View);
                    permissions.Add(PermissionConstants.Enrollments.View);
                    permissions.Add(PermissionConstants.Enrollments.Create);
                    permissions.Add(PermissionConstants.Attendance.View);
                    break;

                case RoleConstants.Teacher:
                    permissions.Add(PermissionConstants.Classes.View);
                    permissions.Add(PermissionConstants.ClassSessions.View);
                    permissions.Add(PermissionConstants.Attendance.View);
                    permissions.Add(PermissionConstants.Attendance.Mark);
                    break;

                case RoleConstants.Parent:
                    permissions.Add(PermissionConstants.Students.View);
                    permissions.Add(PermissionConstants.Attendance.View);
                    break;

                case RoleConstants.Student:
                    permissions.Add(PermissionConstants.Attendance.View);
                    break;
            }
        }

        return permissions.ToList();
    }

    private static void AddAllPermissions(HashSet<string> permissions)
    {
        permissions.Add(PermissionConstants.Students.View);
        permissions.Add(PermissionConstants.Students.Create);
        permissions.Add(PermissionConstants.Students.Update);
        permissions.Add(PermissionConstants.Students.Delete);

        permissions.Add(PermissionConstants.Courses.View);
        permissions.Add(PermissionConstants.Courses.Create);
        permissions.Add(PermissionConstants.Courses.Update);
        permissions.Add(PermissionConstants.Courses.Delete);

        permissions.Add(PermissionConstants.Classes.View);
        permissions.Add(PermissionConstants.Classes.Create);
        permissions.Add(PermissionConstants.Classes.Update);
        permissions.Add(PermissionConstants.Classes.Delete);

        permissions.Add(PermissionConstants.Enrollments.View);
        permissions.Add(PermissionConstants.Enrollments.Create);
        permissions.Add(PermissionConstants.Enrollments.Update);
        permissions.Add(PermissionConstants.Enrollments.Delete);

        permissions.Add(PermissionConstants.ClassSchedules.View);
        permissions.Add(PermissionConstants.ClassSchedules.Create);
        permissions.Add(PermissionConstants.ClassSchedules.Update);
        permissions.Add(PermissionConstants.ClassSchedules.Delete);

        permissions.Add(PermissionConstants.ClassSessions.View);
        permissions.Add(PermissionConstants.ClassSessions.Create);
        permissions.Add(PermissionConstants.ClassSessions.Update);
        permissions.Add(PermissionConstants.ClassSessions.Delete);
        permissions.Add(PermissionConstants.ClassSessions.Generate);

        permissions.Add(PermissionConstants.Attendance.View);
        permissions.Add(PermissionConstants.Attendance.Mark);
    }
}
