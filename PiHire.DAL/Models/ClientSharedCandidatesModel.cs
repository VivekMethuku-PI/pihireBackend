using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class ClientSharedCandidatesModel
    {
        public string CandName { get; set; }
        public int CandProfId { get; set; }
        public byte? NoticePeriod { get; set; }
        public int? CandProfStatus { get; set; }
        public string CandProfStatusName { get; set; }
        public string CurrLocation { get; set; }
        public string CountryName { get; set; }
        public string RecruiterName { get; set; }
        public int? RecruiterID { get; set; }
        public int? BroughtBy { get; set; }        
        public string BroughtName { get; set; }
        public int JoId { get; set; }
        public string CBCurrency { get; set; }
        public int? CBPerMonth { get; set; }
        public string Experience { get; set; }
        public byte ShortListFlag { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
