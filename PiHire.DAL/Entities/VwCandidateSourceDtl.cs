using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwCandidateSourceDtl
{
    public int CandProfId { get; set; }

    public int CandJobId { get; set; }

    public string CandName { get; set; }

    public int JoId { get; set; }

    public int ClientId { get; set; }

    public string ClientName { get; set; }

    public int? PuId { get; set; }

    public string JobTitle { get; set; }

    public bool? IsTagged { get; set; }

    public short? SourceId { get; set; }

    public string EmailId { get; set; }

    public string MobileNumber { get; set; }

    public string CandProfStatus { get; set; }

    public string StatusCode { get; set; }

    public int? RecruiterId { get; set; }

    public string RecruiterProfilePhoto { get; set; }

    public string RecruiterName { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? BroughtBy { get; set; }

    public string BroughtByName { get; set; }

    public string BroughtByPhoto { get; set; }

    public string CandProfilePhoto { get; set; }
}
