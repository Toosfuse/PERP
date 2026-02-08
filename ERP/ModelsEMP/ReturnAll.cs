using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class ReturnAll
{
    public string? CityName { get; set; }

    public string? CityNameEn { get; set; }

    public string ReturnedNumber { get; set; } = null!;

    public int? Num { get; set; }

    public string? SentDate { get; set; }

    public DateOnly? RecieveDate { get; set; }

    public int? Pending { get; set; }

    public string State { get; set; } = null!;
}
