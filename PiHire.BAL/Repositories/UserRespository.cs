using System;
using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.IRepositories;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.BAL.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PiHire.DAL.Entities;
using static PiHire.BAL.Common.Types.AppConstants;
using PiHire.BAL.Common.Http;
using Newtonsoft.Json;
using PiHire.BAL.Common;
using System.Security.Cryptography;
using PiHire.Utilities.Communications.Emails;
using PiHire.DAL.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Graph;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;
using iText.Commons.Bouncycastle.Asn1.X509;
using Newtonsoft.Json.Linq;

namespace PiHire.BAL.Repositories
{
    public class UserRespository : BaseRepository, IUserRespository
    {
        readonly Logger logger;
        internal static readonly int _applicationId = 2;
        
        private readonly IWebHostEnvironment _environment;

        
        public UserRespository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<CompanyRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        //public async Task<CreatePiHireUserResponse> CreateUser(CreateUserRequestViewModel model)
        //{
        //    try
        //    {
        //        using (var trans = await dbContext.Database.BeginTransactionAsync())
        //        {
        //            try
        //            {
        //                CreatePiHireUserResponse createUserResponse = new CreatePiHireUserResponse();
        //                logger.Log(LogLevel.Debug, LoggingEvents.GetItem, "Start of method:", Newtonsoft.Json.JsonConvert.SerializeObject(model));
        //                model.ApplicationId = _applicationId;
        //                model.CreatedBy = Usr.UserDetails.Id;
        //                var userAvil = dbContext.PiHireUsers.Any(s => s.UserName == model.Email.ToLower().Trim() && s.UserType != (byte)UserType.Candidate && s.Status != (byte)RecordStatus.Delete);
        //                if (userAvil)
        //                {
        //                    return new CreatePiHireUserResponse { UserId = 0, Message = "Email address is already in use.", Status = false };
        //                }

        //                using var client = new HttpClientService();
        //                var response = await client.PostAsync(appSettings.AppSettingsProperties.GatewayUrl, "/api/GWService/user/create", model);
        //                if (!response.IsSuccessStatusCode)
        //                {
        //                    return new CreatePiHireUserResponse { UserId = 0, Message = "Failed to create user.", Status = false };
        //                }

        //                var responseContent = await response.Content.ReadAsStringAsync();
        //                var gwUserResponse = JsonConvert.DeserializeObject<CreateUserResponse>(responseContent);
        //                if (gwUserResponse == null)
        //                {
        //                    createUserResponse.Status = false;
        //                    createUserResponse.Message = "Failed to create user.";
        //                    createUserResponse.UserId = 0;
        //                    trans.Rollback();
        //                    return createUserResponse;
        //                }
        //                else if (!gwUserResponse.Status || gwUserResponse.user == null)
        //                {
        //                    trans.Rollback();
        //                    return new CreatePiHireUserResponse { UserId = 0, Message = gwUserResponse.Message, Status = false };
        //                }

        //                createUserResponse.UserId = gwUserResponse.user.ID;

        //                //piHire User
        //                var piHireUser = new PiHireUser
        //                {
        //                    CreatedBy = gwUserResponse.user.CreatedBy,
        //                    CreatedDate = gwUserResponse.user.CreatedDate,
        //                    Status = gwUserResponse.user.Status,
        //                    EmployId = model.EmployeeId,
        //                    FirstName = gwUserResponse.user.FirstName,
        //                    LastName = gwUserResponse.user.LastName,
        //                    Location = model.LocationName,
        //                    LocationId = model.LocationId,
        //                    MobileNumber = gwUserResponse.user.MobileNumber,
        //                    PasswordHash = gwUserResponse.user.PasswordHash,
        //                    UserId = gwUserResponse.user.ID,
        //                    UpdatedBy = gwUserResponse.user.UpdatedBy,
        //                    UpdatedDate = gwUserResponse.user.UpdatedDate,
        //                    UserName = gwUserResponse.user.UserName.ToLower().Trim(),
        //                    UserRoleId = model.RoleId,
        //                    UserRoleName = model.Role,
        //                    UserType = gwUserResponse.user.UserType,
        //                    Dob = gwUserResponse.user.DOB,
        //                    EmailId = gwUserResponse.user.EmailID,
        //                    Nationality = gwUserResponse.user.Nationality,
        //                    ShiftId = model.ShiftId
        //                };
        //                await dbContext.PiHireUsers.AddAsync(piHireUser);
        //                await dbContext.SaveChangesAsync();

        //                trans.Commit();
        //                createUserResponse.Status = true;
        //                createUserResponse.Message = "User created successfully";
        //                return createUserResponse;
        //            }
        //            catch (Exception ex)
        //            {
        //                trans.Rollback();
        //                logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",model:" + Newtonsoft.Json.JsonConvert.SerializeObject(model), ex);
        //                return new CreatePiHireUserResponse { UserId = 0, Message = ex.Message, Status = false };
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",model:" + Newtonsoft.Json.JsonConvert.SerializeObject(model), ex);
        //        throw;
        //    }
        //}

        public async Task<GetResponseViewModel<int>> OdooCreateUpdateUser(OddoCreateUserRequestViewModel model)
        {
            try
            {
                var respModel = new GetResponseViewModel<int>();
                using (var trans = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        int hireUserId = 0;
                        logger.Log(LogLevel.Debug, LoggingEvents.GetItem, "Start of method:", Newtonsoft.Json.JsonConvert.SerializeObject(model));
                        model.ApplicationId = _applicationId;

                        var roleDtls = await dbContext.PiAppUserRoles.FirstOrDefaultAsync(x => x.UserType == model.UserType && x.Status == (byte)RecordStatus.Active);

                        var userAvil = dbContext.PiHireUsers.FirstOrDefault(s => s.UserName == model.Email.ToLower().Trim() && s.UserType != (byte)UserType.Candidate && s.Status != (byte)RecordStatus.Delete);
                        if (userAvil == null)
                        {
                            var generator = new RandomGenerator();
                            var pswd = generator.RandomPassword(8);
                            var HashPswd = Hashification.SHA(pswd);

                            //piHire User
                            var piHireUser = new PiHireUser
                            {
                                CreatedBy = Usr.Id,
                                CreatedDate = CurrentTime,
                                Status = (byte)RecordStatus.Active,
                                EmployId = model.EmployeeId,

                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                Dob = model.Dob,

                                MobileNumber = model.MobileNo,
                                PasswordHash = HashPswd,

                                UserId = model.UserId,
                                UserName = model.Email,

                                UserRoleId = roleDtls?.Id,
                                UserRoleName = roleDtls?.RoleName,
                                UserType = model.UserType,

                                EmailId = model.Email,
                                Nationality = model.Nationality,
                                VerifiedFlag = true
                            };
                            await dbContext.PiHireUsers.AddAsync(piHireUser);
                            await dbContext.SaveChangesAsync();

                            hireUserId = piHireUser.Id;

                            string redirectURL = appSettings.AppSettingsProperties.HireAppUrl + "/login";

                            var mailBody = EmailTemplates.User_EmailCredentials_Template(piHireUser.FirstName + " " + piHireUser.LastName, piHireUser.UserName, pswd,
                                                    redirectURL, this.appSettings.AppSettingsProperties.HireAppUrl, this.appSettings.AppSettingsProperties.HireApiUrl);

                            SmtpMailing smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                            appSettings.smtpEmailConfig.SmtpLoginName, appSettings.smtpEmailConfig.SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl,
                            appSettings.smtpEmailConfig.SmtpFromEmail, appSettings.smtpEmailConfig.SmtpFromName);
                            _ = smtp.SendMail(piHireUser.UserName, EmailTemplates.GetSubject(EmailTypes.AppUser_Create), mailBody, string.Empty);

                        }
                        else
                        {
                            hireUserId = userAvil.Id;

                            userAvil.Dob = model.Dob;
                            userAvil.FirstName = model.FirstName;
                            userAvil.LastName = model.LastName;
                            userAvil.MobileNumber = model.MobileNo;
                            userAvil.Nationality = model.Nationality;

                            userAvil.UserRoleId = roleDtls?.Id;
                            userAvil.UserRoleName = roleDtls?.RoleName;
                            userAvil.UserType = model.UserType;
                        }

                        var mapDtls = dbContext.PiAppUserRoleMaps.Where(x => x.AppUserId == hireUserId).ToList();
                        if (mapDtls != null && mapDtls.Count > 0)
                        {
                            dbContext.PiAppUserRoleMaps.RemoveRange(mapDtls);
                            await dbContext.SaveChangesAsync();
                        }

                        var respDtls = dbContext.PiAppUserResps.Where(x => x.AppUserId == hireUserId).ToList();
                        if (respDtls != null && respDtls.Count > 0)
                        {
                            dbContext.PiAppUserResps.RemoveRange(respDtls);
                            await dbContext.SaveChangesAsync();
                        }

                        PiAppUserRoleMap userRoleMap = new PiAppUserRoleMap
                        {
                            ApplicationId = model.ApplicationId.Value,
                            AppRoleId = roleDtls != null ? roleDtls.Id : 0,
                            AppUserId = hireUserId,
                            Status = (byte)RecordStatus.Active,
                            UpdatedBy = Usr.Id,
                            UpdatedDate = CurrentTime,
                            CreatedBy = Usr.Id,
                            CreatedDate = CurrentTime
                        };
                        dbContext.PiAppUserRoleMaps.Add(userRoleMap);

                        var roleResp = dbContext.PiAppUserRoleResps.Where(s => s.RoleId == roleDtls.Id && s.Status == (byte)RecordStatus.Active).Select(s => new
                        {
                            s.Id,
                            s.ModuleId,
                            s.Permissions,
                            s.RoleId,
                            s.TaskId
                        }).ToList();

                        PiAppUserResp userResp = null;
                        foreach (var resp in roleResp)
                        {
                            userResp = new PiAppUserResp
                            {
                                ModuleId = resp.ModuleId,
                                ApplicationId = model.ApplicationId.Value,
                                AppUserId = hireUserId,
                                Permissions = resp.Permissions,
                                TaskId = resp.TaskId,
                                Status = (byte)RecordStatus.Active,
                                CreatedBy = Usr.Id,
                                CreatedDate = CurrentTime,
                                UpdatedBy = Usr.Id,
                                UpdatedDate = CurrentTime
                            };
                            dbContext.PiAppUserResps.Add(userResp);
                        }

                        await dbContext.SaveChangesAsync();

                        PiAppUserPuBu pI_APP_USER_PU_BU = null;
                        var userPuBus = dbContext.PiAppUserPuBus.Where(x => x.AppUserId == hireUserId).ToList();
                        if (userPuBus.Count > 0 && userPuBus != null)
                        {
                            dbContext.PiAppUserPuBus.RemoveRange(userPuBus);
                            await dbContext.SaveChangesAsync();
                        }

                        foreach (var item1 in model.puBu)
                        {
                            pI_APP_USER_PU_BU = new PiAppUserPuBu
                            {
                                ApplicationId = model.ApplicationId.Value,
                                AppUserId = hireUserId,
                                ProcessUnit = item1.PUID,
                                BusinessUnit = item1.BUID,
                                Status = (byte)RecordStatus.Active,
                                CreatedBy = Usr.Id,
                                CreatedDate = CurrentTime,
                                UpdatedBy = Usr.Id,
                                UpdatedDate = CurrentTime
                            };

                            dbContext.PiAppUserPuBus.Add(pI_APP_USER_PU_BU);
                        }

                        await dbContext.SaveChangesAsync();

                        respModel.SetResult(hireUserId);

                        trans.Commit();

                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",model:" + Newtonsoft.Json.JsonConvert.SerializeObject(model), ex);

                        respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    }
                }
                return respModel;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",model:" + Newtonsoft.Json.JsonConvert.SerializeObject(model), ex);
                throw;
            }
        }

        public async Task<List<GetUsersViewModel>> GetAllUsersAsync()
        {
            try
            {
                List<int> excludeUsersLst = new List<int> { 1, 5, 8 };
                var users = dbContext.PiHireUsers.Where(s => s.Status != (byte)RecordStatus.Delete && !excludeUsersLst.Contains(s.UserType)).Select(s => new GetUsersViewModel
                {
                    Id = s.Id,
                    Email = s.UserName,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Roles = (from mp in dbContext.PiAppUserRoleMaps
                             join ur in dbContext.PiAppUserRoles on mp.AppRoleId equals ur.Id
                             where mp.Status == (byte)RecordStatus.Active && mp.AppUserId == s.Id
                             select new RoleList { Id = ur.Id, Name = ur.RoleName }).ToList(),
                    Status = s.Status,
                    UserType = s.UserType,
                    MobileNo = s.MobileNumber,
                    CreatedDate = s.CreatedDate,
                    UserBUPUs = (from m in dbContext.PiAppUserPuBus
                                 where m.AppUserId == _applicationId && m.AppUserId == s.Id && m.Status == (byte)RecordStatus.Active
                                 select new UserBUPU { BuId = m.BusinessUnit, PuId = m.ProcessUnit }).ToList()
                }).ToList();
                return users;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<GetProfilesViewModel>> GetProfiles()
        {
            try
            {
                List<GetProfilesViewModel> profiles = null;
                profiles = dbContext.PiAppUserRoles.Where(s => s.ApplicationId == _applicationId && s.Status == (byte)RecordStatus.Active).OrderByDescending(o => o.CreatedDate).Select(s => new GetProfilesViewModel
                {
                    Id = s.Id,
                    Description = s.RoleDesc,
                    Role = s.RoleName,
                    UserType = s.UserType
                }).ToList();
                return profiles;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CreateUserResponse> UpdateUser(UpdateUserViewModel model)
        {
            try
            {
                var appUser = await dbContext.PiHireUsers.FirstOrDefaultAsync(f => f.Id == model.Id && f.Status != (byte)RecordStatus.Delete);
                if (appUser == null)
                {
                    return new CreateUserResponse { user = null, Message = "User not found", Status = false };
                }

                using (var trans = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        appUser.FirstName = model.FirstName;
                        appUser.LastName = model.LastName;
                        appUser.MobileNumber = model.MobileNo;
                        appUser.UserRoleId = model.RoleId;
                        appUser.UserRoleName = model.Role;
                        appUser.UserType = model.UserType;
                        appUser.ShiftId = model.ShiftId;

                        PiAppUserPuBu pI_APP_USER_PU_BU = null;


                        var userPuBus = dbContext.PiAppUserPuBus.Where(x => x.AppUserId == appUser.Id).ToList();
                        if (userPuBus.Count > 0 && userPuBus != null)
                        {
                            dbContext.PiAppUserPuBus.RemoveRange(userPuBus);
                            await dbContext.SaveChangesAsync();

                        }

                        foreach (var item1 in model.PuBuId)
                        {
                            pI_APP_USER_PU_BU = new PiAppUserPuBu
                            {
                                ApplicationId = model.ApplicationId,
                                AppUserId = appUser.Id,
                                ProcessUnit = item1.PUID,
                                BusinessUnit = item1.BUID,
                                Status = (byte)RecordStatus.Active,
                                CreatedBy = Usr.Id,
                                CreatedDate = CurrentTime,
                                UpdatedBy = Usr.Id,
                                UpdatedDate = CurrentTime
                            };
                            dbContext.PiAppUserPuBus.Add(pI_APP_USER_PU_BU);
                        }


                        if (!string.IsNullOrEmpty(model.LocationName))
                        {
                            appUser.LocationId = model.LocationId;
                            appUser.Location = model.LocationName;
                        }

                        await dbContext.SaveChangesAsync();


                        var userRoleMaps = dbContext.PiAppUserRoleMaps.Where(x => x.AppUserId == appUser.Id && x.AppRoleId != model.RoleId && x.Status == (byte)RecordStatus.Active).ToList();
                        if (userPuBus != null && userPuBus.Count > 0)
                        {
                            dbContext.PiAppUserRoleMaps.RemoveRange(userRoleMaps);
                            await dbContext.SaveChangesAsync();

                            var userResps = dbContext.PiAppUserResps.Where(x => x.AppUserId == appUser.Id).ToList();
                            if (userResps != null && userResps.Count > 0)
                            {
                                dbContext.PiAppUserResps.RemoveRange(userResps);
                                await dbContext.SaveChangesAsync();
                            }

                            PiAppUserRoleMap userRoleMap = new PiAppUserRoleMap
                            {
                                ApplicationId = appSettings.AppSettingsProperties.AppId,
                                AppRoleId = model.RoleId,
                                AppUserId = appUser.Id,
                                Status = (byte)RecordStatus.Active,
                                UpdatedBy = Usr.Id,
                                UpdatedDate = CurrentTime,
                                CreatedBy = Usr.Id,
                                CreatedDate = CurrentTime
                            };
                            dbContext.PiAppUserRoleMaps.Add(userRoleMap);

                            var roleResp = dbContext.PiAppUserRoleResps.Where(s => s.RoleId == model.RoleId && s.Status == (byte)RecordStatus.Active).Select(s => new
                            {
                                s.Id,
                                s.ModuleId,
                                s.Permissions,
                                s.RoleId,
                                s.TaskId
                            }).ToList();

                            PiAppUserResp userResp = null;
                            foreach (var resp in roleResp)
                            {
                                userResp = new PiAppUserResp
                                {
                                    ModuleId = resp.ModuleId,
                                    ApplicationId = appSettings.AppSettingsProperties.AppId,
                                    AppUserId = appUser.Id,
                                    Permissions = resp.Permissions,
                                    TaskId = resp.TaskId,
                                    Status = (byte)RecordStatus.Active,
                                    CreatedBy = Usr.Id,
                                    CreatedDate = CurrentTime,
                                    UpdatedBy = Usr.Id,
                                    UpdatedDate = CurrentTime
                                };
                                dbContext.PiAppUserResps.Add(userResp);
                            }

                            await dbContext.SaveChangesAsync();

                        }


                        trans.Commit();


                        //var odooUserRequestViewModel = new OdooUserRequestViewModel
                        //{
                        //    FirstName = model.FirstName,
                        //    LastName = model.LastName,
                        //    MobileNo = model.MobileNo,
                        //    UserType = model.UserType,
                        //    UserId = appUser.Id,
                        //    puBu = new List<UserRequestPUBU>()
                        //};
                        //odooUserRequestViewModel.puBu = model.PuBuId;                        


                        //using var client = new HttpClientService();
                        //var OdooAccessResponse = client.Get(appSettings.AppSettingsProperties.OdooBaseURL,
                        //              appSettings.AppSettingsProperties.OdooLoginURL, appSettings.AppSettingsProperties.OdooDb, appSettings.AppSettingsProperties.OdooUsername, appSettings.AppSettingsProperties.OdooPassword);

                        //var responseContent = await OdooAccessResponse.Content.ReadAsStringAsync();
                        //string access_token = JObject.Parse(responseContent)["access_token"].ToString();

                        //var cnvtEmployee = await client.PostAsync(appSettings.AppSettingsProperties.OdooBaseURL, appSettings.AppSettingsProperties.OdooUpdateUser, access_token, odooUserRequestViewModel);
                        //if (cnvtEmployee.IsSuccessStatusCode)
                        //{
                        //    var cnvtEmployeeResponseContent = await cnvtEmployee.Content.ReadAsStringAsync();
                        //    var result = JsonConvert.DeserializeObject<int>(cnvtEmployeeResponseContent);

                        //    logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, ", Odoo update user success:" + Newtonsoft.Json.JsonConvert.SerializeObject(cnvtEmployeeResponseContent));
                        //}
                        //else
                        //{
                        //    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ", Odoo update user fail:" + Newtonsoft.Json.JsonConvert.SerializeObject(cnvtEmployee));
                        //}


                        return new CreateUserResponse { user = null, Message = "Updated user successfully", Status = true };
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        
        public async Task<GetResponseViewModel<List<RecsViewModel>>> GetRecruiters()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<RecsViewModel>>();
            try
            {
                List<RecsViewModel> data = null;

                data = await dbContext.GetRecuiters();

                respModel.SetResult(data);
                respModel.Status = true;

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        #region Modules && Tasks

        
        public async Task<GetResponseViewModel<List<GetModuleModel>>> GetModules()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<GetModuleModel>>();
            try
            {
                

                List<GetModuleModel> data = null;
                data = await dbContext.GetModules(this.appSettings.AppSettingsProperties.AppId);

                respModel.SetResult(data);
                respModel.Status = true;

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        
        public async Task<GetResponseViewModel<List<GetTasksModel>>> GetTasks(int? ModuleId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<GetTasksModel>>();
            try
            {
                

                List<TasksViewModel> data = null;
                data = await dbContext.GetTasks(this.appSettings.AppSettingsProperties.AppId, ModuleId);

                var response = new List<GetTasksModel>();
                foreach (var item in data)
                {
                    var getTasksModel = new GetTasksModel
                    {
                        Activities = item.Activities,
                        ActivityFlag = item.ActivityFlag,
                        Code = item.Code,
                        Description = item.Description,
                        Id = item.Id,
                        ModuleId = item.ModuleId,
                        Task = item.Task
                    };
                    getTasksModel.IsValid = Enum.IsDefined(typeof(WorkFlowTaskCode), item.Code);
                    response.Add(getTasksModel);
                }
                respModel.SetResult(response);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        #endregion

        #region
        public async Task<bool> CreateRoleAsync(CreateRoleViewModel model)
        {
            try
            {
                try
                {
                    var role = new PiAppUserRole
                    {
                        ApplicationId = 2,
                        CreatedBy = Usr.Id,
                        CreatedDate = DateTime.UtcNow,
                        RoleName = model.Role,
                        RoleDesc = model.RoleDescription,
                        Status = (byte)RecordStatus.Active,
                        UserType = model.UserType
                    };
                    dbContext.PiAppUserRoles.Add(role);
                    await dbContext.SaveChangesAsync();

                    if (model.IsClone && model.CloneRoleId.HasValue)
                    {
                        var cloneRoleResp = dbContext.PiAppUserRoleResps.Where(s => s.Status == (byte)RecordStatus.Active && s.RoleId == model.CloneRoleId).ToList();
                        foreach (var resp in cloneRoleResp)
                        {
                            resp.Id = 0;
                            resp.RoleId = role.Id;
                            resp.CreatedBy = Usr.Id;
                            resp.CreatedDate = DateTime.UtcNow;
                            resp.UpdatedBy = Usr.Id;
                            resp.UpdatedDate = DateTime.UtcNow;
                            dbContext.PiAppUserRoleResps.Add(resp);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        var resps = (from task in dbContext.PiAppTasksSes
                                     join module in dbContext.PiAppModulesSes on task.ModuleId equals module.Id
                                     where task.Status == (byte)RecordStatus.Active && task.ApplicationId == model.ApplicationId
                                     select new
                                     {
                                         task.Id,
                                         task.ModuleId,
                                         task.Activities,
                                         task.ActivityFlag
                                     }).ToList();
                        PiAppUserRoleResp respObj = null;
                        foreach (var resp in resps)
                        {
                            respObj = new PiAppUserRoleResp
                            {
                                ModuleId = resp.ModuleId,
                                CreatedBy = Usr.Id,
                                CreatedDate = DateTime.UtcNow,
                                Permissions = resp.Activities,
                                RoleId = role.Id,
                                Status = (byte)RecordStatus.Active,
                                TaskId = resp.Id,
                                UpdatedBy = Usr.Id,
                                UpdatedDate = DateTime.UtcNow
                            };
                            dbContext.PiAppUserRoleResps.Add(respObj);
                            await dbContext.SaveChangesAsync();
                        }
                    }


                    return true;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<GetProfileDetailsViewModel> GetProfileDetailsAsync(int ProfileId)
        {
            try
            {
                GetProfileDetailsViewModel profilesDetails = (from appuser in dbContext.PiAppUserRoles
                                                              where appuser.ApplicationId == _applicationId && appuser.Status == (byte)RecordStatus.Active && appuser.Id == ProfileId
                                                              select new GetProfileDetailsViewModel
                                                              {
                                                                  Id = appuser.Id,
                                                                  Description = appuser.RoleDesc,
                                                                  Name = appuser.RoleName,
                                                                  ProfileResps = (from urr in dbContext.PiAppUserRoleResps
                                                                                  join task in dbContext.PiAppTasksSes on urr.TaskId equals task.Id
                                                                                  join module in dbContext.PiAppModulesSes on urr.ModuleId equals module.Id

                                                                                  where urr.RoleId == appuser.Id && urr.Status == (byte)RecordStatus.Active
                                                                                  select new ProfileRespViewModel
                                                                                  {
                                                                                      ModuleId = urr.ModuleId,
                                                                                      TaskId = urr.TaskId,
                                                                                      Module = module.ModuleName,
                                                                                      Task = task.TaskName,
                                                                                      ModuleDesc = module.ModuleDesc,
                                                                                      TaskDesc = task.TaskDesc,
                                                                                      ActivityFlag = task.ActivityFlag,
                                                                                      Permissions = urr.Permissions.Trim(),
                                                                                      TaskPermissions = task.Activities
                                                                                  }
                                                                                 ).ToList()
                                                              }).FirstOrDefault();

                var excludeActivities = profilesDetails.ProfileResps.Select(s => s.TaskId).ToList();
                var newActivities = (from task in dbContext.PiAppTasksSes
                                     join module in dbContext.PiAppModulesSes on task.ModuleId equals module.Id
                                     where task.Status == (byte)RecordStatus.Active && !excludeActivities.Contains(task.Id)
                                     select new ProfileRespViewModel
                                     {
                                         ModuleId = task.ModuleId,
                                         TaskId = task.Id,
                                         Module = module.ModuleName,
                                         Task = task.TaskName,
                                         ModuleDesc = module.ModuleDesc,
                                         TaskDesc = task.TaskDesc,
                                         ActivityFlag = task.ActivityFlag,
                                         Permissions = task.Activities,
                                         TaskPermissions = task.Activities
                                     }).ToList();
                if (newActivities != null && newActivities.Count != 0)
                {
                    profilesDetails.ProfileResps.AddRange(newActivities);
                }
                return profilesDetails;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<UpdateProfilePermisRespVM> UpdateProfilePermissionsAsync(UpdateRolePermissionsViewModel model)
        {
            try
            {
                var roleDtls = dbContext.PiAppUserRoles.Where(x => x.Id != model.RoleId && x.Status == (byte)RecordStatus.Active && x.RoleName == model.Role).FirstOrDefault();
                if (roleDtls != null)
                {
                    return new UpdateProfilePermisRespVM { Message = "Role is available", Status = false };
                }
                roleDtls = dbContext.PiAppUserRoles.Where(x => x.Id == model.RoleId).FirstOrDefault();
                if (roleDtls != null)
                {
                    roleDtls.RoleName = model.Role;
                    roleDtls.RoleDesc = model.RoleDescription;

                    dbContext.SaveChanges();
                }

                var rolePermissions = dbContext.PiAppUserRoleResps.Where(s => s.RoleId == model.RoleId && s.Status == (byte)RecordStatus.Active).ToList();
                if (rolePermissions == null || rolePermissions.Count == 0)
                {
                    return new UpdateProfilePermisRespVM { Message = "Role Not Found", Status = false };
                }
                foreach (var per in model.Permissions)
                {
                    var roleResp = rolePermissions.Where(s => s.ModuleId == per.ModuleId && s.TaskId == per.TaskId).FirstOrDefault();
                    if (roleResp != null)
                    {
                        roleResp.Permissions = per.Activities;
                        roleResp.UpdatedBy = 1;
                        roleResp.UpdatedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        dbContext.PiAppUserRoleResps.Add(new PiAppUserRoleResp
                        {
                            ModuleId = (short)per.ModuleId,
                            TaskId = (short)per.TaskId,
                            Permissions = per.Activities,
                            RoleId = model.RoleId,
                            Status = (byte)RecordStatus.Active,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow,
                            CreatedBy = 1,
                            UpdatedBy = 1,
                        });
                    }
                }
                dbContext.SaveChanges();
                return new UpdateProfilePermisRespVM { Message = "Permissions Updated SuccessFully", Status = true };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<GetResponseViewModel<List<HireUserProfileViewModel>>> GetUserProfiles(int PuId)
        {
            var respModel = new GetResponseViewModel<List<HireUserProfileViewModel>>();
            try
            {
                var usersViewModels = new List<HireUserProfileViewModel>();

                usersViewModels = await (from user in dbContext.PiHireUsers
                                         where user.Status == (byte)RecordStatus.Active && user.UserId != 0
                                         select new HireUserProfileViewModel
                                         {
                                             UserId = user.Id,
                                             Email = user.UserName,
                                             MobileNo = user.MobileNumber,
                                             FirstName = user.FirstName,
                                             LastName = user.LastName,
                                             UserType = user.UserType,
                                             FullName = user.FirstName + " " + user.LastName,
                                             RoleName = user.UserRoleName,
                                             Status = user.Status,
                                             LocationId = user.LocationId,
                                             Location = user.Location,
                                             Nationality = user.Nationality,

                                             UserPubuList = (from pubu in dbContext.PiAppUserPuBus
                                                             join pu in dbContext.TblParamProcessUnitMasters on pubu.ProcessUnit equals pu.Id
                                                             join bu in dbContext.TblParamPuBusinessUnits on pubu.BusinessUnit equals bu.Id
                                                             where pubu.AppUserId == user.Id && pubu.Status != (byte)RecordStatus.Delete
                                                             select new PUBUList
                                                             {
                                                                 PuId = pubu.ProcessUnit,
                                                                 BuId = pubu.BusinessUnit,
                                                                 BuName = bu.BusUnitFullName,
                                                                 PuName = pu.PuName,
                                                                 CountryId = pu.Country
                                                             }).ToList()
                                         }).ToListAsync();

                respModel.SetResult(usersViewModels);
                respModel.Status = true;
            }
            catch (Exception)
            {
                throw;
            }
            return respModel;
        }

        public async Task<GetUserDetailsViewModel> GetUserDetails(int UserId)
        {
            try
            {

                var userDetails = (from user in dbContext.PiHireUsers
                                   where user.Status == (byte)RecordStatus.Active && user.Id == UserId
                                   select new GetUserDetailsViewModel
                                   {
                                       Id = user.Id,
                                       Email = user.UserName,
                                       LocationId = user.LocationId,
                                       Location = string.Empty,
                                       MobileNo = user.MobileNumber,
                                       FirstName = user.FirstName,
                                       LastName = user.LastName,
                                       RoleName = user.UserRoleName,
                                       RoleId = user.UserRoleId,
                                       UserTypeId = user.UserType,

                                       Permission = (from ur in dbContext.PiAppUserResps
                                                     join ro in dbContext.PiAppUserRoleMaps on ur.AppUserId equals ro.AppUserId
                                                     join role in dbContext.PiAppUserRoles on ro.AppRoleId equals role.Id
                                                     join mo in dbContext.PiAppModulesSes on ur.ModuleId equals mo.Id
                                                     join ta in dbContext.PiAppTasksSes on ur.TaskId equals ta.Id

                                                     where ur.ApplicationId == _applicationId && ur.AppUserId == user.Id
                                                     && ur.Status == (byte)RecordStatus.Active && ta.Status == (byte)RecordStatus.Active
                                                     select new ProfileRespViewModel
                                                     {
                                                         TaskId = ur.TaskId,
                                                         ModuleId = ur.ModuleId,
                                                         RoleName = role.RoleName,
                                                         Permissions = ur.Permissions,
                                                         ActivityFlag = ta.ActivityFlag,
                                                         CreatedBy = string.Empty,
                                                         Module = mo.ModuleName,
                                                         ModuleDesc = mo.ModuleDesc,
                                                         Task = ta.TaskName,
                                                         TaskDesc = ta.TaskDesc,
                                                         UpdatedBy = string.Empty,
                                                         TaskCode = ta.TaskCode,
                                                         UpdatedDate = ro.UpdatedDate,
                                                         TaskPermissions = (dbContext.PiAppUserRoleResps.Where(s => s.ModuleId == ur.ModuleId && s.TaskId == ur.TaskId && s.RoleId == ro.AppRoleId)).Select(p => p.Permissions).FirstOrDefault()
                                                     }).OrderByDescending(x => x.UpdatedDate).ToList(),

                                       UserPubuList = (from pubu in dbContext.PiAppUserPuBus
                                                       join pu in dbContext.TblParamProcessUnitMasters on pubu.ProcessUnit equals pu.Id
                                                       join bu in dbContext.TblParamPuBusinessUnits on pubu.BusinessUnit equals bu.Id
                                                       where pubu.AppUserId == user.Id && pubu.Status != (byte)RecordStatus.Delete
                                                       select new PUBUList
                                                       {
                                                           BuId = pubu.BusinessUnit,
                                                           PuId = pubu.ProcessUnit,
                                                           BuName = bu.BusUnitFullName,
                                                           PuName = pu.PuName,
                                                           CountryId = pu.Country
                                                       }).ToList()
                                   }).FirstOrDefault();
                              


                if (userDetails != null)
                {

                    var taskIds = userDetails.Permission.Select(x => x.TaskId).ToList();

                    var newPermissions = (from ur in dbContext.PiAppUserRoleResps
                                          join role in dbContext.PiAppUserRoles on ur.RoleId equals role.Id
                                          join mo in dbContext.PiAppModulesSes on ur.ModuleId equals mo.Id
                                          join ta in dbContext.PiAppTasksSes on ur.TaskId equals ta.Id

                                          where ur.RoleId == userDetails.RoleId && !taskIds.Contains(ur.TaskId)
                                          && ur.Status == (byte)RecordStatus.Active && ta.Status == (byte)RecordStatus.Active
                                          select new ProfileRespViewModel
                                          {
                                              TaskId = ur.TaskId,
                                              ModuleId = ur.ModuleId,
                                              RoleName = role.RoleName,
                                              Permissions = ur.Permissions.Length > 1 ? "0000000000" : "0",
                                              ActivityFlag = ta.ActivityFlag,
                                              CreatedBy = string.Empty,
                                              Module = mo.ModuleName,
                                              ModuleDesc = mo.ModuleDesc,
                                              Task = ta.TaskName,
                                              TaskDesc = ta.TaskDesc,
                                              UpdatedBy = string.Empty,
                                              TaskCode = ta.TaskCode,
                                              UpdatedDate = ur.UpdatedDate.Value,
                                              TaskPermissions = ur.Permissions
                                          }).OrderByDescending(x => x.UpdatedDate).ToList();

                    userDetails.Permission.AddRange(newPermissions);

                    if (userDetails.LocationId != null)
                    {
                        userDetails.Location = dbContext.TblParamPuOfficeLocations.Where(x => x.Id == userDetails.LocationId.Value).Select(x => x.LocationName).FirstOrDefault();
                    }
                }

                return userDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }

        
        public async Task<GetResponseViewModel<List<UsersViewModel>>> GetUsersbyType(List<int> UserType, int? puId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<UsersViewModel>>();
            try
            {
                
                List<UsersViewModel> data = null;

                data = await GetUserbyTypes(UserType, puId);

                respModel.SetResult(data);
                respModel.Status = true;

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        
        public async Task<GetResponseViewModel<List<UsersViewModel>>> GetUsers(int Puid)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<UsersViewModel>>();
            try
            {
                
                List<UsersViewModel> data = null;

                List<int> UserTypes = new List<int>
                {
                    (int)UserType.SuperAdmin,
                    (int)UserType.Admin
                };

                data = await GetUserbyTypes(UserTypes, Puid);


                respModel.SetResult(data);
                respModel.Status = true;

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        
        public async Task<GetResponseViewModel<List<UsersViewModel>>> GetSignatureAuthorityUsers()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<UsersViewModel>>();
            try
            {
                
                List<UsersViewModel> data = null;

                List<int> UserTypes = new List<int>
                {
                    (int)UserType.SuperAdmin,
                    (int)UserType.Admin
                };

                data = await GetUserbyTypes(UserTypes, 0);

                respModel.SetResult(data);
                respModel.Status = true;

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<UpdateUserPermisRespVM> UpdateUserPermissionsAsync(UpdateUserPermissionsVM model)
        {
            try
            {
                var permissions = dbContext.PiAppUserResps.Where(s => s.ApplicationId == model.ApplicationId && s.AppUserId == model.UserId && s.Status != (byte)RecordStatus.Delete).ToList();
                if (permissions == null)
                {
                    return new UpdateUserPermisRespVM { Message = "User permissions not found." };
                }
                foreach (var item in model.Permissions)
                {
                    var modelResp = permissions.FirstOrDefault(s => s.ModuleId == item.ModuleId && s.TaskId == item.TaskId);
                    if (modelResp != null)
                    {
                        modelResp.Permissions = item.Activities;
                        modelResp.UpdatedBy = Usr.Id;
                        modelResp.UpdatedDate = CurrentTime;

                        dbContext.PiAppUserResps.Update(modelResp);

                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        var piAppUserResps = new PiAppUserResp
                        {
                            Permissions = item.Activities,
                            TaskId = (short)item.TaskId,
                            ApplicationId = model.ApplicationId,    
                            AppUserId = model.UserId,   
                            ModuleId   = (short)item.ModuleId,
                            CreatedBy = Usr.Id,
                            CreatedDate =CurrentTime,
                            Status = (byte)RecordStatus.Active 
                        };

                        dbContext.PiAppUserResps.Add(piAppUserResps);

                        await dbContext.SaveChangesAsync();
                    }
                }
                dbContext.SaveChanges();
                return new UpdateUserPermisRespVM { Message = "Permissions Updated SuccessFully", Status = true };
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<UpdateUserStatusResponseViewModel> UpdateUserStatus(UpdateUserStatusViewModel model)
        {
            try
            {
                var user = dbContext.PiHireUsers.Where(s => s.Id == model.UserId).FirstOrDefault();
                if (user == null)
                {
                    return new UpdateUserStatusResponseViewModel { Status = false, Message = "User not found" };
                }

                user.Status = model.Status;
                user.UpdatedBy = Usr.Id;
                user.UpdatedDate = CurrentTime;

                UpdateUserStatusResponseViewModel data = new UpdateUserStatusResponseViewModel { Status = true, Message = "Updated Successfully" };

                await dbContext.SaveChangesAsync();

                return data;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<UpdateResponseViewModel<string>> UpdateUserStatus(int UserId, RecordStatus Status)
        {
            var respModel = new UpdateResponseViewModel<string>();
            try
            {
                var user = dbContext.PiHireUsers.Where(s => s.Id == UserId).FirstOrDefault();
                if (user == null)
                {
                    respModel.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The resource is not available", true);

                    return respModel;
                }

                user.Status = (byte)Status;
                user.UpdatedBy = Usr.Id;
                user.UpdatedDate = CurrentTime;

                await dbContext.SaveChangesAsync();

                respModel.SetResult("Updated successfully");
                respModel.Status = true;
            }
            catch (Exception)
            {

                throw;
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<int>> IsUserExists(UserExistModel userExistModel)
        {
            var respModel = new GetResponseViewModel<int>();
            try
            {
                var user = await dbContext.PiHireUsers.Where(s => s.UserName == userExistModel.UserName && s.UserType != (byte)UserType.Candidate).FirstOrDefaultAsync();
                if (user == null)
                {
                    respModel.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The resource is not available", true);
                    respModel.Status = false;
                }
                else
                {
                    respModel.SetResult(user.Id);
                    respModel.Status = true;
                }

            }
            catch (Exception)
            {

                throw;
            }
            return respModel;
        }


        public async Task<(UserAuthorizationViewModel data, string msg, AuthenticateStatus loginStatus, string token, List<UserAuthorizationViewModel_UserDetail_Permission> permissions, List<UserAuthorizationViewModel_UserDetail_UserPubuList> pubuLists)> authenticate(string username, string password)
        {
            try
            {
                var HashPswd = Hashification.SHA(password);
                var piHireUser = dbContext.PiHireUsers.Where(s => s.UserName == username && s.PasswordHash == HashPswd && s.UserType != (byte)UserType.Candidate).FirstOrDefault();
                if (piHireUser == null)
                {
                    return (null, "Username/password is wrong", AuthenticateStatus.Failed, null, null, null);
                }
                else if (piHireUser.Status != (byte)RecordStatus.Active)
                {
                    return (null, "Inactive User Account", AuthenticateStatus.Failed, null, null, null);
                }
                else if (piHireUser.UserType != (byte)UserType.SuperAdmin)
                {
                    return (null, "User is not super admin", AuthenticateStatus.Failed, null, null, null);
                }

                //var loginSatus = await dbContext.PiUserLog.Where(f => f.UserId == piHireUser.Id).Select(s => s.LoginStatus).FirstOrDefaultAsync();
                //if (loginSatus.HasValue && loginSatus.Value)
                //{
                //    var verifyToken = RandomTokenString();
                //    piHireUser.VerifyToken = verifyToken;
                //    await dbContext.SaveChangesAsync();
                //    return (null, "already login", AuthenticateStatus.AlreadyLogin, verifyToken);
                //}

                UserAuthorizationViewModel data = new UserAuthorizationViewModel();
                var userDetails = (from user in dbContext.PiHireUsers
                                   where user.Status == (byte)RecordStatus.Active && user.UserId == piHireUser.UserId
                                   select new UserAuthorizationViewModel_UserDetail
                                   {
                                       Id = user.Id,
                                       Email = user.UserName,
                                       MobileNo = user.MobileNumber,
                                       FirstName = user.FirstName,
                                       LastName = user.LastName,
                                       UserTypeId = user.UserType
                                   }).FirstOrDefault();

                var Permissions = (from ur in dbContext.PiAppUserResps
                                   join ro in dbContext.PiAppUserRoleMaps on ur.AppUserId equals ro.AppUserId
                                   join role in dbContext.PiAppUserRoles on ro.AppRoleId equals role.Id
                                   join mo in dbContext.PiAppModulesSes on ur.ModuleId equals mo.Id
                                   join ta in dbContext.PiAppTasksSes on ur.TaskId equals ta.Id
                                   where ur.ApplicationId == _applicationId && ur.AppUserId == userDetails.Id
                                   && ur.Status == (byte)RecordStatus.Active && ta.Status == (byte)RecordStatus.Active
                                   select new UserAuthorizationViewModel_UserDetail_Permission
                                   {
                                       TaskId = ur.TaskId,
                                       ModuleId = ur.ModuleId,
                                       RoleName = role.RoleName,
                                       Permissions = ur.Permissions,
                                       ActivityFlag = ta.ActivityFlag,
                                       Module = mo.ModuleName,
                                       ModuleDesc = mo.ModuleDesc,
                                       Task = ta.TaskName,
                                       TaskDesc = ta.TaskDesc,
                                       TaskCode = ta.TaskCode,
                                       UpdatedDate = ro.UpdatedDate,
                                       TaskPermissions = (dbContext.PiAppUserRoleResps.Where(s => s.ModuleId == ur.ModuleId && s.TaskId == ur.TaskId && s.RoleId == ro.AppRoleId)).Select(p => p.Permissions).FirstOrDefault()
                                   }).OrderByDescending(x => x.UpdatedDate).ToList();
                var UserPubuList = (from pubu in dbContext.PiAppUserPuBus
                                    join pu in dbContext.TblParamProcessUnitMasters on pubu.ProcessUnit equals pu.Id
                                    join bu in dbContext.TblParamPuBusinessUnits on pubu.BusinessUnit equals bu.Id
                                    where pubu.AppUserId == userDetails.Id && pubu.Status != (byte)RecordStatus.Delete
                                    select new UserAuthorizationViewModel_UserDetail_UserPubuList
                                    {
                                        BuId = pubu.BusinessUnit,
                                        PuId = pubu.ProcessUnit,
                                        BuName = bu.BusUnitFullName,
                                        PuName = pu.PuName,
                                        CountryId = pu.Country
                                    }).ToList();

                if (userDetails != null)
                {
                    if (Permissions.Count > 0)
                    {
                        userDetails.RoleName = Permissions[0].RoleName;
                    }
                }
                data.Id = piHireUser.Id;
                data.EmpId = piHireUser.EmployId;
                data.UserDetails = userDetails;
                data.UserTypeId = piHireUser.UserType;

                if (piHireUser.UserType == (byte)UserType.SuperAdmin)
                {
                    data.RoleName = "Super Admin";
                }
                else
                {
                    data.RoleName = userDetails.RoleName;
                }

                data.ProfilePhoto = piHireUser.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + piHireUser.Id + "/ProfilePhoto/" + piHireUser.ProfilePhoto : string.Empty;

                data.SessionTxnId = Guid.NewGuid().ToString();

                return (data, string.Empty, AuthenticateStatus.Success, null, Permissions, UserPubuList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<(UserAuthorizationViewModel data, string msg, AuthenticateStatus loginStatus, string token)> OdooAuthenticate(string username, string password)
        {
            try
            {
                var HashPswd = Hashification.SHA(password);
                var piHireUser = dbContext.PiHireUsers.Where(s => s.UserName == username && s.PasswordHash == HashPswd && s.UserType == (byte)UserType.ApiUser).FirstOrDefault();
                if (piHireUser == null)
                {
                    return (null, "Username/password is wrong", AuthenticateStatus.Failed, null);
                }
                if (piHireUser.Status != (byte)RecordStatus.Active)
                {
                    return (null, "Inactive User Account", AuthenticateStatus.Failed, null);
                }

                UserAuthorizationViewModel data = new UserAuthorizationViewModel();
                data.Id = piHireUser.Id;
                data.EmpId = piHireUser.EmployId;
                data.SessionTxnId = Guid.NewGuid().ToString();
                return (data, string.Empty, AuthenticateStatus.Success, null);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #region Microsoft
        string MicrosoftOutlookRedirectUrl
        {
            get
            {
                return appSettings.AppSettingsProperties.HireApiUrl +
                    (appSettings.AppSettingsProperties.HireApiUrl.EndsWith("?") ? "" : "/") + "api/v1/user/authenticate/outlook/authResp";
            }
        }
        class MicrosoftOutlookAuthReqDtls
        {
            public string username { get; set; }
            public DateTime ReqDt { get; set; }
            public string code { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsSuccess { get; set; }
            public string Token { get; internal set; }
        }
        static List<MicrosoftOutlookAuthReqDtls> MicrosoftOutlookAuthReqData = new List<MicrosoftOutlookAuthReqDtls>();
        public GetResponseViewModel<AuthenticationOutlookRequestViewModel> authenticateMicrosoftRequest()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<AuthenticationOutlookRequestViewModel>();
            var obj = MicrosoftOutlookAuthReqData.FirstOrDefault(da => da.IsCompleted == false);
            if (obj == null)
            {
                string cd;
                do
                {
                    cd = (Guid.NewGuid() + "").Split('-')[0];
                } while (MicrosoftOutlookAuthReqData.FirstOrDefault(da => da.code == cd) != null);
                obj = new MicrosoftOutlookAuthReqDtls { code = cd, ReqDt = DateTime.UtcNow };
                MicrosoftOutlookAuthReqData.Add(obj);
            }
            respModel.SetResult(new AuthenticationOutlookRequestViewModel { RequestCode = obj.code, RequestUrl = Common._3rdParty.MicrosoftOutlook.getAuthorizeUrl(MicrosoftOutlookRedirectUrl, obj.code) });
            return respModel;
        }
        public async Task<(bool IsCompleted, bool IsSuccess, UserAuthorizationViewModel data, string msg, List<UserAuthorizationViewModel_UserDetail_Permission> permissions, List<UserAuthorizationViewModel_UserDetail_UserPubuList> pubuLists)> authenticateMicrosoft(string RequestCode, System.Net.IPAddress clientLocation)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.GetItem, "Start of method: ");
                var obj = MicrosoftOutlookAuthReqData.FirstOrDefault(da => da.code == RequestCode);
                if (obj != null)
                {
                    UserAuthorizationViewModel data = null;
                    if (obj.IsCompleted)
                    {
                        MicrosoftOutlookAuthReqData.Remove(obj);

                        var piHireUser = dbContext.PiHireUsers.Where(s => s.UserName == obj.username && s.UserType != (byte)UserType.Candidate).FirstOrDefault();
                        if (piHireUser == null)
                        {
                            return (obj.IsCompleted, false, data, $"Username({obj.username}) does not exist", null, null);
                        }
                        if (piHireUser.Status != (byte)RecordStatus.Active)
                        {
                            return (obj.IsCompleted, false, data, $"Inactive User Account({obj.username})", null, null);
                        }

                        {
                            data = new UserAuthorizationViewModel();

                            var userDetails = (from user in dbContext.PiHireUsers
                                               where user.Status == (byte)RecordStatus.Active && user.UserId == piHireUser.UserId
                                               select new UserAuthorizationViewModel_UserDetail
                                               {
                                                   Id = user.Id,
                                                   Email = user.UserName,
                                                   MobileNo = user.MobileNumber,
                                                   FirstName = user.FirstName,
                                                   LastName = user.LastName,
                                                   UserTypeId = user.UserType
                                               }).FirstOrDefault();

                            var Permissions = (from ur in dbContext.PiAppUserResps
                                               join ro in dbContext.PiAppUserRoleMaps on ur.AppUserId equals ro.AppUserId
                                               join role in dbContext.PiAppUserRoles on ro.AppRoleId equals role.Id
                                               join mo in dbContext.PiAppModulesSes on ur.ModuleId equals mo.Id
                                               join ta in dbContext.PiAppTasksSes on ur.TaskId equals ta.Id
                                               where ur.ApplicationId == _applicationId && ur.AppUserId == userDetails.Id
                                               && ur.Status == (byte)RecordStatus.Active && ta.Status == (byte)RecordStatus.Active
                                               select new UserAuthorizationViewModel_UserDetail_Permission
                                               {
                                                   TaskId = ur.TaskId,
                                                   ModuleId = ur.ModuleId,
                                                   RoleName = role.RoleName,
                                                   Permissions = ur.Permissions,
                                                   ActivityFlag = ta.ActivityFlag,
                                                   Module = mo.ModuleName,
                                                   ModuleDesc = mo.ModuleDesc,
                                                   Task = ta.TaskName,
                                                   TaskDesc = ta.TaskDesc,
                                                   TaskCode = ta.TaskCode,
                                                   UpdatedDate = ro.UpdatedDate,
                                                   TaskPermissions = (dbContext.PiAppUserRoleResps.Where(s => s.ModuleId == ur.ModuleId && s.TaskId == ur.TaskId && s.RoleId == ro.AppRoleId)).Select(p => p.Permissions).FirstOrDefault()
                                               }).OrderByDescending(x => x.UpdatedDate).ToList();
                            var UserPubuList = (from pubu in dbContext.PiAppUserPuBus
                                                join pu in dbContext.TblParamProcessUnitMasters on pubu.ProcessUnit equals pu.Id
                                                join bu in dbContext.TblParamPuBusinessUnits on pubu.BusinessUnit equals bu.Id
                                                where pubu.AppUserId == userDetails.Id && pubu.Status != (byte)RecordStatus.Delete
                                                select new UserAuthorizationViewModel_UserDetail_UserPubuList
                                                {
                                                    BuId = pubu.BusinessUnit,
                                                    PuId = pubu.ProcessUnit,
                                                    BuName = bu.BusUnitFullName,
                                                    PuName = pu.PuName,
                                                    CountryId = pu.Country
                                                }).ToList();

                            if (userDetails != null)
                            {
                                if (Permissions.Count > 0)
                                {
                                    userDetails.RoleName = Permissions[0].RoleName;
                                }
                            }
                            data.Id = piHireUser.Id;
                            data.EmpId = piHireUser.EmployId;
                            data.UserDetails = userDetails;
                            data.UserTypeId = piHireUser.UserType;
                            if (piHireUser.UserType == (byte)UserType.SuperAdmin)
                            {
                                data.RoleName = "Super Admin";
                            }
                            else
                            {
                                data.RoleName = userDetails.RoleName;
                            }
                            data.ProfilePhoto = piHireUser.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + piHireUser.Id + "/ProfilePhoto/" + piHireUser.ProfilePhoto : string.Empty;
                            data.SessionTxnId = Guid.NewGuid().ToString();

                            data.setMicrosoftToken(obj.Token, obj.username, isSmtpWorking: await isSmtpWorking(piHireUser.Id, clientLocation));

                            return (obj.IsCompleted, obj.IsSuccess, data, "", Permissions, UserPubuList);
                        }
                    }
                    return (obj.IsCompleted, obj.IsSuccess, data, "", null, null);
                }
                else
                {
                    return (true, false, null, "Invalid request", null, null);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, string.Empty, ex);
                return (true, false, null, "Something went wrong", null, null);
            }
        }

        async Task<bool> isSmtpWorking(int UserId, System.Net.IPAddress clientLocation)
        {
            try
            {
                var smtpConfiguration = dbContext.PhUsersConfigs.Where(x => x.UserId == UserId && x.VerifyFlag == true).FirstOrDefault();

                var simpleEncrypt = new SimpleEncrypt();
                if (smtpConfiguration != null)
                {
                    var SmtpLoginName = smtpConfiguration.UserName;
                    var SmtpLoginPassword = simpleEncrypt.passwordDecrypt(smtpConfiguration.PasswordHash);

                    var smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        SmtpLoginName, SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl, SmtpLoginName, appSettings.smtpEmailConfig.SmtpFromName);

                    var mailBody = "piHire new login.";
                    if (clientLocation != null)
                        mailBody += "IP:" + clientLocation.ToString();
                    await smtp.SendMail(SmtpLoginName, "piHire Login", mailBody, string.Empty);
                    return true;
                }
            }
            catch (Exception e)
            {

            }
            return false;
        }

        public async Task<string> authenticateMicrosoftSetToken(string code, string state, string session_state, string error)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var obj = MicrosoftOutlookAuthReqData.FirstOrDefault(da => da.code == state);
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method: code:" + code + ",state:" + state + ",session_state:" + session_state + ",error:" + error);
                var tokenObj = Common._3rdParty.MicrosoftOutlook.getRefreshToken(code, MicrosoftOutlookRedirectUrl, logger);
                if (obj != null)
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "tokenObj:" + Newtonsoft.Json.JsonConvert.SerializeObject(tokenObj) + " of " + Newtonsoft.Json.JsonConvert.SerializeObject(obj));
                    obj.IsSuccess = string.IsNullOrEmpty(tokenObj.token) == false;
                    if (obj.IsSuccess)
                    {
                        Common.Meeting.Teams tm = new Common.Meeting.Teams(tokenObj.token, MicrosoftOutlookRedirectUrl, logger);
                        Common.Meeting.TeamCalendarProfileViewModel prfl = null;
                        try
                        {
                            prfl = await tm.getProfile();
                        }
                        catch (Exception)
                        { }
                        obj.username = prfl.mail;
                        obj.IsSuccess = prfl != null;
                        obj.Token = tm.accessToken;
                    }
                    obj.IsCompleted = true;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, "code:" + code + ",state:" + state + ",session_state:" + session_state + ",error:" + error, ex);
                obj.IsCompleted = true;
                obj.IsSuccess = false;
                return "Something went wrong";
            }
        }
        #endregion

        public async Task<(string message, bool status)> UpdateUserLogs(UpdateLogsViewModel model)
        {
            try
            {
                using (var trans = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        PiUserTxnLog txnLog = await dbContext.PiUserTxnLogs.FirstOrDefaultAsync(da =>
                        da.ApplicationId == _applicationId
                            && da.SessionId == model.SessionId && da.UserId == model.UserId
                            && da.Status == (byte)RecordStatus.Active && da.TxnOutDate.HasValue == false);
                        if (txnLog == null)
                        {
                            txnLog = new PiUserTxnLog
                            {
                                ApplicationId = _applicationId,
                                DeviceType = model.DeviceType,
                                DeviceUid = model.DeviceUiId,
                                Ipaddress = model.IpAddress,
                                Lat = model.Lat,
                                Long = model.Longi,
                                Status = (byte)RecordStatus.Active,
                                UserId = model.UserId,
                                TxnStartDate = CurrentTime,
                                TxnDesc = string.Empty,
                                TxnOutDate = null,
                                SessionId = model.SessionId
                            };
                            dbContext.PiUserTxnLogs.Add(txnLog);
                        }
                        else
                        {
                            txnLog.DeviceUid = model.DeviceUiId;
                        }
                        await dbContext.SaveChangesAsync();

                        PiUserLog userLog = dbContext.PiUserLogs.FirstOrDefault(f => f.ApplicationId == _applicationId && f.UserId == model.UserId);
                        if (userLog == null)
                        {
                            userLog = new PiUserLog();
                            userLog.ApplicationId = _applicationId;
                            userLog.UserId = model.UserId;
                            userLog.LastTxnId = txnLog.Id;
                            userLog.LoginStatus = model.LoginStatus;
                            dbContext.PiUserLogs.Add(userLog);
                        }
                        else
                        {
                            userLog.LastTxnId = txnLog.Id;
                            userLog.LoginStatus = model.LoginStatus;
                        }
                        await dbContext.SaveChangesAsync();
                        trans.Commit();
                        return ("Updated log successfully.", true);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return ("Failed to update the log.", false);
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion

        public async Task<List<AppUserViewModel>> GetAppUsers()
        {
            try
            {
                var resp = new List<AppUserViewModel>();
                IQueryable<AppUserViewModel> appUsers = dbContext.PiHireUsers.Where(s => s.UserType != (byte)UserType.Candidate
              && s.Status != (byte)RecordStatus.Delete).OrderByDescending(o => o.CreatedDate).Select(s => new AppUserViewModel
              {
                  Id = s.Id,
                  UserType = s.UserType,
                  CreatedBy = s.CreatedBy,
                  CreatedDate = s.CreatedDate,
                  EmailSignature = s.EmailSignature,
                  EmployId = s.EmployId,
                  FirstName = s.FirstName,
                  LastName = s.LastName,
                  Location = s.Location,
                  LocationId = s.LocationId,
                  MobileNumber = s.MobileNumber,
                  Status = s.Status,
                  UpdatedBy = s.UpdatedBy,
                  UpdatedDate = s.UpdatedDate,
                  UserName = s.UserName,
                  UserRoleId = s.UserRoleId,
                  UserRoleName = s.UserRoleName,
                  Nationality = s.Nationality,
                  ShiftId = s.ShiftId,
                  ProfilePhoto = s.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + s.Id + "/ProfilePhoto/" + s.ProfilePhoto : string.Empty
              });

                if (Usr.UserTypeId != (byte)UserType.SuperAdmin)
                {
                    resp = await appUsers.Where(x => x.UserType != (byte)UserType.SuperAdmin).ToListAsync();
                }
                else
                {
                    resp = await appUsers.ToListAsync();
                }
                return resp;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<UpdateSignatureViewModel> UpdateSignature(UpdateSignatureRequestViewModel model)
        {
            try
            {
                var user = await dbContext.PiHireUsers.FirstOrDefaultAsync(s => s.Id == model.UserId && s.Status != (byte)RecordStatus.Delete);
                if (user == null)
                {
                    return new UpdateSignatureViewModel { UserId = 0, Status = false, Message = "User not found." };
                }
                user.EmailSignature = model.Signature;
                await dbContext.SaveChangesAsync();
                return new UpdateSignatureViewModel { UserId = user.Id, Status = true, Message = "Updated successfully." };
            }
            catch (Exception)
            {
                throw;
            }
        }

        
        public async Task<GetResponseViewModel<Tuple<string, bool>>> UpdateUserProfilePhoto(UserProfilePhotoViewModel userProfilePhotoViewModel)
        {
            var respModel = new GetResponseViewModel<Tuple<string, bool>>();
            var response = Tuple.Create(string.Empty, false);
            var UserId = Usr.Id;
            try
            {
                var user = await dbContext.PiHireUsers.FirstOrDefaultAsync(s => s.Id == userProfilePhotoViewModel.UserId);
                if (user == null)
                {
                    response = Tuple.Create("User not found", false);
                    respModel.SetResult(response);
                    respModel.Status = true;
                }
                else
                {
                    string webRootPath = _environment.ContentRootPath + "\\Employee" + "\\" + user.Id + "\\ProfilePhoto";

                    // checking for folder is available or not 
                    if (!System.IO.Directory.Exists(webRootPath))
                    {
                        System.IO.Directory.CreateDirectory(webRootPath);
                    }

                    // photo
                    if (userProfilePhotoViewModel.Photo != null)
                    {
                        if (userProfilePhotoViewModel.Photo.Length > 0)
                        {
                            var fileName = Path.GetFileName(userProfilePhotoViewModel.Photo.FileName);
                            fileName = fileName.Replace(" ", "_");
                            if (fileName.Length > 100)
                            {
                                fileName = fileName.Substring(0, 99);
                            }

                            var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await userProfilePhotoViewModel.Photo.CopyToAsync(fileStream);
                            }

                            user.ProfilePhoto = fileName;
                            user.UpdatedDate = CurrentTime;
                            user.UpdatedBy = UserId;

                            dbContext.PiHireUsers.Update(user);
                            await dbContext.SaveChangesAsync();

                            string PicURL = string.Empty;
                            PicURL = appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + user.Id + "/ProfilePhoto/" + fileName;
                            response = Tuple.Create(PicURL, true);

                            respModel.SetResult(response);
                            respModel.Status = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return respModel;
        }

        public async Task<GetPUBUViewModel> GetUserPUBU(int userId)
        {
            try
            {
                var appUser = await dbContext.PiHireUsers.FirstOrDefaultAsync(f => f.Id == userId && f.Status != (byte)RecordStatus.Delete);
                if (appUser == null)
                {
                    return null;
                }
                var data = new GetPUBUViewModel();
                var UserBuPu = dbContext.PiAppUserPuBus.Where(s => s.AppUserId == appUser.Id && s.Status == (byte)RecordStatus.Active).OrderBy(o => o.ProcessUnit).ToList();

                if (UserBuPu == null)
                {
                    return null;
                }

                var puId = UserBuPu.Select(s => s.ProcessUnit).ToList();
                var buId = UserBuPu.Select(s => s.BusinessUnit).ToList();

                data.BUs = dbContext.TblParamPuBusinessUnits.Where(b => buId.Contains(b.Id)).Select(s => new GetBUViewModel
                {
                    Id = s.Id,
                    Name = s.BusUnitFullName,
                    ShortName = s.BusUnitCode,
                    PUID = s.PuId
                }).OrderBy(o => o.Name).ToList();

                data.PUs = dbContext.TblParamProcessUnitMasters.Where(p => puId.Contains(p.Id)).Select(s => new GetPUViewModel
                {
                    Id = s.Id,
                    Name = s.PuName,
                    ShortName = s.ShortName
                }).OrderBy(o => o.Name).ToList();

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(string message, bool status)> ForgotPassword(ForgotPasswordRequest model)
        {
            try
            {
                var user = dbContext.PiHireUsers.FirstOrDefault(s => s.UserName == model.Email.Trim() && s.Status == (byte)RecordStatus.Active && s.UserType != (byte)UserType.Candidate);
                if (user == null)
                {
                    return ("Email not found.", false);
                }
                user.VerifyToken = RandomTokenString();
                user.TokenExpiryDate = DateTime.UtcNow.AddDays(1);

                await dbContext.SaveChangesAsync();

                string AppUrl = string.Empty;
                string Username = user.FirstName + " " + user.LastName;
                if (user.UserType == (byte)UserType.Candidate)
                {
                    AppUrl = appSettings.AppSettingsProperties.CandidateAppUrl;
                }
                else
                {
                    AppUrl = appSettings.AppSettingsProperties.HireAppUrl;
                }

                var mailBody = EmailTemplates.User_ForgotPassword_Template(Username, AppUrl + "/reset-password?token=" + user.VerifyToken, this.appSettings.AppSettingsProperties.HireAppUrl);

                SmtpMailing smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        appSettings.smtpEmailConfig.SmtpLoginName, appSettings.smtpEmailConfig.SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl,
                        appSettings.smtpEmailConfig.SmtpFromEmail, appSettings.smtpEmailConfig.SmtpFromName);
                _ = smtp.SendMail(user.EmailId, EmailTemplates.GetSubject(EmailTypes.ForgotPassword), mailBody, string.Empty);
                return ("", true);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string RandomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }


        public async Task<(string message, bool status)> PasswordReset(int empId)
        {
            try
            {
                var user = dbContext.PiHireUsers.FirstOrDefault(s => s.Id == empId && s.UserType != (byte)UserType.Candidate);
                if (user == null)
                {
                    return ("Your requested user is not available in database.", false);
                }
                var generator = new RandomGenerator();
                var pswd = generator.RandomPassword(8);
                var HashPswd = Hashification.SHA(pswd);

                //using var client = new HttpClientService();
                //var response = await client.PostAsync(appSettings.AppSettingsProperties.GatewayUrl, "/api/GWService/user/ResetPassword", new GWResetPasswordViewModel
                //{
                //    ApplicationId = _applicationId,
                //    EncryptedToken = HashPswd,
                //    Password = pswd,
                //    userName = user.UserName
                //});
                //if (!response.IsSuccessStatusCode)
                //{
                //    return ("Failed to reset password.", false);
                //}

                user.VerifyToken = null;
                user.TokenExpiryDate = null;
                user.PasswordHash = HashPswd;
                user.VerifiedFlag = true;

                await dbContext.SaveChangesAsync();

                string AppUrl = string.Empty;
                AppUrl = appSettings.AppSettingsProperties.HireAppUrl;

                var mailBody = EmailTemplates.User_RestPassword2_Template(AppUrl, appSettings.AppSettingsProperties.HireAppUrl, user.UserName, pswd);

                SmtpMailing smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        appSettings.smtpEmailConfig.SmtpLoginName, appSettings.smtpEmailConfig.SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl,
                        appSettings.smtpEmailConfig.SmtpFromEmail, appSettings.smtpEmailConfig.SmtpFromName);
                _ = smtp.SendMail(user.UserName, EmailTemplates.GetSubject(EmailTypes.ResetPassword), mailBody, string.Empty);
                return (string.Empty, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(string message, bool status)> ResetPassword(ResetPasswordRequest model)
        {
            try
            {
                var user = dbContext.PiHireUsers.FirstOrDefault(s => s.VerifyToken == model.Token.Trim() && s.TokenExpiryDate > DateTime.UtcNow);
                if (user == null)
                {
                    return ("Your reset password session is expire.", false);
                }
                var HashPswd = Hashification.SHA(model.Password);

                //using var client = new HttpClientService();
                //var response = await client.PostAsync(appSettings.AppSettingsProperties.GatewayUrl, "/api/GWService/user/ResetPassword", new GWResetPasswordViewModel
                //{
                //    ApplicationId = _applicationId,
                //    EncryptedToken = HashPswd,
                //    Password = model.Password,
                //    userName = user.UserName
                //});
                //if (!response.IsSuccessStatusCode)
                //{
                //    return ("Failed to reset password.", false);
                //}

                user.VerifyToken = null;
                user.TokenExpiryDate = null;
                user.PasswordHash = HashPswd;
                user.VerifiedFlag = true;

                await dbContext.SaveChangesAsync();

                string AppUrl = string.Empty;
                string Username = user.FirstName + " " + user.LastName;
                if (user.UserType == (byte)UserType.Candidate)
                {
                    AppUrl = appSettings.AppSettingsProperties.CandidateAppUrl;
                }
                else
                {
                    AppUrl = appSettings.AppSettingsProperties.HireAppUrl;
                }

                var mailBody = EmailTemplates.User_RestPassword_Template(Username, AppUrl, appSettings.AppSettingsProperties.HireAppUrl);

                SmtpMailing smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        appSettings.smtpEmailConfig.SmtpLoginName, appSettings.smtpEmailConfig.SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl,
                        appSettings.smtpEmailConfig.SmtpFromEmail, appSettings.smtpEmailConfig.SmtpFromName);
                _ = smtp.SendMail(user.UserName, EmailTemplates.GetSubject(EmailTypes.ResetPassword), mailBody, string.Empty);
                return (string.Empty, true);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<(string message, bool status)> DisconnectUser(string connectionId)
        {
            try
            {
                int extraTime = appSettings.AppSettingsProperties?.OnlineStatusValidMins ?? 1441;
                var expiredStartTime = CurrentTime.AddMinutes(extraTime * -1);

                var userTxns = await dbContext.PiUserTxnLogs.Where(s => s.ApplicationId == _applicationId
                                        && s.Status == (byte)RecordStatus.Active
                                        && s.UserId == Usr.Id && s.TxnOutDate.HasValue == false
                                         && (s.DeviceUid == connectionId || s.TxnStartDate < expiredStartTime)).ToListAsync();
                //if (userTxns == null || userTxns.Count == 0)
                //{
                //    return ("already disconnected", false);
                //}
                foreach (var userTxn in userTxns)
                    userTxn.TxnOutDate = CurrentTime;
                await dbContext.SaveChangesAsync();
                //var ids = userTxns.Select(da => da.Id);
                if (!await dbContext.PiUserTxnLogs.Where(da => da.ApplicationId == _applicationId && da.UserId == Usr.Id && /*(ids.Contains(da.Id) == false) &&*/ da.TxnOutDate.HasValue == false).AnyAsync())
                {
                    var loginLog = await dbContext.PiUserLogs.FirstOrDefaultAsync(f => f.UserId == Usr.Id);
                    //if (loginLog == null)
                    //{
                    //    return ("already disconnected", false);
                    //}
                    loginLog.LoginStatus = false;
                }
                await dbContext.SaveChangesAsync();
                return ("disconnected successfully", true);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task<List<(int userId, string email)>> DisconnectSessionOutUsers()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
                var userIds = new List<int>();
                #region Disconnect users whos time exceeded
                {
                    var tm = CurrentTime.AddMinutes(-1 * (appSettings.AppSettingsProperties.JwtValidityMinutes + 5));
                    var trns = await dbContext.PiUserTxnLogs.Where(da => da.TxnOutDate.HasValue == false && da.TxnStartDate < tm).ToListAsync();
                    foreach (var trn in trns)
                    {
                        trn.TxnOutDate = CurrentTime;
                    }
                    await dbContext.SaveChangesAsync();
                    foreach (var usr in (await dbContext.PiUserLogs.Where(f => f.LoginStatus != false).ToListAsync()))
                        if (!await dbContext.PiUserTxnLogs.Where(da => da.ApplicationId == usr.ApplicationId && da.UserId == usr.UserId && da.TxnOutDate.HasValue == false).AnyAsync())
                        {
                            {
                                usr.LoginStatus = false;
                                if (usr.UserId.HasValue)
                                    userIds.Add(usr.UserId.Value);
                            }
                        }
                    await dbContext.SaveChangesAsync();
                }
                #endregion
                logger.Log(LogLevel.Error, LoggingEvents.Authenticate, "disconnected users:" + string.Join(",", userIds));
                var lst = await dbContext.PiHireUsers.Where(da => userIds.Contains(da.Id)).Select(da => new { da.Id, da.UserName }).ToListAsync();
                return lst.Select(da => (da.Id, da.UserName)).ToList();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.Authenticate, "", ex);
                return new List<(int userId, string email)>();
            }
        }
    }
}
