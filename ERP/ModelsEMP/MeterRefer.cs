using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class MeterRefer
{
    public int Id { get; set; }

    public int? MeterId { get; set; }

    public int? PackageUserPk { get; set; }

    public string? DatePackage { get; set; }

    public int? PackagePk { get; set; }

    public string? Date { get; set; }

    public int? UserPk { get; set; }
}
