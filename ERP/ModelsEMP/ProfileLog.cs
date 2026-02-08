using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class ProfileLog
{
    public int? Id { get; set; }

    public int? CityId { get; set; }

    public string? MeterType { get; set; }

    public string? OperatorPassword { get; set; }

    public string? SettingPassword { get; set; }

    public string? UtilityPassword { get; set; }

    public string? ManufactoryPassword { get; set; }

    public string? Date { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public int? Last { get; set; }
}
