using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class AppTypeClass
{
    public int ClassId { get; set; }

    public string? ClassName { get; set; }

    public virtual ICollection<AppType> AppTypes { get; set; } = new List<AppType>();
}
