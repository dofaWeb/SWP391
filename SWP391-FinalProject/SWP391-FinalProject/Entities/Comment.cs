using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class Comment
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string? Comment1 { get; set; }

    public DateTime Date { get; set; }

    public string? ProductId { get; set; }

    public virtual Product? Product { get; set; }

    public virtual ICollection<ReplyComment> ReplyComments { get; set; } = new List<ReplyComment>();

    public virtual User User { get; set; } = null!;
}
