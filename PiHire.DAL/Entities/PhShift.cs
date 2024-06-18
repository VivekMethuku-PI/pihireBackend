using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhShift
{
    public int Id { get; set; }

    public string ShiftName { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public byte? Status { get; set; }
}
