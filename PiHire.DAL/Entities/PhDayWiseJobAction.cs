using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhDayWiseJobAction
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool? Assign { get; set; }

    public bool? Priority { get; set; }

    public bool? Note { get; set; }

    public bool? Interviews { get; set; }

    public bool? JobStatus { get; set; }

    public bool? CandStatus { get; set; }
}
