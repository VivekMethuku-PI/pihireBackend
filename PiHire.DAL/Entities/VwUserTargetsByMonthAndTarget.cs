using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwUserTargetsByMonthAndTarget
{
    public int UserId { get; set; }

    public int? TargetQtySet { get; set; }

    public DateTime? MonthYear { get; set; }

    public string TargetValue { get; set; }

    public string TargetDescription { get; set; }
}
