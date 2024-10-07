using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class ProductItem
{
    public string Id { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal ImportPrice { get; set; }

    public decimal? SellingPrice { get; set; }

    public decimal? Discount { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<DiscountLog> DiscountLogs { get; set; } = new List<DiscountLog>();

    public virtual ICollection<NameLog> NameLogs { get; set; } = new List<NameLog>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PriceLog> PriceLogs { get; set; } = new List<PriceLog>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductConfiguration> ProductConfigurations { get; set; } = new List<ProductConfiguration>();

    public virtual ICollection<QuantityLog> QuantityLogs { get; set; } = new List<QuantityLog>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
