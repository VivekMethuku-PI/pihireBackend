using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobCandidateEvaluation
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int CandProfId { get; set; }

    public byte RatingType { get; set; }

    public byte Rating { get; set; }

    public string Remakrs { get; set; }

    public int? RefId { get; set; }

    public int Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
