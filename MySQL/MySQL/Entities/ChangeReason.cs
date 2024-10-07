using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class ChangeReason
{
    public string Id { get; set; } = null!;

    public string? Reason { get; set; }

    public virtual ICollection<ProductLog> ProductLogs { get; set; } = new List<ProductLog>();
}
