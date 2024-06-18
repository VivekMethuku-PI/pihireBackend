using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class SalaryCalculatorCountryViewModel
    {
        public int Id { get; set; }
        public string Iso { get; set; }
        public string Name { get; set; }
        public string Nicename { get; set; }
        public string Iso3 { get; set; }
        public int? Numcode { get; set; }
        public int Phonecode { get; set; }
        public string Currency { get; set; }
        public string CurrSymbol { get; set; }
    }
    #region Citywise benefits
    public class CityWiseBenefitViewModel
    {
        public int? Id { get; set; }
        public int CityId { get; set; }
        public string BenefitTitle { get; set; }
        public string BenefitDesc { get; set; }
    }
    #endregion
    public class CountryWiseBenefitViewModel
    {
        public int? Id { get; set; }
        public int CountryId { get; set; }
        public bool IsSalaryWise { get; set; }
        public string BenefitTitle { get; set; }
        public string BenefitDesc { get; set; }

    }
    public class CountryWiseAllowanceDetailViewModel
    {
        public string AllowanceCode { get; set; }
        public string AllowanceTitle { get; set; }
        public decimal? AllowancePrice { get; set; }
        public decimal? AllowancePercentage { get; set; }
    }
    public class CountryWiseAllowanceViewModel
    {
        public int? Id { get; set; }
        public int CountryId { get; set; }
        public bool? IsCitizenWise { get; set; }
        public string AllowanceCode { get; set; }
        public string AllowanceTitle { get; set; }
        public string AllowanceDesc { get; set; }
        public decimal? AllowancePrice { get; set; }
        public decimal? AllowancePercentage { get; set; }
    }

    public class GetBURequestVM
    {
        public List<int> PuIds { get; set; }
    }

    public class CommonEnumViewModel
    {
        public List<SelectViewModel> ActionType { get; set; }
        public List<SelectViewModel> ActionMode { get; set; }
        public List<SelectViewModel> SendMode { get; set; }
        public List<SelectViewModel> SendTo { get; set; }
        public List<SelectViewModel> Gender { get; set; }
        public List<SelectViewModel> SourceType { get; set; }
        public List<SelectViewModel> FileGroup { get; set; }
        public List<SelectViewModel> MessageType { get; set; }
        public List<SelectViewModel> ProfileType { get; set; }
        public List<SelectViewModel> SentBy { get; set; }
        public List<SelectViewModel> CandOverallStatus { get; set; }
        public List<SelectViewModel> DocStatus { get; set; }
        public List<SelectViewModel> ClreviewStatus { get; set; }
        public List<SelectViewModel> InterviewStatus { get; set; }
        public List<SelectViewModel> ModeOfInterview { get; set; }
        public List<SelectViewModel> ScheduledBy { get; set; }
        public List<SelectViewModel> ActivityType { get; set; }
        public List<SelectViewModel> AuditType { get; set; }
    }

    public class SelectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class SelectWithCodeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class UserRemarksViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string NoteDesc { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }

    public class UserRemarksRequestModel
    {
        public int UserId { get; set; }
        public dashboardDateFilter DateFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class CreateUserRemarksModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        [MaxLength(1000)]
        public string NoteDesc { get; set; }
    }

    public class UpdateUserRemarksModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        [MaxLength(1000)]
        public string NoteDesc { get; set; }
    }

    public class CountryViewModel
    {
        public int Id { get; set; }
        public string Iso { get; set; }
        public string Name { get; set; }
        public string Nicename { get; set; }
        public string Iso3 { get; set; }
        public int? Numcode { get; set; }
        public int Phonecode { get; set; }
        public string Currency { get; set; }
        public string CurrSymbol { get; set; }
    }

    public class CityViewModel
    {
        public int? CountryId { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Iso2 { get; set; }
        public string Iso3 { get; set; }
        public int Id { get; set; }
    }


    public class ClientContactViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string contactEmailId { get; set; }
    }

    public class SpocViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? ClientId { get; set; }
    }

    public class AssessmentViewModel
    {
        public string Id { get; set; }
        public string SurveyCode { get; set; }
        public string SurveyRefCode { get; set; }
        public string SurveyName { get; set; }
        public string SurveyDesc { get; set; }
        public string SurveyType { get; set; }
        public string PreviewUrl { get; set; }
    }

    public class AssessmentsList
    {
        public string surveyId { get; set; }
        public string surveyCode { get; set; }
        public string surveyRefCode { get; set; }
        public string surveyTitle { get; set; }
        public string surveyDesc { get; set; }
        public string surveyType { get; set; }
        public string previewUrl { get; set; }
    }

    public class PageLinks
    {
        public object first { get; set; }
        public object prev { get; set; }
        public string self { get; set; }
        public object next { get; set; }
        public object last { get; set; }
    }

    public class Pagination
    {
        public int perPage { get; set; }
        public int totalRecords { get; set; }
        public int totalPages { get; set; }
        public int currentPage { get; set; }
        public PageLinks pageLinks { get; set; }
    }

    public class AssessmentsModel
    {
        public List<AssessmentsList> elements { get; set; }
        public Pagination pagination { get; set; }
    }

    public class HappinessApiUserToken
    {
        public string Token { get; set; }
        public string Token_type { get; set; }
        public int Expires_in { get; set; }
    }

    public class UserTokenResult
    {
        public HappinessApiUserToken Elements { get; set; }
    }

    public class DistributionResult
    {
        public string DistributionID { get; set; }
        public int ContactsCount { get; set; }
    }

    public partial class Meta
    {
        public int HttpStatusCode { get; set; }
        public string HttpStatusMessage { get; set; }
        public string RequestID { get; set; }
        public string DateTime { get; set; }
        public object Error { get; set; }
    }

    public class HappinessApiBaseViewModel<T>
    {
        public T Result { get; set; }
        public Meta Meta { get; set; }
        public bool Status { get; set; }
    }

    public class DistributionResponse
    {
        public string DistributionChannel { get; set; }
        public bool Responded { get; set; }
        public DistributionContact DistributionContact { get; set; }
        public string ResponseID { get; set; }
        public string ResponseStatus { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class DistributionContact
    {
        public string ContactID { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailID { get; set; }
        public string Isdcode { get; set; }
        public string MobileNo { get; set; }
        public string SalesForceId { get; set; }
    }




    public class SurveyListViewModel
    {
        public int? SurveyTypeId { get; set; }
        public int? IndustryId { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }

    public class CreateActivityViewModel
    {
        public byte ActivityType { get; set; }
        public byte ActivityMode { get; set; }
        public int JobId { get; set; }
        public int? ActivityOn { get; set; }
        public string ActivityDesc { get; set; }
        public int UserId { get; set; }
        public int? CurrentStatusId { get; set; }
        public int? UpdateStatusId { get; set; }
    }

    public class CreateAuditViewModel
    {
        public byte ActivityType { get; set; }
        public int? TaskID { get; set; }
        public string ActivitySubject { get; set; }
        public string ActivityDesc { get; set; }
        public int UserId { get; set; }
    }

    public class AuditViewModel
    {
        public List<AuditListViewModel> AuditListViewModel { get; set; }
        public int TtlCount { get; set; }
    }

    public class AuditListViewModel
    {
        public byte ActivityType { get; set; }
        public int? TaskID { get; set; }
        public string ActivitySubject { get; set; }
        public string ActivityDesc { get; set; }
        public int UserId { get; set; }
    }

    public class MediaFilesViewModel
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ConfiguredSmptViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public bool VerifyFlag { get; set; }
        public string VerifyToken { get; set; }
    }

    public class ConfigureSmptMailViewModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; }
        [Required]
        [MaxLength(20)]
        public string PasswordHash { get; set; }
    }

    public class UserMailConfigureSuccessViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Token { get; set; }
    }

    public class AssessmentResponseViewModel
    {
        public DateTime StartDateTime { get; set; }
        public string ResponseChannel { get; set; }
        public string ResponseStatus { get; set; }
        public ContactDetailsViewModel ContactDetails { get; set; }
        public string ResponseAnonymousUrl { get; set; }
        public string DistributionId { get; set; }
    }

    public class PostVM
    {
        public string data { get; set; }
    }


    public class ContactDetailsViewModel
    {
        public int Id { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string IsdCode { get; set; }
    }


    public class UpdateStatusModel
    {
        public int Id { get; set; }
        public byte Status { get; set; }
    }


    public class ActivityListSearchViewModel
    {
        public int CurrentPage { get; set; }
        public int PerPage { get; set; }
        public int? ActivityType { get; set; }
        public int? SUserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public byte? DateFilter { get; set; }
    }

    public class AuditListSearchViewModel
    {
        public int CurrentPage { get; set; }
        public int PerPage { get; set; }
        public int? AuditType { get; set; }
        public int? SUserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public byte? DateFilter { get; set; }
    }


}
