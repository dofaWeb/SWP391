using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class DiscountLog
{
    public string Id { get; set; } = null!;

    public string ProductItemId { get; set; } = null!;

    public decimal OldDiscount { get; set; }

    public decimal NewDiscount { get; set; }

    public DateTime ChangeTimestamp { get; set; }

    public virtual ProductItem ProductItem { get; set; } = null!;

    public virtual ICollection<ProductLog> ProductLogs { get; set; } = new List<ProductLog>();
}
