using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class CandidateSimilarJobs
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
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public int? MinExpeInMonths { get; set; }
        public int? MaxExpeInMonths { get; set; }
        public int MinExp { get; set; }
        public int MaxExp { get; set; }
        public string ShortJobDesc { get; set; }
    }

    public class CandidateSimilarJobCount
    {
        public int SimilarJobCount { get; set; }
    }

    public class CandidateSimilarJobModel
    {
        public List<CandidateSimilarJobs> CandidateSimilarJobs { get; set; }
        public int SimilarJobCount { get; set; }
    }



    

}
