using System;
using System.Collections.Generic;

namespace SWP391_FinalProject.Entities;

public partial class VariationOption
{
    public string Id { get; set; } = null!;

    public string VariationId { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual Variation Variation { get; set; } = null!;
}
