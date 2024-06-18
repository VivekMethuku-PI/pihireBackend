using System;
using System.Collections.Generic;

namespace PiHire.DAL.Models
{
    public class DashboardCountModel
    {
        public int TotCnt { get; set; }
    }

    public class InterviewStageStatus
    {
        public int TotCnt { get; set; }
        public int tabNo { get; set; }
    }

    public class DashboardCandiateInterviewModel
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int CandProfId { get; set; }

        public byte InterviewStatus { get; set; }
        public byte ModeOfInterview { get; set; }
        public string InterviewStartTime { get; set; }
        public string InterviewEndTime { get; set; }
        public DateTime InterviewDate { get; set; }

        public byte? NoticePeriod { get; set; }
        public int ClientID { get; set; }
        public string ClientName { get; set; } 
        public string JobTitle { get; set; }
        public string CandName { get; set; }      
        public string EmailID { get; set; }      
        public string ContactNo { get; set; }

        public string bdmName { get; set; }
        public string bdmPhoto { get; set; }
        public string recrName { get; set; }
        public string recrPhoto { get; set; }
        public int? bdmId { get; set; }
        public int? recruiterID { get; set; }
        public int tabNo { get; set; }
        public int candProfStatus { get; set; }
        public string candProfStatusCode { get; set; }
        public string CandProfStatusName { get; set; }
        public string finalCVUrl { get; set; }

        public string CityName { get; set; }
        public string Experience { get; set; }
        public int? ExperienceInMonths { get; set; }
        public string TimeLine { get; set; }
    }

    public class DashboardJobStageModel
    {
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public int JobId { get; set; }
        public int? JobOpeningStatus { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public int? NoOfCvsRequired { get; set; }
        public int? NoOfCvsFullfilled { get; set; }
        public DateTime? ReopenedDate { get; set; }
        public int? bdmId { get; set; }
        public int? jobCityId { get; set; }
        public int? jobCountryId { get; set; }
    }
    public class DashboardJobRecruiterStageModel
    {
        public int JobId { get; set; }
        public int? bdmId { get; set; }
        public int recruiterID { get; set; }
        public DateTime PostedDate { get; set; }
        public string recrName { get; set; }
    }
    public class DashboardRecruiterStatusModel
    {
        public int jobId { get; set; }
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public int? bdmId { get; set; }
        public int? recruiterID { get; set; }
        public short? NoCVSRequired { get; set; }
        public int? NoOfFinalCVsFilled { get; set; }
        public int? jobCityId { get; set; }
        public int? jobCountryId { get; set; }
    }
    public class Sp_Dashboard_Daywise_FilterModel
    {
        public int jobId { get; set; }
        public int? bdmId { get; set; }
        public int recruiterID { get; set; }
        public short? NoCVSRequired { get; set; }
        public short? NoCVSUploadded { get; set; }
        public short? NoOfFinalCVsFilled { get; set; }
        public DateTime? jobAssignmentDate { get; set; }
    }
    public class GetDashboardHireAdminModel
    {
        public int? activeJobCount { get; set; }
        public int? holdJobCount { get; set; }
        public int? newJobCount { get; set; }
        public int? reopenJobCount { get; set; }
        public int? morecvsJobCount { get; set; }
        public int? submittedJobsFilled { get; set; }
        public int? submittedJobsRequired { get; set; }
        public int? highlightTrgtPeriodFilled { get; set; }
        public int? highlightTrgtPeriodRequired { get; set; }
        public int? highlightYetToJoin { get; set; }
        public int? highlightJoined { get; set; }
    }
    public class Sp_Similar_JobsModel
    {
        public int JobId { get; set; }
    }
    public class GetDashboardBdmModel
    {
        public int? ReqFinalCvsCount { get; set; }
        public int? FinalCvsCount { get; set; }
        public int? CvSubmissionCount { get; set; }
        public int? InterviewCount { get; set; }
        public int? ResultDueCount { get; set; }
        public int? pf2fCount { get; set; }
        public int? highlightTrgtPeriodFilled { get; set; }
        public int? highlightTrgtPeriodRequired { get; set; }
        public int? highlightYetToJoin { get; set; }
        public int? highlightJoined { get; set; }
        public int? newJobCount { get; set; }
        public int? closedJobCount { get; set; }
    }
    public class GetDashboardRecruiterModel
    {
        public int? highlightTrgtPeriodFilled { get; set; }
        public int? highlightTrgtPeriodRequired { get; set; }
        public int? highlightYetToJoin { get; set; }
        public int? highlightJoined { get; set; }
        public int? AssignedCount { get; set; }
        public int? SourcedCount { get; set; }
        public int? TaggedCount { get; set; }
        public int? ReqFinalCvsCount { get; set; }
        public int? FinalCvsCount { get; set; }
        public int? CvSubmissionCount { get; set; }
        public int? InterviewCount { get; set; }
        public int? AccountRejectedCount { get; set; }
        public int? InterviewBackoutCount { get; set; }
        public int? JoinBackoutCount { get; set; }
    }
    public class GetDashboardRecruiterJobCategoryModel
    {
        public int JobId { get; set; }
        public string JobCategory { get; set; }
        public DateTime? ClosedDate { get; set; }
        public int? resumeCount { get; set; }
    }
    public class GetDashboardRecruiterCandidateModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string ClientName { get; set; }
        public int? CandProfId { get; set; }
        public string CandName { get; set; }
        public string StatusCode { get; set; }
        public DateTime? ActivityDate { get; set; }
    }

    public class GetDashboardRecruiterAnalyticModel
    {
        public int? workingJobCount { get; set; }
        public int? closedJobCount { get; set; }
        public int? totalCandidateCount { get; set; }
        public int? hiredCandidateCount { get; set; }
        public int? rejectedCandidateCount { get; set; }
        public int? recruitmentApplicantsCount { get; set; }
        public int? recruitmentPreScreenedCount { get; set; }
        public int? recruitmentInterviewedCount { get; set; }
        public int? recruitmentHiredCount { get; set; }
        public int? offerAcceptedCount { get; set; }
        public int? offerProvidedCount { get; set; }
        public int? recruiterRejectCount { get; set; }
        public int? clientRejectCount { get; set; }
        public int? candidateRejectCount { get; set; }
    }
    public class GetDashboardRecruiterAnalyticGraphModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string StatusCode { get; set; }
        public int? OPGrossPayPerMonth { get; set; }
        public int? CandProfId { get; set; }
    }

    public class CandidateStatusBasedViewModel
    {
        public int ID { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public int CandidateId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public string Experience { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterPhoto { get; set; }
        public string BdmName { get; set; }
        public string BdmPhoto { get; set; }
        public int CandProfStatus { get; set; }
        public string CandProfStatusCode { get; set; }
        public string FinalCVUrl { get; set; }
        public string CityName { get; set; }


    }
}
