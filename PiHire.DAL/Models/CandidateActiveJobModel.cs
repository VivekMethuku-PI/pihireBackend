using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class CandidateActiveJobModel
    {
        public int? JobId {get;set;}
        public int? ClientId{get;set;}
        public string ClientName{get;set;}
        public string JobTitle {get;set;}
        public string JobDescription {get;set;}
        public int JobOpeningStatus { get; set; }
        public string JobRole {get;set;}
        public DateTime? PostedDate{get;set;}
        public DateTime? ClosedDate {get;set;}       
        public int? MinExp { get; set; }
        public int? MaxExp { get; set; }
        public bool? OPConfirmFlag { get; set; }
        public int? EPTakeHomePerMonth { get; set; }
        public string EPCurrency { get; set; }
        public string OPCurrency { get; set; }
        public int? OPGrossPayPerMonth { get; set; }
        public string JobCurrency { get; set; }
        public int? OPGrossPayPerAnnum { get; set; }
        public int? OPDeductionsPerAnnum { get; set; }
        public int? OPVarPayPerAnnum { get; set; }
        public int? OPNetPayPerAnnum { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string JobCityName { get; set; }
        public string CandidateCity { get; set; }
        public string CandidateCountry { get; set; }
        public string JobCountryName { get; set; }
        public int? JobCountryId { get; set; }
        public int?  CandProfStatus { get; set; }
        public string CPCurrency { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        public string ShortJobDesc { get; set; }

        public int? RecruiterId { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterMobile { get; set; }
        public string RecuiterRole { get; set; }
        public string RecuiterPhoto { get; set; }
        public string RecruiterEmail { get; set; }
        public DateTime? AppliedDate { get; set; }
    }


}
