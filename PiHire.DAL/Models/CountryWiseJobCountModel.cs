using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class CountryWiseJobCountModel
    {
        public string Iso { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }     
        public int JobCount { get; set; }
    }


    public class LocationWiseJobCountModel
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public int JobCount { get; set; }
    }
}
