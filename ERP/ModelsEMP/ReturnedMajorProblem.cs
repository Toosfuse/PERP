using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class ReturnedMajorProblem
{
    public short Code { get; set; }

    public string English { get; set; } = null!;

    public string Persian { get; set; } = null!;
}
