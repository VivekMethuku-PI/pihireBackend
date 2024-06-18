using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandidateBgvDetail
{
    public int Id { get; set; }

    public int CandProfId { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string LastName { get; set; }

    public string AnotherName { get; set; }

    public string PlaceOfBirth { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int? Gender { get; set; }

    public int? BloodGroup { get; set; }

    public int? Nationality { get; set; }

    public int? MaritalStatus { get; set; }

    public int? ExpeInYears { get; set; }

    public int? ExpeInMonths { get; set; }

    public string FatherName { get; set; }

    public string MotherName { get; set; }

    public string SpouseName { get; set; }

    public byte? NoOfKids { get; set; }

    public string HomePhone { get; set; }

    public string MobileNo { get; set; }

    public string EmerContactPerson { get; set; }

    public string EmerContactNo { get; set; }

    public string EmerContactRelation { get; set; }

    public string Ppnumber { get; set; }

    public string EmiratesId { get; set; }

    public bool UgmedicalTreaFlag { get; set; }

    public string UgmedicalTreaDetails { get; set; }

    public string PresAddress { get; set; }

    public string PresAddrLandMark { get; set; }

    public int? PresAddrCityId { get; set; }

    public int? PresAddrCountryId { get; set; }

    public DateTime? PresAddrResiSince { get; set; }

    public byte? PresAddrResiType { get; set; }

    public string PresAddrContactPerson { get; set; }

    public string PresAddrContactNo { get; set; }

    public string PresContactRelation { get; set; }

    public string PresAddrPrefTimeForVerification { get; set; }

    public string PermAddress { get; set; }

    public string PermAddrLandMark { get; set; }

    public int? PermAddrCityId { get; set; }

    public int? PermAddrCountryId { get; set; }

    public byte? PermAddrResiType { get; set; }

    public DateTime? PermAddrResiSince { get; set; }

    public DateTime? PermAddrResiTill { get; set; }

    public string PermAddrContactPerson { get; set; }

    public string PermAddrContactNo { get; set; }

    public string PermContactRelation { get; set; }

    public byte BgcompStatus { get; set; }

    public bool? BgacceptFlag { get; set; }

    public byte Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public DateTime? PpexpiryDate { get; set; }

    public int? PresPinCode { get; set; }
    public int? PermPinCode { get; set; }

    public bool? IsOdooSync { get; set; }
    public bool? IsGatewaySync { get; set; }
}
