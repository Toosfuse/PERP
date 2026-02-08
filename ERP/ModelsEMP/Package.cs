using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class Package
{
    public int PackagingId { get; set; }

    public int? MeterBaseId { get; set; }

    public int? MeterTypeId { get; set; }

    public int? PackageTypeId { get; set; }

    public int? CoverTypeId { get; set; }

    public int? ModuleTypeId { get; set; }

    public int? BoardTypeId { get; set; }

    public int? AccessoriesTypeId { get; set; }

    public int? RsportTypeId { get; set; }

    public int? OrderId { get; set; }

    public int? PackageCount { get; set; }

    public int? MeterCount { get; set; }

    public string? StartSerial { get; set; }

    public string? EndSerial { get; set; }

    public DateTime? RegisterTime { get; set; }

    public int? OperatorId { get; set; }

    public string? FrimwareVersion { get; set; }

    public string? ProfileName { get; set; }

    public string? CheckSum { get; set; }

    public bool? ShowInBarcodeScan { get; set; }

    public virtual AppType? CoverType { get; set; }
 
    public virtual Order? Order { get; set; }

}
