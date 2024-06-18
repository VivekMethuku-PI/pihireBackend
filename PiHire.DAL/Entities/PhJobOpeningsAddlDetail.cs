using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobOpeningsAddlDetail
{
    public int Id { get; set; }

    public int Joid { get; set; }

    public int? Buid { get; set; }

    public int? Puid { get; set; }

    public int? CurrencyId { get; set; }

    public string SalaryPackage { get; set; }

    public int? JobTenure { get; set; }
    public int? JobWorkPattern { get; set; }
    public byte? JobWorkPatternPrefTyp { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public decimal? MinSalary { get; set; }

    public decimal? MaxSalary { get; set; }

    public decimal? AnnualSalary { get; set; }

    public string SalaryRemarks { get; set; }

    public bool? AccessToAll { get; set; }

    public int? Spocid { get; set; }

    public DateTime? ApprJoinDate { get; set; }

    public int? CandPrefLcation { get; set; }

    public int? NoOfCvsRequired { get; set; }

    public int? NoticePeriod { get; set; }
    public byte? NoticePeriodPrefTyp { get; set; }

    public bool? CvWithLogo { get; set; }

    public bool? CvAsPdf { get; set; }

    public int? NoOfCvsFilled { get; set; }

    public int? NoOfCvsToBeFilled { get; set; }

    public int? CrmoppoId { get; set; }

    public bool ClientReviewFlag { get; set; }

    public string AddlComments { get; set; }

    public int? ClientBilling { get; set; }
}
