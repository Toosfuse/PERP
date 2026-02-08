using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class KeyManagement
{
    public int CityId { get; set; }

    public int MeterTypeId { get; set; }

    public string? Masterkey { get; set; }

    public string? Ak { get; set; }

    public string? Ek { get; set; }

    public string? Bk { get; set; }

    public string? ReadingClientAk { get; set; }

    public string? ReadingClientEk { get; set; }
}
