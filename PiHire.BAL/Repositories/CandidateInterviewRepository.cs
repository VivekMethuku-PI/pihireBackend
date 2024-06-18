using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using PiHire.BAL.Common;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Entities;
using PiHire.Utilities.Communications.Emails;
using PiHire.Utilities.Communications.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class CandidateInterviewRepository : BaseRepository, ICandidateInterviewRepository
    {
        readonly Logger logger;

        private readonly IWebHostEnvironment _environment;


        public CandidateInterviewRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<CandidateInterviewRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }


        public async Task<CreateResponseViewModel<string>> ShareProfilesToClient(CandidateShareProfileViewModel candidateShareProfileViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Shared Successfully";
            int UserId = Usr.Id;
            string UserName = Usr.UserDetails.Email;

            try
            {
                
                var opening = (from job in dbContext.PhJobOpenings
                               join jobAddtl in dbContext.PhJobOpeningsAddlDetails on job.Id equals jobAddtl.Joid
                               join Cury in dbContext.PhRefMasters on jobAddtl.CurrencyId equals Cury.Id
                               where job.Id == candidateShareProfileViewModel.JobId
                               select new
                               {
                                   job.JobTitle,
                                   job.JobRole,
                                   job.ClientId,
                                   job.ClientName,
                                   Cury.Rmvalue,
                                   jobAddtl.Spocid
                               }).FirstOrDefault();

                if (opening != null)
                {
                    var clientCandidateShareProfiles = new List<ClientCandidateShareProfiles>();
                    var candidateResumeAttachments = new List<CandidateResumeAttachments>();
                    List<int> confViews = candidateShareProfileViewModel.ConfView?.Split(',').Select(int.Parse).ToList();
                    Guid g = Guid.NewGuid();
                    string BatchNo = g.ToString();

                    foreach (var item in candidateShareProfileViewModel.SelectedProfileViewModel)
                    {
                        var jobCandidate = (from cand in dbContext.PhJobCandidates
                                            join canStatus in dbContext.PhCandStatusSes on cand.CandProfStatus equals canStatus.Id
                                            where cand.CandProfId == item.CanPrfId && cand.Joid == candidateShareProfileViewModel.JobId
                                            select new
                                            {
                                                canStatus.Cscode,
                                                cand.RecruiterId
                                            }).FirstOrDefault();
                        item.RecruiterId = jobCandidate?.RecruiterId;
                        if (jobCandidate?.Cscode != "FCV")
                        {
                            message = " Candidate Should be in final cv status - " + item.CandidateName + "";
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                            return respModel;
                        }
                    }
                    var CandidateStatus = dbContext.PhCandStatusSes.Select(x => new { x.Cscode, x.Id }).Where(x => x.Cscode == CandidateStatusCodes.CSB.ToString()).FirstOrDefault();
                    foreach (var can in candidateShareProfileViewModel.SelectedProfileViewModel)
                    {
                        var canShare = new PhCandidateProfilesShared
                        {
                            CandProfId = can.CanPrfId,
                            BatchNo = BatchNo,
                            ClemailId = candidateShareProfileViewModel.ToEmailID,
                            ClientId = opening.ClientId,
                            Clname = opening.ClientName,
                            ClreviewStatus = (byte)ClreviewStatus.NotReviewd,
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            Joid = candidateShareProfileViewModel.JobId,
                            Status = (byte)RecordStatus.Active,
                            CcemailIds = candidateShareProfileViewModel.CCEmailIDs,
                            EmailSubject = candidateShareProfileViewModel.EmailSubject,
                            ConfView = candidateShareProfileViewModel.ConfView,
                            EmailFields = candidateShareProfileViewModel.EmailFields
                        };
                        dbContext.PhCandidateProfilesShareds.Add(canShare);
                        await dbContext.SaveChangesAsync();

                        var JobCandidates = await dbContext.PhJobCandidates.Where(x => x.Joid == candidateShareProfileViewModel.JobId
                        && x.CandProfId == can.CanPrfId).FirstOrDefaultAsync();
                        if (JobCandidates != null)
                        {
                            JobCandidates.UpdatedDate = CurrentTime;
                            JobCandidates.Cbcurrency = can.CbCurrency;
                            JobCandidates.CbperMonth = can.CbSalary;
                            dbContext.PhJobCandidates.Update(JobCandidates);
                            await dbContext.SaveChangesAsync();

                            var changeStatusViewModel = new ChangeStatusViewModel
                            {
                                ActionMode = (byte)WorkflowActionMode.Candidate,
                                CanProfId = can.CanPrfId,
                                CurrentStatusId = JobCandidates.CandProfStatus,
                                UpdatedStatusId = CandidateStatus.Id,
                                JobId = candidateShareProfileViewModel.JobId,
                                UserId = UserId,
                                Remarks = string.Empty
                            };
                            await ChangeStatusRules(changeStatusViewModel);
                        }

                        if (confViews.Contains((int)ConfigureView.ResumeAttachment))
                        {
                            var canDoc = dbContext.PhCandidateDocs.Where(x => x.CandProfId == canShare.CandProfId && x.Joid == canShare.Joid && x.DocType == "Final CV").OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                            if (canDoc != null)
                            {
                                if (!string.IsNullOrEmpty(canDoc.FileName))
                                {
                                    var candidateResumeAttachment = new CandidateResumeAttachments
                                    {
                                        CandName = can.CandidateName,
                                        CandProfId = canShare.CandProfId,
                                        JoId = canShare.Joid,
                                        FileType = canDoc.FileType,
                                        FileName = canDoc.FileName
                                    };

                                    string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + can.CanPrfId + "";

                                    var filePath = Path.Combine(webRootPath, string.Empty, candidateResumeAttachment.FileName);
                                    if ((System.IO.File.Exists(filePath)))
                                    {
                                        byte[] data = System.IO.File.ReadAllBytes(filePath);
                                        candidateResumeAttachment.Data = data;
                                        candidateResumeAttachments.Add(candidateResumeAttachment);
                                    }
                                }
                            }
                        }

                        var canProfile = dbContext.PhCandidateProfiles.Select(x => new { x.Id, x.ContactNo, x.EmailId }).Where(x => x.Id == can.CanPrfId).FirstOrDefault();

                        // Sharing to client 
                        clientCandidateShareProfiles.Add(new ClientCandidateShareProfiles
                        {
                            RecId = can.RecruiterId,
                            ResourceName = can.CandidateName,
                            CandProfId = canShare.CandProfId,
                            CanShareId = canShare.Id,
                            CurrLocation = can.City,
                            Country = can.Country,
                            cbCurrency = can.CbCurrency,
                            cbSalary = can.CbSalary,
                            MobileNumber = canProfile?.ContactNo,
                            Email = canProfile?.EmailId,
                            TotalExperiance = can.Experience == null ? " " : can.Experience + " Years",
                            NoticePeriod = can.NoticePeriod == null ? " " : can.NoticePeriod + " Days",
                            Review = this.appSettings.AppSettingsProperties.HireAppUrl + "/client-view/" + BatchNo
                        });
                    }

                    var openingStatus = dbContext.PhJobOpenings.Where(x => x.Id == candidateShareProfileViewModel.JobId).FirstOrDefault();
                    if (openingStatus != null)
                    {
                        openingStatus.JobOpeningStatus = (byte)JobStatusCodes.SUB;
                        openingStatus.UpdatedDate = CurrentTime;

                        dbContext.PhJobOpenings.Update(openingStatus);
                        await dbContext.SaveChangesAsync();
                    }

                    string userDispName = string.Empty; string userDesgn = string.Empty; string userEmailId = string.Empty; string userMobileNo = string.Empty;
                    if (Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        var userDetails = dbContext.PiHireUsers.Where(x => x.Id == UserId
                        && x.UserType != (byte)UserType.Candidate).FirstOrDefault();
                        if (userDetails != null)
                        {
                            userDispName = userDetails.FirstName + " " + userDetails.LastName;
                            userDesgn = userDetails.UserRoleName;
                            userEmailId = userDetails.EmailId;
                            userMobileNo = userDetails.MobileNumber;
                        }
                        else
                        {
                            userDispName = Usr.Name;
                            userDesgn = Usr.UserDetails.Designation;
                            userEmailId = Usr.UserDetails.Email;
                            userMobileNo = Usr.UserDetails.MobileNo;
                        }
                    }
                    else
                    {
                        userDispName = Usr.Name;
                        userDesgn = Usr.UserDetails.Designation;
                        userEmailId = Usr.UserDetails.Email;
                        userMobileNo = Usr.UserDetails.MobileNo;
                    }

                    var mailBody = EmailTemplates.Candidate_ProfilesToClient_Template(userDispName, userDesgn,
                        opening.JobTitle, opening.JobRole, opening.ClientName, candidateShareProfileViewModel.SpocName,
                        userMobileNo, clientCandidateShareProfiles, opening.Rmvalue, candidateShareProfileViewModel.EmailFields,
                        this.appSettings.AppSettingsProperties.HireAppUrl, this.appSettings.AppSettingsProperties.HireApiUrl, userEmailId);

                    string SmtpLoginName = appSettings.smtpEmailConfig.SmtpLoginName;
                    string SmtpLoginPassword = appSettings.smtpEmailConfig.SmtpLoginPassword;
                    var smtpConfiguration = dbContext.PhUsersConfigs.Where(x => x.UserId == UserId && x.VerifyFlag == true).FirstOrDefault();

                    var simpleEncrypt = new SimpleEncrypt();
                    if (smtpConfiguration != null)
                    {
                        SmtpLoginName = smtpConfiguration.UserName;
                        SmtpLoginPassword = simpleEncrypt.passwordDecrypt(smtpConfiguration.PasswordHash);
                    }

                    var smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                        SmtpLoginName, SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl, SmtpLoginName, appSettings.smtpEmailConfig.SmtpFromName);


                    smtp.SendMail(candidateShareProfileViewModel.ToEmailID, candidateShareProfileViewModel.EmailSubject,
                        mailBody, candidateShareProfileViewModel.CCEmailIDs, candidateResumeAttachments);

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = " Job is not available";
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


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> UpdateClientCandidatePreference(UpdateShareProfileTimeViewModel updateShareProfileTimeViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = " Updated Successfully";
            try
            {
                
                var CanShareDetls = dbContext.PhCandidateProfilesShareds.Where(x => x.Id == updateShareProfileTimeViewModel.CanShareId).FirstOrDefault();
                if (CanShareDetls != null)
                {
                    var jobCan = dbContext.PhJobCandidates.Where(x => x.Joid == CanShareDetls.Joid && x.CandProfId == CanShareDetls.CandProfId).FirstOrDefault();

                    CanShareDetls.ClreviewStatus = (byte)ClreviewStatus.Reviewed;
                    CanShareDetls.ModeOfInterview = updateShareProfileTimeViewModel.ModeofInterview;
                    CanShareDetls.InterviewTimeZone = updateShareProfileTimeViewModel.InterviewTimeZone;
                    CanShareDetls.InterviewDate = updateShareProfileTimeViewModel.InterviewDate;
                    CanShareDetls.StartTime = updateShareProfileTimeViewModel.InterviewStartTime;
                    CanShareDetls.EndTime = updateShareProfileTimeViewModel.InterviewEndTime;
                    CanShareDetls.ReviewedOn = CurrentTime;
                    CanShareDetls.UpdatedDate = CurrentTime;


                    dbContext.PhCandidateProfilesShareds.Update(CanShareDetls);
                    await dbContext.SaveChangesAsync();

                    var activityList = new List<CreateActivityViewModel>();
                    var audList = new List<CreateAuditViewModel>();

                    //Audit
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Interview time preferences",
                        ActivityDesc = " Candidate Interview time preferences",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = CanShareDetls.CandProfId,
                        UserId = jobCan.RecruiterId != null ? jobCan.RecruiterId.Value : 0
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    //Activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = CanShareDetls.CandProfId,
                        JobId = CanShareDetls.Joid,
                        ActivityType = (byte)LogActivityType.ScheduleInterviewUpdates,
                        ActivityDesc = " Candidate Interview time preferences",
                        UserId = jobCan.RecruiterId != null ? jobCan.RecruiterId.Value : 0
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    // Applying workflow rule 
                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    {
                        ActionMode = (byte)WorkflowActionMode.Other,
                        CanProfId = CanShareDetls.CandProfId,
                        JobId = CanShareDetls.Joid,
                        TaskCode = TaskCode.CST.ToString(),
                        UserId = jobCan.RecruiterId != null ? jobCan.RecruiterId.Value : 0
                    };

                    var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
                    if (wfResp.Status && wfResp.isNotification)
                    {
                        foreach (var item in wfResp.WFNotifications)
                        {
                            var notificationPushed = new NotificationPushedViewModel
                            {
                                JobId = wfResp.JoId,
                                PushedTo = item.UserIds,
                                NoteDesc = item.NoteDesc,
                                Title = item.Title,
                                CreatedBy = jobCan.RecruiterId != null ? jobCan.RecruiterId.Value : 0
                            };
                            notificationPushedViewModel.Add(notificationPushed);
                        }
                    }

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = " Candidate is not available";
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
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> ScheduleCandidateInterview(ScheduleCandidateInterview scheduleCandidateInterview)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = "Scheduled Successfully";
            try
            {
                

                var candidateProfile = await (from can in dbContext.PhCandidateProfiles
                                              join jobCan in dbContext.PhJobCandidates on can.Id equals jobCan.CandProfId
                                              where jobCan.Joid == scheduleCandidateInterview.JobId && jobCan.CandProfId == scheduleCandidateInterview.CanPrfId
                                              select new
                                              {
                                                  can.Id,
                                                  can.CandName,
                                                  can.EmailId,
                                                  jobCan.CandProfStatus,
                                                  jobCan.RecruiterId
                                              }).FirstOrDefaultAsync();

                if (candidateProfile != null)
                {
                    var canInterview = new PhJobCandidateInterview
                    {
                        CreatedDate = CurrentTime,
                        InterviewDate = scheduleCandidateInterview.InterviewDate,
                        CandProfId = scheduleCandidateInterview.CanPrfId,
                        InterviewDuration = scheduleCandidateInterview.InterviewDuration,
                        InterviewerEmail = scheduleCandidateInterview.ClientPannel,
                        InterviewStatus = (byte)InterviewStatus.Active,
                        InterviewStartTime = scheduleCandidateInterview.InterviewStartTime,
                        InterviewEndTime = scheduleCandidateInterview.InterviewEndTime,
                        Joid = scheduleCandidateInterview.JobId,
                        Location = scheduleCandidateInterview.Location,
                        ModeOfInterview = scheduleCandidateInterview.ModeofInterview,
                        Remarks = scheduleCandidateInterview.Remarks,
                        ScheduledBy = scheduleCandidateInterview.ScheduledBy,
                        Status = (byte)RecordStatus.Active,
                        CreatedBy = scheduleCandidateInterview.UserId,
                        PiTeamEmailIds = scheduleCandidateInterview.HirePannel,
                        InterviewTimeZone = scheduleCandidateInterview.InterviewTimeZone,
                    };

                    dbContext.PhJobCandidateInterviews.Add(canInterview);
                    await dbContext.SaveChangesAsync();


                    var documents = await (from canDoc in dbContext.PhCandidateDocs
                                           where canDoc.CandProfId == scheduleCandidateInterview.CanPrfId
                                           && canDoc.Joid == scheduleCandidateInterview.JobId
                                           && canDoc.DocType == "Final CV"
                                           && canDoc.FileGroup == (byte)FileGroup.Profile
                                           && canDoc.Status != (byte)RecordStatus.Delete
                                           select new CandidateFilesViewModel
                                           {
                                               FileName = canDoc.FileName,
                                               CandProfId = canDoc.CandProfId,
                                               Id = canDoc.Id,
                                               DocType = canDoc.DocType,
                                               FileGroup = canDoc.FileGroup,
                                               DocStatus = canDoc.DocStatus,
                                               Remarks = canDoc.Remerks,
                                               CreatedDate = canDoc.CreatedDate,
                                               UploadedFromDrive = false
                                           }).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                    if (documents != null)
                    {
                        if (!string.IsNullOrEmpty(documents.FileName))
                        {
                            if (ValidHttpURL(documents.FileName))
                            {
                                documents.FilePath = documents.FileName;
                                documents.UploadedFromDrive = true;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(documents.FileName))
                                {
                                    documents.FileName = documents.FileName.Replace("#", "%23");
                                    documents.FilePath = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + documents.CandProfId + "/" + documents.FileName;
                                }
                            }
                        }
                    }

                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        // Day Wise Actions 
                        var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == scheduleCandidateInterview.JobId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                        if (IsDayWiseAction == null)
                        {
                            IsDayWiseAction = new PhDayWiseJobAction
                            {
                                CreatedBy = scheduleCandidateInterview.UserId,
                                CreatedDate = CurrentTime,
                                Joid = scheduleCandidateInterview.JobId,
                                Interviews = true
                            };
                            dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            IsDayWiseAction.Interviews = true;
                            dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                    }


                    var mode = (ModeOfInterview)scheduleCandidateInterview.ModeofInterview;
                    if (mode == ModeOfInterview.Google || mode == ModeOfInterview.Microsoft)
                    {
                        var jobTitle = await dbContext.PhJobOpenings.Where(da => da.Id == canInterview.Joid).Select(da => da.JobTitle).FirstOrDefaultAsync();

                        string subject = (canInterview.Joid + " - " + jobTitle) ?? "piHire Interview", body = canInterview.Remarks, loc = canInterview.Location;
                        DateTime strt = canInterview.InterviewDate,
                            end = canInterview.InterviewDate;
                        if (!string.IsNullOrEmpty(canInterview.InterviewStartTime))
                        {
                            var tm = canInterview.InterviewStartTime.Trim().Split(' ')[0].Split(':');
                            strt = strt.AddHours(Convert.ToInt32(tm[0]))
                                .AddMinutes(Convert.ToInt32(tm[1]));
                            if (canInterview.InterviewStartTime.Trim().Split(' ')[1].Equals("pm", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (Convert.ToInt32(tm[0]) < 12)
                                {
                                    strt = strt.AddHours(12);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(canInterview.InterviewEndTime))
                        {
                            var tm = canInterview.InterviewEndTime.Trim().Split(' ')[0].Split(':');
                            end = end.AddHours(Convert.ToInt32(tm[0]))
                                .AddMinutes(Convert.ToInt32(tm[1]));
                            if (canInterview.InterviewEndTime.Trim().Split(' ')[1].Equals("pm", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (Convert.ToInt32(tm[0]) < 12)
                                {
                                    end = end.AddHours(12);
                                }
                            }
                        }
                        List<Common.Meeting.MeetingAttendeeEmailAddressViewModel> attend =
                            new List<Common.Meeting.MeetingAttendeeEmailAddressViewModel>();
                        if (!string.IsNullOrEmpty(canInterview.InterviewerEmail))
                            foreach (var Interviewer in canInterview.InterviewerEmail.Split(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                attend.Add(new Common.Meeting.MeetingAttendeeEmailAddressViewModel
                                {
                                    address = Interviewer,
                                    type = Common.Meeting.MeetingAttendeeType.required,
                                    name = string.Empty
                                });
                            }
                        if (!string.IsNullOrEmpty(canInterview.PiTeamEmailIds))
                            foreach (var PiTeamEmailId in canInterview.PiTeamEmailIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                attend.Add(new Common.Meeting.MeetingAttendeeEmailAddressViewModel
                                {
                                    address = PiTeamEmailId,
                                    type = Common.Meeting.MeetingAttendeeType.optional,
                                    name = string.Empty
                                });
                            }
                        attend.Add(new Common.Meeting.MeetingAttendeeEmailAddressViewModel
                        {
                            address = candidateProfile.EmailId,
                            type = Common.Meeting.MeetingAttendeeType.required,
                            name = candidateProfile.CandName
                        });
                        if (mode == ModeOfInterview.Google)
                        {
                            var tkn = await dbContext.PhIntegrationsSes.Where(da => da.Status == (byte)RecordStatus.Active && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.GoogleMeet)
                                .Select(da => da.RefreshToken).FirstOrDefaultAsync();
                            if (string.IsNullOrEmpty(tkn))
                            {
                                throw new Exception("Google token not exist/not valid");
                            }
                            Common.Meeting.GoogleMeet met = new Common.Meeting.GoogleMeet(tkn, logger);

                            var resp = await met.CreateEvent(subject, body + " <br> Please find the resume : <a href=" + documents.FilePath + "> Click here</a>", loc, strt, end, attend, scheduleCandidateInterview.InterviewTimeZone);

                            logger.Log(LogLevel.Information, LoggingEvents.Other, "Create google schedule event:" + Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                            canInterview.CalendarEventId = resp.eventId;
                            if (string.IsNullOrWhiteSpace(canInterview.CalendarEventId))
                                throw new Exception(resp.msg);
                        }
                        else
                        if (mode == ModeOfInterview.Microsoft)
                        {
                            var tknObj = await dbContext.PhIntegrationsSes.Where(da => da.Status == (byte)RecordStatus.Active && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.Office365)
                                   .Select(da => new { da.RefreshToken, da.ReDirectUrl }).FirstOrDefaultAsync();
                            if (tknObj == null)
                            {
                                throw new Exception("Microsoft token not exist/not valid");
                            }
                            Common.Meeting.Teams met = new Common.Meeting.Teams(tknObj.RefreshToken, tknObj.ReDirectUrl, logger);
                            var resp = await met.CreateEvent(subject, body + " <br> Please find the resume : <a href=" + documents.FilePath + "> Click here</a>", loc, strt, end, false, attend, scheduleCandidateInterview.InterviewTimeZone);
                            logger.Log(LogLevel.Information, LoggingEvents.Other, "Create Microsoft schedule event:" + Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                            canInterview.CalendarEventId = resp.eventId;
                            if (string.IsNullOrWhiteSpace(canInterview.CalendarEventId))
                                throw new Exception(resp.msg);
                        }
                        await dbContext.SaveChangesAsync();
                    }

                    string Interviewmode = Enum.GetName(typeof(ModeOfInterview), scheduleCandidateInterview.ModeofInterview);

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    //Audit
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Schedule Candidate Interview",
                        ActivityDesc = " Schedule " + Interviewmode + " Interview for " + candidateProfile.CandName + "",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = scheduleCandidateInterview.CanPrfId,
                        UserId = candidateProfile.RecruiterId != null ? candidateProfile.RecruiterId.Value : 0
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    //Activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = scheduleCandidateInterview.CanPrfId,
                        JobId = scheduleCandidateInterview.JobId,
                        ActivityType = (byte)LogActivityType.ScheduleInterviewUpdates,
                        ActivityDesc = " Scheduled " + Interviewmode + " Interview for " + candidateProfile.CandName + "",
                        UserId = candidateProfile.RecruiterId != null ? candidateProfile.RecruiterId.Value : 0
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    if (scheduleCandidateInterview.ScheduledBy == (byte)ScheduledBy.Client)
                    {
                        // Applying work flow conditions 
                        var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                        {
                            ActionMode = (byte)WorkflowActionMode.Candidate,
                            CanProfId = scheduleCandidateInterview.CanPrfId,
                            CurrentStatusId = candidateProfile.CandProfStatus,
                            JobId = scheduleCandidateInterview.JobId,
                            UserId = candidateProfile.RecruiterId != null ? candidateProfile.RecruiterId.Value : 0,
                            TaskCode = TaskCode.SCI.ToString(),
                            LocationId = scheduleCandidateInterview.LocationId
                        };

                        var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
                        if (wfResp.Status && wfResp.isNotification)
                        {
                            foreach (var item in wfResp.WFNotifications)
                            {
                                var notificationPushed = new NotificationPushedViewModel
                                {
                                    JobId = wfResp.JoId,
                                    PushedTo = item.UserIds,
                                    NoteDesc = item.NoteDesc,
                                    Title = item.Title,
                                    CreatedBy = candidateProfile.RecruiterId != null ? candidateProfile.RecruiterId.Value : 0
                                };
                                notificationPushedViewModel.Add(notificationPushed);
                            }
                        }
                    }

                    if (scheduleCandidateInterview.ScheduledBy == (byte)ScheduledBy.PiTeam && mode == ModeOfInterview.PiF2F)
                    {
                        // Applying work flow conditions 
                        var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                        {
                            ActionMode = (byte)WorkflowActionMode.Candidate,
                            CanProfId = scheduleCandidateInterview.CanPrfId,
                            CurrentStatusId = candidateProfile.CandProfStatus,
                            JobId = scheduleCandidateInterview.JobId,
                            UserId = candidateProfile.RecruiterId != null ? candidateProfile.RecruiterId.Value : 0,
                            TaskCode = TaskCode.PFF.ToString(),
                            LocationId = scheduleCandidateInterview.LocationId
                        };

                        var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
                        if (wfResp.Status && wfResp.isNotification)
                        {
                            foreach (var item in wfResp.WFNotifications)
                            {
                                var notificationPushed = new NotificationPushedViewModel
                                {
                                    JobId = wfResp.JoId,
                                    PushedTo = item.UserIds,
                                    NoteDesc = item.NoteDesc,
                                    Title = item.Title,
                                    CreatedBy = candidateProfile.RecruiterId != null ? candidateProfile.RecruiterId.Value : 0
                                };
                                notificationPushedViewModel.Add(notificationPushed);
                            }
                        }
                    }


                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = "Candidate is not available";
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
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ReScheduleCandidateInterview(ReScheduleScheduleCandidateInterview reScheduleScheduleCandidateInterview)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = " Rescheduled Successfully";
            int UserId = Usr.Id;
            try
            {
                
                var JobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == reScheduleScheduleCandidateInterview.JobId
                && x.CandProfId == reScheduleScheduleCandidateInterview.CanPrfId).Select(x => new { x.Joid }).FirstOrDefaultAsync();
                if (JobCandidate != null)
                {
                    var Candidate = await dbContext.PhCandidateProfiles.Where(x => x.Id == reScheduleScheduleCandidateInterview.CanPrfId).Select(x => new { x.CandName, x.EmailId }).FirstOrDefaultAsync();
                    var JobCandidateInterviews = dbContext.PhJobCandidateInterviews.Where(x => x.Joid == reScheduleScheduleCandidateInterview.JobId
                            && x.CandProfId == reScheduleScheduleCandidateInterview.CanPrfId && x.Id == reScheduleScheduleCandidateInterview.InterviewId).FirstOrDefault();
                    ModeOfInterview? oldModeOfInterview = null;
                    if (JobCandidateInterviews != null)
                    {
                        oldModeOfInterview = (ModeOfInterview)JobCandidateInterviews?.ModeOfInterview;
                        JobCandidateInterviews.UpdatedBy = UserId;
                        JobCandidateInterviews.UpdatedDate = CurrentTime;
                        JobCandidateInterviews.InterviewStatus = (byte)InterviewStatus.Rescheduled;
                        JobCandidateInterviews.InterviewDuration = reScheduleScheduleCandidateInterview.InterviewDuration;
                        JobCandidateInterviews.InterviewerEmail = reScheduleScheduleCandidateInterview.ClientPannel;
                        JobCandidateInterviews.PiTeamEmailIds = reScheduleScheduleCandidateInterview.HirePannel;
                        JobCandidateInterviews.InterviewTimeZone = reScheduleScheduleCandidateInterview.InterviewTimeZone;
                        JobCandidateInterviews.InterviewDate = reScheduleScheduleCandidateInterview.InterviewDate;
                        JobCandidateInterviews.InterviewStartTime = reScheduleScheduleCandidateInterview.InterviewStartTime;
                        JobCandidateInterviews.InterviewEndTime = reScheduleScheduleCandidateInterview.InterviewEndTime;
                        JobCandidateInterviews.Location = reScheduleScheduleCandidateInterview.Location;
                        JobCandidateInterviews.ModeOfInterview = reScheduleScheduleCandidateInterview.ModeofInterview;
                        JobCandidateInterviews.Remarks = reScheduleScheduleCandidateInterview.Remarks;

                        dbContext.PhJobCandidateInterviews.Update(JobCandidateInterviews);
                    }

                    await dbContext.SaveChangesAsync();

                    var documents = await (from canDoc in dbContext.PhCandidateDocs
                                           where canDoc.CandProfId == reScheduleScheduleCandidateInterview.CanPrfId
                                           && canDoc.Joid == reScheduleScheduleCandidateInterview.JobId
                                           && canDoc.DocType == "Final CV"
                                           && canDoc.FileGroup == (byte)FileGroup.Profile
                                           && canDoc.Status != (byte)RecordStatus.Delete
                                           select new CandidateFilesViewModel
                                           {
                                               FileName = canDoc.FileName,
                                               CandProfId = canDoc.CandProfId,
                                               Id = canDoc.Id,
                                               DocType = canDoc.DocType,
                                               FileGroup = canDoc.FileGroup,
                                               DocStatus = canDoc.DocStatus,
                                               Remarks = canDoc.Remerks,
                                               CreatedDate = canDoc.CreatedDate,
                                               UploadedFromDrive = false
                                           }).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                    if (documents != null)
                    {
                        if (!string.IsNullOrEmpty(documents.FileName))
                        {
                            if (ValidHttpURL(documents.FileName))
                            {
                                documents.FilePath = documents.FileName;
                                documents.UploadedFromDrive = true;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(documents.FileName))
                                {
                                    documents.FileName = documents.FileName.Replace("#", "%23");
                                    documents.FilePath = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + documents.CandProfId + "/" + documents.FileName;
                                }
                            }
                        }
                    }

                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        // Day Wise Actions 
                        var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == reScheduleScheduleCandidateInterview.JobId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                        if (IsDayWiseAction == null)
                        {
                            IsDayWiseAction = new PhDayWiseJobAction
                            {
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                Joid = reScheduleScheduleCandidateInterview.JobId,
                                Interviews = true
                            };
                            dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            IsDayWiseAction.Interviews = true;
                            dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                    }


                    var newModeOfInterview = (ModeOfInterview)reScheduleScheduleCandidateInterview.ModeofInterview;
                    if ((oldModeOfInterview == ModeOfInterview.Google || oldModeOfInterview == ModeOfInterview.Microsoft) && oldModeOfInterview != newModeOfInterview)
                    {
                        if (string.IsNullOrEmpty(JobCandidateInterviews.CalendarEventId))
                            logger.Log(LogLevel.Warning, LoggingEvents.MandatoryDataMissing, "CalendarEventId is missing");
                        else
                        {
                            logger.Log(LogLevel.Information, LoggingEvents.Other, "Mode of interview changed from " + oldModeOfInterview + " to " + newModeOfInterview + ". so starting cancellation of old event. eventId:" + JobCandidateInterviews.CalendarEventId);
                            if (oldModeOfInterview == ModeOfInterview.Google)
                            {
                                var tkn = await dbContext.PhIntegrationsSes.Where(da => da.Status == (byte)RecordStatus.Active && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.GoogleMeet)
                                    .Select(da => da.RefreshToken).FirstOrDefaultAsync();
                                if (string.IsNullOrEmpty(tkn))
                                {
                                    throw new Exception("Google token not exist/not valid");
                                }
                                Common.Meeting.GoogleMeet met = new Common.Meeting.GoogleMeet(tkn, logger);
                                var resp = await met.DeleteEvent(JobCandidateInterviews.CalendarEventId);
                                logger.Log(LogLevel.Information, LoggingEvents.Other, "Cancelled google schedule event:" + Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                            }
                            else
                            if (oldModeOfInterview == ModeOfInterview.Microsoft)
                            {
                                var tknObj = await dbContext.PhIntegrationsSes.Where(da => da.Status == (byte)RecordStatus.Active && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.Office365)
                                       .Select(da => new { da.RefreshToken, da.ReDirectUrl }).FirstOrDefaultAsync();
                                if (tknObj == null)
                                {
                                    throw new Exception(" Microsoft token not exist/not valid");
                                }
                                Common.Meeting.Teams met = new Common.Meeting.Teams(tknObj.RefreshToken, tknObj.ReDirectUrl, logger);
                                var resp = await met.DeleteEvent(JobCandidateInterviews.CalendarEventId);
                                logger.Log(LogLevel.Information, LoggingEvents.Other, "Cancelled Microsoft schedule event:" + Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                            }
                            JobCandidateInterviews.CalendarEventId = string.Empty;
                            logger.Log(LogLevel.Information, LoggingEvents.Other, "Cancelled schedule event done");
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    if (newModeOfInterview == ModeOfInterview.Google || newModeOfInterview == ModeOfInterview.Microsoft)
                    {
                        var jobTitle = await dbContext.PhJobOpenings.Where(da => da.Id == reScheduleScheduleCandidateInterview.JobId).Select(da => da.JobTitle).FirstOrDefaultAsync();

                        string subject = (reScheduleScheduleCandidateInterview.JobId + " - " + jobTitle) ?? "piHire Interview", body = reScheduleScheduleCandidateInterview.Remarks, loc = reScheduleScheduleCandidateInterview.Location;
                        DateTime strt = reScheduleScheduleCandidateInterview.InterviewDate,
                            end = reScheduleScheduleCandidateInterview.InterviewDate;
                        if (!string.IsNullOrEmpty(reScheduleScheduleCandidateInterview.InterviewStartTime))
                        {
                            var tm = reScheduleScheduleCandidateInterview.InterviewStartTime.Trim().Split(' ')[0].Split(':');
                            strt = strt.AddHours(Convert.ToInt32(tm[0]))
                                .AddMinutes(Convert.ToInt32(tm[1]));
                            if (reScheduleScheduleCandidateInterview.InterviewStartTime.Trim().Split(' ')[1].Equals("pm", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (Convert.ToInt32(tm[0]) < 12)
                                {
                                    strt = strt.AddHours(12);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(reScheduleScheduleCandidateInterview.InterviewEndTime))
                        {
                            var tm = reScheduleScheduleCandidateInterview.InterviewEndTime.Trim().Split(' ')[0].Split(':');
                            end = end.AddHours(Convert.ToInt32(tm[0]))
                                .AddMinutes(Convert.ToInt32(tm[1]));
                            if (reScheduleScheduleCandidateInterview.InterviewEndTime.Trim().Split(' ')[1].Equals("pm", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (Convert.ToInt32(tm[0]) < 12)
                                {
                                    end = end.AddHours(12);
                                }
                            }
                        }
                        List<Common.Meeting.MeetingAttendeeEmailAddressViewModel> attend =
                            new List<Common.Meeting.MeetingAttendeeEmailAddressViewModel>();
                        if (!string.IsNullOrEmpty(reScheduleScheduleCandidateInterview.ClientPannel))
                            foreach (var Interviewer in reScheduleScheduleCandidateInterview.ClientPannel.Split(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                attend.Add(new Common.Meeting.MeetingAttendeeEmailAddressViewModel
                                {
                                    address = Interviewer,
                                    type = Common.Meeting.MeetingAttendeeType.required,
                                    name = ""
                                });
                            }
                        if (!string.IsNullOrEmpty(reScheduleScheduleCandidateInterview.HirePannel))
                            foreach (var PiTeamEmailId in reScheduleScheduleCandidateInterview.HirePannel.Split(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                attend.Add(new Common.Meeting.MeetingAttendeeEmailAddressViewModel
                                {
                                    address = PiTeamEmailId,
                                    type = Common.Meeting.MeetingAttendeeType.optional,
                                    name = ""
                                });
                            }
                        attend.Add(new Common.Meeting.MeetingAttendeeEmailAddressViewModel
                        {
                            address = Candidate.EmailId,
                            type = Common.Meeting.MeetingAttendeeType.required,
                            name = Candidate.EmailId
                        });
                        if (newModeOfInterview == ModeOfInterview.Google)
                        {
                            var tkn = await dbContext.PhIntegrationsSes.Where(da => da.Status == (byte)RecordStatus.Active && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.GoogleMeet)
                                .Select(da => da.RefreshToken).FirstOrDefaultAsync();
                            if (string.IsNullOrEmpty(tkn))
                            {
                                throw new Exception("Google token not exist/not valid");
                            }
                            Common.Meeting.GoogleMeet met = new Common.Meeting.GoogleMeet(tkn, logger);
                            if (string.IsNullOrEmpty(JobCandidateInterviews.CalendarEventId))
                            {
                                var resp = await met.CreateEvent(subject, body + " <br> Please find the resume : <a href=" + documents.FilePath + "> Click here</a>", loc, strt, end, attend, reScheduleScheduleCandidateInterview.InterviewTimeZone);
                                logger.Log(LogLevel.Information, LoggingEvents.Other, "Create google schedule event:" + Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                                JobCandidateInterviews.CalendarEventId = resp.eventId;
                                if (string.IsNullOrWhiteSpace(JobCandidateInterviews.CalendarEventId))
                                    throw new Exception(resp.msg);
                            }
                            else
                            {
                                var resp = await met.UpdateEvent(JobCandidateInterviews.CalendarEventId, subject, body + " <br> Please find the resume : <a href=" + documents.FilePath + "> Click here</a>", loc, strt, end, attend, reScheduleScheduleCandidateInterview.InterviewTimeZone);
                                logger.Log(LogLevel.Information, LoggingEvents.Other, "Update google schedule event:" + Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                                JobCandidateInterviews.CalendarEventId = resp.eventId;
                                if (string.IsNullOrWhiteSpace(JobCandidateInterviews.CalendarEventId))
                                    throw new Exception(resp.msg);
                            }
                        }
                        else
                        if (newModeOfInterview == ModeOfInterview.Microsoft)
                        {
                            var tknObj = await dbContext.PhIntegrationsSes.Where(da => da.Status == (byte)RecordStatus.Active && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.Office365)
                                   .Select(da => new { da.RefreshToken, da.ReDirectUrl }).FirstOrDefaultAsync();
                            if (tknObj == null)
                            {
                                throw new Exception(" Microsoft token not exist/not valid");
                            }
                            Common.Meeting.Teams met = new Common.Meeting.Teams(tknObj.RefreshToken, tknObj.ReDirectUrl, logger);

                            if (string.IsNullOrEmpty(JobCandidateInterviews.CalendarEventId))
                            {
                                //var resp = await met.CreateEvent(subject, body, loc, strt, end, false, attend, reScheduleScheduleCandidateInterview.InterviewTimeZone);
                                var resp = await met.CreateEvent(subject, body + " <br> Please find the resume : <a href=" + documents.FilePath + "> Click here</a>", loc, strt, end, false, attend, reScheduleScheduleCandidateInterview.InterviewTimeZone);
                                logger.Log(LogLevel.Information, LoggingEvents.Other, "Create microsoft schedule event:" + Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                                JobCandidateInterviews.CalendarEventId = resp.eventId;
                                if (string.IsNullOrWhiteSpace(JobCandidateInterviews.CalendarEventId))
                                    throw new Exception(resp.msg);
                            }
                            else
                            {
                                var resp = await met.UpdateEvent(JobCandidateInterviews.CalendarEventId, subject, body + " <br> Please find the resume : <a href=" + documents.FilePath + "> Click here</a>", loc, strt, end, false, attend, reScheduleScheduleCandidateInterview.InterviewTimeZone);
                                logger.Log(LogLevel.Information, LoggingEvents.Other, "Update microsoft schedule event:" + Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                                JobCandidateInterviews.CalendarEventId = resp.eventId;
                                if (string.IsNullOrWhiteSpace(JobCandidateInterviews.CalendarEventId))
                                    throw new Exception(resp.msg);
                            }
                        }
                        await dbContext.SaveChangesAsync();
                    }

                    // Applying workflow rule 
                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    {
                        ActionMode = (byte)WorkflowActionMode.Other,
                        CanProfId = reScheduleScheduleCandidateInterview.CanPrfId,
                        JobId = reScheduleScheduleCandidateInterview.JobId,
                        TaskCode = TaskCode.RSI.ToString(),
                        UserId = UserId
                    };

                    var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
                    if (wfResp.Status && wfResp.isNotification)
                    {
                        foreach (var item in wfResp.WFNotifications)
                        {
                            var notificationPushed = new NotificationPushedViewModel
                            {
                                JobId = wfResp.JoId,
                                PushedTo = item.UserIds,
                                NoteDesc = item.NoteDesc,
                                Title = item.Title,
                                CreatedBy = UserId
                            };
                            notificationPushedViewModel.Add(notificationPushed);
                        }
                    }

                    //Activity
                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = reScheduleScheduleCandidateInterview.CanPrfId,
                        JobId = reScheduleScheduleCandidateInterview.JobId,
                        ActivityType = (byte)LogActivityType.ScheduleInterviewUpdates,
                        ActivityDesc = "  Rescheduled Interview for " + Candidate.CandName + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = " Candidate is not available";
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
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> RejectCandidateInterview(CandidateInterviewRejectModel candidateInterviewRejectModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = "Rejected Successfully";
            try
            {
                

                var JobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == candidateInterviewRejectModel.JobId && x.CandProfId == candidateInterviewRejectModel.CanPrfId).FirstOrDefaultAsync();
                if (JobCandidate != null)
                {
                    var candidateName = await dbContext.PhCandidateProfiles.Where(x => x.Id == candidateInterviewRejectModel.CanPrfId).Select(x => x.CandName).FirstOrDefaultAsync();

                    if (candidateInterviewRejectModel.ScheduledBy == (byte)ScheduledBy.Client)
                    {
                        var CanShareDetls = dbContext.PhCandidateProfilesShareds.Where(x => x.Id == candidateInterviewRejectModel.CanShareId).FirstOrDefault();
                        if (CanShareDetls != null)
                        {
                            CanShareDetls.UpdatedDate = CurrentTime;
                            CanShareDetls.ReviewedOn = CurrentTime;
                            CanShareDetls.UpdatedBy = candidateInterviewRejectModel.UserId;
                            CanShareDetls.ClreviewStatus = (byte)ClreviewStatus.Rejected;
                            CanShareDetls.Remarks = candidateInterviewRejectModel.Remarks;
                            CanShareDetls.Reasons = candidateInterviewRejectModel.Reasons;

                            dbContext.PhCandidateProfilesShareds.Update(CanShareDetls);
                            await dbContext.SaveChangesAsync();
                        }

                        var JobCandidateInterviews = dbContext.PhJobCandidateInterviews.Where(x => x.Joid == candidateInterviewRejectModel.JobId && x.CandProfId == candidateInterviewRejectModel.CanPrfId && x.ScheduledBy == (byte)ScheduledBy.Client).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                        if (JobCandidateInterviews != null)
                        {
                            JobCandidateInterviews.UpdatedDate = CurrentTime;
                            JobCandidateInterviews.UpdatedBy = candidateInterviewRejectModel.UserId;
                            JobCandidateInterviews.InterviewStatus = (byte)InterviewStatus.Rejected;
                            JobCandidateInterviews.Remarks = candidateInterviewRejectModel.Remarks;

                            dbContext.PhJobCandidateInterviews.Update(JobCandidateInterviews);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        var JobCandidateInterviews = dbContext.PhJobCandidateInterviews.Where(x => x.Joid == candidateInterviewRejectModel.JobId && x.CandProfId == candidateInterviewRejectModel.CanPrfId && x.Id == candidateInterviewRejectModel.InterviewId).FirstOrDefault();
                        if (JobCandidateInterviews != null)
                        {
                            JobCandidateInterviews.UpdatedDate = CurrentTime;
                            JobCandidateInterviews.UpdatedBy = candidateInterviewRejectModel.UserId;
                            JobCandidateInterviews.InterviewStatus = (byte)InterviewStatus.Rejected;
                            JobCandidateInterviews.Remarks = candidateInterviewRejectModel.Remarks;

                            dbContext.PhJobCandidateInterviews.Update(JobCandidateInterviews);

                            await dbContext.SaveChangesAsync();
                        }
                    }

                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        // Day Wise Actions 
                        var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == candidateInterviewRejectModel.JobId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                        if (IsDayWiseAction == null)
                        {
                            IsDayWiseAction = new PhDayWiseJobAction
                            {
                                CreatedBy = candidateInterviewRejectModel.UserId,
                                CreatedDate = CurrentTime,
                                Joid = candidateInterviewRejectModel.JobId,
                                Interviews = true
                            };
                            dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            IsDayWiseAction.Interviews = true;
                            dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                    }


                    //Activity
                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = candidateInterviewRejectModel.CanPrfId,
                        JobId = candidateInterviewRejectModel.JobId,
                        ActivityType = (byte)LogActivityType.ScheduleInterviewUpdates,
                        ActivityDesc = " " + candidateName + " is Rejected by Client",
                        UserId = candidateInterviewRejectModel.UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    // Applying work flow conditions 
                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    {
                        ActionMode = (byte)WorkflowActionMode.Candidate,
                        CanProfId = JobCandidate.CandProfId,
                        CurrentStatusId = JobCandidate.CandProfStatus,
                        JobId = JobCandidate.Joid,
                        UserId = JobCandidate.CreatedBy,
                        TaskCode = TaskCode.CRU.ToString()
                    };
                    var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
                    if (wfResp.Status && wfResp.isNotification)
                    {
                        foreach (var item in wfResp.WFNotifications)
                        {
                            var notificationPushed = new NotificationPushedViewModel
                            {
                                JobId = wfResp.JoId,
                                PushedTo = item.UserIds,
                                NoteDesc = item.NoteDesc,
                                Title = item.Title,
                                CreatedBy = JobCandidate.CreatedBy
                            };
                            notificationPushedViewModel.Add(notificationPushed);
                        }
                    }

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = "Candidate is not available";
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
            return Tuple.Create(notificationPushedViewModel, respModel);
        }

        public async Task<UpdateResponseViewModel<string>> CancelInterviewInvitation(CancelCandidateInterviewInterview cancelCandidateInterviewInterview)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = " Cancelled the interview invitation Successfully";
            int UserId = Usr.Id;
            try
            {
                

                var JobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == cancelCandidateInterviewInterview.JobId
                && x.CandProfId == cancelCandidateInterviewInterview.CanPrfId).Select(x => new { x.Joid }).FirstOrDefaultAsync();

                if (JobCandidate != null)
                {
                    var Candidate = await dbContext.PhCandidateProfiles.Where(x => x.Id == cancelCandidateInterviewInterview.CanPrfId).Select(x => new { x.CandName, x.EmailId }).FirstOrDefaultAsync();

                    var JobCandidateInterviews = dbContext.PhJobCandidateInterviews.Where(x => x.Joid == cancelCandidateInterviewInterview.JobId
                            && x.CandProfId == cancelCandidateInterviewInterview.CanPrfId && x.Id == cancelCandidateInterviewInterview.InterviewId).FirstOrDefault();

                    ModeOfInterview? oldModeOfInterview = null;
                    if (JobCandidateInterviews != null)
                    {
                        oldModeOfInterview = (ModeOfInterview)JobCandidateInterviews?.ModeOfInterview;
                        JobCandidateInterviews.UpdatedBy = UserId;
                        JobCandidateInterviews.UpdatedDate = CurrentTime;
                        JobCandidateInterviews.InterviewStatus = (byte)InterviewStatus.Canceled;
                        if (!string.IsNullOrEmpty(cancelCandidateInterviewInterview.Remarks))
                        {
                            JobCandidateInterviews.Remarks = cancelCandidateInterviewInterview.Remarks;
                        }
                        dbContext.PhJobCandidateInterviews.Update(JobCandidateInterviews);
                        await dbContext.SaveChangesAsync();
                    }

                    if ((oldModeOfInterview == ModeOfInterview.Google || oldModeOfInterview == ModeOfInterview.Microsoft))
                    {
                        if (string.IsNullOrEmpty(JobCandidateInterviews.CalendarEventId))
                            logger.Log(LogLevel.Warning, LoggingEvents.MandatoryDataMissing, "CalendarEventId is missing");
                        else
                        {
                            if (oldModeOfInterview == ModeOfInterview.Google)
                            {
                                var tkn = await dbContext.PhIntegrationsSes.Where(da => da.Status == (byte)RecordStatus.Active && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.GoogleMeet)
                                    .Select(da => da.RefreshToken).FirstOrDefaultAsync();
                                if (string.IsNullOrEmpty(tkn))
                                {
                                    throw new Exception("Google token not exist/not valid");
                                }
                                Common.Meeting.GoogleMeet met = new Common.Meeting.GoogleMeet(tkn, logger);
                                var resp = await met.DeleteEvent(JobCandidateInterviews.CalendarEventId);
                                logger.Log(LogLevel.Information, LoggingEvents.Other, "Cancelled google schedule event:" + Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                            }
                            else
                            if (oldModeOfInterview == ModeOfInterview.Microsoft)
                            {
                                var tknObj = await dbContext.PhIntegrationsSes.Where(da => da.Status == (byte)RecordStatus.Active && da.Category == (byte)IntegrationCategory.Calenders && da.SubCategory == (byte)IntegrationCalendersCategory.Office365)
                                       .Select(da => new { da.RefreshToken, da.ReDirectUrl }).FirstOrDefaultAsync();
                                if (tknObj == null)
                                {
                                    throw new Exception(" Microsoft token not exist/not valid");
                                }
                                Common.Meeting.Teams met = new Common.Meeting.Teams(tknObj.RefreshToken, tknObj.ReDirectUrl, logger);
                                var resp = await met.DeleteEvent(JobCandidateInterviews.CalendarEventId);
                                logger.Log(LogLevel.Information, LoggingEvents.Other, "Cancelled Microsoft schedule event:" + Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                            }
                            JobCandidateInterviews.CalendarEventId = string.Empty;
                            logger.Log(LogLevel.Information, LoggingEvents.Other, "Cancelled schedule event done");
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        // Day Wise Actions 
                        var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == cancelCandidateInterviewInterview.JobId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                        if (IsDayWiseAction == null)
                        {
                            IsDayWiseAction = new PhDayWiseJobAction
                            {
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                Joid = cancelCandidateInterviewInterview.JobId,
                                Interviews = true
                            };
                            dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            IsDayWiseAction.Interviews = true;
                            dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                    }



                    //Activity
                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = cancelCandidateInterviewInterview.CanPrfId,
                        JobId = cancelCandidateInterviewInterview.JobId,
                        ActivityType = (byte)LogActivityType.ScheduleInterviewUpdates,
                        ActivityDesc = "  Canceled " + oldModeOfInterview + " Interview Invitation  for " + Candidate.CandName + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = " Candidate is not available";
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

        public async Task<UpdateResponseViewModel<string>> UpdateCandidateInterview(UpdateCandidateInterviewModel updateCandidateInterviewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new NotificationPushedViewModel();
            string message = "Rejected Successfully";
            int UserId = Usr.Id;
            try
            {
                
                var JobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == updateCandidateInterviewModel.JobId && x.CandProfId == updateCandidateInterviewModel.CanPrfId).FirstOrDefaultAsync();
                if (JobCandidate != null)
                {
                    var candidateName = await dbContext.PhCandidateProfiles.Where(x => x.Id == updateCandidateInterviewModel.CanPrfId).Select(x => x.CandName).FirstOrDefaultAsync();

                    var JobCandidateInterviews = dbContext.PhJobCandidateInterviews.Where(x => x.Joid == updateCandidateInterviewModel.JobId && x.CandProfId == updateCandidateInterviewModel.CanPrfId && x.Id == updateCandidateInterviewModel.InterviewId).FirstOrDefault();
                    if (JobCandidateInterviews != null)
                    {
                        JobCandidateInterviews.UpdatedBy = UserId;
                        JobCandidateInterviews.UpdatedDate = CurrentTime;
                        JobCandidateInterviews.InterviewStatus = (byte)InterviewStatus.Completed;
                        JobCandidateInterviews.Remarks = updateCandidateInterviewModel.Remarks;

                        dbContext.PhJobCandidateInterviews.Update(JobCandidateInterviews);
                    }

                    await dbContext.SaveChangesAsync();

                    //Activity
                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = updateCandidateInterviewModel.CanPrfId,
                        JobId = updateCandidateInterviewModel.JobId,
                        ActivityType = (byte)LogActivityType.ScheduleInterviewUpdates,
                        ActivityDesc = "  " + candidateName + " Interview is completed",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = "Candidate is not available";
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

        public async Task<GetResponseViewModel<ClientCandidateInterviewViewModel>> GetClientCandidatePreferences(int JobId, int CandProfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<ClientCandidateInterviewViewModel>();
            var response = new ClientCandidateInterviewViewModel();
            string message = string.Empty;
            try
            {
                

                var candidateProfile = await (from can in dbContext.PhCandidateProfiles
                                              join jobCan in dbContext.PhJobCandidates on can.Id equals jobCan.CandProfId
                                              where jobCan.Joid == JobId && jobCan.CandProfId == CandProfId
                                              select new
                                              {
                                                  can.Id,
                                                  can.CandName,
                                                  jobCan.CandProfStatus
                                              }).FirstOrDefaultAsync();
                if (candidateProfile != null)
                {
                    var JobCandidateInterviews = await dbContext.PhCandidateProfilesShareds.Where(x => x.Joid == JobId
                    && x.CandProfId == CandProfId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                    if (JobCandidateInterviews != null)
                    {
                        response.ModeofInterview = JobCandidateInterviews.ModeOfInterview;
                        response.ModeofInterviewName = Enum.GetName(typeof(ModeOfInterview), JobCandidateInterviews.ModeOfInterview);
                        response.InterviewTimeZone = JobCandidateInterviews.InterviewTimeZone;
                        response.InterviewDate = JobCandidateInterviews.InterviewDate;
                        response.InterviewStartTime = JobCandidateInterviews.StartTime;
                        response.InterviewEndTime = JobCandidateInterviews.EndTime;

                        response.CanPrfId = JobCandidateInterviews.CandProfId;
                        response.Id = JobCandidateInterviews.Id;
                        response.UserId = JobCandidateInterviews.CreatedBy;
                        response.JobId = JobId;

                        respModel.SetResult(response);
                        respModel.Status = true;
                    }
                    else
                    {
                        message = "There is no interviews for Candidate";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                }
                else
                {
                    message = "The Candidate is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
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

        public async Task<GetResponseViewModel<List<SharedProfileCandidateModel>>> GetClientSharedProfiles(string BatchId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<SharedProfileCandidateModel>>();
            var response = new List<SharedProfileCandidateModel>();
            try
            {
                

                response = await (from share in dbContext.PhCandidateProfilesShareds
                                  join canPrf in dbContext.PhCandidateProfiles on share.CandProfId equals canPrf.Id
                                  join job in dbContext.PhJobOpenings on share.Joid equals job.Id
                                  where share.BatchNo == BatchId && share.Status != (byte)RecordStatus.Delete
                                  select new SharedProfileCandidateModel
                                  {
                                      JobId = share.Joid,
                                      BatchId = share.BatchNo,
                                      CandidateShareProfId = share.Id,
                                      CanPrfId = share.CandProfId,
                                      CandidateName = canPrf.CandName,
                                      JobName = job.JobTitle,
                                      InterviewTimeZone = share.InterviewTimeZone
                                  }).ToListAsync();
                foreach (var item in response)
                {
                    var StatuDtls = (from jobCan in dbContext.PhJobCandidates
                                     join canStatus in dbContext.PhCandStatusSes on jobCan.CandProfStatus equals canStatus.Id
                                     where jobCan.Joid == item.JobId && jobCan.CandProfId == item.CanPrfId
                                     select new
                                     {
                                         jobCan.CandProfStatus,
                                         canStatus.Title,
                                         canStatus.Cscode
                                     }).FirstOrDefault();
                    if (StatuDtls != null)
                    {
                        item.CandProfStatus = StatuDtls.CandProfStatus;
                        item.CandProfStatusName = StatuDtls.Title;
                        item.CandProfStatusCode = StatuDtls.Cscode;
                    }
                }

                respModel.SetResult(response);
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

        public async Task<GetResponseViewModel<CandidateInterviewViewModel>> GetCandidateInterviewDtls(BacthProfilesModel bacthProfilesModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateInterviewViewModel>();
            var response = new CandidateInterviewViewModel
            {
                InterviewPrefernce = false
            };
            string message = string.Empty;
            try
            {
                

                var candidateProfile = await (from can in dbContext.PhCandidateProfiles
                                              join jobCan in dbContext.PhJobCandidates on can.Id equals jobCan.CandProfId
                                              join canStus in dbContext.PhCandStatusSes on jobCan.CandProfStatus equals canStus.Id
                                              where jobCan.Joid == bacthProfilesModel.JobId && jobCan.CandProfId == bacthProfilesModel.CanPrfId
                                              select new CandidateInterviewViewModel
                                              {
                                                  CanPrfId = can.Id,
                                                  CandidateName = can.CandName,
                                                  Location = can.CurrLocation,
                                                  ExperienceInMonths = can.ExperienceInMonths,
                                                  RecruiterId = jobCan.RecruiterId,
                                                  Gender = can.Gender,
                                                  MaritalStatus = can.MaritalStatus,
                                                  CandProfStatus = jobCan.CandProfStatus,
                                                  CandProfStatusName = canStus.Title,
                                                  CandProfStatusCode = canStus.Cscode,
                                                  Nationality = can.Nationality
                                              }).FirstOrDefaultAsync();
                if (candidateProfile != null)
                {
                    response.UserId = dbContext.PiHireUsers.Where(x => x.UserType == (byte)UserType.SuperAdmin).Select(x => x.Id).FirstOrDefault();
                    response.JobId = bacthProfilesModel.JobId;
                    response.CandProfStatus = candidateProfile.CandProfStatus;
                    response.CandProfStatusCode = candidateProfile.CandProfStatusCode;
                    response.CandProfStatusName = candidateProfile.CandProfStatusName;
                    response.YearsofExperience = ConvertYears(candidateProfile.ExperienceInMonths);

                    var CandidateProfileShared = await dbContext.PhCandidateProfilesShareds.Where(x => x.BatchNo == bacthProfilesModel.BatchId
                    && x.Joid == bacthProfilesModel.JobId && x.CandProfId == bacthProfilesModel.CanPrfId).FirstOrDefaultAsync();
                    if (CandidateProfileShared != null)
                    {
                        response.Reasons = CandidateProfileShared.Reasons;
                        response.EmailFields = CandidateProfileShared.EmailFields;
                        response.ConfView = CandidateProfileShared.ConfView;
                        response.Remarks = CandidateProfileShared.Remarks;
                        response.ClreviewStatus = CandidateProfileShared.ClreviewStatus;

                        response.InterviewTimeZone = CandidateProfileShared.InterviewTimeZone;
                        response.InterviewDate = CandidateProfileShared.InterviewDate;
                        response.InterviewStartTime = CandidateProfileShared.StartTime;
                        response.InterviewEndTime = CandidateProfileShared.EndTime;

                        response.ModeofInterview = CandidateProfileShared.ModeOfInterview;
                        response.ShortListFlag = CandidateProfileShared.ShortListFlag;
                        
                        if (!string.IsNullOrEmpty(response.InterviewStartTime))
                        {
                            response.InterviewPrefernce = true;
                        }
                    }

                    var JobCandidateInterviews = await dbContext.PhJobCandidateInterviews.Where(x => x.Joid == bacthProfilesModel.JobId && x.CandProfId == bacthProfilesModel.CanPrfId && x.ScheduledBy == (byte)ScheduledBy.Client).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                    if (JobCandidateInterviews != null)
                    {
                        response.ModeofInterview = JobCandidateInterviews.ModeOfInterview;
                        if (JobCandidateInterviews.ModeOfInterview != 0)
                        {
                            response.ModeofInterviewName = Enum.GetName(typeof(ModeOfInterview), JobCandidateInterviews.ModeOfInterview);
                        }
                        response.InterviewTimeZone = JobCandidateInterviews.InterviewTimeZone;
                        response.InterviewDate = JobCandidateInterviews.InterviewDate;
                        response.InterviewStartTime = JobCandidateInterviews.InterviewStartTime;
                        response.InterviewEndTime = JobCandidateInterviews.InterviewEndTime;
                        
                        response.InterviewPrefernce = true;
                    }

                    respModel.SetResult(response);
                    respModel.Status = true;
                }
                else
                {
                    message = "The Candidate is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
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

        public async Task<GetResponseViewModel<CandidateInterViewDtls>> GetCandidateInterviewDtlsToReSchedule(CandidateInterviewDtlsRequestViewModel CandidateInterviewDtlsRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateInterViewDtls>();
            var response = new CandidateInterViewDtls
            {
                InterviewPrefernce = false
            };
            string message = string.Empty;
            try
            {
                

                var JobCandidateInterviews = await dbContext.PhJobCandidateInterviews.Where(x => x.Joid == CandidateInterviewDtlsRequestViewModel.JobId
                && x.CandProfId == CandidateInterviewDtlsRequestViewModel.CanPrfId && x.ScheduledBy == (byte)ScheduledBy.Client).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                if (JobCandidateInterviews != null)
                {
                    response.ModeofInterview = JobCandidateInterviews.ModeOfInterview;
                    if (JobCandidateInterviews.ModeOfInterview != 0)
                    {
                        response.ModeofInterviewName = Enum.GetName(typeof(ModeOfInterview), JobCandidateInterviews.ModeOfInterview);
                    }
                    response.InterviewTimeZone = JobCandidateInterviews.InterviewTimeZone;
                    response.InterviewDate = JobCandidateInterviews.InterviewDate;
                    response.InterviewStartTime = JobCandidateInterviews.InterviewStartTime;
                    response.InterviewEndTime = JobCandidateInterviews.InterviewEndTime;
                    
                    response.InterviewPrefernce = true;
                    response.InterviewId = JobCandidateInterviews.Id;
                    response.JobId = JobCandidateInterviews.Joid;
                    response.Location = JobCandidateInterviews.Location;
                    response.Remarks = JobCandidateInterviews.Remarks;
                    response.ClientPannel = JobCandidateInterviews.InterviewerEmail;
                    response.HirePannel = JobCandidateInterviews.PiTeamEmailIds;
                    response.CanPrfId = JobCandidateInterviews.CandProfId;

                    respModel.SetResult(response);
                    respModel.Status = true;
                }
                else
                {
                    message = " The Candidate is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
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


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> ShortlistCandidate(int ShareId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();

            string message = "Shortlisted Successfully";
            try
            {
                
                var ShareProfile = await dbContext.PhCandidateProfilesShareds.Where(x => x.Id == ShareId).FirstOrDefaultAsync();
                if (ShareProfile != null)
                {
                    var cadDtls = (from canJob in dbContext.PhJobCandidates
                                   join canPrf in dbContext.PhCandidateProfiles on canJob.CandProfId equals canPrf.Id
                                   where canJob.Joid == ShareProfile.Joid && canJob.CandProfId == ShareProfile.CandProfId
                                   select new
                                   {
                                       canPrf.CandName,
                                       canJob.CandProfStatus
                                   }).FirstOrDefault();

                    ShareProfile.ShortListFlag = true;
                    dbContext.PhCandidateProfilesShareds.Update(ShareProfile);
                    dbContext.SaveChanges();

                    //Activity
                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = ShareProfile.CandProfId,
                        JobId = ShareProfile.Joid,
                        ActivityType = (byte)LogActivityType.ScheduleInterviewUpdates,
                        ActivityDesc = " Shortlisted -" + cadDtls?.CandName + " by client",
                        UserId = ShareProfile.CreatedBy
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    // Applying work flow conditions 
                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    {
                        ActionMode = (byte)WorkflowActionMode.Candidate,
                        CanProfId = ShareProfile.CandProfId,
                        CurrentStatusId = cadDtls.CandProfStatus,
                        JobId = ShareProfile.Joid,
                        UserId = ShareProfile.CreatedBy,
                        TaskCode = TaskCode.CSLD.ToString()
                    };
                    var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
                    if (wfResp.Status && wfResp.isNotification)
                    {
                        foreach (var item in wfResp.WFNotifications)
                        {
                            var notificationPushed = new NotificationPushedViewModel
                            {
                                JobId = wfResp.JoId,
                                PushedTo = item.UserIds,
                                NoteDesc = item.NoteDesc,
                                Title = item.Title,
                                CreatedBy = ShareProfile.CreatedBy
                            };
                            notificationPushedViewModel.Add(notificationPushed);
                        }
                    }


                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = "Share details are not available";
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
            return Tuple.Create(notificationPushedViewModel, respModel);
        }

        public async Task<CreateResponseViewModel<string>> UpdateCandidateProfileClientViewStatus(int ShareId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Updated Successfully";
            try
            {
                

                var phCandidateProfilesShared = await dbContext.PhCandidateProfilesShareds.Where(x => x.Id == ShareId).FirstOrDefaultAsync();
                if (phCandidateProfilesShared != null)
                {
                    if (phCandidateProfilesShared.ClreviewStatus == (byte)ClreviewStatus.NotReviewd)
                    {
                        phCandidateProfilesShared.UpdatedDate = CurrentTime;
                        phCandidateProfilesShared.ClreviewStatus = (byte)ClreviewStatus.Reviewed;

                        dbContext.PhCandidateProfilesShareds.Update(phCandidateProfilesShared);
                        await dbContext.SaveChangesAsync();

                        var stats = await dbContext.PhJobOpeningActvCounters.Where(x => x.Joid == phCandidateProfilesShared.Joid).FirstOrDefaultAsync();
                        if (stats != null)
                        {
                            stats.ClientViewsCounter += 1;
                            stats.UpdatedDate = CurrentTime;

                            dbContext.PhJobOpeningActvCounters.Update(stats);
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = "Share Candidate is not available";
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

        public async Task<GetResponseViewModel<string>> getMailCredExistAsync()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<string>();
            string message = "";
            try
            {
                

                var userId = Usr.Id;
                //SimpleEncrypt simpleEncrypt = new SimpleEncrypt();
                //var cred = await dbContext.PhUsersConfigs.Where(da => da.UserId == userId && da.VerifyFlag == true).Select(da => new { da.UserName, da.PasswordHash }).FirstOrDefaultAsync();

                //if (cred == null || string.IsNullOrEmpty(cred.UserName) || string.IsNullOrEmpty(cred.PasswordHash))
                //{
                //    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ". User cred are not available", respModel.Meta.RequestID);
                //    message = "User smtp credentials are not available";
                //    respModel.Status = false;
                //    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                //    respModel.Result = message;
                //    return respModel;
                //}
                //var pass = simpleEncrypt.passwordDecrypt(cred.PasswordHash);
                //Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(cred.UserName, pass);

                var MicrosoftToken = Usr.MicrosoftToken;
                if (string.IsNullOrEmpty(MicrosoftToken))
                {
                    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ". User cred are not available", respModel.Meta.RequestID);
                    message = "User smtp credentials are not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    respModel.Result = message;
                    return respModel;
                }
                Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(MicrosoftToken);

                var chk = await outlook.getAccessTokenAsync(logger);
                if (string.IsNullOrEmpty(chk.token))
                {
                    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "token generating failed." + Newtonsoft.Json.JsonConvert.SerializeObject(chk), respModel.Meta.RequestID);

                    message = "User smtp credentials are not valid";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    respModel.Result = message;
                    return respModel;
                }
                else
                {
                    respModel.SetResult("");
                    respModel.Status = true;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<JobMailCountViewModel>> getJobMailsCountAsync(int JobId, int CandidateId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<JobMailCountViewModel>();
            string message = "";
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, "Start of method:", respModel.Meta.RequestID + ", JobId:" + JobId + ", CandidateId:" + CandidateId);
                var jobObj = await dbContext.PhJobOpenings.Where(da => da.Id == JobId).Select(da => new { da.JobTitle }).FirstOrDefaultAsync();
                var candObj = await dbContext.PhCandidateProfiles.Where(da => da.Id == CandidateId).Select(da => new { da.EmailId }).FirstOrDefaultAsync();
                if (jobObj != null && candObj != null)
                {
                    var userId = Usr.Id;

                    //SimpleEncrypt simpleEncrypt = new SimpleEncrypt();
                    //var cred = await dbContext.PhUsersConfigs.Where(da => da.UserId == userId && da.VerifyFlag == true).Select(da => new { da.UserName, da.PasswordHash }).FirstOrDefaultAsync();

                    //if (cred == null || string.IsNullOrEmpty(cred.UserName) || string.IsNullOrEmpty(cred.PasswordHash))
                    //{
                    //    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". User cred are not available", respModel.Meta.RequestID);
                    //    message = "User cred are not available";
                    //    respModel.Status = false;
                    //    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    //    return respModel;
                    //}
                    //var pass = simpleEncrypt.passwordDecrypt(cred.PasswordHash);
                    //Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(cred.UserName, pass);

                    var MicrosoftToken = Usr.MicrosoftToken;
                    if (string.IsNullOrEmpty(MicrosoftToken))
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ". User cred are not available", respModel.Meta.RequestID);
                        message = "User smtp credentials are not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }
                    Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(MicrosoftToken);

                    var obj = await outlook.SearchByEmailAsync(logger, candObj.EmailId);
                    if (!string.IsNullOrEmpty(obj.Item2))
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". " + obj.Item2, respModel.Meta.RequestID);
                        message = obj.Item2;
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }
                    var subjectKey = getJobMailSubject(JobId, jobObj.JobTitle);
                    var subjectKey2 = getJobMailSubjectReverse(JobId, jobObj.JobTitle);
                    var lst = obj.Item1?.value?.Where(da => da.subject.Contains(subjectKey) || da.subject.Contains(subjectKey2))?.ToList();
                    var totalCount = lst?.Select(da => da.conversationId)?.Distinct()?.Count() ?? 0;
                    var unreadCount = lst?.Where(da => da.isRead == false)?.Select(da => da.conversationId)?.Distinct()?.Count() ?? 0;

                    respModel.SetResult(new JobMailCountViewModel { TotalCount = totalCount, UnreadCount = unreadCount });
                    respModel.Status = true;
                }
                else
                {
                    if (jobObj == null)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + " is not available", respModel.Meta.RequestID);
                        message = "Job are not available";
                    }
                    else if (candObj == null)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", CandidateId:" + CandidateId + " is not available", respModel.Meta.RequestID);
                        message = "Candidate not found";
                    }
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    return respModel;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        private string getJobMailSubject(int JobId, string JobTitle)
        {
            return JobId + " - " + JobTitle;
        }
        private string getJobMailSubjectReverse(int JobId, string JobTitle)
        {
            return JobTitle + " - " + JobId;
        }

        public async Task<GetResponseViewModel<List<JobMailGroupViewModel>>> getJobMailsAsync(int JobId, int CandidateId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<JobMailGroupViewModel>>();
            string message = "";
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, "Start of method:", respModel.Meta.RequestID + ", JobId:" + JobId + ", CandidateId:" + CandidateId);
                var jobObj = await dbContext.PhJobOpenings.Where(da => da.Id == JobId).Select(da => new { da.JobTitle }).FirstOrDefaultAsync();
                var candObj = await dbContext.PhCandidateProfiles.Where(da => da.Id == CandidateId).Select(da => new { da.EmailId }).FirstOrDefaultAsync();
                if (jobObj != null && candObj != null)
                {
                    var userId = Usr.Id;

                    //SimpleEncrypt simpleEncrypt = new SimpleEncrypt();
                    //var cred = await dbContext.PhUsersConfigs.Where(da => da.UserId == userId && da.VerifyFlag == true).Select(da => new { da.UserName, da.PasswordHash }).FirstOrDefaultAsync();

                    //if (cred == null || string.IsNullOrEmpty(cred.UserName) || string.IsNullOrEmpty(cred.PasswordHash))
                    //{
                    //    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". User cred are not available", respModel.Meta.RequestID);
                    //    message = "User cred are not available";
                    //    respModel.Status = false;
                    //    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    //    return respModel;
                    //}
                    //var pass = simpleEncrypt.passwordDecrypt(cred.PasswordHash);
                    //Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(cred.UserName, pass);

                    var MicrosoftToken = Usr.MicrosoftToken;
                    if (string.IsNullOrEmpty(MicrosoftToken))
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ". User cred are not available", respModel.Meta.RequestID);
                        message = "User smtp credentials are not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }
                    Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(MicrosoftToken);

                    var obj = await outlook.SearchByEmailAsync(logger, candObj.EmailId);
                    if (!string.IsNullOrEmpty(obj.Item2))
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". " + obj.Item2, respModel.Meta.RequestID);
                        message = obj.Item2;
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }
                    var subjectKey = getJobMailSubject(JobId, jobObj.JobTitle);
                    var ids = obj.Item1?.value?.Where(da => da.subject.Contains(subjectKey))?.Select(da => da.conversationId)?.Distinct()?.ToList() ?? new List<string>();
                    List<JobMailGroupViewModel> resp = new List<JobMailGroupViewModel>();
                    foreach (var conversationId in ids)
                    {
                        var msg = await outlook.GetMailsByConversationAsync(logger, conversationId);
                        if (!string.IsNullOrEmpty(msg.Item2))
                        {
                            logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ", conversationId:" + conversationId + ". " + msg.Item2, respModel.Meta.RequestID);
                        }
                        else
                        {
                            //var ms = msg.Item1.value.Select(da => JobMailViewModel.ToViewModel(da, cred.UserName)).ToList();
                            var ms = msg.Item1.value.Select(da => JobMailViewModel.ToViewModel(da, Usr.MicrosoftUserName)).ToList();
                            var attIds = msg.Item1.value.Where(da => da.hasAttachments ?? false).Select(da => da.id).ToList();
                            foreach (var attId in attIds)
                            {
                                var att = await outlook.getAttachmentsAsync(logger, attId);
                                if (!string.IsNullOrEmpty(att.Item2))
                                {
                                    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ", conversationId:" + conversationId + ", messageId:" + attId + ". " + att.Item2, respModel.Meta.RequestID);
                                }
                                else
                                {
                                    var _obj = ms.FirstOrDefault(da => da.id == attId);
                                    _obj.attachments = att.Item1.value.Select(da => new ViewModels.JobMailViewModels.Attachment { FileName = da.name, content = da.contentBytes, contentType = da.contentType }).ToList();
                                }
                            }
                            resp.Add(new JobMailGroupViewModel
                            {
                                conversationId = conversationId,
                                jobMails = ms
                            });
                        }
                    }

                    respModel.SetResult(resp);
                    respModel.Status = true;
                }
                else
                {
                    if (jobObj == null)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + " is not available", respModel.Meta.RequestID);
                        message = "Job are not available";
                    }
                    else if (candObj == null)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", CandidateId:" + CandidateId + " is not available", respModel.Meta.RequestID);
                        message = "Candidate not found";
                    }
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    return respModel;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        public async Task<GetResponseViewModel<string>> setMailStatusReadedAsync(string messageId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<string>();
            string message = "";
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, "Start of method:", respModel.Meta.RequestID + ", messageId:" + messageId);
                var userId = Usr.Id;

                //SimpleEncrypt simpleEncrypt = new SimpleEncrypt();
                //var cred = await dbContext.PhUsersConfigs.Where(da => da.UserId == userId && da.VerifyFlag == true).Select(da => new { da.UserName, da.PasswordHash }).FirstOrDefaultAsync();
                //if (cred == null || string.IsNullOrEmpty(cred.UserName) || string.IsNullOrEmpty(cred.PasswordHash))
                //{
                //    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", messageId:" + messageId + ", userId:" + userId + ". User cred are not available", respModel.Meta.RequestID);
                //    message = "User cred are not available";
                //    respModel.Status = false;
                //    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                //    return respModel;
                //}
                //var pass = simpleEncrypt.passwordDecrypt(cred.PasswordHash);
                //Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(cred.UserName, pass);

                var MicrosoftToken = Usr.MicrosoftToken;
                if (string.IsNullOrEmpty(MicrosoftToken))
                {
                    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ". User cred are not available", respModel.Meta.RequestID);
                    message = "User smtp credentials are not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    return respModel;
                }
                Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(MicrosoftToken);


                var obj = await outlook.setMailStatusReadedAsync(logger, messageId);
                if (obj.status ?? false)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", messageId:" + messageId + ", userId:" + userId + ". " + obj, respModel.Meta.RequestID);
                    message = obj.response;
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    return respModel;
                }

                respModel.SetResult("");
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", messageId:" + messageId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<string>> sendJobMailsAsync(int JobId, int CandidateId, Common._3rdParty.Microsoft.SendMail.SendMail_ViewModel model, List<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<string>();
            string message = "";
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, "Start of method:", respModel.Meta.RequestID + ", JobId:" + JobId + ", CandidateId:" + CandidateId);
                var jobObj = await dbContext.PhJobOpenings.Where(da => da.Id == JobId).Select(da => new { da.JobTitle }).FirstOrDefaultAsync();
                var candObj = await dbContext.PhCandidateProfiles.Where(da => da.Id == CandidateId).Select(da => new { da.CandName, da.EmailId }).FirstOrDefaultAsync();
                if (jobObj != null && candObj != null)
                {
                    var userId = Usr.Id;
                    var subjectKey = getJobMailSubject(JobId, jobObj.JobTitle);
                    if (model.message.subject.IndexOf(subjectKey) == -1)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". Subject does not contains job title:" + jobObj.JobTitle, respModel.Meta.RequestID);
                        message = "Subject does not contain job title/not prorper";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }
                    //SimpleEncrypt simpleEncrypt = new SimpleEncrypt();
                    //var cred = await dbContext.PhUsersConfigs.Where(da => da.UserId == userId && da.VerifyFlag == true).Select(da => new { da.UserName, da.PasswordHash }).FirstOrDefaultAsync();
                    //if (cred == null || string.IsNullOrEmpty(cred.UserName) || string.IsNullOrEmpty(cred.PasswordHash))
                    //{
                    //    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". User cred are not available", respModel.Meta.RequestID);
                    //    message = "User cred are not available";
                    //    respModel.Status = false;
                    //    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    //    return respModel;
                    //}
                    //var pass = simpleEncrypt.passwordDecrypt(cred.PasswordHash);
                    //Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(cred.UserName, pass);
                    var MicrosoftToken = Usr.MicrosoftToken;
                    if (string.IsNullOrEmpty(MicrosoftToken))
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ". User cred are not available", respModel.Meta.RequestID);
                        message = "User smtp credentials are not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }
                    Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(MicrosoftToken);

                    model.message = model.message ?? new Common._3rdParty.Microsoft.SendMail.SendMail_Message();
                    model.message.toRecipients = new List<Common._3rdParty.Microsoft.Microsoft_ToRecipient>
                    {
                        new Common._3rdParty.Microsoft.Microsoft_ToRecipient
                        {
                            emailAddress=new Common._3rdParty.Microsoft.Microsoft_EmailAddress
                            {
                                 address= candObj.EmailId
                            }
                        }
                    };

                    var obj = await outlook.SendEmailAsync(logger, model, files);
                    if (!string.IsNullOrEmpty(obj))
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". " + obj, respModel.Meta.RequestID);
                        message = obj;
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }

                    respModel.SetResult(obj);
                    respModel.Status = true;
                }
                else
                {
                    if (jobObj == null)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + " is not available", respModel.Meta.RequestID);
                        message = "Job are not available";
                    }
                    else if (candObj == null)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", CandidateId:" + CandidateId + " is not available", respModel.Meta.RequestID);
                        message = "Candidate not found";
                    }
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    return respModel;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<string>> replyJobMailsAsync(int JobId, int CandidateId, string messageId, Common._3rdParty.Microsoft.ReplyMail.ReplyMail_ViewModel model, List<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<string>();
            string message = "";
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, "Start of method:", respModel.Meta.RequestID + ", JobId:" + JobId + ", CandidateId:" + CandidateId);
                var jobObj = await dbContext.PhJobOpenings.Where(da => da.Id == JobId).Select(da => new { da.JobTitle }).FirstOrDefaultAsync();
                var candObj = await dbContext.PhCandidateProfiles.Where(da => da.Id == CandidateId).Select(da => new { da.CandName, da.EmailId }).FirstOrDefaultAsync();
                if (jobObj != null && candObj != null)
                {
                    var userId = Usr.Id;
                    //SimpleEncrypt simpleEncrypt = new SimpleEncrypt();
                    //var cred = await dbContext.PhUsersConfigs.Where(da => da.UserId == userId && da.VerifyFlag == true).Select(da => new { da.UserName, da.PasswordHash }).FirstOrDefaultAsync();
                    //if (cred == null || string.IsNullOrEmpty(cred.UserName) || string.IsNullOrEmpty(cred.PasswordHash))
                    //{
                    //    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". User cred are not available", respModel.Meta.RequestID);
                    //    message = "User cred are not available";
                    //    respModel.Status = false;
                    //    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    //    return respModel;
                    //}
                    //var pass = simpleEncrypt.passwordDecrypt(cred.PasswordHash);
                    //Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(cred.UserName, pass);
                    var MicrosoftToken = Usr.MicrosoftToken;
                    if (string.IsNullOrEmpty(MicrosoftToken))
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ". User cred are not available", respModel.Meta.RequestID);
                        message = "User smtp credentials are not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }
                    Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(MicrosoftToken);

                    model.message = model.message ?? new Common._3rdParty.Microsoft.ReplyMail.ReplyMail_Message();
                    model.message.toRecipients = new List<Common._3rdParty.Microsoft.Microsoft_ToRecipient>
                    {
                        new Common._3rdParty.Microsoft.Microsoft_ToRecipient
                        {
                            emailAddress=new Common._3rdParty.Microsoft.Microsoft_EmailAddress
                            {
                                 address= candObj.EmailId
                            }
                        }
                    };

                    var obj = await outlook.ReplyEmailAsync(logger, messageId, model, files);
                    if (!string.IsNullOrEmpty(obj))
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". " + obj, respModel.Meta.RequestID);
                        message = obj;
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }

                    respModel.SetResult(obj);
                    respModel.Status = true;
                }
                else
                {
                    if (jobObj == null)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + " is not available", respModel.Meta.RequestID);
                        message = "Job are not available";
                    }
                    else if (candObj == null)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", CandidateId:" + CandidateId + " is not available", respModel.Meta.RequestID);
                        message = "Candidate not found";
                    }
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    return respModel;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<string>> replyAllJobMailsAsync(int JobId, int CandidateId, string messageId, Common._3rdParty.Microsoft.ReplyAllMail.ReplyAllMail_ViewModel model, List<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<string>();
            string message = "";
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, "Start of method:", respModel.Meta.RequestID + ", JobId:" + JobId + ", CandidateId:" + CandidateId);
                var jobObj = await dbContext.PhJobOpenings.Where(da => da.Id == JobId).Select(da => new { da.JobTitle }).FirstOrDefaultAsync();
                var candObj = await dbContext.PhCandidateProfiles.Where(da => da.Id == CandidateId).Select(da => new { da.CandName, da.EmailId }).FirstOrDefaultAsync();
                if (jobObj != null && candObj != null)
                {
                    var userId = Usr.Id;
                    //SimpleEncrypt simpleEncrypt = new SimpleEncrypt();
                    //var cred = await dbContext.PhUsersConfigs.Where(da => da.UserId == userId && da.VerifyFlag == true).Select(da => new { da.UserName, da.PasswordHash }).FirstOrDefaultAsync();
                    //if (cred == null || string.IsNullOrEmpty(cred.UserName) || string.IsNullOrEmpty(cred.PasswordHash))
                    //{
                    //    logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". User cred are not available", respModel.Meta.RequestID);
                    //    message = "User cred are not available";
                    //    respModel.Status = false;
                    //    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    //    return respModel;
                    //}
                    //var pass = simpleEncrypt.passwordDecrypt(cred.PasswordHash);
                    //Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(cred.UserName, pass);
                    var MicrosoftToken = Usr.MicrosoftToken;
                    if (string.IsNullOrEmpty(MicrosoftToken))
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ". User cred are not available", respModel.Meta.RequestID);
                        message = "User smtp credentials are not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }
                    Common._3rdParty.MicrosoftOutlook outlook = new Common._3rdParty.MicrosoftOutlook(MicrosoftToken);

                    model.message = model.message ?? new Common._3rdParty.Microsoft.ReplyAllMail.ReplyAllMail_Message();

                    var obj = await outlook.ReplyAllEmailAsync(logger, messageId, model, files);
                    if (!string.IsNullOrEmpty(obj))
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + ", userId:" + userId + ". " + obj, respModel.Meta.RequestID);
                        message = obj;
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        return respModel;
                    }

                    respModel.SetResult(obj);
                    respModel.Status = true;
                }
                else
                {
                    if (jobObj == null)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId + " is not available", respModel.Meta.RequestID);
                        message = "Job are not available";
                    }
                    else if (candObj == null)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", CandidateId:" + CandidateId + " is not available", respModel.Meta.RequestID);
                        message = "Candidate not found";
                    }
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    return respModel;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, "respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel) + ", JobId:" + JobId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }
    }
}
