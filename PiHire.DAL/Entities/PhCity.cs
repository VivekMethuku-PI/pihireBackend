using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCity
{
    public int Id { get; set; }

    public int? CountryId { get; set; }

    public string Name { get; set; }

    public string Country { get; set; }

    public string Iso2 { get; set; }

    public string Iso3 { get; set; }
}
