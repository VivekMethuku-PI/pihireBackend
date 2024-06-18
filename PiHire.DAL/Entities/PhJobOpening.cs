using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhJobOpening
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string ClientName { get; set; }

    public int? JobCategoryId { get; set; }
    public string JobCategory { get; set; }

    public string JobTitle { get; set; }

    public string JobRole { get; set; }

    public string JobDescription { get; set; }

    public int? NoOfPositions { get; set; }

    public string KeyRequirements { get; set; }

    public int? MinExpeInMonths { get; set; }
    public int? MaxExpeInMonths { get; set; }
    public byte? ExpeInMonthsPrefTyp { get; set; }
    public int? MaxReleventExpInMonths { get; set; }
    public int? MinReleventExpInMonths { get; set; }
    public byte? ReleventExpInMonthsPrefTyp { get; set; }

    public int? ExpeInYears { get; set; }

    public int? ExpeInMonths { get; set; }

    public int JobLocationId { get; set; }
    public bool? JobLocationLocal { get; set; }

    public int CountryId { get; set; }

    public int? BroughtBy { get; set; }

    public int? Priority { get; set; }

    public DateTime PostedDate { get; set; }

    public DateTime ClosedDate { get; set; }

    public int JobOpeningStatus { get; set; }

    public string CreatedByName { get; set; }

    public int Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string Remarks { get; set; }

    public string ShortJobDesc { get; set; }

    public DateTime? ReopenedDate { get; set; }
}
