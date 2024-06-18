using PiHire.BAL.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.IRepositories
{
    public interface IAccountRepository: IBaseRepository
    {
        Task<CandidateRegistrationRespViewModel> CandidateRegistration(CandidateRegistrationViewModel model);
        Task<bool> EmailVarify(string token);
        Task<(AuthenticateStatus, AuthorizationViewModel, string token)> Authenticate(AccountAuthenticate model, System.Net.IPAddress clientLocation);
        Task<CandidateRegistrationRespViewModel> CandidateRegistrationGoogle(CandidateRegistrationGoogleViewModel model);
        Task<CandidateRegistrationRespViewModel> CandidateRegistrationFacebook(CandidateRegistrationFacebookViewModel model);
        Task<(bool status, AuthorizationViewModel data, bool googleFlag)> AuthenticateGoogle(AccountAuthenticateGoogle model, System.Net.IPAddress clientLocation);
        Task<(bool status, AuthorizationViewModel data, bool facebookFlag)> AuthenticateFacebook(AccountAuthenticateFacebook model, System.Net.IPAddress clientLocation);
        Task<(string message, bool status)> ForgotPassword(ForgotPasswordRequest model);
        Task<(string message, bool status)> ResetPassword(ResetPasswordRequest model);
        Task<(List<GetUserCurrentStatusVM> data, bool status, string message)> GetUserCurrentStatus(OnlineStatusFilterViewModel filterViewModel);
        Task<(bool status, string message, string deviceId)> LogoutFromAllDevices(string token, string email);
        bool LoginTracking(int UserId,string Name,string LoggedInBy); 

    }
}
