using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandidateStatusLog
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int CanProfId { get; set; }

    public string CandProfStatus { get; set; }

    public string ActivityDesc { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
