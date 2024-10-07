﻿using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class Province
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}