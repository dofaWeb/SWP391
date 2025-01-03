﻿using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class ProductNameLog
{
    public string Id { get; set; } = null!;

    public string? ProductId { get; set; }

    public string OldName { get; set; } = null!;

    public string NewName { get; set; } = null!;

    public DateTime ChangeTimestamp { get; set; }

    public virtual ICollection<ProductLog> ProductLogs { get; set; } = new List<ProductLog>();
}
