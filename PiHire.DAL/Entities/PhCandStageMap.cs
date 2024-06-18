using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandStageMap
{
    public short Id { get; set; }

    public int StageId { get; set; }

    public int CandStatusId { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
