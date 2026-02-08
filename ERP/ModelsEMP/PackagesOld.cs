using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class PackagesOld
{
    public int PackagingId { get; set; }

    public int? MeterBaseId { get; set; }

    public int? MeterTypeId { get; set; }

    public int? PackageTypeId { get; set; }

    public int? CoverTypeId { get; set; }

    public int? OrderId { get; set; }

    public string? MeterSerialStart { get; set; }

    public string? MeterSerialEnd { get; set; }

    public int? PackageCount { get; set; }

    public int? MeterCount { get; set; }

    public virtual AppType? CoverType { get; set; }

    public virtual AppType? MeterBase { get; set; }

    public virtual AppType? MeterType { get; set; }

    public virtual Order? Order { get; set; }

    public virtual AppType? PackageType { get; set; }
}
