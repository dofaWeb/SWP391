using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class User
{
    public string AccountId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Point { get; set; }

    public string? Province { get; set; }

    public string? District { get; set; }

    public string? Address { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
