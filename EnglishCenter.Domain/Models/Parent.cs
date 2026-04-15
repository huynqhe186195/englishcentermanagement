using System;
using System.Collections.Generic;

namespace EnglishCenter.Domain.Models;

public partial class Parent
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Occupation { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();

    public virtual User? User { get; set; }
}
