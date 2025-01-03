﻿using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class StaffShift
{
    public string Id { get; set; } = null!;

    public string StaffId { get; set; } = null!;

    public string? Shift { get; set; }

    public decimal? HourlyRate { get; set; }

    public DateOnly? Date { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Staff Staff { get; set; } = null!;
}
