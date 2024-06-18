using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PiHire.BAL.Common;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using PiHire.DAL.Entities;
using PiHire.Utilities.Communications.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class AccountRepository : BaseRepository, IAccountRepository
    {
        readonly Logger logger;
        
        
        public AccountRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<CandidateRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }

        private string RandomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        public async Task<(AppUserGoogleAuthenticateViewModel data, bool status)> ValidateGoogleTokenAsync(string id_token)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://oauth2.googleapis.com/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync("tokeninfo?id_token=" + id_token);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = response.Content.ReadAsStringAsync().Result;
                        var retModel = JsonConvert.DeserializeObject<AppUserGoogleAuthenticateViewModel>(data);
                        return (retModel, true);
                    }
                    else
                    {
                        return (null, false);
                    }
                }
            }
            catch (Exception ex)
            {
                return (null, false);
            }
        }

        public async Task<(AccessTokenViewModel data, bool status)> ValidateFacebookTokenAsync(string id_token)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://graph.facebook.com/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync("oauth/access_token?client_id=1012220919138455&client_secret=f5c3dec2ace49ee29f02a48714670f39&grant_type=client_credentials");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = response.Content.ReadAsStringAsync().Result;
                        var retModel = JsonConvert.DeserializeObject<AccessTokenViewModel>(data);
                        return (retModel, true);
                    }
                    else
                    {
                        return (null, false);
                    }
                }
            }
            catch (Exception ex)
            {
                return (null, false);
            }
        }

        public async Task<CandidateRegistrationRespViewModel> CandidateRegistrationGoogle(CandidateRegistrationGoogleViewModel model)
        {
            try
            {
                using (var trans = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var validate = await ValidateGoogleTokenAsync(model.Token);
                        if (!validate.status)
                        {
                            return new CandidateRegistrationRespViewModel { Status = false, CandidateId = 0, Message = "Unable to authorized google account" };
                        }
                        var isuserNameAvai = await dbContext.PiHireUsers.AnyAsync(f => f.UserName == model.EmailId.ToLower().Trim());
                        if (isuserNameAvai)
                        {
                            return new CandidateRegistrationRespViewModel { Status = false, CandidateId = 0, Message = "Email address is already in use." };
                        }
                        var generator = new RandomGenerator();
                        var pswd = generator.RandomPassword(8);
                        var HashPswd = Hashification.SHA(pswd);
                        var user = new PiHireUser
                        {
                            UserType = (byte)UserType.Candidate,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            UserName = model.EmailId,
                            PasswordHash = HashPswd,
                            CreatedBy = 0,
                            CreatedDate = DateTime.UtcNow,
                            VerifiedFlag = true,
                            VerifyToken = null,
                            Status = (byte)RecordStatus.Active,
                            EmailId = model.EmailId
                        };
                        dbContext.PiHireUsers.Add(user);
                        await dbContext.SaveChangesAsync();

                        string redirectURL = appSettings.AppSettingsProperties.CandidateAppUrl + "/login";
                        if (model.JobId != null)
                        {
                            redirectURL = appSettings.AppSettingsProperties.CandidateAppUrl + "/login?jobid=" + model.JobId + "&sf=" + (byte)SourceType.Google + "";
                        }

                        var mailBody = EmailTemplates.User_EmailCredentials_Template(user.FirstName, user.UserName, pswd,
                            redirectURL, this.appSettings.AppSettingsProperties.HireAppUrl, this.appSettings.AppSettingsProperties.HireApiUrl);

                        SmtpMailing smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        appSettings.smtpEmailConfig.SmtpLoginName, appSettings.smtpEmailConfig.SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl,
                        appSettings.smtpEmailConfig.SmtpFromEmail, appSettings.smtpEmailConfig.SmtpFromName);
                        _ = smtp.SendMail(user.UserName, EmailTemplates.GetSubject(EmailTypes.AppUser_Create), mailBody, string.Empty);

                        trans.Commit();
                        return new CandidateRegistrationRespViewModel
                        {
                            CandidateId = user.Id,
                            Message = "Account created successfully.",
                            Status = true
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new CandidateRegistrationRespViewModel
                        {
                            CandidateId = 0,
                            Message = ex.Message,
                            Status = false
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<CandidateRegistrationRespViewModel> CandidateRegistration(CandidateRegistrationViewModel model)
        {
            try
            {
                using (var trans = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var isValidReg = await dbContext.PiHireUsers.Where(f => f.UserName == model.EmailId.ToLower().Trim() && f.VerifiedFlag == false).FirstOrDefaultAsync();
                        if (isValidReg != null)
                        {
                            var createdDate = isValidReg.CreatedDate.AddHours(24);
                            if (createdDate > CurrentTime)
                            {
                                return new CandidateRegistrationRespViewModel { Status = false, CandidateId = 0, Message = "This Email address is already in use. Please verify your email to continue services" };
                            }
                            else
                            {
                                dbContext.PiHireUsers.Remove(isValidReg);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                        var isuserNameAvai = await dbContext.PiHireUsers.AnyAsync(f => f.UserName == model.EmailId.ToLower().Trim());
                        if (isuserNameAvai)
                        {
                            return new CandidateRegistrationRespViewModel { Status = false, CandidateId = 0, Message = "Email address is already in use." };
                        }
                        var generator = new RandomGenerator();
                        var HashPswd = Hashification.SHA(model.Password);
                        string Name = string.Empty;
                        var data = model.EmailId.Split("@");
                        if (data.Length > 0)
                        {
                            Name = data[0];
                        }
                        var user = new PiHireUser
                        {
                            UserType = (byte)UserType.Candidate,
                            FirstName = Name,
                            UserName = model.EmailId,
                            EmailId = model.EmailId,
                            PasswordHash = HashPswd,
                            CreatedBy = 0,
                            CreatedDate = DateTime.UtcNow,
                            VerifiedFlag = false,
                            VerifyToken = RandomTokenString(),
                            Status = (byte)RecordStatus.Active
                        };
                        dbContext.PiHireUsers.Add(user);
                        await dbContext.SaveChangesAsync();

                        string redirectURL = appSettings.AppSettingsProperties.CandidateAppUrl + "/login?token=" + user.VerifyToken;
                        if (model.JobId != null)
                        {
                            redirectURL = appSettings.AppSettingsProperties.CandidateAppUrl + "/login?token=" + user.VerifyToken + "&jobid=" + model.JobId + "";

                            if (model.SfId != null)
                            {
                                redirectURL = appSettings.AppSettingsProperties.CandidateAppUrl + "/login?token=" + user.VerifyToken + "&jobid=" + model.JobId + "&sf=" + model.SfId + "";
                            }
                        }

                        var mailBody = EmailTemplates.Candidate_EmailVerification_Template(user.FirstName + " " + user.LastName, redirectURL,
                        this.appSettings.AppSettingsProperties.HireAppUrl);
                        SmtpMailing smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        appSettings.smtpEmailConfig.SmtpLoginName, appSettings.smtpEmailConfig.SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl,
                        appSettings.smtpEmailConfig.SmtpFromEmail, appSettings.smtpEmailConfig.SmtpFromName);
                        _ = smtp.SendMail(user.UserName, EmailTemplates.GetSubject(EmailTypes.CandidateEmailVarification), mailBody, string.Empty);

                        trans.Commit();
                        return new CandidateRegistrationRespViewModel
                        {
                            CandidateId = user.Id,
                            Message = "Account created successfully",
                            Status = true
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new CandidateRegistrationRespViewModel
                        {
                            CandidateId = 0,
                            Message = ex.Message,
                            Status = false
                        };
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> EmailVarify(string token)
        {
            try
            {
                var user = await dbContext.PiHireUsers.FirstOrDefaultAsync(f => f.VerifyToken == token);
                if (user == null)
                {
                    return false;
                }
                var createdDate = user.CreatedDate.AddHours(24);
                if (createdDate < CurrentTime)
                {
                    return false;
                }
                user.VerifiedFlag = true;
                user.VerifyToken = null;
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(AuthenticateStatus, AuthorizationViewModel, string token)> Authenticate(AccountAuthenticate model, System.Net.IPAddress clientLocation)
        {
            try
            {
                var HashPswd = Hashification.SHA(model.Password);
                var user = await dbContext.PiHireUsers.FirstOrDefaultAsync(f => f.UserName == model.Username && f.PasswordHash == HashPswd && f.VerifiedFlag && f.Status == (byte)RecordStatus.Active && f.UserType == (byte)UserType.Candidate);
                if (user == null)
                {
                    return (AuthenticateStatus.Failed, null, null);
                }
                bool IsNewJob = false;
                var NewApplications = (from Profile in dbContext.PhCandidateProfiles
                                       join JobCand in dbContext.PhJobCandidates on Profile.Id equals JobCand.CandProfId
                                       join ProfStatus in dbContext.PhCandStatusSes on JobCand.CandProfStatus equals ProfStatus.Id
                                       where Profile.EmailId == user.EmailId && ProfStatus.Cscode == CandidateStatusCodes.CCV.ToString()
                                       select new
                                       {
                                           Profile.EmailId
                                       }).FirstOrDefault();
                if (NewApplications != null)
                {
                    IsNewJob = true;
                }
                //var loginSatus = await dbContext.PiUserLog.Where(f => f.UserId == user.Id).OrderByDescending(s=>s.Id).Select(s=>s.LoginStatus).FirstOrDefaultAsync();
                //if (loginSatus.HasValue && loginSatus.Value)
                //{
                //    var VerifyToken = RandomTokenString();
                //    user.VerifyToken = VerifyToken;
                //    await dbContext.SaveChangesAsync();
                //    return (AuthenticateStatus.AlreadyLogin, null, VerifyToken);
                //}


                var AuthorizationViewModel = new AuthorizationViewModel
                {
                    Id = user.Id,
                    Name = user.FirstName,
                    UserDetails = new GetUserDetailsViewModel
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.UserName,
                        UserTypeId = (byte)UserType.Candidate
                    },
                    UserTypeId = (byte)UserType.Candidate,
                    IsNewJob = IsNewJob
                };
                AuthorizationViewModel.SessionTxnId = Guid.NewGuid().ToString();

                return (AuthenticateStatus.Success, AuthorizationViewModel, null);
            }
            catch (Exception ex)
            {
                return (AuthenticateStatus.Failed, null, null);
            }
        }

        public async Task<(bool status, AuthorizationViewModel data, bool facebookFlag)> AuthenticateFacebook(AccountAuthenticateFacebook model, System.Net.IPAddress clientLocation)
        {
            try
            {
                var validate = await ValidateFacebookTokenAsync(model.Token);
                if (!validate.status)
                {
                    return (false, null, false);
                }
                var user = await dbContext.PiHireUsers.FirstOrDefaultAsync(f => f.UserName == model.Username && f.VerifiedFlag && f.Status == (byte)RecordStatus.Active && f.UserType == (byte)UserType.Candidate);
                if (user == null)
                {
                    return (false, null, true);
                }

                bool IsNewJob = false;
                var NewApplications = (from Profile in dbContext.PhCandidateProfiles
                                       join JobCand in dbContext.PhJobCandidates on Profile.Id equals JobCand.CandProfId
                                       join ProfStatus in dbContext.PhCandStatusSes on JobCand.CandProfStatus equals ProfStatus.Id
                                       where Profile.EmailId == user.EmailId && ProfStatus.Cscode == CandidateStatusCodes.CCV.ToString()
                                       select new
                                       {
                                           Profile.EmailId
                                       }).FirstOrDefault();
                if (NewApplications != null)
                {
                    IsNewJob = true;
                }

                return (true, new AuthorizationViewModel
                {
                    Id = user.Id,
                    Name = user.FirstName,
                    UserDetails = new GetUserDetailsViewModel
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.UserName,
                        UserTypeId = (byte)UserType.Candidate
                    },
                    UserTypeId = (byte)UserType.Candidate,
                    IsNewJob = IsNewJob,
                    SessionTxnId = Guid.NewGuid().ToString()
                }, true);
            }
            catch (Exception ex)
            {
                return (false, null, true);
            }
        }

        public async Task<(bool status, AuthorizationViewModel data, bool googleFlag)> AuthenticateGoogle(AccountAuthenticateGoogle model, System.Net.IPAddress clientLocation)
        {
            try
            {
                var validate = await ValidateGoogleTokenAsync(model.Token);
                if (!validate.status)
                {
                    return (false, null, false);
                }
                var user = await dbContext.PiHireUsers.FirstOrDefaultAsync(f => f.UserName == model.Username && f.VerifiedFlag && f.Status == (byte)RecordStatus.Active && f.UserType == (byte)UserType.Candidate);
                if (user == null)
                {
                    return (false, null, true);
                }

                bool IsNewJob = false;
                var NewApplications = (from Profile in dbContext.PhCandidateProfiles
                                       join JobCand in dbContext.PhJobCandidates on Profile.Id equals JobCand.CandProfId
                                       join ProfStatus in dbContext.PhCandStatusSes on JobCand.CandProfStatus equals ProfStatus.Id
                                       where Profile.EmailId == user.EmailId && ProfStatus.Cscode == CandidateStatusCodes.CCV.ToString()
                                       select new
                                       {
                                           Profile.EmailId
                                       }).FirstOrDefault();
                if (NewApplications != null)
                {
                    IsNewJob = true;
                }

                return (true, new AuthorizationViewModel
                {
                    Id = user.Id,
                    Name = user.FirstName,
                    UserDetails = new GetUserDetailsViewModel
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.UserName,
                        UserTypeId = (byte)UserType.Candidate
                    },
                    UserTypeId = (byte)UserType.Candidate,
                    IsNewJob = IsNewJob,

                    SessionTxnId = Guid.NewGuid().ToString()
                }, true);
            }
            catch (Exception ex)
            {
                return (false, null, true);
            }
        }

        public async Task<CandidateRegistrationRespViewModel> CandidateRegistrationFacebook(CandidateRegistrationFacebookViewModel model)
        {
            try
            {
                using (var trans = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var validate = await ValidateFacebookTokenAsync(model.Token);
                        if (!validate.status)
                        {
                            return new CandidateRegistrationRespViewModel { Status = false, CandidateId = 0, Message = "Unable to authorized facebook account." };
                        }

                        var isuserNameAvai = await dbContext.PiHireUsers.AnyAsync(f => f.UserName == model.EmailId.ToLower().Trim());
                        if (isuserNameAvai)
                        {
                            return new CandidateRegistrationRespViewModel { Status = false, CandidateId = 0, Message = "Email address is already in use." };
                        }
                        var generator = new RandomGenerator();
                        var pswd = generator.RandomPassword(8);
                        var HashPswd = Hashification.SHA(pswd);
                        var user = new PiHireUser
                        {
                            UserType = (byte)UserType.Candidate,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            UserName = model.EmailId,
                            EmailId = model.EmailId,
                            PasswordHash = HashPswd,
                            CreatedBy = 0,
                            CreatedDate = DateTime.UtcNow,
                            VerifiedFlag = true,
                            VerifyToken = null,
                            Status = (byte)RecordStatus.Active
                        };
                        dbContext.PiHireUsers.Add(user);
                        await dbContext.SaveChangesAsync();

                        string redirectURL = appSettings.AppSettingsProperties.CandidateAppUrl + "/login";
                        if (model.JobId != null)
                        {
                            redirectURL = appSettings.AppSettingsProperties.CandidateAppUrl + "/login?jobid=" + model.JobId + "&sf=" + (byte)SourceType.Facebook + "";
                        }

                        var mailBody = EmailTemplates.User_EmailCredentials_Template(user.FirstName, user.UserName, pswd, redirectURL,
                            this.appSettings.AppSettingsProperties.HireAppUrl, this.appSettings.AppSettingsProperties.HireApiUrl);

                        SmtpMailing smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        appSettings.smtpEmailConfig.SmtpLoginName, appSettings.smtpEmailConfig.SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl,
                        appSettings.smtpEmailConfig.SmtpFromEmail, appSettings.smtpEmailConfig.SmtpFromName);

                        _ = smtp.SendMail(user.UserName, EmailTemplates.GetSubject(EmailTypes.AppUser_Create), mailBody, string.Empty);

                        trans.Commit();
                        return new CandidateRegistrationRespViewModel
                        {
                            CandidateId = user.Id,
                            Message = "Account created successfully.",
                            Status = true
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new CandidateRegistrationRespViewModel
                        {
                            CandidateId = 0,
                            Message = ex.Message,
                            Status = false
                        };
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<(string message, bool status)> ForgotPassword(ForgotPasswordRequest model)
        {
            try
            {
                var user = dbContext.PiHireUsers.FirstOrDefault(s => s.UserName == model.Email.Trim() && s.Status == (byte)RecordStatus.Active && s.UserType == (byte)UserType.Candidate);
                if (user == null)
                {
                    return ("Email not found.", false);
                }
                user.VerifyToken = RandomTokenString();
                user.TokenExpiryDate = DateTime.UtcNow.AddDays(1);

                await dbContext.SaveChangesAsync();

                var mailBody = EmailTemplates.User_ForgotPassword_Template(user.FirstName, appSettings.AppSettingsProperties.CandidateAppUrl + "/reset-password?token=" + user.VerifyToken, this.appSettings.AppSettingsProperties.HireAppUrl);

                SmtpMailing smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        appSettings.smtpEmailConfig.SmtpLoginName, appSettings.smtpEmailConfig.SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl,
                        appSettings.smtpEmailConfig.SmtpFromEmail, appSettings.smtpEmailConfig.SmtpFromName);
                _ = smtp.SendMail(user.UserName, EmailTemplates.GetSubject(EmailTypes.ForgotPassword), mailBody, string.Empty);
                //_ = 
                return ("", true);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<(string message, bool status)> ResetPassword(ResetPasswordRequest model)
        {
            try
            {
                var user = dbContext.PiHireUsers.FirstOrDefault(s => s.VerifyToken == model.Token.Trim() && s.TokenExpiryDate > CurrentTime);
                if (user == null)
                {
                    return ("Your reset password session is expire.", false);
                }
                var HashPswd = Hashification.SHA(model.Password);
                user.VerifyToken = null;
                user.TokenExpiryDate = null;
                user.PasswordHash = HashPswd;

                await dbContext.SaveChangesAsync();

                var mailBody = EmailTemplates.User_RestPassword_Template(user.FirstName, appSettings.AppSettingsProperties.CandidateAppUrl, this.appSettings.AppSettingsProperties.HireAppUrl);

                SmtpMailing smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        appSettings.smtpEmailConfig.SmtpLoginName, appSettings.smtpEmailConfig.SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl,
                        appSettings.smtpEmailConfig.SmtpFromEmail, appSettings.smtpEmailConfig.SmtpFromName);
                _ = smtp.SendMail(user.UserName, EmailTemplates.GetSubject(EmailTypes.ResetPassword), mailBody, string.Empty);
                //_ = 
                return ("", true);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<(List<GetUserCurrentStatusVM> data, bool status, string message)> GetUserCurrentStatus(OnlineStatusFilterViewModel filterViewModel)
        {
            try
            {
                var qry = dbContext.PiHireUsers.AsQueryable();
                if (filterViewModel.UserIds != null && filterViewModel.UserIds.Length > 0
                    && filterViewModel.EmailIds != null && filterViewModel.EmailIds.Length > 0)
                    qry = qry.Where(da => filterViewModel.UserIds.Contains(da.Id) || filterViewModel.EmailIds.Contains(da.UserName));
                else if (filterViewModel.UserIds != null && filterViewModel.UserIds.Length > 0)
                    qry = qry.Where(da => filterViewModel.UserIds.Contains(da.Id));
                else if (filterViewModel.EmailIds != null && filterViewModel.EmailIds.Length > 0)
                    qry = qry.Where(da => filterViewModel.EmailIds.Contains(da.UserName));

                if (filterViewModel.UserTypes != null && filterViewModel.UserTypes.Length > 0)
                {
                    qry = qry.Where(da => filterViewModel.UserTypes.Contains(da.UserType));
                }
                var userStatus = await
                    qry.Join(dbContext.PiUserLogs, da => da.Id, da2 => da2.UserId, (da, da2) => new GetUserCurrentStatusVM
                    {
                        UserId = da.Id,
                        Status = da2.LoginStatus ?? false,
                        EmailId = da.UserName
                    }).ToListAsync();
                //    dbContext.PiUserLog.Where(s => s.UserId.HasValue && filterViewModel.UserIds.Contains(s.UserId.Value))
                //    .Select(s => new GetUserCurrentStatusVM
                //{
                //    UserId = s.UserId.Value,
                //    Status = s.LoginStatus ?? false,
                //    EmailId = s.UserId
                //}).ToListAsync();
                return (userStatus, true, "");
            }
            catch (Exception ex)
            {
                return (null, false, ex.Message);
            }
        }

        public async Task<(bool status, string message, string deviceId)> LogoutFromAllDevices(string token, string email)
        {
            try
            {
                var user = await dbContext.PiHireUsers.FirstOrDefaultAsync(s => s.UserName == email && s.VerifyToken == token && s.Status == (byte)RecordStatus.Active);
                if (user == null)
                {
                    return (false, "Invalid Token", null);
                }
                user.VerifyToken = null;

                var userLog = await dbContext.PiUserLogs.Where(s => s.ApplicationId == UserRespository._applicationId && s.UserId == user.Id).OrderByDescending(s => s.Id).FirstOrDefaultAsync();
                if (userLog == null)
                {
                    return (false, "Invalid Token", null);
                }
                userLog.LoginStatus = false;
                var trans = await dbContext.PiUserTxnLogs.Where(f => f.ApplicationId == UserRespository._applicationId && f.UserId == user.Id && f.TxnOutDate.HasValue == false).ToListAsync();
                if (trans == null || trans.Count == 0)
                {
                    return (false, "Invalid Token", null);
                }
                foreach (var tran in trans)
                    tran.TxnOutDate = CurrentTime;
                await dbContext.SaveChangesAsync();
                return (true, "", trans.Select(da => da.DeviceUid).FirstOrDefault());
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public bool LoginTracking(int UserId, string Name, string LoggedInBy)
        {
            try
            {
                // audit 
                var audList = new List<CreateAuditViewModel>();
                var auditLog = new CreateAuditViewModel
                {
                    ActivitySubject = " LogIn",
                    ActivityDesc = " logged into the application  " + LoggedInBy,
                    ActivityType = (byte)AuditActivityType.Authentication,
                    TaskID = UserId,
                    UserId = UserId
                };
                audList.Add(auditLog);
                SaveAuditLog(audList);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
