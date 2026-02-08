using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class ReturnedMeter
{
    public long Id { get; set; }

    public string ReturnedNumber { get; set; } = null!;

    public string ReceivedMeter { get; set; } = null!;

    public short CityId { get; set; }

    public string Customer { get; set; } = null!;

    public short StatuesId { get; set; }

    public string? SimcardNumber { get; set; }

    public string Report { get; set; } = null!;

    public string? SentMeter { get; set; }

    public string? SentDate { get; set; }

    public string? SentBy { get; set; }

    public short? MajorProblemId { get; set; }

    public short? MinorProblemId { get; set; }

    public string? Solution { get; set; }

    public short? ResponsibleId { get; set; }

    public string? Comment { get; set; }

    public DateTime? InsertDate { get; set; }

    public int? OperatorId { get; set; }

    public DateTime? UpdateDate { get; set; }
}
