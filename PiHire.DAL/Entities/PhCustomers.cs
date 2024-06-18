using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities
{
    public partial class PhCustomers
    {
        public int CandProfId { get; set; }
        public short? SourceId { get; set; }
        public string EmailId { get; set; }
        public string CandName { get; set; }
        public string FullNameInPp { get; set; }
        public DateTime? Dob { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string CurrOrganization { get; set; }
        public string CurrLocation { get; set; }
        public int? CurrLocationId { get; set; }
        public byte? NoticePeriod { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public byte? ReasonType { get; set; }
        public string ReasonsForReloc { get; set; }
        public string Experience { get; set; }
        public int? ExperienceInMonths { get; set; }
        public string RelevantExperience { get; set; }
        public int? ReleExpeInMonths { get; set; }
        public string ContactNo { get; set; }
        public string AlteContactNo { get; set; }
        public string Cpcurrency { get; set; }
        public int? CptakeHomeSalPerMonth { get; set; }
        public byte SelfRating { get; set; }
    }
}
