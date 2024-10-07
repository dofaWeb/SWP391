using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class UserAddress
{
    public string UserId { get; set; } = null!;

    public string? AddressId { get; set; }

    public ulong IsDefault { get; set; }

    public virtual Address? Address { get; set; }

    public virtual User User { get; set; } = null!;
}
