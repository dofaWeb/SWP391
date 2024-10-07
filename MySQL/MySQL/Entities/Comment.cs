using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class Comment
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string ProductItemId { get; set; } = null!;

    public string? Comment1 { get; set; }

    public DateTime Date { get; set; }

    public virtual ProductItem ProductItem { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
