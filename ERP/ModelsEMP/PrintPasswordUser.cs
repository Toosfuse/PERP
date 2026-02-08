using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class PrintPasswordUser
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Password { get; set; } = null!;
}
