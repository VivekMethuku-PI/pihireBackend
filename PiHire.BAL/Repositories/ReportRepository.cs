using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.Common.Types;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Entities;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Security.AccessControl;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class ReportRepository : BaseRepository, IReportRepository
    {
        readonly Logger logger;


        public ReportRepository(DAL.PiHIRE2Context dbContext, Common.Extensions.AppSettings appSettings, ILogger<ReportRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }

        #region Dashboard

        public async Task<GetResponseViewModel<DashboardCandidateInterviewViewModel>> GetCandidateInterviewAsync(DashboardCandidateInterviewFilterViewModel filterViewModel, int? tabId = null)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardCandidateInterviewViewModel>();
            try
            {
                if (filterViewModel.CurrentPage < 1)
                {
                    filterViewModel.CurrentPage = 1;
                }
                if (filterViewModel.PerPage < 0)
                {
                    filterViewModel.PerPage = 0;
                }

                int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;
                var dtls = await dbContext.GetDashboardCandidateInterviewAsync(null, null, filterViewModel.PuIds, filterViewModel.BuIds, tabId, filterViewModel.PerPage, skip, Usr.UserTypeId, Usr.Id);
                var interviewTotCnt = await dbContext.GetDashboardCandidateInterviewCountAsync(null, null, filterViewModel.PuIds, filterViewModel.BuIds, tabId, Usr.UserTypeId, Usr.Id);
                var totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(interviewTotCnt?.TotCnt ?? 0) / filterViewModel.PerPage) : dtls.Count;
                respModel.Status = true;

                respModel.SetResult(new DashboardCandidateInterviewViewModel
                {
                    interviews = dtls.Select(da => _DashboardCandidateInterviewViewModel.ToViewModel(da, this)).ToList(),
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }


        public async Task<GetResponseViewModel<DashboardJobStageViewModel>> GetDashboardJobStageAsync(DashboardCandidateInterviewFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardJobStageViewModel>();
            try
            {
                if (filterViewModel.CurrentPage < 1)
                {
                    filterViewModel.CurrentPage = 1;
                }
                if (filterViewModel.PerPage < 0)
                {
                    filterViewModel.PerPage = 0;
                }
                int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                var rawDtls = await dbContext.GetDashboardJobStageAsync(filterViewModel.PuIds, filterViewModel.BuIds, filterViewModel.PerPage, skip, Usr.UserTypeId, Usr.Id);
                var jobTotCnt = await dbContext.GetDashboardJobStageCountAsync(filterViewModel.PuIds, filterViewModel.BuIds, Usr.UserTypeId, Usr.Id);

                var joIds = rawDtls.Select(x => x.JobId).Distinct().ToArray();
                var stages = await dbContext.PhCandStagesSes.AsNoTracking().Where(x => x.Status != (byte)RecordStatus.Delete).ToListAsync();
                var counters = await dbContext.PhJobOpeningStatusCounters.AsNoTracking().Where(x => joIds.Contains(x.Joid) && x.Status != (byte)RecordStatus.Delete).ToListAsync();

                var bdmIds = rawDtls.Where(da => da.bdmId.HasValue).Select(da => da.bdmId).Distinct().ToArray();
                var countryIds = rawDtls.Where(da => da.jobCountryId.HasValue).Select(da => da.jobCountryId).Distinct().ToArray();
                var cityIds = rawDtls.Where(da => da.jobCityId.HasValue).Select(da => da.jobCityId).Distinct().ToArray();

                var bdms = await dbContext.PiHireUsers.AsNoTracking().Where(da => bdmIds.Contains(da.Id)).ToListAsync();
                var countries = await dbContext.PhCountries.AsNoTracking().Where(da => countryIds.Contains(da.Id)).ToDictionaryAsync(da => da.Id, da2 => da2.Name);
                var cities = await dbContext.PhCities.AsNoTracking().Where(da => cityIds.Contains(da.Id)).ToDictionaryAsync(da => da.Id, da2 => da2.Name);

                var JobOpeningStatusIds = rawDtls.Where(da => da.JobOpeningStatus.HasValue).Select(da => da.JobOpeningStatus.Value).Distinct().ToArray();
                var _JobOpeningStatus = await dbContext.PhJobStatusSes.AsNoTracking().Where(da => JobOpeningStatusIds.Contains(da.Id)).Select(da => new { da.Id, da.Title, da.Jscode }).ToListAsync();
                var JobOpeningStatus = _JobOpeningStatus.ToDictionary(da => da.Id, da2 => da2.Title);
                var JobOpeningStatuCodes = _JobOpeningStatus.ToDictionary(da => da.Id, da2 => da2.Jscode);

                var jobs = rawDtls.Select(da => _DashboardJobStageViewModel.ToViewModel(this, da, bdms, countries, cities, JobOpeningStatus, JobOpeningStatuCodes)).ToList();
                foreach (var job in jobs)
                {
                    job.JobOpeningStatusCounter = new List<JobOpeningStatusCounterViewModel>();
                    foreach (var stats in stages)
                    {
                        job.JobOpeningStatusCounter.Add(new JobOpeningStatusCounterViewModel
                        {
                            StageID = stats.Id,
                            StageColor = stats.ColorCode,
                            StageName = stats.Title,
                            Counter = counters.Where(x => x.Joid == job.JobId && x.StageId == stats.Id).Sum(x => x.Counter)
                        });
                    }
                }
                var JobOpeningStatusCounter = new List<JobOpeningStatusCounterViewModel>();
                foreach (var stage in stages)
                {
                    JobOpeningStatusCounter.Add(new JobOpeningStatusCounterViewModel
                    {
                        StageID = stage.Id,
                        StageColor = stage.ColorCode,
                        StageName = stage.Title,
                        Counter = 0
                    });
                }
                var totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(jobTotCnt?.TotCnt ?? 0) / filterViewModel.PerPage) : jobs.Count;

                respModel.Status = true;
                respModel.SetResult(new DashboardJobStageViewModel
                {
                    jobs = jobs,
                    JobOpeningStatusCounter = JobOpeningStatusCounter,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<List<PiplineCandidatesViewModel>>> GetDashboardJobStageCandidatesAsync(int StageId, DashboardCandidateInterviewFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<PiplineCandidatesViewModel>>();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: StageId->" + StageId, respModel.Meta.RequestID);
                if (filterViewModel.CurrentPage < 1)
                {
                    filterViewModel.CurrentPage = 1;
                }
                if (filterViewModel.PerPage < 0)
                {
                    filterViewModel.PerPage = 0;
                }
                int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                var rawDtls = await dbContext.GetDashboardJobStageAsync(filterViewModel.PuIds, filterViewModel.BuIds, filterViewModel.PerPage, skip, Usr.UserTypeId, Usr.Id);
                var joIds = rawDtls.Select(x => x.JobId).Distinct().ToArray();

                var model = new List<PiplineCandidatesViewModel>();
                var jobStatus_close = await dbContext.PhJobStatusSes.Where(da => da.Jscode == (JobStatusCodes.CLS + "")).Select(da => da.Id).FirstOrDefaultAsync();
                var piplineCandidates = await (from canProfile in dbContext.PhCandidateProfiles
                                               join jobProfile in dbContext.PhJobCandidates on canProfile.Id equals jobProfile.CandProfId
                                               join canStatus in dbContext.PhCandStatusSes on jobProfile.CandProfStatus equals canStatus.Id
                                               join user in dbContext.PiHireUsers on canProfile.EmailId equals user.UserName
                                               where jobProfile.StageId == StageId && joIds.Contains(jobProfile.Joid)
                                               && canProfile.Status != (byte)RecordStatus.Delete && jobProfile.Status != (byte)RecordStatus.Delete
                                               && canStatus.Status != (byte)RecordStatus.Delete
                                               && user.Status != (byte)RecordStatus.Delete && user.UserType == (byte)UserType.Candidate
                                               select new
                                               {
                                                   jobProfile.Joid,
                                                   jobProfile.CandProfStatus,
                                                   jobProfile.CreatedDate,
                                                   jobProfile.RecruiterId,
                                                   canStatus.Title,
                                                   canProfile.FullNameInPp,
                                                   canProfile.CandName,
                                                   jobProfile.CandProfId,
                                                   canStatus.Cscode,
                                                   user.Id,
                                                   user.ProfilePhoto
                                               }).ToListAsync();
                var jobIds = piplineCandidates.Select(da => (int?)da.Joid).Distinct().ToArray();
                var jobs = await dbContext.PhJobOpenings.AsNoTracking().Where(da => da.JobOpeningStatus != jobStatus_close && jobIds.Contains(da.Id)).Select(da => new { da.Id, da.JobTitle }).ToListAsync();
                var candIds = piplineCandidates.Select(da => (int?)da.CandProfId).Distinct().ToArray();
                var candStatusIds = piplineCandidates.Select(da => (int?)da.CandProfStatus).Distinct().ToArray();
                var candStatusHistory = await dbContext.PhActivityLogs.AsNoTracking()
                    .Where(da => (da.ActivityMode == (byte)ActivityOn.Candidate) && (da.ActivityType == (byte)AuditActivityType.StatusUpdates)
                                    && jobIds.Contains(da.Joid) && candIds.Contains(da.ActivityOn) && candStatusIds.Contains(da.UpdateStatusId))
                    .Select(da => new { CandProfId = da.ActivityOn, newCandStatusId = da.UpdateStatusId, da.Joid, da.CreatedBy, da.CreatedDate }).ToListAsync();
                var usrsIds = candStatusHistory.Select(da => da.CreatedBy).Union(piplineCandidates.Where(da => da.RecruiterId.HasValue).Select(da => da.RecruiterId.Value)).Distinct().ToArray();
                var Usrs = await dbContext.PiHireUsers.Where(da => usrsIds.Contains(da.Id)).Select(da => new { da.Id, da.FirstName, da.LastName, da.ProfilePhoto }).ToListAsync();
                foreach (var profile in piplineCandidates)
                {
                    if (jobs.FirstOrDefault(da => da.Id == profile.Joid) != null)
                    {
                        var stageHistory = candStatusHistory.FirstOrDefault(da => da.CandProfId == profile.CandProfId && da.Joid == profile.Joid && da.newCandStatusId == profile.CandProfStatus);
                        var recruiter = Usrs.Where(da => da.Id == profile.RecruiterId).FirstOrDefault();
                        var piplineCandidatesViewModel1 = new PiplineCandidatesViewModel
                        {
                            UserId = profile.Id,
                            CandidateStatuName = profile.Title,
                            CandidateStatusId = profile.CandProfStatus,
                            CandidateId = profile.CandProfId,
                            Name = profile.CandName == string.Empty ? profile.CandName : profile.CandName,
                            JobId = profile.Joid,
                            Cscode = profile.Cscode,
                            StageId = StageId,
                            ProfilePhoto = getCandidatePhotoUrl(profile.ProfilePhoto, profile.Id),
                            isBlockListed = profile.Cscode == "BLT" ? true : false,
                            ProfileAge = GetTimeDiff(profile.CreatedDate),
                            ProfileDate = profile.CreatedDate,
                            stageUpdatedDate = stageHistory?.CreatedDate,
                            stageUpdatedAge = stageHistory == null ? "" : GetTimeDiff(stageHistory.CreatedDate),
                            stageUpdatedBy = stageHistory == null ? "" : Usrs.Where(da => da.Id == stageHistory.CreatedBy).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                            stageUpdatedByPhoto = stageHistory == null ? "" : getEmployeePhotoUrl(Usrs.Where(da => da.Id == stageHistory.CreatedBy).Select(da => da.ProfilePhoto).FirstOrDefault(), stageHistory.CreatedBy),
                            RecruiterId = profile.RecruiterId,
                            RecruiterName = recruiter == null ? "" : (recruiter.FirstName + " " + recruiter.LastName),
                            RecruiterPhoto = recruiter == null ? "" : getEmployeePhotoUrl(recruiter.ProfilePhoto, recruiter.Id),
                            JobName = jobs.Where(da => da.Id == profile.Joid).Select(da => da.JobTitle).FirstOrDefault()
                        };
                        model.Add(piplineCandidatesViewModel1);
                    }
                }
                model = model.OrderByDescending(da => da.stageUpdatedDate).ToList();
                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, "StageId->" + StageId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<List<PiplineCandidatesViewModel>>> GetDashboardJobStageCandidatesAsync(int JobId, int StageId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<PiplineCandidatesViewModel>>();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: jobId->" + JobId + ", StageId->" + StageId, respModel.Meta.RequestID);
                var model = new List<PiplineCandidatesViewModel>();
                var piplineCandidates = await (from canProfile in dbContext.PhCandidateProfiles
                                               join jobProfile in dbContext.PhJobCandidates on canProfile.Id equals jobProfile.CandProfId
                                               join canStatus in dbContext.PhCandStatusSes on jobProfile.CandProfStatus equals canStatus.Id
                                               join user in dbContext.PiHireUsers on canProfile.EmailId equals user.UserName
                                               where jobProfile.StageId == StageId && jobProfile.Joid == JobId
                                               && canProfile.Status != (byte)RecordStatus.Delete && jobProfile.Status != (byte)RecordStatus.Delete
                                               && canStatus.Status != (byte)RecordStatus.Delete
                                               && user.Status != (byte)RecordStatus.Delete && user.UserType == (byte)UserType.Candidate
                                               select new
                                               {
                                                   jobProfile.CandProfStatus,
                                                   jobProfile.RecruiterId,
                                                   canStatus.Title,
                                                   canProfile.FullNameInPp,
                                                   canProfile.CandName,
                                                   jobProfile.CandProfId,
                                                   jobProfile.CreatedDate,
                                                   canStatus.Cscode,
                                                   user.Id,
                                                   user.ProfilePhoto
                                               }).ToListAsync();
                var usrsIds = piplineCandidates.Where(da => da.RecruiterId.HasValue).Select(da => da.RecruiterId.Value).Distinct().ToArray();
                var Usrs = await dbContext.PiHireUsers.Where(da => usrsIds.Contains(da.Id)).Select(da => new { da.Id, da.FirstName, da.LastName, da.ProfilePhoto }).ToListAsync();
                foreach (var profile in piplineCandidates)
                {
                    var recruiter = Usrs.Where(da => da.Id == profile.RecruiterId).FirstOrDefault();
                    var piplineCandidatesViewModel1 = new PiplineCandidatesViewModel
                    {
                        UserId = profile.Id,
                        CandidateStatuName = profile.Title,
                        CandidateStatusId = profile.CandProfStatus,
                        CandidateId = profile.CandProfId,
                        Name = profile.CandName == string.Empty ? profile.CandName : profile.CandName,
                        ProfileAge = GetTimeDiff(profile.CreatedDate),
                        ProfileDate = profile.CreatedDate,
                        StageId = StageId,
                        ProfilePhoto = getCandidatePhotoUrl(profile.ProfilePhoto, profile.Id),
                        isBlockListed = profile.Cscode == "BLT" ? true : false,

                        RecruiterId = profile.RecruiterId,
                        RecruiterName = recruiter == null ? "" : (recruiter.FirstName + " " + recruiter.LastName),
                        RecruiterPhoto = recruiter == null ? "" : getEmployeePhotoUrl(recruiter.ProfilePhoto, recruiter.Id)
                    };
                    model.Add(piplineCandidatesViewModel1);
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, "jobId->" + JobId + ", StageId->" + StageId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }


        public async Task<GetResponseViewModel<List<PiplineCandidatesViewModel>>> GetDashboardJobBDMStageCandidatesAsync(int bdmId, int StageId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<PiplineCandidatesViewModel>>();
            try
            {

                var model = new List<PiplineCandidatesViewModel>();

                var piplineCandidates = await (from canProfile in dbContext.PhCandidateProfiles
                                               join jobProfile in dbContext.PhJobCandidates on canProfile.Id equals jobProfile.CandProfId
                                               join jobAssgn in dbContext.PhJobAssignments on new { AssignedTo = jobProfile.RecruiterId.Value, jobProfile.Joid } equals new { jobAssgn.AssignedTo, jobAssgn.Joid }

                                               join canStatus in dbContext.PhCandStatusSes on jobProfile.CandProfStatus equals canStatus.Id
                                               // join user in dbContext.PiHireUsers on canProfile.EmailId equals user.UserName

                                               join job in dbContext.PhJobOpenings on jobProfile.Joid equals job.Id
                                               where jobProfile.StageId == StageId && job.BroughtBy == bdmId
                                               && jobAssgn.DeassignDate == null && jobAssgn.Status == (byte)RecordStatus.Active
                                               && canStatus.Status != (byte)RecordStatus.Delete
                                               //&& user.Status != (byte)RecordStatus.Delete && user.UserType == (byte)UserType.Candidate
                                               select new
                                               {
                                                   jobProfile.CandProfStatus,
                                                   jobProfile.RecruiterId,
                                                   jobProfile.Joid,
                                                   canStatus.Title,
                                                   canProfile.FullNameInPp,
                                                   canProfile.CandName,
                                                   jobProfile.CandProfId,
                                                   jobProfile.CreatedDate,
                                                   canStatus.Cscode,
                                                   // user.Id,
                                                   //  user.ProfilePhoto
                                               }).ToListAsync();
                var usrsIds = piplineCandidates.Where(da => da.RecruiterId.HasValue).Select(da => da.RecruiterId.Value).Distinct().ToArray();
                var Usrs = await dbContext.PiHireUsers.Where(da => usrsIds.Contains(da.Id)).Select(da => new { da.Id, da.FirstName, da.LastName, da.ProfilePhoto }).ToListAsync();

                foreach (var profile in piplineCandidates)
                {
                    var recruiter = Usrs.Where(da => da.Id == profile.RecruiterId).FirstOrDefault();
                    var piplineCandidatesViewModel1 = new PiplineCandidatesViewModel
                    {
                        //UserId = profile.Id,
                        CandidateStatuName = profile.Title,
                        CandidateStatusId = profile.CandProfStatus,
                        CandidateId = profile.CandProfId,
                        Name = profile.CandName == string.Empty ? profile.CandName : profile.CandName,
                        ProfileAge = GetTimeDiff(profile.CreatedDate),
                        ProfileDate = profile.CreatedDate,
                        StageId = StageId,
                        //ProfilePhoto = getCandidatePhotoUrl(profile.ProfilePhoto, profile.Id),
                        isBlockListed = profile.Cscode == "BLT" ? true : false,

                        RecruiterId = profile.RecruiterId,
                        RecruiterName = recruiter == null ? "" : (recruiter.FirstName + " " + recruiter.LastName),
                        RecruiterPhoto = recruiter == null ? "" : getEmployeePhotoUrl(recruiter.ProfilePhoto, recruiter.Id)
                    };
                    model.Add(piplineCandidatesViewModel1);
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, "StageId->" + StageId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }


        public async Task<GetResponseViewModel<DashboardJobRecruiterStageViewModel>> GetDashboardJobRecruiterStageAsync(int jobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardJobRecruiterStageViewModel>();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: jobId->" + jobId, respModel.Meta.RequestID);

                //var rawDtls = await dbContext.GetDashboardJobRecruiterStageAsync(jobId, Usr.UserTypeId, Usr.Id);
                //var dtls = rawDtls.Select(da => _DashboardJobRecruiterStageViewModel.ToViewModel(da)).ToList();
                //var recIds = rawDtls.Select(da => da.recruiterID).ToArray();
                var jb = await dbContext.PhJobOpenings.Where(da => da.Id == jobId).Select(da => new { da.PostedDate }).FirstOrDefaultAsync();
                var jobAssignments = await dbContext.PhJobAssignments.Where(da => da.Joid == jobId && da.Status != (byte)RecordStatus.Delete)
                    .Select(da => new { da.AssignedTo, da.DeassignDate, da.CreatedDate, da.NoCvsrequired, da.NoOfFinalCvsFilled }).ToListAsync();
                var recIds = jobAssignments.Select(da => da.AssignedTo).ToArray();
                var recrs = await dbContext.PiHireUsers.Where(da => recIds.Contains(da.Id)).Select(da => new { da.Id, da.FirstName, da.LastName, da.ProfilePhoto }).ToListAsync();





                var stages = await dbContext.PhCandStagesSes.Where(x => x.Status != (byte)RecordStatus.Delete).ToListAsync();
                var counters = await dbContext.PhJobCandidates.Where(x => jobId == x.Joid && x.RecruiterId.HasValue && recIds.Contains(x.RecruiterId.Value) && x.Status != (byte)RecordStatus.Delete)
                    .Select(da => new { da.RecruiterId, da.StageId }).ToListAsync();

                List<_DashboardJobRecruiterStageViewModel> dtls = new List<_DashboardJobRecruiterStageViewModel>();
                TimeSpan timeSpent = new TimeSpan();
                foreach (var AssignedTo in jobAssignments.Select(da => da.AssignedTo).Distinct())
                {
                    var dtl = new _DashboardJobRecruiterStageViewModel
                    {
                        PostedDate = jb?.PostedDate ?? CurrentTime,
                        recrPhoto = getEmployeePhotoUrl(recrs.Where(dai => dai.Id == AssignedTo).Select(dai => dai.ProfilePhoto).FirstOrDefault(), AssignedTo),
                        recrName = recrs.Where(dai => dai.Id == AssignedTo).Select(dai => dai.FirstName + " " + dai.LastName).FirstOrDefault(),
                        recruiterID = AssignedTo,
                        //Sourcing = da.Sourcing,
                        //Screening = da.Screening,
                        //Submission = da.Submission,
                        //Interview = da.Interview,
                        //Offered = da.Offered,
                        //Hired = da.Hired
                        NoCvsRequired = jobAssignments.Where(dai => dai.AssignedTo == AssignedTo).Sum(dai => dai.NoCvsrequired ?? 0),
                        NoOfFinalCvsFilled = jobAssignments.Where(dai => dai.AssignedTo == AssignedTo).Sum(dai => dai.NoOfFinalCvsFilled ?? 0)
                    };
                    dtl.JobOpeningStatusCounter = new List<DAL.Models.JobOpeningStatusCounterViewModel>();
                    dtl.DeassignDates = new List<DateTime?>();
                    TimeSpan _timeSpent = new TimeSpan();
                    foreach (var jobAssignment in jobAssignments.Where(da => da.AssignedTo == dtl.recruiterID))
                    {
                        dtl.DeassignDates.Add(jobAssignment.DeassignDate);
                        var tm = (jobAssignment.DeassignDate ?? CurrentTime) - jobAssignment.CreatedDate;
                        _timeSpent = _timeSpent.Add(tm);
                    }
                    dtl.isDeassigned = !dtl.DeassignDates.Where(da => da.HasValue == false).Any();
                    timeSpent = timeSpent.Add(_timeSpent);
                    dtl.timeSpent = _timeSpent;
                    dtl.timeSpentDays = _timeSpent.Days;
                    dtl.timeSpentHours = _timeSpent.Hours;
                    dtl.timeSpentMinutes = _timeSpent.Minutes;

                    foreach (var stats in stages)
                    {
                        dtl.JobOpeningStatusCounter.Add(new DAL.Models.JobOpeningStatusCounterViewModel
                        {
                            StageID = stats.Id,
                            StageColor = stats.ColorCode,
                            StageName = stats.Title,
                            Counter = counters.Where(x => x.RecruiterId == dtl.recruiterID && x.StageId == stats.Id).Count()
                        });
                    }
                    dtls.Add(dtl);
                }
                var jobCloseDt = await dbContext.PhJobOpenings.Where(da => da.Id == jobId).Select(da => da.ClosedDate).FirstOrDefaultAsync();
                TimeSpan timeRemaining = new TimeSpan();
                if (jobCloseDt > CurrentTime)
                {
                    timeRemaining = jobCloseDt - CurrentTime;
                }
                var JobOpeningStatusCounter = new List<DAL.Models.JobOpeningStatusCounterViewModel>();
                foreach (var stats in stages)
                {
                    JobOpeningStatusCounter.Add(new DAL.Models.JobOpeningStatusCounterViewModel
                    {
                        StageID = stats.Id,
                        StageColor = stats.ColorCode,
                        StageName = stats.Title,
                        Counter = counters.Where(x => x.StageId == stats.Id).Count()
                    });
                }
                var totalPages = dtls.Count;

                respModel.Status = true;
                respModel.SetResult(new DashboardJobRecruiterStageViewModel
                {
                    recruiters = dtls,
                    JobOpeningStatusCounter = JobOpeningStatusCounter,
                    totalPages = totalPages,
                    timeSpent = timeSpent,
                    timeSpentDays = timeSpent.Days,
                    timeSpentHours = timeSpent.Hours,
                    timeSpentMinutes = timeSpent.Minutes,
                    timeRemaining = timeRemaining,
                    timeRemainingDays = timeRemaining.Days,
                    timeRemainingHours = timeRemaining.Hours,
                    timeRemainingMinutes = timeRemaining.Minutes
                });
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", jobId->" + jobId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<DashboardRecruiterStatusViewModel>> GetDashboardRecruiterStatusAsync(DashboardFilterViewModel filterViewModel, bool onLeave)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardRecruiterStatusViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardRecruiterStatusViewModel();
                var fmDt = CurrentTime.Date;
                var monthEndDate = getMonthEnd(CurrentTime, false);
                var rawDtls = await dbContext.GetDashboardRecruiterStatusAsync(CurrentTime, fmDt, monthEndDate, onLeave, Usr.UserTypeId, Usr.Id);
                {
                    var countryIds = rawDtls.Where(da => da.jobCountryId.HasValue).Select(da => da.jobCountryId).Distinct().ToArray();
                    var cityIds = rawDtls.Where(da => da.jobCityId.HasValue).Select(da => da.jobCityId).Distinct().ToArray();

                    var countries = await dbContext.PhCountries.AsNoTracking().Where(da => countryIds.Contains(da.Id)).ToDictionaryAsync(da => da.Id, da2 => da2.Name);
                    var cities = await dbContext.PhCities.AsNoTracking().Where(da => cityIds.Contains(da.Id)).ToDictionaryAsync(da => da.Id, da2 => da2.Name);


                    //var recruiterTotCnt = await dbContext.GetDashboardRecruiterStatusCountAsync(fmDt, monthEndDate, onLeave, Usr.UserTypeId, Usr.Id);
                    var onleaveEmps = await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.Status == (byte)LeaveStatus.Accepted && da.LeaveStartDate.Date <= CurrentDate && CurrentDate <= da.LeaveEndDate.Date).Select(da => da.EmpId).ToArrayAsync();
                    int[] recruiterIds = null;
                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {
                                var qry = dbContext.PiHireUsers.AsNoTracking();
                                recruiterIds = await qry.Where(da => da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.Recruiter && (onleaveEmps.Contains(da.Id) == onLeave)).Select(da => da.Id).ToArrayAsync();
                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == UserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync();

                                var qry = dbContext.PiHireUsers.AsNoTracking();
                                recruiterIds = await qry.Where(da => da.Status == (byte)RecordStatus.Active && allowedUsers.Contains(da.Id) && da.UserType == (byte)UserType.Recruiter && (onleaveEmps.Contains(da.Id) == onLeave)).Select(da => da.Id).ToArrayAsync();
                            }
                            break;
                        case UserType.BDM:
                            {
                                recruiterIds = rawDtls.Where(da => da.recruiterID.HasValue).Select(da => da.recruiterID.Value).Distinct().ToArray();

                                var qry = dbContext.PiHireUsers.AsNoTracking();
                                recruiterIds = await qry.Where(da => da.Status == (byte)RecordStatus.Active && recruiterIds.Contains(da.Id) && da.UserType == (byte)UserType.Recruiter && (onleaveEmps.Contains(da.Id) == onLeave)).Select(da => da.Id).ToArrayAsync();
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                var qry = dbContext.PiHireUsers.AsNoTracking();
                                recruiterIds = await qry.Where(da => da.Id == UserId && da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.Recruiter && (onleaveEmps.Contains(da.Id) == onLeave)).Select(da => da.Id).ToArrayAsync();
                            }
                            break;
                        default:
                            break;
                    }

                    //var recruiterIds = rawDtls.Where(da => da.recruiterID.HasValue).Select(da => da.recruiterID.Value).Distinct().ToArray();
                    var recruiters = await dbContext.PiHireUsers.Where(da => recruiterIds.Contains(da.Id))
                        .Select(da => new { da.FirstName, da.LastName, da.Id, da.UserRoleName, da.Location, da.ProfilePhoto }).ToListAsync();
                    var pus = await dbContext.VwUserPuBus.Where(da => recruiterIds.Contains(da.UserId)).Select(da => new { da.UserId, da.ProcessUnit }).ToListAsync();

                    var weekEndDate = getWeekEnd(CurrentTime, false);

                    //Job count
                    var recrJobs = await dbContext.PhJobAssignments.AsNoTracking().Where(da => da.DeassignDate.HasValue == false && recruiterIds.Contains(da.AssignedTo) && da.Status != (byte)RecordStatus.Delete)
                        .Select(da => new { da.AssignedTo, da.Joid }).Distinct().ToListAsync();
                    var recrJobsIds = recrJobs.Select(da => da.Joid).ToArray();
                    var jobStatusClosedId = await dbContext.PhJobStatusSes.AsNoTracking().Where(da => da.Jscode == "CLS" && da.Status != (byte)RecordStatus.Delete).Select(da => da.Id).FirstOrDefaultAsync();
                    var closedJobIds = await dbContext.PhJobOpenings.AsNoTracking().Where(da => recrJobsIds.Contains(da.Id) && da.JobOpeningStatus == jobStatusClosedId).Select(da => da.Id).ToArrayAsync();
                    var recruiterJobCnt = recrJobs.Where(da => closedJobIds.Contains(da.Joid) == false).GroupBy(da => da.AssignedTo).ToDictionary(da => da.Key, da2 => da2.LongCount());

                    foreach (var recruiterId in recruiterIds)
                    {
                        var _GetWorkScheduleDtls = await GetWorkScheduleDtls(recruiterId, CurrentDate);
                        model.recruiters.Add(new _DashboardRecruiterStatusViewModel
                        {
                            id = recruiterId,
                            name = recruiters.Where(da => da.Id == recruiterId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                            profilePhoto = getEmployeePhotoUrl(recruiters.Where(da => da.Id == recruiterId).Select(da => da.ProfilePhoto).FirstOrDefault(), recruiterId),
                            role = recruiters.Where(da => da.Id == recruiterId).Select(da => da.UserRoleName).FirstOrDefault(),
                            location = recruiters.Where(da => da.Id == recruiterId).Select(da => da.Location).FirstOrDefault(),
                            puIds = pus.Where(da => da.UserId == recruiterId).Select(da => da.ProcessUnit).Distinct().ToList(),
                            todayCvsRequired = rawDtls.Where(da => da.recruiterID == recruiterId && da.ClosedDate.Date == CurrentTime.Date).Sum(da => da.NoCVSRequired ?? 0),
                            tommorrowCvsRequired = rawDtls.Where(da => da.recruiterID == recruiterId && da.ClosedDate.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoCVSRequired ?? 0),
                            weekCvsRequired = rawDtls.Where(da => da.recruiterID == recruiterId && da.ClosedDate.Date <= weekEndDate).Sum(da => da.NoCVSRequired ?? 0),
                            monthCvsRequired = rawDtls.Where(da => da.recruiterID == recruiterId && da.ClosedDate.Date <= monthEndDate).Sum(da => da.NoCVSRequired ?? 0),

                            todayCvsFilled = rawDtls.Where(da => da.recruiterID == recruiterId && da.ClosedDate.Date == CurrentTime.Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                            tommorrowCvsFilled = rawDtls.Where(da => da.recruiterID == recruiterId && da.ClosedDate.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                            weekCvsFilled = rawDtls.Where(da => da.recruiterID == recruiterId && da.ClosedDate.Date <= weekEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                            monthCvsFilled = rawDtls.Where(da => da.recruiterID == recruiterId && da.ClosedDate.Date <= monthEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),

                            jobs = rawDtls.Where(da => da.recruiterID == recruiterId).Select(da => __DashboardRecruiterStatusViewModel.toViewModel(da, countries, cities)).ToList(),
                            //onLeave = onleaveEmps.Contains(recruiterId),
                            onLeave = _GetWorkScheduleDtls.isOnLeave,
                            isWeekEnd = _GetWorkScheduleDtls.isWeekEnd,
                            isShiftExist = _GetWorkScheduleDtls.isShiftExist,
                            LeaveTypeName = _GetWorkScheduleDtls.LeaveHolidayName,

                            totalActiveJobCount = recruiterJobCnt.ContainsKey(recruiterId) ? recruiterJobCnt[recruiterId] : 0
                        });
                    }
                    //model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(recruiterTotCnt?.TotCnt ?? 0) / filterViewModel.PerPage) : recruiterIds.Length;
                    model.totalPages = recruiterIds.Length;
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        async Task<(bool isShiftExist, bool isHireStrt, bool isOnLeave, bool isWeekEnd, string LeaveHolidayName)> GetWorkScheduleDtls(int userId, DateTime srchDt)
        {
            (bool isShiftExist, bool isHireStrt, bool isOnLeave, bool isWeekEnd, string LeaveHolidayName) respModel = (false, false, false, false, "");
            DateTime hireStrtDt = DateTime.ParseExact(this.appSettings.AppSettingsProperties.GatewayMigratedToHireOn, @"d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);// Hire Start Date
            if (hireStrtDt < srchDt)
            {
                respModel.isHireStrt = true;
                try
                {
                    var shiftId = await dbContext.PiHireUsers.AsNoTracking().Where(da => da.Id == userId).Select(da => da.ShiftId).FirstOrDefaultAsync();

                    var shiftDetl = await dbContext.PhShiftDetls.AsNoTracking().Where(da => da.ShiftId == shiftId && da.Status != (byte)RecordStatus.Delete)
                                            .Select(da => new { da.IsWeekend, da.IsAlternateWeekend, da.DayName, da.WeekendModel }).ToListAsync();

                    if (shiftDetl.Count > 0)
                    {
                        respModel.isShiftExist = true;

                        {
                            var Weekends = shiftDetl.Where(da => da.IsWeekend == true && da.IsAlternateWeekend == false).Select(x => x.DayName).ToList();
                            var Weekend = Weekends.FirstOrDefault(wEnd => srchDt.DayOfWeek.ToString() == wEnd);
                            if (string.IsNullOrEmpty(Weekend) == false)
                            {
                                respModel.isWeekEnd = true;
                                respModel.isOnLeave = false;
                                respModel.LeaveHolidayName = $"weekend - {Weekend}";
                            }
                        }
                        if (respModel.isWeekEnd != true)
                        {
                            var alternativeWeekends = shiftDetl.Where(da => da.IsWeekend == true && da.IsAlternateWeekend == true).Select(x => new SubClassWeekend { WeekendModel = x.WeekendModel, DayName = x.DayName }).ToList();
                            if (CheckAlternativeWeekend(srchDt.Date, alternativeWeekends))
                            {
                                respModel.isOnLeave = false;
                                respModel.isWeekEnd = false;

                                var accptLeaveObj = await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.EmpId == userId && da.LeaveStartDate.Date <= srchDt.Date && srchDt.Date <= da.LeaveEndDate.Date && da.Status == (byte)LeaveStatus.Accepted)
                                    .Join(dbContext.PhRefMasters.AsNoTracking(), da => da.LeaveType, da2 => da2.Id, (da, da2) => new { da2.Rmvalue }).FirstOrDefaultAsync();

                                if (accptLeaveObj != null)
                                {
                                    respModel.LeaveHolidayName = accptLeaveObj.Rmvalue;
                                    respModel.isOnLeave = true;
                                }
                            }
                            else
                            {
                                respModel.isWeekEnd = true;
                                respModel.isOnLeave = false;
                                respModel.LeaveHolidayName = $"weekend-{srchDt.DayOfWeek.ToString()}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.Other, $"userId:{userId}, srchDt:{srchDt}", string.Empty, ex);
                }
            }
            return respModel;
        }
        async Task<(HashSet<int> weekOffEmpIds, HashSet<int> leaveEmpIds)> getOffEmpIdsAsync(DateTime srchDt)
        {
            HashSet<int> WeekoffEmpIds = new HashSet<int>();
            HashSet<int> leaveEmpIds = new HashSet<int>();
            DateTime hireStrtDt = DateTime.ParseExact(this.appSettings.AppSettingsProperties.GatewayMigratedToHireOn, @"d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);// Hire Start Date
            if (hireStrtDt < srchDt)
            {
                try
                {
                    var availUserTypes = (new[] { UserType.SuperAdmin, UserType.Admin, UserType.BDM, UserType.Recruiter }).Select(da => (byte)da).ToHashSet();
                    var userShifts = await dbContext.PiHireUsers.AsNoTracking().Where(da => da.Status != (byte)RecordStatus.Delete && da.ShiftId.HasValue && availUserTypes.Contains(da.UserType)).Select(da => new { da.Id, da.ShiftId }).ToListAsync();
                    var shiftIds = userShifts.Select(da => da.ShiftId).Distinct().ToHashSet();

                    var shiftDtls = await dbContext.PhShiftDetls.AsNoTracking().Where(da => shiftIds.Contains(da.ShiftId) && da.Status != (byte)RecordStatus.Delete)
                        .Select(da => new { da.ShiftId, da.IsWeekend, da.IsAlternateWeekend, da.DayName, da.WeekendModel }).ToListAsync();
                    foreach (var userId in userShifts.Select(da => da.Id).Distinct())
                    {
                        var shiftDetl = userShifts.Where(da => da.Id == userId).Join(shiftDtls, da => da.ShiftId, da2 => da2.ShiftId, (da, da2) => da2).ToList();
                        if (shiftDetl.Count > 0)
                        {
                            var Weekends = shiftDetl.Where(da => da.IsWeekend == true && da.IsAlternateWeekend == false).Select(x => x.DayName).ToList();
                            var Weekend = Weekends.FirstOrDefault(wEnd => srchDt.DayOfWeek.ToString() == wEnd);
                            if (string.IsNullOrEmpty(Weekend) == false)
                            {
                                WeekoffEmpIds.Add(userId);
                                continue;
                            }
                            else
                            {
                                var alternativeWeekends = shiftDetl.Where(da => da.IsWeekend == true && da.IsAlternateWeekend == true).Select(x => new SubClassWeekend { WeekendModel = x.WeekendModel, DayName = x.DayName }).ToList();
                                if (CheckAlternativeWeekend(srchDt.Date, alternativeWeekends))
                                {
                                    var accptLeaveObj = await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.EmpId == userId && da.LeaveStartDate.Date <= srchDt.Date && srchDt.Date <= da.LeaveEndDate.Date && da.Status == (byte)LeaveStatus.Accepted)
                                        .Join(dbContext.PhRefMasters.AsNoTracking(), da => da.LeaveType, da2 => da2.Id, (da, da2) => new { da2.Rmvalue }).FirstOrDefaultAsync();

                                    if (accptLeaveObj != null)
                                    {
                                        leaveEmpIds.Add(userId);
                                        continue;
                                    }
                                }
                                else
                                {
                                    WeekoffEmpIds.Add(userId);
                                    continue;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.Other, $"getWeekoffEmpIdsAsync - srchDt:{srchDt}", string.Empty, ex);
                }
            }
            return (WeekoffEmpIds, leaveEmpIds);
        }

        public async Task<GetResponseViewModel<DashboardRecruiterDaywiseViewModel>> GetDashboardRecruiterDaywiseAsync(DashboardRecruiterDaywiseFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardRecruiterDaywiseViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardRecruiterDaywiseViewModel();
                {
                    var offEmps = await getOffEmpIdsAsync(CurrentDate);
                    var onWeekOffEmps = offEmps.weekOffEmpIds;
                    //var onleaveEmps = await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.Status == (byte)LeaveStatus.Accepted && da.LeaveStartDate.Date <= CurrentDate && CurrentDate <= da.LeaveEndDate.Date).Select(da => da.EmpId).ToArrayAsync();
                    var onleaveEmps = offEmps.leaveEmpIds;

                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.Recruiter);
                    if (filterViewModel.OnLeave.HasValue)
                    {
                        //hireUsrQry = hireUsrQry.Where(da => onleaveEmps.Contains(da.Id) == filterViewModel.OnLeave);
                        if (filterViewModel.OnLeave == true)
                            hireUsrQry = hireUsrQry.Where(da => onleaveEmps.Contains(da.Id));
                        else
                            hireUsrQry = hireUsrQry.Where(da => onleaveEmps.Contains(da.Id) == false && onWeekOffEmps.Contains(da.Id) == false);
                    }
                    if (filterViewModel.RecruiterId.HasValue)
                    {
                        hireUsrQry = hireUsrQry.Where(da => da.Id == filterViewModel.RecruiterId);
                    }
                    if (filterViewModel.JoId.HasValue)
                    {
                        var AssignedTos = dbContext.PhJobAssignments.Where(da => da.Joid == filterViewModel.JoId && da.Status == (byte)RecordStatus.Active && da.DeassignDate.HasValue == false).Select(da => da.AssignedTo).ToHashSet();
                        hireUsrQry = hireUsrQry.Where(da => AssignedTos.Contains(da.Id));
                    }

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                var allowedUsers = (await dbContext.VwDashboardDaywiseFilterData.Where(da => da.BdmId == loginUserId).Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                hireUsrQry = hireUsrQry.Where(da => da.Id == loginUserId);
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    model.LocationwiseCounts = await hireUsrQry.GroupBy(da => da.LocationId).Select(da => new LocationwiseCountDaywiseViewModel { LocationId = da.Key, Count = da.Count() }).ToListAsync();
                    if (filterViewModel.LocationId.HasValue)
                    {
                        hireUsrQry = hireUsrQry.Where(da => da.LocationId == filterViewModel.LocationId);
                    }
                    model.totalCount = await hireUsrQry.CountAsync();

                    if (filterViewModel.CurrentPage < 1)
                    {
                        filterViewModel.CurrentPage = 1;
                    }
                    if (filterViewModel.PerPage < 0)
                    {
                        filterViewModel.PerPage = 10;
                    }
                    model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                    int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                    int[] recruiterIds = await hireUsrQry.OrderBy(da => da.FirstName + " " + da.LastName).Select(da => da.Id).Skip(skip).Take(filterViewModel.PerPage).ToArrayAsync();

                    var recruiters = await dbContext.PiHireUsers.Where(da => recruiterIds.Contains(da.Id))
                        .Select(da => new { da.FirstName, da.LastName, da.Id, da.UserRoleName, da.Location, da.ProfilePhoto }).ToListAsync();
                    var pus = await dbContext.VwUserPuBus.Where(da => recruiterIds.Contains(da.UserId)).Select(da => new { da.UserId, da.ProcessUnit }).ToListAsync();

                    var fmDt = CurrentTime.Date;
                    var monthEndDate = getMonthEnd(CurrentTime, false);
                    var weekEndDate = getWeekEnd(CurrentTime, false);

                    var rawDtls = await dbContext.GetDashboardDaywiseFilterAsync(CurrentTime, (int)UserType.Recruiter, filterViewModel.OnLeave, filterViewModel.LocationId, filterViewModel.JoId, recruiterIds, Usr.UserTypeId, Usr.Id);

                    ////Job count
                    var recrJobs = await dbContext.PhJobAssignments.AsNoTracking().Where(da => da.DeassignDate.HasValue == false && recruiterIds.Contains(da.AssignedTo) && da.Status == (byte)RecordStatus.Active)
                        .Select(da => new { da.AssignedTo, da.Joid }).Distinct().ToListAsync();
                    //var recrJobsIds = recrJobs.Select(da => da.Joid).ToArray();
                    //var recrJobAssignDaywise = await dbContext.PhJobAssignmentsDayWises.AsNoTracking().Where(da => recrJobsIds.Contains(da.Joid) && recruiterIds.Contains(da.AssignedTo) && da.Status != (byte)RecordStatus.Delete)
                    //    .Select(da => new { da.CreatedDate, da.AssignedTo, da.Joid, da.NoCvsrequired, da.NoOfFinalCVsFilled }).Distinct().ToListAsync();
                    //var jobStatusClosedId = await dbContext.PhJobStatusSes.AsNoTracking().Where(da => da.Jscode == "CLS" && da.Status != (byte)RecordStatus.Delete).Select(da => da.Id).FirstOrDefaultAsync();
                    //var closedJobIds = await dbContext.PhJobOpenings.AsNoTracking().Where(da => recrJobsIds.Contains(da.Id) && da.JobOpeningStatus == jobStatusClosedId).Select(da => da.Id).ToArrayAsync();
                    //var recruiterJobCnt = recrJobs.Where(da => closedJobIds.Contains(da.Joid) == false).GroupBy(da => da.AssignedTo).ToDictionary(da => da.Key, da2 => da2.LongCount());

                    foreach (var recruiterId in recruiterIds)
                    {
                        var _GetWorkScheduleDtls = await GetWorkScheduleDtls(recruiterId, CurrentDate);
                        var activeJobIds = recrJobs.Where(da => da.AssignedTo == recruiterId).Select(da => da.Joid).Distinct().ToHashSet();
                        var _rawDtls = rawDtls.Where(da => da.recruiterID == recruiterId && activeJobIds.Contains(da.jobId));
                        model.recruiters.Add(new _DashboardRecruiterDaywiseViewModel
                        {
                            id = recruiterId,
                            name = recruiters.Where(da => da.Id == recruiterId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                            profilePhoto = getEmployeePhotoUrl(recruiters.Where(da => da.Id == recruiterId).Select(da => da.ProfilePhoto).FirstOrDefault(), recruiterId),
                            role = recruiters.Where(da => da.Id == recruiterId).Select(da => da.UserRoleName).FirstOrDefault(),
                            location = recruiters.Where(da => da.Id == recruiterId).Select(da => da.Location).FirstOrDefault(),
                            puIds = pus.Where(da => da.UserId == recruiterId).Select(da => da.ProcessUnit).Distinct().ToList(),

                            todayCvsRequired = _rawDtls.Where(da => da.recruiterID == recruiterId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoCVSRequired ?? 0),
                            tommorrowCvsRequired = _rawDtls.Where(da => da.recruiterID == recruiterId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoCVSRequired ?? 0),
                            weekCvsRequired = _rawDtls.Where(da => da.recruiterID == recruiterId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoCVSRequired ?? 0),
                            monthCvsRequired = _rawDtls.Where(da => da.recruiterID == recruiterId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoCVSRequired ?? 0),
                            cumulativeCvsRequired = _rawDtls.Where(da => da.recruiterID == recruiterId).Sum(da => da.NoCVSRequired ?? 0),

                            todayCvsFilled = _rawDtls.Where(da => da.recruiterID == recruiterId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                            tommorrowCvsFilled = _rawDtls.Where(da => da.recruiterID == recruiterId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                            weekCvsFilled = _rawDtls.Where(da => da.recruiterID == recruiterId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                            monthCvsFilled = _rawDtls.Where(da => da.recruiterID == recruiterId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                            cumulativeCvsFilled = _rawDtls.Where(da => da.recruiterID == recruiterId).Sum(da => da.NoOfFinalCVsFilled ?? 0),

                            //jobs = _rawDtls.Where(da => da.recruiterID == recruiterId).Select(da => __DashboardRecruiterStatusViewModel.toViewModel(da, countries, cities)).ToList(),
                            //onLeave = onleaveEmps.Contains(recruiterId),
                            onLeave = _GetWorkScheduleDtls.isOnLeave,
                            isWeekEnd = _GetWorkScheduleDtls.isWeekEnd,
                            isShiftExist = _GetWorkScheduleDtls.isShiftExist,
                            LeaveTypeName = _GetWorkScheduleDtls.LeaveHolidayName,
                            totalActiveJobCount = activeJobIds.Count()
                        });
                    }
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<DashboardBdmDaywiseViewModel>> GetDashboardBdmDaywisePipelineAsync(DashboardRecruiterDaywiseFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardBdmDaywiseViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardBdmDaywiseViewModel();
                {
                    var offEmps = await getOffEmpIdsAsync(CurrentDate);
                    var onWeekOffEmps = offEmps.weekOffEmpIds;
                    //var onleaveEmps = await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.Status == (byte)LeaveStatus.Accepted && da.LeaveStartDate.Date <= CurrentDate && CurrentDate <= da.LeaveEndDate.Date).Select(da => da.EmpId).ToArrayAsync();
                    var onleaveEmps = offEmps.leaveEmpIds;

                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.BDM);
                    if (filterViewModel.OnLeave.HasValue)
                    {
                        //hireUsrQry = hireUsrQry.Where(da => onleaveEmps.Contains(da.Id) == filterViewModel.OnLeave);

                        if (filterViewModel.OnLeave == true)
                            hireUsrQry = hireUsrQry.Where(da => onleaveEmps.Contains(da.Id));
                        else
                            hireUsrQry = hireUsrQry.Where(da => onleaveEmps.Contains(da.Id) == false && onWeekOffEmps.Contains(da.Id) == false);
                    }
                    if (filterViewModel.RecruiterId.HasValue)
                    {
                        hireUsrQry = hireUsrQry.Where(da => da.Id == filterViewModel.RecruiterId);
                    }
                    if (filterViewModel.JoId.HasValue)
                    {
                        var AssignedTos = dbContext.PhJobAssignments.Where(da => da.Joid == filterViewModel.JoId && da.Status == (byte)RecordStatus.Active && da.DeassignDate.HasValue == false).Select(da => da.AssignedTo).ToHashSet();
                        hireUsrQry = hireUsrQry.Where(da => AssignedTos.Contains(da.Id));
                    }

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                //var allowedUsers = (await dbContext.vwDashboardDaywiseFilterDatas.Where(da => da.BdmId == loginUserId).Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                //hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                                hireUsrQry = hireUsrQry.Where(da => da.Id == loginUserId);
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                //hireUsrQry = hireUsrQry.Where(da => da.Id == loginUserId);
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    model.LocationwiseCounts = await hireUsrQry.GroupBy(da => da.LocationId).Select(da => new LocationwiseCountDaywiseViewModel { LocationId = da.Key, Count = da.Count() }).ToListAsync();
                    if (filterViewModel.LocationId.HasValue)
                    {
                        hireUsrQry = hireUsrQry.Where(da => da.LocationId == filterViewModel.LocationId);
                    }
                    model.totalCount = await hireUsrQry.CountAsync();

                    if (filterViewModel.CurrentPage < 1)
                    {
                        filterViewModel.CurrentPage = 1;
                    }
                    if (filterViewModel.PerPage < 0)
                    {
                        filterViewModel.PerPage = 10;
                    }
                    model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                    int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                    int[] bdmIds = await hireUsrQry.OrderBy(da => da.FirstName + " " + da.LastName).Select(da => da.Id).Skip(skip).Take(filterViewModel.PerPage).ToArrayAsync();

                    var bdms = await dbContext.PiHireUsers.Where(da => bdmIds.Contains(da.Id))
                        .Select(da => new { da.FirstName, da.LastName, da.Id, da.UserRoleName, da.Location, da.ProfilePhoto }).ToListAsync();
                    var pus = await dbContext.VwUserPuBus.Where(da => bdmIds.Contains(da.UserId)).Select(da => new { da.UserId, da.ProcessUnit }).ToListAsync();

                    var fmDt = CurrentTime.Date;
                    var monthEndDate = getMonthEnd(CurrentTime, false);
                    var weekEndDate = getWeekEnd(CurrentTime, false);

                    var rawDtls = await dbContext.GetDashboardDaywiseFilterAsync(CurrentTime, (int)UserType.BDM, filterViewModel.OnLeave, filterViewModel.LocationId, filterViewModel.JoId, bdmIds, Usr.UserTypeId, Usr.Id);

                    var bdmJobsIds = rawDtls.Select(da => da.jobId).Distinct().ToHashSet();
                    var stages = await dbContext.PhCandStagesSes.Where(da => da.Status != (byte)RecordStatus.Delete).ToListAsync();
                    var counters = await dbContext.PhJobOpeningStatusCounters.Where(da => bdmJobsIds.Contains(da.Joid) && da.Status != (byte)RecordStatus.Delete)
                        .Select(da => new { da.Joid, da.StageId, da.Counter }).ToListAsync();

                    var recrJobs = await dbContext.PhJobAssignments.AsNoTracking().Where(da => da.DeassignDate.HasValue == false && bdmJobsIds.Contains(da.Joid) && da.Status == (byte)RecordStatus.Active)
                        .Select(da => new { da.AssignedTo, da.Joid }).Distinct().ToListAsync();
                    foreach (var bdmId in bdmIds)
                    {
                        var _GetWorkScheduleDtls = await GetWorkScheduleDtls(bdmId, CurrentDate);
                        var _rawDtls = rawDtls.Join(recrJobs, da => new { recId = da.recruiterID, joId = da.jobId }, da2 => new { recId = da2.AssignedTo, joId = da2.Joid }, (da, da2) => da);
                        var dtl = new _DashboardBdmRecruiterStatusViewModel
                        {
                            id = bdmId,
                            name = bdms.Where(da => da.Id == bdmId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                            profilePhoto = getEmployeePhotoUrl(bdms.Where(da => da.Id == bdmId).Select(da => da.ProfilePhoto).FirstOrDefault(), bdmId),
                            role = bdms.Where(da => da.Id == bdmId).Select(da => da.UserRoleName).FirstOrDefault(),
                            location = bdms.Where(da => da.Id == bdmId).Select(da => da.Location).FirstOrDefault(),
                            puIds = pus.Where(da => da.UserId == bdmId).Select(da => da.ProcessUnit).Distinct().ToList(),

                            todayCvsRequired = _rawDtls.Where(da => da.bdmId == bdmId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoCVSRequired ?? 0),
                            tommorrowCvsRequired = _rawDtls.Where(da => da.bdmId == bdmId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoCVSRequired ?? 0),
                            weekCvsRequired = _rawDtls.Where(da => da.bdmId == bdmId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoCVSRequired ?? 0),
                            monthCvsRequired = _rawDtls.Where(da => da.bdmId == bdmId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoCVSRequired ?? 0),

                            todayCvsFilled = _rawDtls.Where(da => da.bdmId == bdmId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                            tommorrowCvsFilled = _rawDtls.Where(da => da.bdmId == bdmId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                            weekCvsFilled = _rawDtls.Where(da => da.bdmId == bdmId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                            monthCvsFilled = _rawDtls.Where(da => da.bdmId == bdmId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),

                            //onLeave = onleaveEmps.Contains(bdmId),
                            onLeave = _GetWorkScheduleDtls.isOnLeave,
                            isWeekEnd = _GetWorkScheduleDtls.isWeekEnd,
                            isShiftExist = _GetWorkScheduleDtls.isShiftExist,
                            LeaveTypeName = _GetWorkScheduleDtls.LeaveHolidayName,

                            totalActiveJobCount = _rawDtls.Where(da => da.bdmId == bdmId).Select(da => da.jobId).Distinct().Count()
                        };

                        dtl.JobOpeningStatusCounter = new List<JobOpeningStatusCounterViewModel>();
                        foreach (var stats in stages)
                        {
                            dtl.JobOpeningStatusCounter.Add(new JobOpeningStatusCounterViewModel
                            {
                                StageID = stats.Id,
                                StageColor = stats.ColorCode,
                                StageName = stats.Title,
                                Counter = _rawDtls.Where(da => da.bdmId == bdmId).Select(da => da.jobId).Distinct().Join(counters.Where(dai => dai.StageId == stats.Id), da => da, da2 => da2.Joid, (da, da2) => da2.Counter).Sum()
                            });
                        }

                        model.bdms.Add(dtl);
                    }
                    //model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(recruiterTotCnt?.TotCnt ?? 0) / filterViewModel.PerPage) : bdmIds.Length;
                    //model.totalPages = bdmIds.Length;
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<DashboardJobsDaywisePipelineViewModel>> GetDashboardJobsDaywisePipelineAsync(UserType userType, int userId, DashboardFilterPaginationViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardJobsDaywisePipelineViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardJobsDaywisePipelineViewModel();
                {
                    var onleaveEmps = await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.Status == (byte)LeaveStatus.Accepted && da.LeaveStartDate.Date <= CurrentDate && CurrentDate <= da.LeaveEndDate.Date).Select(da => da.EmpId).ToArrayAsync();

                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && da.UserType == (byte)userType && da.Id == userId);

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/ && da.UserId == userId).Select(da => da.UserId).ToArrayAsync();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                    if (userType == UserType.BDM)
                                        allowedUsers = (await qry.Where(da => da.BdmId > 0).Select(da => da.BdmId.Value).Distinct().ToListAsync()).ToHashSet();
                                    else
                                        allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.RecruiterId == loginUserId);
                                    if (userType == UserType.BDM)
                                        allowedUsers = (await qry.Where(da => da.BdmId > 0).Select(da => da.BdmId.Value).Distinct().ToListAsync()).ToHashSet();
                                    else
                                        allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    var userIds = await hireUsrQry.OrderBy(da => da.FirstName + " " + da.LastName).Select(da => da.Id).ToArrayAsync();

                    {
                        var rawDtls = await dbContext.GetDashboardDaywiseFilterAsync(CurrentTime, (int)userType, null, null, null, userIds, Usr.UserTypeId, Usr.Id);

                        var jobIds = rawDtls.Select(da => da.jobId).Distinct().ToHashSet();
                        var recIds = rawDtls.Where(da => da.recruiterID > 0).Select(da => da.recruiterID).ToArray();
                        var assigns = await dbContext.PhJobAssignments.Where(da => da.Status == (byte)RecordStatus.Active && da.DeassignDate.HasValue == false && jobIds.Contains(da.Joid) && recIds.Contains(da.AssignedTo))
                                                .Select(da => new { da.Joid, da.AssignedTo, jobAssignmentDate = (da.ReassignDate ?? da.CreatedDate) }).ToListAsync();

                        var lst = rawDtls.Join(assigns, da => new { recId = da.recruiterID, joId = da.jobId }, da2 => new { recId = da2.AssignedTo, joId = da2.Joid }, (da, da2) => da).GroupBy(da => new { da.jobId, da.recruiterID }).Select(da => new { da.Key.jobId, da.Key.recruiterID });
                        model.totalCount = lst.Count();

                        if (filterViewModel.CurrentPage < 1)
                        {
                            filterViewModel.CurrentPage = 1;
                        }
                        if (filterViewModel.PerPage < 0)
                        {
                            filterViewModel.PerPage = 10;
                        }
                        model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                        int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                        lst = lst.Skip(skip).Take(filterViewModel.PerPage);
                        jobIds = lst.Select(da => da.jobId).Distinct().ToHashSet();

                        var activities = await dbContext.PhActivityLogs.Where(da => da.Joid.HasValue && jobIds.Contains(da.Joid.Value) && da.ActivityMode == (byte)ActivityOn.Opening && (da.ActivityType == (byte)LogActivityType.RecordUpdates || da.ActivityType == (byte)LogActivityType.JobEditUpdates))
                                                        .GroupBy(da => da.Joid).Select(da => da.Max(dai => dai.Id))
                                                        .Join(dbContext.PhActivityLogs, da => da, da2 => da2.Id, (da, da2) => new { da2.CreatedDate, da2.CreatedBy }).ToListAsync();

                        recIds = rawDtls.Where(da => jobIds.Contains(da.jobId) && da.recruiterID > 0).Select(da => da.recruiterID).ToArray();
                        var bdmIds = rawDtls.Where(da => jobIds.Contains(da.jobId) && da.bdmId > 0).Select(da => da.bdmId.Value).ToArray();

                        var usrIds = activities.Select(da => da.CreatedBy).Union(recIds).Union(bdmIds).ToHashSet();
                        var usrs = await dbContext.PiHireUsers.Where(da => usrIds.Contains(da.Id))
                                            .Select(da => new { da.FirstName, da.LastName, da.Id, da.ProfilePhoto }).ToListAsync();


                        var jobDtls = await dbContext.PhJobOpenings.Where(da => da.Status != (byte)RecordStatus.Delete && jobIds.Contains(da.Id))
                                                .Select(da => new { da.Id, da.JobTitle, da.ClientName, da.PostedDate, da.ClosedDate, da.JobLocationId, da.CountryId }).ToListAsync();

                        var jobAddDtls = await dbContext.PhJobOpeningsAddlDetails.Where(da => jobIds.Contains(da.Joid))
                                                .Select(da => new { da.Joid, da.Puid, da.Buid }).ToListAsync();

                        var recUsrs = usrs.Where(da => recIds.Contains(da.Id)).ToList();
                        var bdmUsrs = usrs.Where(da => bdmIds.Contains(da.Id)).ToList();

                        var countryIds = jobDtls.Where(rawDtl => rawDtl.CountryId > 0).Select(rawDtl => rawDtl.CountryId).Distinct().ToArray();
                        var cityIds = jobDtls.Where(rawDtl => rawDtl.JobLocationId > 0).Select(rawDtl => rawDtl.JobLocationId).Distinct().ToArray();

                        var countries = await dbContext.PhCountries.AsNoTracking().Where(rawDtl => countryIds.Contains(rawDtl.Id)).ToDictionaryAsync(rawDtl => rawDtl.Id, da2 => da2.Name);
                        var cities = await dbContext.PhCities.AsNoTracking().Where(rawDtl => cityIds.Contains(rawDtl.Id)).ToDictionaryAsync(rawDtl => rawDtl.Id, da2 => da2.Name);

                        var stages = await dbContext.PhCandStagesSes.Where(da => da.Status != (byte)RecordStatus.Delete).ToListAsync();
                        var counters = await dbContext.PhJobOpeningStatusCounters.Where(da => jobIds.Contains(da.Joid) && da.Status != (byte)RecordStatus.Delete)
                            .Select(da => new { da.Joid, da.StageId, da.Counter }).ToListAsync();


                        var fmDt = CurrentTime.Date;
                        var monthEndDate = getMonthEnd(CurrentTime, false);
                        var weekEndDate = getWeekEnd(CurrentTime, false);
                        foreach (var rawDtl in lst)
                        {
                            var bdm = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID)
                                .Join(bdmUsrs, da => da.bdmId, da2 => da2.Id, (da, da2) => da2);
                            var _jobDtls = jobDtls.Where(da => da.Id == rawDtl.jobId);
                            var jobAssignDtls = assigns.FirstOrDefault(da => da.AssignedTo == rawDtl.recruiterID && da.Joid == rawDtl.jobId);

                            var activity = activities.Join(usrs, da => da.CreatedBy, da2 => da2.Id, (da, da2) => new { da.CreatedDate, da2.FirstName, da2.LastName });
                            var dtl = new DashboardJobsDaywisePipelineViewModel_job
                            {
                                jobId = rawDtl.jobId,
                                ClientName = _jobDtls.Select(da => da.ClientName).FirstOrDefault(),
                                //ClosedDate = rawDtl.ClosedDate,
                                //PostedDate = rawDtl.PostedDate,
                                JobTitle = _jobDtls.Select(da => da.JobTitle).FirstOrDefault(),
                                //JobCountry = rawDtl.jobCountryId > 0 && countries.ContainsKey(rawDtl.jobCountryId) ? countries[rawDtl.jobCountryId] : "",
                                //JobCity = rawDtl.jobCityId > 0 && cities.ContainsKey(rawDtl.jobCityId) ? cities[rawDtl.jobCityId] : "",
                                //NoCVSRequired = jobAddlDtls.Where(dai => dai.Joid == rawDtl.Joid).Select(dai => dai.NoOfCvsRequired).FirstOrDefault(),
                                //NoOfFinalCVsFilled = dbContext.PhJobAssignments.Where(da => da.Joid == da.Joid && da.Status != (byte)RecordStatus.Delete).Sum(da => da.NoOfFinalCvsFilled),
                                //jobAddlDtls.Where(da => da.Joid == rawDtl.Joid).Select(da => da.NoOfFinalCVsFilled).FirstOrDefault(),
                                //JobDateStatus = rawDtl.ClosedDate.Date > Repositories.BaseRepository.CurrentTime.Date ? 1 :
                                //                                    rawDtl.ClosedDate.Date < Repositories.BaseRepository.CurrentTime.Date ? -1 : 0,

                                RecruiterID = rawDtl.recruiterID,
                                RecruiterName = recUsrs.Where(da => da.Id == rawDtl.recruiterID).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                                RecruiterProfilePhoto = rawDtl.recruiterID > 0 ? getEmployeePhotoUrl(recUsrs.Where(da => da.Id == rawDtl.recruiterID).Select(da => da.ProfilePhoto).FirstOrDefault(), rawDtl.recruiterID) : "",

                                BdmID = bdm.Select(da => da.Id).FirstOrDefault(),
                                BdmName = bdm.Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                                BdmProfilePhoto = bdm.Count() > 0 ? getEmployeePhotoUrl(bdm.Select(da => da.ProfilePhoto).FirstOrDefault(), bdm.Select(da => da.Id).FirstOrDefault()) : "",

                                todayCvsRequired = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoCVSRequired ?? 0),
                                tommorrowCvsRequired = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoCVSRequired ?? 0),
                                weekCvsRequired = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoCVSRequired ?? 0),
                                monthCvsRequired = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoCVSRequired ?? 0),
                                cumulativeCvsRequired = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID).Sum(da => da.NoCVSRequired ?? 0),

                                todayCvsFilled = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                tommorrowCvsFilled = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                weekCvsFilled = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                monthCvsFilled = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                cumulativeCvsFilled = rawDtls.Where(da => da.jobId == rawDtl.jobId && da.recruiterID == rawDtl.recruiterID).Sum(da => da.NoOfFinalCVsFilled ?? 0),

                                isAssigned = assigns.Exists(da => da.Joid == rawDtl.jobId && da.AssignedTo == rawDtl.recruiterID),

                                ModificationOn = activity.Select(da => da.CreatedDate).FirstOrDefault(),
                                ModificationBy = activity.Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),

                                JobCity = _jobDtls.Join(cities, da => da.JobLocationId, da2 => da2.Key, (da, da2) => da2.Value).FirstOrDefault(),
                                JobCountry = _jobDtls.Join(countries, da => da.CountryId, da2 => da2.Key, (da, da2) => da2.Value).FirstOrDefault(),

                                PostedDate = _jobDtls.Select(da => da.PostedDate).FirstOrDefault(),
                                ClosedDate = _jobDtls.Select(da => da.ClosedDate).FirstOrDefault(),

                                JobPuId = jobAddDtls.Where(da => da.Joid == rawDtl.jobId).Select(da => da.Puid).FirstOrDefault(),
                                JobBuId = jobAddDtls.Where(da => da.Joid == rawDtl.jobId).Select(da => da.Buid).FirstOrDefault(),

                                jobAssignedDt = jobAssignDtls?.jobAssignmentDate
                            };
                            if (dtl.jobAssignedDt.HasValue)
                                dtl.jobAssignedAge = Convert.ToInt32((CurrentDate - dtl.jobAssignedDt.Value.Date).TotalDays);


                            dtl.JobOpeningStatusCounter = new List<JobOpeningStatusCounterViewModel>();
                            foreach (var stats in stages)
                            {
                                dtl.JobOpeningStatusCounter.Add(new JobOpeningStatusCounterViewModel
                                {
                                    StageID = stats.Id,
                                    StageColor = stats.ColorCode,
                                    StageName = stats.Title,
                                    Counter = counters.Where(dai => dai.StageId == stats.Id && dai.Joid == rawDtl.jobId).Select(dai => dai.Counter).Sum()
                                });
                            }

                            model.jobs.Add(dtl);
                        }
                    }
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<DashboardJobRecruitersDaywisePipelineViewModel>> GetDashboardJobRecruitersDaywisePipelineAsync(int jobId, DashboardFilterPaginationViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardJobRecruitersDaywisePipelineViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                //var jobDtls = await dbContext.PhJobOpenings.Where(da => da.Status != (byte)RecordStatus.Delete && da.Id == jobId)
                //                                .Select(da => new { da.Id, da.JobTitle, da.JobDescription, da.ClientName, da.ClosedDate, da.PostedDate, da.JobRole, da.JobOpeningStatus }).FirstOrDefaultAsync();
                //var JobOpeningStatus = await dbContext.PhJobStatusSes.AsNoTracking().Where(da => jobDtls.JobOpeningStatus == da.Id).Select(da => new { da.Title, da.Jscode }).FirstOrDefaultAsync();
                var model = new DashboardJobRecruitersDaywisePipelineViewModel()
                {

                    //JobId = jobId,
                    //JobRole = jobDtls?.JobRole,
                    //ClientName = jobDtls?.ClientName,
                    //JobOpeningStatus = JobOpeningStatus?.Title,
                    //JobOpeningStatusCode= JobOpeningStatus?.Jscode,
                    //JobTitle = jobDtls?.JobTitle,
                    //ClosedDate = jobDtls?.ClosedDate,
                    //PostedDate = jobDtls?.PostedDate,
                    //JobDescription = jobDtls?.JobDescription,
                    //JobDateStatus = jobDtls?.ClosedDate.Date > Repositories.BaseRepository.CurrentTime.Date ? 1 :
                    //                                    jobDtls?.ClosedDate.Date < Repositories.BaseRepository.CurrentTime.Date ? -1 : 0,
                };

                {
                    var onleaveEmps = await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.Status == (byte)LeaveStatus.Accepted && da.LeaveStartDate.Date <= CurrentDate && CurrentDate <= da.LeaveEndDate.Date).Select(da => da.EmpId).ToArrayAsync();

                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.Recruiter);

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = (await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync()).ToHashSet();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.RecruiterId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    var userIds = await hireUsrQry.OrderBy(da => da.FirstName + " " + da.LastName).Select(da => da.Id).ToArrayAsync();

                    var rawDtls = await dbContext.GetDashboardDaywiseFilterAsync(CurrentTime, (int)UserType.Recruiter, null, null, jobId, userIds, Usr.UserTypeId, Usr.Id);
                    userIds = userIds.Join(rawDtls, da => da, da2 => da2.recruiterID, (da, da2) => da).Distinct().ToArray();
                    {
                        var assigns = await dbContext.PhJobAssignments.Where(da => da.Status == (byte)RecordStatus.Active && da.DeassignDate.HasValue == false && da.Joid == jobId && userIds.Contains(da.AssignedTo))
                                                .Select(da => new { da.AssignedTo }).ToListAsync();
                        userIds = userIds.Join(assigns, da => da, da2 => da2.AssignedTo, (da, da2) => da).Distinct().ToArray();
                        model.totalCount = userIds.Count();

                        if (filterViewModel.CurrentPage < 1)
                        {
                            filterViewModel.CurrentPage = 1;
                        }
                        if (filterViewModel.PerPage < 0)
                        {
                            filterViewModel.PerPage = 10;
                        }
                        model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                        int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                        var recIds = userIds.Skip(skip).Take(filterViewModel.PerPage).ToHashSet();

                        var priorities = await dbContext.PhJobRecruiterPriorities.Where(da => da.Status == (byte)RecordStatus.Active && da.Joid == jobId && recIds.Contains(da.AssignedTo))
                                                .Select(da => new { da.AssignedTo, da.Priority }).ToListAsync();

                        var recUsrs = await dbContext.PiHireUsers.Where(da => recIds.Contains(da.Id))
                            .Select(da => new { da.FirstName, da.LastName, da.Id, da.UserRoleName, da.Location, da.ProfilePhoto }).ToListAsync();
                        var pus = await dbContext.VwUserPuBus.Where(da => recIds.Contains(da.UserId)).Select(da => new { da.UserId, da.ProcessUnit }).ToListAsync();
                        var priorityRef = dbContext.PhRefMasters.Where(x => x.GroupId == 34).ToList(); //priority


                        var fmDt = CurrentTime.Date;
                        var monthEndDate = getMonthEnd(CurrentTime, false);
                        var weekEndDate = getWeekEnd(CurrentTime, false);
                        foreach (var rawDtl in rawDtls.Join(assigns, da => da.recruiterID, da2 => da2.AssignedTo, (da, da2) => new { da.recruiterID }).Distinct())
                        {
                            var _rawDtls = rawDtls.Where(da => da.recruiterID == rawDtl.recruiterID && da.jobId == jobId);
                            var dtl = new DashboardJobRecruitersDaywisePipelineViewModel_Recruiter
                            {
                                id = rawDtl.recruiterID,
                                name = recUsrs.Where(da => da.Id == rawDtl.recruiterID).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                                profilePhoto = rawDtl.recruiterID > 0 ? getEmployeePhotoUrl(recUsrs.Where(da => da.Id == rawDtl.recruiterID).Select(da => da.ProfilePhoto).FirstOrDefault(), rawDtl.recruiterID) : "",

                                role = recUsrs.Where(da => da.Id == rawDtl.recruiterID).Select(da => da.UserRoleName).FirstOrDefault(),
                                location = recUsrs.Where(da => da.Id == rawDtl.recruiterID).Select(da => da.Location).FirstOrDefault(),
                                puIds = pus.Where(da => da.UserId == rawDtl.recruiterID).Select(da => da.ProcessUnit).Distinct().ToList(),

                                todayCvsRequired = _rawDtls.Where(da => da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoCVSRequired ?? 0),
                                tommorrowCvsRequired = _rawDtls.Where(da => da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoCVSRequired ?? 0),
                                weekCvsRequired = _rawDtls.Where(da => da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoCVSRequired ?? 0),
                                monthCvsRequired = _rawDtls.Where(da => da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoCVSRequired ?? 0),

                                todayCvsFilled = _rawDtls.Where(da => da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                tommorrowCvsFilled = _rawDtls.Where(da => da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                weekCvsFilled = _rawDtls.Where(da => da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                monthCvsFilled = _rawDtls.Where(da => da.recruiterID == rawDtl.recruiterID && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),

                                jobPriority = priorities.Where(da => da.AssignedTo == rawDtl.recruiterID).FirstOrDefault()?.Priority,


                                isAssigned = assigns.Exists(da => da.AssignedTo == rawDtl.recruiterID)
                            };

                            dtl.jobPriorityName = priorityRef.Where(da => da.Id == dtl.jobPriority).FirstOrDefault()?.Rmvalue;
                            model.recruiters.Add(dtl);
                        }

                        var stages = await dbContext.PhCandStagesSes.Where(da => da.Status != (byte)RecordStatus.Delete).ToListAsync();
                        var counters = await dbContext.PhJobOpeningStatusCounters.Where(da => da.Joid == jobId && da.Status != (byte)RecordStatus.Delete)
                            .Select(da => new { da.StageId, da.Counter }).ToListAsync();

                        model.JobOpeningStatusCounter = new List<JobOpeningStatusCounterViewModel>();
                        foreach (var stats in stages)
                        {
                            model.JobOpeningStatusCounter.Add(new JobOpeningStatusCounterViewModel
                            {
                                StageID = stats.Id,
                                StageColor = stats.ColorCode,
                                StageName = stats.Title,
                                Counter = counters.Where(dai => dai.StageId == stats.Id).Select(dai => dai.Counter).Sum()
                            });
                        }
                    }
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_assigned>> GetDashboardDaywiseJobRecruitersAsync_assigned(int jobId, DashboardFilterPaginationViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_assigned>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardDaywiseJobRecruitersViewModel_assigned();

                {
                    var onleaveEmps = await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.Status == (byte)LeaveStatus.Accepted && da.LeaveStartDate.Date <= CurrentDate && CurrentDate <= da.LeaveEndDate.Date).Select(da => da.EmpId).ToArrayAsync();

                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.Recruiter);

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = (await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync()).ToHashSet();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.RecruiterId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    var userIds = await hireUsrQry.OrderBy(da => da.FirstName + " " + da.LastName).Select(da => da.Id).ToArrayAsync();
                    var userIds_hsh = userIds.ToHashSet();

                    var assigns = await dbContext.PhJobAssignments.Where(da => da.Status == (byte)RecordStatus.Active && da.DeassignDate.HasValue == false && userIds_hsh.Contains(da.AssignedTo))
                                            .Select(da => new { da.AssignedTo, da.Joid, jobAssignmentDate = (da.ReassignDate ?? da.CreatedDate) }).ToListAsync();

                    userIds = userIds.Join(assigns.Where(da => da.Joid == jobId), da => da, da2 => da2.AssignedTo, (da, da2) => da).Distinct().ToArray();
                    {
                        model.totalCount = userIds.Count();

                        if (filterViewModel.CurrentPage < 1)
                        {
                            filterViewModel.CurrentPage = 1;
                        }
                        if (filterViewModel.PerPage < 0)
                        {
                            filterViewModel.PerPage = 10;
                        }
                        model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                        int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                        var recIds = userIds.Skip(skip).Take(filterViewModel.PerPage).ToHashSet();

                        var recUsrs = await dbContext.PiHireUsers.Where(da => recIds.Contains(da.Id))
                            .Select(da => new { da.FirstName, da.LastName, da.Id, da.UserRoleName, da.Location, da.ProfilePhoto }).ToListAsync();
                        var pus = await dbContext.VwUserPuBus.Where(da => recIds.Contains(da.UserId)).Select(da => new { da.UserId, da.ProcessUnit }).ToListAsync();


                        var rawDtls = await dbContext.GetDashboardDaywiseFilterAsync(CurrentTime, (int)UserType.Recruiter, null, null, jobId, recIds.ToArray(), Usr.UserTypeId, Usr.Id);

                        var fmDt = CurrentTime.Date;
                        var monthEndDate = getMonthEnd(CurrentTime, false);
                        var weekEndDate = getWeekEnd(CurrentTime, false);
                        foreach (var recId in recIds)
                        {
                            var _GetWorkScheduleDtls = await GetWorkScheduleDtls(recId, CurrentDate);
                            var jobAssignDtls = assigns.FirstOrDefault(da => da.AssignedTo == recId && da.Joid == jobId);
                            var activeJobIds = assigns.Where(da => da.AssignedTo == recId).Select(da => da.Joid).Distinct().ToHashSet();
                            var _rawDtls = rawDtls.Where(da => da.recruiterID == recId && activeJobIds.Contains(da.jobId) && da.jobId == jobId);

                            var dtl = new DashboardDaywiseJobRecruitersViewModel_assigned_Recruiter
                            {
                                id = recId,
                                name = recUsrs.Where(da => da.Id == recId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                                profilePhoto = recId > 0 ? getEmployeePhotoUrl(recUsrs.Where(da => da.Id == recId).Select(da => da.ProfilePhoto).FirstOrDefault(), recId) : "",

                                role = recUsrs.Where(da => da.Id == recId).Select(da => da.UserRoleName).FirstOrDefault(),
                                location = recUsrs.Where(da => da.Id == recId).Select(da => da.Location).FirstOrDefault(),
                                puIds = pus.Where(da => da.UserId == recId).Select(da => da.ProcessUnit).Distinct().ToList(),

                                todayCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoCVSRequired ?? 0),
                                tommorrowCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoCVSRequired ?? 0),
                                weekCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoCVSRequired ?? 0),
                                monthCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoCVSRequired ?? 0),

                                todayCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                tommorrowCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                weekCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                monthCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),

                                //onLeave = onleaveEmps.Contains(recId),
                                onLeave = _GetWorkScheduleDtls.isOnLeave,
                                isWeekEnd = _GetWorkScheduleDtls.isWeekEnd,
                                isShiftExist = _GetWorkScheduleDtls.isShiftExist,
                                LeaveTypeName = _GetWorkScheduleDtls.LeaveHolidayName,
                                totalActiveJobCount = activeJobIds.Count(),

                                jobAssignedDt = jobAssignDtls?.jobAssignmentDate
                            };
                            if (dtl.jobAssignedDt.HasValue)
                                dtl.jobAssignedAge = Convert.ToInt32((CurrentDate - dtl.jobAssignedDt.Value.Date).TotalDays);
                            model.recruiters.Add(dtl);
                        }
                    }
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_deassigned>> GetDashboardDaywiseJobRecruitersAsync_deassigned(int jobId, DashboardFilterPaginationViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_deassigned>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardDaywiseJobRecruitersViewModel_deassigned();

                {
                    var onleaveEmps = (await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.Status == (byte)LeaveStatus.Accepted && da.LeaveStartDate.Date <= CurrentDate && CurrentDate <= da.LeaveEndDate.Date).Select(da => da.EmpId).ToArrayAsync()).ToHashSet();

                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && onleaveEmps.Contains(da.Id) == false && da.UserType == (byte)UserType.Recruiter);

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = (await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync()).ToHashSet();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.RecruiterId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    var userIds = await hireUsrQry.Select(da => da.Id).ToArrayAsync();
                    var userIds_hsh = userIds.ToHashSet();

                    var assigns = await dbContext.PhJobAssignments.Where(da => da.Status == (byte)RecordStatus.Active && userIds_hsh.Contains(da.AssignedTo))
                                            .Select(da => new { da.AssignedTo, da.Joid, da.DeassignDate, jobAssignmentDate = (da.ReassignDate ?? da.CreatedDate) }).ToListAsync();

                    var deassignedUsrs = assigns.Where(da => da.Joid == jobId && da.DeassignDate.HasValue == true).Select(da => da.AssignedTo).ToHashSet();
                    var assignedUsrs = assigns.Where(da => da.Joid == jobId && da.DeassignDate.HasValue == false).Select(da => da.AssignedTo).ToHashSet();
                    userIds = userIds.Where(da => deassignedUsrs.Contains(da) == true && assignedUsrs.Contains(da) == false).Distinct().ToArray();

                    userIds_hsh = userIds.ToHashSet();

                    var todayDt = CurrentTime.Date;

                    {
                        model.totalCount = userIds.Count();

                        if (filterViewModel.CurrentPage < 1)
                        {
                            filterViewModel.CurrentPage = 1;
                        }
                        if (filterViewModel.PerPage < 0)
                        {
                            filterViewModel.PerPage = 10;
                        }
                        model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                        int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                        var assignsToday = await dbContext.PhJobRecruiterPriorities.Where(da => da.Status == (byte)RecordStatus.Active && userIds_hsh.Contains(da.AssignedTo) && da.CreatedDate.Date == todayDt)
                            .GroupBy(da => da.AssignedTo).Select(da => new { AssignedTo = da.Key, Priority = da.Min(dai => dai.Priority) }).ToListAsync();
                        var maxNum = int.MaxValue;
                        var usr_with_Priority = userIds.GroupJoin(assignsToday, da => da, da2 => da2.AssignedTo, (da, da2) => new { userId = da, Priority = da2.Select(dai => dai.Priority).FirstOrDefault() ?? maxNum })
                            .ToList();
                        var recIds = usr_with_Priority.OrderByDescending(da => da.Priority).Select(da => da.userId)
                                        .Skip(skip).Take(filterViewModel.PerPage).ToHashSet();

                        var recUsrs = await dbContext.PiHireUsers.Where(da => recIds.Contains(da.Id))
                            .Select(da => new { da.FirstName, da.LastName, da.Id, da.UserRoleName, da.Location, da.ProfilePhoto }).ToListAsync();
                        var pus = await dbContext.VwUserPuBus.Where(da => recIds.Contains(da.UserId)).Select(da => new { da.UserId, da.ProcessUnit }).ToListAsync();

                        var rawDtls = await dbContext.GetDashboardDaywiseFilterAsync(CurrentTime, (int)UserType.Recruiter, null, null, jobId, recIds.ToArray(), Usr.UserTypeId, Usr.Id);

                        var monthEndDate = getMonthEnd(CurrentTime, false);
                        var weekEndDate = getWeekEnd(CurrentTime, false);
                        foreach (var recId in recIds)
                        {
                            var _GetWorkScheduleDtls = await GetWorkScheduleDtls(recId, CurrentDate);
                            var jobAssignDtls = assigns.FirstOrDefault(da => da.AssignedTo == recId && da.Joid == jobId);
                            var activeJobIds = assigns.Where(da => da.DeassignDate.HasValue == false && da.AssignedTo == recId).Select(da => da.Joid).Distinct().ToHashSet();
                            var _rawDtls = rawDtls.Where(da => da.recruiterID == recId && activeJobIds.Contains(da.jobId) && da.jobId == jobId);

                            var dtl = new DashboardDaywiseJobRecruitersViewModel_deassigned_Recruiter
                            {
                                id = recId,
                                name = recUsrs.Where(da => da.Id == recId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                                profilePhoto = recId > 0 ? getEmployeePhotoUrl(recUsrs.Where(da => da.Id == recId).Select(da => da.ProfilePhoto).FirstOrDefault(), recId) : "",

                                role = recUsrs.Where(da => da.Id == recId).Select(da => da.UserRoleName).FirstOrDefault(),
                                location = recUsrs.Where(da => da.Id == recId).Select(da => da.Location).FirstOrDefault(),
                                puIds = pus.Where(da => da.UserId == recId).Select(da => da.ProcessUnit).Distinct().ToList(),

                                todayCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoCVSRequired ?? 0),
                                tommorrowCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoCVSRequired ?? 0),
                                weekCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoCVSRequired ?? 0),
                                monthCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoCVSRequired ?? 0),

                                todayCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                tommorrowCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                weekCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                monthCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),

                                //onLeave = onleaveEmps.Contains(recId),
                                onLeave = _GetWorkScheduleDtls.isOnLeave,
                                isWeekEnd = _GetWorkScheduleDtls.isWeekEnd,
                                isShiftExist = _GetWorkScheduleDtls.isShiftExist,
                                LeaveTypeName = _GetWorkScheduleDtls.LeaveHolidayName,

                                totalActiveJobCount = activeJobIds.Count(),

                                jobAssignedDt = jobAssignDtls?.jobAssignmentDate,
                                jobDeassignedDt = jobAssignDtls?.DeassignDate
                            };
                            if (dtl.jobAssignedDt.HasValue)
                                dtl.jobAssignedAge = Convert.ToInt32(((dtl.jobDeassignedDt?.Date ?? CurrentDate) - dtl.jobAssignedDt.Value.Date).TotalDays);
                            model.recruiters.Add(dtl);
                        }
                    }
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_unassigned>> GetDashboardDaywiseJobRecruitersAsync_notAssigned(int jobId, DashboardFilterPaginationViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_unassigned>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardDaywiseJobRecruitersViewModel_unassigned();

                {
                    var onleaveEmps = (await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.Status == (byte)LeaveStatus.Accepted && da.LeaveStartDate.Date <= CurrentDate && CurrentDate <= da.LeaveEndDate.Date).Select(da => da.EmpId).ToArrayAsync()).ToHashSet();

                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && onleaveEmps.Contains(da.Id) == false && da.UserType == (byte)UserType.Recruiter);

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = (await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync()).ToHashSet();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.RecruiterId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    var userIds = await hireUsrQry.Select(da => da.Id).ToArrayAsync();

                    var userIds_hsh = userIds.ToHashSet();
                    var assignedUsrs = (await dbContext.PhJobAssignments.Where(da => da.Status == (byte)RecordStatus.Active && da.Joid == jobId && userIds_hsh.Contains(da.AssignedTo))
                                            .Select(da => da.AssignedTo).Distinct().ToListAsync()).ToHashSet();

                    userIds = userIds.Where(da => assignedUsrs.Contains(da) == false).Distinct().ToArray();

                    userIds_hsh = userIds.ToHashSet();
                    var assigns = await dbContext.PhJobAssignments.Where(da => da.Status == (byte)RecordStatus.Active && userIds_hsh.Contains(da.AssignedTo) && da.DeassignDate.HasValue == false)
                                            .Select(da => new { da.AssignedTo, da.Joid }).ToListAsync();

                    var todayDt = CurrentTime.Date;

                    {
                        model.totalCount = userIds.Count();

                        if (filterViewModel.CurrentPage < 1)
                        {
                            filterViewModel.CurrentPage = 1;
                        }
                        if (filterViewModel.PerPage < 0)
                        {
                            filterViewModel.PerPage = 10;
                        }
                        model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                        int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                        var assignsToday = await dbContext.PhJobRecruiterPriorities.Where(da => da.Status == (byte)RecordStatus.Active && userIds_hsh.Contains(da.AssignedTo) && da.CreatedDate.Date == todayDt)
                            .GroupBy(da => da.AssignedTo).Select(da => new { AssignedTo = da.Key, Priority = da.Min(dai => dai.Priority) }).ToListAsync();
                        var maxNum = int.MaxValue;
                        var usr_with_Priority = userIds.GroupJoin(assignsToday, da => da, da2 => da2.AssignedTo, (da, da2) => new { userId = da, Priority = da2.Select(dai => dai.Priority).FirstOrDefault() ?? maxNum })
                            .ToList();
                        var recIds = usr_with_Priority.OrderByDescending(da => da.Priority).Select(da => da.userId)
                                        .Skip(skip).Take(filterViewModel.PerPage).ToHashSet();

                        var recUsrs = await dbContext.PiHireUsers.Where(da => recIds.Contains(da.Id))
                            .Select(da => new { da.FirstName, da.LastName, da.Id, da.UserRoleName, da.Location, da.ProfilePhoto }).ToListAsync();
                        var pus = await dbContext.VwUserPuBus.Where(da => recIds.Contains(da.UserId)).Select(da => new { da.UserId, da.ProcessUnit }).ToListAsync();

                        var rawDtls = await dbContext.GetDashboardDaywiseFilterAsync(CurrentTime, (int)UserType.Recruiter, null, null, null, recIds.ToArray(), Usr.UserTypeId, Usr.Id);

                        var monthEndDate = getMonthEnd(CurrentTime, false);
                        var weekEndDate = getWeekEnd(CurrentTime, false);
                        foreach (var recId in recIds)
                        {
                            var _GetWorkScheduleDtls = await GetWorkScheduleDtls(recId, CurrentDate);

                            var activeJobIds = assigns.Where(da => da.AssignedTo == recId).Select(da => da.Joid).Distinct().ToHashSet();
                            var _rawDtls = rawDtls.Where(da => da.recruiterID == recId && activeJobIds.Contains(da.jobId));

                            var dtl = new DashboardDaywiseJobRecruitersViewModel_unassigned_Recruiter
                            {
                                id = recId,
                                name = recUsrs.Where(da => da.Id == recId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                                profilePhoto = recId > 0 ? getEmployeePhotoUrl(recUsrs.Where(da => da.Id == recId).Select(da => da.ProfilePhoto).FirstOrDefault(), recId) : "",

                                role = recUsrs.Where(da => da.Id == recId).Select(da => da.UserRoleName).FirstOrDefault(),
                                location = recUsrs.Where(da => da.Id == recId).Select(da => da.Location).FirstOrDefault(),
                                puIds = pus.Where(da => da.UserId == recId).Select(da => da.ProcessUnit).Distinct().ToList(),

                                todayCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoCVSRequired ?? 0),
                                tommorrowCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoCVSRequired ?? 0),
                                weekCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoCVSRequired ?? 0),
                                monthCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoCVSRequired ?? 0),

                                todayCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                tommorrowCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                weekCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                monthCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),

                                //onLeave = onleaveEmps.Contains(recId),
                                onLeave = _GetWorkScheduleDtls.isOnLeave,
                                isWeekEnd = _GetWorkScheduleDtls.isWeekEnd,
                                isShiftExist = _GetWorkScheduleDtls.isShiftExist,
                                LeaveTypeName = _GetWorkScheduleDtls.LeaveHolidayName,
                                totalActiveJobCount = activeJobIds.Count()
                            };
                            model.recruiters.Add(dtl);
                        }
                    }
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_unassigned>> GetDashboardDaywiseJobRecruitersAsync_hireSuggest(int jobId, DashboardFilterPaginationViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardDaywiseJobRecruitersViewModel_unassigned>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardDaywiseJobRecruitersViewModel_unassigned();

                {
                    var onleaveEmps = (await dbContext.PhEmpLeaveRequests.AsNoTracking().Where(da => da.Status == (byte)LeaveStatus.Accepted && da.LeaveStartDate.Date <= CurrentDate && CurrentDate <= da.LeaveEndDate.Date).Select(da => da.EmpId).ToArrayAsync()).ToHashSet();

                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && onleaveEmps.Contains(da.Id) == false && da.UserType == (byte)UserType.Recruiter);

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = (await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync()).ToHashSet();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.RecruiterId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    var userIds = await hireUsrQry.Select(da => da.Id).ToArrayAsync();

                    var userIds_hsh = userIds.ToHashSet();
                    var assignedUsrs = (await dbContext.PhJobAssignments.Where(da => da.Status == (byte)RecordStatus.Active && da.Joid == jobId && userIds_hsh.Contains(da.AssignedTo))
                                            .Select(da => da.AssignedTo).Distinct().ToListAsync()).ToHashSet();
                    userIds = userIds.Where(da => assignedUsrs.Contains(da) == false).Distinct().ToArray();

                    userIds_hsh = userIds.ToHashSet();
                    var similarJobs = (await dbContext.GetSimilarJobsAsync(jobId)).Select(da => da.JobId).ToHashSet();
                    var similarJobUsrs = (await dbContext.PhJobAssignments.Where(da => da.Status == (byte)RecordStatus.Active && similarJobs.Contains(da.Joid) && da.IsJoinerSuc == true && userIds_hsh.Contains(da.AssignedTo))
                                            .Select(da => da.AssignedTo).ToListAsync()).ToHashSet();
                    userIds = userIds.Where(da => similarJobUsrs.Contains(da)).Distinct().ToArray();

                    userIds_hsh = userIds.ToHashSet();
                    var assigns = await dbContext.PhJobAssignments.Where(da => da.Status == (byte)RecordStatus.Active && userIds_hsh.Contains(da.AssignedTo) && da.DeassignDate.HasValue == false)
                                            .Select(da => new { da.AssignedTo, da.Joid }).ToListAsync();

                    var todayDt = CurrentTime.Date;
                    {
                        model.totalCount = userIds.Count();

                        if (filterViewModel.CurrentPage < 1)
                        {
                            filterViewModel.CurrentPage = 1;
                        }
                        if (filterViewModel.PerPage < 0)
                        {
                            filterViewModel.PerPage = 10;
                        }
                        model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                        int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                        var assignsToday = await dbContext.PhJobRecruiterPriorities.Where(da => da.Status == (byte)RecordStatus.Active && userIds_hsh.Contains(da.AssignedTo) && da.CreatedDate.Date == todayDt)
                            .GroupBy(da => da.AssignedTo).Select(da => new { AssignedTo = da.Key, Priority = da.Min(dai => dai.Priority) }).ToListAsync();
                        var maxNum = int.MaxValue;
                        var usr_with_Priority = userIds.GroupJoin(assignsToday, da => da, da2 => da2.AssignedTo, (da, da2) => new { userId = da, Priority = da2.Select(dai => dai.Priority).FirstOrDefault() ?? maxNum })
                            .ToList();
                        var recIds = usr_with_Priority.OrderByDescending(da => da.Priority).Select(da => da.userId)
                                        .Skip(skip).Take(filterViewModel.PerPage).ToHashSet();

                        var recUsrs = await dbContext.PiHireUsers.Where(da => recIds.Contains(da.Id))
                            .Select(da => new { da.FirstName, da.LastName, da.Id, da.UserRoleName, da.Location, da.ProfilePhoto }).ToListAsync();
                        var pus = await dbContext.VwUserPuBus.Where(da => recIds.Contains(da.UserId)).Select(da => new { da.UserId, da.ProcessUnit }).ToListAsync();

                        var rawDtls = await dbContext.GetDashboardDaywiseFilterAsync(CurrentTime, (int)UserType.Recruiter, null, null, null, recIds.ToArray(), Usr.UserTypeId, Usr.Id);

                        var monthEndDate = getMonthEnd(CurrentTime, false);
                        var weekEndDate = getWeekEnd(CurrentTime, false);
                        foreach (var recId in recIds)
                        {
                            var _GetWorkScheduleDtls = await GetWorkScheduleDtls(recId, CurrentDate);

                            var activeJobIds = assigns.Where(da => da.AssignedTo == recId).Select(da => da.Joid).Distinct().ToHashSet();
                            var _rawDtls = rawDtls.Where(da => da.recruiterID == recId && activeJobIds.Contains(da.jobId));

                            var dtl = new DashboardDaywiseJobRecruitersViewModel_unassigned_Recruiter
                            {
                                id = recId,
                                name = recUsrs.Where(da => da.Id == recId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                                profilePhoto = recId > 0 ? getEmployeePhotoUrl(recUsrs.Where(da => da.Id == recId).Select(da => da.ProfilePhoto).FirstOrDefault(), recId) : "",

                                role = recUsrs.Where(da => da.Id == recId).Select(da => da.UserRoleName).FirstOrDefault(),
                                location = recUsrs.Where(da => da.Id == recId).Select(da => da.Location).FirstOrDefault(),
                                puIds = pus.Where(da => da.UserId == recId).Select(da => da.ProcessUnit).Distinct().ToList(),

                                todayCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoCVSRequired ?? 0),
                                tommorrowCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoCVSRequired ?? 0),
                                weekCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoCVSRequired ?? 0),
                                monthCvsRequired = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoCVSRequired ?? 0),

                                todayCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                tommorrowCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date == CurrentTime.AddDays(1).Date).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                weekCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= weekEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),
                                monthCvsFilled = _rawDtls.Where(da => da.recruiterID == recId && da.jobAssignmentDate.HasValue && da.jobAssignmentDate.Value.Date <= monthEndDate).Sum(da => da.NoOfFinalCVsFilled ?? 0),

                                //onLeave = onleaveEmps.Contains(recId),
                                onLeave = _GetWorkScheduleDtls.isOnLeave,
                                isWeekEnd = _GetWorkScheduleDtls.isWeekEnd,
                                isShiftExist = _GetWorkScheduleDtls.isShiftExist,
                                LeaveTypeName = _GetWorkScheduleDtls.LeaveHolidayName,
                                totalActiveJobCount = activeJobIds.Count()
                            };
                            model.recruiters.Add(dtl);
                        }
                    }
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<DashboardSimilarJobsDaywiseViewModel>> GetDashboardDaywiseJobRecruitersAsync_hireSuggest_similarJobs(int jobId, int recruiterId, DashboardFilterPaginationViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardSimilarJobsDaywiseViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                {
                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && da.Id == recruiterId && da.UserType == (byte)UserType.Recruiter);

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = (await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync()).ToHashSet();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.RecruiterId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    if (await hireUsrQry.AnyAsync(da => da.Id == recruiterId))
                    {
                        var similarJobs = (await dbContext.GetSimilarJobsAsync(jobId)).Select(da => da.JobId).ToHashSet();
                        var assigns = await dbContext.PhJobAssignments.Where(da => da.Status == (byte)RecordStatus.Active && da.AssignedTo == recruiterId)
                                                .Select(da => new { da.DeassignDate, da.Joid, da.IsJoinerSuc }).ToListAsync();
                        similarJobs = assigns.Where(da => similarJobs.Contains(da.Joid) && da.IsJoinerSuc == true).Select(da => da.Joid).Distinct().ToHashSet();


                        var model = new DashboardSimilarJobsDaywiseViewModel();

                        var todayDt = CurrentTime.Date;
                        {
                            model.totalCount = similarJobs.Count();

                            if (filterViewModel.CurrentPage < 1)
                            {
                                filterViewModel.CurrentPage = 1;
                            }
                            if (filterViewModel.PerPage < 0)
                            {
                                filterViewModel.PerPage = 10;
                            }
                            model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                            int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                            var assignsToday = await dbContext.PhJobRecruiterPriorities.Where(da => da.Status == (byte)RecordStatus.Active && da.AssignedTo == recruiterId && da.CreatedDate.Date == todayDt)
                                .GroupBy(da => da.Joid).Select(da => new { Joid = da.Key, Priority = da.Min(dai => dai.Priority) }).ToListAsync();
                            var maxNum = int.MaxValue;
                            var usr_with_Priority = similarJobs.GroupJoin(assignsToday, da => da, da2 => da2.Joid, (da, da2) => new { joId = da, Priority = da2.Select(dai => dai.Priority).FirstOrDefault() ?? maxNum })
                                .ToList();
                            var similarJobIds = usr_with_Priority.OrderByDescending(da => da.Priority).Select(da => da.joId)
                                            .Skip(skip).Take(filterViewModel.PerPage).ToHashSet();

                            var jobDtls = await dbContext.PhJobOpenings.Where(da => da.Status != (byte)RecordStatus.Delete && similarJobIds.Contains(da.Id))
                                                    .Select(da => new { da.Id, da.JobTitle, da.ClientName, da.PostedDate, da.ClosedDate, da.JobLocationId, da.CountryId, bdmId = da.BroughtBy ?? da.CreatedBy }).ToListAsync();

                            var jobAddDtls = await dbContext.PhJobOpeningsAddlDetails.Where(da => similarJobIds.Contains(da.Joid))
                                                    .Select(da => new { da.Joid, da.Puid, da.Buid }).ToListAsync();

                            var activities = await dbContext.PhActivityLogs.Where(da => da.Joid.HasValue && similarJobIds.Contains(da.Joid.Value) && da.ActivityMode == (byte)ActivityOn.Opening && (da.ActivityType == (byte)LogActivityType.RecordUpdates || da.ActivityType == (byte)LogActivityType.JobEditUpdates))
                                                            .GroupBy(da => da.Joid).Select(da => da.Max(dai => dai.Id))
                                                            .Join(dbContext.PhActivityLogs, da => da, da2 => da2.Id, (da, da2) => new { da2.CreatedDate, da2.CreatedBy }).ToListAsync();

                            var usrIds = jobDtls.Where(da => da.bdmId > 0).Select(da => da.bdmId).Union(activities.Select(da => da.CreatedBy)).Distinct().ToHashSet();
                            usrIds.Add(recruiterId);

                            var usrs = await dbContext.PiHireUsers.Where(da => usrIds.Contains(da.Id))
                                                .Select(da => new { da.FirstName, da.LastName, da.Id, da.ProfilePhoto }).ToListAsync();

                            var countryIds = jobDtls.Where(rawDtl => rawDtl.CountryId > 0).Select(rawDtl => rawDtl.CountryId).Distinct().ToArray();
                            var cityIds = jobDtls.Where(rawDtl => rawDtl.JobLocationId > 0).Select(rawDtl => rawDtl.JobLocationId).Distinct().ToArray();

                            var countries = await dbContext.PhCountries.AsNoTracking().Where(rawDtl => countryIds.Contains(rawDtl.Id)).ToDictionaryAsync(rawDtl => rawDtl.Id, da2 => da2.Name);
                            var cities = await dbContext.PhCities.AsNoTracking().Where(rawDtl => cityIds.Contains(rawDtl.Id)).ToDictionaryAsync(rawDtl => rawDtl.Id, da2 => da2.Name);


                            foreach (var similarJobId in similarJobIds)
                            {
                                var jobDtl = jobDtls.FirstOrDefault(da => da.Id == similarJobId);

                                var bdm = usrs.FirstOrDefault(da => da.Id == jobDtl.bdmId);

                                var activity = activities.Join(usrs, da => da.CreatedBy, da2 => da2.Id, (da, da2) => new { da.CreatedDate, da2.FirstName, da2.LastName });

                                var dtl = new DashboardSimilarJobsDaywiseViewModel_job
                                {
                                    jobId = jobId,
                                    JobTitle = jobDtl.JobTitle,
                                    ClientName = jobDtl.ClientName,
                                    ClosedDate = jobDtl.ClosedDate,
                                    PostedDate = jobDtl.PostedDate,
                                    JobCountry = jobDtl.CountryId > 0 && countries.ContainsKey(jobDtl.CountryId) ? countries[jobDtl.CountryId] : "",
                                    JobCity = jobDtl.JobLocationId > 0 && cities.ContainsKey(jobDtl.JobLocationId) ? cities[jobDtl.JobLocationId] : "",
                                    JobDateStatus = jobDtl.ClosedDate.Date > Repositories.BaseRepository.CurrentTime.Date ? 1 :
                                                                        jobDtl.ClosedDate.Date < Repositories.BaseRepository.CurrentTime.Date ? -1 : 0,

                                    RecruiterName = usrs.Where(da => da.Id == recruiterId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                                    RecruiterProfilePhoto = recruiterId > 0 ? getEmployeePhotoUrl(usrs.Where(da => da.Id == recruiterId).Select(da => da.ProfilePhoto).FirstOrDefault(), recruiterId) : "",

                                    BdmName = bdm?.FirstName + " " + bdm?.LastName,
                                    BdmProfilePhoto = bdm != null ? getEmployeePhotoUrl(bdm.ProfilePhoto, bdm.Id) : "",

                                    isAssigned = assigns.Exists(da => da.Joid == similarJobId && da.DeassignDate.HasValue == false),

                                    ModificationOn = activity.Select(da => da.CreatedDate).FirstOrDefault(),
                                    ModificationBy = activity.Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),

                                    JobPuId = jobAddDtls.Where(da => da.Joid == jobId).Select(da => da.Puid).FirstOrDefault(),
                                    JobBuId = jobAddDtls.Where(da => da.Joid == jobId).Select(da => da.Buid).FirstOrDefault(),
                                };

                                model.jobs.Add(dtl);
                            }
                        }
                        respModel.Status = true;
                        respModel.SetResult(model);
                    }
                    else
                    {
                        respModel.SetError(ApiResponseErrorCodes.InvalidBodyContent, "Invalid/unauthorized recruiter Id", true);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<DashboardJobRecruitersAssignmentHistoryViewModel>> GetDashboardRecruiterAssignmentHistoryAsync(int? jobId, int recruiterId, DashboardFilterPaginationViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardJobRecruitersAssignmentHistoryViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardJobRecruitersAssignmentHistoryViewModel();

                {
                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Id == recruiterId && da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.Recruiter);

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = (await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync()).ToHashSet();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.RecruiterId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    var userIds = await hireUsrQry.Select(da => da.Id).ToArrayAsync();

                    var userIds_hsh = userIds.ToHashSet();


                    var todayDt = CurrentTime.Date;
                    {
                        var qry = dbContext.PhJobAssignmentHistories.AsNoTracking().Where(da => da.Status == (byte)RecordStatus.Active && userIds_hsh.Contains(da.AssignedTo));
                        if (jobId.HasValue)
                            qry = qry.Where(da => da.Joid == jobId);

                        model.totalCount = qry.Count();
                        if (filterViewModel.CurrentPage < 1)
                        {
                            filterViewModel.CurrentPage = 1;
                        }
                        if (filterViewModel.PerPage < 0)
                        {
                            filterViewModel.PerPage = 10;
                        }
                        model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                        int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                        var rawDtls = await qry.OrderByDescending(da => da.CreatedDate).Skip(skip).Take(filterViewModel.PerPage)
                            .Select(da => new { da.Id, da.Joid, da.AssignedTo, da.CreatedDate, da.CreatedBy, da.DeassignDate, da.DeassignBy }).ToListAsync();

                        var joIds = rawDtls.Select(da => da.Joid).Distinct().ToHashSet();
                        var jobs = await dbContext.PhJobOpenings.AsNoTracking().Where(da => joIds.Contains(da.Id)).Select(da => new { da.Id, da.JobTitle }).ToListAsync();


                        var createdByIds = rawDtls.Where(da => da.CreatedBy > 0).Select(da => da.CreatedBy)
                                            .Union(rawDtls.Where(da => da.DeassignBy > 0).Select(da => da.DeassignBy.Value))
                                            .Union(rawDtls.Select(dai => dai.AssignedTo))
                                            .Distinct().ToHashSet();
                        var createdBys = await dbContext.PiHireUsers.AsNoTracking().Where(da => createdByIds.Contains(da.Id)).Select(da => new { da.Id, da.FirstName, da.LastName }).ToListAsync();

                        foreach (var rawDtl in rawDtls)
                        {
                            var dtl = new DashboardJobRecruitersAssignmentHistoryViewModel_data
                            {
                                Id = rawDtl.Id,

                                JoId = rawDtl.Joid,
                                JobTitle = jobs.Where(dai => dai.Id == rawDtl.Joid).Select(da => da.JobTitle).FirstOrDefault(),

                                AssignedTo = rawDtl.AssignedTo,
                                AssignedToName = createdBys.Where(dai => dai.Id == rawDtl.AssignedTo).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),

                                AssignedDate = rawDtl.CreatedDate,
                                AssignedBy = createdBys.Where(dai => dai.Id == rawDtl.CreatedBy).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                                DeassignedDate = rawDtl.DeassignDate,
                                DeassignedBy = createdBys.Where(dai => dai.Id == rawDtl.DeassignBy).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),
                            };
                            dtl.AssignedAge = Convert.ToInt32(((dtl.DeassignedDate ?? CurrentDate) - dtl.AssignedDate.Date).TotalDays);
                            model.data.Add(dtl);
                        }
                    }
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<DashboardJobRecruitersDaywiseHistoryViewModel>> GetDashboardJobRecruiterDaywiseHistoryAsync(int? jobId, int recruiterId, DashboardRecruiterDaywiseHistoryFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardJobRecruitersDaywiseHistoryViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardJobRecruitersDaywiseHistoryViewModel();

                {
                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Id == recruiterId && da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.Recruiter);

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = (await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync()).ToHashSet();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.RecruiterId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    var userIds = await hireUsrQry.Select(da => da.Id).ToArrayAsync();

                    var userIds_hsh = userIds.ToHashSet();


                    var todayDt = CurrentTime.Date;
                    {
                        var qry = dbContext.PhJobAssignmentsDayWises.AsNoTracking().Where(da => da.Status == (byte)RecordStatus.Active && userIds_hsh.Contains(da.AssignedTo));
                        if (jobId.HasValue)
                            qry = qry.Where(da => da.Joid == jobId);

                        var dt = calcDate(filterViewModel.DateFilter);
                        qry = qry.Where(da => dt.fmDt <= da.AssignmentDate && da.AssignmentDate <= dt.toDt);

                        model.totalCount = qry.Count();
                        if (filterViewModel.CurrentPage < 1)
                        {
                            filterViewModel.CurrentPage = 1;
                        }
                        if (filterViewModel.PerPage < 0)
                        {
                            filterViewModel.PerPage = 10;
                        }
                        model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                        int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                        var rawDtls = await qry.OrderByDescending(da => da.AssignmentDate).Skip(skip).Take(filterViewModel.PerPage)
                            .Select(da => new { da.Id, da.Joid, da.AssignedTo, da.AssignmentDate, da.NoCvsrequired, da.NoOfFinalCvsFilled }).ToListAsync();

                        var joIds = rawDtls.Select(da => da.Joid).Distinct().ToHashSet();
                        var jobs = await dbContext.PhJobOpenings.AsNoTracking().Where(da => joIds.Contains(da.Id)).Select(da => new { da.Id, da.JobTitle }).ToListAsync();

                        var JobAssignmentDayWiseIds = rawDtls.Select(da => da.Id).ToHashSet();

                        var JobAssignmentDayWiseLogs = await dbContext.PhJobAssignmentsDayWisesLogs.AsNoTracking().Where(da => da.Status == (byte)RecordStatus.Active && JobAssignmentDayWiseIds.Contains(da.JobAssignmentDayWiseId))
                                                        .Select(da => new { da.JobAssignmentDayWiseId, da.CreatedBy, da.CreatedDate, da.LogType, da.NoCvsrequired }).ToListAsync();
                        var createdByIds = JobAssignmentDayWiseLogs.Where(da => da.CreatedBy > 0).Select(da => da.CreatedBy.Value).Union(rawDtls.Select(dai => dai.AssignedTo)).Distinct().ToHashSet();
                        var createdBys = await dbContext.PiHireUsers.AsNoTracking().Where(da => createdByIds.Contains(da.Id)).Select(da => new { da.Id, da.FirstName, da.LastName }).ToListAsync();

                        foreach (var AssignmentObj in rawDtls.Select(da => new { AssignmentDate = da.AssignmentDate.Value.Date, da.AssignedTo, da.Joid }).Distinct())
                        {
                            var rawDtl = rawDtls.Where(da => da.AssignmentDate.Value.Date == AssignmentObj.AssignmentDate && da.Joid == AssignmentObj.Joid && da.AssignedTo == AssignmentObj.AssignedTo).ToList();
                            var logs = JobAssignmentDayWiseLogs.Join(rawDtl, da => da.JobAssignmentDayWiseId, da2 => da2.Id, (da, da2) => da);

                            var dtl = new DashboardJobRecruitersDaywiseHistoryViewModel_data
                            {
                                Id = rawDtl.Select(da => da.Id).ToArray(),

                                JoId = AssignmentObj.Joid,
                                JobTitle = jobs.Where(dai => dai.Id == AssignmentObj.Joid).Select(da => da.JobTitle).FirstOrDefault(),

                                AssignedTo = AssignmentObj.AssignedTo,
                                AssignedToName = createdBys.Where(dai => dai.Id == AssignmentObj.AssignedTo).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),

                                AssignmentDate = AssignmentObj.AssignmentDate,

                                NoCvsrequired = rawDtl.Sum(da => da.NoCvsrequired),
                                NoOfFinalCvsFilled = rawDtl.Sum(da => da.NoOfFinalCvsFilled),

                                logs = logs.Select(da => new DashboardJobRecruitersDaywiseHistoryViewModel_log
                                {
                                    CreatedOn = da.CreatedDate,
                                    CreatedBy = createdBys.Where(dai => dai.Id == da.CreatedBy).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),

                                    CvsCount = da.NoCvsrequired,
                                    CarryForward = ((da.LogType == (byte)RecruiterJobAssignmentLogType.AutoPendingCvForwardTo || da.LogType == (byte)RecruiterJobAssignmentLogType.AutoPendingCvForwardFrom) ? da.NoCvsrequired : 0),
                                    IncrementCvs = ((da.LogType == (byte)RecruiterJobAssignmentLogType.ManualCvIncrement) ? da.NoCvsrequired : 0),
                                    DecrementCvs = ((da.LogType == (byte)RecruiterJobAssignmentLogType.ManualCvDecrement) ? da.NoCvsrequired : 0),

                                    IsCarryForwardTo = da.LogType == (byte)RecruiterJobAssignmentLogType.AutoPendingCvForwardTo,
                                    IsCarryForwardFrom = da.LogType == (byte)RecruiterJobAssignmentLogType.AutoPendingCvForwardFrom
                                }).ToList()
                            };
                            model.data.Add(dtl);
                        }
                    }
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<DashboardJobRecruitersDaywiseHistoryViewModel>> GetDashboardBdmDaywiseHistoryAsync(int bdmId, DashboardRecruiterDaywiseHistoryFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardJobRecruitersDaywiseHistoryViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);

                var model = new DashboardJobRecruitersDaywiseHistoryViewModel();

                {
                    var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Id == bdmId && da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.BDM);

                    switch ((UserType)Usr.UserTypeId)
                    {
                        case UserType.SuperAdmin:
                            {

                            }
                            break;
                        case UserType.Admin:
                            {
                                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                                var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                                //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                                var allowedUsers = (await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync()).ToHashSet();

                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.BDM:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        case UserType.Recruiter:
                            {
                                HashSet<int> allowedUsers = new HashSet<int>();
                                {
                                    var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.RecruiterId == loginUserId);
                                    allowedUsers = (await qry.Select(da => da.RecruiterId).Distinct().ToListAsync()).ToHashSet();
                                }
                                hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                            }
                            break;
                        default:
                            {
                                hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                            }
                            break;
                    }

                    var userIds = await hireUsrQry.Select(da => da.Id).ToArrayAsync();

                    var userIds_hsh = userIds.ToHashSet();

                    var srchDtls = await dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId.HasValue && userIds_hsh.Contains(da.BdmId.Value))
                                        .Select(da => new { da.BdmId, da.RecruiterId, da.JobId }).ToListAsync();

                    var todayDt = CurrentTime.Date;
                    {
                        var recIds = srchDtls.Select(da => da.RecruiterId).ToHashSet();
                        var joIds = srchDtls.Select(da => da.JobId).ToHashSet();
                        var qry = dbContext.PhJobAssignmentsDayWises.AsNoTracking().Where(da => da.Status == (byte)RecordStatus.Active && recIds.Contains(da.AssignedTo) && joIds.Contains(da.Joid));

                        var dt = calcDate(filterViewModel.DateFilter);
                        qry = qry.Where(da => dt.fmDt <= da.AssignmentDate && da.AssignmentDate <= dt.toDt);

                        model.totalCount = qry.Count();
                        if (filterViewModel.CurrentPage < 1)
                        {
                            filterViewModel.CurrentPage = 1;
                        }
                        if (filterViewModel.PerPage < 0)
                        {
                            filterViewModel.PerPage = 10;
                        }
                        model.totalPages = filterViewModel.PerPage > 0 ? (int)Math.Ceiling((decimal)(model.totalCount) / filterViewModel.PerPage) : model.totalCount;
                        int skip = (filterViewModel.CurrentPage - 1) * filterViewModel.PerPage;

                        var rawDtls = await qry.OrderByDescending(da => da.AssignmentDate).Skip(skip).Take(filterViewModel.PerPage)
                            .Select(da => new { da.Id, da.Joid, da.AssignedTo, da.AssignmentDate, da.NoCvsrequired, da.NoOfFinalCvsFilled }).ToListAsync();

                        var jobs = await dbContext.PhJobOpenings.AsNoTracking().Where(da => joIds.Contains(da.Id)).Select(da => new { da.Id, da.JobTitle }).ToListAsync();

                        var JobAssignmentDayWiseIds = rawDtls.Select(da => da.Id).ToHashSet();

                        var JobAssignmentDayWiseLogs = await dbContext.PhJobAssignmentsDayWisesLogs.AsNoTracking().Where(da => da.Status == (byte)RecordStatus.Active && JobAssignmentDayWiseIds.Contains(da.JobAssignmentDayWiseId))
                                                        .Select(da => new { da.JobAssignmentDayWiseId, da.CreatedBy, da.CreatedDate, da.LogType, da.NoCvsrequired }).ToListAsync();
                        var createdByIds = JobAssignmentDayWiseLogs.Where(da => da.CreatedBy > 0).Select(da => da.CreatedBy.Value).Union(rawDtls.Select(dai => dai.AssignedTo)).Distinct().ToHashSet();
                        var createdBys = await dbContext.PiHireUsers.AsNoTracking().Where(da => createdByIds.Contains(da.Id)).Select(da => new { da.Id, da.FirstName, da.LastName }).ToListAsync();

                        foreach (var AssignmentObj in rawDtls.Select(da => new { AssignmentDate = da.AssignmentDate.Value.Date, da.AssignedTo, da.Joid }).Distinct())
                        {
                            var rawDtl = rawDtls.Where(da => da.AssignmentDate.Value.Date == AssignmentObj.AssignmentDate && da.Joid == AssignmentObj.Joid && da.AssignedTo == AssignmentObj.AssignedTo).ToList();
                            var logs = JobAssignmentDayWiseLogs.Join(rawDtl, da => da.JobAssignmentDayWiseId, da2 => da2.Id, (da, da2) => da);

                            var dtl = new DashboardJobRecruitersDaywiseHistoryViewModel_data
                            {
                                Id = rawDtl.Select(da => da.Id).ToArray(),

                                JoId = AssignmentObj.Joid,
                                JobTitle = jobs.Where(dai => dai.Id == AssignmentObj.Joid).Select(da => da.JobTitle).FirstOrDefault(),

                                AssignedTo = AssignmentObj.AssignedTo,
                                AssignedToName = createdBys.Where(dai => dai.Id == AssignmentObj.AssignedTo).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),

                                AssignmentDate = AssignmentObj.AssignmentDate,

                                NoCvsrequired = rawDtl.Sum(da => da.NoCvsrequired),
                                NoOfFinalCvsFilled = rawDtl.Sum(da => da.NoOfFinalCvsFilled),

                                logs = logs.Select(da => new DashboardJobRecruitersDaywiseHistoryViewModel_log
                                {
                                    CreatedOn = da.CreatedDate,
                                    CreatedBy = createdBys.Where(dai => dai.Id == da.CreatedBy).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault(),

                                    CvsCount = da.NoCvsrequired,
                                    CarryForward = ((da.LogType == (byte)RecruiterJobAssignmentLogType.AutoPendingCvForwardTo || da.LogType == (byte)RecruiterJobAssignmentLogType.AutoPendingCvForwardFrom) ? da.NoCvsrequired : 0),
                                    IncrementCvs = ((da.LogType == (byte)RecruiterJobAssignmentLogType.ManualCvIncrement) ? da.NoCvsrequired : 0),
                                    DecrementCvs = ((da.LogType == (byte)RecruiterJobAssignmentLogType.ManualCvDecrement) ? da.NoCvsrequired : 0),

                                    IsCarryForwardTo = da.LogType == (byte)RecruiterJobAssignmentLogType.AutoPendingCvForwardTo,
                                    IsCarryForwardFrom = da.LogType == (byte)RecruiterJobAssignmentLogType.AutoPendingCvForwardFrom
                                }).ToList()
                            };
                            model.data.Add(dtl);
                        }
                    }
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        [Obsolete]
        public async Task<GetResponseViewModel<DashboardHireAdminViewModel>> GetDashboardHireAdminAsync(DashboardCandidateInterviewFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardHireAdminViewModel>();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);
                if (Usr.UserTypeId != (byte)UserType.Admin && Usr.UserTypeId != (byte)UserType.SuperAdmin)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not: " + UserType.Admin + " && " + UserType.SuperAdmin, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                var rawDtls = await dbContext.GetDashboardHireAdminAsync(null, null, filterViewModel.PuIds, filterViewModel.BuIds, CurrentTime.Date, Usr.UserTypeId, Usr.Id);
                var model = DashboardHireAdminViewModel.ToModel(rawDtls);

                var jobRawDtls = await dbContext.GetDashboardRecruiterStatusAsync(CurrentTime, CurrentTime.Date, null, null, Usr.UserTypeId, Usr.Id);
                {
                    var currentDt = CurrentTime.Date;
                    var monthEndDate = getMonthEnd(CurrentTime, false);
                    var weekEndDate = getWeekEnd(CurrentTime, false);
                    model.todayCvsRequired = jobRawDtls.Where(da => da.ClosedDate.Date == currentDt).Sum(da => da.NoCVSRequired ?? 0);
                    model.todayCvsFilled = jobRawDtls.Where(da => da.ClosedDate.Date == currentDt).Sum(da => da.NoOfFinalCVsFilled ?? 0);
                    var tommorrowDt = currentDt.AddDays(1).Date;
                    model.tommorrowCvsRequired = jobRawDtls.Where(da => currentDt <= da.ClosedDate.Date && da.ClosedDate.Date <= tommorrowDt).Sum(da => da.NoCVSRequired ?? 0);
                    model.weekCvsRequired = jobRawDtls.Where(da => currentDt <= da.ClosedDate.Date && da.ClosedDate.Date <= weekEndDate).Sum(da => da.NoCVSRequired ?? 0);
                    model.monthCvsRequired = jobRawDtls.Where(da => currentDt <= da.ClosedDate.Date && da.ClosedDate.Date <= monthEndDate).Sum(da => da.NoCVSRequired ?? 0);
                }
                {
                    var currentDt = CurrentTime.Date;
                    var day1 = currentDt.AddDays(-1).Date;
                    model.overdue1DayCvsRequired = jobRawDtls.Where(da => currentDt > da.ClosedDate.Date && da.ClosedDate.Date >= day1).Sum(da => da.NoCVSRequired ?? 0);
                    var day5 = currentDt.AddDays(-5).Date;
                    model.overdue5DaysCvsRequired = jobRawDtls.Where(da => currentDt > da.ClosedDate.Date && da.ClosedDate.Date >= day5).Sum(da => da.NoCVSRequired ?? 0);
                    var day10 = currentDt.AddDays(-10).Date;
                    model.overdue10DaysCvsRequired = jobRawDtls.Where(da => currentDt > da.ClosedDate.Date && da.ClosedDate.Date >= day10).Sum(da => da.NoCVSRequired ?? 0);
                    var day30 = currentDt.AddDays(-30).Date;
                    model.overdue30DaysCvsRequired = jobRawDtls.Where(da => currentDt > da.ClosedDate.Date && da.ClosedDate.Date >= day30).Sum(da => da.NoCVSRequired ?? 0);
                }
                //model.submittedCvsRequired = jobRawDtls.Sum(da => da.NoCVSRequired ?? 0);
                //model.submittedCvsFilled = jobRawDtls.Sum(da => da.NoOfFinalCVsFilled ?? 0);
                model.filterFromDt = null;
                model.filterToDt = null;

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        [Obsolete]
        public async Task<GetResponseViewModel<DashboardBdmViewModel>> GetDashboardBdmAsync(DashboardBdmFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardBdmViewModel>();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);
                if (Usr.UserTypeId != (byte)UserType.BDM)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not:" + UserType.BDM, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                var dt = calcDate(filterViewModel.DateFilter);
                var rawDtls = await dbContext.GetDashboardBdmAsync(dt.fmDt, dt.toDt, filterViewModel.JobCategory, Usr.UserTypeId, Usr.Id);
                var model = DashboardBdmViewModel.ToModel(rawDtls);

                model.filterFromDt = dt.fmDt;
                model.filterToDt = dt.toDt;

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        [Obsolete]
        public async Task<GetResponseViewModel<DashboardRecruiterViewModel>> GetDashboardRecruiterAsync(DashboardRecruiterFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardRecruiterViewModel>();
            try
            {
                // logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);
                if (Usr.UserTypeId != (byte)UserType.Recruiter)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not:" + UserType.Recruiter, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                var dt = calcDate(filterViewModel.DateFilter);
                var rawDtls = await dbContext.GetDashboardRecruiterAsync(dt.fmDt, dt.toDt, Usr.UserTypeId, Usr.Id);
                var model = DashboardRecruiterViewModel.ToModel(rawDtls);
                model.filterFromDt = dt.fmDt;
                model.filterToDt = dt.toDt;

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        [Obsolete]
        public async Task<GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>> GetDashboardRecruiterJobCatgLast14DaysAsync()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>();
            try
            {

                if (Usr.UserTypeId != (byte)UserType.Recruiter)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not:" + UserType.Recruiter, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                var model = new List<DashboardRecruiterJobCategoryViewModel>();
                {
                    var fmDt = CurrentTime.AddDays(-14).Date;
                    var toDt = CurrentTime.AddDays(1).Date.AddMinutes(-1);
                    var rawDtls = await dbContext.GetDashboardRecruiterJobCategoryAsync(fmDt, toDt, Usr.UserTypeId, Usr.Id, false);

                    //59 - OpeningType - job opening types
                    var src = (await dbContext.PhRefMasters.Where(da => da.Status != (byte)RecordStatus.Delete && da.GroupId == 59)
                       .Select(da => da.Rmvalue).ToListAsync());
                    var cntSrc = rawDtls.GroupBy(da => da.JobCategory)
                        .Select(da => new DashboardRecruiterJobCategoryViewModel { JobCategory = da.Key, Count = da.Sum(dai => dai.resumeCount) ?? 0 })
                        .ToList();
                    src.ForEach(da =>
                        model.Add(new DashboardRecruiterJobCategoryViewModel
                        {
                            JobCategory = da,
                            Count = cntSrc.Where(dai => dai.JobCategory?.Trim().ToLower() == da?.Trim().ToLower()).Sum(dai => dai.Count)
                        }));
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, "", respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        [Obsolete]
        public async Task<GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>> GetDashboardRecruiterJobCatgPastAsync(DashboardRecruiterFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);
                if (Usr.UserTypeId != (byte)UserType.Recruiter)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not:" + UserType.Recruiter, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                var model = new List<DashboardRecruiterJobCategoryViewModel>();
                {
                    var dt = calcPrevDate(filterViewModel.DateFilter);
                    var rawDtls = await dbContext.GetDashboardRecruiterJobCategoryAsync(dt.fmDt, dt.toDt, Usr.UserTypeId, Usr.Id);

                    //59 - OpeningType - job opening types
                    var src = (await dbContext.PhRefMasters.Where(da => da.Status != (byte)RecordStatus.Delete && da.GroupId == 59)
                       .Select(da => da.Rmvalue).ToListAsync());
                    var cntSrc = rawDtls.GroupBy(da => da.JobCategory)
                        .Select(da => new DashboardRecruiterJobCategoryViewModel { JobCategory = da.Key, Count = da.Sum(dai => dai.resumeCount) ?? 0 })
                        .ToList();
                    src.ForEach(da =>
                        model.Add(new DashboardRecruiterJobCategoryViewModel
                        {
                            JobCategory = da,
                            Count = cntSrc.Where(dai => dai.JobCategory?.Trim().ToLower() == da?.Trim().ToLower()).Sum(dai => dai.Count)
                        }));
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        [Obsolete]
        public async Task<GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>> GetDashboardRecruiterJobCatgPresentAsync(DashboardRecruiterFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);
                if (Usr.UserTypeId != (byte)UserType.Recruiter)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not:" + UserType.Recruiter, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                var model = new List<DashboardRecruiterJobCategoryViewModel>();
                {
                    var dt = calcDate(filterViewModel.DateFilter, isMaxCurrent: true);
                    var rawDtls = await dbContext.GetDashboardRecruiterJobCategoryAsync(dt.fmDt, dt.toDt, Usr.UserTypeId, Usr.Id);


                    //59 - OpeningType - job opening types
                    var src = (await dbContext.PhRefMasters.Where(da => da.Status != (byte)RecordStatus.Delete && da.GroupId == 59)
                       .Select(da => da.Rmvalue).ToListAsync());
                    var cntSrc = new List<DashboardRecruiterJobCategoryViewModel>();
                    cntSrc = rawDtls.GroupBy(da => da.JobCategory)
                        .Select(da => new DashboardRecruiterJobCategoryViewModel { JobCategory = da.Key, Count = da.Sum(dai => dai.resumeCount) ?? 0 })
                        .ToList();
                    src.ForEach(da =>
                        model.Add(new DashboardRecruiterJobCategoryViewModel
                        {
                            JobCategory = da,
                            Count = cntSrc.Where(dai => dai.JobCategory?.Trim().ToLower() == da?.Trim().ToLower()).Sum(dai => dai.Count)
                        }));
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        [Obsolete]
        public async Task<GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>> GetDashboardRecruiterJobCatgAllAsync(DashboardRecruiterFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<DashboardRecruiterJobCategoryViewModel>>();
            try
            {
                //   logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);
                if (Usr.UserTypeId != (byte)UserType.Recruiter)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not:" + UserType.Recruiter, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                var model = new List<DashboardRecruiterJobCategoryViewModel>();
                {
                    var rawDtls = await dbContext.GetDashboardRecruiterJobCategoryAsync(null, null, Usr.UserTypeId, Usr.Id);

                    //59 - OpeningType - job opening types
                    var src = (await dbContext.PhRefMasters.Where(da => da.Status != (byte)RecordStatus.Delete && da.GroupId == 59)
                       .Select(da => da.Rmvalue).ToListAsync());
                    var cntSrc = new List<DashboardRecruiterJobCategoryViewModel>();
                    cntSrc = rawDtls.GroupBy(da => da.JobCategory)
                        .Select(da => new DashboardRecruiterJobCategoryViewModel { JobCategory = da.Key, Count = da.Sum(dai => dai.resumeCount) ?? 0 })
                        .ToList();
                    src.ForEach(da =>
                        model.Add(new DashboardRecruiterJobCategoryViewModel
                        {
                            JobCategory = da,
                            Count = cntSrc.Where(dai => dai.JobCategory?.Trim().ToLower() == da?.Trim().ToLower()).Sum(dai => dai.Count)
                        }));
                }

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        [Obsolete]
        public async Task<GetResponseViewModel<DashboardRecruiterAnalyticsViewModel>> GetDashboardAdminRecruiterAnalyticAsync(int recruiterId, DashboardRecruiterFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardRecruiterAnalyticsViewModel>();
            try
            {
                //   logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: recruiterId->" + recruiterId + ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);
                if (Usr.UserTypeId != (byte)UserType.Admin && Usr.UserTypeId != (byte)UserType.SuperAdmin)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not: " + UserType.Admin + " && " + UserType.SuperAdmin, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                var dt = calcDate(filterViewModel.DateFilter);
                var rawDtls = await dbContext.GetDashboardRecruiterAnalyticAsync(dt.fmDt, dt.toDt, (byte)UserType.Recruiter, recruiterId);
                var recruiter = await dbContext.PiHireUsers.AsNoTracking().Where(da => da.Id == recruiterId)
                    .Select(da => new { da.FirstName, da.LastName, da.Id, da.UserRoleName, da.Location, da.ProfilePhoto }).FirstOrDefaultAsync();
                var model = DashboardRecruiterAnalyticsViewModel.ToModel(rawDtls);

                model.RecruiterName = recruiter?.FirstName + " " + recruiter?.LastName;
                model.RecruiterPhoto = this.getEmployeePhotoUrl(recruiter?.ProfilePhoto, recruiter?.Id ?? 0);
                model.RecruiterLocation = recruiter?.Location;
                model.RecruiterRole = recruiter?.UserRoleName;
                #region Grph
                {
                    var grphDtls = await dbContext.GetDashboardRecruiterAnalyticGrphAsync(dt.fmDt, dt.toDt, (byte)UserType.Recruiter, recruiterId);
                    model.candOverTimeGrph = new List<DashboardRecruiterAnalyticsCandOverTimeGrpViewModel>();
                    var fmDt = dt.fmDt ?? (grphDtls.Where(da => da.ActivityDate.HasValue).Count() > 0 ? grphDtls.Where(da => da.ActivityDate.HasValue).Min(da => da.ActivityDate.Value.Date) : CurrentTime);
                    var toDt = dt.toDt ?? (grphDtls.Where(da => da.ActivityDate.HasValue).Count() > 0 ? grphDtls.Where(da => da.ActivityDate.HasValue).Max(da => da.ActivityDate.Value.Date) : CurrentTime);
                    for (; fmDt <= toDt; fmDt = fmDt.AddDays(1))
                    {
                        model.candOverTimeGrph.Add(new DashboardRecruiterAnalyticsCandOverTimeGrpViewModel
                        {
                            dt = fmDt,
                            hiredCount = grphDtls.Where(da => da.ActivityDate.HasValue && da.ActivityDate.Value.Date == fmDt && da.StatusCode == "SUC").Select(da => da.CandProfId).Distinct().Count(),
                            clientRejectCount = grphDtls.Where(da => da.ActivityDate.HasValue && da.ActivityDate.Value.Date == fmDt && da.StatusCode == "CRT").Select(da => da.CandProfId).Distinct().Count(),
                            candBackoutCount = grphDtls.Where(da => da.ActivityDate.HasValue && da.ActivityDate.Value.Date == fmDt && da.StatusCode == "CDB").Select(da => da.CandProfId).Distinct().Count()
                        });
                    }
                    model.hiredGrph =
                    grphDtls.Where(da => da.StatusCode == "SUC").GroupBy(da => da.JobTitle).Select(da => new DashboardRecruiterAnalyticsHiredGrpViewModel { jobTitle = da.Key, hiredCount = da.Select(dai => dai.CandProfId).Distinct().Count() }).ToList();

                    model.rawHiredSalaryGrph =
                    grphDtls.Where(da => da.StatusCode == "SUC" && da.OPGrossPayPerMonth.HasValue).GroupBy(da => da.OPGrossPayPerMonth.Value).Select(da => new DashboardRecruiterAnalyticsSalRngGrpViewModel { strt = da.Key, hiredCount = da.Select(dai => dai.CandProfId).Distinct().Count() }).ToList();
                    model.hiredSalaryGrph = new List<DashboardRecruiterAnalyticsSalRngGrpViewModel>();
                    if (model.rawHiredSalaryGrph.Count > 0)
                    {
                        model.hiredSalaryGrph.Add(new DashboardRecruiterAnalyticsSalRngGrpViewModel() { strt = 0, end = 1000 });
                        model.hiredSalaryGrph.Add(new DashboardRecruiterAnalyticsSalRngGrpViewModel() { strt = 1001, end = 5000 });
                        model.hiredSalaryGrph.Add(new DashboardRecruiterAnalyticsSalRngGrpViewModel() { strt = 5001, end = 10000 });
                        model.hiredSalaryGrph.Add(new DashboardRecruiterAnalyticsSalRngGrpViewModel() { strt = 10001, end = 15000 });
                        model.hiredSalaryGrph.Add(new DashboardRecruiterAnalyticsSalRngGrpViewModel() { strt = 15001, end = 20000 });
                        model.hiredSalaryGrph.Add(new DashboardRecruiterAnalyticsSalRngGrpViewModel() { strt = 20001, end = 25000 });
                        model.hiredSalaryGrph.Add(new DashboardRecruiterAnalyticsSalRngGrpViewModel() { strt = 25001, end = 0 });
                        foreach (var hiredSalaryGrph in model.hiredSalaryGrph)
                        {
                            if (model.rawHiredSalaryGrph.Where(dai => hiredSalaryGrph.strt <= dai.strt && dai.strt <= hiredSalaryGrph.end).Count() > 0)
                                hiredSalaryGrph.hiredCount = model.rawHiredSalaryGrph.Where(dai => hiredSalaryGrph.strt <= dai.strt && dai.strt <= hiredSalaryGrph.end).Sum(dai => dai.hiredCount);
                        }
                    }
                }
                #endregion
                model.filterFromDt = dt.fmDt;
                model.filterToDt = dt.toDt;

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", recruiterId->" + recruiterId + ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<double?>> GetDashboardRecruiterAvgHireDaysAsync(int recruiterId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<double?>();
            try
            {
                //    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: recruiterId->" + recruiterId, respModel.Meta.RequestID);
                if (Usr.UserTypeId != (byte)UserType.Admin && Usr.UserTypeId != (byte)UserType.SuperAdmin)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not: " + UserType.Admin + " && " + UserType.SuperAdmin, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                var recJoiningDate = await dbContext.PiHireUsers.Where(da => da.Id == recruiterId).Select(da => da.CreatedDate).FirstOrDefaultAsync();
                var candSuccessStatusId = await dbContext.PhCandStatusSes.Where(da => da.Status != (byte)RecordStatus.Delete && da.Cscode == "SUC").Select(da => da.Id).FirstOrDefaultAsync();
                var successJoindaysCount = await dbContext.PhJobCandidates.Where(da => da.Status != (byte)RecordStatus.Delete && da.RecruiterId == recruiterId && da.CandProfStatus == candSuccessStatusId &&
                 da.OpconfirmDate.HasValue).Select(da => da.OpconfirmDate.Value).Distinct().LongCountAsync();

                double successRate = successJoindaysCount > 0 ? (CurrentTime - recJoiningDate).TotalDays / successJoindaysCount : 0;

                respModel.Status = true;
                respModel.SetResult(successRate);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", recruiterId->" + recruiterId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        [Obsolete]
        public async Task<GetResponseViewModel<DashboardJobTimeViewModel>> GetDashboardJobTimeAsync(int jobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardJobTimeViewModel>();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: jobId->" + jobId, respModel.Meta.RequestID);
                var jobCloseDt = await dbContext.PhJobOpenings.Where(da => da.Id == jobId).Select(da => da.ClosedDate).FirstOrDefaultAsync();
                var jobAssignments = await dbContext.PhJobAssignments.Where(da => da.Joid == jobId && da.Status != (byte)RecordStatus.Delete)
                    .Select(da => new { da.DeassignDate, da.CreatedDate }).ToListAsync();
                TimeSpan timeSpent = new TimeSpan();
                foreach (var jobAssignment in jobAssignments)
                {
                    var tm = (jobAssignment.DeassignDate ?? CurrentTime) - jobAssignment.CreatedDate;
                    timeSpent = timeSpent.Add(tm);
                }
                TimeSpan timeRemaining = new TimeSpan();
                if (jobCloseDt > CurrentTime)
                {
                    timeRemaining = jobCloseDt - CurrentTime;
                }
                var model = new DashboardJobTimeViewModel()
                {
                    timeSpent = timeSpent,
                    timeSpentDays = timeSpent.Days,
                    timeSpentHours = timeSpent.Hours,
                    timeSpentMinutes = timeSpent.Minutes,
                    timeRemaining = timeRemaining,
                    timeRemainingDays = timeRemaining.Days,
                    timeRemainingHours = timeRemaining.Hours,
                    timeRemainingMinutes = timeRemaining.Minutes
                };

                respModel.Status = true;
                respModel.SetResult(model);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", jobId->" + jobId, respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        [Obsolete]
        public async Task<GetResponseViewModel<DashboardRecruiterCandidateViewModel>> GetDashboardRecruiterCandidatesPastAsync(DashboardRecruiterFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardRecruiterCandidateViewModel>();
            try
            {
                //   logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);
                if (Usr.UserTypeId != (byte)UserType.Recruiter)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not:" + UserType.Recruiter, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                {
                    var dt = calcPrevDate(filterViewModel.DateFilter);
                    var rawDtls = await dbContext.GetDashboardRecruiterCandidatesAsync(dt.fmDt, dt.toDt, Usr.UserTypeId, Usr.Id);

                    respModel.Status = true;
                    respModel.SetResult(new DashboardRecruiterCandidateViewModel
                    {
                        candidates = rawDtls.Select(da => DashboardRecruiterCandidateViewModel_cands.ToViewModel(da)).ToList(),
                        filterFromDt = dt.fmDt,
                        filterToDt = dt.toDt,
                        counts = rawDtls.GroupBy(da => da.StatusCode).ToDictionary(da => da.Key, da2 => da2.Count())
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        [Obsolete]
        public async Task<GetResponseViewModel<DashboardRecruiterCandidateViewModel>> GetDashboardRecruiterCandidatesPresentAsync(DashboardRecruiterFilterViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardRecruiterCandidateViewModel>();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID);
                if (Usr.UserTypeId != (byte)UserType.Recruiter)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, "Login user not:" + UserType.Recruiter, true);
                    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, respModel.Meta.Error?.ErrorMessage ?? "");
                    respModel.Result = null;
                }
                var model = new List<DashboardRecruiterJobCategoryViewModel>();
                {
                    var dt = calcDate(filterViewModel.DateFilter);
                    var rawDtls = await dbContext.GetDashboardRecruiterCandidatesAsync(dt.fmDt, dt.toDt, Usr.UserTypeId, Usr.Id);


                    respModel.Status = true;
                    respModel.SetResult(new DashboardRecruiterCandidateViewModel
                    {
                        candidates = rawDtls.Select(da => DashboardRecruiterCandidateViewModel_cands.ToViewModel(da)).ToList(),
                        filterFromDt = dt.fmDt,
                        filterToDt = dt.toDt,
                        counts = rawDtls.GroupBy(da => da.StatusCode).ToDictionary(da => da.Key, da2 => da2.Count())
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        #endregion


        #region New Dashboard 
        public async Task<GetResponseViewModel<List<Sp_BroughtBy_Job_ClientNamesModel>>> GetBroughtByJobClientNamesAsync(int boughtBy, int? puId = null)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<Sp_BroughtBy_Job_ClientNamesModel>>();
            try
            {
                var dtls = await dbContext.GetBroughtByJobClientNamesAsync(boughtBy, puId, Usr.UserTypeId, Usr.Id);
                respModel.SetResult(dtls);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, $"boughtBy:{boughtBy}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<List<Sp_BroughtBy_Job_ClientNamesModel>>> GetBroughtByDayWiseJobClientNamesAsync(int boughtBy, DayWiseBoughtByAccountSearchModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<Sp_BroughtBy_Job_ClientNamesModel>>();
            try
            {
                if (model.FromDate == null)
                {
                    var dt = calcDate(model.DateFilter);
                    model.FromDate = dt.fmDt;
                    model.ToDate = dt.toDt;
                }
                var dtls = await dbContext.GetBroughtByDayWiseJobClientNamesAsync(boughtBy, model.ProcessUnit, model.FromDate, model.ToDate, Usr.UserTypeId, Usr.Id);
                respModel.SetResult(dtls);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, $"boughtBy:{boughtBy}, model:{JsonConvert.SerializeObject(model)}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }
        public async Task<GetResponseViewModel<List<Sp_BroughtBy_Job_ClientNamesModel>>> GetBroughtByInterviewClientNamesAsync(int boughtBy)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<Sp_BroughtBy_Job_ClientNamesModel>>();
            try
            {
                var dtls = await dbContext.GetBroughtByInterviewClientNamesAsync(boughtBy, Usr.UserTypeId, Usr.Id);
                respModel.SetResult(dtls);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, $"boughtBy:{boughtBy}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<DashboardJobListViewModel>> JobsList(HireAssignmentsSearchViewModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            string Sort = string.Empty;
            string SortDirection = string.Empty;
            int UserId = Usr.Id;

            var respModel = new GetResponseViewModel<DashboardJobListViewModel>();
            try
            {

                var Openings = new DashboardJobListViewModel();
                model.CurrentPage = (model.CurrentPage.Value - 1) * model.PerPage.Value;
                if (model.FromDate == null)
                {
                    var dt = calcDate(model.DateFilter);
                    model.FromDate = dt.fmDt;
                    model.ToDate = dt.toDt;
                }

                var dtls = await dbContext.GetAssignmentJobsList(Usr.UserTypeId, UserId, model.SearchKey, model.BroughtBy, model.ProcessUnit, model.JobPriority, model.FromDate, model.ToDate, model.PerPage, model.CurrentPage, model.ClientId, model.JobStatus, model.AssignmentStatus);

                if (dtls?.JobList.Count > 0)
                {
                    Openings.OpeningList = new List<DashboardJobsList>();
                    Openings.OpeningCount = dtls.JobCount;

                    var joIds = dtls.JobList.Select(x => x.Id).ToArray();

                    var stages = await dbContext.PhCandStagesSes.Where(x => x.Status != (byte)RecordStatus.Delete).ToListAsync();

                    var counters = await dbContext.PhJobOpeningStatusCounters.Where(x => joIds.Contains(x.Joid) && x.Status != (byte)RecordStatus.Delete).ToListAsync();

                    foreach (var item in dtls.JobList)
                    {
                        item.JobOpeningStatusCounter = new List<JobOpeningStatusCounterViewModel>();
                        foreach (var stats in stages)
                        {
                            item.JobOpeningStatusCounter.Add(new JobOpeningStatusCounterViewModel
                            {
                                StageID = stats.Id,
                                StageColor = stats.ColorCode,
                                StageName = stats.Title,
                                Counter = counters.Where(x => x.Joid == item.Id && x.StageId == stats.Id).Sum(x => x.Counter)
                            });
                        }
                        Openings.OpeningList.Add(item);
                    }
                }

                respModel.Status = true;
                respModel.SetResult(Openings);
            }
            catch (Exception ex)
            {

                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(model), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }

            return respModel;
        }

        public async Task<GetResponseViewModel<DashboardJobListViewModel>> DayWiseJobsList(DayWiseAssignmentsJobsSearchViewModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            string Sort = string.Empty;
            string SortDirection = string.Empty;
            int UserId = Usr.Id;

            var respModel = new GetResponseViewModel<DashboardJobListViewModel>();
            try
            {

                var Openings = new DashboardJobListViewModel();
                model.CurrentPage = (model.CurrentPage.Value - 1) * model.PerPage.Value;
                if (model.FromDate == null)
                {
                    var dt = calcDate(model.DateFilter);
                    model.FromDate = dt.fmDt;
                    model.ToDate = dt.toDt;
                }

                var dtls = await dbContext.GetDayWiseAssignmentJobsList(Usr.UserTypeId, UserId, model.SearchKey, model.BroughtBy, model.ProcessUnit, model.JobPriority,
                    model.Assign, model.PriorityUpdate, model.Note, model.Interviews, model.JobStatus,
                    model.FromDate, model.ToDate, model.PerPage, model.CurrentPage, model.ClientId);

                if (dtls?.JobList.Count > 0)
                {
                    Openings.OpeningList = new List<DashboardJobsList>();
                    Openings.OpeningCount = dtls.JobCount;

                    var joIds = dtls.JobList.Select(x => x.Id).ToArray();

                    var stages = await dbContext.PhCandStagesSes.Where(x => x.Status != (byte)RecordStatus.Delete).ToListAsync();

                    var counters = await dbContext.PhJobOpeningStatusCounters.Where(x => joIds.Contains(x.Joid) && x.Status != (byte)RecordStatus.Delete).ToListAsync();

                    foreach (var item in dtls.JobList)
                    {
                        item.JobOpeningStatusCounter = new List<JobOpeningStatusCounterViewModel>();
                        foreach (var stats in stages)
                        {
                            item.JobOpeningStatusCounter.Add(new JobOpeningStatusCounterViewModel
                            {
                                StageID = stats.Id,
                                StageColor = stats.ColorCode,
                                StageName = stats.Title,
                                Counter = counters.Where(x => x.Joid == item.Id && x.StageId == stats.Id).Sum(x => x.Counter)
                            });
                        }
                        Openings.OpeningList.Add(item);
                    }
                }

                respModel.Status = true;
                respModel.SetResult(Openings);
            }
            catch (Exception ex)
            {

                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(model), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }

            return respModel;
        }

        public async Task<GetResponseViewModel<_JobCandidatesBasedOnProfileStatusViewModel>> GetJobCandidatesBasedOnProfileStatusAsync(CandidateProfileStatusSearchViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<_JobCandidatesBasedOnProfileStatusViewModel>();
            try
            {
                filterViewModel.CurrentPage = (filterViewModel.CurrentPage.Value - 1) * filterViewModel.PerPage.Value;
                var model = new _JobCandidatesBasedOnProfileStatusViewModel();
                model.CandidatesViewModel = new List<JobCandidatesBasedOnProfileStatusViewModel>();

                model.CandidatesViewModel = await dbContext.GetJobCandidatesBasedStatusAsync(filterViewModel.SearchKey, filterViewModel.JobId, filterViewModel.ProfileStatus, filterViewModel.PerPage, filterViewModel.CurrentPage);

                var interviewTotCnt = await dbContext.GetJobCandidatesBasedStatusCountAsync(filterViewModel.SearchKey, filterViewModel.JobId, filterViewModel.ProfileStatus);

                model.CandidateCount = interviewTotCnt.TotCnt;

                respModel.SetResult(model);

                respModel.Status = true;

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<JobDescriptionViewModel>> GetJobDescriptionWithPipeline(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            byte Usertype = Usr.UserTypeId;
            //int? EmpId = Usr.EmpId;
            var respModel = new GetResponseViewModel<JobDescriptionViewModel>();
            try
            {
                var Job = new JobDescriptionViewModel();

                Job = await (from opn in dbContext.PhJobOpenings
                             join Stus in dbContext.PhJobStatusSes on opn.JobOpeningStatus equals Stus.Id
                             join opnDtls in dbContext.PhJobOpeningsAddlDetails on opn.Id equals opnDtls.Joid
                             join prty in dbContext.PhRefMasters on opn.Priority equals prty.Id
                             join cunty in dbContext.PhCountries on opn.CountryId equals cunty.Id
                             join user in dbContext.PiHireUsers on opn.CreatedBy equals user.Id
                             join cur in dbContext.PhRefMasters on opnDtls.CurrencyId equals cur.Id into ps
                             from cur in ps.DefaultIfEmpty()
                             join tnr in dbContext.PhRefMasters on opnDtls.JobTenure equals tnr.Id into tnr_
                             from tnr in tnr_.DefaultIfEmpty()
                             join ct in dbContext.PhCities on opn.JobLocationId equals ct.Id into pp
                             from ct in pp.DefaultIfEmpty()
                             join brt in dbContext.PiHireUsers on opn.BroughtBy equals brt.Id into pr
                             from brt in pr.DefaultIfEmpty()
                             where opn.Id == Id
                             select new JobDescriptionViewModel
                             {
                                 CreatedByName = user.FirstName + " " + user.LastName,
                                 CreatedByProfilePhoto = user.ProfilePhoto,
                                 BroughtByProfilePhoto = brt.ProfilePhoto,
                                 CreatedByRole = user.UserRoleName,
                                 NoticePeriod = opnDtls.NoticePeriod,
                                 JobTenure = tnr.Rmvalue,// opnDtls.JobTenure,
                                 Country = cunty.Nicename,
                                 ClientName = opn.ClientName,
                                 ClientId = opn.ClientId,
                                 SpocId = opnDtls.Spocid,
                                 CreatedDate = opn.CreatedDate,
                                 JobRole = opn.JobRole,
                                 CurrencyId = opnDtls.CurrencyId,
                                 CurrencyName = cur.Rmvalue,
                                 JobId = opn.Id,
                                 JobTitle = opn.JobTitle,
                                 JobStatus = opn.JobOpeningStatus,
                                 JobStatusCode = Stus.Jscode,
                                 JobStatusName = Stus.Title,
                                 NoOfPositions = opn.NoOfPositions,
                                 NoOfCvsRequired = opnDtls.NoOfCvsRequired,


                                 JoiningDate = opnDtls.ApprJoinDate,
                                 StartDate = opn.PostedDate,
                                 TargetDate = opn.ClosedDate,
                                 ReopenedDate = opn.ReopenedDate,


                                 MaxSalary = opnDtls.MaxSalary,
                                 MinSalary = opnDtls.MinSalary,
                                 EmploymentType = opn.JobCategory,
                                 JobDescription = opn.JobDescription,
                                 ShortJobDesc = opn.ShortJobDesc,
                                 MaxExpeInMonths = opn.MaxExpeInMonths,
                                 MinExpeInMonths = opn.MinExpeInMonths,
                                 BroughtBy = opn.BroughtBy,

                                 BroughtByName = brt.FirstName + " " + brt.LastName,
                                 BroughtByRole = brt.UserRoleName,

                                 AccountManagerName = string.Empty,

                                 City = ct.Name,
                                 PuId = opnDtls.Puid,

                                 BuId = opnDtls.Buid,
                                 BuName = string.Empty,

                                 CreatedBy = opn.CreatedBy,

                                 Priority = opn.Priority,
                                 PriorityName = prty.Rmvalue,


                             }).FirstOrDefaultAsync();
                if (Job != null)
                {

                    Job.JobDescSkillViewModel = (from tech in dbContext.PhJobOpeningSkills
                                                 where tech.Joid == Job.JobId && tech.Status == (byte)RecordStatus.Active
                                                 select new JobSkillViewModel
                                                 {
                                                     ExpInMonths = tech.ExpMonth,
                                                     ExpInYears = tech.ExpYears,
                                                     TechnologyName = tech.Technology
                                                 }).ToList();

                    Job.JobTeam = (from jobAsmt in dbContext.PhJobAssignments
                                   join usr in dbContext.PiHireUsers on jobAsmt.AssignedTo equals usr.Id
                                   where jobAsmt.Joid == Job.JobId
                                   select new JobWorkingTeamViewModl
                                   {
                                       Name = usr.FirstName + " " + usr.LastName,
                                       UserId = usr.Id,
                                       Location = usr.Location,
                                       Role = usr.UserRoleName,
                                       NoOfCv = jobAsmt.NoCvsrequired,
                                       NoOfFinalCvsFilled = jobAsmt.NoOfFinalCvsFilled,
                                       DeAssignDate = jobAsmt.DeassignDate,
                                       ProfilePhoto = usr.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + usr.Id + "/ProfilePhoto/" + usr.ProfilePhoto : string.Empty
                                   }).ToList();


                    Job.MaxYear = ConvertYears(Job.MaxExpeInMonths);
                    Job.MinYear = ConvertYears(Job.MinExpeInMonths);

                    var cvDetails = Job.JobTeam;
                    if (cvDetails != null && cvDetails.Count > 0)
                    {
                        var NoOfCvsRequired = cvDetails.Where(x => x.DeAssignDate == null).Sum(x => x.NoOfCv);
                        if (NoOfCvsRequired > Job.NoOfCvsRequired)
                        {
                            Job.NoOfCvsRequired = NoOfCvsRequired;
                        }
                        Job.NoOfFinalCVsFilled = cvDetails.Where(x => x.DeAssignDate == null).Sum(x => x.NoOfFinalCvsFilled);
                    }

                    if (Job.ReopenedDate == null)
                    {
                        Job.JobType = "NEW";
                        Job.JobAge = Convert.ToInt32((Job.TargetDate - Job.StartDate.Value).TotalDays);
                        Job.CompletedAge = Convert.ToInt32((CurrentTime - Job.StartDate.Value).TotalDays);
                    }
                    else
                    {
                        Job.JobType = "REWORK";
                        Job.JobAge = Convert.ToInt32((Job.TargetDate - Job.ReopenedDate.Value).TotalDays);
                        Job.CompletedAge = Convert.ToInt32((CurrentTime - Job.ReopenedDate.Value).TotalDays);
                    }


                    var GetClient = await dbContext.GetClient(Job.ClientId);
                    if (GetClient.Count > 0)
                    {
                        Job.AccountManagerName = GetClient.Where(x => x.Id == Job.ClientId).Select(x => x.AccountManagerName).FirstOrDefault();
                    }

                    var Spocs = await dbContext.GetClientSpocs(Job.ClientId);
                    if (Spocs.Count > 0)
                    {
                        Job.SpocName = Spocs.Where(x => x.Id == Job.SpocId).Select(x => x.Name).FirstOrDefault();
                    }

                    var getPUs = await dbContext.GetPUs();
                    if (getPUs.Count > 0)
                    {
                        var pudtls = getPUs.Where(x => x.Id == Job.PuId).FirstOrDefault();
                        if (pudtls != null)
                        {
                            Job.PuName = pudtls.Name;
                            Job.PuShortName = pudtls.ShortName;
                        }
                    }

                    string PuId = Job.PuId.ToString();
                    var getBUs = await dbContext.GetBUs(PuId);
                    if (getBUs.Count > 0)
                    {
                        var budtls = getBUs.Where(x => x.Id == Job.BuId).FirstOrDefault();
                        if (budtls != null)
                        {
                            Job.BuName = budtls.Name;
                        }
                    }

                    Job.CreatedByProfilePhoto = Job.CreatedByProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + Job.CreatedBy + "/ProfilePhoto/" + Job.CreatedByProfilePhoto : string.Empty;
                    Job.BroughtByProfilePhoto = Job.BroughtByProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + Job.BroughtBy + "/ProfilePhoto/" + Job.BroughtByProfilePhoto : string.Empty;

                    var stages = await dbContext.PhCandStagesSes.Where(x => x.Status != (byte)RecordStatus.Delete).ToListAsync();
                    var counters = await dbContext.PhJobOpeningStatusCounters.Where(x => x.Joid == Job.JobId && x.Status != (byte)RecordStatus.Delete).ToListAsync();



                    Job.JobOpeningStatusCounter = new List<JobOpeningStatusCounterViewModel>();
                    foreach (var stats in stages)
                    {
                        Job.JobOpeningStatusCounter.Add(new JobOpeningStatusCounterViewModel
                        {
                            StageID = stats.Id,
                            StageColor = stats.ColorCode,
                            StageName = stats.Title,
                            Counter = counters.Where(x => x.Joid == Job.JobId && x.StageId == stats.Id).Sum(x => x.Counter)
                        });
                    }
                }

                respModel.Status = true;
                respModel.SetResult(Job);
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

        public async Task<GetResponseViewModel<List<CandidateStageWiseViewModel>>> GetCandidateStageWiseInfo(int JobId, int CandProfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var resp = new List<CandidateStageWiseViewModel>();
            var respModel = new GetResponseViewModel<List<CandidateStageWiseViewModel>>();

            resp = await dbContext.GetCandidateStageWiseInfoAsync(JobId, CandProfId);

            respModel.SetResult(resp);

            return respModel;
        }

        public async Task<GetResponseViewModel<DashboardCandidateInterviewViewModel>> GetCandidateInterviewsAsync(CandidateInterviewSearchViewModel filterViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserType = Usr.UserTypeId;
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<DashboardCandidateInterviewViewModel>();
            try
            {
                filterViewModel.CurrentPage = (filterViewModel.CurrentPage.Value - 1) * filterViewModel.PerPage.Value;

                if (filterViewModel.FromDate == null)
                {
                    var dt = calcDate(filterViewModel.DateFilter);
                    filterViewModel.FromDate = dt.fmDt;
                    filterViewModel.ToDate = dt.toDt;
                }

                var dtls = await dbContext.GetDashboardCandidateInterviewsAsync(filterViewModel.SearchKey, filterViewModel.JobId, filterViewModel.PuId, filterViewModel.BDMId, filterViewModel.RecId, filterViewModel.Tab, filterViewModel.FromDate, filterViewModel.ToDate, filterViewModel.ClientId, loginUserType, loginUserId, filterViewModel.PerPage, filterViewModel.CurrentPage);

                var interviewTotCnt = await dbContext.GetDashboardCandidateInterviewsCountAsync(filterViewModel.SearchKey, filterViewModel.JobId, filterViewModel.PuId, filterViewModel.BDMId, filterViewModel.RecId, filterViewModel.FromDate, filterViewModel.ToDate, filterViewModel.ClientId, loginUserType, loginUserId);


                respModel.SetResult(new DashboardCandidateInterviewViewModel
                {
                    interviews = dtls.Select(da => _DashboardCandidateInterviewViewModel.ToViewModel(da, this)).ToList(),
                    interviewStageStatuses = interviewTotCnt
                });
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(filterViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<UpdateResponseViewModel<string>> JobPriorityUpdateAsync(PriorityChangeViewModel item)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            int UserId = Usr.Id;

            try
            {
                var jobDtls = await dbContext.PhJobOpenings.Where(x => x.Id == item.JoId).FirstOrDefaultAsync();
                if (jobDtls != null)
                {
                    var refDtls = dbContext.PhRefMasters.Where(x => x.Id == item.Priority).FirstOrDefault();

                    jobDtls.Priority = item.Priority;
                    jobDtls.UpdatedBy = UserId;
                    jobDtls.UpdatedDate = CurrentTime;

                    dbContext.PhJobOpenings.Update(jobDtls);
                    await dbContext.SaveChangesAsync();

                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        // Day Wise Actions 
                        var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == item.JoId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                        if (IsDayWiseAction == null)
                        {
                            IsDayWiseAction = new PhDayWiseJobAction
                            {
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                Joid = item.JoId,
                                Priority = true
                            };
                            dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            IsDayWiseAction.Priority = true;
                            dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                    }


                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        ActivityOn = item.JoId,
                        JobId = item.JoId,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has updated priority of " + jobDtls.JobTitle + " (" + item.JoId + ") to " + refDtls?.Rmvalue + " ",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Job is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
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

        public async Task<UpdateResponseViewModel<string>> JobRecruiterPriorityUpdateAsync(PriorityChangeViewModel item)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            int UserId = Usr.Id;

            try
            {
                var jobDtls = await dbContext.PhJobOpenings.Where(x => x.Id == item.JoId).FirstOrDefaultAsync();
                if (item.Recruiter > 0 && jobDtls != null)
                {
                    var recDtls = dbContext.PiHireUsers.Where(x => x.Id == item.Recruiter && x.UserType != (byte)UserType.Candidate).FirstOrDefault();

                    var jobPrioriyDtls = await dbContext.PhJobRecruiterPriorities.Where(x => x.Joid == item.JoId && x.AssignedTo == item.Recruiter).FirstOrDefaultAsync();
                    if (jobPrioriyDtls != null)
                    {
                        jobPrioriyDtls.Priority = item.Priority;
                        jobPrioriyDtls.UpdatedBy = UserId;
                        jobPrioriyDtls.UpdatedDate = CurrentTime;

                        dbContext.PhJobRecruiterPriorities.Update(jobPrioriyDtls);
                        await dbContext.SaveChangesAsync();


                        respModel.Status = true;
                        respModel.SetResult(message);
                    }
                    else
                    {
                        var PhJobRecruiterPriority = new PhJobRecruiterPriority
                        {
                            Joid = item.JoId,
                            AssignedTo = item.Recruiter.Value,
                            CreatedDate = CurrentTime,
                            CreatedBy = UserId,
                            Priority = item.Priority,
                            Status = (byte)RecordStatus.Active
                        };
                        dbContext.PhJobRecruiterPriorities.Add(PhJobRecruiterPriority);
                        await dbContext.SaveChangesAsync();

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }

                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        // Day Wise Actions 
                        var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == item.JoId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                        if (IsDayWiseAction == null)
                        {
                            IsDayWiseAction = new PhDayWiseJobAction
                            {
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                Joid = item.JoId
                            };
                            dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            IsDayWiseAction.Priority = true;
                            dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                    }


                    var refDtls = dbContext.PhRefMasters.Where(x => x.Id == item.Priority).FirstOrDefault();
                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        ActivityOn = item.JoId,
                        JobId = item.JoId,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has updated priority of " + jobDtls.JobTitle + " (" + item.JoId + ") to " + refDtls?.Rmvalue + " under the recruiter " + recDtls?.FirstName + " " + recDtls?.LastName + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.Status = true;
                    respModel.SetResult(message);

                }
                else
                {
                    message = "Recruiter is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
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


        #region Reports


        public async Task<GetResponseViewModel<RecruitersVM>> GetRecruitersOverview(ReportRequestViewModel reportRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<RecruitersVM>();
            var recruitersVM = new RecruitersVM();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID);
                if (reportRequestViewModel == null)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidJsonFormat, "Invalid Request Json format", true);
                }
                else
                {
                    if (reportRequestViewModel.FromDate == null)
                    {
                        var dt = calcDate(reportRequestViewModel.DateFilter);
                        reportRequestViewModel.FromDate = dt.fmDt;
                        reportRequestViewModel.ToDate = dt.toDt;
                    }
                    else
                    {
                        if (reportRequestViewModel.ToDate == null)
                        {
                            reportRequestViewModel.ToDate = CurrentTime;
                        }
                    }

                    var response = await dbContext.GetRecruitersOverview(reportRequestViewModel.FromDate.Value, reportRequestViewModel.ToDate.Value, Usr.UserTypeId, Usr.Id, reportRequestViewModel.PuId);
                    foreach (var item in response)
                    {
                        item.ProfilePhoto = getEmployeePhotoUrl(item.ProfilePhoto, item.UserId);
                    }

                    recruitersVM.RecruitersOverviewModel = response;
                    recruitersVM.FromDate = reportRequestViewModel.FromDate.Value;
                    recruitersVM.ToDate = reportRequestViewModel.ToDate.Value;
                    respModel.Status = true;
                    respModel.SetResult(recruitersVM);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<RecruiterReportModel>> GetRecruiterOverview(ReportRequestViewModel reportRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<RecruiterReportModel>();
            var response = new RecruiterReportModel
            {
                RecruiterOverviewModel = new List<RecruiterOverviewModel>(),
                JobsCount = 0,
                AccountsCount = 0
            };
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID);
                if (reportRequestViewModel == null)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidJsonFormat, "Invalid Request Json format", true);
                }
                else
                {
                    if (reportRequestViewModel.FromDate == null)
                    {
                        var dt = calcDate(reportRequestViewModel.DateFilter);
                        reportRequestViewModel.FromDate = dt.fmDt;
                        reportRequestViewModel.ToDate = dt.toDt;
                    }
                    else
                    {
                        if (reportRequestViewModel.ToDate == null)
                        {
                            reportRequestViewModel.ToDate = CurrentTime;
                        }
                    }
                    response.FromDate = reportRequestViewModel.FromDate.Value;
                    response.ToDate = reportRequestViewModel.ToDate.Value;

                    List<RecruiterOverviewModel> records = await dbContext.GetRecruiterOverview(reportRequestViewModel.FromDate.Value,
                        reportRequestViewModel.ToDate.Value, reportRequestViewModel.PuId, reportRequestViewModel.StatusCode, Usr.UserTypeId,
                        Usr.Id, reportRequestViewModel.UserId);

                    response.RecruiterOverviewModel = records;
                    int ttlCount = records.Count;
                    if (ttlCount > 0)
                    {
                        foreach (var RecruiterOverviewModel in response.RecruiterOverviewModel)
                        {
                            RecruiterOverviewModel.RecruiterProfilePhoto = getEmployeePhotoUrl(RecruiterOverviewModel.RecruiterProfilePhoto, RecruiterOverviewModel.RecruiterId ?? 0);
                            RecruiterOverviewModel.BroughtbyProfilePhoto = getEmployeePhotoUrl(RecruiterOverviewModel.BroughtbyProfilePhoto, RecruiterOverviewModel.BroughtBy ?? 0);
                        }
                        response.PerformanceOnStatusWise = new List<PerformanceOnFieldWise>();

                        var uniqueJobCount = new List<int>();
                        foreach (var item in response.RecruiterOverviewModel)
                        {
                            if (!uniqueJobCount.Contains(item.JobId))
                            {
                                uniqueJobCount.Add(item.JobId);
                            }
                        }
                        response.JobsCount = uniqueJobCount.Count;

                        // Account
                        var accountDtls = from r in response.RecruiterOverviewModel
                                          orderby r.ActivityDate
                                          group r by r.ClientID into grp
                                          select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in accountDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.Account,
                                RecId = item.data.RecruiterId,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                RecruiterName = item.data.RecruiterName,
                                BdmProfilePhoto = item.data.BroughtbyProfilePhoto,
                                BdmName = item.data.BroughtByName,
                                BdmId = item.data.BroughtBy,
                                CreatedDate = item.data.ActivityDate,
                                JobStatus = item.data.JobStatus,
                                Title = item.data.ClientName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }

                        response.AccountsCount = accountDtls.Count();

                        // Job title
                        var jobDtls = from r in response.RecruiterOverviewModel
                                      orderby r.ActivityDate
                                      group r by r.JobId into grp
                                      select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in jobDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.Opening,
                                RecId = item.data.RecruiterId,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                RecruiterName = item.data.RecruiterName,
                                BdmProfilePhoto = item.data.BroughtbyProfilePhoto,
                                BdmName = item.data.BroughtByName,
                                BdmId = item.data.BroughtBy,
                                Title = item.data.JobTitle,
                                CreatedDate = item.data.ActivityDate,
                                JobStatus = item.data.JobStatus,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }

                        // BDM
                        var broughtDtls = from r in response.RecruiterOverviewModel
                                          orderby r.ActivityDate
                                          group r by r.BroughtBy into grp
                                          select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in broughtDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.BDM,
                                RecId = item.data.RecruiterId,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                RecruiterName = item.data.RecruiterName,
                                BdmProfilePhoto = item.data.BroughtbyProfilePhoto,
                                BdmName = item.data.BroughtByName,
                                BdmId = item.data.BroughtBy,
                                CreatedDate = item.data.ActivityDate,
                                JobStatus = item.data.JobStatus,
                                Title = item.data.BroughtByName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }

                        // Recruiter 
                        var recruiterDtls = from r in response.RecruiterOverviewModel
                                            orderby r.ActivityDate
                                            group r by r.RecruiterId into grp
                                            select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in recruiterDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.Recruiter,
                                RecId = item.data.RecruiterId,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                RecruiterName = item.data.RecruiterName,
                                BdmProfilePhoto = item.data.BroughtbyProfilePhoto,
                                BdmName = item.data.BroughtByName,
                                BdmId = item.data.BroughtBy,
                                CreatedDate = item.data.ActivityDate,
                                JobStatus = item.data.JobStatus,
                                Title = item.data.RecruiterName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }


                    }

                    respModel.Status = true;
                    respModel.SetResult(response);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<RecruiterOpeningReportModel>> GetRecruiterOpeningOverview(ReportRequestViewModel reportRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<RecruiterOpeningReportModel>();
            var response = new RecruiterOpeningReportModel
            {
                RecruiterOpeningOverviewModel = new List<RecruiterOpeningOverviewModel>(),
                JobsCount = 0,
                AccountsCount = 0
            };
            try
            {
                //   logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID);
                if (reportRequestViewModel == null)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidJsonFormat, "Invalid Request Json format", true);
                }
                else
                {
                    if (reportRequestViewModel.FromDate == null)
                    {
                        var dt = calcDate(reportRequestViewModel.DateFilter);
                        reportRequestViewModel.FromDate = dt.fmDt;
                        reportRequestViewModel.ToDate = dt.toDt;
                    }
                    else
                    {
                        if (reportRequestViewModel.ToDate == null)
                        {
                            reportRequestViewModel.ToDate = CurrentTime;
                        }
                    }
                    response.FromDate = reportRequestViewModel.FromDate.Value;
                    response.ToDate = reportRequestViewModel.ToDate.Value;

                    var records = await dbContext.GetRecruiterOpeningOverview(reportRequestViewModel.FromDate.Value,
                        reportRequestViewModel.ToDate.Value, Usr.Id, Usr.UserTypeId, reportRequestViewModel.PuId, reportRequestViewModel.UserId);

                    response.RecruiterOpeningOverviewModel = records;
                    foreach (var RecruiterOpeningOverviewModel in response.RecruiterOpeningOverviewModel)
                    {
                        RecruiterOpeningOverviewModel.BdmProfilePhoto = getEmployeePhotoUrl(RecruiterOpeningOverviewModel.BdmProfilePhoto, RecruiterOpeningOverviewModel.BdmId ?? 0);
                        RecruiterOpeningOverviewModel.RecruiterProfilePhoto = getEmployeePhotoUrl(RecruiterOpeningOverviewModel.RecruiterProfilePhoto, RecruiterOpeningOverviewModel.RecId ?? 0);
                    }
                    int ttlCount = records.Count;

                    if (ttlCount > 0)
                    {
                        response.PerformanceOnStatusWise = new List<PerformanceOnFieldWise>();
                        var uniqeJobs = new List<RecruiterOpeningOverviewModel>();

                        foreach (var item in response.RecruiterOpeningOverviewModel)
                        {
                            var dtls = uniqeJobs.Where(x => x.JobId == item.JobId).FirstOrDefault();
                            if (dtls == null)
                            {
                                var jobObj = new RecruiterOpeningOverviewModel()
                                {
                                    JobId = item.JobId,
                                    ClientID = item.ClientID,
                                    ClientName = item.ClientName,
                                    CreatedDate = item.CreatedDate,
                                    BdmId = item.BdmId,
                                    BdmName = item.BdmName,
                                    BdmProfilePhoto = item.BdmProfilePhoto,
                                    RecId = item.RecId,
                                    RecruiterName = item.RecruiterName,
                                    RecruiterProfilePhoto = item.RecruiterProfilePhoto,
                                    JobTitle = item.JobTitle,
                                    JobStatus = item.JobStatus,
                                    AgeBetweenDates = item.AgeBetweenDates,
                                };
                                uniqeJobs.Add(jobObj);
                            }
                        }

                        response.JobsCount = uniqeJobs.Count;

                        // Account
                        var accountDtls = from c in uniqeJobs
                                          group c by new
                                          {
                                              c.ClientID
                                          } into grp
                                          select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in accountDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.Account,
                                Title = item.data.ClientName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate,
                                BdmId = item.data.BdmId,
                                BdmName = item.data.BdmName,
                                BdmProfilePhoto = item.data.BdmProfilePhoto,
                                RecId = item.data.RecId,
                                RecruiterName = item.data.RecruiterName,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                JobStatus = item.data.JobStatus
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }
                        response.AccountsCount = accountDtls.Count();

                        // BDM
                        var broughtDtls = from r in uniqeJobs
                                          orderby r.CreatedDate
                                          group r by new
                                          {
                                              r.BdmId
                                          } into grp
                                          select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in broughtDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.BDM,
                                Title = item.data.BdmName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate,
                                BdmId = item.data.BdmId,
                                BdmName = item.data.BdmName,
                                BdmProfilePhoto = item.data.BdmProfilePhoto,
                                RecId = item.data.RecId,
                                RecruiterName = item.data.RecruiterName,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                JobStatus = item.data.JobStatus
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }

                        // Recruiter 
                        var recruiterDtls = from r in records
                                            orderby r.CreatedDate
                                            group r by r.RecId into grp
                                            select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in recruiterDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.Recruiter,
                                Title = item.data.RecruiterName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate,
                                BdmId = item.data.BdmId,
                                BdmName = item.data.BdmName,
                                BdmProfilePhoto = item.data.BdmProfilePhoto,
                                RecId = item.data.RecId,
                                RecruiterName = item.data.RecruiterName,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                JobStatus = item.data.JobStatus
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }
                    }

                    respModel.Status = true;
                    respModel.SetResult(response);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<BDMsVM>> GetBDMsOverview(ReportRequestViewModel reportRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<BDMsVM>();
            var bDMsVM = new BDMsVM();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID);

                if (reportRequestViewModel == null)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidJsonFormat, "Invalid Request Json format", true);
                }
                else
                {
                    if (reportRequestViewModel.FromDate == null)
                    {
                        var dt = calcDate(reportRequestViewModel.DateFilter);
                        reportRequestViewModel.FromDate = dt.fmDt;
                        reportRequestViewModel.ToDate = dt.toDt;
                    }
                    else
                    {
                        if (reportRequestViewModel.ToDate == null)
                        {
                            reportRequestViewModel.ToDate = CurrentTime;
                        }
                    }

                    var response = await dbContext.GetBDMsOverview(reportRequestViewModel.FromDate.Value, reportRequestViewModel.ToDate.Value, reportRequestViewModel.PuId, Usr.UserTypeId, Usr.Id);
                    foreach (var item in response)
                    {
                        item.ProfilePhoto = getEmployeePhotoUrl(item.ProfilePhoto, item.UserId);
                    }
                    bDMsVM.BDMsOverviewModel = response;
                    bDMsVM.FromDate = reportRequestViewModel.FromDate.Value;
                    bDMsVM.ToDate = reportRequestViewModel.ToDate.Value;
                    respModel.Status = true;
                    respModel.SetResult(bDMsVM);
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<BDMReportModel>> GetBDMOverview(ReportRequestViewModel reportRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<BDMReportModel>();
            var response = new BDMReportModel
            {
                BDMOverviewModel = new List<BDMOverviewModel>(),
                JobsCount = 0,
                AccountsCount = 0
            };
            try
            {
                //   logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID);

                if (reportRequestViewModel == null)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidJsonFormat, "Invalid Request Json format", true);
                }
                else
                {
                    if (reportRequestViewModel.FromDate == null)
                    {
                        var dt = calcDate(reportRequestViewModel.DateFilter);
                        reportRequestViewModel.FromDate = dt.fmDt;
                        reportRequestViewModel.ToDate = dt.toDt;
                    }
                    else
                    {
                        if (reportRequestViewModel.ToDate == null)
                        {
                            reportRequestViewModel.ToDate = CurrentTime;
                        }
                    }
                    response.FromDate = reportRequestViewModel.FromDate.Value;
                    response.ToDate = reportRequestViewModel.ToDate.Value;

                    List<BDMOverviewModel> records = await dbContext.GetBDMOverview(reportRequestViewModel.FromDate.Value, reportRequestViewModel.ToDate.Value, reportRequestViewModel.PuId,
                        reportRequestViewModel.StatusCode, Usr.UserTypeId, UserId, reportRequestViewModel.UserId);
                    response.BDMOverviewModel = records;
                    int ttlCount = records.Count;
                    foreach (var BDMOpeningOverviewModel in response.BDMOverviewModel)
                    {
                        BDMOpeningOverviewModel.BroughtbyProfilePhoto = getEmployeePhotoUrl(BDMOpeningOverviewModel.BroughtbyProfilePhoto, BDMOpeningOverviewModel.BroughtBy ?? 0);
                        BDMOpeningOverviewModel.RecruiterProfilePhoto = getEmployeePhotoUrl(BDMOpeningOverviewModel.RecruiterProfilePhoto, BDMOpeningOverviewModel.RecruiterId ?? 0);
                    }
                    if (ttlCount > 0)
                    {
                        response.PerformanceOnStatusWise = new List<PerformanceOnFieldWise>();

                        var uniqueJobCount = new List<int>();
                        foreach (var item in response.BDMOverviewModel)
                        {
                            if (!uniqueJobCount.Contains(item.JobId))
                            {
                                uniqueJobCount.Add(item.JobId);
                            }
                        }
                        response.JobsCount = uniqueJobCount.Count;


                        // account
                        var accountDtls = from r in response.BDMOverviewModel
                                          orderby r.ActivityDate
                                          group r by r.ClientID into grp
                                          select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in accountDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.Account,
                                RecId = item.data.RecruiterId,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                RecruiterName = item.data.RecruiterName,
                                BdmProfilePhoto = item.data.BroughtbyProfilePhoto,
                                BdmName = item.data.BroughtByName,
                                BdmId = item.data.BroughtBy,
                                Title = item.data.ClientName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }
                        response.AccountsCount = accountDtls.Count();

                        // job title
                        var jobDtls = from r in response.BDMOverviewModel
                                      orderby r.ActivityDate
                                      group r by r.JobId into grp
                                      select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in jobDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.Opening,
                                RecId = item.data.RecruiterId,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                RecruiterName = item.data.RecruiterName,
                                BdmProfilePhoto = item.data.BroughtbyProfilePhoto,
                                BdmName = item.data.BroughtByName,
                                BdmId = item.data.BroughtBy,
                                Title = item.data.JobTitle,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }

                        // bdm
                        var broughtDtls = from r in response.BDMOverviewModel
                                          orderby r.ActivityDate
                                          group r by r.BroughtBy into grp
                                          select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in broughtDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.BDM,
                                RecId = item.data.RecruiterId,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                RecruiterName = item.data.RecruiterName,
                                BdmProfilePhoto = item.data.BroughtbyProfilePhoto,
                                BdmName = item.data.BroughtByName,
                                BdmId = item.data.BroughtBy,
                                Title = item.data.BroughtByName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }

                        // recuiter 
                        var recruiterDtls = from r in response.BDMOverviewModel
                                            orderby r.ActivityDate
                                            group r by r.RecruiterId into grp
                                            select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in recruiterDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.Recruiter,
                                RecId = item.data.RecruiterId,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                RecruiterName = item.data.RecruiterName,
                                BdmProfilePhoto = item.data.BroughtbyProfilePhoto,
                                BdmName = item.data.BroughtByName,
                                BdmId = item.data.BroughtBy,
                                Title = item.data.RecruiterName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }

                        // current status
                        var currentStatusDtls = from r in response.BDMOverviewModel
                                                orderby r.ActivityDate
                                                group r by r.CurrentStatusId into grp
                                                select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in currentStatusDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.CurrentStatus,
                                RecId = item.data.RecruiterId,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                RecruiterName = item.data.RecruiterName,
                                BdmProfilePhoto = item.data.BroughtbyProfilePhoto,
                                BdmName = item.data.BroughtByName,
                                BdmId = item.data.BroughtBy,
                                Title = item.data.CurrentStatusName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }

                        // updated status 
                        var updateStatusDtls = from r in response.BDMOverviewModel
                                               orderby r.ActivityDate
                                               group r by r.UpdateStatusId into grp
                                               select new { data = grp.First(), cnt = grp.Count() };

                        foreach (var item in updateStatusDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.UpdateStatus,
                                RecId = item.data.RecruiterId,
                                RecruiterProfilePhoto = item.data.RecruiterProfilePhoto,
                                RecruiterName = item.data.RecruiterName,
                                BdmProfilePhoto = item.data.BroughtbyProfilePhoto,
                                BdmName = item.data.BroughtByName,
                                BdmId = item.data.BroughtBy,
                                Title = item.data.UpdateStatuName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }
                    }

                    respModel.Status = true;
                    respModel.SetResult(response);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<BDMOpeingReportModel>> GetBDMOpeningOverview(ReportRequestViewModel reportRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<BDMOpeingReportModel>();
            var response = new BDMOpeingReportModel
            {
                BDMOpeningOverviewModel = new List<BDMOpeningOverviewModel>(),
                JobsCount = 0
            };
            try
            {
                //   logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID);

                if (reportRequestViewModel == null)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidJsonFormat, "Invalid Request Json format", true);
                }
                else
                {
                    if (reportRequestViewModel.FromDate == null)
                    {
                        var dt = calcDate(reportRequestViewModel.DateFilter);
                        reportRequestViewModel.FromDate = dt.fmDt;
                        reportRequestViewModel.ToDate = dt.toDt;
                    }
                    else
                    {
                        if (reportRequestViewModel.ToDate == null)
                        {
                            reportRequestViewModel.ToDate = CurrentTime;
                        }
                    }
                    response.FromDate = reportRequestViewModel.FromDate.Value;
                    response.ToDate = reportRequestViewModel.ToDate.Value;

                    List<BDMOpeningOverviewModel> records = await dbContext.GetBDMOpeningOverview(reportRequestViewModel.FromDate.Value, reportRequestViewModel.ToDate.Value, reportRequestViewModel.PuId,
                        Usr.UserTypeId, UserId, reportRequestViewModel.UserId);
                    response.BDMOpeningOverviewModel = records;
                    foreach (var BDMOpeningOverviewModel in response.BDMOpeningOverviewModel)
                    {
                        BDMOpeningOverviewModel.BdmProfilePhoto = getEmployeePhotoUrl(BDMOpeningOverviewModel.BdmProfilePhoto, BDMOpeningOverviewModel.BdmId ?? 0);
                    }
                    int ttlCount = records.Count;
                    if (ttlCount > 0)
                    {
                        response.PerformanceOnStatusWise = new List<PerformanceOnFieldWise>();

                        var uniqueJobCount = new List<int>();
                        foreach (var item in response.BDMOpeningOverviewModel)
                        {
                            if (!uniqueJobCount.Contains(item.JobId))
                            {
                                uniqueJobCount.Add(item.JobId);
                            }
                        }
                        response.JobsCount = uniqueJobCount.Count;

                        // account
                        var accountDtls = from r in response.BDMOpeningOverviewModel
                                          orderby r.CreatedDate
                                          group r by r.ClientID into grp
                                          select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in accountDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.Account,
                                Title = item.data.ClientName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate,
                                BdmId = item.data.BdmId,
                                BdmName = item.data.BdmName,
                                BdmProfilePhoto = item.data.BdmProfilePhoto,
                                JobStatus = item.data.JobStatus,
                                CreatedDate = item.data.CreatedDate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }
                        response.AccountsCount = accountDtls.Count();

                        // bdm
                        var bdmDtls = from r in response.BDMOpeningOverviewModel
                                      orderby r.CreatedDate
                                      group r by r.BdmId into grp
                                      select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in bdmDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.BDM,
                                Title = item.data.BdmName,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate,
                                BdmId = item.data.BdmId,
                                BdmName = item.data.BdmName,
                                BdmProfilePhoto = item.data.BdmProfilePhoto,
                                JobStatus = item.data.JobStatus,
                                CreatedDate = item.data.CreatedDate
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }

                        // date
                        var dateDtls = from r in response.BDMOpeningOverviewModel
                                       orderby r.CreatedDate
                                       group r by r.CreatedDate into grp
                                       select new { data = grp.First(), cnt = grp.Count() };
                        foreach (var item in dateDtls)
                        {
                            double count = (double)item.cnt / (double)ttlCount;
                            decimal ResponseRate = Convert.ToDecimal(count * 100);
                            if (ResponseRate > 0)
                            {
                                ResponseRate = Math.Round(ResponseRate, 0, MidpointRounding.AwayFromZero);
                            }
                            var performanceOnFieldWise = new PerformanceOnFieldWise
                            {
                                Count = item.cnt,
                                TabDispay = (byte)TabNamesPerColumnWisePerformanceeReport.Date,
                                Title = item.data.BdmName,
                                CreatedDate = item.data.CreatedDate,
                                TotalCount = ttlCount,
                                Percentage = ResponseRate,
                                BdmId = item.data.BdmId,
                                BdmName = item.data.BdmName,
                                BdmProfilePhoto = item.data.BdmProfilePhoto,
                                JobStatus = item.data.JobStatus
                            };
                            response.PerformanceOnStatusWise.Add(performanceOnFieldWise);
                        }

                    }

                    respModel.Status = true;
                    respModel.SetResult(response);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<CandidatesSourceVM>> GetCandidatesSourceOverview(ReportRequestViewModel reportRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<CandidatesSourceVM>();
            var candidatesSourceVM = new CandidatesSourceVM();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID);
                if (reportRequestViewModel == null)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidJsonFormat, "Invalid Request Json format", true);
                }
                else
                {
                    if (reportRequestViewModel.FromDate == null)
                    {
                        var dt = calcDate(reportRequestViewModel.DateFilter);
                        reportRequestViewModel.FromDate = dt.fmDt;
                        reportRequestViewModel.ToDate = dt.toDt;
                    }
                    else
                    {
                        if (reportRequestViewModel.ToDate == null)
                        {
                            reportRequestViewModel.ToDate = CurrentTime;
                        }
                    }

                    var response = await dbContext.GetCandidatesSourceOverview(reportRequestViewModel.FromDate.Value, reportRequestViewModel.ToDate.Value, reportRequestViewModel.PuId, Usr.UserTypeId, Usr.Id);
                    candidatesSourceVM.CandidatesSourceOverviewModel = response;
                    foreach (var item in candidatesSourceVM.CandidatesSourceOverviewModel)
                    {
                        item.ProfilePhoto = getEmployeePhotoUrl(item.ProfilePhoto, item.UserId);
                    }
                    candidatesSourceVM.FromDate = reportRequestViewModel.FromDate.Value;
                    candidatesSourceVM.ToDate = reportRequestViewModel.ToDate.Value;
                    respModel.Status = true;
                    respModel.SetResult(candidatesSourceVM);
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<SourcedCandidatesVM>> GetSourcedCandidates(ReportRequestViewModel reportRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<SourcedCandidatesVM>();
            var sourcedCandidatesVM = new SourcedCandidatesVM();
            try
            {
                //    logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID);
                if (reportRequestViewModel == null)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidJsonFormat, "Invalid Request Json format", true);
                }
                else
                {
                    if (reportRequestViewModel.FromDate == null)
                    {
                        var dt = calcDate(reportRequestViewModel.DateFilter);
                        reportRequestViewModel.FromDate = dt.fmDt;
                        reportRequestViewModel.ToDate = dt.toDt;
                    }
                    else
                    {
                        if (reportRequestViewModel.ToDate == null)
                        {
                            reportRequestViewModel.ToDate = CurrentTime;
                        }
                    }
                    var response = await dbContext.GetSourcedCandidates(reportRequestViewModel.FromDate.Value, reportRequestViewModel.ToDate.Value, reportRequestViewModel.PuId, Usr.UserTypeId, Usr.Id, reportRequestViewModel.UserId, reportRequestViewModel.SourcedFrom);
                    foreach (var item in response)
                    {
                        item.RecruiterProfilePhoto = getEmployeePhotoUrl(item.RecruiterProfilePhoto, item.RecruiterId ?? 0);
                        item.BdmProfilePhoto = getEmployeePhotoUrl(item.BdmProfilePhoto, item.BdmId ?? 0);
                    }
                    sourcedCandidatesVM.SourcedCandidatesModel = response;
                    sourcedCandidatesVM.FromDate = reportRequestViewModel.FromDate.Value;
                    sourcedCandidatesVM.ToDate = reportRequestViewModel.ToDate.Value;
                    respModel.Status = true;
                    respModel.SetResult(sourcedCandidatesVM);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        public async Task<GetResponseViewModel<WebSourceRecruitersVM>> GetSourcedWebsiteCandidates(ReportRequestViewModel reportRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<WebSourceRecruitersVM>();
            var webSourceRecruitersVM = new WebSourceRecruitersVM();
            try
            {
                //  logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "Start of method: filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID);
                if (reportRequestViewModel == null)
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.InvalidJsonFormat, "Invalid Request Json format", true);
                }
                else
                {
                    if (reportRequestViewModel.FromDate == null)
                    {
                        var dt = calcDate(reportRequestViewModel.DateFilter);
                        reportRequestViewModel.FromDate = dt.fmDt;
                        reportRequestViewModel.ToDate = dt.toDt;
                    }
                    else
                    {
                        if (reportRequestViewModel.ToDate == null)
                        {
                            reportRequestViewModel.ToDate = CurrentTime;
                        }
                    }

                    var response = await dbContext.GetSourcedWebsiteCandidates(reportRequestViewModel.FromDate.Value, reportRequestViewModel.ToDate.Value, reportRequestViewModel.UserId, reportRequestViewModel.PuId);
                    foreach (var item in response)
                    {
                        item.ProfilePhoto = getEmployeePhotoUrl(item.ProfilePhoto, item.UserId);
                    }

                    webSourceRecruitersVM.RecruitersOverviewModel = response;
                    webSourceRecruitersVM.FromDate = reportRequestViewModel.FromDate.Value;
                    webSourceRecruitersVM.ToDate = reportRequestViewModel.ToDate.Value;
                    respModel.Status = true;
                    respModel.SetResult(webSourceRecruitersVM);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", filterViewModel->" + JsonConvert.SerializeObject(reportRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        #endregion

    }
}


