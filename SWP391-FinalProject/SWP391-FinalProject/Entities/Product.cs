using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class Product
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Picture { get; set; }

    public string? Description { get; set; }

    public string BrandId { get; set; } = null!;

    public int StateId { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<ProductItem> ProductItems { get; set; } = new List<ProductItem>();

    public virtual ProductState State { get; set; } = null!;
}
