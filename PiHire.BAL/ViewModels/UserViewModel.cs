using Microsoft.AspNetCore.Http;
using PiHire.BAL.Common.Attribute;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.ViewModels
{
    public class GetUsersViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<RoleList> Roles { get; set; }
        public string Email { get; set; }
        public byte Status { get; set; }
        public byte UserType { get; set; }
        public string MobileNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Nationality { get; set; }
        public List<UserBUPU> UserBUPUs { get; set; }
    }




    public class AppUserViewModel
    {
        public int Id { get; set; }
        public byte UserType { get; set; }
        public int? EmployId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string UserName { get; set; }
        public int? UserRoleId { get; set; }
        public string UserRoleName { get; set; }
        public int? LocationId { get; set; }
        public string Location { get; set; }
        public string Nationality { get; set; }
        public string EmailSignature { get; set; }
        public int? ShiftId { get; set; }
        public string ProfilePhoto { get; set; }
    }

    public class UserBUPU
    {
        public int PuId { get; set; }
        public int? BuId { get; set; }
        public string PU { get; set; }
        public string BU { get; set; }
    }

    public class RoleList
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UsersbyTypeRequestModel
    {
        public int? PuId { get; set; }
        public List<int> UserTypes { get; set; }

    }
    public class UpdateUserViewModel
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNo { get; set; }
        public int RoleId { get; set; }
        public string Role { get; set; }
        public byte UserType { get; set; }
        public string Nationality { get; set; }
        [Required]
        public int ShiftId { get; set; }
        [Required]
        public int LocationId { get; set; }
        [Required]
        [MaxLength(100)]
        public string LocationName { get; set; }
        public List<UserRequestPUBU> PuBuId { get; set; }
    }
    public class CreateUserRequestViewModel
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }


        public List<UserRequestPUBU> puBu { get; set; }

        public byte UserType { get; set; }
        public string Nationality { get; set; }
        public int? EmployeeId { get; set; }


        public int RoleId { get; set; }
        public int ApplicationId { get; set; }
        public string Role { get; set; }
        public int CreatedBy { get; set; }


        [Required]
        public int ShiftId { get; set; }
        [Required]
        public int LocationId { get; set; }
        [Required]
        [MaxLength(100)]
        public string LocationName { get; set; }
    }


    public class OddoCreateUserRequestViewModel
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }


        public byte UserType { get; set; }
        public string Nationality { get; set; }
        public int EmployeeId { get; set; }
        public int UserId { get; set; }
        public DateTime Dob { get; set; }
        public List<UserRequestPUBU> puBu { get; set; }

        public int? ApplicationId { get; set; }
    }

    public class OdooUserRequestViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNo { get; set; }


        public byte UserType { get; set; }
        public int UserId { get; set; }
        public List<UserRequestPUBU> puBu { get; set; }
    }

    public class UserExistModel
    {
        [Required]
        public string UserName { get; set; }
    }



    public class CreatePiHireUserResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
    }

    public class CreateUserResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public GWUserViewModel user { get; set; }
    }

    public class UpdateUserResponseViewModel
    {
        public int UserId { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class GWUserViewModel
    {
        public int ID { get; set; }
        public int PUID { get; set; }
        public int BUID { get; set; }
        public int ApplicationID { get; set; }
        public byte UserType { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string MobileNumber { get; set; }
        public byte Status { get; set; }
        public string Nationality { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public int? EmployID { get; set; }
        public string EmailID { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public DateTime? DOB { get; set; }
    }

    public class UserRequestPUBU
    {
        public int PUID { get; set; }
        public int BUID { get; set; }
    }

    public class GetProfileDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ProfileRespViewModel> ProfileResps { get; set; }
    }

    public class UpdateRolePermissionsViewModel
    {
        public int RoleId { get; set; }
        public int ApplicationId { get; set; }
        public string Role { get; set; }
        public string RoleDescription { get; set; }
        public List<UpdatePermissionsViewModel> Permissions { get; set; }
    }

    public class UpdateProfilePermisRespVM
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class UpdatePermissionsViewModel
    {
        public int ModuleId { get; set; }
        public int TaskId { get; set; }
        public string Activities { get; set; }
        public bool ActivitiesFlag { get; set; }

    }

    public class GetTasksModel
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Task { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Activities { get; set; }
        public bool ActivityFlag { get; set; }
        public bool IsValid { get; set; }
    }

    public class ProfileRespViewModel
    {
        public int ModuleId { get; set; }
        public int TaskId { get; set; }
        public string Permissions { get; set; }
        public string Module { get; set; }
        public string Task { get; set; }
        public string ModuleDesc { get; set; }
        public string TaskDesc { get; set; }
        public bool ActivityFlag { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string TaskPermissions { get; set; }
        public string RoleName { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string TaskCode { get; set; }
    }

    public class GetProfilesViewModel
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
        public byte UserType { get; set; }
    }
    public class UpdateUserPermissionsVM
    {
        public int UserId { get; set; }
        public int ApplicationId { get; set; }
        public List<UpdatePermissionsViewModel> Permissions { get; set; }
    }

    public class UpdateUserPermisRespVM
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }


    public class CreateRoleViewModel
    {
        public int ApplicationId { get; set; }
        public string Role { get; set; }
        public string RoleDescription { get; set; }
        public bool IsClone { get; set; }
        public int? CloneRoleId { get; set; }
        public byte UserType { get; set; }

    }

    public class GetUserDetailsViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
        public string UserType { get; set; }
        public byte UserTypeId { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string Location { get; set; }
        public int? LocationId { get; set; }
        public string Timezone { get; set; }
        public DateTime? Dob { get; set; }
        public string ProfilePhoto { get; set; }
        public List<ProfileRespViewModel> Permission { get; set; }
        public List<PUBUList> UserPubuList { get; set; }
    }

    public class ProfilesViewModel
    {
        public int ApplicationId { get; set; }
        public List<int> UserId { get; set; }
    }

    public class GWUserProfileViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte UserType { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public List<PUBUList> UserPubuList { get; set; }
    }

    public class HireUserProfileViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserType { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public byte Status { get; set; }
        public string Location { get; set; }
        public int? LocationId { get; set; }
        public int UserId { get; set; }
        public string Nationality { get; set; }
        public List<PUBUList> UserPubuList { get; set; }
    }

    public class PUBUList
    {
        public int PuId { get; set; }
        public int? BuId { get; set; }
        public string PuName { get; set; }
        public string BuName { get; set; }
        public int CountryId { get; set; }
    }

    public class PUList
    {
        public int PuId { get; set; }
        public string PuFullName { get; set; }
        public string PuShortName { get; set; }
        public int CountryId { get; set; }
    }

    public class UserProfilePhotoViewModel
    {
        [Required]
        public int UserId { get; set; }

        [MaxFileSize((byte)FileType.File)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg" })]
        public IFormFile Photo { get; set; }
    }

    public class UpdateUserStatusViewModel
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public byte Status { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
    }

    public class UpdateUserStatusResponseViewModel
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class DisconnectUserViewModel
    {
        public string ConnectionId { get; set; }
    }

    public class UpdateLogsViewModel
    {
        public string IpAddress { get; set; }
        public string Lat { get; set; }
        public string Longi { get; set; }
        public string DeviceUiId { get; set; }
        public byte DeviceType { get; set; }
        public int UserId { get; set; }
        public string SessionId { get; set; }

        public bool LoginStatus { get; set; }
    }

    public class UserAuthorizationViewModel_UserDetail
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleName { get; set; }
        public string UserType { get; set; }
        public byte UserTypeId { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        //public string Location { get; set; }
        //public int? LocationId { get; set; }
        public string Timezone { get; set; }
        //public DateTime? Dob { get; set; }
        //public string ProfilePhoto { get; set; }
        //public List<UserAuthorizationViewModel_UserDetail_Permission> Permission { get; set; }
        //public List<UserAuthorizationViewModel_UserDetail_UserPubuList> UserPubuList { get; set; }
    }
    public class UserAuthorizationViewModel_UserDetail_UserPubuList
    {
        public int PuId { get; set; }
        public int? BuId { get; set; }
        public string PuName { get; set; }
        public string BuName { get; set; }
        public int CountryId { get; set; }
    }
    public class UserAuthorizationViewModel_UserDetail_Permission
    {
        public int ModuleId { get; set; }
        public int TaskId { get; set; }
        public string Permissions { get; set; }
        public string Module { get; set; }
        public string Task { get; set; }
        public string ModuleDesc { get; set; }
        public string TaskDesc { get; set; }
        public bool ActivityFlag { get; set; }
        public string TaskPermissions { get; set; }
        public string RoleName { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string TaskCode { get; set; }
    }
    public class UserTokenAuthorizationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int? EmpId { get; set; }
        public byte UserTypeId { get; set; }
        public string ProfilePhoto { get; set; }
        public string RoleName { get; set; }
        public UserAuthorizationViewModel_UserDetail UserDetails { get; set; }
        public string SessionTxnId { get; set; }

        //microsoft login
        public string OutlookTokenCode { get; set; } = string.Empty;
        public bool isSmtpWorking { get; set; }
        public void setMicrosoftToken_jwt(UserAuthorizationViewModel src)
        {
            this.OutlookTokenCode = src.OutlookTokenCode;
            this.isSmtpWorking = src.isSmtpWorking;
        }
    }
    public class UserAuthorizationViewModel: UserTokenAuthorizationViewModel
    {
        static Dictionary<string, (string MicrosoftToken, string MicrosoftUserName)> MicrosoftTokens = new Dictionary<string, (string MicrosoftToken, string MicrosoftUserName)>();
        //microsoft login
        public string MicrosoftToken
        {
            get
            {
                if (MicrosoftTokens.ContainsKey(OutlookTokenCode))
                {
                    return MicrosoftTokens[OutlookTokenCode].MicrosoftToken;
                }
                else
                    return string.Empty;
            }
        }
        public string MicrosoftUserName
        {
            get
            {
                if (MicrosoftTokens.ContainsKey(OutlookTokenCode))
                {
                    return MicrosoftTokens[OutlookTokenCode].MicrosoftUserName;
                }
                else
                    return string.Empty;
            }
        }
        public void setMicrosoftToken(string MicrosoftToken, string MicrosoftUserName, bool isSmtpWorking)
        {
            do
            {
                this.OutlookTokenCode = Guid.NewGuid().ToString();
            } while (MicrosoftTokens.ContainsKey(this.OutlookTokenCode));
            MicrosoftTokens.Add(this.OutlookTokenCode, (MicrosoftToken, MicrosoftUserName));
            this.isSmtpWorking = isSmtpWorking;
        }
    }
    public class AuthenticationOutlookRequestViewModel
    {
        public string RequestUrl { get; set; }
        public string RequestCode { get; set; }
    }

    public class GWAppLoginViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public int ApplicationId { get; set; }
    }

    public class UpdateSignatureViewModel
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
    }



    public class GetPUBUViewModel
    {
        public List<GetPUViewModel> PUs { get; set; }
        public List<GetBUViewModel> BUs { get; set; }
    }

    public class UpdateSignatureRequestViewModel
    {
        public string Signature { get; set; }
        public int UserId { get; set; }
    }
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class GWResetPasswordViewModel
    {
        public int ApplicationId { get; set; }
        public string userName { get; set; }
        public string Password { get; set; }
        public string EncryptedToken { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }


    public class GetUserCurrentStatusVM
    {
        public int UserId { get; set; }
        public bool Status { get; set; }
        public string EmailId { get; set; }
    }

}
