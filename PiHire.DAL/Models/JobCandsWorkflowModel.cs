using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class JobCandsWorkflowModel
    {
        public int CandUserId { get; set; }
        public string EmailId { get; set; }
        public string CandName { get; set; }
        public string RecEmail { get; set; }
    }

    public class CandDtlsWorkflowModel
    {
        public string ContactNo { get; set; }
        public string EmailId { get; set; }
        public string CandName { get; set; }
        public string CandStatus { get; set; }
        public int? CandProfStatus { get; set; }
        public int? OfferGrossPackagePerMonth { get; set; }
        public string OfferPackageCurrency { get; set; }
        public int? OfferNetSalMonth { get; set; }
        public int? RecruiterId { get; set; }
        public int UserId { get; set; }
        public string ToEmail { get; set; }
        public byte? ModeOfInterview { get; set; }
        public DateTime? InterviewDate { get; set; }
        public string InterviewLoc { get; set; }
        public string InterviewStartTime { get; set; }
        public string InterviewEndTime { get; set; }
        public string InterviewTimeZone { get; set; }
        public DateTime? JoiningDate { get; set; }
    }



    public class JobSkillsWorkflowModel
    {
        public int? ExpMonth { get; set; }
        public int? ExpYears { get; set; }
        public string Technology { get; set; }
        public int? TechnologyId { get; set; }
    }


    public class JobDtlsWorkflowModel
    {
        public string JobDesc { get; set; }
        public string JobTitle { get; set; }
        public DateTime JobEndDate { get; set; }
        public DateTime JobPostedOn { get; set; }
        public int? BroughtBy { get; set; }
        public string BroughtByName { get; set; }
        public int? MaxExpeInMonths { get; set; }
        public int? MinExpeInMonths { get; set; }
        public string JobCountry { get; set; }
        public string JobLocation { get; set; }
        public int JobOpeningStatus { get; set; }
        public string JobOpeningStatusName { get; set; }
        public string JobCurrencyName { get; set; }
        public int? SPOCID { get; set; }
        public int? ClientID { get; set; }
        public string ClientName { get; set; }
        public int? PuId { get; set; }
    }



}
