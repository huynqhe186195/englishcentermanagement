using System;
using System.Collections.Generic;

namespace EnglishCenter.Domain.Models;

public partial class Room
{
    public long Id { get; set; }

    public long CampusId { get; set; }

    public string RoomCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Capacity { get; set; }

    public int RoomType { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Campus Campus { get; set; } = null!;

    public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();

    public virtual ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
