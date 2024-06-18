using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwAllCandidate
{
    public int? CandJobId { get; set; }

    public int? JoId { get; set; }

    public int CandProfId { get; set; }

    public short? SourceId { get; set; }

    public string EmailId { get; set; }

    public string CandName { get; set; }

    public string FullNameInPp { get; set; }

    public DateTime? Dob { get; set; }

    public string Gender { get; set; }

    public string MaritalStatus { get; set; }

    public byte CandProfStatus { get; set; }

    public string CandProfStatusName { get; set; }

    public string TagWords { get; set; }

    public int? SelfRating { get; set; }

    public int? Evaluation { get; set; }

    public int? StageId { get; set; }

    public int? CsCode { get; set; }

    public string CurrOrganization { get; set; }

    public string CurrLocation { get; set; }

    public int? CurrLocationId { get; set; }

    public byte? NoticePeriod { get; set; }

    public int? CountryId { get; set; }

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

    public string Cpcurrency { get; set; }

    public int? CptakeHomeSalPerMonth { get; set; }

    public int? CpgrossPayPerAnnum { get; set; }

    public string Epcurrency { get; set; }

    public int? EptakeHomePerMonth { get; set; }

    public string OpCurrency { get; set; }

    public int? OpTakeHomePerMonth { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? PuId { get; set; }

    public string JobCategory { get; set; }
}
