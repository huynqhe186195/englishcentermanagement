namespace EnglishCenter.Domain.Constants;

public static class PermissionConstants
{
    public static class Students
    {
        public const string View = "students.view";
        public const string Create = "students.create";
        public const string Update = "students.update";
        public const string Delete = "students.delete";
    }

    public static class Users
    {
        public const string ResetPassword = "users.resetpassword";
    }

    public static class Courses
    {
        public const string View = "courses.view";
        public const string Create = "courses.create";
        public const string Update = "courses.update";
        public const string Delete = "courses.delete";
    }

    public static class Classes
    {
        public const string View = "classes.view";
        public const string Create = "classes.create";
        public const string Update = "classes.update";
        public const string Delete = "classes.delete";
    }

    public static class Enrollments
    {
        public const string View = "enrollments.view";
        public const string Create = "enrollments.create";
        public const string Update = "enrollments.update";
        public const string Delete = "enrollments.delete";
    }

    public static class Attendance
    {
        public const string View = "attendance.view";
        public const string Mark = "attendance.mark";
    }

    public static class ClassSessions
    {
        public const string View = "classsessions.view";
        public const string Create = "classsessions.create";
        public const string Update = "classsessions.update";
        public const string Delete = "classsessions.delete";
        public const string Generate = "classsessions.generate";
    }

    public static class ClassSchedules
    {
        public const string View = "classschedules.view";
        public const string Create = "classschedules.create";
        public const string Update = "classschedules.update";
        public const string Delete = "classschedules.delete";
    }
}