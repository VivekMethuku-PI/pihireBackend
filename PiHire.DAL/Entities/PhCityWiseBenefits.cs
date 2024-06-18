using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCityWiseBenefit
{
    public int Id { get; set; }

    public int CityId { get; set; }

    public string BenefitTitle { get; set; }

    public string BenefitDesc { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

}
