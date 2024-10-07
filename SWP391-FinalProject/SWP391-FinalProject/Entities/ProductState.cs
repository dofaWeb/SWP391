using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class ProductState
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
