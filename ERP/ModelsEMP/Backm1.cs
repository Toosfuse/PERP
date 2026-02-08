using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class Backm1
{
    public int Id { get; set; }

    public string Serial { get; set; } = null!;

    public string? Type { get; set; }

    public int? InsertUserPk { get; set; }

    public int? PackageUserPk { get; set; }

    public string? DateInsert { get; set; }

    public string? DatePackage { get; set; }

    public int? PackagePk { get; set; }

    public int? PackageOrderId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? DatePhoneNumber { get; set; }

    public int? PhoneNumberUserPk { get; set; }

    public bool? IsReferMeter { get; set; }

    public string? PcbserialNumber { get; set; }

    public int? PcbserialNumberUserPk { get; set; }

    public string? PcbserialNumberDate { get; set; }

    public int? PackagingStationId { get; set; }

    public int? PackagingStationUserPk { get; set; }

    public string? PackagingStationDate { get; set; }

    public int? PackagingStationLevel2UserPk { get; set; }

    public string? PackagingStationLevel2Date { get; set; }
}
