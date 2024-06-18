using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.Repositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;


namespace PiHire.BAL.IRepositories
{
    public interface IReportRepository : IBaseRepository
    {
        #region Old Dashboard

        Task<GetResponseViewModel<DashboardCandidateInterviewViewModel>> GetCandidateInterviewAsync(DashboardCandidateInterviewFilterViewModel filterViewModel, int? tabId = null);
        Task<GetResponseViewModel<DashboardJobStageViewModel>> GetDashboardJobStageAsync(DashboardCandidateInterviewFilterViewModel filterViewModel);
        Task<GetResponseViewModel<List<Repositories.PiplineCandidatesViewModel>>> GetDashboardJobStageCandidatesAsync(int StageId, DashboardCandidateInterviewFilterViewModel filterViewModel);
        Task<GetResponseViewModel<List<Repositories.PiplineCandidatesViewModel>>> GetDashboardJobStageCandidatesAsync(int JobId, int StageId);
        Task<GetResponseViewModel<DashboardJobRecruiterStageViewModel>> GetDashboardJobRecruiterStageAsync(int jobId);
        Task<GetResponseViewModel<DashboardRecruiterStatusViewModel>> GetDashboardRecruiterStatusAsync(DashboardFilterViewModel filterViewModel, bool onLeave);
        Task<GetResponseViewModel<DashboardRecruiterDaywiseViewModel>> GetDashboardRecruiterDaywiseAsync(DashboardRecruiterDaywiseFilterViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardBdmDaywiseViewModel>> GetDashboardBdmDaywisePipelineAsync(DashboardRecruiterDaywiseFilterViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardJobsDaywisePipelineViewModel>> GetDashboardJobsDaywisePipelineAsync(Common.Types.AppConstants.UserType userType, int userId, DashboardFilterPaginationViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardJobRecruitersDaywisePipelineViewModel>> GetDashboardJobRecruitersDaywisePipelineAsync(int jobId, DashboardFilterPaginationViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_assigned>> GetDashboardDaywiseJobRecruitersAsync_assigned(int jobId, DashboardFilterPaginationViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_deassigned>> GetDashboardDaywiseJobRecruitersAsync_deassigned(int jobId, DashboardFilterPaginationViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_unassigned>> GetDashboardDaywiseJobRecruitersAsync_notAssigned(int jobId, DashboardFilterPaginationViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_unassigned>> GetDashboardDaywiseJobRecruitersAsync_hireSuggest(int jobId, DashboardFilterPaginationViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardSimilarJobsDaywiseViewModel>> GetDashboardDaywiseJobRecruitersAsync_hireSuggest_similarJobs(int jobId, int recruiterId, DashboardFilterPaginationViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardJobRecruitersAssignmentHistoryViewModel>> GetDashboardRecruiterAssignmentHistoryAsync(int? jobId, int recruiterId, DashboardFilterPaginationViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardJobRecruitersDaywiseHistoryViewModel>> GetDashboardJobRecruiterDaywiseHistoryAsync(int? jobId, int recruiterId, DashboardRecruiterDaywiseHistoryFilterViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardJobRecruitersDaywiseHistoryViewModel>> GetDashboardBdmDaywiseHistoryAsync(int bdmId, DashboardRecruiterDaywiseHistoryFilterViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardHireAdminViewModel>> GetDashboardHireAdminAsync(DashboardCandidateInterviewFilterViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardBdmViewModel>> GetDashboardBdmAsync(DashboardBdmFilterViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardRecruiterViewModel>> GetDashboardRecruiterAsync(DashboardRecruiterFilterViewModel filterViewModel);
        Task<GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>> GetDashboardRecruiterJobCatgLast14DaysAsync();
        Task<GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>> GetDashboardRecruiterJobCatgPastAsync(DashboardRecruiterFilterViewModel filterViewModel);
        Task<GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>> GetDashboardRecruiterJobCatgPresentAsync(DashboardRecruiterFilterViewModel filterViewModel);
        Task<GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>> GetDashboardRecruiterJobCatgAllAsync(DashboardRecruiterFilterViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardRecruiterAnalyticsViewModel>> GetDashboardAdminRecruiterAnalyticAsync(int recruiterId, DashboardRecruiterFilterViewModel filterViewModel);
        Task<GetResponseViewModel<double?>> GetDashboardRecruiterAvgHireDaysAsync(int recruiterId);
        Task<GetResponseViewModel<DashboardJobTimeViewModel>> GetDashboardJobTimeAsync(int jobId);
        Task<GetResponseViewModel<DashboardRecruiterCandidateViewModel>> GetDashboardRecruiterCandidatesPastAsync(DashboardRecruiterFilterViewModel filterViewModel);
        Task<GetResponseViewModel<DashboardRecruiterCandidateViewModel>> GetDashboardRecruiterCandidatesPresentAsync(DashboardRecruiterFilterViewModel filterViewModel);


        #endregion

        #region New Dashboard
        Task<GetResponseViewModel<List<Sp_BroughtBy_Job_ClientNamesModel>>> GetBroughtByJobClientNamesAsync(int boughtBy, int? puId = null);
        Task<GetResponseViewModel<List<Sp_BroughtBy_Job_ClientNamesModel>>> GetBroughtByDayWiseJobClientNamesAsync(int boughtBy, DayWiseBoughtByAccountSearchModel model);
        Task<GetResponseViewModel<List<Sp_BroughtBy_Job_ClientNamesModel>>> GetBroughtByInterviewClientNamesAsync(int boughtBy);

        Task<GetResponseViewModel<JobDescriptionViewModel>> GetJobDescriptionWithPipeline(int Id);

        Task<GetResponseViewModel<DashboardJobListViewModel>> JobsList(HireAssignmentsSearchViewModel model);

        Task<GetResponseViewModel<DashboardJobListViewModel>> DayWiseJobsList(DayWiseAssignmentsJobsSearchViewModel model);

        Task<GetResponseViewModel<DashboardCandidateInterviewViewModel>> GetCandidateInterviewsAsync(CandidateInterviewSearchViewModel filterViewModel);

        Task<GetResponseViewModel<List<CandidateStageWiseViewModel>>> GetCandidateStageWiseInfo(int JobId, int CandProfId);

        Task<GetResponseViewModel<_JobCandidatesBasedOnProfileStatusViewModel>> GetJobCandidatesBasedOnProfileStatusAsync(CandidateProfileStatusSearchViewModel filterViewModel);

        Task<UpdateResponseViewModel<string>> JobPriorityUpdateAsync(PriorityChangeViewModel model);

        Task<UpdateResponseViewModel<string>> JobRecruiterPriorityUpdateAsync(PriorityChangeViewModel model);

        Task<GetResponseViewModel<List<Repositories.PiplineCandidatesViewModel>>> GetDashboardJobBDMStageCandidatesAsync(int bdmId, int StageId);


        #endregion

        #region Reports

        Task<GetResponseViewModel<BDMsVM>> GetBDMsOverview(ReportRequestViewModel reportRequestViewModel);
        Task<GetResponseViewModel<BDMReportModel>> GetBDMOverview(ReportRequestViewModel reportRequestViewModel);
        Task<GetResponseViewModel<BDMOpeingReportModel>> GetBDMOpeningOverview(ReportRequestViewModel reportRequestViewModel);
        Task<GetResponseViewModel<RecruitersVM>> GetRecruitersOverview(ReportRequestViewModel reportRequestViewModel);
        Task<GetResponseViewModel<RecruiterReportModel>> GetRecruiterOverview(ReportRequestViewModel reportRequestViewModel);
        Task<GetResponseViewModel<RecruiterOpeningReportModel>> GetRecruiterOpeningOverview(ReportRequestViewModel reportRequestViewModel);
        Task<GetResponseViewModel<CandidatesSourceVM>> GetCandidatesSourceOverview(ReportRequestViewModel reportRequestViewModel);
        Task<GetResponseViewModel<SourcedCandidatesVM>> GetSourcedCandidates(ReportRequestViewModel reportRequestViewModel);
        Task<GetResponseViewModel<WebSourceRecruitersVM>> GetSourcedWebsiteCandidates(ReportRequestViewModel reportRequestViewModel);

        #endregion
    }
}
