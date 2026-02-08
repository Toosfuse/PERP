using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class ActivationLog
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? UserCode { get; set; }

    public string? Tel { get; set; }

    public string? Mail { get; set; }

    public int? CityId { get; set; }

    public string? Date { get; set; }

    public int? UserId { get; set; }

    public string? UserNameInsert { get; set; }
}
