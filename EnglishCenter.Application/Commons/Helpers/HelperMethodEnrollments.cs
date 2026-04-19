using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Commons.Helpers
{
    public class HelperMethodEnrollments
    {
        public const string AttendanceWarningMarker = "[ATTENDANCE_WARNING_SENT_OVER_10]";
        public HelperMethodEnrollments() { }
        public bool HasAttendanceWarningSent(string? note)
        {
            return !string.IsNullOrWhiteSpace(note) &&
                   note.Contains(AttendanceWarningMarker, StringComparison.OrdinalIgnoreCase);
        }

        public string AppendAttendanceWarningMarker(string? note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return AttendanceWarningMarker;
            }

            if (note.Contains(AttendanceWarningMarker, StringComparison.OrdinalIgnoreCase))
            {
                return note;
            }

            return $"{note} {AttendanceWarningMarker}";
        }

        public string BuildAttendanceWarningEmailBody(
            string studentName,
            string className,
            string classCode,
            decimal absentRate,
            int absentCount,
            int validSessionCount)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Hello {studentName},");
            sb.AppendLine();
            sb.AppendLine("This is an attendance warning from the English Center.");
            sb.AppendLine();
            sb.AppendLine($"Class: {className} ({classCode})");
            sb.AppendLine($"Total valid sessions: {validSessionCount}");
            sb.AppendLine($"Absent count: {absentCount}");
            sb.AppendLine($"Absent rate: {absentRate}%");
            sb.AppendLine();
            sb.AppendLine("Your absence rate has exceeded the allowed threshold of 10%.");
            sb.AppendLine("Please improve your attendance to avoid disciplinary actions.");
            sb.AppendLine();
            sb.AppendLine("Best regards,");
            sb.AppendLine("English Center");

            return sb.ToString();
        }
    }
}
