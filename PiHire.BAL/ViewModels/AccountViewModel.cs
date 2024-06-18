using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class CandidateRegistrationViewModel
    {
        [Required]
        public string EmailId { get; set; }
        [Required]
        public string Password { get; set; }
        public int? JobId { get; set; }
        public int? SfId { get; set; }
    }

    public class CandidateRegistrationGoogleViewModel
    {
        [Required]
        public string EmailId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Token { get; set; }
        public int? JobId { get; set; }
        public int? SfId { get; set; }

    }

    public class CandidateRegistrationFacebookViewModel
    {
        [Required]
        public string EmailId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Token { get; set; }
        public int? JobId { get; set; }
        public int? SfId { get; set; }

    }

    public class CandidateRegistrationRespViewModel
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public int CandidateId { get; set; }
    }

    public class AccountAuthenticate
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class LogoutDevicesVm
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string Email { get; set; }
    }

    public class AccountAuthenticateGoogle
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Token { get; set; }
    }

    public class AccountAuthenticateFacebook
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Token { get; set; }
    }

    public class AuthorizationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public byte UserTypeId { get; set; }
        public bool IsNewJob { get; set; }
        public GetUserDetailsViewModel UserDetails { get; set; }
        public string SessionTxnId { get; set; }
    }

    public class EmailVerifyViewModel
    {
        [Required]
        public string Token { get; set; }
    }


}
