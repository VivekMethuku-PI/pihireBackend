using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public  class ClientViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int? CountryId { get; set; }
        public string Country { get; set; }
        public int? CityId { get; set; }
        public string City { get; set; }
        public int? IndustryId { get; set; }
        public string IndustryName { get; set; }
        public int? AccountManager { get; set; }
        public string AccountManagerName { get; set; }
    }

    public class ClientSpocsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public int? ClientId { get; set; }
    }
}
