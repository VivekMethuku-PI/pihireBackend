using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class SaveCandidateBGVViewModel
    {
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public int JoId { get; set; }
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        [MaxLength(50)]
        public string OtherName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(50)]
        public string PlaceOfBirth { get; set; }
        //[MaxLength(1)]
        public int? Gender { get; set; }
        //[MaxLength(1)]
        public int? MaritalStatus { get; set; }
        public int? Nationality { get; set; }

        public int? ExpeInYears { get; set; }
        public int? ExpeInMonths { get; set; }

        [MaxLength(50)]
        public string FatherName { get; set; }
        [MaxLength(50)]
        public string MotherName { get; set; }
        [MaxLength(50)]
        public string SpouseName { get; set; }
        public byte? NoOfKids { get; set; }
        [MaxLength(20)]
        public string HomePhone { get; set; }
        [MaxLength(20)]
        public string MobileNo { get; set; }

        [MaxLength(100)]
        public string EmerContactPerson { get; set; }
        [MaxLength(50)]
        public string EmerContactNo { get; set; }
        [MaxLength(50)]
        public string EmerContactRelation { get; set; }

        [MaxLength(50)]
        public string Ppnumber { get; set; }
        [MaxLength(50)]
        public string EmiratesId { get; set; }
        public bool UGMedicalTreaFlag { get; set; }
        [MaxLength(500)]
        public string UGMedicalTreaDetails { get; set; }
        public DateTime? PPExpiryDate { get; set; }

        [MaxLength(500)]
        public string PresAddress { get; set; }
        public DateTime? PresAddrResiSince { get; set; }
        public byte? PresAddrResiType { get; set; }
        public int? PresAddrCountryId { get; set; }
        public int? PresAddrCityId { get; set; }
        [MaxLength(50)]
        public string PresAddrContactPerson { get; set; }
        [MaxLength(20)]
        public string PresAddrContactNo { get; set; }
        [MaxLength(100)]
        public string PresAddrLandMark { get; set; }
        [MaxLength(50)]
        public string PresAddrPrefTimeForVerification { get; set; }
        [MaxLength(50)]
        public string PresContactRelation { get; set; }

        [MaxLength(500)]
        public string PermAddress { get; set; }
        [MaxLength(100)]
        public string PermAddrLandMark { get; set; }
        public int? PermAddrCityID { get; set; }
        public int? PermAddrCountryID { get; set; }
        public byte? PermAddrResiType { get; set; }
        public DateTime? PermAddrResiSince { get; set; }
        public DateTime? PermAddrResiTill { get; set; }
        [MaxLength(50)]
        public string PermAddrContactPerson { get; set; }
        [MaxLength(20)]
        public string PermAddrContactNo { get; set; }
        [MaxLength(50)]
        public string PermContactRelation { get; set; }
        public int? BloodGroup { get; set; }
        [MaxLength(50)]
        public string MiddleName { get; set; }
        public bool FinalSubmit { get; set; }
        public int? PresPinCode { get; set; }
        public int? PermPinCode { get; set; }
    }

    public class SaveCandidateBGVEmpViewModel
    {
        public int Id { get; set; }
        [Required]
        public int CandProfId { get; set; }
        [Required]
        [MaxLength(100)]
        public string EmployerName { get; set; }
        public string EmployId { get; set; }
        public DateTime? EmptFromDate { get; set; }
        public DateTime? EmptToDate { get; set; }
        [MaxLength(500)]
        public string Address { get; set; }
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        public int? CityId { get; set; }
        public int? CountryId { get; set; }
        public int? DesignationId { get; set; }
        [MaxLength(100)]
        public string Designation { get; set; }
        [MaxLength(50)]
        public string Cpname { get; set; }
        [MaxLength(50)]
        public string Cpdesignation { get; set; }
        [MaxLength(50)]
        public string Cpnumber { get; set; }
        [MaxLength(100)]
        public string CpemailId { get; set; }
        public bool? CurrentWorkingFlag { get; set; }
        [MaxLength(100)]
        public string OfficialEmailId { get; set; }
        [MaxLength(20)]
        public string HrcontactNo { get; set; }
        [MaxLength(100)]
        public string HremailId { get; set; }
    }

    public class SaveCandidateBGVEduViewModel
    {
        public int Id { get; set; }
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public int QualificationId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Qualification { get; set; }
        [Required]
        public int? CourseId { get; set; }
        [MaxLength(100)]
        public string Course { get; set; }
        [MaxLength(100)]
        public string UnivOrInstitution { get; set; }
        public int? YearofPassing { get; set; }
        public DateTime? DurationFrom { get; set; }
        public DateTime? DurationTo { get; set; }
        [MaxLength(50)]
        public string Grade { get; set; }
        public double? Percentage { get; set; }
    }

    public class CandidateBGVViewModel
    {
        public int CandProfId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string OtherName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public int? Gender { get; set; }
        public string GenderName { get; set; }
        public int? MaritalStatus { get; set; }
        public string MaritalStatusName { get; set; }
        public int? Nationality { get; set; }

        public int? ExpeInYears { get; set; }
        public int? ExpeInMonths { get; set; }

        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string SpouseName { get; set; }
        public int? NoOfKids { get; set; }
        public string HomePhone { get; set; }
        public string MobileNo { get; set; }

        public string EmerContactPerson { get; set; }
        public string EmerContactNo { get; set; }
        public string EmerContactRelation { get; set; }

        public string Ppnumber { get; set; }
        public string EmiratesId { get; set; }
        public bool UGMedicalTreaFlag { get; set; }
        public string UGMedicalTreaDetails { get; set; }
        public DateTime? PPExpiryDate { get; set; }

        public string PresAddress { get; set; }
        public DateTime? PresAddrResiSince { get; set; }
        public int? PresAddrResiType { get; set; }
        public int? PresAddrCountryId { get; set; }
        public int? PresAddrCityId { get; set; }
        public string PresAddrContactPerson { get; set; }
        public string PresAddrContactNo { get; set; }
        public string PresAddrLandMark { get; set; }
        public string PresAddrPrefTimeForVerification { get; set; }

        public string PermAddress { get; set; }
        public string PermAddrLandMark { get; set; }
        public int? PermAddrCityID { get; set; }
        public int? PermAddrCountryID { get; set; }
        public byte? PermAddrResiType { get; set; }
        public DateTime? PermAddrResiSince { get; set; }
        public DateTime? PermAddrResiTill { get; set; }
        public string PermAddrContactPerson { get; set; }
        public string PermAddrContactNo { get; set; }
        public string PermContactRelation { get; set; }
        public int? BloodGroup { get; set; }

        public string PresAddrCountryName { get; set; }
        public string PresAddrCityName { get; set; }
        public string PermAddrCountryName { get; set; }
        public string PermAddrCityName { get; set; }

        public string BloodGroupName { get; set; }
        public string NationalityName { get; set; }
        public string PermAddrResiTypeName { get; set; }
        public string PresAddrResiTypeName { get; set; }
        public string Bgvcomments { get; set; }
        public string CadidateStatusCode { get; set; }
        public string EmailId { get; set; }
        public bool? BgacceptFlag { get; set; }
        public int JoId { get; set; }
        public int? PuId { get; set; }

        public int? PresPinCode { get; set; }
        public int? PermPinCode { get; set; }

        public List<CandidateBGVEmployeeViewModel> CandidateBGVEmployeeViewModel { get; set; }
        public List<CandidateBGVEduViewModel> CandidateBGVEduViewModel { get; set; }
    }


    public class CandidateEmploymentEducationCertificationViewModel
    {
        public List<CandidateBGVEmployeeViewModel> CandidateEmpMentModel { get; set; }

        public List<CandidateBGVEduViewModel> CandidateEduViewModel { get; set; }

        public List<CandidateCertificationViewModel> CandidateCertificationViewModel { get; set; }

    }


    public class CandidateCertificationViewModel
    {
        public int CertificationId { get; set; }
        public string CertificationName { get; set; }
    }




    public class CandidateBGVEduViewModel
    {
        public int Id { get; set; }
        public int QualificationId { get; set; }
        public string Qualification { get; set; }
        public string Course { get; set; }
        public int? CourseId { get; set; }
        public string UnivOrInstitution { get; set; }
        public int? YearofPassing { get; set; }
        public DateTime? DurationFrom { get; set; }
        public DateTime? DurationTo { get; set; }
        public string Grade { get; set; }
        public double? Percentage { get; set; }
    }

    public class CandidateBGVEmployeeViewModel
    {
        public int Id { get; set; }
        public string EmployerName { get; set; }
        public string EmployId { get; set; }
        public DateTime? EmptFromDate { get; set; }
        public DateTime? EmptToDate { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public int? CityId { get; set; }
        public int? CountryId { get; set; }
        public int? DesignationId { get; set; }
        public string Designation { get; set; }
        public string Cpname { get; set; }
        public string Cpdesignation { get; set; }
        public string Cpnumber { get; set; }
        public string CpemailId { get; set; }
        public bool? CurrentWorkingFlag { get; set; }
        public string OfficialEmailId { get; set; }
        public string HrcontactNo { get; set; }
        public string HremailId { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }

    }

    public class AcknowledgementDwndViewModel
    {
        [Required]
        [Range(1,int.MaxValue)]
        public int CandId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int JobId { get; set; }
        public List<ReceiptsViewModel> ReceiptsViewModel { get; set; }
    }

    public class ReceiptsViewModel
    {
        // 1 : Passport, 2 : Education
        public int ReceiptType { get; set; }
        public int ReceiptId { get; set; }
    }


    public class AcceptCandidateBGVViewModel
    {
        [Required]
        public bool Accept { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
        [Required]
        public int CandProfId { get; set; }
        [Required]
        public int JoId { get; set; }
    }

    public class CandidateToEmployeeViewModel
    {

        public DateTime? JoiningDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Middlename { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string MaritalStatus { get; set; }
        public byte? NoOfKids { get; set; }
        public string SpouseName { get; set; }
        public int? BloodGroup { get; set; }
        public int? Nationality { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportValidTill { get; set; }
        public int? DepartmentId { get; set; }
        public string SkillSet { get; set; }
        public int? SpecializationId { get; set; }
        public int? EmployeeType { get; set; }
        public string? JobLocation { get; set; }

        // Contact
        public string EmailId { get; set; }
        public string MobileNum { get; set; }
        public string ContactPersonMobileNum { get; set; }
        public string EmergencyContactNum { get; set; }
        public string EmergencyContactPerson { get; set; }
        public string ContactPersonLandlineNum { get; set; }
        public string ContactPerson { get; set; }
        public string HomePhone { get; set; }

        // Present Address 
        public string PresentAddress { get; set; }
        public int? PresentAddressCity { get; set; }
        public int? PresentCountry { get; set; }
        public DateTime? PresentResidingSince { get; set; }
        public DateTime? PresentResidingTill { get; set; }
        public int? PresentAddressState { get; set; }
        public string PresentAddressLandMark { get; set; }
        public string PresentContactPerson { get; set; }
        public string PresentPersonRelationship { get; set; }
        public int? PresPinCode { get; set; }


        // Permanent Address
        public string PermanentAddress { get; set; }
        public int? PermanentAddressCity { get; set; }
        public string PermanentAddressCityName { get; set; }
        public int? PermanentCountry { get; set; }
        public DateTime? PermanentResidingSince { get; set; }
        public DateTime? PermanentResidingTill { get; set; }
        public int? PermanentAddressState { get; set; }
        public string PermanentAddressLandMark { get; set; }
        public string PermanentContactPerson { get; set; }
        public string PermanentContactPersonRelationship { get; set; }
        public int? PermPinCode { get; set; }

        public string AnotherName { get; set; }
        public string EmiratesId { get; set; }
        public int? ProcessUnit { get; set; }
        public int? BusinessUnit { get; set; }

        public int? UpdatedBy { get; set; }
        public int? RecruiterId { get; set; }
        public int? BroughtBy { get; set; }

        public List<CTECandidateOffers> Candoffers { get; set; }
        public List<CTEAllowanceDetails> AllowanceDetails { get; set; }
        public List<CTEEmpReference> EmpReference { get; set; }
        public List<CTEEmpQualification> EmpQualification { get; set; }
        public List<CandDocuments> CandDocuments { get; set; }
        public List<CandSkills> CandSkills { get; set; }
    }


    public class CandDocuments
    {
        public int Id { get; set; }
        public byte FileGroup { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string DocType { get; set; }
    }

    public class CandSkills
    {
        public string Title { get; set; }
        public int ExpInYears { get; set; }
        public int ExpInMonths { get; set; }
        public byte SelfRating { get; set; }
    }

    public class CTEAllowanceDetails
    {
        public string AllowDescription { get; set; }
        public decimal? Amount { get; set; }

    }

    public class CTECandidateOffers
    {
        public int IntentOfferId { get; set; }
        public int? UpdatedBy { get; set; } // Employee Id
        public string UpdatedByName { get; set; } // Employee Name
        public int? Basic { get; set; }
        public int? Hra { get; set; }
        public int? Conveyance { get; set; }
        public int? Otbonus { get; set; }
        public int? Sickness { get; set; }
        public int? Gratuity { get; set; }
        public int? TotalNet { get; set; }
        public int Currency { get; set; }
        public int? TotalGrass { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string CompanyDisplayName { get; set; }
        public int? DesignationId { get; set; }
        public int? DepartmentId { get; set; }
        public int? SpecializationId { get; set; }
        public int? EmployeeDesignation { get; set; } // Signature Authority
        public string FileName { get; set; }
        public string FileURL { get; set; }
        public string FileType { get; set; }
    }

    public class CTEEmpReference
    {
        public string ContactPerson { get; set; }
        public string ContactPersonDesignation { get; set; }
        public string CompanyName { get; set; }
        public string ContactPersonPhoneNum { get; set; }
        public string ContactPersonEmail { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Designation { get; set; }
    }

    public class CTEEmpQualification
    {
        public string Degree { get; set; }
        public string Course { get; set; }
        public string UniversityOrInstitution { get; set; }
        public int? YearofPassing { get; set; }
        public DateTime? DurationFrom { get; set; }
        public DateTime? DurationTo { get; set; }
        public string Grade { get; set; }
        public int? Percentage { get; set; }
    }
}
