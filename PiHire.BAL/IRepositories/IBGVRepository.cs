using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.IRepositories
{
    public interface IBGVRepository : IBaseRepository
    {

        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateCandidateInfo(UpdateCandidateInfoViewModel updateCandidateInfoViewModel);
        Task<GetResponseViewModel<List<RecruiterViewModel>>> GetRecruiterDetails(int CandId, int jobId);
        Task<GetResponseViewModel<CandidateInfoViewModel>> GetCandidateInfo(string EmailId);

        Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> SaveCandidateBGV(SaveCandidateBGVViewModel saveCandidateBGVViewModel);

        Task<GetResponseViewModel<CandidateBGVViewModel>> GetCandidateBGVDtls(int CanId, int JobId);

        Task<CreateResponseViewModel<string>> SaveCandidateEduBGV(SaveCandidateBGVEduViewModel saveCandidateBGVEduViewModel);

        Task<CreateResponseViewModel<string>> SaveCandidateEmpBGV(SaveCandidateBGVEmpViewModel saveCandidateBGVEmpViewModel);

        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> AcceptCandidateBGV(AcceptCandidateBGVViewModel acceptCandidateBGVViewModel);

        Task<GetResponseViewModel<FileURLViewModel>> DownloadCandidateBGVForm(int CandId, int JobId);

        Task<GetResponseViewModel<FileURLViewModel>> DownloadCandidateAcknowledgementForm(AcknowledgementDwndViewModel acknowledgementDwndViewModel);

        Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ConvertToEmployee(int CanId, int JobId);

        Task<GetResponseViewModel<CandidateEmploymentEducationCertificationViewModel>> GetCandidateEmpEduCertVDtls(int CanId);

        Task<dynamic> Odoologin();
    }
}
