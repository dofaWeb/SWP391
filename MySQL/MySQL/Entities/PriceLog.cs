using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class PriceLog
{
    public string Id { get; set; } = null!;

    public string ProductItemId { get; set; } = null!;

    public decimal OldPrice { get; set; }

    public decimal NewPrice { get; set; }

    public DateTime ChangeTimestamp { get; set; }

    public virtual ProductItem ProductItem { get; set; } = null!;

    public virtual ICollection<ProductLog> ProductLogs { get; set; } = new List<ProductLog>();
}
