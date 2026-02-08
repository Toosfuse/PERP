using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class PackagingKasri
{
    public int Id { get; set; }

    public int? PackagingStationId { get; set; }

    public int? CartonNumber { get; set; }

    public int? PackageId { get; set; }
}
