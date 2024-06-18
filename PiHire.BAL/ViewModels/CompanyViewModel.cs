using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.ViewModel
{
    public class CompanyViewModel
    {
        public int id { get; set; }
        public string pu_name { get; set; }
        public string short_name { get; set; }
        public DateTime date_of_establisment { get; set; }
        public int city { get; set; }
        public string state { get; set; }
        public int country { get; set; }
        public string time_zone { get; set; }
        public string iso_code { get; set; }
        public string mobile_number { get; set; }
        public string website { get; set; }
        public string logo { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public string VAT_Number { get; set; }      
        public string city_name { get; set; }
        public string country_name { get; set; }
        public string created_user { get; set; }
        public List<SocialMediaLink> socialMediaLinks { get; set; }
        public string pan_no { get; set; }
        public string gst_no { get; set; }
        public string service_tax_no { get; set; }
        public string tin_no { get; set; }
        public string tan_no { get; set; }
        public string PayslipEmail { get; set; }
    }
    public class SocialMediaLink
    {
        public int id { get; set; }
        public string url { get; set; }
    }


    public class CreateUpdateProcessUnitViewModel
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int id { get; set; }
        [Required]
        public string pu_name { get; set; }
        [Required]
        public string short_name { get; set; }
        public DateTime date_of_establishment { get; set; }
        public string state { get; set; }
        public string time_zone { get; set; }
        public string iso_code { get; set; }
        public string mobile_number { get; set; }
        public string website { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string LogoURL { get; set; }
        public string VAT_Number { get; set; }
        [Required]
        public string city_name { get; set; }
        [Required]
        public string country_name { get; set; }
        public string pan_no { get; set; }
        public string gst_no { get; set; }
        public string service_tax_no { get; set; }
        public string tin_no { get; set; }
        public string tan_no { get; set; }
    }

    public class CreateUpdateProcessUnitBusinessUnitViewModel
    {
        [Required]
        [Range(0,int.MaxValue)]
        public int id { get; set; }
        public int pu_id { get; set; }
        public string bus_unit_full_name { get; set; }
        [MaxLength(8)]
        public string bus_unit_code { get; set; }  // 8 characters only
        public string description { get; set; }
    }


    public class CreateUpdateProcessUnitLocationViewModel
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int id { get; set; }
        public int pu_id { get; set; }
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
        public string city { get; set; }
        public string State { get; set; }
        public string country { get; set; }
        public string pin { get; set; }
        public string website { get; set; }

        public string location_map { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string location_map_name { get; set; }
        public string location_map_type { get; set; }
        public bool? isMainLocation { get; set; }
    }

}
