using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PiHire.BAL.Common.Http;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using PiHire.DAL.Entities;
using PiHire.BAL.Common;
using PiHire.DAL.Models;
using System.Diagnostics;
using PiHire.Utilities.Communications.Emails;

namespace PiHire.BAL.Repositories
{
    public class CommonRepository : BaseRepository, ICommonRepository
    {
        readonly Logger logger;

        private readonly IWebHostEnvironment _environment;


        public CommonRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<CommonRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        #region PU & BU


        public async Task<GetResponseViewModel<List<GetBUViewModel>>> GetBUAsync(GetBURequestVM model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<GetBUViewModel>>();
            try
            {

                var data = new List<GetBUViewModel>();
                string puIds = string.Join(",", model.PuIds).ToString();

                data = await dbContext.GetBUs(puIds);

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


        public async Task<GetResponseViewModel<List<GetPUViewModel>>> GetPUAsync()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<GetPUViewModel>>();
            try
            {

                var data = new List<GetPUViewModel>();

                data = await dbContext.GetPUs();

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



        public async Task<GetResponseViewModel<List<GetBUViewModel>>> UserBuListAsync(GetBURequestVM model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<GetBUViewModel>>();
            int UserId = Usr.Id;
            int UserType = Usr.UserTypeId;
            try
            {

                var data = new List<GetBUViewModel>();
                string puIds = string.Join(",", model.PuIds).ToString();

                data = await dbContext.GetUserBuListAsync(puIds, UserType, UserId);

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


        public async Task<GetResponseViewModel<List<GetUserPUViewModel>>> UserPuListAsync()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<GetUserPUViewModel>>();
            int UserId = Usr.Id;
            int UserType = Usr.UserTypeId;
            try
            {

                var data = new List<GetUserPUViewModel>();

                data = await dbContext.GetUserPuListAsync(UserType, UserId);

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

        #endregion
        public async Task<GetResponseViewModel<List<SelectWithCodeViewModel>>> GetJobStatusAsync()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<SelectWithCodeViewModel>>();
            try
            {
                var data = await dbContext.PhJobStatusSes.Where(da => da.Status == (byte)RecordStatus.Active)
                    .Select(da => new SelectWithCodeViewModel { Id = da.Id, Name = da.Title, Code = da.Jscode }).ToListAsync();
                respModel.SetResult(data);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, "", respModel.Meta.RequestID, ex);

                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }

        #region User Remarks 
        public async Task<GetResponseViewModel<List<UserRemarksViewModel>>> GetUserRemarksList(UserRemarksRequestModel userRemarksRequestModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<UserRemarksViewModel>>();
            int UserId = Usr.Id;
            try
            {


                if (userRemarksRequestModel.FromDate == null)
                {
                    var dt = calcDate(userRemarksRequestModel.DateFilter);
                    userRemarksRequestModel.FromDate = dt.fmDt;
                    userRemarksRequestModel.ToDate = dt.toDt;
                }
                else
                {
                    if (userRemarksRequestModel.ToDate == null)
                    {
                        userRemarksRequestModel.ToDate = CurrentTime;
                    }
                }

                var response = await (from remark in dbContext.PhUsersRemarks
                                      join user in dbContext.PiHireUsers on remark.CreatedBy equals user.Id
                                      where remark.Status != (byte)RecordStatus.Delete && remark.UserId == userRemarksRequestModel.UserId && remark.CreatedDate.Date >= userRemarksRequestModel.FromDate.Value && remark.CreatedDate.Date <= userRemarksRequestModel.ToDate.Value
                                      select new UserRemarksViewModel
                                      {
                                          Title = remark.Title,
                                          CreatedDate = remark.CreatedDate,
                                          Id = remark.Id,
                                          NoteDesc = remark.NotesDesc,
                                          CreatedBy = user.CreatedBy,
                                          CreatedByName = user.FirstName + " " + user.LastName
                                      }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                respModel.Status = true;
                respModel.SetResult(response);
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

        public async Task<CreateResponseViewModel<string>> CreateUserRemark(CreateUserRemarksModel createUserRemarksModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            int UserId = Usr.Id;

            try
            {
                

                var phUsersRemarks = new PhUsersRemark
                {
                    CreatedBy = UserId,
                    UserId = createUserRemarksModel.UserId,
                    CreatedDate = CurrentTime,
                    NotesDesc = createUserRemarksModel.NoteDesc,
                    Status = (byte)RecordStatus.Active,
                    Title = createUserRemarksModel.Title
                };
                await dbContext.PhUsersRemarks.AddAsync(phUsersRemarks);
                await dbContext.SaveChangesAsync();

                respModel.Status = true;
                respModel.SetResult(message);

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<UpdateResponseViewModel<string>> UpdateUserRemark(UpdateUserRemarksModel updateUserRemarksModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            int UserId = Usr.Id;
            try
            {
                

                var phUsersRemarks = await dbContext.PhUsersRemarks.Where(x => x.Id == updateUserRemarksModel.Id).FirstOrDefaultAsync();
                if (phUsersRemarks != null)
                {
                    phUsersRemarks.NotesDesc = updateUserRemarksModel.NoteDesc;
                    phUsersRemarks.Title = updateUserRemarksModel.Title;
                    phUsersRemarks.UpdatedBy = UserId;
                    phUsersRemarks.UpdatedDate = CurrentTime;

                    dbContext.PhUsersRemarks.Update(phUsersRemarks);
                    await dbContext.SaveChangesAsync();


                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "The Remarks are not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<UpdateResponseViewModel<string>> DeleteUserRemark(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Deleted Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var phUsersRemarks = await dbContext.PhUsersRemarks.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (phUsersRemarks != null)
                {
                    phUsersRemarks.Status = (byte)RecordStatus.Delete;
                    phUsersRemarks.UpdatedBy = UserId;
                    phUsersRemarks.UpdatedDate = CurrentTime;

                    dbContext.PhUsersRemarks.Update(phUsersRemarks);
                    await dbContext.SaveChangesAsync();

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "The Remarks are not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        #endregion

        #region Timezones
        public async Task<GetResponseViewModel<List<Common.Meeting.TeamCalendarTimeZoneViewModel_value>>> getTeamTimeZones()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<Common.Meeting.TeamCalendarTimeZoneViewModel_value>>();
            string message = string.Empty;
            try
            {
                
                var tknObj = await dbContext.PhIntegrationsSes.Where(da => da.Status == (byte)RecordStatus.Active && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.Office365)
                                       .Select(da => new { da.RefreshToken, da.ReDirectUrl }).FirstOrDefaultAsync();
                if (tknObj == null || string.IsNullOrWhiteSpace(tknObj.RefreshToken))
                {
                    message = "Microsoft token not exist/not valid";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    logger.Log(LogLevel.Information, LoggingEvents.GetItem, message);
                }
                else
                {
                    Common.Meeting.Teams met = new Common.Meeting.Teams(tknObj.RefreshToken, tknObj.ReDirectUrl, logger);
                    var zones = await met.getTimezones();
                    var zonesValue = zones.value.OrderBy(da => da.displayName).ToList();
                    if (zonesValue.FirstOrDefault(da => da.displayName.Contains("muscat", StringComparison.InvariantCultureIgnoreCase)) != null)
                    {
                        var muscat = zonesValue.FirstOrDefault(da => da.displayName.Contains("muscat", StringComparison.InvariantCultureIgnoreCase));
                        var index = zonesValue.IndexOf(muscat);
                        zonesValue.RemoveAt(index);
                        zonesValue.Insert(0, muscat);
                    }
                    if (zonesValue.FirstOrDefault(da => da.displayName.Contains("kolkata", StringComparison.InvariantCultureIgnoreCase)) != null)
                    {
                        var muscat = zonesValue.FirstOrDefault(da => da.displayName.Contains("kolkata", StringComparison.InvariantCultureIgnoreCase));
                        var index = zonesValue.IndexOf(muscat);
                        zonesValue.RemoveAt(index);
                        zonesValue.Insert(1, muscat);
                    }
                    respModel.SetResult(zonesValue);
                }
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
        public async Task<GetResponseViewModel<List<Common.Meeting.TeamCalendarTimeZoneViewModel_value>>> getGoogleTimeZones()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<Common.Meeting.TeamCalendarTimeZoneViewModel_value>>();
            string message = string.Empty;
            try
            {
                
                var tkn = await dbContext.PhIntegrationsSes.Where(da => da.Status == (byte)RecordStatus.Active && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.GoogleMeet)
                                    .Select(da => da.RefreshToken).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(tkn))
                {
                    message = "Google token not exist/not valid";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    logger.Log(LogLevel.Information, LoggingEvents.GetItem, message);
                }
                else
                {
                    var zones = Common.Meeting.GoogleMeet.getTimezones();

                    var zonesValue = zones.OrderBy(da => da.displayName).ToList();
                    if (zonesValue.FirstOrDefault(da => da.displayName.Contains("muscat", StringComparison.InvariantCultureIgnoreCase)) != null)
                    {
                        var muscat = zonesValue.FirstOrDefault(da => da.displayName.Contains("muscat", StringComparison.InvariantCultureIgnoreCase));
                        var index = zonesValue.IndexOf(muscat);
                        zonesValue.RemoveAt(index);
                        zonesValue.Insert(0, muscat);
                    }
                    if (zonesValue.FirstOrDefault(da => da.displayName.Contains("kolkata", StringComparison.InvariantCultureIgnoreCase)) != null)
                    {
                        var muscat = zonesValue.FirstOrDefault(da => da.displayName.Contains("kolkata", StringComparison.InvariantCultureIgnoreCase));
                        var index = zonesValue.IndexOf(muscat);
                        zonesValue.RemoveAt(index);
                        zonesValue.Insert(1, muscat);
                    }
                    respModel.SetResult(zonesValue);
                }
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
        #endregion

        public GetResponseViewModel<CommonEnumViewModel> GetTypes()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CommonEnumViewModel>();
            try
            {

                var Enums = new CommonEnumViewModel
                {
                    ActionMode = new List<SelectViewModel>(),
                    ActionType = new List<SelectViewModel>(),
                    SendMode = new List<SelectViewModel>(),
                    SendTo = new List<SelectViewModel>(),
                    Gender = new List<SelectViewModel>(),
                    SourceType = new List<SelectViewModel>(),
                    FileGroup = new List<SelectViewModel>(),
                    MessageType = new List<SelectViewModel>(),
                    ProfileType = new List<SelectViewModel>(),
                    SentBy = new List<SelectViewModel>(),
                    CandOverallStatus = new List<SelectViewModel>(),
                    ClreviewStatus = new List<SelectViewModel>(),
                    InterviewStatus = new List<SelectViewModel>(),
                    ModeOfInterview = new List<SelectViewModel>(),
                    ScheduledBy = new List<SelectViewModel>(),
                    DocStatus = new List<SelectViewModel>(),
                    ActivityType = new List<SelectViewModel>(),
                    AuditType = new List<SelectViewModel>()
                };
                foreach (WorkflowActionMode r in Enum.GetValues(typeof(WorkflowActionMode)))
                {
                    Enums.ActionMode.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (WorkflowActionTypes r in Enum.GetValues(typeof(WorkflowActionTypes)))
                {
                    Enums.ActionType.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (SendMode r in Enum.GetValues(typeof(SendMode)))
                {
                    Enums.SendMode.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (UserType r in Enum.GetValues(typeof(UserType)))
                {
                    Enums.SendTo.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (Gender r in Enum.GetValues(typeof(Gender)))
                {
                    Enums.Gender.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (SourceType r in Enum.GetValues(typeof(SourceType)))
                {
                    var SourceType = new SelectViewModel
                    {
                        Id = (int)r,
                        Name = r.ToString()
                    };
                    if (SourceType.Name == "NaukriGulf")
                    {
                        SourceType.Name = "Naukri Gulf";
                    }
                    if (SourceType.Name == "MonsterIndia")
                    {
                        SourceType.Name = "Monster India";
                    }
                    if (SourceType.Name == "MonsterGulf")
                    {
                        SourceType.Name = "Monster Gulf";
                    }
                    if (SourceType.Name == "PersonalContact")
                    {
                        SourceType.Name = "Personal Contact";
                    }
                    Enums.SourceType.Add(SourceType);
                };
                foreach (FileGroup r in Enum.GetValues(typeof(FileGroup)))
                {
                    Enums.FileGroup.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (MessageType r in Enum.GetValues(typeof(MessageType)))
                {
                    Enums.MessageType.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (ProfileType r in Enum.GetValues(typeof(ProfileType)))
                {
                    Enums.ProfileType.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (SentBy r in Enum.GetValues(typeof(SentBy)))
                {
                    Enums.SentBy.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (CandOverallStatus r in Enum.GetValues(typeof(CandOverallStatus)))
                {
                    Enums.CandOverallStatus.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };

                foreach (ClreviewStatus r in Enum.GetValues(typeof(ClreviewStatus)))
                {
                    Enums.ClreviewStatus.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (InterviewStatus r in Enum.GetValues(typeof(InterviewStatus)))
                {
                    Enums.InterviewStatus.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (ModeOfInterview r in Enum.GetValues(typeof(ModeOfInterview)))
                {
                    Enums.ModeOfInterview.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (ScheduledBy r in Enum.GetValues(typeof(ScheduledBy)))
                {
                    Enums.ScheduledBy.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (DocStatus r in Enum.GetValues(typeof(DocStatus)))
                {
                    Enums.DocStatus.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };

                foreach (LogActivityType r in Enum.GetValues(typeof(LogActivityType)))
                {
                    Enums.ActivityType.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };
                foreach (AuditActivityType r in Enum.GetValues(typeof(AuditActivityType)))
                {
                    Enums.AuditType.Add(new SelectViewModel { Id = (int)r, Name = r.ToString() });
                };

                respModel.SetResult(Enums);
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

        public async Task<GetResponseViewModel<List<DocTypesViewModel>>> DocTypes(int[] GroupId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<DocTypesViewModel>>();
            try
            {


                var RefData = new List<DocTypesViewModel>();

                RefData = await (from data in dbContext.PhRefMasters
                                 where GroupId.Contains(data.GroupId) && data.Status == (byte)RecordStatus.Active
                                 select new DocTypesViewModel
                                 {
                                     Id = data.Id,
                                     GroupId = data.GroupId,
                                     Rmdesc = data.Rmdesc,
                                     Rmtype = data.Rmtype,
                                     Rmvalue = data.Rmvalue
                                 }).OrderBy(x => x.Rmvalue).ToListAsync();
                foreach (var item in RefData)
                {
                    item.FileGroup = (int)Enum.Parse(typeof(FileGroup), item.Rmdesc);
                }

                respModel.SetResult(RefData);
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

        public async Task<GetResponseViewModel<List<RefDataViewModel>>> GetRefData(int[] GroupId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<RefDataViewModel>>();
            try
            {


                var RefData = new List<RefDataViewModel>();

                RefData = await (from data in dbContext.PhRefMasters
                                 where GroupId.Contains(data.GroupId) && data.Status == (byte)RecordStatus.Active
                                 select new RefDataViewModel
                                 {
                                     Id = data.Id,
                                     GroupId = data.GroupId,
                                     Rmdesc = data.Rmdesc,
                                     Rmtype = data.Rmtype,
                                     Rmvalue = data.Rmvalue
                                 }).OrderBy(x => x.Rmvalue).ToListAsync();

                respModel.SetResult(RefData);
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


        public async Task<GetResponseViewModel<List<CountryModel>>> GetCountries()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CountryModel>>();
            try
            {


                var countrys = new List<CountryModel>();

                countrys = await dbContext.GeCountryList();

                respModel.SetResult(countrys);
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


        public async Task<GetResponseViewModel<List<CityModel>>> GetCities(int CountryId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CityModel>>();
            try
            {

                var cities = new List<CityModel>();

                cities = await dbContext.GeCityList();
                cities = cities.Where(x => x.CountryId == CountryId).ToList();

                respModel.SetResult(cities);
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


        public async Task<GetResponseViewModel<List<CityModel>>> GetCities(int?[] CountryIds)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CityModel>>();
            try
            {

                var cities = new List<CityModel>();

                cities = await dbContext.GeCityList();
                cities = cities.Where(x => CountryIds.Contains(x.CountryId)).ToList();

                respModel.SetResult(cities);
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

        public async Task<GetResponseViewModel<List<SalaryCalculatorCountryViewModel>>> SalaryCalculatorCountriesAsync(bool IsFrom = false, bool IsTo = false, int? FromCountryId = null)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<SalaryCalculatorCountryViewModel>>();
            try
            {
                var qry = dbContext.PhCountries.Where(da => string.IsNullOrEmpty(da.Currency) == false);
                if (IsFrom)
                {
                    qry = qry.Join(dbContext.PhCurrencyExchangeRates, da => da.Currency.Trim(), da2 => da2.FromCurrency.Trim(), (da, da2) => da).Distinct();
                }
                if (IsTo)
                {
                    qry = qry.Join(dbContext.PhCurrencyExchangeRates, da => da.Currency.Trim(), da2 => da2.ToCurrency.Trim(), (da, da2) => da).Distinct();
                }
                if (FromCountryId.HasValue)
                {
                    var FromCountryCurrency = await dbContext.PhCountries.Where(da => da.Id == FromCountryId).Select(da => da.Currency).FirstOrDefaultAsync();
                    qry = qry.Join(dbContext.PhCurrencyExchangeRates.Where(dai => dai.FromCurrency.Trim() == FromCountryCurrency), da => da.Currency.Trim(), da2 => da2.ToCurrency.Trim(), (da, da2) => da).Distinct();
                }

                var countrys = await qry.Select(da => new SalaryCalculatorCountryViewModel { Id = da.Id, Currency = da.Currency, Iso = da.Iso, Name = da.Name, Nicename = da.Nicename, Iso3 = da.Iso3, CurrSymbol = da.CurrSymbol, Numcode = da.Numcode, Phonecode = da.Phonecode }).ToListAsync();

                respModel.SetResult(countrys);
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
        public async Task<GetResponseViewModel<List<string>>> GetCityWiseBenefitsAsync(int CityId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<string>>();
            try
            {
                var benefits = await dbContext.PhCityWiseBenefits.Where(da => da.Status == (byte)RecordStatus.Active && da.CityId == CityId).Select(da => da.BenefitTitle).ToListAsync();

                respModel.SetResult(benefits);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"CityId:{CityId}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<List<CityWiseBenefitViewModel>>> GetCityWiseBenefitListAsync(int CityId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CityWiseBenefitViewModel>>();
            try
            {
                var benefits = await dbContext.PhCityWiseBenefits.Where(da => da.Status == (byte)RecordStatus.Active && da.CityId == CityId).Select(da => new CityWiseBenefitViewModel { Id = da.Id, BenefitTitle = da.BenefitTitle, BenefitDesc = da.BenefitDesc, CityId = da.CityId }).ToListAsync();

                respModel.SetResult(benefits);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"CityId:{CityId}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<string>> SetCityWiseBenefitsAsync(CityWiseBenefitViewModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<string>();
            try
            {
                if (await dbContext.PhCityWiseBenefits.Where(da => da.Id != model.Id && da.CityId == model.CityId && da.BenefitTitle == model.BenefitTitle.Trim()).CountAsync() > 0)
                {
                    respModel.SetError(ApiResponseErrorCodes.InvalidBodyContent, $"'{model.BenefitTitle.Trim()}' already exist for this city", true);
                }
                else
                {
                    var dbModel_benefit = await dbContext.PhCityWiseBenefits.FirstOrDefaultAsync(da => da.Id == model.Id);
                    if (dbModel_benefit == null)
                    {
                        dbModel_benefit = new PhCityWiseBenefit
                        {
                            CityId = model.CityId,
                            BenefitTitle = model.BenefitTitle.Trim(),
                            BenefitDesc = model.BenefitDesc.Trim(),
                            CreatedDate = CurrentTime,
                            CreatedBy = Usr.Id,
                            Status = (byte)RecordStatus.Active
                        };
                        dbContext.PhCityWiseBenefits.Add(dbModel_benefit);
                    }
                    else
                    {
                        dbModel_benefit.BenefitTitle = model.BenefitTitle.Trim();
                        dbModel_benefit.BenefitDesc = model.BenefitDesc.Trim();
                        dbModel_benefit.UpdatedBy = Usr.Id;
                        dbModel_benefit.UpdatedDate = CurrentTime;
                    }
                    await dbContext.SaveChangesAsync();

                    respModel.SetResult(string.Empty);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"model:{JsonConvert.SerializeObject(model)}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<List<string>>> GetCountryWiseBenefitsAsync(int CountryId, bool? isSalaryWise)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<string>>();
            try
            {
                var qry = dbContext.PhCountryWiseBenefits.Where(da => da.Status == (byte)RecordStatus.Active && da.CountryId == CountryId);
                if (isSalaryWise.HasValue)
                    qry = qry.Where(da => da.IsSalaryWise == isSalaryWise);
                var benefits = await qry.Select(da => da.BenefitTitle).ToListAsync();

                respModel.SetResult(benefits);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"CountryId:{CountryId}, isSalaryWise:{isSalaryWise}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<List<CountryWiseBenefitViewModel>>> GetCountryWiseBenefitListAsync(int CountryId, bool? isSalaryWise)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CountryWiseBenefitViewModel>>();
            try
            {
                var qry = dbContext.PhCountryWiseBenefits.Where(da => da.Status == (byte)RecordStatus.Active && da.CountryId == CountryId);
                if (isSalaryWise.HasValue)
                    qry = qry.Where(da => da.IsSalaryWise == isSalaryWise);

                var benefits = await qry.Select(da => new CountryWiseBenefitViewModel { Id = da.Id, IsSalaryWise = da.IsSalaryWise, BenefitTitle = da.BenefitTitle, BenefitDesc = da.BenefitDesc, CountryId = da.CountryId }).ToListAsync();

                respModel.SetResult(benefits);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"CountryId:{CountryId}, isSalaryWise:{isSalaryWise}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<string>> SetCountryWiseBenefitsAsync(CountryWiseBenefitViewModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<string>();
            try
            {
                if (await dbContext.PhCountryWiseBenefits.Where(da => da.Id != model.Id && da.CountryId == model.CountryId && da.BenefitTitle == model.BenefitTitle.Trim() && da.IsSalaryWise == model.IsSalaryWise).CountAsync() > 0)
                {
                    respModel.SetError(ApiResponseErrorCodes.InvalidBodyContent, $"'{model.BenefitTitle.Trim()}' already exist for this country", true);
                }
                else
                {
                    var dbModel_benefit = await dbContext.PhCountryWiseBenefits.FirstOrDefaultAsync(da => da.Id == model.Id);
                    if (dbModel_benefit == null)
                    {
                        dbModel_benefit = new PhCountryWiseBenefit
                        {
                            CountryId = model.CountryId,
                            IsSalaryWise = model.IsSalaryWise,
                            BenefitTitle = model.BenefitTitle.Trim(),
                            BenefitDesc = model.BenefitDesc.Trim(),
                            CreatedDate = CurrentTime,
                            CreatedBy = Usr.Id,
                            Status = (byte)RecordStatus.Active
                        };
                        dbContext.PhCountryWiseBenefits.Add(dbModel_benefit);
                    }
                    else
                    {
                        dbModel_benefit.IsSalaryWise = model.IsSalaryWise;
                        dbModel_benefit.BenefitTitle = model.BenefitTitle.Trim();
                        dbModel_benefit.BenefitDesc = model.BenefitDesc.Trim();
                        dbModel_benefit.UpdatedBy = Usr.Id;
                        dbModel_benefit.UpdatedDate = CurrentTime;
                    }
                    await dbContext.SaveChangesAsync();

                    respModel.SetResult(string.Empty);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"model:{JsonConvert.SerializeObject(model)}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<List<CountryWiseAllowanceDetailViewModel>>> GetCountryWiseAllowancesAsync(int CountryId, bool? IsCitizenWise)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CountryWiseAllowanceDetailViewModel>>();
            try
            {
                var qry = dbContext.PhCountryWiseAllowances.Where(da => da.Status == (byte)RecordStatus.Active && da.CountryId == CountryId);
                if (IsCitizenWise.HasValue)
                    qry = qry.Where(da => da.IsCitizenWise == IsCitizenWise);
                var benefits = await qry.Select(da => new CountryWiseAllowanceDetailViewModel { AllowanceCode = da.AllowanceCode, AllowanceTitle = da.AllowanceTitle, AllowancePrice = da.AllowancePrice, AllowancePercentage = da.AllowancePercentage }).ToListAsync();

                respModel.SetResult(benefits);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"CountryId:{CountryId}, IsCitizenWise:{IsCitizenWise}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<List<CountryWiseAllowanceViewModel>>> GetCountryWiseAllowanceListAsync(int CountryId, bool? IsCitizenWise)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CountryWiseAllowanceViewModel>>();
            try
            {
                var qry = dbContext.PhCountryWiseAllowances.Where(da => da.Status == (byte)RecordStatus.Active && da.CountryId == CountryId);
                if (IsCitizenWise.HasValue)
                    qry = qry.Where(da => da.IsCitizenWise == IsCitizenWise);

                var benefits = await qry.Select(da => new CountryWiseAllowanceViewModel { Id = da.Id, IsCitizenWise = da.IsCitizenWise, AllowanceCode = da.AllowanceCode, AllowanceTitle = da.AllowanceTitle, AllowanceDesc = da.AllowanceDesc, AllowancePrice = da.AllowancePrice, AllowancePercentage = da.AllowancePercentage, CountryId = da.CountryId }).ToListAsync();

                respModel.SetResult(benefits);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"CountryId:{CountryId}, IsCitizenWise:{IsCitizenWise}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<string>> SetCountryWiseAllowancesAsync(CountryWiseAllowanceViewModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<string>();
            try
            {
                if (await dbContext.PhCountryWiseAllowances.Where(da => da.Id != model.Id && da.CountryId == model.CountryId && (da.AllowanceTitle == model.AllowanceTitle.Trim() || da.AllowanceCode == model.AllowanceCode.Trim()) && da.IsCitizenWise == model.IsCitizenWise).CountAsync() > 0)
                {
                    respModel.SetError(ApiResponseErrorCodes.InvalidBodyContent, $"'{model.AllowanceTitle.Trim()}/{model.AllowanceCode.Trim()}' already exist for this country", true);
                }
                else
                {
                    var dbModel_Allowance = await dbContext.PhCountryWiseAllowances.FirstOrDefaultAsync(da => da.Id == model.Id);
                    if (dbModel_Allowance == null)
                    {
                        dbModel_Allowance = new PhCountryWiseAllowance
                        {
                            CountryId = model.CountryId,
                            IsCitizenWise = model.IsCitizenWise,
                            AllowanceCode = model.AllowanceCode.Trim(),
                            AllowanceTitle = model.AllowanceTitle.Trim(),
                            AllowanceDesc = model.AllowanceDesc.Trim(),
                            AllowancePrice = model.AllowancePrice,
                            AllowancePercentage = model.AllowancePercentage,
                            CreatedDate = CurrentTime,
                            CreatedBy = Usr.Id,
                            Status = (byte)RecordStatus.Active
                        };
                        dbContext.PhCountryWiseAllowances.Add(dbModel_Allowance);
                    }
                    else
                    {
                        dbModel_Allowance.IsCitizenWise = model.IsCitizenWise;
                        dbModel_Allowance.AllowanceCode = model.AllowanceCode.Trim();
                        dbModel_Allowance.AllowanceTitle = model.AllowanceTitle.Trim();
                        dbModel_Allowance.AllowanceDesc = model.AllowanceDesc.Trim();
                        dbModel_Allowance.AllowancePrice = model.AllowancePrice;
                        dbModel_Allowance.AllowancePercentage = model.AllowancePercentage;
                        dbModel_Allowance.UpdatedBy = Usr.Id;
                        dbModel_Allowance.UpdatedDate = CurrentTime;
                    }
                    await dbContext.SaveChangesAsync();

                    respModel.SetResult(string.Empty);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"model:{JsonConvert.SerializeObject(model)}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<List<ClientViewModel>>> GetClients()
        {
            int? EmpId = Usr.EmpId;
            int Usertype = Usr.UserTypeId;
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<ClientViewModel>>();
            try
            {

                var data = await ListClients(EmpId, Usertype);

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


        public async Task<GetResponseViewModel<List<ClientSpocsModel>>> GetSpocs(int ClientId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<ClientSpocsModel>>();
            try
            {

                var data = new List<ClientSpocsModel>();

                data = await dbContext.GetClientSpocs(ClientId);

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

        public async Task<GetResponseViewModel<List<AssessmentViewModel>>> GetAssessments()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<AssessmentViewModel>>();
            try
            {

                var data = new List<AssessmentViewModel>();

                data = await GetAssessmentList();

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

        public async Task<GetResponseViewModel<List<MediaFilesViewModel>>> GetMediaFiles()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<MediaFilesViewModel>>();
            try
            {


                var mediaFiles = await (from media in dbContext.PhMediaFiles
                                        where media.Status != (byte)RecordStatus.Delete
                                        select new MediaFilesViewModel
                                        {
                                            FileName = media.FileName,
                                            FileType = media.FileType,
                                            Id = media.Id,
                                            FilePath = string.Empty,
                                            CreatedDate = media.CreatedDate
                                        }).ToListAsync();
                foreach (var item in mediaFiles)
                {
                    item.FilePath = appSettings.AppSettingsProperties.HireApiUrl + "/TemplateGallery/" + item.FileName;
                }

                respModel.SetResult(mediaFiles);
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


        public async Task<CreateResponseViewModel<string>> UploadMediaFile(CreateMediaViewModel createMediaViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string message = "File Not Uploaded";
            try
            {
                

                string webRootPath = _environment.ContentRootPath + "\\TemplateGallery";

                // Checking for folder is available or not 
                if (!Directory.Exists(webRootPath))
                {
                    Directory.CreateDirectory(webRootPath);
                }
                if (createMediaViewModel.File != null)
                {
                    if (createMediaViewModel.File.Length > 0)
                    {
                        var fileName = Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + createMediaViewModel.File.FileName);
                        fileName = fileName.Replace(" ", "_");
                        if (fileName.Length > 200)
                        {
                            fileName = fileName.Substring(0, 199);
                        }
                        var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await createMediaViewModel.File.CopyToAsync(fileStream);
                        }
                        var phMediaFile = new PhMediaFile
                        {
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            FileGroup = 1,
                            FileName = fileName,
                            Status = (byte)RecordStatus.Active,
                            FileType = createMediaViewModel.File.ContentType
                        };

                        dbContext.PhMediaFiles.Add(phMediaFile);
                        await dbContext.SaveChangesAsync();

                        message = "Uploaded Successfully";

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = " Uploaded Media document successfully",
                            ActivityDesc = " has uploaded the Media document successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = phMediaFile.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);
                    }
                }

                respModel.SetResult(message);
                respModel.Status = true;

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        #region SMTP

        public async Task<GetResponseViewModel<ConfiguredSmptViewModel>> GetConfiguredUserMailDetails(int UserId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<ConfiguredSmptViewModel>();
            var configuredSmptViewModel = new ConfiguredSmptViewModel();
            try
            {


                var simpleEncrypt = new SimpleEncrypt();

                configuredSmptViewModel = await (from smtp in dbContext.PhUsersConfigs
                                                 where smtp.UserId == UserId
                                                 select new ConfiguredSmptViewModel
                                                 {
                                                     Id = smtp.Id,
                                                     UserName = smtp.UserName,
                                                     PasswordHash = smtp.PasswordHash,
                                                     VerifyFlag = smtp.VerifyFlag
                                                 }).FirstOrDefaultAsync();
                //if (configuredSmptViewModel != null)
                //{
                //    configuredSmptViewModel.PasswordHash = simpleEncrypt.passwordEncrypt(configuredSmptViewModel.PasswordHash);
                //}


                respModel.SetResult(configuredSmptViewModel);
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

        public async Task<CreateResponseViewModel<string>> ConfigureUserMail(ConfigureSmptMailViewModel configureSmptMailViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string message = "Saved Successfully";
            try
            {
                
                var simpleEncrypt = new SimpleEncrypt();
                var UserDtls = dbContext.PiHireUsers.Where(x => x.Id == configureSmptMailViewModel.UserId).FirstOrDefault();
                if (UserDtls != null)
                {
                    var configuredSmpt = await dbContext.PhUsersConfigs.Where(x => x.UserId == configureSmptMailViewModel.UserId).FirstOrDefaultAsync();
                    var Pswd = simpleEncrypt.passwordEncrypt(configureSmptMailViewModel.PasswordHash);
                    bool VerifyFlag = ((configuredSmpt?.VerifyFlag ?? false) && (configureSmptMailViewModel.UserName == configuredSmpt?.UserName && Pswd == configuredSmpt?.PasswordHash)) ? true : await isSmtpWorking(configureSmptMailViewModel.UserName, configureSmptMailViewModel.PasswordHash);
                    if (configuredSmpt == null)
                    {
                        configuredSmpt = new PhUsersConfig
                        {
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            PasswordHash = Pswd,
                            Status = (byte)RecordStatus.Active,
                            UserName = configureSmptMailViewModel.UserName,
                            VerifyFlag = VerifyFlag,//no more verification is required per new scenario
                            UserId = configureSmptMailViewModel.UserId
                        };
                        dbContext.PhUsersConfigs.Add(configuredSmpt);
                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        configuredSmpt.PasswordHash = Pswd;
                        configuredSmpt.Status = (byte)RecordStatus.Active;
                        configuredSmpt.VerifyFlag = VerifyFlag;//no more verification is required per new scenario
                        configuredSmpt.UserName = configureSmptMailViewModel.UserName;
                        configuredSmpt.UpdatedBy = UserId;
                        configuredSmpt.UpdatedDate = CurrentTime;

                        dbContext.PhUsersConfigs.Update(configuredSmpt);
                        await dbContext.SaveChangesAsync();
                    }

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Configuring User Mail",
                        ActivityDesc = " Configuring Mail to " + UserDtls?.FirstName + "",
                        ActivityType = (byte)AuditActivityType.Other,
                        TaskID = UserDtls?.UserId,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = "User Email is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }
        async Task<bool> isSmtpWorking(string SmtpLoginName, string SmtpLoginPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(SmtpLoginName) == false && string.IsNullOrEmpty(SmtpLoginPassword) == false)
                {
                    var smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        SmtpLoginName, SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl, SmtpLoginName, appSettings.smtpEmailConfig.SmtpFromName);

                    var mailBody = "piHire smtp verification.";
                    await smtp.SendMail(SmtpLoginName, "piHire smtp", mailBody, string.Empty);
                    return true;
                }
            }
            catch (Exception e)
            {

            }
            return false;
        }

        //public async Task<CreateResponseViewModel<string>> UserMailConfigureSuccess(UserMailConfigureSuccessViewModel UserMailConfigureSuccessViewModel)
        //{
        //    logger.SetMethodName(MethodBase.GetCurrentMethod());
        //    var respModel = new CreateResponseViewModel<string>();
        //    string message = "Successfully Updated";
        //    try
        //    {
        //        

        //        var configuredSmpt = await dbContext.PhUsersConfigs.Where(x => x.Id == UserMailConfigureSuccessViewModel.Id && x.VerifyToken == UserMailConfigureSuccessViewModel.Token).FirstOrDefaultAsync();
        //        if (configuredSmpt != null)
        //        {
        //            if (configuredSmpt.VerifyFlag)
        //            {
        //                message = "Link is Expired";
        //            }
        //            else
        //            {
        //                configuredSmpt.VerifyFlag = true;
        //                configuredSmpt.PasswordHash = UserMailConfigureSuccessViewModel.Token;
        //                configuredSmpt.UpdatedDate = CurrentTime;

        //                dbContext.PhUsersConfigs.Update(configuredSmpt);
        //                await dbContext.SaveChangesAsync();
        //            }
        //        }
        //        else
        //        {
        //            message = "Incorrect URL";
        //        }

        //        respModel.SetResult(message);
        //        respModel.Status = true;

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

        //        respModel.Status = false;
        //        respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
        //        respModel.Result = null;
        //    }
        //    return respModel;
        //}

        //public async Task<GetResponseViewModel<string>> GetOutlookTokenAsync()
        //{
        //    logger.SetMethodName(MethodBase.GetCurrentMethod());
        //    var respModel = new GetResponseViewModel<string>();
        //    try
        //    {
        //        
        //        int UserId = Usr.Id;

        //        var cred = await dbContext.PhUsersConfigs.Where(x => x.UserId == UserId && x.VerifyFlag == true)
        //                .Select(da => new { da.UserName, da.PasswordHash }).FirstOrDefaultAsync();

        //        if (cred != null && string.IsNullOrEmpty(cred.UserName) == false && string.IsNullOrEmpty(cred.PasswordHash) == false)
        //        {
        //            SimpleEncrypt simpleEncrypt = new SimpleEncrypt();
        //            var pass = simpleEncrypt.passwordDecrypt(cred.PasswordHash);
        //            Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(cred.UserName, pass);
        //            var tkn = await outlook.getAccessTokenAsync(logger);
        //            if (!string.IsNullOrEmpty(tkn.token))
        //            {
        //                respModel.SetResult(tkn.token);
        //                respModel.Status = true;
        //                return respModel;
        //            }
        //            else
        //            {
        //                respModel.Status = false;
        //                respModel.Meta.SetError(ApiResponseErrorCodes.InvalidBodyContent, "User smtp credentials are not valid", true);
        //                respModel.Result = null;
        //                return respModel;
        //            }
        //        }
        //        else
        //        {
        //            respModel.Status = false;
        //            respModel.Meta.SetError(ApiResponseErrorCodes.InvalidBodyContent, "User smtp credentials are not available", true);
        //            respModel.Result = null;
        //            return respModel;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

        //        respModel.Status = false;
        //        respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
        //        respModel.Result = null;
        //        return respModel;
        //    }
        //}

        #endregion


        #region 
        public GetResponseViewModel<List<SearchKeyModel>> GetSearchListModel()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<SearchKeyModel>>();
            try
            {

                var Technologies = new List<SearchKeyModel>();

                Technologies = (from stus in dbContext.PhTechnologysSes
                                select new SearchKeyModel
                                {
                                    Value = stus.Id,
                                    Category = "TE",
                                    Display = stus.Title
                                }).ToList();

                respModel.SetResult(Technologies);
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

        public GetResponseViewModel<SearchKeyModel> GetSkillDtls(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<SearchKeyModel>();
            try
            {

                var Technologies = new SearchKeyModel();

                Technologies = (from stus in dbContext.PhTechnologysSes
                                where stus.Id == Id
                                select new SearchKeyModel
                                {
                                    Value = stus.Id,
                                    Category = "TE",
                                    Display = stus.Title
                                }).FirstOrDefault();

                respModel.SetResult(Technologies);
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
        public async Task<GetResponseViewModel<AuditModel>> GetAuditList(AuditListSearchViewModel audit)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            int UserTypeId = Usr.UserTypeId;
            var respModel = new GetResponseViewModel<AuditModel>();
            try
            {


                if (audit.FromDate == null)
                {
                    var dt = calcDate(dashboardDateFilter.ThisMonth);
                    audit.FromDate = dt.fmDt;
                    audit.ToDate = dt.toDt;
                }
                else
                {
                    if (audit.ToDate == null)
                    {
                        audit.ToDate = CurrentTime;
                    }
                }
                audit.FromDate = audit.FromDate.Value;
                audit.ToDate = audit.ToDate.Value;


                if (audit.CurrentPage <= 0)
                {
                    audit.CurrentPage = 1;
                }
                audit.CurrentPage = (audit.CurrentPage - 1) * audit.PerPage;
                var dtls = await dbContext.GetAuditList(audit.PerPage, audit.CurrentPage, UserTypeId, UserId, audit.AuditType, audit.SUserId, audit.FromDate, audit.ToDate);

                respModel.SetResult(dtls);
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

        public async Task<GetResponseViewModel<ActivitesModel>> GetActivitiesList(ActivityListSearchViewModel activity)// SUserId - Searched User
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<ActivitesModel>();
            byte UserType = Usr.UserTypeId;
            int UserId = Usr.Id;
            try
            {



                if (activity.FromDate == null)
                {
                    var dt = calcDate(dashboardDateFilter.ThisMonth);
                    activity.FromDate = dt.fmDt;
                    activity.ToDate = dt.toDt;
                }
                else
                {
                    if (activity.ToDate == null)
                    {
                        activity.ToDate = CurrentTime;
                    }
                }
                activity.FromDate = activity.FromDate.Value;
                activity.ToDate = activity.ToDate.Value;


                if (activity.CurrentPage <= 0)
                {
                    activity.CurrentPage = 1;
                }
                activity.CurrentPage = (activity.CurrentPage - 1) * activity.PerPage;
                var dtls = await dbContext.GetActivitiesList(UserType, activity.PerPage, activity.CurrentPage, UserId, activity.ActivityType, activity.SUserId, activity.FromDate, activity.ToDate);

                respModel.SetResult(dtls);
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


        #region Integrations 
        public async Task<GetResponseViewModel<List<IntegrationsViewModel>>> GetIntegrationList(byte Category)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<IntegrationsViewModel>>();
            try
            {

                var integrationsViewModels = new List<IntegrationsViewModel>();

                integrationsViewModels = await dbContext.PhIntegrationsSes.Select(x => new IntegrationsViewModel
                {
                    Account = x.Account,
                    Category = x.Category,
                    SubCategory = x.SubCategory,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate,
                    Id = x.Id,
                    InteDesc = x.InteDesc,
                    Logo = x.Logo,
                    Price = x.Price,
                    QtyOrPeriodFlag = x.QtyOrPeriodFlag,
                    Quantity = x.Quantity,
                    Status = x.Status,
                    Title = x.Title,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedDate = x.UpdatedDate,
                    ValiPeriod = x.ValiPeriod
                }).ToListAsync();

                if (Category != 0)
                {
                    integrationsViewModels = integrationsViewModels.Where(x => x.Category == Category).ToList();
                }

                respModel.SetResult(integrationsViewModels);
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
        public async Task<UpdateResponseViewModel<IntegrationStatusRespViewModel>> UpdateIntegrationStatus(int[] Id, RecordStatus Status)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<IntegrationStatusRespViewModel>();
            IntegrationStatusRespViewModel messageObj = new IntegrationStatusRespViewModel();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var phIntegrationsS = await dbContext.PhIntegrationsSes.Where(x => Id.Contains(x.Id) && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (phIntegrationsS != null)
                {
                    bool isUpdating = true;
                    if (RecordStatus.Active == Status)
                        switch ((IntegrationCategory)phIntegrationsS.Category)
                        {
                            case IntegrationCategory.JobBoards:
                                break;
                            case IntegrationCategory.Communications:
                                break;
                            case IntegrationCategory.Marketing:
                                break;
                            case IntegrationCategory.Calenders:
                                switch ((IntegrationCalendersCategory)phIntegrationsS.SubCategory)
                                {
                                    case IntegrationCalendersCategory.GoogleMeet:
                                        {
                                            isUpdating = false;
                                            messageObj = new IntegrationStatusRespViewModel
                                            {
                                                IsAuth = true,
                                                AuthUrl = GoogleMeetReq(),
                                                message = ""
                                            };
                                        }
                                        break;
                                    case IntegrationCalendersCategory.Office365:
                                        {
                                            isUpdating = false;
                                            messageObj = new IntegrationStatusRespViewModel
                                            {
                                                IsAuth = true,
                                                AuthUrl = TeamCalendarAuthReq(),
                                                message = ""
                                            };
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    if (isUpdating)
                    {
                        phIntegrationsS.UpdatedBy = UserId;
                        phIntegrationsS.UpdatedDate = CurrentTime;
                        phIntegrationsS.Status = (byte)Status;
                        dbContext.PhIntegrationsSes.Update(phIntegrationsS);

                        await dbContext.SaveChangesAsync();


                        respModel.Status = true;
                        messageObj = new IntegrationStatusRespViewModel { message = "Updated Successfully" };
                    }
                }
                else
                {
                    respModel.Status = false;
                    messageObj = new IntegrationStatusRespViewModel { message = "Selected integration not found" };
                }
                respModel.SetResult(messageObj);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }
        public async Task<UpdateResponseViewModel<IntegrationReqStatusViewModel>> GetUpdateIntegrationStatus(IntegrationCategory category, int subCategory)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<IntegrationReqStatusViewModel>();
            IntegrationReqStatusViewModel messageObj = null;
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.GetItem, "Start of method:", respModel.Meta.RequestID + ", category:" + category + ", SubCategory:" + subCategory);
                switch (category)
                {
                    case IntegrationCategory.JobBoards:
                        break;
                    case IntegrationCategory.Communications:
                        break;
                    case IntegrationCategory.Marketing:
                        break;
                    case IntegrationCategory.Calenders:
                        switch ((IntegrationCalendersCategory)subCategory)
                        {
                            case IntegrationCalendersCategory.GoogleMeet:
                                {
                                    var obj = GoogleMeetReqData.FirstOrDefault(da => da.UserId == Usr.Id);
                                    if (obj != null)
                                    {
                                        if (obj.IsCompleted)
                                        {
                                            GoogleMeetReqData.Remove(obj);
                                        }
                                        messageObj = new IntegrationReqStatusViewModel
                                        {
                                            IsCompleted = obj.IsCompleted,
                                            IsSuccess = obj.IsSuccess
                                        };
                                    }
                                }
                                break;
                            case IntegrationCalendersCategory.Office365:
                                {
                                    var obj = TeamCalendarAuthReqData.FirstOrDefault(da => da.UserId == Usr.Id);
                                    if (obj != null)
                                    {
                                        if (obj.IsCompleted)
                                        {
                                            TeamCalendarAuthReqData.Remove(obj);
                                        }
                                        messageObj = new IntegrationReqStatusViewModel
                                        {
                                            IsCompleted = obj.IsCompleted,
                                            IsSuccess = obj.IsSuccess
                                        };
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }

                if (messageObj == null)
                {
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Request already processed/bad request", true);
                }
                else
                    respModel.SetResult(messageObj);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        #region Google meet
        //string GoogleCalendarRedirectUrl
        //{
        //    get
        //    {
        //        return SiteUrl + "/api/v1/Common/GoogleMeetResp";
        //    }
        //}
        class GoogleMeetReqDtls
        {
            public int UserId { get; set; }
            public DateTime RerDt { get; set; }
            public string code { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsSuccess { get; set; }
        }
        static List<GoogleMeetReqDtls> GoogleMeetReqData = new List<GoogleMeetReqDtls>();
        string GoogleMeetReq()
        {
            var obj = GoogleMeetReqData.FirstOrDefault(da => da.IsCompleted == false && da.UserId == Usr.Id);
            if (obj == null)
            {
                string cd;
                do
                {
                    cd = (Guid.NewGuid() + "").Split('-')[0];
                } while (GoogleMeetReqData.FirstOrDefault(da => da.code == cd) != null);
                obj = new GoogleMeetReqDtls { UserId = Usr.Id, code = cd, RerDt = DateTime.UtcNow };
                GoogleMeetReqData.Add(obj);
            }
            return Common.Meeting.GoogleMeet.getAuthorizeUrl(obj.code);

        }
        public async Task<string> SetGoogleMeetToken(string access_token, string code, string state)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var obj = GoogleMeetReqData.FirstOrDefault(da => da.code == state);
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method: access_token" + access_token + ", code:" + code + ", state:" + state);
                var clsObj = Common.Meeting.GoogleMeet.GetSettings();//accesstoken_prfx
                string token = string.Empty;
                if (!string.IsNullOrEmpty(access_token))
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Access token exist");
                    token = Common.Meeting.GoogleMeet.accesstoken_prfx + access_token;
                }
                else
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Access code exist");
                    token = Common.Meeting.GoogleMeet.ExchangeAuthorizationCode(code, clsObj.web.redirect_uris[0], logger);
                }
                if (obj != null)
                {
                    obj.IsSuccess = string.IsNullOrEmpty(token) == false;
                    if (obj.IsSuccess)
                    {
                        var intObj = await dbContext.PhIntegrationsSes.FirstOrDefaultAsync(da => da.Status != (byte)RecordStatus.Delete && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.GoogleMeet);
                        Common.Meeting.GoogleMeet tm = new Common.Meeting.GoogleMeet(token, logger);
                        Google.Apis.Oauth2.v2.Data.Userinfoplus prfl = null;
                        try
                        {
                            prfl = await tm.getProfile();
                        }
                        catch (Exception) { }
                        if (intObj == null)
                        {
                            intObj = new PhIntegrationsS
                            {
                                Status = (byte)RecordStatus.Active,
                                Category = (byte)IntegrationCategory.Calenders,
                                SubCategory = (byte)IntegrationCalendersCategory.GoogleMeet,
                                Title = "Google calender",
                                CreatedBy = obj.UserId,
                                RefreshToken = token,
                                Account = prfl?.Email ?? ""
                            };
                            dbContext.PhIntegrationsSes.Add(intObj);
                        }
                        else
                        {
                            intObj.RefreshToken = token;
                            intObj.Status = (byte)RecordStatus.Active;
                            intObj.UpdatedBy = obj.UserId;
                            intObj.UpdatedDate = CurrentTime;
                            intObj.Account = prfl?.Email ?? "";
                        }
                        await dbContext.SaveChangesAsync();

                        // Audit 
                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Update on Integration",
                            ActivityDesc = " has integrated Google meet Calendar integration",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = intObj.Id,
                            UserId = obj.UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);
                    }
                    obj.IsCompleted = true;
                }
                else logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "State code not exist");
                return "";
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, "access_token" + access_token + ", code:" + code + ", state:" + state, ex);
                obj.IsCompleted = true;
                obj.IsSuccess = false;
                return "Something went wrong";
            }
        }
        public async Task<CreateResponseViewModel<string>> CheckGoogleMeetCalendarTokenExist()
        {
            var resp = new CreateResponseViewModel<string>();
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.GetItem, "Start of method: ");
                var obj = await dbContext.PhIntegrationsSes.FirstOrDefaultAsync(da => da.Status != (byte)RecordStatus.Delete && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.GoogleMeet);
                if (obj != null)
                {
                    if (obj.Status == (byte)RecordStatus.Active)
                    {
                        resp.Status = true;
                        resp.SetResult(obj.Account);
                    }
                    else
                    {
                        resp.Result = "";
                        resp.Status = false;
                        resp.Meta.SetError(ApiResponseErrorCodes.InvalidBodyContent, "Token not valid.", true);
                    }
                }
                else
                {
                    resp.Result = "";
                    resp.Status = false;
                    resp.Meta.SetError(ApiResponseErrorCodes.InvalidBodyContent, "Token not exist.", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, "", ex);
                resp.Result = "";
                resp.Status = false;
                resp.Meta.SetError(ApiResponseErrorCodes.Exception, "Something went wrong");
            }
            return resp;
        }
        #endregion
        #region Microsoft Team
        string TeamCalendarRedirectUrl
        {
            get
            {
                return appSettings.AppSettingsProperties.HireApiUrl +
                    (appSettings.AppSettingsProperties.HireApiUrl.EndsWith("?") ? "" : "/") + "api/v1/Common/TeamCalendarAuthResp";
            }
        }
        class TeamCalendarAuthReqDtls
        {
            public int UserId { get; set; }
            public DateTime ReqDt { get; set; }
            public string code { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsSuccess { get; set; }
        }
        static List<TeamCalendarAuthReqDtls> TeamCalendarAuthReqData = new List<TeamCalendarAuthReqDtls>();
        string TeamCalendarAuthReq()
        {
            var obj = TeamCalendarAuthReqData.FirstOrDefault(da => da.IsCompleted == false && da.UserId == Usr.Id);
            if (obj == null)
            {
                string cd;
                do
                {
                    cd = (Guid.NewGuid() + "").Split('-')[0];
                } while (TeamCalendarAuthReqData.FirstOrDefault(da => da.code == cd) != null);
                obj = new TeamCalendarAuthReqDtls { UserId = Usr.Id, code = cd, ReqDt = DateTime.UtcNow };
                TeamCalendarAuthReqData.Add(obj);
            }
            return Common.Meeting.Teams.getAuthorizeUrl(TeamCalendarRedirectUrl, obj.code);
        }
        public async Task<string> SetTeamCalendarToken(string code, string state, string session_state, string error)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var obj = TeamCalendarAuthReqData.FirstOrDefault(da => da.code == state);
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method: code:" + code + ",state:" + state + ",session_state:" + session_state + ",error:" + error);
                var tokenObj = BAL.Common.Meeting.Teams.getRefreshToken(code, TeamCalendarRedirectUrl, logger);
                if (obj != null)
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "tokenObj:" + Newtonsoft.Json.JsonConvert.SerializeObject(tokenObj) + " of " + Newtonsoft.Json.JsonConvert.SerializeObject(obj));
                    obj.IsSuccess = string.IsNullOrEmpty(tokenObj.token) == false;
                    if (obj.IsSuccess)
                    {
                        var intObj = await dbContext.PhIntegrationsSes.FirstOrDefaultAsync(da => da.Status != (byte)RecordStatus.Delete && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.Office365);
                        Common.Meeting.Teams tm = new Common.Meeting.Teams(tokenObj.token, TeamCalendarRedirectUrl, logger);
                        Common.Meeting.TeamCalendarProfileViewModel prfl = null;
                        try
                        {
                            prfl = await tm.getProfile();
                        }
                        catch (Exception)
                        { }

                        if (intObj == null)
                        {
                            intObj = new PhIntegrationsS
                            {
                                Status = (byte)(prfl == null ? RecordStatus.Inactive : RecordStatus.Active),
                                Category = (byte)IntegrationCategory.Calenders,
                                SubCategory = (byte)IntegrationCalendersCategory.Office365,
                                Title = "Teams calender",
                                CreatedBy = obj.UserId,
                                RefreshToken = tokenObj.token,
                                ReDirectUrl = TeamCalendarRedirectUrl,
                                Account = prfl?.mail ?? ""
                            };
                            dbContext.PhIntegrationsSes.Add(intObj);
                        }
                        else
                        {
                            intObj.RefreshToken = tokenObj.token;
                            intObj.ReDirectUrl = TeamCalendarRedirectUrl;
                            intObj.Status = (byte)(prfl == null ? RecordStatus.Inactive : RecordStatus.Active);
                            intObj.UpdatedBy = obj.UserId;
                            intObj.UpdatedDate = CurrentTime;
                            intObj.Account = prfl?.mail ?? "";
                        }
                        await dbContext.SaveChangesAsync();
                        // Audit 
                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Update on Integration",
                            ActivityDesc = " has integrated Microsoft Teams Calendar integration",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = intObj.Id,
                            UserId = obj.UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);
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
        public async Task<CreateResponseViewModel<string>> CheckTeamCalendarTokenExist()
        {
            var resp = new CreateResponseViewModel<string>();
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.GetItem, "Start of method: ");
                var obj = await dbContext.PhIntegrationsSes.FirstOrDefaultAsync(da => da.Status != (byte)RecordStatus.Delete && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.Office365);
                if (obj != null)
                {
                    if (obj.Status == (byte)RecordStatus.Active)
                    {
                        resp.Status = true;
                        resp.SetResult(obj.Account);
                    }
                    else
                    {
                        resp.Result = string.Empty;
                        resp.Status = false;
                        resp.Meta.SetError(ApiResponseErrorCodes.InvalidBodyContent, "Token not valid.", true);
                    }
                }
                else
                {
                    resp.Result = string.Empty;
                    resp.Status = false;
                    resp.Meta.SetError(ApiResponseErrorCodes.InvalidBodyContent, "Token not exist.", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, string.Empty, ex);
                resp.Result = string.Empty;
                resp.Status = false;
                resp.Meta.SetError(ApiResponseErrorCodes.Exception, "Something went wrong");
            }
            return resp;
        }
        #endregion
        #endregion



    }
}



