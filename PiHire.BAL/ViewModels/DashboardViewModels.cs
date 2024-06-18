using PiHire.BAL.Repositories;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public enum dashboardDateFilter
    {
        Today = 0, ThisMonth = 1, LastMonth = 2, ThisQuarter = 3, LastQuarter = 4, ThisWeek = 5, LastWeek = 6, CustomDates = 7, Yesterday = 8, Tomorrow = 9
    }
    public class DashboardFilterViewModel
    {

    }
    public class DashboardRecruiterDaywiseFilterViewModel : DashboardFilterPaginationViewModel
    {
        public bool? OnLeave { get; set; }
        public int? LocationId { get; set; }
        public int? JoId { get; set; }
        public int? RecruiterId { get; set; }
    }
    public class DashboardBdmFilterViewModel : DashboardFilterViewModel
    {
        public dashboardDateFilter DateFilter { get; set; }
        public string JobCategory { get; set; }
    }
    public class DashboardRecruiterFilterViewModel : DashboardFilterViewModel
    {
        public dashboardDateFilter DateFilter { get; set; }
    }
    public class DashboardFilterPaginationViewModel : DashboardFilterViewModel
    {
        public int PerPage { get; set; } = 0;
        public int CurrentPage { get; set; } = 1;
    }
    public class DashboardRecruiterDaywiseHistoryFilterViewModel : DashboardFilterViewModel
    {
        public dashboardDateFilter DateFilter { get; set; }
        public int PerPage { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }
    public class DashboardCandidateInterviewFilterViewModel : DashboardFilterPaginationViewModel
    {
        public int[] PuIds { get; set; }
        public int[] BuIds { get; set; }
    }
    public class DashboardCandidateInterviewViewModel
    {
        public List<InterviewStageStatus> interviewStageStatuses { get; set; }
        public int totalPages { get; set; }
        public List<_DashboardCandidateInterviewViewModel> interviews { get; set; }
    }
    public class _DashboardCandidateInterviewViewModel
    {
        public int ID { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public int CandidateId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public byte? NoticePeriod { get; set; }
        public string CandidateNumber { get; set; }
        public int InterviewStatus { get; set; }
        public byte ModeOfInterview { get; set; }

        public DateTime StartDt { get; set; }
        public string StartTm { get; set; }
        public DateTime EndDt { get; set; }
        public string EndTm { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterPhoto { get; set; }
        public string BdmName { get; set; }
        public string BdmPhoto { get; set; }
        public int tabNo { get; set; }


        public int candProfStatus { get; set; }
        public string candProfStatusCode { get; set; }
        public string CandProfStatusName { get; set; }
        public string finalCVUrl { get; set; }
        public string CityName { get; set; }
        public string TimeLine { get; set; }
        public string Experience { get; set; }
        public int? ExperienceInMonths { get; set; }

        internal static _DashboardCandidateInterviewViewModel ToViewModel(DashboardCandiateInterviewModel da, Repositories.BaseRepository baseRepo)
        {
            return new _DashboardCandidateInterviewViewModel
            {
                ID = da.Id,
                BdmName = da.bdmName,
                BdmPhoto = baseRepo.getEmployeePhotoUrl(da.bdmPhoto, da.bdmId ?? 0),
                CandidateEmail = da.EmailID,
                CandidateId = da.CandProfId,
                CandidateName = da.CandName,
                CandidateNumber = da.ContactNo,
                ClientId = da.ClientID,
                ClientName = da.ClientName,
                EndDt = da.InterviewDate,
                StartDt = da.InterviewDate,
                StartTm = da.InterviewStartTime,
                EndTm = da.InterviewEndTime,
                JobId = da.JobId,
                JobTitle = da.JobTitle,
                RecruiterName = da.recrName,
                RecruiterPhoto = baseRepo.getEmployeePhotoUrl(da.recrPhoto, da.recruiterID ?? 0),
                CityName = da.CityName,
                candProfStatus = da.candProfStatus,
                candProfStatusCode = da.candProfStatusCode,
                CandProfStatusName = da.CandProfStatusName,
                finalCVUrl = baseRepo.getCandidateFileUrl(da.finalCVUrl, da.CandProfId),
                InterviewStatus = da.InterviewStatus,
                ModeOfInterview = da.ModeOfInterview,
                tabNo = da.tabNo,
                Experience= da.Experience,
                TimeLine= da.TimeLine,
                NoticePeriod= da.NoticePeriod,
                ExperienceInMonths = da.ExperienceInMonths
            };
        }
    }
    public class DashboardJobStageViewModel
    {
        public int totalPages { get; set; }
        public List<JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; }
        public List<_DashboardJobStageViewModel> jobs { get; set; }
    }
    public class _DashboardJobStageViewModel
    {
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public int JobId { get; set; }
        public string JobOpeningStatus { get; set; }
        public string JobOpeningStatusCode { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public int? NoOfCvsRequired { get; set; }
        public int? NoOfCvsFullfilled { get; set; }
        public DateTime? ReopenedDate { get; set; }
        public string JobCountry { get; set; }
        public string JobCity { get; set; }
        public int? BdmId { get; set; }
        public string BdmName { get; set; }
        public string BdmPhoto { get; set; }
        public List<JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; }

        internal static _DashboardJobStageViewModel ToViewModel(Repositories.BaseRepository baseRepo, DashboardJobStageModel da, List<DAL.Entities.PiHireUser> bdms, Dictionary<int, string> countries, Dictionary<int, string> cities, Dictionary<short, string> jobOpeningStatus, Dictionary<short, string> jobOpeningStatuCodes)
        {
            return new _DashboardJobStageViewModel
            {
                JobCountry = da.jobCountryId.HasValue && countries.ContainsKey(da.jobCountryId.Value) ? countries[da.jobCountryId.Value] : "",
                JobCity = da.jobCityId.HasValue && cities.ContainsKey(da.jobCityId.Value) ? cities[da.jobCityId.Value] : "",
                JobOpeningStatus = da.JobOpeningStatus.HasValue && jobOpeningStatus.ContainsKey((short)da.JobOpeningStatus.Value) ? jobOpeningStatus[(short)da.JobOpeningStatus.Value] : "",
                JobOpeningStatusCode = da.JobOpeningStatus.HasValue && jobOpeningStatuCodes.ContainsKey((short)da.JobOpeningStatus.Value) ? jobOpeningStatuCodes[(short)da.JobOpeningStatus.Value] : "",
                BdmId = da.bdmId,
                BdmName = bdms.Where(dai => dai.Id == da.bdmId).Select(da => da.FirstName + ' ' + da.LastName).FirstOrDefault() ?? "",
                BdmPhoto = baseRepo.getEmployeePhotoUrl(bdms.Where(dai => dai.Id == da.bdmId).Select(da => da.ProfilePhoto).FirstOrDefault() ?? "", da.bdmId ?? 0),
                ClientName = da.ClientName,
                PostedDate = da.PostedDate,
                ClosedDate = da.ClosedDate,
                JobId = da.JobId,
                JobTitle = da.JobTitle,
                NoOfCvsRequired = da.NoOfCvsRequired,
                NoOfCvsFullfilled = da.NoOfCvsFullfilled,
                ReopenedDate = da.ReopenedDate
            };
        }
    }

    public class DashboardJobRecruiterStageViewModel
    {
        public int timeSpentDays { get; set; }
        public int timeSpentHours { get; set; }
        public int timeSpentMinutes { get; set; }
        public TimeSpan timeSpent { get; set; }

        public int timeRemainingDays { get; set; }
        public int timeRemainingHours { get; set; }
        public int timeRemainingMinutes { get; set; }
        public TimeSpan timeRemaining { get; set; }

        public int totalPages { get; set; }
        public List<DAL.Models.JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; }
        public List<_DashboardJobRecruiterStageViewModel> recruiters { get; set; }
    }
    public class _DashboardJobRecruiterStageViewModel
    {
        public int recruiterID { get; set; }
        public DateTime PostedDate { get; set; }
        public string recrName { get; set; }
        public List<DateTime?> DeassignDates { get; set; }
        public bool isDeassigned { get; set; }
        public int timeSpentDays { get; set; }
        public int timeSpentHours { get; set; }
        public int timeSpentMinutes { get; set; }
        public TimeSpan timeSpent { get; set; }

        public List<DAL.Models.JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; }
        public string recrPhoto { get; set; }
        public int? NoCvsRequired { get; set; }
        public int? NoOfFinalCvsFilled { get; set; }

        internal static _DashboardJobRecruiterStageViewModel ToViewModel(DashboardJobRecruiterStageModel da)
        {
            return new _DashboardJobRecruiterStageViewModel
            {
                PostedDate = da.PostedDate,
                recrName = da.recrName,
                recruiterID = da.recruiterID
            };
        }
    }
    public class DashboardRecruiterStatusViewModel
    {
        public int totalPages { get; set; }
        public List<_DashboardRecruiterStatusViewModel> recruiters { get; set; } = new List<_DashboardRecruiterStatusViewModel>();
    }
    public class _DashboardRecruiterStatusViewModel
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string profilePhoto { get; set; }
        public string role { get; set; }
        public string location { get; set; }
        public List<int> puIds { get; set; }
        public List<__DashboardRecruiterStatusViewModel> jobs { get; set; }
        public bool onLeave { get; set; }
        public bool isWeekEnd { get; set; }
        public bool isShiftExist { get; set; }
        public string LeaveTypeName { get; set; }
        public int todayCvsRequired { get; set; }
        public int tommorrowCvsRequired { get; set; }
        public int weekCvsRequired { get; set; }
        public int monthCvsRequired { get; set; }
        public int todayCvsFilled { get; set; }
        public int tommorrowCvsFilled { get; set; }
        public int weekCvsFilled { get; set; }
        public int monthCvsFilled { get; set; }
        public long totalActiveJobCount { get; set; }
    }
    public class __DashboardRecruiterStatusViewModel
    {
        public int jobId { get; set; }
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public string JobCountry { get; set; }
        public string JobCity { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public int? NoCVSRequired { get; set; }
        public int? NoOfFinalCVsFilled { get; set; }
        public int JobDateStatus { get; set; }
        internal static __DashboardRecruiterStatusViewModel toViewModel(DashboardRecruiterStatusModel da, Dictionary<int, string> countries, Dictionary<int, string> cities)
        {
            return new __DashboardRecruiterStatusViewModel
            {
                jobId = da.jobId,
                ClientName = da.ClientName,
                ClosedDate = da.ClosedDate,
                PostedDate = da.PostedDate,
                JobTitle = da.JobTitle,
                JobCountry = da.jobCountryId.HasValue && countries.ContainsKey(da.jobCountryId.Value) ? countries[da.jobCountryId.Value] : "",
                JobCity = da.jobCityId.HasValue && cities.ContainsKey(da.jobCityId.Value) ? cities[da.jobCityId.Value] : "",
                NoCVSRequired = da.NoCVSRequired,
                NoOfFinalCVsFilled = da.NoOfFinalCVsFilled,
                JobDateStatus = da.ClosedDate.Date > Repositories.BaseRepository.CurrentTime.Date ? 1 :
                da.ClosedDate.Date < Repositories.BaseRepository.CurrentTime.Date ? -1 : 0
            };
        }
    }
    public class DashboardRecruiterDaywiseViewModel
    {
        public int totalCount { get; set; }
        public int totalPages { get; set; }
        public List<_DashboardRecruiterDaywiseViewModel> recruiters { get; set; } = new List<_DashboardRecruiterDaywiseViewModel>();
        public List<LocationwiseCountDaywiseViewModel> LocationwiseCounts { get; set; }
    }
    public class _DashboardRecruiterDaywiseViewModel
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string profilePhoto { get; set; }
        public string role { get; set; }
        public string location { get; set; }
        public List<int> puIds { get; set; }
        public bool onLeave { get; set; }
        public bool isWeekEnd { get; set; }
        public bool isShiftExist { get; set; }
        public string LeaveTypeName { get; set; }
        public int todayCvsRequired { get; set; }
        public int tommorrowCvsRequired { get; set; }
        public int weekCvsRequired { get; set; }
        public int monthCvsRequired { get; set; }
        public int todayCvsFilled { get; set; }
        public int tommorrowCvsFilled { get; set; }
        public int weekCvsFilled { get; set; }
        public int monthCvsFilled { get; set; }
        public int cumulativeCvsRequired { get; set; }
        public int cumulativeCvsFilled { get; set; }
        public long totalActiveJobCount { get; set; }
    }
    public class DashboardBdmDaywiseViewModel
    {
        public int totalCount { get; set; }
        public int totalPages { get; set; }
        public List<_DashboardBdmRecruiterStatusViewModel> bdms { get; set; } = new List<_DashboardBdmRecruiterStatusViewModel>();
        public List<LocationwiseCountDaywiseViewModel> LocationwiseCounts { get; set; }
    }
    public class _DashboardBdmRecruiterStatusViewModel
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string profilePhoto { get; set; }
        public string role { get; set; }
        public string location { get; set; }
        public List<int> puIds { get; set; }
        //public List<__DashboardBdmRecruiterStatusViewModel> jobs { get; set; }
        public bool onLeave { get; set; }
        public bool isWeekEnd { get; set; }
        public bool isShiftExist { get; set; }
        public string LeaveTypeName { get; set; }
        public int todayCvsRequired { get; set; }
        public int tommorrowCvsRequired { get; set; }
        public int weekCvsRequired { get; set; }
        public int monthCvsRequired { get; set; }
        public int todayCvsFilled { get; set; }
        public int tommorrowCvsFilled { get; set; }
        public int weekCvsFilled { get; set; }
        public int monthCvsFilled { get; set; }
        public long totalActiveJobCount { get; set; }
        public List<JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; }
    }
    public class __DashboardBdmRecruiterStatusViewModel
    {
        public int jobId { get; set; }
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public string JobCountry { get; set; }
        public string JobCity { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public int? NoCVSRequired { get; set; }
        public int? NoOfFinalCVsFilled { get; set; }
        public int JobDateStatus { get; set; }
        internal static __DashboardBdmRecruiterStatusViewModel toViewModel(DashboardRecruiterStatusModel da, Dictionary<int, string> countries, Dictionary<int, string> cities)
        {
            return new __DashboardBdmRecruiterStatusViewModel
            {
                jobId = da.jobId,
                ClientName = da.ClientName,
                ClosedDate = da.ClosedDate,
                PostedDate = da.PostedDate,
                JobTitle = da.JobTitle,
                JobCountry = da.jobCountryId.HasValue && countries.ContainsKey(da.jobCountryId.Value) ? countries[da.jobCountryId.Value] : "",
                JobCity = da.jobCityId.HasValue && cities.ContainsKey(da.jobCityId.Value) ? cities[da.jobCityId.Value] : "",
                NoCVSRequired = da.NoCVSRequired,
                NoOfFinalCVsFilled = da.NoOfFinalCVsFilled,
                JobDateStatus = da.ClosedDate.Date > Repositories.BaseRepository.CurrentTime.Date ? 1 :
                da.ClosedDate.Date < Repositories.BaseRepository.CurrentTime.Date ? -1 : 0
            };
        }
    }
    public class DashboardJobsDaywisePipelineViewModel
    {
        public int totalCount { get; set; }
        public int totalPages { get; set; }
        public List<DashboardJobsDaywisePipelineViewModel_job> jobs { get; set; } = new List<DashboardJobsDaywisePipelineViewModel_job>();
    }
    public class DashboardJobsDaywisePipelineViewModel_job
    {
        public int jobId { get; set; }
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public string JobCountry { get; set; }
        public string JobCity { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public DateTime? ModificationOn { get; set; }
        public string ModificationBy { get; set; }
        //public int? NoCVSRequired { get; set; }
        //public int? NoOfFinalCVsFilled { get; set; }
        //public int JobDateStatus { get; set; }

        public List<JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; }

        public int RecruiterID { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterProfilePhoto { get; set; }

        public int BdmID { get; set; }
        public string BdmName { get; set; }
        public string BdmProfilePhoto { get; set; }

        public int todayCvsRequired { get; set; }
        public int tommorrowCvsRequired { get; set; }
        public int weekCvsRequired { get; set; }
        public int monthCvsRequired { get; set; }
        public int cumulativeCvsRequired { get; set; }

        public int todayCvsFilled { get; set; }
        public int tommorrowCvsFilled { get; set; }
        public int weekCvsFilled { get; set; }
        public int monthCvsFilled { get; set; }
        public int cumulativeCvsFilled { get; set; }

        public bool isAssigned { get; set; }
        public int? JobPuId { get; set; }
        public int? JobBuId { get; set; }

        public DateTime? jobAssignedDt { get; set; }
        public int? jobAssignedAge { get; set; }
    }
    public class DashboardSimilarJobsDaywiseViewModel
    {
        public int totalCount { get; set; }
        public int totalPages { get; set; }
        public List<DashboardSimilarJobsDaywiseViewModel_job> jobs { get; set; } = new List<DashboardSimilarJobsDaywiseViewModel_job>();
    }
    public class DashboardSimilarJobsDaywiseViewModel_job
    {
        public int jobId { get; set; }
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public string JobCountry { get; set; }
        public string JobCity { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public DateTime? ModificationOn { get; set; }
        public string ModificationBy { get; set; }
        public int JobDateStatus { get; set; }

        public List<JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterProfilePhoto { get; set; }

        public string BdmName { get; set; }
        public string BdmProfilePhoto { get; set; }

        public bool isAssigned { get; set; }
        public int? JobPuId { get; set; }
        public int? JobBuId { get; set; }
    }
    public class DashboardJobRecruitersDaywisePipelineViewModel
    {
        //public int JobId { get; set; }
        //public string JobTitle { get; set; }
        //public string JobRole { get; set; }
        //public string ClientName { get; set; }
        //public string JobOpeningStatus { get; set; }
        //public string JobOpeningStatusCode { get; set; }
        //public DateTime? PostedDate { get; set; }
        //public DateTime? ClosedDate { get; set; }
        //public int JobDateStatus { get; set; }
        //public string JobDescription { get; set; }

        public int totalCount { get; set; }
        public int totalPages { get; set; }
        public List<DashboardJobRecruitersDaywisePipelineViewModel_Recruiter> recruiters { get; set; } = new List<DashboardJobRecruitersDaywisePipelineViewModel_Recruiter>();
        public List<JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; } = new List<JobOpeningStatusCounterViewModel>();
    }
    public class DashboardJobRecruitersDaywisePipelineViewModel_Recruiter
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string profilePhoto { get; set; }
        public string role { get; set; }
        public string location { get; set; }
        public List<int> puIds { get; set; }
        public int todayCvsRequired { get; set; }
        public int tommorrowCvsRequired { get; set; }
        public int weekCvsRequired { get; set; }
        public int monthCvsRequired { get; set; }
        public int todayCvsFilled { get; set; }
        public int tommorrowCvsFilled { get; set; }
        public int weekCvsFilled { get; set; }
        public int monthCvsFilled { get; set; }

        public bool isAssigned { get; set; }
        public int? jobPriority { get; set; }
        public string jobPriorityName { get; set; }
    }
    public class DashboardDaywiseJobRecruitersViewModel_assigned
    {
        public int totalCount { get; set; }
        public int totalPages { get; set; }
        public List<DashboardDaywiseJobRecruitersViewModel_assigned_Recruiter> recruiters { get; set; } = new List<DashboardDaywiseJobRecruitersViewModel_assigned_Recruiter>();
    }
    public class DashboardDaywiseJobRecruitersViewModel_assigned_Recruiter
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string profilePhoto { get; set; }
        public string role { get; set; }
        public string location { get; set; }
        public List<int> puIds { get; set; }
        public int todayCvsRequired { get; set; }
        public int tommorrowCvsRequired { get; set; }
        public int weekCvsRequired { get; set; }
        public int monthCvsRequired { get; set; }
        public int todayCvsFilled { get; set; }
        public int tommorrowCvsFilled { get; set; }
        public int weekCvsFilled { get; set; }
        public int monthCvsFilled { get; set; }

        public bool onLeave { get; set; }
        public bool isWeekEnd { get; set; }
        public bool isShiftExist { get; set; }
        public string LeaveTypeName { get; set; }
        public int totalActiveJobCount { get; set; }

        public DateTime? jobAssignedDt { get; set; }
        public int? jobAssignedAge { get; set; }
    }
    public class DashboardDaywiseJobRecruitersViewModel_deassigned
    {
        public int totalCount { get; set; }
        public int totalPages { get; set; }
        public List<DashboardDaywiseJobRecruitersViewModel_deassigned_Recruiter> recruiters { get; set; } = new List<DashboardDaywiseJobRecruitersViewModel_deassigned_Recruiter>();
    }
    public class DashboardDaywiseJobRecruitersViewModel_deassigned_Recruiter
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string profilePhoto { get; set; }
        public string role { get; set; }
        public string location { get; set; }
        public List<int> puIds { get; set; }
        public int todayCvsRequired { get; set; }
        public int tommorrowCvsRequired { get; set; }
        public int weekCvsRequired { get; set; }
        public int monthCvsRequired { get; set; }
        public int todayCvsFilled { get; set; }
        public int tommorrowCvsFilled { get; set; }
        public int weekCvsFilled { get; set; }
        public int monthCvsFilled { get; set; }

        public bool onLeave { get; set; }
        public bool isWeekEnd { get; set; }
        public bool isShiftExist { get; set; }
        public string LeaveTypeName { get; set; }
        public int totalActiveJobCount { get; set; }

        public DateTime? jobAssignedDt { get; set; }
        public DateTime? jobDeassignedDt { get; set; }
        public int? jobAssignedAge { get; set; }
    }
    public class DashboardDaywiseJobRecruitersViewModel_unassigned
    {
        public int totalCount { get; set; }
        public int totalPages { get; set; }
        public List<DashboardDaywiseJobRecruitersViewModel_unassigned_Recruiter> recruiters { get; set; } = new List<DashboardDaywiseJobRecruitersViewModel_unassigned_Recruiter>();
    }
    public class DashboardDaywiseJobRecruitersViewModel_unassigned_Recruiter
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string profilePhoto { get; set; }
        public string role { get; set; }
        public string location { get; set; }
        public List<int> puIds { get; set; }
        public int todayCvsRequired { get; set; }
        public int tommorrowCvsRequired { get; set; }
        public int weekCvsRequired { get; set; }
        public int monthCvsRequired { get; set; }
        public int todayCvsFilled { get; set; }
        public int tommorrowCvsFilled { get; set; }
        public int weekCvsFilled { get; set; }
        public int monthCvsFilled { get; set; }

        public bool onLeave { get; set; }
        public bool isWeekEnd { get; set; }
        public bool isShiftExist { get; set; }
        public string LeaveTypeName { get; set; }
        public int totalActiveJobCount { get; set; }
    }
    public class DashboardJobRecruitersDaywiseHistoryViewModel
    {
        public int totalCount { get; set; }
        public int totalPages { get; set; }
        public List<DashboardJobRecruitersDaywiseHistoryViewModel_data> data { get; set; } = new List<DashboardJobRecruitersDaywiseHistoryViewModel_data>();
    }
    public class DashboardJobRecruitersDaywiseHistoryViewModel_data
    {
        public int[] Id { get; set; }

        public int JoId { get; set; }
        public string JobTitle { get; set; }

        public int AssignedTo { get; set; }
        public string AssignedToName { get; set; }

        public DateTime AssignmentDate { get; set; }
        public int? NoCvsrequired { get; set; }
        public int? NoOfFinalCvsFilled { get; set; }
        public List<DashboardJobRecruitersDaywiseHistoryViewModel_log> logs { get; set; } = new List<DashboardJobRecruitersDaywiseHistoryViewModel_log>();
    }
    public class DashboardJobRecruitersAssignmentHistoryViewModel
    {
        public int totalCount { get; set; }
        public int totalPages { get; set; }
        public List<DashboardJobRecruitersAssignmentHistoryViewModel_data> data { get; set; } = new List<DashboardJobRecruitersAssignmentHistoryViewModel_data>();
    }
    public class DashboardJobRecruitersAssignmentHistoryViewModel_data
    {
        public int Id { get; set; }

        public int JoId { get; set; }
        public string JobTitle { get; set; }

        public int AssignedTo { get; set; }
        public string AssignedToName { get; set; }

        public DateTime AssignedDate { get; set; }
        public string AssignedBy { get; set; }
        public DateTime? DeassignedDate { get; set; }
        public string DeassignedBy { get; set; }

        public int AssignedAge { get; set; }
    }
    public class DashboardJobRecruitersDaywiseHistoryViewModel_log
    {
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }

        public short? CvsCount { get; set; }

        public bool IsCarryForwardTo { get; set; }
        public bool IsCarryForwardFrom { get; set; }

        public short? CarryForward { get; set; }
        public short? IncrementCvs { get; set; }
        public short? DecrementCvs { get; set; }
    }
    public class LocationwiseCountDaywiseViewModel
    {
        public int? LocationId { get; set; }
        public int Count { get; set; }
    }
    public class DashboardHireAdminViewModel
    {
        public int activeJobCount { get; set; }
        public int holdJobCount { get; set; }
        public int newJobCount { get; set; }
        public int reopenJobCount { get; set; }
        public int morecvsJobCount { get; set; }

        public int todayCvsRequired { get; set; }
        public int todayCvsFilled { get; set; }
        public int tommorrowCvsRequired { get; set; }
        public int weekCvsRequired { get; set; }
        public int monthCvsRequired { get; set; }
        public int overdue1DayCvsRequired { get; set; }
        public int overdue5DaysCvsRequired { get; set; }
        public int overdue10DaysCvsRequired { get; set; }
        public int overdue30DaysCvsRequired { get; set; }
        public int submittedJobsRequired { get; set; }
        public int submittedJobsFilled { get; set; }
        public int highlightTrgtPeriodFilled { get; set; }
        public int highlightTrgtPeriodRequired { get; set; }
        public int highlightYetToJoin { get; set; }
        public int highlightJoined { get; set; }


        public DateTime? filterFromDt { get; set; }
        public DateTime? filterToDt { get; set; }

        internal static DashboardHireAdminViewModel ToModel(GetDashboardHireAdminModel da)
        {
            return new DashboardHireAdminViewModel
            {
                activeJobCount = da.activeJobCount ?? 0,
                holdJobCount = da.holdJobCount ?? 0,
                morecvsJobCount = da.morecvsJobCount ?? 0,
                newJobCount = da.newJobCount ?? 0,
                reopenJobCount = da.reopenJobCount ?? 0,
                submittedJobsFilled = da.submittedJobsFilled ?? 0,
                submittedJobsRequired = da.submittedJobsRequired ?? 0,

                highlightTrgtPeriodFilled = da.highlightTrgtPeriodFilled ?? 0,
                highlightTrgtPeriodRequired = da.highlightTrgtPeriodRequired ?? 0,
                highlightYetToJoin = da.highlightYetToJoin ?? 0,
                highlightJoined = da.highlightJoined ?? 0
            };
        }
    }
    public class DashboardBdmViewModel
    {
        public int ReqFinalCvsCount { get; set; }
        public int FinalCvsCount { get; set; }
        public int CvSubmissionCount { get; set; }
        public int InterviewCount { get; set; }
        public int ResultDueCount { get; set; }
        public int pf2fCount { get; set; }

        public int highlightTrgtPeriodFilled { get; set; }
        public int highlightTrgtPeriodRequired { get; set; }
        public int highlightYetToJoin { get; set; }
        public int highlightJoined { get; set; }

        public int newJobCount { get; set; }
        public int closedJobCount { get; set; }
        public DateTime? filterFromDt { get; set; }
        public DateTime? filterToDt { get; set; }

        internal static DashboardBdmViewModel ToModel(GetDashboardBdmModel da)
        {
            return new DashboardBdmViewModel
            {
                ReqFinalCvsCount = da.ReqFinalCvsCount ?? 0,
                FinalCvsCount = da.FinalCvsCount ?? 0,
                CvSubmissionCount = da.CvSubmissionCount ?? 0,
                InterviewCount = da.InterviewCount ?? 0,
                ResultDueCount = da.ResultDueCount ?? 0,
                pf2fCount = da.pf2fCount ?? 0,

                highlightTrgtPeriodFilled = da.highlightTrgtPeriodFilled ?? 0,
                highlightTrgtPeriodRequired = da.highlightTrgtPeriodRequired ?? 0,
                highlightYetToJoin = da.highlightYetToJoin ?? 0,
                highlightJoined = da.highlightJoined ?? 0,
                newJobCount = da.newJobCount ?? 0,
                closedJobCount = da.closedJobCount ?? 0
            };
        }
    }
    public class DashboardRecruiterViewModel
    {
        public int ReqFinalCvsCount { get; set; }
        public int FinalCvsCount { get; set; }
        public int CvSubmissionCount { get; set; }
        public int InterviewCount { get; set; }

        public int highlightTrgtPeriodFilled { get; set; }
        public int highlightTrgtPeriodRequired { get; set; }
        public int highlightYetToJoin { get; set; }
        public int highlightJoined { get; set; }

        public int AssignedCount { get; set; }
        public int SourcedCount { get; set; }
        public int TaggedCount { get; set; }
        public int AccountRejectedCount { get; set; }
        public int InterviewBackoutCount { get; set; }
        public int JoinBackoutCount { get; set; }
        public DateTime? filterFromDt { get; set; }
        public DateTime? filterToDt { get; set; }

        internal static DashboardRecruiterViewModel ToModel(GetDashboardRecruiterModel da)
        {
            return new DashboardRecruiterViewModel
            {
                ReqFinalCvsCount = da.ReqFinalCvsCount ?? 0,
                FinalCvsCount = da.FinalCvsCount ?? 0,
                CvSubmissionCount = da.CvSubmissionCount ?? 0,
                InterviewCount = da.InterviewCount ?? 0,

                highlightTrgtPeriodFilled = da.highlightTrgtPeriodFilled ?? 0,
                highlightTrgtPeriodRequired = da.highlightTrgtPeriodRequired ?? 0,
                highlightYetToJoin = da.highlightYetToJoin ?? 0,
                highlightJoined = da.highlightJoined ?? 0,

                AccountRejectedCount = da.AccountRejectedCount ?? 0,
                AssignedCount = da.AssignedCount ?? 0,
                InterviewBackoutCount = da.InterviewBackoutCount ?? 0,
                JoinBackoutCount = da.JoinBackoutCount ?? 0,
                SourcedCount = da.SourcedCount ?? 0,
                TaggedCount = da.TaggedCount ?? 0
            };
        }
    }
    public class DashboardRecruiterJobCategoryViewModel
    {
        public string JobCategory { get; set; }
        public int Count { get; set; }
    }
    public class DashboardRecruiterCandidateViewModel
    {
        public DateTime? filterFromDt { get; set; }
        public DateTime? filterToDt { get; set; }

        public List<DashboardRecruiterCandidateViewModel_cands> candidates { get; set; }
        public Dictionary<string, int> counts { get; set; }
    }
    public class DashboardRecruiterCandidateViewModel_cands
    {
        public string ClientName { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }

        public string CandidateName { get; set; }
        public int? CandProfId { get; set; }

        public DateTime? ActivityDate { get; set; }
        public string StatusCode { get; set; }
        public bool IsSUC { get; private set; }
        public bool IsPNS { get; private set; }

        internal static DashboardRecruiterCandidateViewModel_cands ToViewModel(GetDashboardRecruiterCandidateModel da)
        {
            return new DashboardRecruiterCandidateViewModel_cands
            {
                JobId = da.JobId,
                JobTitle = da.JobTitle,
                ClientName = da.ClientName,
                CandidateName = da.CandName,
                CandProfId = da.CandProfId,
                ActivityDate = da.ActivityDate,
                StatusCode = da.StatusCode,
                IsSUC = da.StatusCode == "SUC",
                IsPNS = da.StatusCode == "PNS"
            };
        }
    }

    public class DashboardRecruiterAnalyticsViewModel
    {
        public int workingJobCount { get; set; }
        public int closedJobCount { get; set; }
        public int totalCandidateCount { get; set; }
        public int hiredCandidateCount { get; set; }
        public int rejectedCandidateCount { get; set; }

        public int recruitmentApplicantsCount { get; set; }
        public int recruitmentPreScreenedCount { get; set; }
        public int recruitmentInterviewedCount { get; set; }
        public int recruitmentHiredCount { get; set; }

        public int offerAcceptedCount { get; set; }
        public int offerProvidedCount { get; set; }

        public int recruiterRejectCount { get; set; }
        public int clientRejectCount { get; set; }
        public int candidateRejectCount { get; set; }

        public DateTime? filterFromDt { get; set; }
        public DateTime? filterToDt { get; set; }
        public List<DashboardRecruiterAnalyticsCandOverTimeGrpViewModel> candOverTimeGrph { get; set; }
        public List<DashboardRecruiterAnalyticsHiredGrpViewModel> hiredGrph { get; set; }
        public List<DashboardRecruiterAnalyticsSalRngGrpViewModel> hiredSalaryGrph { get; set; }
        public List<DashboardRecruiterAnalyticsSalRngGrpViewModel> rawHiredSalaryGrph { get; set; }

        public string RecruiterName { get; set; }
        public string RecruiterPhoto { get; set; }
        public string RecruiterRole { get; set; }
        public string RecruiterLocation { get; set; }
        internal static DashboardRecruiterAnalyticsViewModel ToModel(GetDashboardRecruiterAnalyticModel da)
        {
            return new DashboardRecruiterAnalyticsViewModel
            {
                workingJobCount = da.workingJobCount ?? 0,
                closedJobCount = da.closedJobCount ?? 0,
                totalCandidateCount = da.totalCandidateCount ?? 0,
                hiredCandidateCount = da.hiredCandidateCount ?? 0,
                rejectedCandidateCount = da.rejectedCandidateCount ?? 0,
                recruitmentApplicantsCount = da.recruitmentApplicantsCount ?? 0,
                recruitmentPreScreenedCount = da.recruitmentPreScreenedCount ?? 0,
                recruitmentInterviewedCount = da.recruitmentInterviewedCount ?? 0,
                recruitmentHiredCount = da.recruitmentHiredCount ?? 0,
                offerAcceptedCount = da.offerAcceptedCount ?? 0,
                offerProvidedCount = da.offerProvidedCount ?? 0,
                recruiterRejectCount = da.recruiterRejectCount ?? 0,
                clientRejectCount = da.clientRejectCount ?? 0,
                candidateRejectCount = da.candidateRejectCount ?? 0
            };
        }
    }
    public class DashboardRecruiterAnalyticsCandOverTimeGrpViewModel
    {
        public DateTime dt { get; set; }
        public int hiredCount { get; set; }
        public int clientRejectCount { get; set; }
        public int candBackoutCount { get; set; }
    }
    public class DashboardRecruiterAnalyticsHiredGrpViewModel
    {
        public string jobTitle { get; set; }
        public int hiredCount { get; set; }
    }
    public class DashboardRecruiterAnalyticsSalRngGrpViewModel
    {
        public int strt { get; set; }
        public int end { get; set; }
        public int hiredCount { get; set; }
    }

    public class DashboardJobTimeViewModel
    {
        public int timeSpentDays { get; set; }
        public int timeSpentHours { get; set; }
        public int timeSpentMinutes { get; set; }
        public TimeSpan timeSpent { get; set; }

        public int timeRemainingDays { get; set; }
        public int timeRemainingHours { get; set; }
        public int timeRemainingMinutes { get; set; }
        public TimeSpan timeRemaining { get; set; }
    }

    #region  New Implementation 
    public class DayWiseBoughtByAccountSearchModel
    {
        public int? ProcessUnit { get; set; }
        public dashboardDateFilter DateFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    //26-4-2024


    public class _DashboardCandidateStatusBasedViewModel
    {
        public List<CandidateStatusBasedViewModel> CandidateStatusBasedViewModel { get; set; }
        public int totalPages { get; set; }
    }

    public class HireAssignmentsSearchViewModel
    {
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        [MaxLength(256)]
        public string SearchKey { get; set; }

        public int? ProcessUnit { get; set; }
        public int? BroughtBy { get; set; }


        public dashboardDateFilter DateFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? JobPriority { get; set; }

        public int? ClientId { get; set; }
        public bool? AssignmentStatus { get; set; }
        public int? JobStatus { get; set; }
    }
    public class DayWiseAssignmentsJobsSearchViewModel
    {
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        [MaxLength(256)]
        public string SearchKey { get; set; }

        public int? ProcessUnit { get; set; }
        public int? BroughtBy { get; set; }


        public dashboardDateFilter DateFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? JobPriority { get; set; }

        public bool? Assign { get; set; }
        public bool? PriorityUpdate { get; set; }
        public bool? Note { get; set; }
        public bool? Interviews { get; set; }
        public bool? JobStatus { get; set; }

        public int? ClientId { get; set; }
    }

    public class CandidateInterviewSearchViewModel
    {
        public string? SearchKey { get; set; }
        public int? PuId { get; set; }
        public int? BDMId { get; set; }
        public int? RecId { get; set; }
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        public int? JobId { get; set; }
        public int Tab { get; set; }
        public int? ClientId { get; set; }

        public dashboardDateFilter DateFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class CandidateProfileStatusSearchViewModel
    {
        public string? SearchKey { get; set; }
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        public string ProfileStatus { get; set; }
        public int JobId { get; set; }
    }


    public class DashboardJobListViewModel
    {
        public int OpeningCount { get; set; }
        public List<DashboardJobsList> OpeningList { get; set; }
    }

    public class CandidateStageWiseInfo
    {
        public string Stage  { get; set; }
        public string CandProfStatus { get; set; }
        public DateTime ActivityDate { get; set; }
        public int ActivityAge { get; set; }
    }


    #endregion 


}
