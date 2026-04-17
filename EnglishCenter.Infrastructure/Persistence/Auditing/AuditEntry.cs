using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EnglishCenter.Infrastructure.Persistence.Auditing;

public class AuditEntry
{
    public EntityEntry Entry { get; set; } = null!;
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;

    public Dictionary<string, object?> KeyValues { get; set; } = [];
    public Dictionary<string, object?> OldValues { get; set; } = [];
    public Dictionary<string, object?> NewValues { get; set; } = [];

    public bool HasTemporaryProperties { get; set; }
    public List<PropertyEntry> TemporaryProperties { get; set; } = [];
}
