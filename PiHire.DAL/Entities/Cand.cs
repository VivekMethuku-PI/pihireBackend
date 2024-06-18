using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class Cand
{
    public int Id { get; set; }

    public int JoId { get; set; }

    public int? RecruiterId { get; set; }

    public int? StageId { get; set; }

    public string Epcurrency { get; set; }

    public int CandProfId { get; set; }

    public long? RowNumber { get; set; }

    public int? EptakeHomePerMonth { get; set; }

    public string OpCurrency { get; set; }

    public int? OpgrossPayPerMonth { get; set; }

    public int? CandProfStatus { get; set; }

    public int? PuId { get; set; }

    public string JobCategory { get; set; }
}
