using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class Staff
{
    public string AccountId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Salary { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();
}
