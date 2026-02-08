using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CityId { get; set; }

    public DateTime? RegDate { get; set; }

    public string? OrderNumber { get; set; }

    public string? OrderRegNumber { get; set; }

    public string? StartSerial { get; set; }

    public string? EndSerial { get; set; }

    public DateTime? RegisterTime { get; set; }

    public int? OperatorId { get; set; }

    public string? CustomerName { get; set; }

    public virtual City? City { get; set; }
    public string? CodeProduct { get; set; } // نام فیلد دلخواه شما 
    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();

    public virtual ICollection<PackagesOld> PackagesOlds { get; set; } = new List<PackagesOld>();
}
