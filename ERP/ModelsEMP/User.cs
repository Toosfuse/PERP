using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class User
{
    public int Id { get; set; }

    public int ProgramId { get; set; }

    public string UserName { get; set; } = null!;
}
