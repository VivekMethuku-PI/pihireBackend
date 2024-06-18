using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{

    public class BDMsVM
    {
        public List<BDMsOverviewModel> BDMsOverviewModel { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class BDMsOverviewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string ProfilePhoto { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
        public int? TotalJobCount { get; set; }
        public int? AssociatedJobcount { get; set; }
        public int? FinalCVCount { get; set; }
        public int? PiRejectCount { get; set; }
        public int? ClntSubmisionCount { get; set; }
        public int? InterviewsCount { get; set; }
        public int? ClntReject { get; set; }
        public int? ClntSelectionCount { get; set; }
        public int? PreJoinCount { get; set; }
        public int? JoinedCount { get; set; }
        public int? JoingBackOutCount { get; set; }
        public int? CandidateBackoutCount { get; set; }
        public int? DeclinedOfferCount { get; set; }
        public int? NoteCount { get; set; }
    }

    public class BDMOverviewModel
    {
        public int JobId { get; set; }
        public int CandProfId { get; set; }
        public string CandName { get; set; }
        public string CandProfilePhoto { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string StatusCode { get; set; }
        public DateTime ActivityDate { get; set; }
        public string JobTitle { get; set; }
        public string JobStatus { get; set; }
        public int? ClientID { get; set; }
        public string ClientName { get; set; }
        public int? BroughtBy { get; set; }
        public string BroughtByName { get; set; }
        public string BroughtbyProfilePhoto { get; set; }
        public int? RecruiterId { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterProfilePhoto { get; set; }
        public int? CurrentStatusId { get; set; }
        public string CurrentStatusName { get; set; }
        public int? UpdateStatusId { get; set; }
        public string UpdateStatuName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string AgeBetweenDates { get; set; }
    }

    public class BDMOpeningOverviewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string JobStatus { get; set; }
        public int? ClientID { get; set; }
        public string ClientName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? BdmId { get; set; }
        public string BdmName { get; set; }
        public string BdmProfilePhoto { get; set; }
        public string AgeBetweenDates { get; set; }

    }
}
