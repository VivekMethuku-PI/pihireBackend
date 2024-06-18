using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCandidateEmpmtDetail
{
    public int Id { get; set; }

    public int CandProfId { get; set; }

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

    public byte Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
