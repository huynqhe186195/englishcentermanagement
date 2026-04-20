using System;
using System.Collections.Generic;

namespace EnglishCenter.Domain.Models;

public partial class User
{
    public long Id { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string FullName { get; set; } = null!;

    public int Status { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public long? CampusId { get; set; }

    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Campus? Campus { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Parent> Parents { get; set; } = new List<Parent>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ProgressReport> ProgressReports { get; set; } = new List<ProgressReport>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
