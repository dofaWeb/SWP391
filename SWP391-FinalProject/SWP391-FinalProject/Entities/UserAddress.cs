using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class UserAddress
{
    public string UserId { get; set; } = null!;

    public string? AddressId { get; set; }

    public bool IsDefault { get; set; }

    public virtual Address? Address { get; set; }

    public virtual User User { get; set; } = null!;
}
