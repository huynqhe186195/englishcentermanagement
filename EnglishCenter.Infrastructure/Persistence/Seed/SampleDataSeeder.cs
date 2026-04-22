using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Seed;

public class SampleDataSeeder
{
    private readonly IApplicationDbContext _context;

    public SampleDataSeeder(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedCampusesAsync();
        await SeedCoursesAsync();
        await SeedClassesAsync();
        await SeedRoomsAsync();
        await SeedTeachersAsync();
        await SeedStudentsAsync();
        await SeedEnrollmentsAndSessionsAsync();
        await SeedClassTeachersAsync();
        await SeedParentsAndStudentParentsAsync();
        await SeedAssignmentsAndSubmissionsAsync();
        await SeedExamsAndScoresAsync();
        await SeedDiscountsInvoicesPaymentsAsync();
        await SeedNotificationsAsync();
        await SeedProgressReportsAsync();
    }

    private async Task SeedCampusesAsync()
    {
        var list = new[] {
            new Campus { CampusCode = "HCM01", Name = "HCM Downtown", Address = "123 Le Loi, HCMC", Phone = "0123456789", Status = 1, CreatedAt = DateTime.UtcNow },
            new Campus { CampusCode = "HCM02", Name = "HCM East", Address = "45 Vo Van Kiet, HCMC", Phone = "0987654321", Status = 1, CreatedAt = DateTime.UtcNow }
        };

        foreach (var c in list)
        {
            var exists = await _context.Campuses.AnyAsync(x => x.CampusCode == c.CampusCode);
            if (!exists) _context.Campuses.Add(c);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedCoursesAsync()
    {
        var courses = new[] {
            new Course { CourseCode = "ELM-BEG", Name = "English Beginner", Description = "Basic English course", Level = "Beginner", TotalSessions = 24, DefaultFee = 2000000, Status = 1, CreatedAt = DateTime.UtcNow },
            new Course { CourseCode = "ELM-INT", Name = "English Intermediate", Description = "Intermediate English", Level = "Intermediate", TotalSessions = 30, DefaultFee = 3000000, Status = 1, CreatedAt = DateTime.UtcNow }
        };

        foreach (var c in courses)
        {
            var exists = await _context.Courses.AnyAsync(x => x.CourseCode == c.CourseCode);
            if (!exists) _context.Courses.Add(c);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedClassesAsync()
    {
        var course = await _context.Courses.FirstOrDefaultAsync(x => x.CourseCode == "ELM-BEG");
        var campus = await _context.Campuses.FirstOrDefaultAsync();
        if (course == null || campus == null) return;

        var classes = new[] {
            new Class { ClassCode = "CL-HCM-BEG-1", CourseId = course.Id, CampusId = campus.Id, Name = "Beginners A", StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(3)), MaxStudents = 20, TuitionFee = course.DefaultFee, Status = 1, CreatedAt = DateTime.UtcNow },
            new Class { ClassCode = "CL-HCM-BEG-2", CourseId = course.Id, CampusId = campus.Id, Name = "Beginners B", StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7)), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(3).AddDays(7)), MaxStudents = 20, TuitionFee = course.DefaultFee, Status = 1, CreatedAt = DateTime.UtcNow }
        };

        foreach (var cl in classes)
        {
            var exists = await _context.Classes.AnyAsync(x => x.ClassCode == cl.ClassCode);
            if (!exists) _context.Classes.Add(cl);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedRoomsAsync()
    {
        var campus = await _context.Campuses.FirstOrDefaultAsync();
        if (campus == null) return;

        var rooms = new[] {
            new Room { RoomCode = "R-101", Name = "Room 101", CampusId = campus.Id, Status = 1, CreatedAt = DateTime.UtcNow },
            new Room { RoomCode = "R-102", Name = "Room 102", CampusId = campus.Id, Status = 1, CreatedAt = DateTime.UtcNow }
        };

        foreach (var r in rooms)
        {
            var exists = await _context.Rooms.AnyAsync(x => x.RoomCode == r.RoomCode);
            if (!exists) _context.Rooms.Add(r);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedTeachersAsync()
    {
        var campus = await _context.Campuses.FirstOrDefaultAsync();
        if (campus == null) return;

        var teachers = new[] {
            new Teacher { TeacherCode = "T-001", FullName = "Nguyen Van A", Email = "nguyenvana@englishcenter.local", Phone = "0900000001", Status = 1, CreatedAt = DateTime.UtcNow, CampusId = campus.Id },
            new Teacher { TeacherCode = "T-002", FullName = "Tran Thi B", Email = "tranthib@englishcenter.local", Phone = "0900000002", Status = 1, CreatedAt = DateTime.UtcNow, CampusId = campus.Id }
        };

        foreach (var t in teachers)
        {
            var exists = await _context.Teachers.AnyAsync(x => x.TeacherCode == t.TeacherCode);
            if (!exists) _context.Teachers.Add(t);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedStudentsAsync()
    {
        var students = new[] {
            new Student { StudentCode = "S-001", FullName = "Le Student One", DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-15)), Phone = "0910000001", Email = "student1@school.local", Status = 1, CreatedAt = DateTime.UtcNow },
            new Student { StudentCode = "S-002", FullName = "Pham Student Two", DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-14)), Phone = "0910000002", Email = "student2@school.local", Status = 1, CreatedAt = DateTime.UtcNow },
            new Student { StudentCode = "S-003", FullName = "Ho Student Three", DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-13)), Phone = "0910000003", Email = "student3@school.local", Status = 1, CreatedAt = DateTime.UtcNow }
        };

        foreach (var s in students)
        {
            var exists = await _context.Students.AnyAsync(x => x.StudentCode == s.StudentCode);
            if (!exists) _context.Students.Add(s);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedEnrollmentsAndSessionsAsync()
    {
        var cl = await _context.Classes.FirstOrDefaultAsync(x => x.ClassCode == "CL-HCM-BEG-1");
        if (cl == null) return;

        var students = await _context.Students.Where(s => s.StudentCode.StartsWith("S-")) .ToListAsync();
        if (!students.Any()) return;

        // Enroll students
        foreach (var s in students)
        {
            var exists = await _context.Enrollments.AnyAsync(e => e.StudentId == s.Id && e.ClassId == cl.Id && !e.IsDeleted);
            if (!exists)
            {
                _context.Enrollments.Add(new Enrollment { StudentId = s.Id, ClassId = cl.Id, EnrollDate = DateOnly.FromDateTime(DateTime.UtcNow.Date), Status = 1, CreatedAt = DateTime.UtcNow });
            }
        }

        await _context.SaveChangesAsync();

        // Create weekly sessions for next 8 weeks on Monday and Wednesday at 16:00-17:30
        var sessionDate = DateTime.UtcNow.Date.AddDays(1); // start tomorrow
        var sessionsToCreate = new List<ClassSession>();
        var teacher = await _context.Teachers.FirstOrDefaultAsync();
        for (int week = 0; week < 8; week++)
        {
            var mon = sessionDate.AddDays(week * 7);
            var wed = mon.AddDays(2);

            sessionsToCreate.Add(new ClassSession { ClassId = cl.Id, SessionNo = week * 2 + 1, SessionDate = DateOnly.FromDateTime(mon), StartTime = new TimeOnly(16,0), EndTime = new TimeOnly(17,30), TeacherId = teacher?.Id, Status = 1, CreatedAt = DateTime.UtcNow });
            sessionsToCreate.Add(new ClassSession { ClassId = cl.Id, SessionNo = week * 2 + 2, SessionDate = DateOnly.FromDateTime(wed), StartTime = new TimeOnly(16,0), EndTime = new TimeOnly(17,30), TeacherId = teacher?.Id, Status = 1, CreatedAt = DateTime.UtcNow });
        }

        foreach (var cs in sessionsToCreate)
        {
            var exists = await _context.ClassSessions.AnyAsync(x => x.ClassId == cs.ClassId && x.SessionDate == cs.SessionDate && x.StartTime == cs.StartTime);
            if (!exists) _context.ClassSessions.Add(cs);
        }

        await _context.SaveChangesAsync();

        // Create attendance records for first session
        var firstSession = await _context.ClassSessions.Where(x => x.ClassId == cl.Id).OrderBy(x => x.SessionDate).FirstOrDefaultAsync();
        if (firstSession != null)
        {
            var enrollments = await _context.Enrollments.Where(e => e.ClassId == cl.Id && !e.IsDeleted && e.Status == 1).ToListAsync();
            foreach (var en in enrollments)
            {
                var exists = await _context.AttendanceRecords.AnyAsync(a => a.SessionId == firstSession.Id && a.StudentId == en.StudentId);
                if (!exists)
                {
                    _context.AttendanceRecords.Add(new AttendanceRecord { SessionId = firstSession.Id, StudentId = en.StudentId, Status = 1, CheckedAt = DateTime.UtcNow, CheckedByUserId = null });
                }
            }
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedClassTeachersAsync()
    {
        var cl = await _context.Classes.FirstOrDefaultAsync();
        var teacher = await _context.Teachers.FirstOrDefaultAsync();
        if (cl == null || teacher == null) return;

        var exists = await _context.ClassTeachers.AnyAsync(ct => ct.ClassId == cl.Id && ct.TeacherId == teacher.Id);
        if (!exists)
        {
            _context.ClassTeachers.Add(new ClassTeacher { ClassId = cl.Id, TeacherId = teacher.Id, IsMainTeacher = true, AssignedFrom = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedParentsAndStudentParentsAsync()
    {
        var parents = new[] {
            new Parent { FullName = "Parent One", Phone = "0900111001", Email = "parent1@example.local", CreatedAt = DateTime.UtcNow },
            new Parent { FullName = "Parent Two", Phone = "0900111002", Email = "parent2@example.local", CreatedAt = DateTime.UtcNow }
        };

        foreach (var p in parents)
        {
            var exists = await _context.Parents.AnyAsync(x => x.Email == p.Email || x.FullName == p.FullName);
            if (!exists) _context.Parents.Add(p);
        }
        await _context.SaveChangesAsync();

        // link first parent to first student
        var student = await _context.Students.FirstOrDefaultAsync();
        var parent = await _context.Parents.FirstOrDefaultAsync();
        if (student != null && parent != null)
        {
            var exists = await _context.StudentParents.AnyAsync(sp => sp.StudentId == student.Id && sp.ParentId == parent.Id);
            if (!exists)
            {
                _context.StudentParents.Add(new StudentParent { StudentId = student.Id, ParentId = parent.Id, Relationship = "Father", IsPrimaryContact = true, CreatedAt = DateTime.UtcNow });
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task SeedAssignmentsAndSubmissionsAsync()
    {
        var cl = await _context.Classes.FirstOrDefaultAsync();
        var enroll = await _context.Enrollments.FirstOrDefaultAsync(e => e.ClassId == cl.Id);
        if (cl == null || enroll == null) return;

        var assignment = new Assignment { ClassId = cl.Id, Title = "Homework 1", Description = "Read chapter 1", DueDate = DateTime.UtcNow.AddDays(7), CreatedAt = DateTime.UtcNow };
        if (!await _context.Assignments.AnyAsync(a => a.ClassId == cl.Id && a.Title == assignment.Title))
        {
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // submission
            _context.AssignmentSubmissions.Add(new AssignmentSubmission { AssignmentId = assignment.Id, StudentId = enroll.StudentId, SubmittedAt = DateTime.UtcNow, SubmissionText = "Submitted via portal" });
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedExamsAndScoresAsync()
    {
        var cl = await _context.Classes.FirstOrDefaultAsync(x => x.ClassCode == "CL-HCM-BEG-1");
        if (cl == null) return;

        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "admin") ;

        // create an exam in two weeks
        var examDate = DateTime.UtcNow.Date.AddDays(14).AddHours(9);
        var existsExam = await _context.Exams.AnyAsync(e => e.ClassId == cl.Id && e.ExamDate == examDate);
        if (!existsExam)
        {
            var exam = new Exam { ClassId = cl.Id, Title = "Unit Test 1", ExamType = 1, ExamDate = examDate, MaxScore = 10, Description = "Unit test for chapter 1-3", CreatedByUserId = adminUser?.Id, CreatedAt = DateTime.UtcNow };
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            // create sample scores for enrolled students
            var enrollments = await _context.Enrollments.Where(e => e.ClassId == cl.Id && !e.IsDeleted && e.Status == 1).ToListAsync();
            var rnd = new Random();
            foreach (var en in enrollments)
            {
                var scoreVal = (decimal)(rnd.NextDouble() * 10.0);
                _context.Scores.Add(new Score { ExamId = exam.Id, StudentId = en.StudentId, ScoreValue = Math.Round(scoreVal,2), CreatedAt = DateTime.UtcNow });
            }
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedDiscountsInvoicesPaymentsAsync()
    {
        var student = await _context.Students.FirstOrDefaultAsync();
        var cl = await _context.Classes.FirstOrDefaultAsync();
        if (student == null) return;

        // discount
        var discount = new Discount { DiscountCode = "DISC10", Name = "10% Off", DiscountType = 1, Value = 10, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        if (!await _context.Discounts.AnyAsync(d => d.DiscountCode == discount.DiscountCode)) _context.Discounts.Add(discount);
        await _context.SaveChangesAsync();

        // invoice
        if (cl != null)
        {
            var invoiceNo = "INV-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var exists = await _context.Invoices.AnyAsync(i => i.InvoiceNo == invoiceNo);
            if (!exists)
            {
                var inv = new Invoice { InvoiceNo = invoiceNo, StudentId = student.Id, ClassId = cl.Id, TotalAmount = cl.TuitionFee, DiscountAmount = 0, FinalAmount = cl.TuitionFee, PaidAmount = 0, RefundedAmount = 0, Status = 0, CreatedAt = DateTime.UtcNow };
                _context.Invoices.Add(inv);
                await _context.SaveChangesAsync();

                // payment
                _context.Payments.Add(new Payment { InvoiceId = inv.Id, Amount = inv.FinalAmount, PaymentDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
                inv.PaidAmount = inv.FinalAmount;
                inv.Status = 1; // paid
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task SeedNotificationsAsync()
    {
        var user = await _context.Users.FirstOrDefaultAsync();
        if (user == null) return;
        if (!await _context.Notifications.AnyAsync(n => n.Title == "Welcome"))
        {
            _context.Notifications.Add(new Notification { Title = "Welcome", Content = "Welcome to English Center system.", Channel = 0, TargetType = 0, TargetId = user.Id, Status = 1, CreatedByUserId = user.Id, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedProgressReportsAsync()
    {
        var cl = await _context.Classes.FirstOrDefaultAsync();
        var student = await _context.Students.FirstOrDefaultAsync();
        if (cl == null || student == null) return;
        if (!await _context.ProgressReports.AnyAsync(pr => pr.ClassId == cl.Id && pr.StudentId == student.Id))
        {
            _context.ProgressReports.Add(new ProgressReport { ClassId = cl.Id, StudentId = student.Id, ReportPeriod = "Initial", TeacherComment = "Initial report", CreatedByUserId = null, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
        }
    }
}
