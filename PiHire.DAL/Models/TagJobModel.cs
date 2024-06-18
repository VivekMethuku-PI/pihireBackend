using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class TagJobModel
    {
        public List<ToTagJobsList> JobList { get; set; }
        public int JobCount { get; set; }
    }

    public class ToTagJobsList
    {
        public int JobId { get; set; }
        public int? ClientId { get; set; }
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public string JobRole { get; set; }
        public DateTime? PostedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public bool PublishedOnCareerPortal { get; set; }
        public int? TotalDays { get; set; }
        public int? CompletedDays { get; set; }
        public string ShortJobDesc { get; set; }
        
    }

    public class JobsModel
    {
        public List<JobsList> JobList { get; set; }
        public int JobCount { get; set; }
    }

    public class JobsList
    {

        public int Id { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public int CountryId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public DateTime? ClosedDate { get; set; }       
        public string JobRole { get; set; }
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public DateTime StartDate { get; set; }
        public int JobOpeningStatus { get; set; }
        public string JobOpeningStatusName { get; set; }
        public string Jscode { get; set; }
        public int Status { get; set; }
        public int MaxExp { get; set; }
        public int MinExp { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string ShortJobDesc { get; set; }
        public int? AsmtCounter { get; set; }
        public int? JobPostingCounter { get; set; }
        public int? ClientViewsCounter { get; set; }
        public int? EmailsCounter { get; set; }
        public int? ProfilesSharedToClientCounter { get; set; }
        public DateTime? ModificationOn { get; set; }
        public string ModificationBy { get; set; }
        public int? isAssigned { get; set; }
        public int? Priority { get; set; }
        public string PriorityName { get; set; }
        public string Age { get; set; }
        public List<JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; }
    }

    public class DashboardJobsModel
    {
        public List<DashboardJobsList> JobList { get; set; }
        public int JobCount { get; set; }
    }



    public class DashboardJobsList
    {

        public int Id { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public int CountryId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string JobRole { get; set; }
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public DateTime StartDate { get; set; }
        public int JobOpeningStatus { get; set; }
        public string JobOpeningStatusName { get; set; }
        public string Jscode { get; set; }
        public int Status { get; set; }
        public int MaxExp { get; set; }
        public int MinExp { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string ShortJobDesc { get; set; }
        public int? AsmtCounter { get; set; }
        public int? JobPostingCounter { get; set; }
        public int? ClientViewsCounter { get; set; }
        public int? EmailsCounter { get; set; }
        public int? ProfilesSharedToClientCounter { get; set; }
        public DateTime? ModificationOn { get; set; }
        public string ModificationBy { get; set; }     
        public int Assinged { get; set; }
        public int? Priority { get; set; }
        public string PriorityName { get; set; }
        public string Age { get; set; }
        public int? NoOfCvsRequired { get; set; }
        public int? NoOfCvsFullfilled { get; set; }

        public Nullable<bool> Assign { get; set; }
        public Nullable<bool> PriorityUpdate { get; set; }
        public Nullable<bool> Note { get; set; }
        public Nullable<bool> Interviews { get; set; }
        public Nullable<bool> JobStatus { get; set; }
        public Nullable<bool> CandStatus { get; set; }

        public List<JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; }
    }


    public class ClientSharedURLs
    {
        public string URL { get; set; }
        public int CandidateCount { get; set; }
    }

    public class JobOpeningStatusCounterViewModel
    {
        public int? StageID { get; set; }
        public string StageName { get; set; }
        public int? Counter { get; set; }
        public string StageColor { get; set; }

    }


    public class BDMJobOpeningStatusCounter
    {
        public int BdmId { get; set; }
        public int? Counter { get; set; }
    }


    public class JobsListCount
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
    }

    public class JobCountModel
    {
        public int JobsCount { get; set; }
    }

}
