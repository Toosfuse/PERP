using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class VerifiedProfileByQc
{
    public int Id { get; set; }

    public string MeterType { get; set; } = null!;

    public int CityId { get; set; }

    public string Date { get; set; } = null!;

    public int? UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Profile { get; set; } = null!;
}
