using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class PuLocationViewModel
    {
        public int id { get; set; }
        public string company_name { get; set; }
        public int company_id { get; set; }
        public string location_name { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string land_mark { get; set; }
        public string contact_person { get; set; }
        public string land_number { get; set; }
        public string mobile_number { get; set; }
        public string fax_no { get; set; }
        public string email_address { get; set; }
        public int city { get; set; }
        public String State { get; set; }
        public int country { get; set; }
        public string pin { get; set; }
        public string website { get; set; }
        public string? location_map { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public string city_name { get; set; }
        public string country_name { get; set; }
        public string location_map_name { get; set; }
        public string location_map_type { get; set; }
        public bool? isMainLocation { get; set; }
        public string? PuLogo { get; set; }
    }
}
