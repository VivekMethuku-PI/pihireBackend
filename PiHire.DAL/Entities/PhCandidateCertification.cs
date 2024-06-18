using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandidateCertification
{
    public int Id { get; set; }

    public int CandProfId { get; set; }

    public int CertificationId { get; set; }

    public byte Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
