using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class AppUserAuthorizationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public byte UserTypeId { get; set; }
        public GetUserDetailsViewModel UserDetails { get; set; }

    }

    public class AccessTokenViewModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
    }

    public class AppUserGoogleAuthenticateViewModel
    {
        public string iss { get; set; }
        public string sub { get; set; }
        public string azp { get; set; }
        public string aud { get; set; }
        public string iat { get; set; }
        public string exp { get; set; }
        public string email { get; set; }
        public string email_verified { get; set; }
        public string name { get; set; }
        public string picture { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string locale { get; set; }
    }

}
