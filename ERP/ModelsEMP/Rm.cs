using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class Rm
{
    public string ReturnedNumber { get; set; } = null!;

    public string ReceivedMeter { get; set; } = null!;

    public string Customer { get; set; } = null!;

    public string? CityName { get; set; }

    public string? CityNameEn { get; set; }

    public string? SimcardNumber { get; set; }

    public string Report { get; set; } = null!;

    public string? SentMeter { get; set; }

    public string? SentDate { get; set; }

    public string? SentBy { get; set; }

    public string? Solution { get; set; }

    public string? State { get; set; }

    public string? English { get; set; }

    public string? Persian { get; set; }

    public string? Expr2 { get; set; }

    public string? Expr3 { get; set; }

    public short? MajorCode { get; set; }

    public string? Name { get; set; }

    public string? Comment { get; set; }

    public DateTime? InsertDate { get; set; }
}
