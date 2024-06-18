using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.Common.Types;
using PiHire.BAL.Repositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Entities;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.IRepositories
{
    public interface ICandidateRepository : IBaseRepository
    {
        #region Candidate Portal
        Task<GetResponseViewModel<GetJobCandidatePortalViewModel>> GetJobCandidateAsync(int CandId, int JobId);
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateJobCandidateAsync(UpdateJobCandidatePortalViewModel candidateModel);
        #endregion
        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> CreateCandidate(CreateCandidateViewModel createCandidateViewModel);
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateCandidate(UpdateCandidateViewModel updateCandidateViewModel);

        #region Candidate listing actions

        Task<GetResponseViewModel<GetCandidateViewModel>> GetCandidate(int Id, int JobId);
        Task<GetResponseViewModel<CandidateListModel>> CandidateList(CandidateListSearchViewModel candidateListSearchViewModel);
        Task<GetResponseViewModel<JobCandidateListModel>> JobCandidateList(CandidateListSearchViewModel candidateListSearchViewModel);
        Task<GetResponseViewModel<JobCandidateListFilterDataViewModel>> JobCandidateListFilterData(int? JobId = null);
        Task<GetResponseViewModel<JobCandidateListFilterDataViewModel>> SuggestCandidateListFilterData(int JobId);
        Task<GetResponseViewModel<JobCandidateListModel>> SuggestCandidateList(SuggestCandidateListSearchViewModel suggestCandidateListSearchViewModel);
        Task<GetResponseViewModel<List<CandidatesViewModel>>> GetExportCandidates(CandidateExportSearchViewModel candidateExportSearchViewModel);
        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> MapCandidateToJob(MapCandidateViewModel mapCandidateViewModel);
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateCandidatePrfStatus(UpdateCandidatePrfStatusViewModel updateCandidatePrfStatusViewModel);
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateCandidatePrfStatus(UpdateCandidateCVStatusViewModel updateCandidateCVStatusViewModel);
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateCandidateOtherStatus(CandidateOtherStatusViewModel candidateOtherStatusViewModel);

        #endregion

        #region  Candidate view actions

        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ValidateCandidateSalary(ValidateCanJobSalaryViewModel validateCanJobSalaryViewModel);
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ReOfferApprove(ReOfferApproveViewModel reOfferApproveViewModel);
        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ReofferPackage(ReofferPackageViewModel reofferPackageViewModel);
        Task<GetResponseViewModel<CandidateJobOfferDtlsViewModel>> GetCandidateJobPackageDtls(int CandId, int jobId);
        Task<GetResponseViewModel<CandidateSkillViewModel>> GetCandidateSkills(int CandId, int JobId);
        Task<GetResponseViewModel<CandidateOverViewModel>> GetCandidateOverview(int CandId, int JobId);
        Task<GetResponseViewModel<List<CandidateJobsViewModel>>> GetCandidateTagJobs(int CandId);
        Task<GetResponseViewModel<JobActivities>> GetCandidateActivities(int JobId,int CanPrfId);
        Task<GetResponseViewModel<List<CandidateTagsViewModel>>> GetCandidateTags(int CandidateId);
        Task<CreateResponseViewModel<string>> CreateCandidateTag(CreateTag createTag);
        Task<DeleteResponseViewModel<string>> DeleteCandidateTag(int TagId);

        Task<CreateResponseViewModel<string>> CandidateStatusReview(CandidateStatusReviewViewModel model);
        Task<GetResponseViewModel<List<CandidateStatusReviewListViewModel>>> CandidateStatusReviewList(int JobId, int CanPrfId);
        #endregion

        #region Candidate qualifications and certifications and employment 

        Task<CreateResponseViewModel<string>> CreateCandidateCertification(CreateCandidateCertificationModel createCandidateCertificationModel);

        #endregion

        #region Candidate documents actions

        Task<GetResponseViewModel<CandidateFilesViewModel>> GetCandidateResume(ResumeViewModel resumeViewModel);
        Task<GetResponseViewModel<CandidateFilesViewModel>> GetCandidateVideoProfile(ResumeViewModel resumeViewModel);
        Task<CreateResponseViewModel<string>> UpdateCandidateFile(UpdateCandidateFileViewModel uploadCandidateFileViewModel);
        Task<GetResponseViewModel<List<CandidateFilesViewModel>>> GetCandidateFiles(int CandId, int JobId);
        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> UploadCandidateFiles(UploadCandidateFileViewModel uploadCandidateFileViewModel);
        Task<CreateResponseViewModel<string>> CandidateFileApproveReject(CandidateFileApproveRejectViewModel candidateFileApproveRejectViewModel);
        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> CandidateDocumentRequest(CandidateDocumentRequestViewModel candidateDocumentRequestViewModel);
        Task<GetResponseViewModel<List<CandidateDocumentsModel>>> GetCandidateRequestedFiles(int CandPrfId, int JobId);

        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> CandidateFinalCVReject(CandidateFinalCVRejectViewModel candidateFinalCVRejectViewModel);

        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> CandidateMultiDocumentRequest(CandidateMultiDocumentRequestViewModel candidateDocumentRequestViewModel);

        #endregion      

        #region Candidate rating actions

        Task<GetResponseViewModel<CandidateCSATViewModel>> GetTeamCandidateRating(int JoId, int CandProfId);
        Task<GetResponseViewModel<CandidateCSATViewModel>> GetClientCandidateRating(int JoId, int CandProfId);
        Task<GetResponseViewModel<CandidateCSATViewModel>> GetCandidateJobEvaluationSummary(int JoId, int CandProfId);
        Task<GetResponseViewModel<CandidateCSATViewModel>> GetCandidateOverallEvaluationSummary(int CandProfId);
        Task<CreateResponseViewModel<string>> UpdateCandidateRating(UpdateCandidateRatingModel candidateRatingModel);

        #endregion      

        #region Candidate Assessment actions

        Task<CreateResponseViewModel<string>> CreateCandidateAssessment(CreateCandidateAssessmentViewModel createCandidateAssessmentViewModel);
        Task<GetResponseViewModel<List<CandidateAssessmentViewModel>>> CandidateAssessments(int CandId, int JobId);
        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> MapAssessmentResponse(PostVM postVM);

        #endregion

        #region   Unsubscribe Emails

        Task<UpdateResponseViewModel<string>> Unsubscribe(CandidateUnSubscribeRequestModel candidateUnSubscribeRequestModel);

        #endregion

        #region   Currency Exchange
        GetResponseViewModel<string> CurrConv(CurrConvViewModel currConvViewModel);
        #endregion

        #region Resend User Cred
        Task<UpdateResponseViewModel<string>> ResendUserCred(ResendUserCredViewModel resendUserCredViewModel);

        #endregion


        Task<CreateResponseViewModel<string>> UpdateCandidateRecruiter(UpdateCandidateRecModel candidateRatingModel);
        Task<GetResponseViewModel<List<CandidateRecHistoryViewModel>>> GetCandidateRecHistory(int JoId, int CandProfId);
    }
}

