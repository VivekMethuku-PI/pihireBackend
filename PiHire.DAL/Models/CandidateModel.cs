using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.DAL.Models
{  

    public class CandidateListModel
    {
        public int CandidateCount { get; set; }
        public List<CandidatesViewModel> CandidatesViewModel { get; set; }
    }
    public class JobCandidateListModel
    {
        public int CandidateCount { get; set; }
        public List<JobCandidatesViewModel> CandidatesViewModel { get; set; }
    }
    public class JobCandidatesViewModel
    {
        public int? JoId { get; set; }
        public int? CandProfID { get; set; }
        public short? SourceID { get; set; }
        public string EmailID { get; set; }
        public string CandName { get; set; }
        public string FullNameInPP { get; set; }
        public DateTime? DOB { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public int? CandProfStatus { get; set; }
        public string CandProfStatusName { get; set; }
        public string TagWords { get; set; }
        public int? StageID { get; set; }
        public string CsCode { get; set; }
        public string CurrOrganization { get; set; }
        public string CurrLocation { get; set; }
        public int? CurrLocationID { get; set; }
        public byte? NoticePeriod { get; set; }
        public int? CountryID { get; set; }
        public string CountryName { get; set; }
        public int? ReasonType { get; set; }
        public string ReasonsForReloc { get; set; }
        public string Nationality { get; set; }
        public string Experience { get; set; }
        public int? ExperienceInMonths { get; set; }
        public string RelevantExperience { get; set; }
        public int? ReleExpeInMonths { get; set; }
        public string ContactNo { get; set; }
        public string AlteContactNo { get; set; }
        public int? RecruiterId { get; set; }
        public string RecName { get; set; }
        public string CPCurrency { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public string EPCurrency { get; set; }
        public int? EPTakeHomePerMonth { get; set; }
        public string OpCurrency { get; set; }
        public int? OpTakeHomePerMonth { get; set; }
        public decimal? SelfRating { get; set; }
        public decimal? Evaluation { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string JobCategory { get; set; }
        public bool? TLReview { get; set; }
        public bool? MReview { get; set; }
    }
    public class CandidatesViewModel
    {
        public int? JoId { get; set; }
        public int? CandProfID { get; set; }
        public short? SourceID { get; set; }
        public string EmailID { get; set; }
        public string CandName { get; set; }
        public string FullNameInPP { get; set; }
        public DateTime? DOB { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; } 
        public byte? CandProfStatus { get; set; } 
        public string CandProfStatusName { get; set; } 
        public string TagWords { get; set; } 
        public int? StageID { get; set; }
        public int? CsCode { get; set; }
        public string CurrOrganization { get; set; }
        public string CurrLocation { get; set; }
        public int? CurrLocationID { get; set; }
        public byte? NoticePeriod { get; set; }
        public int? CountryID { get; set; }
        public string CountryName { get; set; }
        public int? ReasonType { get; set; }
        public string ReasonsForReloc { get; set; }
        public string Nationality { get; set; }
        public string Experience { get; set; }
        public int? ExperienceInMonths { get; set; }
        public string RelevantExperience { get; set; }
        public int? ReleExpeInMonths { get; set; }
        public string ContactNo { get; set; }
        public string AlteContactNo { get; set; }
        public int? RecruiterId { get; set; }
        public string RecName { get; set; }
        public string CPCurrency { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public string EPCurrency { get; set; }
        public int? EPTakeHomePerMonth { get; set; }
        public string OpCurrency { get; set; }
        public int? OpTakeHomePerMonth { get; set; }
        public decimal? SelfRating { get; set; }
        public decimal? Evaluation { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? PuId { get; set; }
        public string JobCategory { get; set; }
    }  
    public class CandidateCountModel
    {
        public int CandidateCount { get; set; }
    }
    public class JobCandidateListFilterDataModel
    {
        public int? CountryID { get; set; }
        public int? Nationality { get; set; }
        public int? CandProfStatus { get; set; }
        public string OpCurrency { get; set; }
    }
    public class SuitableCandidatesViewModel
    {
        public int? CandProfID { get; set; }     
        public string EmailID { get; set; }
        public string CandName { get; set; }
        public string FullNameInPP { get; set; }
    }
    public class _JobCandidatesBasedOnProfileStatusViewModel
    {
        public int CandidateCount { get; set; }
        public List<JobCandidatesBasedOnProfileStatusViewModel> CandidatesViewModel { get; set; }
    }
    public class JobCandidatesBasedOnProfileStatusViewModel
    {
        public int? JoId { get; set; }
        public int? CandProfID { get; set; }
        public short? SourceID { get; set; }
        public string EmailID { get; set; }
        public string CandName { get; set; }
        public string FullNameInPP { get; set; }
        public DateTime? DOB { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public int? CandProfStatus { get; set; }
        public string CandProfStatusName { get; set; }
        public string CandProfStatusAge { get; set; }
        public int? StageID { get; set; }
        public string CsCode { get; set; }
        public string CurrOrganization { get; set; }
        public string CurrLocation { get; set; }
        public int? CurrLocationID { get; set; }
        public byte? NoticePeriod { get; set; }
        public int? CountryID { get; set; }
        public string CountryName { get; set; }
        public int? ReasonType { get; set; }
        public string ReasonsForReloc { get; set; }
        public string Nationality { get; set; }
        public string Experience { get; set; }
        public int? ExperienceInMonths { get; set; }
        public string RelevantExperience { get; set; }
        public int? ReleExpeInMonths { get; set; }
        public string ContactNo { get; set; }
        public string AlteContactNo { get; set; }
        public int? RecruiterId { get; set; }
        public string RecName { get; set; }
        public string CPCurrency { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public string EPCurrency { get; set; }
        public int? EPTakeHomePerMonth { get; set; }
        public string OpCurrency { get; set; }
        public int? OpTakeHomePerMonth { get; set; }
        public decimal? SelfRating { get; set; }
        public decimal? Evaluation { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string JobCategory { get; set; }
        public bool? TLReview { get; set; }
        public bool? MReview { get; set; }
        public int ClientID { get; set; }
    }
}
