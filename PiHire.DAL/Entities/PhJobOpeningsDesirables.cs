using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobOpeningsDesirables
{
    public int Joid { get; set; }

    public int? JobDomain { get; set; }
    public byte? JobDomainPrefType { get; set; }
    public int? JobTeamRole { get; set; }
    public byte? JobTeamRolePrefType { get; set; }
    public bool? CandidateValidPassport { get; set; }
    public byte? CandidateValidPassportPrefType { get; set; }
    public bool? CandidateDOB { get; set; }
    public byte? CandidateDOBPrefType { get; set; }
    public int? CandidateGender { get; set; }
    public byte? CandidateGenderPrefType { get; set; }
    public int? CandidateMaritalStatus { get; set; }
    public byte? CandidateMaritalStatusPrefType { get; set; }
    public int? CandidateLanguage { get; set; }
    public byte? CandidateLanguagePrefType { get; set; }
    public int? CandidateVisaPreference { get; set; }
    public byte? CandidateVisaPreferencePrefType { get; set; }
    public int? CandidateRegion { get; set; }
    public byte? CandidateRegionPrefType { get; set; }
    public bool? CandidateNationality { get; set; }
    public byte? CandidateNationalityPrefType { get; set; }
    public bool? CandidateResidingCountry { get; set; }
    public byte? CandidateResidingCountryPrefType { get; set; }
    public bool? CandidateResidingCity { get; set; }
    public byte? CandidateResidingCityPrefType { get; set; }
    public int? CandidateDrivingLicence { get; set; }
    public byte? CandidateDrivingLicencePrefType { get; set; }
    public bool? CandidateEmployeeStatus { get; set; }
    public byte? CandidateEmployeeStatusPrefType { get; set; }
    public bool? CandidateResume { get; set; }
    public byte? CandidateResumePrefType { get; set; }
    public bool? CandidateVidPrfl { get; set; }
    public byte? CandidateVidPrflPrefType { get; set; }
    public bool? CandidatePaySlp { get; set; }
    public byte? CandidatePaySlpPrefType { get; set; }
    public bool? CandidateNoticePeriod { get; set; }
    public byte? CandidateNoticePeriodPrefType { get; set; }
}
