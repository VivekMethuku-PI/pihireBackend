using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobOfferLetter
{
    public int Id { get; set; }

    public int Joid { get; set; }

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

    public byte Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? SignatureAuthority { get; set; }

    public int? CompanyId { get; set; }

    public int? EmployeeType { get; set; }

    public string FileName { get; set; }

    public string FileUrl { get; set; }

    public string FileType { get; set; }
}
