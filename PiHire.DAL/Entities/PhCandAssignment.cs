using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandAssignment
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int AssignedTo { get; set; }

    public DateTime? DeassignDate { get; set; }

    public int? DeassignBy { get; set; }

    public short? NoCvsrequired { get; set; }

    public short? ProfilesUploaded { get; set; }

    public short? ProfilesRejected { get; set; }

    public byte Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
