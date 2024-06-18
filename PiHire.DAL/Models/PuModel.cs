using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    
    public class PuLocationsModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public string LocationName { get; set; }
    }


    public class CountryModel
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


    public class CityModel
    {
        public int Id { get; set; }
        public int? CountryId { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Iso2 { get; set; }
        public string Iso3 { get; set; }
    }
}
