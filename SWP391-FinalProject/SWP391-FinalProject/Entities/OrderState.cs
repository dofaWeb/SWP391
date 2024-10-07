using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class OrderState
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
