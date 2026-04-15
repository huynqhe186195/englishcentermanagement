using System;
using System.Collections.Generic;
using EnglishCenter.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Infrastructure.Persistence.Context;

public partial class EnglishCenterDbContext : DbContext
{
    public EnglishCenterDbContext(DbContextOptions<EnglishCenterDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }

    public virtual DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Campus> Campuses { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassSchedule> ClassSchedules { get; set; }

    public virtual DbSet<ClassSession> ClassSessions { get; set; }

    public virtual DbSet<ClassTeacher> ClassTeachers { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDiscount> InvoiceDiscounts { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Parent> Parents { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<ProgressReport> ProgressReports { get; set; }

    public virtual DbSet<Refund> Refunds { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentParent> StudentParents { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<VwAttendanceSummary> VwAttendanceSummaries { get; set; }

    public virtual DbSet<VwClassEnrollmentSummary> VwClassEnrollmentSummaries { get; set; }

    public virtual DbSet<VwStudentBillingSummary> VwStudentBillingSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasIndex(e => e.ClassId, "IX_Assignments_ClassId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.MaxScore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Class).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assignments_Classes");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_Assignments_Users");
        });

        modelBuilder.Entity<AssignmentSubmission>(entity =>
        {
            entity.HasIndex(e => new { e.AssignmentId, e.StudentId }, "UX_AssignmentSubmissions_AssignmentId_StudentId").IsUnique();

            entity.Property(e => e.Feedback).HasMaxLength(2000);
            entity.Property(e => e.FileUrl).HasMaxLength(1000);
            entity.Property(e => e.Score).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Assignment).WithMany(p => p.AssignmentSubmissions)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssignmentSubmissions_Assignments");

            entity.HasOne(d => d.GradedByUser).WithMany(p => p.AssignmentSubmissions)
                .HasForeignKey(d => d.GradedByUserId)
                .HasConstraintName("FK_AssignmentSubmissions_Users");

            entity.HasOne(d => d.Student).WithMany(p => p.AssignmentSubmissions)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssignmentSubmissions_Students");
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasIndex(e => new { e.SessionId, e.StudentId }, "UX_AttendanceRecords_SessionId_StudentId").IsUnique();

            entity.Property(e => e.CheckedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.HasOne(d => d.CheckedByUser).WithMany(p => p.AttendanceRecords)
                .HasForeignKey(d => d.CheckedByUserId)
                .HasConstraintName("FK_AttendanceRecords_Users");

            entity.HasOne(d => d.Session).WithMany(p => p.AttendanceRecords)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttendanceRecords_ClassSessions");

            entity.HasOne(d => d.Student).WithMany(p => p.AttendanceRecords)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttendanceRecords_Students");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => new { e.EntityName, e.EntityId }, "IX_AuditLogs_EntityName_EntityId");

            entity.HasIndex(e => e.UserId, "IX_AuditLogs_UserId");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.EntityName).HasMaxLength(255);
            entity.Property(e => e.IpAddress).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AuditLogs_Users");
        });

        modelBuilder.Entity<Campus>(entity =>
        {
            entity.HasIndex(e => e.CampusCode, "UX_Campuses_CampusCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CampusCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status).HasDefaultValue(1);
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasIndex(e => e.ClassCode, "UX_Classes_ClassCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.ClassCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.TuitionFee).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Campus).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CampusId)
                .HasConstraintName("FK_Classes_Campuses");

            entity.HasOne(d => d.Course).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classes_Courses");

            entity.HasOne(d => d.Room).WithMany(p => p.Classes)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_Classes_Rooms");
        });

        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.HasIndex(e => e.ClassId, "IX_ClassSchedules_ClassId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassSchedules)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassSchedules_Classes");

            entity.HasOne(d => d.Room).WithMany(p => p.ClassSchedules)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_ClassSchedules_Rooms");
        });

        modelBuilder.Entity<ClassSession>(entity =>
        {
            entity.HasIndex(e => new { e.ClassId, e.SessionDate }, "IX_ClassSessions_ClassId_SessionDate");

            entity.HasIndex(e => new { e.ClassId, e.SessionDate, e.StartTime }, "UX_ClassSessions_ClassId_SessionDate_StartTime").IsUnique();

            entity.HasIndex(e => new { e.ClassId, e.SessionNo }, "UX_ClassSessions_ClassId_SessionNo").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.Topic).HasMaxLength(255);

            entity.HasOne(d => d.Class).WithMany(p => p.ClassSessions)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassSessions_Classes");

            entity.HasOne(d => d.Room).WithMany(p => p.ClassSessions)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_ClassSessions_Rooms");

            entity.HasOne(d => d.Teacher).WithMany(p => p.ClassSessions)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK_ClassSessions_Teachers");
        });

        modelBuilder.Entity<ClassTeacher>(entity =>
        {
            entity.HasIndex(e => e.ClassId, "IX_ClassTeachers_ClassId");

            entity.HasIndex(e => e.TeacherId, "IX_ClassTeachers_TeacherId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassTeachers)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassTeachers_Classes");

            entity.HasOne(d => d.Teacher).WithMany(p => p.ClassTeachers)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassTeachers_Teachers");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.CourseCode, "UX_Courses_CourseCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DefaultFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Level).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Status).HasDefaultValue(1);
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasIndex(e => e.DiscountCode, "UX_Discounts_DiscountCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DiscountCode).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.Value).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => new { e.ClassId, e.Status }, "IX_Enrollments_ClassId_Status");

            entity.HasIndex(e => new { e.StudentId, e.ClassId }, "UX_Enrollments_StudentId_ClassId")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(d => d.Class).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_Classes");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_Students");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasIndex(e => e.ClassId, "IX_Exams_ClassId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.MaxScore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Class).WithMany(p => p.Exams)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exams_Classes");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Exams)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_Exams_Users");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(e => e.ClassId, "IX_Invoices_ClassId");

            entity.HasIndex(e => e.StudentId, "IX_Invoices_StudentId");

            entity.HasIndex(e => e.InvoiceNo, "UX_Invoices_InvoiceNo")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FinalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.InvoiceNo).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefundedAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Class).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_Invoices_Classes");

            entity.HasOne(d => d.Student).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_Students");
        });

        modelBuilder.Entity<InvoiceDiscount>(entity =>
        {
            entity.HasIndex(e => new { e.InvoiceId, e.DiscountId }, "UX_InvoiceDiscounts_InvoiceId_DiscountId").IsUnique();

            entity.Property(e => e.AppliedValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Discount).WithMany(p => p.InvoiceDiscounts)
                .HasForeignKey(d => d.DiscountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceDiscounts_Discounts");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDiscounts)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceDiscounts_Invoices");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "IX_Notifications_TargetType_TargetId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.Occupation).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.Parents)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Parents_Users");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_Payments_AfterInsertUpdateDelete"));

            entity.HasIndex(e => e.InvoiceId, "IX_Payments_InvoiceId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.TransactionCode).HasMaxLength(100);

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Invoices");

            entity.HasOne(d => d.ReceivedByUser).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ReceivedByUserId)
                .HasConstraintName("FK_Payments_Users");
        });

        modelBuilder.Entity<ProgressReport>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.ClassId }, "IX_ProgressReports_StudentId_ClassId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GrammarScore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ListeningScore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ReadingScore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Recommendation).HasMaxLength(2000);
            entity.Property(e => e.ReportPeriod).HasMaxLength(100);
            entity.Property(e => e.SpeakingScore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TeacherComment).HasMaxLength(2000);
            entity.Property(e => e.VocabularyScore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.WritingScore).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Class).WithMany(p => p.ProgressReports)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgressReports_Classes");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ProgressReports)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_ProgressReports_Users");

            entity.HasOne(d => d.Student).WithMany(p => p.ProgressReports)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgressReports_Students");
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_Refunds_AfterInsertUpdateDelete"));

            entity.HasIndex(e => e.InvoiceId, "IX_Refunds_InvoiceId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Reason).HasMaxLength(1000);

            entity.HasOne(d => d.Invoice).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Refunds_Invoices");

            entity.HasOne(d => d.ProcessedByUser).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.ProcessedByUserId)
                .HasConstraintName("FK_Refunds_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Code, "UX_Roles_Code")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(e => new { e.CampusId, e.RoomCode }, "UX_Rooms_Campus_RoomCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.RoomCode).HasMaxLength(50);
            entity.Property(e => e.RoomType).HasDefaultValue(1);
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(d => d.Campus).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.CampusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rooms_Campuses");
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasIndex(e => new { e.ExamId, e.StudentId }, "UX_Scores_ExamId_StudentId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Remark).HasMaxLength(1000);
            entity.Property(e => e.ScoreValue).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Exam).WithMany(p => p.Scores)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Scores_Exams");

            entity.HasOne(d => d.Student).WithMany(p => p.Scores)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Scores_Students");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.StudentCode, "UX_Students_StudentCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.EnglishLevel).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.SchoolName).HasMaxLength(255);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.StudentCode).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Students_Users");
        });

        modelBuilder.Entity<StudentParent>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.ParentId }, "UX_StudentParents_StudentId_ParentId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Relationship).HasMaxLength(50);

            entity.HasOne(d => d.Parent).WithMany(p => p.StudentParents)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentParents_Parents");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentParents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentParents_Students");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasIndex(e => e.TeacherCode, "UX_Teachers_TeacherCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Qualification).HasMaxLength(255);
            entity.Property(e => e.Specialization).HasMaxLength(255);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.TeacherCode).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Teachers_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.UserName, "UX_Users_UserName")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Users");
        });

        modelBuilder.Entity<VwAttendanceSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_AttendanceSummary");

            entity.Property(e => e.StudentCode).HasMaxLength(50);
            entity.Property(e => e.StudentName).HasMaxLength(255);
        });

        modelBuilder.Entity<VwClassEnrollmentSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ClassEnrollmentSummary");

            entity.Property(e => e.ClassCode).HasMaxLength(50);
            entity.Property(e => e.ClassName).HasMaxLength(255);
            entity.Property(e => e.CourseName).HasMaxLength(255);
        });

        modelBuilder.Entity<VwStudentBillingSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_StudentBillingSummary");

            entity.Property(e => e.StudentCode).HasMaxLength(50);
            entity.Property(e => e.StudentName).HasMaxLength(255);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TotalDiscount).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TotalFinalAmount).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TotalOutstanding).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TotalPaidAmount).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TotalRefundedAmount).HasColumnType("decimal(38, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
