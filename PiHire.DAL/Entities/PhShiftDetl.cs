using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhShiftDetl
{
    public int Id { get; set; }

    public string DayName { get; set; }

    public bool? IsWeekend { get; set; }

    public int? From { get; set; }

    public string FromMeridiem { get; set; }

    public int? To { get; set; }

    public string ToMeridiem { get; set; }

    public int ShiftId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public byte? Status { get; set; }

    public bool? IsAlternateWeekend { get; set; }

    public bool? AlternativeStart { get; set; }

    public DateTime? AlternativeWeekStartDate { get; set; }

    public int? FromMinutes { get; set; }

    public int? ToMinutes { get; set; }

    public int? WeekendModel { get; set; }
}
