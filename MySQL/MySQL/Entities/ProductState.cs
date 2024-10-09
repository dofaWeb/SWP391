using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class ProductState
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
