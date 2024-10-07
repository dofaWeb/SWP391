using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class Brand
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
