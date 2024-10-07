using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class Address
{
    public string Id { get; set; } = null!;

    public string? ProvinceId { get; set; }

    public string Address1 { get; set; } = null!;

    public virtual Province? Province { get; set; }

    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
}
