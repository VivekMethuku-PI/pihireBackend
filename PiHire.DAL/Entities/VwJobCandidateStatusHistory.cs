using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwJobCandidateStatusHistory
{
    public int JobId { get; set; }

    public string JobTitle { get; set; }

    public int ClientId { get; set; }

    public string ClientName { get; set; }

    public int JobOpeningStatus { get; set; }

    public int? CurrentStatusId { get; set; }

    public int? UpdateStatusId { get; set; }

    public int? CandProfId { get; set; }

    public string CandName { get; set; }

    public string StatusCode { get; set; }

    public DateTime? ActivityDate { get; set; }

    public int? Puid { get; set; }

    public int? Buid { get; set; }

    public int? RecruiterId { get; set; }

    public int? OpgrossPayPerMonth { get; set; }

    public int? BroughtBy { get; set; }

    public DateTime CreatedDate { get; set; }
}
