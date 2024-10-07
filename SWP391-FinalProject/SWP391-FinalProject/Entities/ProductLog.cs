using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class ProductLog
{
    public string Id { get; set; } = null!;

    public string? NameLogId { get; set; }

    public string? QuantityLogId { get; set; }

    public string? PriceLogId { get; set; }

    public string? DisocuntLogId { get; set; }

    public string? ChangeReasonId { get; set; }

    public virtual ChangeReason? ChangeReason { get; set; }

    public virtual DiscountLog? DisocuntLog { get; set; }

    public virtual NameLog? NameLog { get; set; }

    public virtual PriceLog? PriceLog { get; set; }

    public virtual QuantityLog? QuantityLog { get; set; }
}
