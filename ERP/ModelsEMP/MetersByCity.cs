using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class MetersByCity
{
    public string Serial { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public int PackagingId { get; set; }

    public int? MeterBaseId { get; set; }

    public int? MeterTypeId { get; set; }

    public int? RsportTypeId { get; set; }

    public string? OrderNumber { get; set; }

    public string? OrderRegNumber { get; set; }

    public string? StartSerial { get; set; }

    public string? EndSerial { get; set; }

    public string? PcbserialNumber { get; set; }

    public string? DatePhoneNumber { get; set; }

    public int? CityId { get; set; }

    public int Id { get; set; }
}
