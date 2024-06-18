using Microsoft.AspNetCore.Http;
using PiHire.BAL.Common.Attribute;
using PiHire.BAL.Common.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.ViewModels
{
    public class CreateCandidateViewModel
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }
        [Required]
        [MaxLength(100)]
        public string EmailAddress { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public byte SourceId { get; set; }
        [MaxLength(500)]
        public string Remarks { get; set; }

        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg" })]
        public IFormFile Photo { get; set; }

        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".doc", ".docx", ".pdf" })]
        public IFormFile Resume { get; set; }
        [DataType(DataType.Upload)]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".pdf", ".jpg", ".png", ".gif", ".jpeg" })]
        public List<IFormFile> PaySlips { get; set; }
        [DataType(DataType.Upload)]
        [MaxFileSize((byte)FileType.Video)]
        public IFormFile CandVideoProfile { get; set; }
        public Nullable<bool> ValidPpflag { get; set; }
        [MaxLength(100)]
        public string FullNameInPp { get; set; }
        public DateTime? CandidateDOB { get; set; }
        public Nullable<bool> CurrEmplFlag { get; set; }
        public int? CurrDesignation { get; set; }
        //[MaxLength(1)]
        public int? Gender { get; set; }
        //[MaxLength(1)]
        public int? MaritalStatus { get; set; }
        public byte NoticePeriod { get; set; }
        public string ReasonsForReloc { get; set; }
        [MaxLength(200)]
        public string Address { get; set; }
        [MaxLength(100)]
        public string CurrLocation { get; set; }
        public int? CurrLocationID { get; set; }
        public int? CountryID { get; set; }
        [MaxLength(100)]
        public string CurrOrganization { get; set; }
        public string TotalExperiance { get; set; }
        public string RelevantExperiance { get; set; }
        [MaxLength(20)]
        public string ContactNo { get; set; }
        [MaxLength(20)]
        public string AlteContactNo { get; set; }
        [MaxLength(5)]
        public string CPCurrency { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public int? CPDeductionsPerAnnum { get; set; }
        public int? CPVariablePayPerAnnum { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        public string EPCurrency { get; set; }
        public int? EPTakeHomeSalPerMonth { get; set; }
        public int? Nationality { get; set; }
        public List<PaySlipsModel> PaySlipsModel { get; set; }
        public string ResumeURL { get; set; }
        public string ResumeURLType { get; set; }
        public List<CreateCandidateSkillViewModel> CreateCandidateSkillViewModel { get; set; }
        public List<CandidateQualificationModel> CandidateQualificationModel { get; set; }
        public List<CandidateCertificationModel> CandidateCertificationModel { get; set; }
        public List<CandidateQuestionResponseModel> CandidateQuestionResponseModel { get; set; }
    }

    public class CandidateQualificationModel
    {
        public int Id { get; set; }
        [Required]
        public int QualificationId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Qualification { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Course { get; set; }
    }

    public class CandidateCertificationModel
    {
        public int Id { get; set; }
        [Required]
        public int CertificationID { get; set; }
    }

    public class CreateCandidateCertificationModel
    {

        [Required]
        public int CertificationID { get; set; }
        [Required]
        public int CandProfId { get; set; }
    }

    public class CandidateQuestionResponseModel
    {
        public int Id { get; set; }
        [Required]
        public int QuestionId { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Response { get; set; }
    }

    public class PaySlipsModel
    {
        public string Type { get; set; }
        public string URL { get; set; }
    }

    public class CreateCandidateSkillViewModel
    {
        public int SkillLevelId { get; set; }
        public string SkillLevel { get; set; }
        public int TechnologyId { get; set; }
        public int ExpInYears { get; set; }
        public int ExpInMonths { get; set; }
        public byte SelfRating { get; set; }
        public bool IsCanSkill { get; set; }
    }
    #region Candidate portal
    public class GetJobCandidatePortalViewModel
    {
        public int JobId { get; set; }
        public int? SourceId { get; set; }

        public int CandProfId { get; set; }

        public string EmailAddress { get; set; }
        public string FullName { get; set; }
        public int? Gender { get; set; }
        public int? MaritalStatus { get; set; }
        public string ContactNo { get; set; }
        public string AlteContactNo { get; set; }
        public DateTime? CandidateDOB { get; set; }

        public int? CountryID { get; set; }
        public int? CurrLocationID { get; set; }
        public string CurrLocation { get; set; }
        public int? Nationality { get; set; }
        public string CurrOrganization { get; set; }
        public int? CurrDesignation { get; set; }

        public string CPCurrency { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public int? CPDeductionsPerAnnum { get; set; }
        public int? CPVariablePayPerAnnum { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }

        public int? CurrentPackage { get; set; }

        public bool? CurrEmplFlag { get; set; }
        public byte? NoticePeriod { get; set; }
        public string ReasonsForReloc { get; set; }

        public string TotalExperiance { get; set; }
        public string RelevantExperiance { get; set; }

        public bool? ValidPpflag { get; set; }//ValidPassport

        public string EPCurrency { get; set; }
        public int? EPTakeHomeSalPerMonth { get; set; }

        public int? ExpectedPackage { get; set; }
        public double SelfRating { get; set; }
        public int? CandPrfStatus { get; set; }
        public int? StageId { get; set; }

        public UpdateJobCandidatePortalViewModel_value<int?> CandidatePrefRegion { get; set; }
        public bool? JobCountryDrivingLicence { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> JobDesirableDomain { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> JobDesirableCategory { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> JobDesirableTenure { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> JobDesirableWorkPattern { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> JobDesirableTeamRole { get; set; }

        public UpdateJobCandidatePortalViewModel_value<int?> CandidatePrefLanguage { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> CandidatePrefVisaPreference { get; set; }

        public CandidateEmployeeStatus? CandidatePrefEmployeeStatus { get; set; }
        public bool? CandidateResignationAccepted { get; set; }

        public DateTime? CandidateLastWorkDate { get; set; }

        public bool? AnyOfferInHand { get; set; }
        public DateTime? CandidateCanJoinDate { get; set; }

        public List<UpdateJobCandidatePortalViewModel_skillRating> CandidateSkills { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableSpecializations { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableImplementations { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableDesigns { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableDevelopments { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableSupports { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableQualities { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableDocumentations { get; set; }


        public List<GetJobCandidatePortalViewModel_CandidateEducation> CandidateQualificationModel { get; set; }//CandidateEducationQualifications
        public List<UpdateJobCandidatePortalViewModel_CandidateCertification> CandidateCertificationModel { get; set; }//CandidateEducationCertifications
        public List<UpdateJobCandidatePortalViewModel_OpeningEducation> OpeningQualifications { get; set; }
        public List<UpdateJobCandidatePortalViewModel_CandidateCertification> OpeningCertifications { get; set; }
        //public string Remarks { get; set; }
        //public int? ReasonType { get; set; }

        //public string Photo { get; set; }
        //public List<GetCandQuestionResponse> QuestionResponse { get; set; }

        public List<string> PaySlipUrls { get; set; }
        public string ResumeUrl { get; set; }
        public string CandVideoProfileUrl { get; set; }

        public bool? InterviewFaceToFace { get; set; }
        public string InterviewFaceToFaceReason { get; set; }
    }
    public class GetJobCandidatePortalViewModel_CandidateEducation
    {
        public int QualificationId { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> Qualification { get; set; }
        public int? CourseId { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> Course { get; set; }
    }
    public class UpdateJobCandidatePortalViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int JobId { get; set; }
        public int SourceId { get; set; }

        public bool? ValidPpflag { get; set; }//ValidPassport

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }
        public DateTime? CandidateDOB { get; set; }//DateofBirth
        [MaxLength(20)]
        public string ContactNo { get; set; }
        [MaxLength(20)]
        public string AlteContactNo { get; set; }
        [Required]
        public int Gender { get; set; }
        [Required]
        public int MaritalStatus { get; set; }

        public UpdateJobCandidatePortalViewModel_value<int?> CandidatePrefRegion { get; set; }
        public int? Nationality { get; set; }
        public int? CountryID { get; set; }//ResidingCountry
        public int? CurrLocationID { get; set; }//ResidingCity
        [MaxLength(100)]
        public string CurrLocation { get; set; }

        public bool? JobCountryDrivingLicence { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> JobDesirableDomain { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> JobDesirableCategory { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> JobDesirableTenure { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> JobDesirableWorkPattern { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> JobDesirableTeamRole { get; set; }

        public UpdateJobCandidatePortalViewModel_value<int?> CandidatePrefLanguage { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> CandidatePrefVisaPreference { get; set; }

        public string TotalExperiance { get; set; }
        public string RelevantExperiance { get; set; }


        [MaxLength(5)]
        public string CPCurrency { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public int? CPDeductionsPerAnnum { get; set; }
        public int? CPVariablePayPerAnnum { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        [MaxLength(5)]
        public string EPCurrency { get; set; }
        public int? EPTakeHomeSalPerMonth { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".pdf", ".jpg", ".png", ".gif", ".jpeg" })]
        public List<IFormFile> PaySlips { get; set; }
        public List<string> PaySlipUrls { get; set; }

        
        [Required]
        public CandidateEmployeeStatus CandidatePrefEmployeeStatus { get; set; }
        public bool? CandidateResignationAccepted { get; set; }
        public bool? CurrEmplFlag { get; set; }
        [MaxLength(100)]
        public string CurrOrganization { get; set; }
        public int? CurrDesignation { get; set; }
        public byte NoticePeriod { get; set; }
        public DateTime? CandidateLastWorkDate { get; set; }

        public bool? AnyOfferInHand { get; set; }
        public DateTime? CandidateCanJoinDate { get; set; }

        public string ReasonsForReloc { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skillRating> CandidateSkills { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableSpecializations { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableImplementations { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableDesigns { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableDevelopments { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableSupports { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableQualities { get; set; }
        public List<UpdateJobCandidatePortalViewModel_skill> JobDesirableDocumentations { get; set; }


        //public List<CandidateQualificationModel> CandidateQualificationModel { get; set; }
        public List<UpdateJobCandidatePortalViewModel_CandidateEducation> CandidateQualificationModel { get; set; }//CandidateEducationQualifications
        //public List<CandidateCertificationModel> CandidateCertificationModel { get; set; }
        public List<UpdateJobCandidatePortalViewModel_CandidateCertification> CandidateCertificationModel { get; set; }//CandidateEducationCertifications
        public List<UpdateJobCandidatePortalViewModel_OpeningEducation> OpeningQualifications { get; set; }
        public List<UpdateJobCandidatePortalViewModel_CandidateCertification> OpeningCertifications { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".doc", ".docx", ".pdf" })]
        public IFormFile Resume { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize((byte)FileType.Video)]
        public IFormFile CandVideoProfile { get; set; }


        public bool? InterviewFaceToFace { get; set; }
        [MaxLength(1000)]
        public string InterviewFaceToFaceReason { get; set; }
        //[MaxLength(500)]
        //public string Remarks { get; set; }


        //[DataType(DataType.Upload)]
        //[MaxFileSize((byte)FileType.File)]
        //[AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg" })]
        //public IFormFile Photo { get; set; }


        //[MaxLength(100)]
        //public string FullNameInPp { get; set; }




        //public string ResumeURL { get; set; }
        //public string ResumeURLType { get; set; }
        //public List<PaySlipsModel> PaySlipsModel { get; set; }

        //public List<CandidateQuestionResponseModel> CandidateQuestionResponseModel { get; set; }
    }
    public class UpdateJobCandidatePortalViewModel_value<T>
    {
        public T Value { get; set; }
        [Required]
        public bool PreferenceType { get; set; }
    }
    public class UpdateJobCandidatePortalViewModel_skill
    {
        [Required]
        public int TechnologyId { get; set; }
        [Required]
        public int ExpInYears { get; set; }
        [Required]
        public int ExpInMonths { get; set; }
    }
    public class UpdateJobCandidatePortalViewModel_skillRating : UpdateJobCandidatePortalViewModel_skill
    {
        [Required]
        public int TechnologyId { get; set; }
        [Required]
        public int ExpYears { get; set; }
        [Required]
        public int ExpMonths { get; set; }
        [Required]
        public byte SelfRating { get; set; }
    }
    public class UpdateJobCandidatePortalViewModel_CandidateEducation
    {
        public int QualificationId { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> Qualification { get; set; }
        public int CourseId { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> Course { get; set; }
    }
    public class UpdateJobCandidatePortalViewModel_OpeningEducation
    {
        public int QualificationId { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> Qualification { get; set; }
    }
    public class UpdateJobCandidatePortalViewModel_CandidateCertification
    {
        public int CertificationId { get; set; }
        public UpdateJobCandidatePortalViewModel_value<int?> Certification { get; set; }
    }
    #endregion
    public class UpdateCandidateViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }
        public int SourceId { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg" })]
        public IFormFile Photo { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".doc", ".docx", ".pdf" })]
        public IFormFile Resume { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".pdf", ".jpg", ".png", ".gif", ".jpeg" })]
        public List<IFormFile> PaySlips { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize((byte)FileType.Video)]
        public IFormFile CandVideoProfile { get; set; }

        public Nullable<bool> ValidPpflag { get; set; }
        [MaxLength(100)]
        public string FullNameInPp { get; set; }
        public DateTime? CandidateDOB { get; set; }

        //[MaxLength(1)]
        public int? Gender { get; set; }
        //[MaxLength(1)]
        public int? MaritalStatus { get; set; }
        public byte NoticePeriod { get; set; }
        public string ReasonsForReloc { get; set; }
        [MaxLength(100)]
        public string CurrLocation { get; set; }
        public int? CurrLocationID { get; set; }
        public int? CountryID { get; set; }
        [MaxLength(100)]
        public string CurrOrganization { get; set; }
        public int? CurrDesignation { get; set; }
        public Nullable<bool> CurrEmplFlag { get; set; }
        public string TotalExperiance { get; set; }
        public string RelevantExperiance { get; set; }
        [MaxLength(20)]
        public string ContactNo { get; set; }
        [MaxLength(20)]
        public string AlteContactNo { get; set; }
        [MaxLength(5)]
        public string CPCurrency { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public int? CPDeductionsPerAnnum { get; set; }
        public int? CPVariablePayPerAnnum { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        [MaxLength(5)]
        public string EPCurrency { get; set; }
        public int? EPTakeHomeSalPerMonth { get; set; }
        public int? Nationality { get; set; }

        public string ResumeURL { get; set; }
        public string ResumeURLType { get; set; }
        public List<PaySlipsModel> PaySlipsModel { get; set; }

        public List<UpdateCandidateSkillViewModel> UpdateCandidateSkillViewModel { get; set; }
        public List<CandidateQualificationModel> CandidateQualificationModel { get; set; }
        public List<CandidateCertificationModel> CandidateCertificationModel { get; set; }
        public List<CandidateQuestionResponseModel> CandidateQuestionResponseModel { get; set; }
    }

    public class UpdateCandidateSkillViewModel
    {
        public int Id { get; set; }
        public int SkillLevelId { get; set; }
        public string SkillLevel { get; set; }
        public int TechnologyId { get; set; }
        public int ExpInYears { get; set; }
        public int ExpInMonths { get; set; }
        public byte SelfRating { get; set; }
        public bool IsCanSkill { get; set; }
    }

    public class MapCandidateViewModel
    {
        [Required]
        public int[] JobId { get; set; }
        [Required]
        public int CandidateId { get; set; }
        public string EPCurrency { get; set; }
        public int? EPTakeHomeSalPerMonth { get; set; }
        public int? ExpectedPackage { get; set; }
    }

    public class CandidateListSearchViewModel
    {
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        public string SearchKey { get; set; }
        public int? JobId { get; set; }
        public int? PuId { get; set; }
        public string Recruiter { get; set; } //commona separate
        public int? Availability { get; set; } //notice period    
        public string Gender { get; set; } //commona separate
        public string Rating { get; set; } //commona separate
        public string ApplicationStatus { get; set; } //commona separate   candidate status
        public string MaritalStatus { get; set; } //commona separate   marital status
        public string Nationality { get; set; } //commona separate
        public string CurrentLocaiton { get; set; } //commona separate  country 
        public string Age { get; set; }
        public string Currency { get; set; }
        public string SalaryRange { get; set; }
        public string Source { get; set; } //common separate
        public bool MyCandidates { get; set; } //refer for recruiters

        public bool? dmReview { get; set; }
        public bool? tlReview { get; set; }
        public bool? l1Review { get; set; }

        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
    }

    public class JobCandidateListFilterDataViewModel
    {
        public List<int> CountryIds { get; set; }
        public List<int> NationalityIds { get; set; }
        public List<int> CandProfStatusIds { get; set; }
        public List<string> OpCurrencyIds { get; set; }
    }

    public class SuggestCandidateListSearchViewModel
    {
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        public string SearchKey { get; set; }

        [Required]
        public int JobId { get; set; }
        public string Recruiter { get; set; } //commona separate
        public int? Availability { get; set; } //  notice period    
        public string Gender { get; set; } //commona separate
        public string Rating { get; set; } //commona separate
        public string ApplicationStatus { get; set; } //commona separate   candidate status
        public string MaritalStatus { get; set; } //commona separate   marital status
        public string Nationality { get; set; } //commona separate
        public string CurrentLocaiton { get; set; } //commona separate  city 
        public string Age { get; set; }
        public string Currency { get; set; }
        public string SalaryRange { get; set; }
        public string Source { get; set; } //commona separate

    }

    public class CandidateExportSearchViewModel
    {
        public string SearchKey { get; set; }
        public int? JobId { get; set; }
        public string Recruiter { get; set; } //commona separate
        public int? Availability { get; set; } //  notice period    
        public string Gender { get; set; } //commona separate
        public string Rating { get; set; } //commona separate
        public string ApplicationStatus { get; set; } //commona separate   candidate status
        public string MaritalStatus { get; set; } //commona separate   marital status
        public string Nationality { get; set; } //commona separate
        public string CurrentLocaiton { get; set; } //commona separate  city 
        public string Age { get; set; }
        public string Currency { get; set; }
        public string SalaryRange { get; set; }
        public string Source { get; set; } //commona separate

    }

    public class GetCandidateViewModel
    {
        public int CandProfId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public int JobId { get; set; }
        public int? SourceId { get; set; }
        public string Remarks { get; set; }
        public Nullable<bool> ValidPpflag { get; set; }
        public string FullNameInPp { get; set; }
        public DateTime? CandidateDOB { get; set; }
        public int? Gender { get; set; }
        public int? MaritalStatus { get; set; }
        public byte? NoticePeriod { get; set; }
        public int? ReasonType { get; set; }
        public string ReasonsForReloc { get; set; }
        public int? CurrentPackage { get; set; }
        public int? ExpectedPackage { get; set; }
        public string CurrLocation { get; set; }
        public int? CurrLocationID { get; set; }
        public int? CountryID { get; set; }
        public string CurrOrganization { get; set; }
        public string TotalExperiance { get; set; }
        public string RelevantExperiance { get; set; }
        public string ContactNo { get; set; }
        public string AlteContactNo { get; set; }
        public string CPCurrency { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public int? CPDeductionsPerAnnum { get; set; }
        public int? CPVariablePayPerAnnum { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        public string EPCurrency { get; set; }
        public int? EPTakeHomeSalPerMonth { get; set; }
        public double SelfRating { get; set; }
        public int? Nationality { get; set; }
        public int? StageId { get; set; }
        public int? CandPrfStatus { get; set; }
        public Nullable<bool> CurrEmplFlag { get; set; }
        public int? CurrDesignation { get; set; }
        public List<string> PaySlips { get; set; }
        public string Resume { get; set; }
        public string Photo { get; set; }
        public List<GetCandidateSkillViewModel> CandidateSkillViewModel { get; set; }
        public List<GetCandCertifications> Certififcates { get; set; }
        public List<GetCandQualifications> Qualifications { get; set; }
        public List<GetCandQuestionResponse> QuestionResponse { get; set; }
    }

    public class CandidateSkillViewModel
    {
        public List<GetCandidateSkillViewModel> JobsSkillViewModel { get; set; }
        public List<GetCandidateSkillViewModel> CandidatesSkillViewModel { get; set; }
    }

    public class GetCandidateSkillViewModel
    {
        public int Id { get; set; }
        public int SkillLevelId { get; set; }
        public string SkillLevel { get; set; }
        public int TechnologyId { get; set; }
        public string TechnologyName { get; set; }
        public int ExpInYears { get; set; }
        public int ExpInMonths { get; set; }
        public byte SelfRating { get; set; }
        public bool IsCanSkill { get; set; }
    }
    public class UpdateCandidatePrfStatusViewModel
    {
        [Required]
        public int CandidateId { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public int? CandidateStatuId { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
    }
    public class UpdateCandidateCVStatusViewModel
    {
        [Required]
        public int CandidateId { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public string CandidateStatuCode { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
    }

    public class ValidateCanJobSalaryViewModel
    {
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public string Curreny { get; set; }
        [DataType(DataType.PhoneNumber, ErrorMessage = "This value is not a number")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(0, 2147483647, ErrorMessage = "Please use values between 0 to 2147483647")]
        public int Salary { get; set; }
        [MaxLength(1000)]
        public string Remarks { get; set; }
    }

    public class CandidateOtherStatusViewModel
    {
        [Required]
        public int CandidateId { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        [MaxLength(5)]
        public string Code { get; set; }
        [Required]
        [MaxLength(10)]
        public string TaskCode { get; set; }
    }


    public class CandidateTagsViewModel
    {
        public int CandidateId { get; set; }
        public int JobId { get; set; }
        public int TagId { get; set; }
        public string TagWord { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class CreateTag
    {
        [Required]
        [MaxLength(50)]
        public string TagWord { get; set; }
        [Required]
        public int CandidateId { get; set; }
        [Required]
        public int JobId { get; set; }
    }
    public class RemoveTag
    {
        [Required]
        public int TagId { get; set; }
        [Required]
        public int CandidateId { get; set; }
        [Required]
        public int JobId { get; set; }
    }
    public class CandidateJobsViewModel
    {
        public int CandidateId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int JobId { get; set; }
        public int? RecId { get; set; }
        public int? BroughtBy { get; set; }
        public string BdmName { get; set; }
        public string BdmProfilePhoto { get; set; }
        public string JobTitle { get; set; }
        public int? StageId { get; set; }
        public string StageName { get; set; }
        public int? CandProfStatus { get; set; }
        public string CandProfStatusName { get; set; }
        public string RecName { get; set; }
        public string ProfilePhoto { get; set; }
        public bool? IsTagged { get; set; }
    }

    public class CandidateFilesViewModel
    {
        public int Id { get; set; }
        public int CandProfId { get; set; }
        public string DocType { get; set; }
        public byte FileGroup { get; set; }
        public string FileGroupName { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int Joid { get; set; }
        public int? UploadedBy { get; set; }
        public string UploadedByName { get; set; }
        public string FilePath { get; set; }
        public string DocStatusName { get; set; }
        public string ProfilePhoto { get; set; }
        public byte DocStatus { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool UploadedFromDrive { get; set; }
    }
    public class UploadCandidateFileViewModel
    {
        [Required]
        public int CandId { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public byte FileGroup { get; set; }
        [Required]
        [MaxLength(100)]
        public string DocType { get; set; }

        [Required]
        [MaxFileSize((byte)FileType.Audio)]
        public List<IFormFile> Files { get; set; }
    }

    public class UpdateCandidateFileViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int CandId { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public byte FileGroup { get; set; }
        [Required]
        [MaxLength(100)]
        public string DocType { get; set; }

        [Required]
        [MaxFileSize((byte)FileType.Audio)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg", ".doc", ".docx", ".pdf" })]
        public List<IFormFile> Files { get; set; }
    }

    public class CandidateOverViewModel
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string ContactNo { get; set; }
        public string AlteContactNo { get; set; }
        public int? Gender { get; set; }
        public int? MaritalStatus { get; set; }
        public byte? NoticePeriod { get; set; }
        public DateTime? CandidateDOB { get; set; }
        public int? Nationality { get; set; }

        public string CurrLocation { get; set; }
        public int? CurrLocationID { get; set; }
        public int? CountryID { get; set; }

        public string CountryName { get; set; }
        public string CPCurrency { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public int? CPDeductionsPerAnnum { get; set; }
        public int? CPVariablePayPerAnnum { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        public string EPCurrency { get; set; }
        public int? EPTakeHomeSalPerMonth { get; set; }
        public string Opcurrency { get; set; }
        public int? OpgrossPayPerAnnum { get; set; }
        public int? OpdeductionsPerAnnum { get; set; }
        public int? OpvarPayPerAnnum { get; set; }
        public int? OpnetPayPerAnnum { get; set; }
        public int? OptakeHomePerMonth { get; set; }
        public int? OpgrossPayPerMonth { get; set; }
        public bool? OpconfirmFlag { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Sourced { get; set; }
        public int? SourceId { get; set; }
        public int? StageId { get; set; }
        public int? CandProfStatus { get; set; }
        public string CandProfStatusName { get; set; }
        public string StageName { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }

        public int? BudgetCurrencyId { get; set; }
        public int JobCountryId { get; set; }
        public int JobCityId { get; set; }

        public int ClientId { get; set; }
        public string BudgetCurrency { get; set; }
        public string ProfilePhoto { get; set; }
        public string PaySlip { get; set; }
        public string CandProfStatusCode { get; set; }
        public int? CandidateUserId { get; set; }
        public string SalaryVerifiedby { get; set; }
        public bool? IsPayslipVerified { get; set; }
        public int? PayslipSalary { get; set; }
        public string PayslipCurrency { get; set; }
        public string JobTitle { get; set; }
        public DateTime? JobStartDate { get; set; }
        public DateTime? JobClosedDate { get; set; }
        public string RelevantExperience { get; set; }
        public string TotalExperience { get; set; }
        public string ReasonsForReloc { get; set; }
        public int? PuId { get; set; }
        public int CandProfId { get; set; }
        public string FinalCVUrl { get; set; }
        public bool BgvacceptedFlag { get; set; }
        public double SelfRating { get; set; }
        public bool? IsTagged { get; set; }
        public string Roles { get; set; }
        public List<string> RolesText { get; set; }
        public List<string> ReasonsText { get; set; }
        public string RejectOtherReason { get; set; }
        public bool UploadedFromDrive { get; set; }

        public bool? TLReview { get; set; }
        public bool? DMReview { get; set; }
        public bool? L1Review { get; set; }

        public List<GetCandCertifications> Certififcates { get; set; }
        public List<GetCandQualifications> Qualifications { get; set; }
        public List<GetCandQuestionResponse> QuestionResponse { get; set; }

        public List<CandidateJobReOfferDtlsViewModel> CandidateJobReOfferDtlsViewModel { get; set; }
    }


    public class CandidateJobReOfferDtlsViewModel
    {
        public string Opcurrency { get; set; }
        public int OpgrossPayPerMonth { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int JoId { get; set; }
        public int CandProfId { get; set; }
    }

    public class CandidateStatusReviewViewModel
    {
        [Required]
        public int JobId { get; set; }
        [Required]
        public int CanProfId { get; set; }
        [Required]
        public string ActivityDesc { get; set; }
        [Required]
        public byte ReviewBy { get; set; } // TL or DM 
        [Required]
        public bool Status { get; set; }
        [Required]
        public string CandProfStatus { get; set; }
    }


    public class CandidateStatusReviewListViewModel
    {
        public int JobId { get; set; }
        public int CanProfId { get; set; }
        public string ActivityDesc { get; set; }
        public int ReviewBy { get; set; } // TL or DM 
        public string ReviewByName { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool Status { get; set; }
        public string CandProfStatus { get; set; }
    }

    public class GetCandCertifications
    {
        public int Id { get; set; }
        public int CertificationId { get; set; }
        public string CertificationName { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class GetCandQualifications
    {
        public int Id { get; set; }
        public string Qualification { get; set; }
        public int QualificationId { get; set; }
        public string Course { get; set; }
        public int? CourseId { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class GetCandQuestionResponse
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string Response { get; set; }
        public byte QuestionType { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CandidateJobOfferDtlsViewModel
    {
        public string CPCurrency { get; set; }
        public int? CPGrossPayPerAnnum { get; set; }
        public int? CPDeductionsPerAnnum { get; set; }
        public int? CPVariablePayPerAnnum { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        public string EPCurrency { get; set; }
        public int? EPTakeHomeSalPerMonth { get; set; }
        public string Opcurrency { get; set; }
        public int? OpgrossPayPerAnnum { get; set; }
        public int? OpdeductionsPerAnnum { get; set; }
        public int? OpvarPayPerAnnum { get; set; }
        public int? OpnetPayPerAnnum { get; set; }
        public int? OptakeHomePerMonth { get; set; }
        public int? OpgrossPayPerMonth { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public int? BudgetCurrencyId { get; set; }
        public string BudgetCurrency { get; set; }
    }


    public class ReofferPackageViewModel
    {
        [DataType(DataType.PhoneNumber, ErrorMessage = "This value is Not a number")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(100, 2147483647, ErrorMessage = "Please use values between 100 to 2147483647")]
        public int Salary { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public int CanPrfId { get; set; }

        public string[] SalaryProposalOfferBenefits { get; set; }
    }

    public class CandidateInfoViewModel
    {
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public int CanPrfId { get; set; }
        public string EmailId { get; set; }
        public string ProfilePhoto { get; set; }
        public string Name { get; set; }
        public int? Nationality { get; set; }
        public DateTime? DateofBirth { get; set; }
        public int? Gender { get; set; }
        public int? MaritalStatus { get; set; }
        public string PhoneNumber { get; set; }
        public string AlterPhoneNumber { get; set; }
        public string Roles { get; set; }
        public byte? NoticePeriod { get; set; }

        public List<GetCandidateSkillViewModel> CandidatesSkillViewModel { get; set; }
        public List<CandidateSocialReferenceViewModel> CandidateSocialReferenceViewModel { get; set; }
    }


    public class UpdateCandidateInfoViewModel
    {
        [Required]
        public int? CountryId { get; set; }
        [Required]
        public int CityId { get; set; }
        [Required]
        [MaxLength(100)]
        public string CityName { get; set; }
        [Required]
        public int CanPrfId { get; set; }
        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg" })]
        public IFormFile ProfilePhoto { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public int? Nationality { get; set; }
        [Required]
        public DateTime? DateofBirth { get; set; }
        [Required]
        //[MaxLength(1)]
        public int? Gender { get; set; }
        [Required]
        //[MaxLength(1)]
        public int? MaritalStatus { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string AlterPhoneNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string Roles { get; set; }
        [Required]
        public byte? NoticePeriod { get; set; }

        public List<GetCandidateSkillViewModel> CandidatesSkillViewModel { get; set; }
        public List<CandidateSocialReferenceViewModel> CandidateSocialReferenceViewModel { get; set; }
    }

    public class CandidateSocialReferenceViewModel
    {
        [Required]
        public byte ProfileType { get; set; }
        public string ProfileTypeName { get; set; }
        [Required]
        [MaxLength(500)]
        public string ProfileURL { get; set; }
    }

    public class ResumeViewModel
    {
        [Required]
        public int CandPrfId { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public string DocType { get; set; }
    }

    public class CandidateAssessmentViewModel
    {
        public int Id { get; set; }
        public string CanAssessmentId { get; set; }
        public string CanAssessmentName { get; set; }
        public int JobId { get; set; }
        public string PreviewURL { get; set; }
        public string DistributionId { get; set; }
        public string ContactId { get; set; }
        public string ResponseURL { get; set; }
        public byte? ResponseStatus { get; set; }
        public string ResponseStatusName { get; set; }
        public DateTime? ResponseDate { get; set; }
        public DateTime AssessmentSentDate { get; set; }
        public byte Status { get; set; }
        public int JobAssessmentId { get; set; }
    }

    public class CreateCandidateAssessmentViewModel
    {
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public int JobAssessmentId { get; set; }
        [Required]
        public string CanAssessmentId { get; set; }
        [Required]
        public string CanAssessmentName { get; set; }
        [Required]
        public int JobId { get; set; }
    }

    public class CandidateFileApproveRejectViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public string DocType { get; set; }
        [Required]
        public byte FileGroup { get; set; }
        [Required]
        public int Joid { get; set; }
        [Required]
        public byte DocStatus { get; set; }
        [MaxLength(100)]
        public string Remarks { get; set; }
    }

    public class CandidateFinalCVRejectViewModel
    {
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public int Joid { get; set; }
        [MaxLength(100)]
        public string Remarks { get; set; }
    }


    public class CandidateDocumentRequestViewModel
    {
        [Required]
        public int Joid { get; set; }
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public byte FileGroup { get; set; }
        [Required]
        public string[] DocType { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
    }


    public class CandidateMultiDocumentRequestViewModel
    {
        [Required]
        public int Joid { get; set; }
        [Required]
        public int CandProfId { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
        public List<DocumentsIdsViewModel> DocumentsIdsViewModel { get; set; }
    }

    public class DocumentsIdsViewModel
    {
        [Required]
        public byte FileGroup { get; set; }
        [Required]
        public string DocType { get; set; }
    }


    public class CandidateVideoProfileRequestViewModel
    {
        [Required]
        public int CanPrfId { get; set; }
        [Required]
        public int JobId { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
    }

    public class ReOfferApproveViewModel
    {
        [Required]
        public bool Approve { get; set; }
        [Required]
        public int CanPrfId { get; set; }
        [Required]
        public int JobId { get; set; }
    }

    public class BacthProfilesModel
    {
        [Required]
        public int CanPrfId { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public string BatchId { get; set; }
        [Required]
        public int CandidateShareProfId { get; set; }
    }

    public class SharedProfileCandidateModel
    {
        public string CandidateName { get; set; }
        public int CanPrfId { get; set; }
        public int JobId { get; set; }
        public string BatchId { get; set; }
        public int? CandProfStatus { get; set; }
        public string CandProfStatusName { get; set; }
        public string CandProfStatusCode { get; set; }
        public string JobName { get; set; }
        public int CandidateShareProfId { get; set; }
        public string InterviewTimeZone { get; set; }
    }


    public class CandidateCSATViewModel
    {
        public List<CandidateRatingModel> CandidateRatingModel { get; set; }
        public decimal CSATScore { get; set; }

    }
    public class CandidateRatingModel
    {
        public int CanPrfId { get; set; }
        public int JobId { get; set; }
        public byte? Rating { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public byte RatingType { get; set; }
    }
    public class MapAssessmentResponseViewModel
    {
        public int SurveyId { get; set; }
    }


    public class CandidateInterViewDtls
    {
        public int InterviewId { get; set; }
        public int JobId { get; set; }
        public int CanPrfId { get; set; }
        public byte ModeofInterview { get; set; }
        public string ModeofInterviewName { get; set; }
        public DateTime? InterviewDate { get; set; }
        public string InterviewEndTime { get; set; }
        public string InterviewStartTime { get; set; }
        public bool InterviewPrefernce { get; set; }
        public string InterviewTimeZone { get; set; }
        public string Location { get; set; }
        public string Remarks { get; set; }
        public string ClientPannel { get; set; }
        public string HirePannel { get; set; }
    }

    public class CandidateInterviewViewModel
    {
        public int? UserId { get; set; }
        public int JobId { get; set; }
        public int CanPrfId { get; set; }
        public byte ModeofInterview { get; set; }
        public DateTime? InterviewDate { get; set; }
        public string InterviewEndTime { get; set; }
        public string InterviewStartTime { get; set; }
        public string Location { get; set; }
        public string Remarks { get; set; }
        public string CandProfStatusCode { get; set; }
        public byte ClreviewStatus { get; set; }
        public int? CandProfStatus { get; set; }
        public string CandProfStatusName { get; set; }
        public string ModeofInterviewName { get; set; }
        public int YearsofExperience { get; set; }
        public int? ExperienceInMonths { get; set; }
        public string CandidateName { get; set; }
        public int? MaritalStatus { get; set; }
        public int? Gender { get; set; }
        public int? Nationality { get; set; }
        public string Reasons { get; set; }
        public string ConfView { get; set; }
        public string EmailFields { get; set; }
        public int? RecruiterId { get; set; }
        public bool ShortListFlag { get; set; }
        public bool InterviewPrefernce { get; set; }
        public string InterviewTimeZone { get; set; }

    }


    public class CandidateInterviewDtlsRequestViewModel
    {
        public int JobId { get; set; }
        public int CanPrfId { get; set; }
    }

    public class ClientCandidateInterviewViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int JobId { get; set; }
        public int CanPrfId { get; set; }
        public DateTime? InterviewDate { get; set; }
        public string InterviewStartTime { get; set; }
        public string InterviewEndTime { get; set; }
        public byte ModeofInterview { get; set; }
        public string ModeofInterviewName { get; set; }
        public string InterviewTimeZone { get; set; }
    }


    public class CandidateUnSubscribeRequestModel
    {
        public string CandProfId { get; set; }
        public string EmailId { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
    }

}

