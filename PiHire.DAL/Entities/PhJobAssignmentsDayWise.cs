using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobAssignmentsDayWise
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int AssignedTo { get; set; }

    public short? NoCvsrequired { get; set; }

    public short? NoOfFinalCvsFilled { get; set; }

    public short? NoCvsuploadded { get; set; }

    public int? AssignBy { get; set; }

    public byte Status { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? AssignmentDate { get; set; }
}
