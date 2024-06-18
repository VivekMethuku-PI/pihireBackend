using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class VwJob
{
    public int JobLocationId { get; set; }

    public int? CityId { get; set; }

    public string CityName { get; set; }

    public string CountryName { get; set; }

    public int CountryId { get; set; }

    public int ClientId { get; set; }

    public string ClientName { get; set; }

    public DateTime ClosedDate { get; set; }

    public int Id { get; set; }

    public string JobRole { get; set; }

    public string JobTitle { get; set; }

    public string JobDescription { get; set; }

    public DateTime StartDate { get; set; }

    public int JobOpeningStatus { get; set; }

    public string JobOpeningStatusName { get; set; }

    public string Jscode { get; set; }

    public int Status { get; set; }

    public int? MinExp { get; set; }

    public int? MaxExp { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public string CreatedByName { get; set; }

    public string ShortJobDesc { get; set; }

    public int? AsmtCounter { get; set; }

    public int? JobPostingCounter { get; set; }

    public int? ClientViewsCounter { get; set; }

    public int? EmailsCounter { get; set; }

    public int? ProfilesSharedToClientCounter { get; set; }
}
