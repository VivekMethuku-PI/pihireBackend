using PiHire.BAL.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class ActorViewModel
    {
        public int UserId { get; set; }
        public string ToEmail { get; set; }
        public string ccEmail { get; set; }
        public string Name { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string JobDesc { get; set; }
        public int? MaxExpeInMonths { get; set; }
        public int? MinExpeInMonths { get; set; }
        public string JobStatus { get; set; }
        public string JobLocation { get; set; }
        public string JobCountry { get; set; }
        public DateTime? JobPostedOn { get; set; }
        public DateTime? JobEndDate { get; set; }
        public string JobCurrencyName { get; set; }
        public int? BroughtBy { get; set; }
        public string BdmName { get; set; }
        public int NoCvs { get; set; }

        public string ClientName { get; set; }
        public string SpocName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientContactNo { get; set; }

        public int? OfferGrossPackagePerMonth { get; set; }
        public int? OfferNetSalMonth { get; set; }
        public string OfferPackageCurrency { get; set; }
        public string HireManagerName { get; set; }

        public int CandId { get; set; }
        public string CandName { get; set; }
        public string CandEmail { get; set; }
        public string CandContactNo { get; set; }
        public string CandStatus { get; set; }
        public string InterviewDate { get; set; }
        public string InterviewEndTime { get; set; }
        public string InterviewStartTime { get; set; }
        public string InterviewTimeZone { get; set; }
        public string InterviewLoc { get; set; }
        public string InterviewMode { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string RequestDocuments { get; set; }

        public string Signature { get; set; }
        public int? RecruiterId { get; set; }
        public string RecName { get; set; }
        public string RecPosition { get; set; }
        public string RecEmailId { get; set; }
        public string RecPhoneNum { get; set; }
        public int AssignNoCvs { get; set; }
        public List<GetOpeningSkillSetViewModel> JobSkills { get; set; }

    }
}
