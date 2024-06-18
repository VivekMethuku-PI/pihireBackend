using PiHire.BAL.Repositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IOpeningRepository : IBaseRepository
    {
        /// <summary>
        /// create Opening
        /// </summary>
        /// <returns></returns>
        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> CreateJobsync(CreateOpeningViewModel CreateOpeningViewModel);

        /// <summary>
        /// edit Opening
        /// </summary>
        /// <returns></returns>
        Task<UpdateResponseViewModel<string>> EditJob(EditOpeningViewModel editOpeningViewModel);

        /// <summary>
        /// get Opening
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<GetOpeningViewModel>> GetJobAsync(int Id);

        /// <summary>
        /// returns opening list
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<JobListViewModel>> JobsList(OpeningListSearchViewModel openingListSearchViewModel);

        Task<GetResponseViewModel<JobListViewModel>> JobsListToAssignRecruiters(JobsListToAssignRecruitersSearchViewModel jobsListToAssignRecruitersSearchViewModel);
        Task<GetResponseViewModel<RecruitersJobListToAssignViewModel>> RecruiterTodayAssignmentsAsync(JobsListToAssignRecruitersSearchViewModel searchViewModel);
        Task<GetResponseViewModel<RecruiterJobAssignmentSearchResponseViewModel>> RecruiterJobAssignmentDayWiseAsync_search(RecruiterJobAssignmentSearchViewModel srchModel);
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> RecruiterJobAssignmentDayWiseAsync(RecruiterJobAssignmentViewModel model);

        Task<GetResponseViewModel<TagJobModel>> GetJobsListToTagCandidate(TagJobListSearchViewModel TagJobListSearchViewModel);

        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> DeAssignJobToTeamMember(DeAssignJobViewmodel DeAssignJobViewmodel);

        //Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> AssignMultipleJobToTeamMember(MultipleJobAssignmentMembersViewModel multipleJobAssignmentMembersViewModel);

        //Task<GetResponseViewModel<string>> SlefAssignJob(int jobId);
        /// <summary>
        /// returns updated status
        /// </summary>
        /// <returns></returns>
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ReOpenJob(int Id);


        /// <summary>
        /// returns updated status
        /// </summary>
        /// <returns></returns>
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> HoldJob(int Id);

        /// <summary>
        /// returns updated status
        /// </summary>
        /// <returns></returns>
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateJobStatus(UpdateOpeningViewModel updateOpeningViewModel);

        /// <summary>
        /// returns updated status
        /// </summary>
        /// <returns></returns>
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> CloseJob(int Id);

        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> AddMoreCVPerJob(MoreCVPerJobViewModel moreCVPerJobViewModel);

        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> AddMoreCVPerJobRecruiter(MoreCVPerJobRecruiterViewModel model);

        /// <summary>
        /// returns updated status
        /// </summary>
        /// <returns></returns>
        Task<CreateResponseViewModel<CloneOpeningViewModel>> CloneJob(int Id);

        Task<GetResponseViewModel<JobInfoViewModel>> GetJobInfo(int Id);

        Task<GetResponseViewModel<List<UsersViewModel>>> GetJobAssociatedPannel(int JobId);

        Task<GetResponseViewModel<List<AssignedJobsViewModel>>> AssignedJobs(AssignedJobsReqViewModel assignedJobsViewModel);

        Task<GetResponseViewModel<JobAssignmentViewModel>> GetTodayJobAssignments();

        Task<GetResponseViewModel<GetOpeningViewModel>> GetPortalJobs(int Id);

        #region Notes 

        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> AddJobNote(CreateJobNotesViewModel createJobNotesViewModel);

        Task<GetResponseViewModel<JobNotesViewModel>> GetJobNotes(int JobId, int CandId);

        Task<DeleteResponseViewModel<string>> DeleteJobNote(int id);

        #endregion

        #region  Job View

        Task<GetResponseViewModel<JobDescriptionViewModel>> GetJobDescription(int Id);

        Task<GetResponseViewModel<List<JobAssessmentViewModel>>> GetJobAssessments(int JobId);

        Task<CreateResponseViewModel<string>> MapAssessmenttoJob(MapAssessmentToJobViewModel mapAssessmentViewModel);

        Task<GetResponseViewModel<List<GetJobAssignedTeamMembersViewModel>>> GetJobTeamMembers(int JobId);

        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> AssignJobToTeamMember(JobAssignedMembersViewModel jobAssignedMembersViewModel);

        Task<GetResponseViewModel<GetJobPiplineViewModel>> GetJobPipeline(int JobId);


        #endregion

        #region Activities 

        Task<GetResponseViewModel<JobActivities>> GetJobActivities(int JobId);

        #endregion

        #region Website 

        /// <summary>
        /// returns Opening list
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<PortalJobModel>> GetPortalJobsList(PortalJobSearchViewModel portalJobSearchViewModel);

        Task<GetResponseViewModel<List<CountryWiseJobCountModel>>> GetCountryWiseJobCounts();

        Task<GetResponseViewModel<List<LocationWiseJobCountModel>>> GetLocationWiseJobCounts(int CountryId);

        Task<GetResponseViewModel<CandidateActiveArchivedJobViewModel>> GetCandidateActiveArchivedJobs(int CanPrfId, int FilterType);

        Task<GetResponseViewModel<List<CandidateArchivedJobModel>>> GetCandidateArchivedJobs(int CanPrfId);

        Task<GetResponseViewModel<CandidateSimilarJobModel>> GetCandidateSimilarJobs(CandidateSimilarJobSearchViewModel candidateSimilarJobSearchViewModel);

        #endregion

        #region  Client view 

        Task<GetResponseViewModel<List<ClientSharedCandidatesModel>>> GetCandidatesSharedToClient(int JobId, int type);

        #endregion
    }
}
