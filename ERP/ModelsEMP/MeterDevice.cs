using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class MeterDevice
{
    public int RegistrationId { get; set; }

    public string? MeterSerial { get; set; }

    public string MeterId { get; set; } = null!;

    public string? MeterCity { get; set; }

    public string? OrderId { get; set; }

    public string? FirmwareVersion { get; set; }

    public DateTime? ProductionTime { get; set; }

    public DateTime? RegistrationTime { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string? OperationPassword { get; set; }

    public int? OperationResult { get; set; }

    public int? OperationId { get; set; }

    public bool? IsValid { get; set; }

    public bool? IgnoreCityConflict { get; set; }

    public string? PreviousMeterSerial { get; set; }

    public int? MeterIdFk { get; set; }
}
