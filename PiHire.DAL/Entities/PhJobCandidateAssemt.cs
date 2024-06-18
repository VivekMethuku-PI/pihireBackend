using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobCandidateAssemt
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int JoassmtId { get; set; }

    public int CandProfId { get; set; }

    public string AssmtId { get; set; }

    public byte? ResponseStatus { get; set; }

    public DateTime? ResponseDate { get; set; }

    public byte Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string ResponseUrl { get; set; }

    public string DistributionId { get; set; }

    public string ContactId { get; set; }
}
