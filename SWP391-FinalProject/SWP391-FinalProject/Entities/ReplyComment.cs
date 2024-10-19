using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class ReplyComment
{
    public string Id { get; set; } = null!;

    public string? CommentId { get; set; }

    public string? Comment { get; set; }

    public DateTime Date { get; set; }

    public virtual Comment? CommentNavigation { get; set; }
}
