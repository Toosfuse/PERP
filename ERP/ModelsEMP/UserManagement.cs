using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class UserManagement
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string? FirstName { get; set; }

    public string LastName { get; set; } = null!;

    public DateTime ActiveDate { get; set; }

    public DateTime? DeactiveDate { get; set; }

    public byte AccessRight { get; set; }

    public string? Comment { get; set; }

    public string? Password { get; set; }
}
