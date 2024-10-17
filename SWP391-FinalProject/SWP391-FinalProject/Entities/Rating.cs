using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class Rating
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public int Rating1 { get; set; }

    public string? ProductId { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User User { get; set; } = null!;
}
