using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.IRepositories
{
    public interface IUserRespository : IBaseRepository
    {
        //Task<CreatePiHireUserResponse> CreateUser(CreateUserRequestViewModel model);
        Task<CreateUserResponse> UpdateUser(UpdateUserViewModel model);
        Task<List<GetProfilesViewModel>> GetProfiles();
        Task<GetResponseViewModel<int>> OdooCreateUpdateUser(OddoCreateUserRequestViewModel model);

        Task<UpdateUserPermisRespVM> UpdateUserPermissionsAsync(UpdateUserPermissionsVM model);

        Task<UpdateProfilePermisRespVM> UpdateProfilePermissionsAsync(UpdateRolePermissionsViewModel model);

        Task<GetProfileDetailsViewModel> GetProfileDetailsAsync(int ProfileId);

        Task<GetResponseViewModel<List<RecsViewModel>>> GetRecruiters();

        Task<GetResponseViewModel<List<HireUserProfileViewModel>>> GetUserProfiles(int PuId);

        Task<List<GetUsersViewModel>> GetAllUsersAsync();
        Task<GetResponseViewModel<List<UsersViewModel>>> GetUsersbyType(List<int> Type, int? puId);

        Task<GetResponseViewModel<List<UsersViewModel>>> GetSignatureAuthorityUsers();

        Task<GetResponseViewModel<List<UsersViewModel>>> GetUsers(int puId);

        Task<List<AppUserViewModel>> GetAppUsers();

        Task<UpdateSignatureViewModel> UpdateSignature(UpdateSignatureRequestViewModel model);

        Task<GetPUBUViewModel> GetUserPUBU(int userId);

        Task<bool> CreateRoleAsync(CreateRoleViewModel model);

        #region - Module & Tasks 
        /// <summary>
        /// returns modules of hire 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<GetModuleModel>>> GetModules();
        /// <summary>
        /// returns tasks 
        /// </summary>
        /// <returns></returns>
        Task<GetResponseViewModel<List<GetTasksModel>>> GetTasks(int? ModuleId);
        #endregion

        Task<GetUserDetailsViewModel> GetUserDetails(int UserId);

        Task<UpdateUserStatusResponseViewModel> UpdateUserStatus(UpdateUserStatusViewModel model);

        Task<UpdateResponseViewModel<string>> UpdateUserStatus(int UserId, RecordStatus Status);

        Task<GetResponseViewModel<int>> IsUserExists(UserExistModel userExistModel);

        Task<GetResponseViewModel<Tuple<string, bool>>> UpdateUserProfilePhoto(UserProfilePhotoViewModel userProfilePhotoViewModel);

        Task<(UserAuthorizationViewModel data, string msg, AuthenticateStatus loginStatus, string token, List<UserAuthorizationViewModel_UserDetail_Permission> permissions, List<UserAuthorizationViewModel_UserDetail_UserPubuList> pubuLists)> authenticate(string username, string password);
        Task<(UserAuthorizationViewModel data, string msg, AuthenticateStatus loginStatus, string token)> OdooAuthenticate(string username, string password);
        #region Microsoft Outlook
        GetResponseViewModel<AuthenticationOutlookRequestViewModel> authenticateMicrosoftRequest();
        Task<(bool IsCompleted, bool IsSuccess, UserAuthorizationViewModel data, string msg, List<UserAuthorizationViewModel_UserDetail_Permission> permissions, List<UserAuthorizationViewModel_UserDetail_UserPubuList> pubuLists)> authenticateMicrosoft(string RequestCode, System.Net.IPAddress clientLocation);
        Task<string> authenticateMicrosoftSetToken(string code, string state, string session_state, string error);
        #endregion
        Task<(string message, bool status)> UpdateUserLogs(UpdateLogsViewModel model);
        Task<(string message, bool status)> DisconnectUser(string connectionId);
        Task<(string message, bool status)> ForgotPassword(ForgotPasswordRequest model);
        Task<(string message, bool status)> ResetPassword(ResetPasswordRequest model);
        Task<(string message, bool status)> PasswordReset(int EmpId);
        Task<List<(int userId, string email)>> DisconnectSessionOutUsers();
    }
}
