using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandidateProfile
{
    public int Id { get; set; }

    public short? SourceId { get; set; }

    public string EmailId { get; set; }

    public string CandName { get; set; }

    public string FullNameInPp { get; set; }

    public DateTime? Dob { get; set; }

    public int? Gender { get; set; }

    public int? MaritalStatus { get; set; }

    public string CurrOrganization { get; set; }

    public string CurrLocation { get; set; }

    public int? CurrLocationId { get; set; }

    public int? CountryId { get; set; }

    public byte? NoticePeriod { get; set; }

    public int? ReasonType { get; set; }

    public string Experience { get; set; }

    public int? ExperienceInMonths { get; set; }

    public string RelevantExperience { get; set; }

    public int? ReleExpeInMonths { get; set; }

    public string ContactNo { get; set; }

    public string AlteContactNo { get; set; }

    public string Cpcurrency { get; set; }

    public int? CpgrossPayPerAnnum { get; set; }

    public int? CpdeductionsPerAnnum { get; set; }

    public int? CpvariablePayPerAnnum { get; set; }

    public int? CptakeHomeSalPerMonth { get; set; }

    public string Epcurrency { get; set; }

    public int? EptakeHomeSalPerMonth { get; set; }

    public byte CandOverallStatus { get; set; }

    public bool ProfUpdateFlag { get; set; }

    public byte ProfCompStatus { get; set; }

    public bool ProfTaggedFlag { get; set; }

    public int Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string Remarks { get; set; }

    public bool? ValidPpflag { get; set; }

    public bool? CurrEmplFlag { get; set; }

    public int? CurrDesignation { get; set; }

    public double OverallRating { get; set; }

    public string ReasonsForReloc { get; set; }

    public int? Nationality { get; set; }

    public string Roles { get; set; }
}
