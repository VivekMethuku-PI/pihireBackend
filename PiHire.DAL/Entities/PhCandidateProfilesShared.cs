using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandidateProfilesShared
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int CandProfId { get; set; }

    public string BatchNo { get; set; }

    public int ClientId { get; set; }

    public string Clname { get; set; }

    public string ClemailId { get; set; }

    public DateTime? ReviewedOn { get; set; }

    public byte ClreviewStatus { get; set; }

    public byte Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string Reasons { get; set; }

    public string Remarks { get; set; }

    public string EmailSubject { get; set; }

    public string CcemailIds { get; set; }

    public string ConfView { get; set; }

    public string EmailFields { get; set; }

    public byte ModeOfInterview { get; set; }

    public DateTime? InterviewDate { get; set; }

    public string StartTime { get; set; }

    public string EndTime { get; set; }

    public bool ShortListFlag { get; set; }

    public string InterviewTimeZone { get; set; }
}
