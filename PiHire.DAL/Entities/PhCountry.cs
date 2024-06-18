using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCountry
{
    public int Id { get; set; }

    public string Iso { get; set; }

    public string Name { get; set; }

    public string Nicename { get; set; }

    public string Iso3 { get; set; }

    public int? Numcode { get; set; }

    public int Phonecode { get; set; }

    public string Currency { get; set; }

    public string CurrSymbol { get; set; }
}
