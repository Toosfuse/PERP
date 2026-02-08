using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class City
{
    public int CityId { get; set; }

    public string? CityName { get; set; }

    public string? CityProfileName { get; set; }

    public string? CityNameEn { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
