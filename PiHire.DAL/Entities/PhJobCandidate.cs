using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobCandidate
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int CandProfId { get; set; }

    public int? StageId { get; set; }

    public int? CandProfStatus { get; set; }

    public bool EmailSentFlag { get; set; }

    public int? RecruiterId { get; set; }

    public DateTime? ProfReceDate { get; set; }

    public bool? OpconfirmFlag { get; set; }

    public DateTime? OpconfirmDate { get; set; }

    public string Opcurrency { get; set; }

    public int? OpgrossPayPerAnnum { get; set; }

    public int? OpdeductionsPerAnnum { get; set; }

    public int? OpvarPayPerAnnum { get; set; }

    public int? OpnetPayPerAnnum { get; set; }

    public int? OptakeHomePerMonth { get; set; }

    public int? OpgrossPayPerMonth { get; set; }

    public string Epcurrency { get; set; }

    public int? EpgrossPayPerAnnum { get; set; }

    public int? EpdeductionsPerAnnum { get; set; }

    public int? EptakeHomePerMonth { get; set; }

    public bool ProfileUpdateFlag { get; set; }

    public int Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public double SelfRating { get; set; }

    public int? SalaryVerifiedby { get; set; }

    public string PayslipCurrency { get; set; }

    public int? PayslipSalary { get; set; }

    public bool? IsPayslipVerified { get; set; }

    public string Cbcurrency { get; set; }

    public int? CbperMonth { get; set; }

    public bool BgvacceptedFlag { get; set; }

    public string Bgvcomments { get; set; }

    public bool? IsTagged { get; set; }

    public bool? Mreview { get; set; }

    public bool? Tlreview { get; set; }

    public bool? L1review { get; set; }



    public bool? CandidatePrefRegion { get; set; }
    public int? CandidatePrefRegionId { get; set; }
    public bool? JobCountryDrivingLicence { get; set; }
    public bool? JobDesirableDomain { get; set; }
    public int? JobDesirableDomainId { get; set; }
    public bool? JobDesirableCategory { get; set; }
    public int? JobDesirableCategoryId { get; set; }
    public bool? JobDesirableTenure { get; set; }
    public int? JobDesirableTenureId { get; set; }
    public bool? JobDesirableWorkPattern { get; set; }
    public int? JobDesirableWorkPatternId { get; set; }
    public bool? JobDesirableTeamRole { get; set; }
    public int? JobDesirableTeamRoleId { get; set; }
    public bool? CandidatePrefLanguage { get; set; }
    public int? CandidatePrefLanguageId { get; set; }
    public bool? CandidatePrefVisaPreference { get; set; }
    public int? CandidatePrefVisaPreferenceId { get; set; }
    public int? CandidatePrefEmployeeStatus { get; set; }
    public bool? CandidateResignationAccepted { get; set; }
    public DateTime? CandidateLastWorkDate { get; set; }
    public bool? AnyOfferInHand { get; set; }
    public DateTime? CandidateCanJoinDate { get; set; }

    public bool? InterviewFaceToFace { get; set; }
    public string InterviewFaceToFaceReason { get; set; }
}
