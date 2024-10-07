using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class ProductConfiguration
{
    public int Id { get; set; }

    public string ProductItemId { get; set; } = null!;

    public string VariationOptionId { get; set; } = null!;

    public virtual ProductItem ProductItem { get; set; } = null!;

    public virtual VariationOption VariationOption { get; set; } = null!;
}
