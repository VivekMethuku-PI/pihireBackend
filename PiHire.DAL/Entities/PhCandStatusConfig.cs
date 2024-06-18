using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandStatusConfig
{
    public short Id { get; set; }

    public int StatusId { get; set; }

    public int NextStatusId { get; set; }

    public byte DispOrder { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
