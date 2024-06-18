using PiHire.BAL.Repositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace PiHire.BAL.IRepositories
{
    public interface ICandidateInterviewRepository : IBaseRepository
    {
       
        Task<GetResponseViewModel<ClientCandidateInterviewViewModel>> GetClientCandidatePreferences(int JobId, int CandProfId);
        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> ScheduleCandidateInterview(ScheduleCandidateInterview scheduleCandidateInterview);
        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> RejectCandidateInterview(CandidateInterviewRejectModel candidateInterviewRejectModel);
         Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ReScheduleCandidateInterview(ReScheduleScheduleCandidateInterview reScheduleScheduleCandidateInterview);

        Task<UpdateResponseViewModel<string>> CancelInterviewInvitation(CancelCandidateInterviewInterview cancelCandidateInterviewInterview);

        Task<UpdateResponseViewModel<string>> UpdateCandidateInterview(UpdateCandidateInterviewModel updateCandidateInterviewModel);
        Task<CreateResponseViewModel<string>> ShareProfilesToClient(CandidateShareProfileViewModel candidateShareProfileViewModel);
        Task<GetResponseViewModel<List<SharedProfileCandidateModel>>> GetClientSharedProfiles(string BatchId);
        Task<GetResponseViewModel<CandidateInterviewViewModel>> GetCandidateInterviewDtls(BacthProfilesModel bacthProfilesModel);
        Task<GetResponseViewModel<CandidateInterViewDtls>> GetCandidateInterviewDtlsToReSchedule(CandidateInterviewDtlsRequestViewModel candidateInterviewDtlsRequestViewModel);
        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> ShortlistCandidate(int ShareId);
        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> UpdateClientCandidatePreference(UpdateShareProfileTimeViewModel updateShareProfileTimeViewModel);
        Task<CreateResponseViewModel<string>> UpdateCandidateProfileClientViewStatus(int ShareId);
        

        Task<GetResponseViewModel<string>> getMailCredExistAsync();
        Task<GetResponseViewModel<JobMailCountViewModel>> getJobMailsCountAsync(int JobId, int CandidateId);
        Task<GetResponseViewModel<List<JobMailGroupViewModel>>> getJobMailsAsync(int JobId, int candidateId);
        Task<GetResponseViewModel<string>> setMailStatusReadedAsync(string messageId);
        Task<GetResponseViewModel<string>> sendJobMailsAsync(int JobId, int CandidateId, Common._3rdParty.Microsoft.SendMail.SendMail_ViewModel model, List<Microsoft.AspNetCore.Http.IFormFile> files);
        Task<GetResponseViewModel<string>> replyJobMailsAsync(int JobId, int CandidateId, string messageId, Common._3rdParty.Microsoft.ReplyMail.ReplyMail_ViewModel model, List<Microsoft.AspNetCore.Http.IFormFile> files);
        Task<GetResponseViewModel<string>> replyAllJobMailsAsync(int JobId, int CandidateId, string messageId, Common._3rdParty.Microsoft.ReplyAllMail.ReplyAllMail_ViewModel model, List<Microsoft.AspNetCore.Http.IFormFile> files);
    }
}
