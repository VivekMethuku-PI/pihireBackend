using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobAssignmentsDayWiseLog
{
    public long Id { get; set; }

    public int JobAssignmentDayWiseId { get; set; }

    public short LogType { get; set; }

    public short? NoCvsrequired { get; set; }

    public byte Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }
}
