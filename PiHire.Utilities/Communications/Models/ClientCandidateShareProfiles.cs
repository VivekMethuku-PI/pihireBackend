using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.Utilities.Communications.Models
{
    public class ClientCandidateShareProfiles
    {
        public string ResourceName { get; set; }
        public int CanShareId { get; set; }
        public int CandProfId { get; set; }
        public string NoticePeriod { get; set; }
        public string TotalExperiance { get; set; }
        public string Country { get; set; }
        public string CurrLocation { get; set; }
        public int? cbSalary { get; set; }
        public string cbCurrency { get; set; }
        public string Review { get; set; }
        public int? RecId { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
    }
}
