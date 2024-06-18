using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class CustomSchedulerViewModel
    {
        public int ID { get; set; }
    }
    public class CustomSchedulerJobViewModel
    {
        public int Id { get; set; }
        public string JobDescription { get; set; }
        public int? MinExpeInMonths { get; set; }
        public int? MaxExpeInMonths { get; set; }
        public string JobTitle { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public string ClientName { get; set; }
        public string JobCountry { get; set; }
        public string JobLocation { get; set; }
        public string JobStatus { get; set; }
        public string JobCurrencyName { get; set; }
        public int? PUID { get; set; }
        public int? BUID { get; set; }
        public string recruiterName { get; set; }
        public string recruiterMobileNumber { get; set; }
        public string recruiterEmailSignature { get; set; }
        public string recruiterEmailID { get; set; }
        public string recruiterPosition { get; set; }
        public string bdmName { get; set; }
    }
}
