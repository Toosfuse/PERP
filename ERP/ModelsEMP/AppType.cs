using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class AppType
{
    public int TypeId { get; set; }

    public int? TypeClassId { get; set; }

    public string? TypeName { get; set; }

    public string? TypeDetails { get; set; }

    public virtual ICollection<OperationsLog> OperationsLogs { get; set; } = new List<OperationsLog>();

    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();

    public virtual ICollection<PackagesOld> PackagesOldCoverTypes { get; set; } = new List<PackagesOld>();

    public virtual ICollection<PackagesOld> PackagesOldMeterBases { get; set; } = new List<PackagesOld>();

    public virtual ICollection<PackagesOld> PackagesOldMeterTypes { get; set; } = new List<PackagesOld>();

    public virtual ICollection<PackagesOld> PackagesOldPackageTypes { get; set; } = new List<PackagesOld>();

    public virtual AppTypeClass? TypeClass { get; set; }
}
