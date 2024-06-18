using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwDashboardCandidateInterview
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public int CandProfId { get; set; }

    public byte InterviewStatus { get; set; }

    public byte ModeOfInterview { get; set; }

    public string InterviewStartTime { get; set; }

    public string InterviewEndTime { get; set; }

    public int ClientId { get; set; }

    public string ClientName { get; set; }

    public string JobTitle { get; set; }

    public DateTime? ReopenedDate { get; set; }

    public string CandName { get; set; }

    public string EmailId { get; set; }

    public string ContactNo { get; set; }

    public byte? NoticePeriod { get; set; }

    public string BdmName { get; set; }

    public string BdmPhoto { get; set; }

    public string RecrName { get; set; }

    public string RecrPhoto { get; set; }

    public DateTime InterviewDate { get; set; }

    public int? BdmId { get; set; }

    public int? RecruiterId { get; set; }

    public int TabNo { get; set; }

    public string CityName { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CandProfStatusCode { get; set; }

    public int? CandProfStatus { get; set; }

    public string Experience { get; set; }
}
