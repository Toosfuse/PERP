using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class OnlineStation
{
    public int Id { get; set; }

    public int PackageId { get; set; }

    public int StationId { get; set; }

    public string? Cartons { get; set; }

    public int? CartonsCount { get; set; }

    public int? MaxCarton { get; set; }

    public int? Priority { get; set; }

    public bool? IsStopped { get; set; }
}
