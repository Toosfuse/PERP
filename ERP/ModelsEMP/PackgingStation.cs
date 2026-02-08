using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class PackgingStation
{
    public int Id { get; set; }

    public string? StationName { get; set; }

    public int? StationNumber { get; set; }

    public int? Capacity { get; set; }

    public bool? Status { get; set; }

    public bool? Enable { get; set; }
}
