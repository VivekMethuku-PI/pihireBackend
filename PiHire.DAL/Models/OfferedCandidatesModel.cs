using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class OfferedCandidatesModel
    {
        public List<OfferdCandidateList> OfferdCandidateList { get; set; }
        public int CandCount { get; set; }
    }
    public class OfferdCandidateCount
    {
        public int CandCount { get; set; }
    }
    public class OfferdCandidateList
    {           
        public int JoId { get; set; }
        public int CandProfID { get; set; }
        public string EmailID { get; set; }
        public string CandName { get; set; }
        public int CandProfStatus { get; set; }
        public string CandProfStatusName { get; set; }
        public int ClientID { get; set; }
        public string ClientName { get; set; }       
        public string JobName { get; set; } 
        public string CsCode { get; set; }
        public string CurrLocation { get; set; }
        public int? CountryID { get; set; }
        public string CountryName { get; set; }
        public string Nationality { get; set; }
        public int? TotalExp { get; set; }
        public int? ReleExp { get; set; }
        public string ContactNo { get; set; }
        public string AlteContactNo { get; set; }      
        public int? RecruiterId { get; set; }
        public string RecruiterName { get; set; }
        public string CPCurrency { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public int? EPTakeHomePerMonth { get; set; }
        public string OpCurrency { get; set; }
        public int? OpTakeHomePerMonth { get; set; }
        public decimal? SelfRating { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string PassportNumber { get; set; }
        public DateTime ApporxJoining { get; set; }
        public string RecProfilePhoto { get; set; }
        public int? OfferId { get; set; }

        public bool? IsOdooSync { get; set; }
        public bool? IsGatewaySync { get; set; }
    }
}
