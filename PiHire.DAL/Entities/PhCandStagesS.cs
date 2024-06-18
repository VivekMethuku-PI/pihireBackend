using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandStagesS
{
    public short Id { get; set; }

    public string Title { get; set; }

    public string StageDesc { get; set; }

    public string ColorCode { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
