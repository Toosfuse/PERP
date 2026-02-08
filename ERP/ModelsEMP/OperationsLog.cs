using System;
using System.Collections.Generic;

namespace ERP.ModelsEMP;

public partial class OperationsLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public DateTime? OperationTime { get; set; }

    public int? OperationId { get; set; }

    public int? OperationResultId { get; set; }

    public string? OperationDetails { get; set; }

    public virtual AppType? Operation { get; set; }
}
