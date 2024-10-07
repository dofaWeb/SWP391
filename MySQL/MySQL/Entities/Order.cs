using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class Order
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Address { get; set; } = null!;

    public int StateId { get; set; }

    public DateTime Date { get; set; }

    public decimal UsePoint { get; set; }

    public decimal EarnPoint { get; set; }

    public string StaffShiftId { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual StaffShift StaffShift { get; set; } = null!;

    public virtual OrderState State { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
