using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class OrderItem
{
    public string Id { get; set; } = null!;

    public string OrderId { get; set; } = null!;

    public string ProductItemId { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal? Discount { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductItem ProductItem { get; set; } = null!;
}
