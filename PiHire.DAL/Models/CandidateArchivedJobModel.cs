using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class CandidateArchivedJobModel
    {
        public int JobId { get; set; }
        public int? ClientId { get; set; }
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public string JobRole { get; set; }
        public DateTime? PostedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }      
        public int MinExp { get; set; }
        public int MaxExp { get; set; }
        public string ShortJobDesc { get; set; }
    }
}
