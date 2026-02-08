using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class ReturnedStatus
{
    public short Code { get; set; }

    public string State { get; set; } = null!;
}
