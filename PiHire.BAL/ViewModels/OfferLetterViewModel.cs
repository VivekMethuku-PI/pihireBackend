using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class OfferLetterViewModel
    {
        public int JobId { get; set; }
        public int CandProfId { get; set; }
        public int? BasicSalary { get; set; }
        public int? Hra { get; set; }
        public int? Conveyance { get; set; }
        public int? Otbonus { get; set; }
        public int? Sickness { get; set; }
        public int? Gratuity { get; set; }
        public int? NetSalary { get; set; }
        public int? Ita { get; set; }
        public int? GrossSalary { get; set; }
        public int? GrossSalaryPerAnnum { get; set; }
        public DateTime? JoiningDate { get; set; }
        public int? DesignationId { get; set; }
        public int? ProcessUnitId { get; set; }
        public int? SpecId { get; set; }
        public int CurrencyId { get; set; }
        public int? DepartmentId { get; set; }
        public int? SignatureAuthority { get; set; }
        public int? LocationId { get; set; }
        public int? EmployeeType { get; set; }
    }

    public class CreateOfferLetterSlabViewModel
    {
        public OfferLetterViewModel OfferLetterViewModel { get; set; }
        public List<SlabComponentValuesModel> SlabComponentValuesModel { get; set; }
    }

    public class CreateOfferLetterWithSlabViewModel
    {
        public OfferLetterViewModel OfferLetterViewModel { get; set; }
        public List<OfferLetterAllowanceViewModel> OfferLetterAllowanceViewModel { get; set; }
    }

    public class OfferLetterAllowanceViewModel
    {
        public string AllowanceTitle { get; set; }
        public decimal Amount { get; set; }
    }

    public class ReleaseIntentOfferViewModel
    {
        public int JobCandOfferId { get; set; }
        public string Remarks { get; set; }
        public List<string> UserIds { get; set; }
    }

    public class SlabComponentValuesModel
    {
        public int SlabId { get; set; }
        public int ComponentId { get; set; }
        public decimal Amount { get; set; }
    }

    public class JobOfferLetterViewModel
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int CandProfId { get; set; }
        public string CandName { get; set; }
        public int? BasicSalary { get; set; }
        public int? Hra { get; set; }
        public int? Conveyance { get; set; }
        public int? Otbonus { get; set; }
        public int? Sickness { get; set; }
        public int? Gratuity { get; set; }
        public int? NetSalary { get; set; }
        public int? Ita { get; set; }
        public int? GrossSalary { get; set; }
        public int? GrossSalaryPerAnnum { get; set; }
        public DateTime? JoiningDate { get; set; }
        public int? DesignationId { get; set; }
        public int? ProcessUnitId { get; set; }
        public int? SpecId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int? DepartmentId { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? EmployeeType { get; set; }
        public string? EmployeeTypeName { get; set; }

    }

    public class FileDownloadViewModel
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
    }

    public class FileURLViewModel
    {
        public string FileName { get; set; }
        public string FileURL { get; set; }
        public string FileType { get; set; }
    }

    public class OfferdCandidateSearchModel
    {
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        [MaxLength(200)]
        public string SearchKey { get; set; }
    }

    public class HtmlMessageBodyViewModel
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string OfferHtml { get; set; }
        public bool Logo { get; set; }
    }

}
