using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class VariationOption
{
    public string Id { get; set; } = null!;

    public string VariationId { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual ICollection<ProductConfiguration> ProductConfigurations { get; set; } = new List<ProductConfiguration>();

    public virtual Variation Variation { get; set; } = null!;
}
