using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.Common.Types;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class CustomSchedulerRepository : BaseRepository, IRepositories.ICustomSchedulerRepository
    {
        readonly Logger logger;
        private readonly IWebHostEnvironment _environment;
        public CustomSchedulerRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<CustomSchedulerRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }
        public async Task<CreateResponseViewModel<List<BgJobSummaryViewModel>>> getSummariesAsync()
        {
            var respModel = new CreateResponseViewModel<List<BgJobSummaryViewModel>>();
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
             //   logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of the method");

                var bgJobs = await this.dbContext.PhBgJobs.AsNoTracking().OrderByDescending(da => da.Id)
                    .Select(da => new { da.Id, da.Status, da.Title, da.JobDesc, da.EventType, da.Frequency, da.ScheduleDate, da.ScheduleTime, da.EmailTemplateId, da.SendTo, da.Pus, da.Bus, da.CandidateStatus, da.Gender, da.CountryIds })
                    .Where(da => da.Status != (byte)RecordStatus.Delete)
                    .OrderByDescending(da => da.Id)
                    .ToListAsync();
                var bgJobIds = bgJobs.Select(da => da.Id).ToArray();
                var bgJobDetails = await this.dbContext.PhBgJobDetails.AsNoTracking()
                    .Where(da => bgJobIds.Contains(da.Bjid))
                    .Select(da => new { da.Bjid, da.ExecutedOn, da.SendCount, da.DeliveredCount, da.ServiceProviderId, da.JobId, da.ExecutionStatus, da.Remarks })
                    .ToListAsync();
                var jobIds = bgJobDetails.Where(da => da.JobId.HasValue).Select(da => da.JobId).ToArray();
                Dictionary<int, string> jobNames = new Dictionary<int, string>();
                if (jobIds.Length > 0)
                {
                    jobNames =
                       await dbContext.PhJobOpenings.AsNoTracking().Where(da => jobIds.Contains(da.Id))
                           .ToDictionaryAsync(daa => daa.Id, da => da.JobTitle);
                }
                var models = new List<BgJobSummaryViewModel>();
                foreach (var bgJob in bgJobs)
                {
                    var obj = new BgJobSummaryViewModel
                    {
                        Id = bgJob.Id,
                        Title = bgJob.Title,
                        Desc = bgJob.JobDesc,
                        EventType = Enum.Parse<CustomSchedulerEventTypes>(bgJob.EventType),
                        EventTypeCode = Enum.Parse<CustomSchedulerEventTypes>(bgJob.EventType) + "",
                        Frequency = (CustomSchedulerFrequency)bgJob.Frequency,
                        ScheduleDate = bgJob.ScheduleDate,
                        ScheduleTime = bgJob.ScheduleTime,
                        EmailTemplateId = bgJob.EmailTemplateId,
                        SendTo = (CustomSchedulerSendTo)bgJob.SendTo,

                        Pus = bgJob.Pus,
                        Bus = bgJob.Bus,
                        CandidateStatus = bgJob.CandidateStatus,
                        Gender = bgJob.Gender,
                        CountryIds = bgJob.CountryIds,

                        Status = (RecordStatus)bgJob.Status,
                        Details = bgJobDetails.Where(da => da.Bjid == bgJob.Id)
                        .Select(da => new BgJobDetailsViewModel
                        {
                            DeliveredCount = da.DeliveredCount,
                            ExecutedOn = da.ExecutedOn,
                            ExecutionStatus = ((CustomSchedulerDtlsExecutionStatus)da.ExecutionStatus) + "",
                            ExecutionStatusId = da.ExecutionStatus,
                            Remarks = da.Remarks,
                            SendCount = da.SendCount,
                            ServiceProvider = ((Utilities.EmailProviders)da.ServiceProviderId) + "",
                            JobName = da.JobId.HasValue && jobNames.ContainsKey(da.JobId.Value) ? jobNames[da.JobId.Value] : ""
                        }).OrderByDescending(da => da.ExecutedOn).ToList()
                    };
                    if (obj.Frequency == CustomSchedulerFrequency.DateAndTime && obj.ScheduleDate.HasValue && (!string.IsNullOrEmpty(obj.ScheduleTime)))
                    {
                        var tm = obj.ScheduleTime.Split(':');
                        obj.ScheduleDate = obj.ScheduleDate.Value.AddHours(Convert.ToInt32(tm[0]))
                            .AddMinutes(Convert.ToInt32(tm[1]));
                    }
                    if (obj.Details.Where(da => da.ExecutionStatusId != (byte)CustomSchedulerDtlsExecutionStatus.NotStarted).Count() > 0)
                    {
                        obj.LastExecutedOn = obj.Details.Where(da => da.ExecutionStatusId != (byte)CustomSchedulerDtlsExecutionStatus.NotStarted).Max(da => da.ExecutedOn);
                        obj.SendCount = obj.Details.Sum(da => da.SendCount);
                        obj.DeliveredCount = obj.Details.Sum(da => da.DeliveredCount);
                    }
                    models.Add(obj);
                }
                respModel.SetResult(models);
              //  logger.Log(LogLevel.Debug, LoggingEvents.Other, "End of the method");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.Other, "", ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }
        public async Task<CreateResponseViewModel<List<BgJobSummaryViewModel>>> getSummariesAsync(int JobId)
        {
            var respModel = new CreateResponseViewModel<List<BgJobSummaryViewModel>>();
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
              //  logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of the method");
                var bgJobDetails = await this.dbContext.PhBgJobDetails.AsNoTracking()
                    .Where(da => da.JobId == JobId)
                    .Select(da => new { da.Bjid, da.ExecutedOn, da.SendCount, da.DeliveredCount, da.ServiceProviderId, da.JobId, da.ExecutionStatus, da.Remarks })
                    .ToListAsync();
                var bgJobIds = bgJobDetails.Select(da => da.Bjid).Distinct().ToArray();

                var bgJobs = await this.dbContext.PhBgJobs.AsNoTracking()
                    .Where(da => bgJobIds.Contains(da.Id))
                    .Select(da => new { da.Id, da.Status, da.Title, da.JobDesc, da.EventType, da.Frequency, da.ScheduleDate, da.ScheduleTime, da.EmailTemplateId, da.SendTo, da.Pus, da.Bus, da.CandidateStatus, da.Gender, da.CountryIds })
                    .Where(da => da.Status != (byte)RecordStatus.Delete)
                    .ToListAsync();
                //var bgJobIds = bgJobs.Select(da => da.Id).ToArray();
                //bgJobDetails = bgJobDetails
                //    .Where(da => bgJobIds.Contains(da.Bjid))
                //    .Select(da => new { da.Bjid, da.ExecutedOn, da.SendCount, da.DeliveredCount, da.ServiceProviderId, da.JobId, da.ExecutionStatus, da.Remarks })
                //    .ToList();
                //var jobIds = bgJobDetails.Where(da => da.JobId.HasValue).Select(da => da.JobId).ToArray();
                Dictionary<int, string> jobNames = new Dictionary<int, string>();
                {
                    jobNames =
                       await dbContext.PhJobOpenings.Where(da => JobId == da.Id)
                           .ToDictionaryAsync(daa => daa.Id, da => da.JobTitle);
                }
                var models = new List<BgJobSummaryViewModel>();
                foreach (var bgJob in bgJobs)
                {
                    var obj = new BgJobSummaryViewModel
                    {
                        Id = bgJob.Id,
                        Title = bgJob.Title,
                        Desc = bgJob.JobDesc,
                        EventType = Enum.Parse<CustomSchedulerEventTypes>(bgJob.EventType),
                        EventTypeCode = Enum.Parse<CustomSchedulerEventTypes>(bgJob.EventType) + "",
                        Frequency = (CustomSchedulerFrequency)bgJob.Frequency,
                        ScheduleDate = bgJob.ScheduleDate,
                        ScheduleTime = bgJob.ScheduleTime,
                        EmailTemplateId = bgJob.EmailTemplateId,
                        SendTo = (CustomSchedulerSendTo)bgJob.SendTo,

                        Pus = bgJob.Pus,
                        Bus = bgJob.Bus,
                        CandidateStatus = bgJob.CandidateStatus,
                        Gender = bgJob.Gender,
                        CountryIds = bgJob.CountryIds,

                        Status = (RecordStatus)bgJob.Status,
                        Details = bgJobDetails.Where(da => da.Bjid == bgJob.Id)
                        .Select(da => new BgJobDetailsViewModel
                        {
                            DeliveredCount = da.DeliveredCount,
                            ExecutedOn = da.ExecutedOn,
                            ExecutionStatus = ((CustomSchedulerDtlsExecutionStatus)da.ExecutionStatus) + "",
                            ExecutionStatusId = da.ExecutionStatus,
                            Remarks = da.Remarks,
                            SendCount = da.SendCount,
                            ServiceProvider = ((Utilities.EmailProviders)da.ServiceProviderId) + "",
                            JobName = da.JobId.HasValue ? jobNames[da.JobId.Value] : ""
                        }).ToList()
                    };
                    if (obj.Frequency == CustomSchedulerFrequency.DateAndTime && obj.ScheduleDate.HasValue && (!string.IsNullOrEmpty(obj.ScheduleTime)))
                    {
                        var tm = obj.ScheduleTime.Split(':');
                        obj.ScheduleDate = obj.ScheduleDate.Value.AddHours(Convert.ToInt32(tm[0]))
                            .AddMinutes(Convert.ToInt32(tm[1]));
                    }
                    if (obj.Details.Where(da => da.ExecutionStatusId != (byte)CustomSchedulerDtlsExecutionStatus.NotStarted).Count() > 0)
                    {
                        obj.LastExecutedOn = obj.Details.Where(da => da.ExecutionStatusId != (byte)CustomSchedulerDtlsExecutionStatus.NotStarted).Max(da => da.ExecutedOn);
                        obj.SendCount = obj.Details.Sum(da => da.SendCount);
                        obj.DeliveredCount = obj.Details.Sum(da => da.DeliveredCount);
                    }
                    models.Add(obj);
                }
                respModel.SetResult(models);
              //  logger.Log(LogLevel.Debug, LoggingEvents.Other, "End of the method");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.Other, "", ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<CreateResponseViewModel<string>> setCustomSchedulerAsync(BgJobSummaryViewModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            DateTime chkScheduleDate = CurrentTime;
            if (model.Frequency == CustomSchedulerFrequency.DateAndTime && model.ScheduleDate.HasValue && (!string.IsNullOrEmpty(model.ScheduleTime)))
            {
                var tm = model.ScheduleTime.Split(':');
                chkScheduleDate = model.ScheduleDate.Value.AddHours(Convert.ToInt32(tm[0]))
                    .AddMinutes(Convert.ToInt32(tm[1]));
            }
            if (model.Frequency == CustomSchedulerFrequency.DateAndTime && CurrentTime > chkScheduleDate)
            {
                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.InvalidBodyContent, "Schedule date/time is past time");
                respModel.Result = "Schedule date/time is past time";
            }
            else
            {
                using (var trans = await dbContext.Database.BeginTransactionAsync())
                    try
                    {
                        string message = string.Empty;
                        //check duplicate
                        var dbModel = await dbContext.PhBgJobs.Where(da => da.Id != model.Id && da.Status != (byte)RecordStatus.Delete &&
                        da.Frequency == (byte)model.Frequency &&
                        ((model.Frequency == CustomSchedulerFrequency.Daily && da.ScheduleTime == model.ScheduleTime) ||
                        (model.Frequency == CustomSchedulerFrequency.DateAndTime && da.ScheduleDate == model.ScheduleDate && da.ScheduleTime == model.ScheduleTime))
                        ).FirstOrDefaultAsync();
                        if (dbModel != null)
                        {
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.InvalidBodyContent, "Schedule date/time already exist");
                            respModel.Result = "Schedule date/time already exist";
                            trans.Rollback();
                        }
                        else
                        {
                            dbModel = await dbContext.PhBgJobs.Where(da => da.Id == model.Id && da.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                            if (dbModel != null)
                            {
                                dbModel.Bus = model.Bus;
                                dbModel.Pus = model.Pus;
                                dbModel.CandidateStatus = model.CandidateStatus;
                                dbModel.CountryIds = model.CountryIds;
                                dbModel.Gender = model.Gender;

                                dbModel.EmailTemplateId = model.EmailTemplateId;
                                dbModel.EventType = model.EventType + "";
                                dbModel.Frequency = (byte)model.Frequency;
                                dbModel.JobDesc = model.Desc;
                                dbModel.Title = model.Title;
                                dbModel.ScheduleDate = model.ScheduleDate;
                                dbModel.ScheduleTime = model.ScheduleTime;
                                dbModel.SendTo = (byte)model.SendTo;
                                dbModel.UpdatedBy = Usr.Id;
                                dbModel.UpdatedDate = CurrentTime;
                                dbContext.PhBgJobs.Update(dbModel);
                                message = "Updated schedule successfully"; //"Updated Successfully";
                            }
                            else
                            {
                                dbModel = new DAL.Entities.PhBgJob
                                {
                                    Bus = model.Bus,
                                    Pus = model.Pus,
                                    CandidateStatus = model.CandidateStatus,
                                    CountryIds = model.CountryIds,
                                    Gender = model.Gender,

                                    EmailTemplateId = model.EmailTemplateId,
                                    EventType = model.EventType + "",
                                    Frequency = (byte)model.Frequency,
                                    JobDesc = model.Desc,
                                    Title = model.Title,
                                    ScheduleDate = model.ScheduleDate,
                                    ScheduleTime = model.ScheduleTime,
                                    SendTo = (byte)model.SendTo,
                                    Status = (byte)RecordStatus.Active,
                                    CreatedBy = Usr.Id
                                    //CreatedDate = CurrentTime
                                };
                                dbContext.PhBgJobs.Add(dbModel);
                                message = "Scheduled successfully";// "Inserted Successfully";
                            }
                            await dbContext.SaveChangesAsync();
                            trans.Commit();
                            respModel.SetResult(message);
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ", model:" + Newtonsoft.Json.JsonConvert.SerializeObject(model), respModel.Meta.RequestID, ex);

                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                        respModel.Result = "Something went wrong try after sometime";
                        trans.Rollback();
                    }
            }
            return respModel;
        }
        public async Task<CreateResponseViewModel<string>> setCustomSchedulerAsync(CreateJobBgViewModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    var job =
                       await dbContext.PhJobOpenings.Where(da => model.JobId == da.Id).Select(daa => new { daa.Id, daa.JobTitle }).FirstOrDefaultAsync();
                    string message = string.Empty;
                    if (job == null)
                    {
                        message = "Job not found";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.InvalidBodyContent, message, true);
                        respModel.Result = message;
                    }
                    else
                    {
                        var dbModel = new DAL.Entities.PhBgJob
                        {
                            //Bus = model.Bus,
                            //Pus = model.Pus,
                            //CandidateStatus = model.CandidateStatus,
                            //CountryIds = model.CountryIds,
                            //Gender = model.Gender,

                            EmailTemplateId = model.EmailTemplateId,
                            EventType = CustomSchedulerEventTypes.NJ /*model.EventType*/ + "",
                            Frequency = (byte)CustomSchedulerFrequency.DateAndTime,//model.Frequency,
                                                                                   //JobDesc = model.Desc,
                            Title = job?.JobTitle + " - " + job?.Id,// model.Title,
                            ScheduleDate = CurrentTime.Date.AddMinutes(5),// model.ScheduleDate,
                            ScheduleTime = CurrentTime.ToString("HH:mm"),// model.ScheduleTime,
                            SendTo = (byte)CustomSchedulerSendTo.piHireCandidates,// model.SendTo,
                            Status = (byte)RecordStatus.Active,
                            CreatedBy = Usr.Id
                            //CreatedDate = CurrentTime
                        };
                        dbContext.PhBgJobs.Add(dbModel);
                        message = "Inserted Successfully";

                        await dbContext.SaveChangesAsync();
                        dbContext.PhBgJobDetails.Add(new DAL.Entities.PhBgJobDetail
                        {
                            Bjid = dbModel.Id,
                            ExecutedOn = CurrentTime,
                            ServiceProviderId = 0,
                            SendCount = 0,
                            DeliveredCount = 0,
                            BulkReferenceId = "",
                            Remarks = "Not started",
                            ExecutionStatus = (byte)CustomSchedulerDtlsExecutionStatus.NotStarted,
                            Status = (byte)RecordStatus.Active,
                            //CreatedDate = CurrentTime,
                            JobId = model.JobId
                        });
                        await dbContext.SaveChangesAsync();
                        trans.Commit();
                        respModel.SetResult(message);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ", model:" + Newtonsoft.Json.JsonConvert.SerializeObject(model), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = "Something went wrong try after sometime";
                    trans.Rollback();
                }
            return respModel;
        }
        public async Task<CreateResponseViewModel<string>> deactiveCustomSchedulerAsync(int bgJobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    string message = string.Empty;
                    var dbModel = await dbContext.PhBgJobs.Where(da => da.Id == bgJobId && da.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                    if (dbModel != null)
                    {
                        dbModel.Status = (byte)RecordStatus.Inactive;
                        dbContext.PhBgJobs.Update(dbModel);

                        await dbContext.SaveChangesAsync();
                        trans.Commit();
                        respModel.SetResult("Deactivated Successfully");
                    }
                    else
                    {
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Custom scheduler not found");
                        respModel.Result = "Custom scheduler not found";
                        trans.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ", bgJobId:" + bgJobId, respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = "Something went wrong try after sometime";
                    trans.Rollback();
                }
            return respModel;
        }
        public async Task<CreateResponseViewModel<string>> activeCustomSchedulerAsync(int bgJobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    string message = string.Empty;
                    var dbModel = await dbContext.PhBgJobs.Where(da => da.Id == bgJobId && da.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                    if (dbModel != null)
                    {
                        dbModel.Status = (byte)RecordStatus.Active;
                        dbContext.PhBgJobs.Update(dbModel);

                        await dbContext.SaveChangesAsync();
                        trans.Commit();
                        respModel.SetResult("Activated Successfully");
                    }
                    else
                    {
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Custom scheduler not found");
                        respModel.Result = "Custom scheduler not found";
                        trans.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ", bgJobId:" + bgJobId, respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = "Something went wrong try after sometime";
                    trans.Rollback();
                }
            return respModel;
        }
        public async Task<CreateResponseViewModel<string>> deleteCustomSchedulerAsync(int bgJobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    string message = string.Empty;
                    var dbModel = await dbContext.PhBgJobs.Where(da => da.Id == bgJobId && da.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                    if (dbModel != null)
                    {
                        dbModel.Status = (byte)RecordStatus.Delete;
                        dbContext.PhBgJobs.Update(dbModel);

                        await dbContext.SaveChangesAsync();
                        trans.Commit();
                        respModel.SetResult("Deleted Successfully");
                    }
                    else
                    {
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Custom scheduler not found");
                        respModel.Result = "Custom scheduler not found";
                        trans.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ", bgJobId:" + bgJobId, respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = "Something went wrong try after sometime";
                    trans.Rollback();
                }
            return respModel;
        }
    }
}
