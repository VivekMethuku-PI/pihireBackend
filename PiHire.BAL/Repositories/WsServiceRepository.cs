using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PiHire.BAL.Common;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.Common.Types;
using PiHire.BAL.ViewModels;
using PiHire.DAL.Entities;
using PiHire.Utilities;
using PiHire.Utilities.Communications.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class WsServiceRepository : BaseRepository, IRepositories.IWsServiceRepository, IDisposable
    {
        int entityPgCnt = 100;
        const string prefix_messageIds = "messageIds:";
        const string PI_SIGNATURE_DIV = "Thanks & Regards <br > Paraminfo";
        const string DefaultUnsubscribeTmplt =
                                     "<table border='0' cellspacing='0' cellpadding='0' width='100%'><tr><td style='padding-top:10px;padding-bottom:10px; font-family: Arial, Helvetica, sans-serif;font-size:13px;'>" +
                                             "If you wish to unsubscribe, <a href='[UnsubscribeLink]'>click here</a>." +
                                     "</td></tr></table>";
        const string COMPANY_NAME = "";
        const int emailSetCount = 1000;

        readonly Logger logger;
        public static IRepositories.IWsServiceRepository Instance(DAL.PiHIRE2Context dbContext, ILoggerProvider logFile, Common.Extensions.AppSettings appSettings)
        {
            var factory = new LoggerFactory(new List<ILoggerProvider> { logFile });
            var lgr = new Logger<WsServiceRepository>(factory);
            return new WsServiceRepository(dbContext, appSettings, lgr);
            //return new WsServiceRepository(dbContext, new Common.Extensions.AppSettings(), null);
        }
        public WsServiceRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<WsServiceRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }

        #region destroyer
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                dbContext.Dispose();
            }

            // Free any unmanaged objects here.
            disposed = true;
        }

        ~WsServiceRepository()
        {
            Dispose(false);
        }
        #endregion


        public async Task<List<ScheduleServiceViewModel>> GetScheduleListSync()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var models = new List<ScheduleServiceViewModel>();
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method");
                    {
                        var schedulers = await dbContext.PhBgJobs.Where(da => da.Status == (byte)RecordStatus.Active)
                            .Select(da => new { da.Id, da.EventType, da.ScheduleDate, da.ScheduleTime, da.Frequency })
                            .ToListAsync();
                        var scIds = schedulers.Select(da => da.Id).ToArray();
                        var subs = await dbContext.PhBgJobDetails.Where(da => scIds.Contains(da.Bjid) && da.ExecutionStatus == (byte)CustomSchedulerDtlsExecutionStatus.NotStarted)
                            .Select(da => da.Bjid).Distinct().ToArrayAsync();
                        foreach (var scheduler in schedulers)
                        {
                            var obj = new ScheduleServiceViewModel
                            {
                                Id = scheduler.Id,
                                Event = Enum.Parse<CustomSchedulerEventTypes>(scheduler.EventType),
                                //ScheduleDate = scheduler.ScheduleDate,
                                //ScheduleTime = scheduler.ScheduleTime,
                                Frequency = (CustomSchedulerFrequency)scheduler.Frequency,
                                AnySubPending = subs.Contains(scheduler.Id)
                            };
                            if (obj.Frequency == CustomSchedulerFrequency.DateAndTime && scheduler.ScheduleDate.HasValue && (!string.IsNullOrEmpty(scheduler.ScheduleTime)))
                            {
                                var tm = scheduler.ScheduleTime.Split(':');
                                obj.ScheduleDate = scheduler.ScheduleDate.Value.AddHours(Convert.ToInt32(tm[0]))
                                    .AddMinutes(Convert.ToInt32(tm[1]));
                                if (obj.ScheduleDate < CurrentTime)
                                {
                                    if (!(await dbContext.PhBgJobDetails.Where(da => da.Bjid == scheduler.Id && da.ExecutionStatus != (byte)CustomSchedulerDtlsExecutionStatus.NotStarted).AnyAsync()))
                                    {
                                        obj.IsMissed = true;
                                    }
                                }
                            }
                            else if (obj.Frequency == CustomSchedulerFrequency.Daily && (!string.IsNullOrEmpty(scheduler.ScheduleTime)))
                            {
                                var tm = scheduler.ScheduleTime.Split(':');
                                obj.ScheduleDate = CurrentTime.Date.AddHours(Convert.ToInt32(tm[0]))
                                    .AddMinutes(Convert.ToInt32(tm[1]));
                            }
                            models.Add(obj);
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.ListItems, "", ex);
                    trans.Rollback();
                }
            return models;
        }

        private async Task InsertSchedulerDetails(int scheduleId, int sendCount, EmailProviders serviceProviderId, string bulkId, List<Utilities.ViewModels.Communications.Emails.SendEmailResponseViewModel_messages> messages,
            string remarks, CustomSchedulerDtlsExecutionStatus executionStatus, int? jobId = null)
        {
            logger.Log(LogLevel.Debug, LoggingEvents.Other, "InsertScheduerDetails -> scheduleId:" + scheduleId);
            try
            {
                if (string.IsNullOrEmpty(bulkId))
                {
                    if (messages != null)
                    {
                        bulkId = prefix_messageIds + "" + string.Join(",", messages.Select(da => da.messageId));
                    }
                }
                PhBgJobDetail obj = null;
                if (jobId.HasValue)
                {
                    obj = await dbContext.PhBgJobDetails.Where(da => da.Bjid == scheduleId && da.JobId == jobId
                   && da.ExecutionStatus == (byte)CustomSchedulerDtlsExecutionStatus.NotStarted
                   && da.Status == (byte)RecordStatus.Active).FirstOrDefaultAsync();
                }
                if (obj != null)
                {
                    {
                        obj.ExecutedOn = CurrentTime;
                        obj.ServiceProviderId = (byte)serviceProviderId;
                        obj.SendCount = sendCount;
                        obj.DeliveredCount = 0;
                        obj.BulkReferenceId = bulkId ?? "";
                        obj.Remarks = remarks;
                        obj.ExecutionStatus = (byte)executionStatus;
                    };
                }
                else
                {
                    obj = new PhBgJobDetail
                    {
                        Bjid = scheduleId,
                        ExecutedOn = CurrentTime,
                        ServiceProviderId = (byte)serviceProviderId,
                        SendCount = sendCount,
                        DeliveredCount = 0,
                        BulkReferenceId = bulkId ?? "",
                        Remarks = remarks,
                        ExecutionStatus = (byte)executionStatus,
                        Status = (byte)RecordStatus.Active,
                        //CreatedDate = CurrentTime,
                        JobId = jobId
                    };
                    dbContext.PhBgJobDetails.Add(obj);
                }
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, LoggingEvents.Other, "InsertScheduerDetails -> scheduleId:" + scheduleId +
                    ", EmailProviders:" + serviceProviderId + ", bulkId:" + bulkId + ", messages:" + JsonConvert.SerializeObject(messages)
            + ", remarks:" + remarks + ", executionStatus:" + executionStatus + ", jobId:" + jobId, e);
            }
        }

        EmailProviders emailProvider = EmailProviders.InfoBip;
        Utilities.Interfaces.Communications.iMailing getEmailProvider(EmailProviders emailProv)
        {
            var obj = dbContext.PiEmailServiceProviders.Where(da => da.Status == (byte)RecordStatus.Active && da.DefaultFlag && da.ProviderCode == "IB").FirstOrDefault();
            if (obj == null) obj = dbContext.PiEmailServiceProviders.Where(da => da.Status == (byte)RecordStatus.Active && da.ProviderCode == "IB").FirstOrDefault();
            var notifyUrl = appSettings.AppSettingsProperties.HireApiUrl + "/api/v1/MailSupport/InfoBip/Webhook";
            if (obj != null)
            {
                return Utilities.Communications.EmailSupport.getObject(emailProv, obj.RequestUrl ?? "https://59k1z.api.infobip.com/", notifyUrl, obj.AuthKey ?? "cGFyYW1pbmZvdXNlcjpQYXJhbUA3ODk2", obj.FromName ?? "ParamInfo Careers", obj.FromEmailId ?? "piHire@email.paraminfo.com", emailSetCount);
            }
            else
                return Utilities.Communications.EmailSupport.getObject(emailProv, "https://59k1z.api.infobip.com/", notifyUrl, "cGFyYW1pbmZvdXNlcjpQYXJhbUA3ODk2", "ParamInfo Careers", "piHire@email.paraminfo.com", emailSetCount);
        }

        #region Birthday
        
        public async Task BirthdayAsync(int scheduleId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            PhBgJob scheduler = await dbContext.PhBgJobs.Where(da => da.Id == scheduleId)
                            .FirstOrDefaultAsync();
            if (scheduler == null || scheduler.Status != (byte)RecordStatus.Active)
            {
                logger.Log(LogLevel.Information, LoggingEvents.Other, "scheduleId:" + scheduleId + (scheduler == null ? " is not exist" : " is not active"));
                await InsertSchedulerDetails(scheduleId, 0, emailProvider, "", null, "Scheduler" + (scheduler == null ? " is not exist" : " is not active"), CustomSchedulerDtlsExecutionStatus.Failed);
            }
            else
            {
                var sendTo = (CustomSchedulerSendTo)scheduler.SendTo;
                if (sendTo == CustomSchedulerSendTo.AllUsers || sendTo == CustomSchedulerSendTo.piHireCandidates)
                    await this.CandidateBirthdayAsync(scheduleId, scheduler);
                if (sendTo == CustomSchedulerSendTo.AllUsers || sendTo == CustomSchedulerSendTo.piHireUsers)
                    await this.HireUserBirthdayAsync(scheduleId, scheduler);
            }
        }
        
        async Task CandidateBirthdayAsync(int scheduleId, PhBgJob scheduler)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var curr = CurrentTime.Date;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of method: scheduleId:" + scheduleId + " on " + curr);
                    var models = (await dbContext.GetCustomSchedulerCandidateBirthday(curr, scheduler.Pus, scheduler.Bus, scheduler.Gender, scheduler.CountryIds, scheduler.CandidateStatus)).ToList();
                    var candIds = models.Select(da => da.ID).ToArray();

                    if (candIds != null && candIds.Length > 0)
                    {
                        await SendCandidatesMailAsync(scheduler, candIds);
                    }
                    else
                    {
                        logger.Log(LogLevel.Information, LoggingEvents.Other, "No candidate birthday on :" + curr + " with:" + scheduler.Pus + "," + scheduler.Bus + "," + scheduler.Gender + "," + scheduler.CountryIds + "," + scheduler.CandidateStatus);

                        await InsertSchedulerDetails(scheduler.Id, 0, emailProvider, "", null, "No candidate birthday on :" + curr + " with :" + scheduler.Pus + "," + scheduler.Bus + "," + scheduler.Gender + "," + scheduler.CountryIds + "," + scheduler.CandidateStatus, CustomSchedulerDtlsExecutionStatus.Failed);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.Other, "scheduleId:" + scheduleId + " on " + curr, ex);
                    trans.Rollback();
                }
        }
        
        async Task HireUserBirthdayAsync(int scheduleId, PhBgJob scheduler)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var curr = CurrentTime.Date;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of method: scheduleId:" + scheduleId + " on " + curr);
                    {
                        var models = (await dbContext.GetCustomSchedulerHireUserBirthday(curr, scheduler.Pus, scheduler.Bus, scheduler.Gender, scheduler.CountryIds)).ToList();
                        var UsrIds = models.Select(da => da.ID).ToArray();
                        if (UsrIds != null && UsrIds.Length > 0)
                        {
                            await SendHireUsersMailAsync(scheduler, UsrIds);
                        }
                        else
                        {
                            logger.Log(LogLevel.Information, LoggingEvents.Other, "No Hire users birthday on :" + curr + " with:" + scheduler.Pus + "," + scheduler.Bus + "," + scheduler.Gender + "," + scheduler.CountryIds + "," + scheduler.CandidateStatus);
                            await InsertSchedulerDetails(scheduler.Id, 0, emailProvider, "", null, "No hire user birthday on: " + curr, CustomSchedulerDtlsExecutionStatus.Failed);
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.Other, "scheduleId:" + scheduleId + " on " + curr, ex);
                    trans.Rollback();
                }
        }
        #endregion
        #region Event is Happened
        
        public async Task EventHappendAsync(int scheduleId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            PhBgJob scheduler = await dbContext.PhBgJobs.Where(da => da.Id == scheduleId)
                            .FirstOrDefaultAsync();
            if (scheduler == null || scheduler.Status != (byte)RecordStatus.Active)
            {
                logger.Log(LogLevel.Information, LoggingEvents.Other, "scheduleId:" + scheduleId + (scheduler == null ? " is not exist" : " is not active"));
                await InsertSchedulerDetails(scheduleId, 0, emailProvider, "", null, "Scheduler" + (scheduler == null ? " is not exist" : " is not active"), CustomSchedulerDtlsExecutionStatus.Failed);
            }
            else
            {
                var sendTo = (CustomSchedulerSendTo)scheduler.SendTo;
                if (sendTo == CustomSchedulerSendTo.AllUsers || sendTo == CustomSchedulerSendTo.piHireCandidates)
                    await this.CandidateEventHappendAsync(scheduleId, scheduler);
                if (sendTo == CustomSchedulerSendTo.AllUsers || sendTo == CustomSchedulerSendTo.piHireUsers)
                    await this.HireUserEventHappendAsync(scheduleId, scheduler);
            }
        }
        
        async Task CandidateEventHappendAsync(int scheduleId, PhBgJob scheduler)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var curr = CurrentTime.Date;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of method: scheduleId:" + scheduleId + " on " + curr);
                    var models = (await dbContext.GetCustomSchedulerCandidate(scheduler.Pus, scheduler.Bus, scheduler.Gender, scheduler.CountryIds, scheduler.CandidateStatus)).ToList();
                    var candIds = models.Select(da => da.ID).ToArray();

                    if (candIds != null && candIds.Length > 0)
                    {
                        await SendCandidatesMailAsync(scheduler, candIds);
                    }
                    else
                    {
                        logger.Log(LogLevel.Information, LoggingEvents.Other, "No candidate to send with :" + scheduler.Pus + "," + scheduler.Bus + "," + scheduler.Gender + "," + scheduler.CountryIds + "," + scheduler.CandidateStatus);
                        await InsertSchedulerDetails(scheduler.Id, 0, emailProvider, "", null, "No candidate to send", CustomSchedulerDtlsExecutionStatus.Failed);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.Other, "scheduleId:" + scheduleId + " on " + curr, ex);
                    trans.Rollback();
                }
        }
        
        async Task HireUserEventHappendAsync(int scheduleId, PhBgJob scheduler)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var curr = CurrentTime.Date;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of method: scheduleId:" + scheduleId + " on " + curr);
                    {
                        var models = (await dbContext.GetCustomSchedulerHireUser(scheduler.Pus, scheduler.Bus, scheduler.Gender, scheduler.CountryIds)).ToList();
                        var UsrIds = models.Select(da => da.ID).ToArray();
                        if (UsrIds != null && UsrIds.Length > 0)
                        {
                            await SendHireUsersMailAsync(scheduler, UsrIds);
                        }
                        else
                        {
                            logger.Log(LogLevel.Information, LoggingEvents.Other, "No Hire users to send with :" + scheduler.Pus + "," + scheduler.Bus + "," + scheduler.Gender + "," + scheduler.CountryIds + "," + scheduler.CandidateStatus);
                            await InsertSchedulerDetails(scheduler.Id, 0, emailProvider, "", null, "No Hire users to send", CustomSchedulerDtlsExecutionStatus.Failed);
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.Other, "scheduleId:" + scheduleId + " on " + curr, ex);
                    trans.Rollback();
                }
        }
        #endregion
        #region Special Day
        
        public async Task SpecialDayAsync(int scheduleId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            PhBgJob scheduler = await dbContext.PhBgJobs.Where(da => da.Id == scheduleId)
                            .FirstOrDefaultAsync();
            if (scheduler == null || scheduler.Status != (byte)RecordStatus.Active)
            {
                logger.Log(LogLevel.Information, LoggingEvents.Other, "scheduleId:" + scheduleId + (scheduler == null ? " is not exist" : " is not active"));
                await InsertSchedulerDetails(scheduleId, 0, emailProvider, "", null, "Scheduler" + (scheduler == null ? " is not exist" : " is not active"), CustomSchedulerDtlsExecutionStatus.Failed);
            }
            else
            {
                var sendTo = (CustomSchedulerSendTo)scheduler.SendTo;
                if (sendTo == CustomSchedulerSendTo.AllUsers || sendTo == CustomSchedulerSendTo.piHireCandidates)
                    await this.CandidateSpecialDayAsync(scheduleId, scheduler);
                if (sendTo == CustomSchedulerSendTo.AllUsers || sendTo == CustomSchedulerSendTo.piHireUsers)
                    await this.HireUserSpecialDayAsync(scheduleId, scheduler);
            }
        }
        
        async Task CandidateSpecialDayAsync(int scheduleId, PhBgJob scheduler)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var curr = CurrentTime.Date;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of method: scheduleId:" + scheduleId + " on " + curr);
                    var models = (await dbContext.GetCustomSchedulerCandidate(scheduler.Pus, scheduler.Bus, scheduler.Gender, scheduler.CountryIds, scheduler.CandidateStatus)).ToList();
                    var candIds = models.Select(da => da.ID).ToArray();

                    if (candIds != null && candIds.Length > 0)
                    {
                        await SendCandidatesMailAsync(scheduler, candIds);
                    }
                    else
                    {
                        logger.Log(LogLevel.Information, LoggingEvents.Other, "2.No candidate to send with :" + scheduler.Pus + "," + scheduler.Bus + "," + scheduler.Gender + "," + scheduler.CountryIds + "," + scheduler.CandidateStatus);
                        await InsertSchedulerDetails(scheduler.Id, 0, emailProvider, "", null, "No candidate to send", CustomSchedulerDtlsExecutionStatus.Failed);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.Other, "scheduleId:" + scheduleId + " on " + curr, ex);
                    trans.Rollback();
                }
        }
        
        async Task HireUserSpecialDayAsync(int scheduleId, PhBgJob scheduler)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var curr = CurrentTime.Date;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of method: scheduleId:" + scheduleId + " on " + curr);
                    {
                        var models = (await dbContext.GetCustomSchedulerHireUser(scheduler.Pus, scheduler.Bus, scheduler.Gender, scheduler.CountryIds)).ToList();
                        var UsrIds = models.Select(da => da.ID).ToArray();
                        if (UsrIds != null && UsrIds.Length > 0)
                        {
                            await SendHireUsersMailAsync(scheduler, UsrIds);
                        }
                        else
                        {
                            logger.Log(LogLevel.Information, LoggingEvents.Other, "2.No Hire users to send with :" + scheduler.Pus + "," + scheduler.Bus + "," + scheduler.Gender + "," + scheduler.CountryIds + "," + scheduler.CandidateStatus);
                            await InsertSchedulerDetails(scheduler.Id, 0, emailProvider, "", null, "No Hire users to send", CustomSchedulerDtlsExecutionStatus.Failed);
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.Other, "scheduleId:" + scheduleId + " on " + curr, ex);
                    trans.Rollback();
                }
        }
        #endregion

        #region New Job
        
        public async Task NewJobPublishedAsync(int scheduleId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            PhBgJob scheduler = await dbContext.PhBgJobs.Where(da => da.Id == scheduleId)
                            .FirstOrDefaultAsync();
            if (scheduler == null || scheduler.Status != (byte)RecordStatus.Active)
            {
                logger.Log(LogLevel.Information, LoggingEvents.Other, "scheduleId:" + scheduleId + (scheduler == null ? " is not exist" : " is not active"));
                await InsertSchedulerDetails(scheduleId, 0, emailProvider, "", null, "Scheduler" + (scheduler == null ? " is not exist" : " is not active"), CustomSchedulerDtlsExecutionStatus.Failed);
            }
            else
            {
                List<int> jobIds = new List<int>();
                var pendingJobids = await dbContext.PhBgJobDetails.Where(da => da.Bjid == scheduleId && da.JobId.HasValue
                 && da.ExecutionStatus == (byte)CustomSchedulerDtlsExecutionStatus.NotStarted
                 && da.Status == (byte)RecordStatus.Active).Select(da => da.JobId.Value).ToListAsync();
                if (pendingJobids.Count > 0)
                    jobIds = pendingJobids;
                else
                {
                    var lastProcessDt = await dbContext.PhBgJobDetails.Where(da => da.Bjid == scheduleId && da.JobId.HasValue).Select(da => new { da.ExecutedOn }).FirstOrDefaultAsync();
                    var processedJobIds = await dbContext.PhBgJobDetails.Where(da => da.Bjid == scheduleId && da.JobId.HasValue).Select(da => da.JobId.Value).ToArrayAsync();
                    var fmDt = DateTime.Now.AddDays(-1);
                    var jobIdsQry = dbContext.PhJobOpenings.Where(da => processedJobIds.Contains(da.Id) == false && da.Status == (byte)RecordStatus.Active && da.PostedDate > fmDt);
                    if (lastProcessDt != null) jobIdsQry = jobIdsQry.Where(da => da.PostedDate > lastProcessDt.ExecutedOn);
                    jobIds = await jobIdsQry.Select(da => da.Id).ToListAsync();
                }
                if (jobIds != null && jobIds.Count > 0)
                {
                    var sendTo = (CustomSchedulerSendTo)scheduler.SendTo;
                    if (sendTo == CustomSchedulerSendTo.AllUsers || sendTo == CustomSchedulerSendTo.piHireCandidates)
                        await this.CandidateNewJobPublishedAsync(scheduleId, scheduler, jobIds);
                    //if (sendTo == CustomSchedulerSendTo.AllUsers || sendTo == CustomSchedulerSendTo.piHireUsers)
                    //    await this.HireUserNewJobPublishedAsync(scheduleId, scheduler, jobIds);
                }
                else
                {
                    logger.Log(LogLevel.Information, LoggingEvents.Other, "No new jobs / all completed");
                }
            }
        }
        
        async Task CandidateNewJobPublishedAsync(int scheduleId, PhBgJob scheduler, List<int> jobIds)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of method: scheduleId:" + scheduleId);

                    if (jobIds != null && jobIds.Count > 0)
                    {
                        foreach (var jobId in jobIds)
                        {
                            var models = (await dbContext.GetCustomSchedulerCandidateJobOpenings(jobId, scheduler.Pus, scheduler.Bus, scheduler.Gender, scheduler.CountryIds, scheduler.CandidateStatus)).ToList();
                            var candIds = models.Select(da => da.ID).ToArray();

                            if (candIds != null && candIds.Length > 0)
                            {
                                await SendCandidatesMailAsync(scheduler, candIds, jobId);
                            }
                            else
                            {
                                logger.Log(LogLevel.Information, LoggingEvents.Other, "No candidate for job :" + jobId);
                                await InsertSchedulerDetails(scheduler.Id, 0, emailProvider, "", null, "No candidate for job :" + jobId, CustomSchedulerDtlsExecutionStatus.SentSuccessfully, jobId);
                            }
                        }
                    }
                    else
                    {
                        logger.Log(LogLevel.Information, LoggingEvents.Other, "No new jobs / all completed");
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.Other, "scheduleId:" + scheduleId, ex);
                    trans.Rollback();
                }
        }
      
        //async Task HireUserNewJobPublishedAsync(int scheduleId, PhBgJobs scheduler, List<int> jobIds)
        //{
        //    logger.SetMethodName(MethodBase.GetCurrentMethod());
        //    using (var trans = await dbContext.Database.BeginTransactionAsync())
        //        try
        //        {
        //            logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of method: scheduleId:" + scheduleId);
        //            if (jobIds != null && jobIds.Count > 0)
        //            {
        //                foreach (var jobId in jobIds)
        //                {
        //                    var models = (await dbContext.GetCustomSchedulerHireUserJobOpening(scheduler.Pus, scheduler.Bus, scheduler.Gender, scheduler.CountryIds)).ToList();
        //                    var UsrIds = models.Select(da => da.ID).ToArray();
        //                    if (UsrIds != null && UsrIds.Length > 0)
        //                    {
        //                        await SendHireUsersMailAsync(scheduler, UsrIds, jobId);
        //                    }
        //                    else
        //                    {
        //                        logger.Log(LogLevel.Information, LoggingEvents.Other, "No Hire users job :" + jobId);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                logger.Log(LogLevel.Information, LoggingEvents.Other, "No new jobs / all completed");
        //            }
        //            trans.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Log(LogLevel.Error, LoggingEvents.Other, "scheduleId:" + scheduleId, ex);
        //            trans.Rollback();
        //        }
        //}
        #endregion

        //      dynamicLablesList = [{ "name": "COMPANY NAME", "value": "[COMPANY_NAME]" },
        //{ "name": "USER EMAIL SIGNATURE", "value": "[USER_EMAIL_SIGNATURE]" },
        //{ "name": "USER NAME", "value": "[USER_NAME]" },
        //{ "name": "USER PASSWORD", "value": "[USER_PASSWORD]" },


        ////{ "name": "RESET PWD LINK", "value": "[RESET_PWD_LINK]" },
        //{ "name": "INTERVIEW DATE", "value": "[INTERVIEW_DATE]" },
        //{ "name": "INTERVIEW LOCATION", "value": "[INTERVIEW_LOCATION]" },

        async Task SendCandidatesMailAsync(PhBgJob scheduler, int[] candIds, int? jobId = null)
        {
            var tmplt = await dbContext.PhMessageTemplates
                            .Where(da => da.Id == scheduler.EmailTemplateId && da.Status == (byte)RecordStatus.Active /*&& da.PublishStatus*/)
                            .Select(da => new { da.TplSubject, da.TplBody }).FirstOrDefaultAsync();
            if (tmplt == null)
            {
                logger.Log(LogLevel.Information, LoggingEvents.Other, "scheduleId :" + scheduler.Id + " Template id: " + scheduler.EmailTemplateId + " is not not active or published");
                await InsertSchedulerDetails(scheduler.Id, 0, emailProvider, "", null, "Template is not not active or published", CustomSchedulerDtlsExecutionStatus.Failed);
            }
            else
            {
                using (var mailObj = getEmailProvider(emailProvider))
                {

                    var mod = await processCandidateDynamicMailContent(tmplt.TplSubject, tmplt.TplBody, candIds, jobId);

                    var mailDtls = new Utilities.ViewModels.Communications.Emails.SendEmailRequestViewModel()
                    {
                        MailBody = mod.TplBody,
                        MailSubject = mod.TplSubject,
                        ToEmails = mod.toMails
                    };


                    var resp = await mailObj.SendBulkEmailsAsync(mailDtls);
                    foreach (var res in resp)
                    {
                        await InsertSchedulerDetails(scheduler.Id, res.Messages.Count, emailProvider, res.BulkId, res.Messages, "Sent successfully", CustomSchedulerDtlsExecutionStatus.SentSuccessfully, jobId);
                    }
                    if (jobId.HasValue)
                    {
                        await UpdateJobCounter(jobId.Value, resp.Sum(da => da.Messages?.Count ?? 0));
                    }
                    logger.Log(LogLevel.Debug, LoggingEvents.Other, "End of method: scheduleId:" + scheduler.Id + " resps " + JsonConvert.SerializeObject(resp));
                }
            }
        }
        
        async Task<(string TplSubject, string TplBody, List<Utilities.ViewModels.Communications.Emails.ToViewModel> toMails)>
            processCandidateDynamicMailContent(string TplSubject, string TplBody, int[] candIds, int? jobId = null)
        {
            TplSubject = TplSubject.Replace("[COMPANY_NAME]", COMPANY_NAME);
            TplBody = TplBody.Replace("[COMPANY_NAME]", COMPANY_NAME);
            if (jobId.HasValue)
            {
                var jbDtls = (await dbContext.GetCustomSchedulerJobDtls(jobId.Value)).FirstOrDefault();

                var JobSkills = (from job in dbContext.PhJobOpeningSkills.AsNoTracking()
                                 join tech in dbContext.PhTechnologysSes.AsNoTracking() on job.TechnologyId equals tech.Id
                                 where job.Joid == jobId && job.Status == (byte)RecordStatus.Active
                                 select new
                                 {
                                     ExpMonth = job.ExpMonth,
                                     ExpYears = job.ExpYears,
                                     Technology = tech.Title
                                 }).ToList();


                //{ "name": "JOB MATCHES", "value": "[JOB_MATCHES]" },
                //{ "name": "INTERVIEW MODE", "value": "[INTERVIEW_MODE]" },

                TplSubject = TplSubject.Replace("[CLIENT_NAME]", jbDtls.ClientName);
                TplBody = TplBody.Replace("[CLIENT_NAME]", jbDtls.ClientName);
                //TplBody = TplBody.Replace("[CLIENT_EMAIL]", jbDtls.ClientEmail);
                //TplBody = TplBody.Replace("[CLIENT_CONTACT_NO]", jbDtls.ClientContactNo);

                TplSubject = TplSubject.Replace("[JOB_ID]", jobId.ToString());
                TplBody = TplBody.Replace("[JOB_ID]", jobId.ToString());
                TplSubject = TplSubject.Replace("[JOB_TITLE]", jbDtls.JobTitle);
                TplBody = TplBody.Replace("[JOB_TITLE]", jbDtls.JobTitle);
                TplSubject = TplSubject.Replace("[JOB_DESC]", jbDtls.JobDescription);
                TplBody = TplBody.Replace("[JOB_DESC]", jbDtls.JobDescription);

                TplSubject = TplSubject.Replace("[JOB_CURRENCY]", jbDtls.JobCurrencyName);
                TplBody = TplBody.Replace("[JOB_CURRENCY]", jbDtls.JobCurrencyName);
                TplSubject = TplSubject.Replace("[JOB_STATUS]", jbDtls.JobStatus);
                TplBody = TplBody.Replace("[JOB_STATUS]", jbDtls.JobStatus);
                TplSubject = TplSubject.Replace("[JOB_LOCATION]", jbDtls.JobLocation + "/" + jbDtls.JobCountry);
                TplBody = TplBody.Replace("[JOB_LOCATION]", jbDtls.JobLocation + "/" + jbDtls.JobCountry);
                //TplBody = TplBody.Replace("[JOB_GROSS_PACKAGE]", jbDtls.OfferGrossPackagePerMonth.ToString());

                //TplBody = TplBody.Replace("[JOB_NET_SAL_MONTH]", jbDtls.OfferNetSalMonth.ToString());
                //TplBody = TplBody.Replace("[JOB_NET_SAL_MONTH_CURY]", jbDtls.OfferPackageCurrency);

                var minYears = ConvertYears(jbDtls.MinExpeInMonths);
                var maxYears = ConvertYears(jbDtls.MaxExpeInMonths);

                TplSubject = TplSubject.Replace("[JOB_EXPE]", minYears + " - " + maxYears + " Years");
                TplBody = TplBody.Replace("[JOB_EXPE]", minYears + " - " + maxYears + " Years");

                //TplBody = TplBody.Replace("[USER_NAME]", jbDtls.UserName);
                //TplBody = TplBody.Replace("[USER_PASSWORD]", jbDtls.UserPassword);
                TplSubject = TplSubject.Replace("[USER_SIGNATURE_DIV]", jbDtls.recruiterEmailSignature);
                TplBody = TplBody.Replace("[USER_SIGNATURE_DIV]", jbDtls.recruiterEmailSignature);

                TplSubject = TplSubject.Replace("[JOB_POSTED_ON]", jbDtls.PostedDate.ToString());
                TplBody = TplBody.Replace("[JOB_POSTED_ON]", jbDtls.PostedDate.ToString());
                TplSubject = TplSubject.Replace("[JOB_END_DATE]", jbDtls.ClosedDate.ToString());
                TplBody = TplBody.Replace("[JOB_END_DATE]", jbDtls.ClosedDate.ToString());

                TplSubject = TplSubject.Replace("[RECRUITER_NAME]", jbDtls.recruiterName);
                TplBody = TplBody.Replace("[RECRUITER_NAME]", jbDtls.recruiterName);
                TplSubject = TplSubject.Replace("[RECRUITER_POSITION]", jbDtls.recruiterPosition);
                TplBody = TplBody.Replace("[RECRUITER_POSITION]", jbDtls.recruiterPosition);
                TplSubject = TplSubject.Replace("[RECRUITER_EMAILID]", jbDtls.recruiterEmailID);
                TplBody = TplBody.Replace("[RECRUITER_EMAILID]", jbDtls.recruiterEmailID);
                TplSubject = TplSubject.Replace("[RECRUITER_PHONE_NUMBER]", jbDtls.recruiterMobileNumber);
                TplBody = TplBody.Replace("[RECRUITER_PHONE_NUMBER]", jbDtls.recruiterMobileNumber);

                TplSubject = TplSubject.Replace("[BDM_NAME]", jbDtls.bdmName);
                TplBody = TplBody.Replace("[BDM_NAME]", jbDtls.bdmName);


                string skillTr = string.Empty;
                string skillTable = "<table border='0' cellspacing='0' cellpadding='0'> !SkillTr </table>";
                if (JobSkills != null)
                {
                    for (int i = 0; i < JobSkills.Count; i++)
                    {
                        skillTr = skillTr + " " +
                            "<tr><td style='border: 1px solid #ccc;height: 31px;padding-left: 7px;font-size: 14px;font-weight:normal;'>" + JobSkills[i].Technology +
                            "</td><td style='border: 1px solid #ccc;height: 31px;padding-left: 7px;font-size: 14px;font-weight:normal;text-align:right;'>" + JobSkills[i].ExpYears +
                            " Years </td><td style='border: 1px solid #ccc;height: 31px;padding-left: 7px;font-size: 14px;font-weight:normal;text-align:right;'>" + JobSkills[i].ExpMonth +
                            " Months </td></tr>";
                    }
                    skillTable = skillTable.Replace("!SkillTr", skillTr);
                    TplSubject = TplSubject.Replace("[JOB_SKILLS]", skillTable);
                    TplBody = TplBody.Replace("[JOB_SKILLS]", skillTable);
                }

                //TplBody = TplBody.Replace("[CAN_REQ_DOCUMENT]", jbDtls.RequestDocuments);
                TplSubject = TplSubject.Replace("[JOB_APPLY_LINK]", appSettings.AppSettingsProperties.CandidateAppUrl + "/login?jobid=" + jobId);
                TplBody = TplBody.Replace("[JOB_APPLY_LINK]", appSettings.AppSettingsProperties.CandidateAppUrl + "/login?jobid=" + jobId);
                TplSubject = TplSubject.Replace("[CAND_LOGIN]", appSettings.AppSettingsProperties.CandidateAppUrl + "/home");
                TplBody = TplBody.Replace("[CAND_LOGIN]", appSettings.AppSettingsProperties.CandidateAppUrl + "/home");

                if (TplSubject.IndexOf("[HIRE_MANAGER]") != -1 && TplBody.IndexOf("[HIRE_MANAGER]") != -1)
                {
                    if (jbDtls.PUID.HasValue && jbDtls.BUID.HasValue)
                    {
                        var hireAdmins = await dbContext.PiHireUsers.AsNoTracking().Where(da => da.UserType == (byte)UserType.Admin).Select(da => new { da.Id, da.FirstName, da.LastName }).ToListAsync();
                        var hireAdminsIds = hireAdmins.Select(da => da.Id).Distinct().ToArray();
                        var hireAdminId = await dbContext.VwUserPuBus.Where(da => hireAdminsIds.Contains(da.UserId) && da.ProcessUnit == jbDtls.PUID /*&& da.BusinessUnit == jbDtls.BUID*/).Select(da => da.UserId).FirstOrDefaultAsync();
                        if (hireAdminId != null && hireAdminId > 0)
                        {
                            var name = hireAdmins.Where(da => da.Id == hireAdminId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault();
                            TplSubject = TplSubject.Replace("[HIRE_MANAGER]", name);
                            TplBody = TplBody.Replace("[HIRE_MANAGER]", name);
                        }
                        else
                        {
                            logger.Log(LogLevel.Information, LoggingEvents.Other, "JobId :" + jobId + " -> Puid:" + jbDtls.PUID + ", Buid->" + jbDtls.PUID + " has no admin");
                        }
                    }
                    else
                    {
                        logger.Log(LogLevel.Information, LoggingEvents.Other, "JobId :" + jobId + " has no Puid or Buid");
                    }
                }
            }
            TplSubject = TplSubject.Replace("[PI_SIGNATURE_DIV]", PI_SIGNATURE_DIV);
            TplBody = TplBody.Replace("[PI_SIGNATURE_DIV]", PI_SIGNATURE_DIV);

            TplSubject = TplSubject.Replace("[CAND_NAME]", "{{CAND_NAME}}");
            TplBody = TplBody.Replace("[CAND_NAME]", "{{CAND_NAME}}");
            TplSubject = TplSubject.Replace("[CAND_ID]", "{{CAND_ID}}");
            TplBody = TplBody.Replace("[CAND_ID]", "{{CAND_ID}}");
            TplSubject = TplSubject.Replace("[CAND_EMAIL]", "{{CAND_EMAIL}}");
            TplBody = TplBody.Replace("[CAND_EMAIL]", "{{CAND_EMAIL}}");
            TplSubject = TplSubject.Replace("[CAND_CONTACT_NO]", "{{CAND_CONTACT_NO}}");
            TplBody = TplBody.Replace("[CAND_CONTACT_NO]", "{{CAND_CONTACT_NO}}");
            //TplBody = TplBody.Replace("[CAND_LOGIN]", "{{CAND_LOGIN}}");
            TplSubject = TplSubject.Replace("[CAND_STATUS]", "{{CAND_STATUS}}");
            TplBody = TplBody.Replace("[CAND_STATUS]", "{{CAND_STATUS}}");

            List<_PhCandidateProfiles> candidates = new List<_PhCandidateProfiles>();
            List<_CandStatus> CandStatus = new List<_CandStatus>();
            var pgCnt = entityPgCnt;
            for (int i = 0; i < candIds.Length; i += pgCnt)
            {
                var _candIds = candIds.Skip(i).Take(pgCnt).ToArray();
                var _candidates =
                    await dbContext.PhCandidateProfiles.Where(can => _candIds.Contains(can.Id) && can.Status != (byte)RecordStatus.Delete && can.Status != (byte)RecordStatus.Unsubscribe)
                        .Select(can => new _PhCandidateProfiles
                        {
                            CandContactNo = can.ContactNo,
                            CandEmail = can.EmailId,
                            CandName = can.CandName,
                            Id = can.Id
                        }).ToListAsync();
                candidates.AddRange(_candidates);

                var _CandStatus =
                  await dbContext.PhJobCandidates.Where(da => da.Joid == jobId && _candIds.Contains(da.CandProfId))
                    .Join(dbContext.PhCandStatusSes, da => da.CandProfStatus, da2 => da2.Id, (da, da2) => new _CandStatus { CandProfId = da.CandProfId, Title = da2.Title }).ToListAsync();
                CandStatus.AddRange(_CandStatus);

                //var candidates = await (from can in dbContext.PhCandidateProfiles
                //                        join JobCan in dbContext.PhJobCandidates on can.Id equals JobCan.CandProfId
                //                        //join user in dbContext.PiHireUsers on can.EmailId equals user.UserName
                //                        where _candIds.Contains(can.Id)
                //                        select new
                //                        {
                //                            CandContactNo = can.ContactNo,
                //                            CandEmail = can.EmailId,
                //                            CandName = can.CandName,
                //                            can.Id,
                //                            CandStatus = dbContext.PhCandStatusSes.Where(x => x.Id == JobCan.CandProfStatus).Select(x => x.Title).FirstOrDefault(),
                //                            //OfferGrossPackagePerMonth = JobCan.OpgrossPayPerMonth,
                //                            //OfferNetSalMonth = JobCan.OptakeHomePerMonth,
                //                            //OfferPackageCurrency = JobCan.Opcurrency,
                //                            //RecruiterId = JobCan.RecruiterId,
                //                            //UserId = user.UserId,
                //                            //ToEmail = user.UserName
                //                        }).ToListAsync();
            }
            var toMails = new List<Utilities.ViewModels.Communications.Emails.ToViewModel>();
            foreach (var candidate in candidates)
            {
                toMails.Add(new Utilities.ViewModels.Communications.Emails.ToViewModel
                {
                    Address = candidate.CandEmail,
                    Name = candidate.CandName,
                    Placeholders = new Dictionary<string, string> {
                                    { "CAND_NAME", candidate.CandName },
                                    { "CAND_ID", candidate.Id+"" },
                                    { "CAND_EMAIL", candidate.CandEmail },
                                    { "CAND_CONTACT_NO", candidate.CandContactNo },
                                    { "CAND_STATUS", CandStatus.Where(da=>da.CandProfId==candidate.Id).Select(da=>da.Title).FirstOrDefault() },
                                    { "UnsubscribeLink", GetCandidateUnSubscribeURL(candidate.Id + "", candidate.CandEmail) }
                                }
                });
            }

            TplBody = GenerateUnsubscribeLink_UpdateMsgBody(await GetTmpltOuterBody(TplBody));
            return (TplSubject, TplBody, toMails);
        }

        async Task SendHireUsersMailAsync(PhBgJob scheduler, int[] usrIds, int? jobId = null)
        {
            var users = new List<_PiHireUsers>();
            var pgCnt = entityPgCnt;
            for (int i = 0; i < usrIds.Length; i += pgCnt)
            {
                var _usrIds = usrIds.Skip(i).Take(pgCnt).ToArray();

                var _users = await dbContext.PiHireUsers.Where(da => _usrIds.Contains(da.Id))
                    .Select(da => new _PiHireUsers { Id = da.Id, EmailId = da.EmailId, FirstName = da.FirstName, LastName = da.LastName, MobileNumber = da.MobileNumber }).ToListAsync();
                users.AddRange(_users);
            }

            var tmplt = await dbContext.PhMessageTemplates
                            .Where(da => da.Id == scheduler.EmailTemplateId && da.Status == (byte)RecordStatus.Active /*&& da.PublishStatus*/)
                            .Select(da => new { da.TplSubject, da.TplBody }).FirstOrDefaultAsync();
            if (tmplt == null)
            {
                logger.Log(LogLevel.Information, LoggingEvents.Other, "scheduleId :" + scheduler.Id + " Template id: " + scheduler.EmailTemplateId + " is not not active or published");
                await InsertSchedulerDetails(scheduler.Id, 0, emailProvider, "", null, "Template is not not active or published", CustomSchedulerDtlsExecutionStatus.Failed);
            }
            else
            {
                using (var mailObj = getEmailProvider(emailProvider))
                {
                    var mod = await processHireUserDynamicMailContent(tmplt.TplSubject, tmplt.TplBody, usrIds, jobId);

                    var mailDtls = new Utilities.ViewModels.Communications.Emails.SendEmailRequestViewModel()
                    {
                        MailBody = mod.TplBody,
                        MailSubject = mod.TplSubject,
                        ToEmails = mod.toMails
                    };

                    var resp = await mailObj.SendBulkEmailsAsync(mailDtls);
                    foreach (var res in resp)
                    {
                        await InsertSchedulerDetails(scheduler.Id, res.Messages.Count, emailProvider, res.BulkId, res.Messages, "Sent successfully", CustomSchedulerDtlsExecutionStatus.SentSuccessfully, jobId);
                    }
                    if (jobId.HasValue)
                    {
                        await UpdateJobCounter(jobId.Value, resp.Sum(da => da.Messages?.Count ?? 0));
                    }
                    logger.Log(LogLevel.Debug, LoggingEvents.Other, "End of method: scheduleId:" + scheduler.Id + " resps " + JsonConvert.SerializeObject(resp));
                }
            }
        }
        
        async Task<(string TplSubject, string TplBody, List<Utilities.ViewModels.Communications.Emails.ToViewModel> toMails)>
            processHireUserDynamicMailContent(string TplSubject, string TplBody, int[] userIds, int? jobId = null)
        {
            TplSubject = TplSubject.Replace("[COMPANY_NAME]", COMPANY_NAME);
            TplBody = TplBody.Replace("[COMPANY_NAME]", COMPANY_NAME);
            if (jobId.HasValue)
            {
                var jbDtls = (await dbContext.GetCustomSchedulerJobDtls(jobId.Value)).FirstOrDefault();

                var JobSkills = (from job in dbContext.PhJobOpeningSkills
                                 join tech in dbContext.PhTechnologysSes on job.TechnologyId equals tech.Id
                                 where job.Id == jobId && job.Status == (byte)RecordStatus.Active
                                 select new
                                 {
                                     ExpMonth = job.ExpMonth,
                                     ExpYears = job.ExpYears,
                                     Technology = tech.Title
                                 }).ToList();


                //{ "name": "JOB MATCHES", "value": "[JOB_MATCHES]" },
                //{ "name": "INTERVIEW MODE", "value": "[INTERVIEW_MODE]" },
                TplSubject = TplSubject.Replace("[CLIENT_NAME]", jbDtls.ClientName);
                TplBody = TplBody.Replace("[CLIENT_NAME]", jbDtls.ClientName);
                //TplBody = TplBody.Replace("[CLIENT_EMAIL]", jbDtls.ClientEmail);
                //TplBody = TplBody.Replace("[CLIENT_CONTACT_NO]", jbDtls.ClientContactNo);
                TplSubject = TplSubject.Replace("[JOB_ID]", jobId.ToString());
                TplBody = TplBody.Replace("[JOB_ID]", jobId.ToString());
                TplSubject = TplSubject.Replace("[JOB_TITLE]", jbDtls.JobTitle);
                TplBody = TplBody.Replace("[JOB_TITLE]", jbDtls.JobTitle);
                TplSubject = TplSubject.Replace("[JOB_DESC]", jbDtls.JobDescription);
                TplBody = TplBody.Replace("[JOB_DESC]", jbDtls.JobDescription);

                TplSubject = TplSubject.Replace("[JOB_CURRENCY]", jbDtls.JobCurrencyName);
                TplBody = TplBody.Replace("[JOB_CURRENCY]", jbDtls.JobCurrencyName);
                TplSubject = TplSubject.Replace("[JOB_STATUS]", jbDtls.JobStatus);
                TplBody = TplBody.Replace("[JOB_STATUS]", jbDtls.JobStatus);
                TplSubject = TplSubject.Replace("[JOB_LOCATION]", jbDtls.JobLocation + "/" + jbDtls.JobCountry);
                TplBody = TplBody.Replace("[JOB_LOCATION]", jbDtls.JobLocation + "/" + jbDtls.JobCountry);
                //TplBody = TplBody.Replace("[JOB_GROSS_PACKAGE]", jbDtls.OfferGrossPackagePerMonth.ToString());

                //TplBody = TplBody.Replace("[JOB_NET_SAL_MONTH]", jbDtls.OfferNetSalMonth.ToString());
                //TplBody = TplBody.Replace("[JOB_NET_SAL_MONTH_CURY]", jbDtls.OfferPackageCurrency);

                var minYears = ConvertYears(jbDtls.MinExpeInMonths);
                var maxYears = ConvertYears(jbDtls.MaxExpeInMonths);
                TplSubject = TplSubject.Replace("[JOB_EXPE]", minYears + " - " + maxYears + " Years");
                TplBody = TplBody.Replace("[JOB_EXPE]", minYears + " - " + maxYears + " Years");

                //TplBody = TplBody.Replace("[USER_NAME]", jbDtls.UserName);
                //TplBody = TplBody.Replace("[USER_PASSWORD]", jbDtls.UserPassword);
                TplSubject = TplSubject.Replace("[USER_SIGNATURE_DIV]", jbDtls.recruiterEmailSignature);
                TplBody = TplBody.Replace("[USER_SIGNATURE_DIV]", jbDtls.recruiterEmailSignature);

                TplSubject = TplSubject.Replace("[JOB_POSTED_ON]", jbDtls.PostedDate.ToString());
                TplBody = TplBody.Replace("[JOB_POSTED_ON]", jbDtls.PostedDate.ToString());
                TplSubject = TplSubject.Replace("[JOB_END_DATE]", jbDtls.ClosedDate.ToString());
                TplBody = TplBody.Replace("[JOB_END_DATE]", jbDtls.ClosedDate.ToString());

                TplSubject = TplSubject.Replace("[RECRUITER_NAME]", jbDtls.recruiterName);
                TplBody = TplBody.Replace("[RECRUITER_NAME]", jbDtls.recruiterName);
                TplSubject = TplSubject.Replace("[RECRUITER_POSITION]", jbDtls.recruiterPosition);
                TplBody = TplBody.Replace("[RECRUITER_POSITION]", jbDtls.recruiterPosition);
                TplSubject = TplSubject.Replace("[RECRUITER_EMAILID]", jbDtls.recruiterEmailID);
                TplBody = TplBody.Replace("[RECRUITER_EMAILID]", jbDtls.recruiterEmailID);
                TplSubject = TplSubject.Replace("[RECRUITER_PHONE_NUMBER]", jbDtls.recruiterMobileNumber);
                TplBody = TplBody.Replace("[RECRUITER_PHONE_NUMBER]", jbDtls.recruiterMobileNumber);

                TplSubject = TplSubject.Replace("[BDM_NAME]", jbDtls.bdmName);
                TplBody = TplBody.Replace("[BDM_NAME]", jbDtls.bdmName);

                string skillTr = string.Empty;
                string skillTable = "<table border='0' cellspacing='0' cellpadding='0'> !SkillTr </table>";
                if (JobSkills != null)
                {
                    for (int i = 0; i < JobSkills.Count; i++)
                    {
                        skillTr = skillTr + " " +
                            "<tr><td style='border: 1px solid #ccc;height: 31px;padding-left: 7px;font-size: 14px;font-weight:normal;'>" + JobSkills[i].Technology +
                            "</td><td style='border: 1px solid #ccc;height: 31px;padding-left: 7px;font-size: 14px;font-weight:normal;text-align:right;'>" + JobSkills[i].ExpYears +
                            " Years </td><td style='border: 1px solid #ccc;height: 31px;padding-left: 7px;font-size: 14px;font-weight:normal;text-align:right;'>" + JobSkills[i].ExpMonth +
                            " Months </td></tr>";
                    }
                    skillTable = skillTable.Replace("!SkillTr", skillTr);
                    TplSubject = TplSubject.Replace("[JOB_SKILLS]", skillTable);
                    TplBody = TplBody.Replace("[JOB_SKILLS]", skillTable);
                }

                //TplBody = TplBody.Replace("[CAN_REQ_DOCUMENT]", jbDtls.RequestDocuments);
                TplSubject = TplSubject.Replace("[JOB_APPLY_LINK]", appSettings.AppSettingsProperties.CandidateAppUrl + "/login?jobid=" + jobId);
                TplBody = TplBody.Replace("[JOB_APPLY_LINK]", appSettings.AppSettingsProperties.CandidateAppUrl + "/login?jobid=" + jobId);
                TplSubject = TplSubject.Replace("[CAND_LOGIN]", appSettings.AppSettingsProperties.CandidateAppUrl + "/home");
                TplBody = TplBody.Replace("[CAND_LOGIN]", appSettings.AppSettingsProperties.CandidateAppUrl + "/home");

                if (TplSubject.IndexOf("[HIRE_MANAGER]") != -1 && TplBody.IndexOf("[HIRE_MANAGER]") != -1)
                {
                    if (jbDtls.PUID.HasValue && jbDtls.BUID.HasValue)
                    {
                        var hireAdmins = await dbContext.PiHireUsers.AsNoTracking().Where(da => da.UserType == (byte)UserType.Admin).Select(da => new { da.Id, da.FirstName, da.LastName }).ToListAsync();
                        var hireAdminsIds = hireAdmins.Select(da => da.Id).Distinct().ToArray();
                        var hireAdminId = await dbContext.VwUserPuBus.Where(da => hireAdminsIds.Contains(da.UserId) && da.ProcessUnit == jbDtls.PUID /*&& da.BusinessUnit == jbDtls.BUID*/).Select(da => da.UserId).FirstOrDefaultAsync();
                        if (hireAdminId != null && hireAdminId > 0)
                        {
                            var name = hireAdmins.Where(da => da.Id == hireAdminId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault();
                            TplSubject = TplSubject.Replace("[HIRE_MANAGER]", name);
                            TplBody = TplBody.Replace("[HIRE_MANAGER]", name);
                        }
                        else
                        {
                            logger.Log(LogLevel.Information, LoggingEvents.Other, "JobId :" + jobId + " -> Puid:" + jbDtls.PUID + ", Buid->" + jbDtls.PUID + " has no admin");
                        }
                    }
                    else
                    {
                        logger.Log(LogLevel.Information, LoggingEvents.Other, "JobId :" + jobId + " has no Puid or Buid");
                    }
                }
            }
            TplSubject = TplSubject.Replace("[PI_SIGNATURE_DIV]", PI_SIGNATURE_DIV);
            TplBody = TplBody.Replace("[PI_SIGNATURE_DIV]", PI_SIGNATURE_DIV);

            TplSubject = TplSubject.Replace("[CAND_NAME]", "{{CAND_NAME}}");
            TplBody = TplBody.Replace("[CAND_NAME]", "{{CAND_NAME}}");
            TplSubject = TplSubject.Replace("[CAND_ID]", "{{CAND_ID}}");
            TplBody = TplBody.Replace("[CAND_ID]", "{{CAND_ID}}");
            TplSubject = TplSubject.Replace("[CAND_EMAIL]", "{{CAND_EMAIL}}");
            TplBody = TplBody.Replace("[CAND_EMAIL]", "{{CAND_EMAIL}}");
            TplSubject = TplSubject.Replace("[CAND_CONTACT_NO]", "{{CAND_CONTACT_NO}}");
            TplBody = TplBody.Replace("[CAND_CONTACT_NO]", "{{CAND_CONTACT_NO}}");
            //TplBody = TplBody.Replace("[CAND_LOGIN]", "{{CAND_LOGIN}}");
            //TplBody = TplBody.Replace("[CAND_STATUS]", "{{CAND_STATUS}}");
            var candidates = new List<_candidates>();
            var pgCnt = entityPgCnt;
            for (int i = 0; i < userIds.Length; i += pgCnt)
            {
                var _userIds = userIds.Skip(i).Take(pgCnt).ToArray();

                var _candidates = await (from can in dbContext.PiHireUsers
                                             //join JobCan in dbContext.PhJobCandidates on can.Id equals JobCan.CandProfId
                                             //join user in dbContext.PiHireUsers on can.EmailId equals user.UserName
                                         where _userIds.Contains(can.Id)
                                         select new _candidates
                                         {
                                             CandContactNo = can.MobileNumber,
                                             CandEmail = can.EmailId,
                                             CandName = can.FirstName + " " + can.LastName,
                                             Id = can.Id
                                             //CandStatus = dbContext.PhCandStatusSes.Where(x => x.Id == JobCan.CandProfStatus).Select(x => x.Title).FirstOrDefault(),
                                             //OfferGrossPackagePerMonth = JobCan.OpgrossPayPerMonth,
                                             //OfferNetSalMonth = JobCan.OptakeHomePerMonth,
                                             //OfferPackageCurrency = JobCan.Opcurrency,
                                             //RecruiterId = JobCan.RecruiterId,
                                             //UserId = user.UserId,
                                             //ToEmail = user.UserName
                                         }).ToListAsync();
                candidates.AddRange(_candidates);
            }
            var toMails = new List<Utilities.ViewModels.Communications.Emails.ToViewModel>();
            TplSubject = TplSubject.Replace("[CAND_NAME]", "{{CAND_NAME}}");
            TplBody = TplBody.Replace("[CAND_NAME]", "{{CAND_NAME}}");
            TplSubject = TplSubject.Replace("[CAND_ID]", "{{CAND_ID}}");
            TplBody = TplBody.Replace("[CAND_ID]", "{{CAND_ID}}");
            TplSubject = TplSubject.Replace("[CAND_EMAIL]", "{{CAND_EMAIL}}");
            TplBody = TplBody.Replace("[CAND_EMAIL]", "{{CAND_EMAIL}}");
            TplSubject = TplSubject.Replace("[CAND_CONTACT_NO]", "{{CAND_CONTACT_NO}}");
            TplBody = TplBody.Replace("[CAND_CONTACT_NO]", "{{CAND_CONTACT_NO}}");
            //TplBody = TplBody.Replace("[CAND_LOGIN]", "{{CAND_LOGIN}}");
            //TplBody = TplBody.Replace("[CAND_STATUS]", "{{CAND_STATUS}}");
            foreach (var candidate in candidates)
            {
                toMails.Add(new Utilities.ViewModels.Communications.Emails.ToViewModel
                {
                    Address = candidate.CandEmail,
                    Name = candidate.CandName,
                    Placeholders = new Dictionary<string, string> {
                                    { "CAND_NAME", candidate.CandName },
                                    { "CAND_ID", candidate.Id+"" },
                                    { "CAND_EMAIL", candidate.CandEmail },
                                    { "CAND_CONTACT_NO", candidate.CandContactNo }
                                }
                });
            }

            TplBody = await GetTmpltOuterBody(TplBody);
            return (TplSubject, TplBody, toMails);
        }
        async Task<string> GetTmpltOuterBody(string TplBody)
        {
            string template = "!html_body";
            var templatePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "EmailTemplates", "DistributionEmail.html");
            if (System.IO.File.Exists(templatePath))
                template = System.IO.File.ReadAllText(templatePath);
            else
            {
                templatePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "EmailTemplates", "DistributionEmail.html");
                if (System.IO.File.Exists(templatePath))
                    template = System.IO.File.ReadAllText(templatePath);
                else
                {
                    if (!System.IO.Directory.Exists(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "piHireService")))
                        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "piHireService"));
                    System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "piHireService", "missing.txt"), "path:" + templatePath);
                    logger.Log(LogLevel.Error, LoggingEvents.MandatoryDataMissing, "file missing path:" + templatePath);
                }
            }
            int startIndx = 0;
            while (TplBody.IndexOf('[', startIndx) != -1)
            {
                //x [abcd] saj
                var indx = TplBody.IndexOf('[', startIndx);//2
                {
                    var len = TplBody.IndexOf(']', indx) - indx + 1;
                    if (len > 0)
                    {
                        var keyWord = TplBody.Substring(indx, len);//7-2+1
                        if (keyWord.Split(' ').Length == 1)
                        {
                            TplBody = TplBody.Replace(keyWord, "");
                        }
                    }
                }
                startIndx = indx + 1;
            }
            return template.Replace("!html_body", TplBody);
        }


        private async Task UpdateJobCounter(int JobId, int emailCount)
        {
            var obj = await dbContext.PhJobOpeningActvCounters.FirstOrDefaultAsync(da => da.Joid == JobId);
            if (obj == null)
            {
                obj = new PhJobOpeningActvCounter
                {
                    AsmtCounter = 0,
                    ClientViewsCounter = 0,
                    EmailsCounter = emailCount,
                    JobPostingCounter = 0,
                    Joid = JobId,
                    Status = (byte)RecordStatus.Active
                };
                dbContext.PhJobOpeningActvCounters.Add(obj);
            }
            else
                obj.EmailsCounter += emailCount;
            await dbContext.SaveChangesAsync();

        }
        private string GenerateUnsubscribeLink_UpdateMsgBody(string TplBody)
        {
            if (TplBody.IndexOf("[UnsubscribeLink]") == -1)
                TplBody += DefaultUnsubscribeTmplt;

            return TplBody.Replace("[UnsubscribeLink]", "{{UnsubscribeLink}}");
        }


        public async Task<string> UpdateInfoBipStatus()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.Other, "Start of method");
                var curDt = CurrentTime;
                using (var mailObj = getEmailProvider(EmailProviders.InfoBip))
                {
                    var obj = new InfoBipStatus(mailObj.ProviderAuth.ProviderUrl ?? "https://59k1z.api.infobip.com/", mailObj.ProviderAuth.AuthKey ?? "cGFyYW1pbmZvdXNlcjpQYXJhbUA3ODk2");
                    #region Report
                    //var ReportRslt = await obj.Report();
                    //logger.Log(LogLevel.Debug, LoggingEvents.Other, "Report Data:" + Newtonsoft.Json.JsonConvert.SerializeObject(ReportRslt));
                    //foreach (var rptRslt in ReportRslt)
                    //{
                    //    MailDeliveryStatus? distStatus = InfoBipStatus.ConvertToMailDeliveryStatus(rptRslt.status);
                    //    //if (distStatus.HasValue)
                    //    //    await dbContext.PiDistributionContacts.Where(da => da.InfobipMsgId == rptRslt.MessageId).UpdateAsync(da => new PiDistributionContacts { InfoBipStatus = (byte)rptRslt.status, DistStatus = (byte)distStatus.Value, UpdatedDate = curDt });
                    //    //else
                    //    //    await dbContext.PiDistributionContacts.Where(da => da.InfobipMsgId == rptRslt.MessageId).UpdateAsync(da => new PiDistributionContacts { InfoBipStatus = (byte)rptRslt.status, UpdatedDate = curDt });
                    //}
                    #endregion

                    #region Logging                
                    //byte[] infoBipStatus = InfoBipStatus.FinalStatus;
                    var dt = CurrentTime.AddDays(-2);
                    //https://dev.infobip.com/email-messaging/email-messages-logs
                    //Email logs are available for the last 48 hours!

                    var lst = await dbContext.PhBgJobDetails.Where(da => da.ExecutedOn > dt && da.SendCount > da.DeliveredCount &&
                                        da.ExecutionStatus == (byte)CustomSchedulerDtlsExecutionStatus.SentSuccessfully &&
                                        da.Status == (byte)RecordStatus.Active &&
                                        da.ServiceProviderId == (int)EmailProviders.InfoBip).ToListAsync();

                    foreach (var BulkObj in lst.Where(da => string.IsNullOrEmpty(da.BulkReferenceId) == false && da.BulkReferenceId.StartsWith(prefix_messageIds) == false).Select(da => new { da.BulkReferenceId, da.SendCount }).Distinct())
                    {
                        var Rslt = await obj.LogStatus(InfobipBulkId: BulkObj.BulkReferenceId, logSize: BulkObj.SendCount);
                        logger.Log(LogLevel.Debug, LoggingEvents.Other, "BulkObj:" + JsonConvert.SerializeObject(BulkObj) + ", Report Data:" + JsonConvert.SerializeObject(Rslt));
                        var _obj = lst.Where(da => BulkObj.BulkReferenceId == da.BulkReferenceId).FirstOrDefault();
                        if (_obj != null)
                        {
                            _obj.DeliveredCount = Rslt.Where(da => da.status == InfoBip_DeliveryStatus.OK || da.status == InfoBip_DeliveryStatus.DELIVERED).Select(da => da.MessageId).Distinct().Count();
                        }
                    }
                    //"messageIds:" + string.Join(",", messages.Select(da => da.messageId))
                    foreach (var messageIds in lst.Where(da => string.IsNullOrEmpty(da.BulkReferenceId) == false && da.BulkReferenceId.StartsWith(prefix_messageIds)).Select(da => da.BulkReferenceId).Distinct())
                    {
                        var _obj = lst.Where(da => messageIds == da.BulkReferenceId).FirstOrDefault();
                        if (_obj != null)
                        {
                            _obj.DeliveredCount = 0;
                            foreach (var messageId in messageIds.Substring(prefix_messageIds.Length).Split(','))
                            {
                                var Rslt = await obj.LogStatus(InfobipBulkId: null, InfobipMsgId: messageId);
                                logger.Log(LogLevel.Debug, LoggingEvents.Other, "InfobipMsgId:" + messageId + ", Report Data:" + JsonConvert.SerializeObject(Rslt));
                                _obj.DeliveredCount += Rslt.Where(da => da.status == InfoBip_DeliveryStatus.OK || da.status == InfoBip_DeliveryStatus.DELIVERED).Select(da => da.MessageId).Distinct().Count();
                            }
                        }
                    }
                    await dbContext.SaveChangesAsync();
                    #endregion
                }
                logger.Log(LogLevel.Debug, LoggingEvents.Other, "Process completed");
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, LoggingEvents.Other, "", e);
            }
            return string.Empty;
        }
        public async Task<int> TmpAllCandidatesUpdate()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method");
                var tmp = await dbContext.TmpAllCandidatesUpdate();
                logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Completed");
                return tmp;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, "", ex);
            }
            return 0;
        }
        public async Task<int> RecruiterJobAssignmentsCarryForwardAsync()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            if (CurrentTime.Hour == 0)
                try
                {
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method");
                    var tmp = await dbContext.RecruiterJobAssignmentsCarryForwardAsync();
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Completed");
                    return tmp;
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.ListItems, "", ex);
                }
            return 0;
        }
    }

    public class _candidates
    {
        public string CandContactNo { get; set; }
        public string CandEmail { get; set; }
        public string CandName { get; set; }
        public int Id { get; set; }
    }

    public class _PiHireUsers
    {
        public int Id { get; set; }
        public string EmailId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
    }

    public class _CandStatus
    {
        public int CandProfId { get; set; }
        public string Title { get; set; }
    }

    public class _PhCandidateProfiles
    {
        public string CandContactNo { get; set; }
        public string CandEmail { get; set; }
        public string CandName { get; set; }
        public int Id { get; set; }
    }
}
