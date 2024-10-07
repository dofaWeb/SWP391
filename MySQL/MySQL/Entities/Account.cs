using System;
using System.Collections.Generic;

namespace MySQL.Entities;

public partial class Account
{
    public string Id { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public ulong IsActive { get; set; }

    public string RoleId { get; set; } = null!;

    public virtual RoleName Role { get; set; } = null!;

    public virtual Staff? Staff { get; set; }

    public virtual User? User { get; set; }
}
