using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class QuantityLog
{
    public string Id { get; set; } = null!;

    public string ProductItemId { get; set; } = null!;

    public int OldQuantity { get; set; }

    public int NewQuantity { get; set; }

    public DateTime ChangeTimestamp { get; set; }

    public virtual ProductItem ProductItem { get; set; } = null!;

    public virtual ICollection<ProductLog> ProductLogs { get; set; } = new List<ProductLog>();
}
