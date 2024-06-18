using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwDashboardJobStage
{
    public string ClientName { get; set; }

    public string JobTitle { get; set; }

    public int JobId { get; set; }

    public int? BdmId { get; set; }

    public DateTime PostedDate { get; set; }

    public DateTime ClosedDate { get; set; }

    public int JobCityId { get; set; }

    public int JobCountryId { get; set; }

    public int JobOpeningStatus { get; set; }

    public int? NoOfCvsRequired { get; set; }

    public DateTime? ReopenedDate { get; set; }

    public int? NoOfCvsFullfilled { get; set; }
}
