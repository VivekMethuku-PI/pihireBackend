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
using PiHire.BAL.Common.Types;

namespace PiHire.BAL.Repositories
{
    public class AutomationRepository : BaseRepository, IAutomationRepository
    {
        readonly Logger logger;
        public AutomationRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<AutomationRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }


        #region -Candidate 
        public async Task<GetResponseViewModel<List<CandidateStatusListViewModel>>> GetCandidateStatus()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<CandidateStatusListViewModel>>();
            try
            {
                
                var Status = new List<CandidateStatusListViewModel>();

                Status = await (from stus in dbContext.PhCandStatusSes
                                select new CandidateStatusListViewModel
                                {
                                    Id = stus.Id,
                                    Status = stus.Status,
                                    StatusDesc = stus.StatusDesc,
                                    Title = stus.Title
                                }).OrderBy(x => x.Title).ToListAsync();

                respModel.SetResult(Status);
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

        public async Task<CreateResponseViewModel<string>> CreateCandidateStatus(CreateCandidateStatusViewModel createCandidateStatusViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            int UserId = Usr.Id;
            try
            {
                

                var CandStatus = dbContext.PhCandStatusSes.Where(x => x.Title.ToLower() == createCandidateStatusViewModel.Title.Trim().ToLower() && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (CandStatus == null)
                {
                    CandStatus = new PhCandStatusS()
                    {
                        CreatedBy = UserId,
                        CreatedDate = CurrentTime,
                        Status = (byte)RecordStatus.Active,
                        Title = createCandidateStatusViewModel.Title.Trim(),
                        //StatusDesc = createCandidateStatusViewModel.StatusDesc?.Trim(),
                        StatusDesc = createCandidateStatusViewModel.Title.Trim()
                    };
                    dbContext.PhCandStatusSes.Add(CandStatus);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Created New Candidate Status",
                        ActivityDesc = " New Candidate Status -" + createCandidateStatusViewModel.Title + " is Created successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = CandStatus.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);


                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Name is already available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
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

        public async Task<UpdateResponseViewModel<string>> EditCandidateStatus(EditCandidateStatusViewModel editCandidateStatusViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);
                var CandStatus = dbContext.PhCandStatusSes.Where(x => x.Id != editCandidateStatusViewModel.Id && x.Title.ToLower() == editCandidateStatusViewModel.Title.Trim().ToLower() && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (CandStatus == null)
                {
                    CandStatus = dbContext.PhCandStatusSes.Where(x => x.Id == editCandidateStatusViewModel.Id).FirstOrDefault();
                    if (CandStatus != null)
                    {
                        CandStatus.UpdatedBy = UserId;
                        CandStatus.UpdatedDate = CurrentTime;
                        CandStatus.Title = editCandidateStatusViewModel.Title.Trim();                      
                        CandStatus.StatusDesc = editCandidateStatusViewModel.StatusDesc.Trim();

                        dbContext.PhCandStatusSes.Update(CandStatus);
                        await dbContext.SaveChangesAsync();

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Updated Candidate Status",
                            ActivityDesc = " Candidate status details is Updated successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = CandStatus.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }
                    else
                    {
                        respModel.Status = false;
                        message = "The Candidate Status Id is not available";
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                }
                else
                {
                    message = "Name is already available";
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

        public async Task<UpdateResponseViewModel<string>> UpdateCandidateStatus(UpdateCandidateStatusViewModel updateCandidateStatusViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = string.Empty;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var CandStatus = dbContext.PhCandStatusSes.Where(x => x.Id == updateCandidateStatusViewModel.ID).FirstOrDefault();
                if (CandStatus != null)
                {
                    CandStatus.UpdatedBy = UserId;
                    CandStatus.UpdatedDate = CurrentTime;
                    CandStatus.Status = updateCandidateStatusViewModel.Status;

                    dbContext.PhCandStatusSes.Update(CandStatus);
                    await dbContext.SaveChangesAsync();
                    if (updateCandidateStatusViewModel.Status == 0)
                    {
                        message = "Disabled Successfully";
                    }
                    else
                    {
                        message = "Enabled Successfully";
                    }

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Update Candidate Status",
                        ActivityDesc = "Candidate Status is " + message + "",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = CandStatus.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    respModel.Status = false;
                    message = "The Candidate Status Id is not available";
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

        #region - Opening 
        public async Task<GetResponseViewModel<List<OpeningStatusListViewModel>>> GetOpeningStatus()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<OpeningStatusListViewModel>>();
            try
            {
                
                var Status = new List<OpeningStatusListViewModel>();

                Status = await (from stus in dbContext.PhJobStatusSes
                                select new OpeningStatusListViewModel
                                {
                                    CreatedBy = stus.CreatedBy,
                                    CreatedDate = stus.CreatedDate,
                                    Id = stus.Id,
                                    Status = stus.Status,
                                    StatusDesc = stus.StatusDesc,
                                    Title = stus.Title
                                }).OrderBy(x=>x.Title).ToListAsync();

                respModel.SetResult(Status);
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

        public async Task<CreateResponseViewModel<string>> CreateOpeningStatus(CreateOpeningStatusViewModel createOpeningStatusViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            int UserId = Usr.Id;
            try
            {
                
                var JobStatus = dbContext.PhJobStatusSes.Where(x => x.Title.ToLower() == createOpeningStatusViewModel.Title.Trim().ToLower() && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (JobStatus == null)
                {
                    JobStatus = new PhJobStatusS()
                    {
                        CreatedBy = UserId,
                        CreatedDate = CurrentTime,
                        Title = createOpeningStatusViewModel.Title.Trim(),
                        Status = (byte)RecordStatus.Active,
                        StatusDesc = createOpeningStatusViewModel.StatusDesc.Trim(),
                        //StatusDesc = createOpeningStatusViewModel.StatusDesc?.Trim()
                    };
                    dbContext.PhJobStatusSes.Add(JobStatus);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Created New Job Status",
                        ActivityDesc = " Job Status is Created successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = JobStatus.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Name is already available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
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

        public async Task<UpdateResponseViewModel<string>> EditOpeningStatus(EditOpeningStatusViewModel editOpeningStatusViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var JobStatus = dbContext.PhJobStatusSes.Where(x => x.Id != editOpeningStatusViewModel.ID && x.Title.ToLower() == editOpeningStatusViewModel.Title.Trim().ToLower() && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (JobStatus == null)
                {
                    JobStatus = dbContext.PhJobStatusSes.Where(x => x.Id == editOpeningStatusViewModel.ID).FirstOrDefault();
                    if (JobStatus != null)
                    {
                        JobStatus.UpdatedBy = UserId;
                        JobStatus.UpdatedDate = CurrentTime;
                        JobStatus.Title = editOpeningStatusViewModel.Title.Trim();
                        //JobStatus.StatusDesc = editOpeningStatusViewModel.StatusDesc?.Trim();
                        JobStatus.StatusDesc = editOpeningStatusViewModel.StatusDesc.Trim();

                        dbContext.PhJobStatusSes.Update(JobStatus);
                        await dbContext.SaveChangesAsync();

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Updated Job Status",
                            ActivityDesc = "Job Status is Updated successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = JobStatus.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }
                    else
                    {
                        respModel.Status = false;
                        message = "The Opening Name Id is not available";
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                }
                else
                {
                    message = "Name is already available";
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

        public async Task<UpdateResponseViewModel<string>> UpdateOpeningStatus(UpdateOpeningStatusViewModel updateOpeningStatusViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod()); 
            
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = string.Empty;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var JobStatus = dbContext.PhJobStatusSes.Where(x => x.Id == updateOpeningStatusViewModel.ID).FirstOrDefault();
                if (JobStatus != null)
                {
                    JobStatus.UpdatedBy = UserId;
                    JobStatus.UpdatedDate = CurrentTime;
                    JobStatus.Status = updateOpeningStatusViewModel.Status;

                    dbContext.PhJobStatusSes.Update(JobStatus);
                    await dbContext.SaveChangesAsync();
                    if (updateOpeningStatusViewModel.Status == 0)
                    {
                        message = "Disabled Successfully";
                    }
                    else
                    {
                        message = "Enabled Successfully";
                    }

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Update Job Status",
                        ActivityDesc = " Job Status is Updated successfully",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = JobStatus.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    respModel.Status = false;
                    message = "The Opening Name Id is not available";
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

        #region - Pipeline 

        public async Task<CreateResponseViewModel<string>> CreateStage(CreateStageViewModel createStageViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            int UserId = Usr.Id;
            try
            {
                

                var CandStage = dbContext.PhCandStagesSes.Where(x => x.Title.ToLower() == createStageViewModel.Title.Trim().ToLower() && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (CandStage == null)
                {
                    var StageColor = dbContext.PhCandStagesSes.Where(x => x.ColorCode.ToLower() == createStageViewModel.ColorCode.Trim().ToLower()
                    && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                    if (StageColor == null)
                    {
                        CandStage = new PhCandStagesS()
                        {
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            Title = createStageViewModel.Title.Trim(),
                            Status = (byte)RecordStatus.Active,
                            StageDesc = createStageViewModel.StageDesc?.Trim(),
                            ColorCode = createStageViewModel.ColorCode
                        };
                        dbContext.PhCandStagesSes.Add(CandStage);
                        await dbContext.SaveChangesAsync();

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Created New Stage",
                            ActivityDesc = " Stage is Created successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = CandStage.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }
                    else
                    {
                        message = "Color is already available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                    }
                }
                else
                {
                    message = "Name is already available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
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

        public async Task<UpdateResponseViewModel<string>> EditStage(EditStageViewModel editStageViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod()); 
            
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var CandStage = dbContext.PhCandStagesSes.Where(x => x.Id != editStageViewModel.Id && x.Title.ToLower() == editStageViewModel.Title.Trim().ToLower() &&
                x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (CandStage == null)
                {
                    var StageColor = dbContext.PhCandStagesSes.Where(x => x.Id != editStageViewModel.Id && x.ColorCode.ToLower() == editStageViewModel.ColorCode.Trim().ToLower() &&
            x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                    if (StageColor == null)
                    {
                        CandStage = dbContext.PhCandStagesSes.Where(x => x.Id == editStageViewModel.Id).FirstOrDefault();
                        if (CandStage != null)
                        {
                            CandStage.UpdatedDate = CurrentTime;
                            CandStage.UpdatedBy = UserId;
                            CandStage.Title = editStageViewModel.Title.Trim();
                            CandStage.ColorCode = editStageViewModel.ColorCode;
                            CandStage.StageDesc = editStageViewModel.StageDesc?.Trim();

                            dbContext.PhCandStagesSes.Update(CandStage);
                            await dbContext.SaveChangesAsync();

                            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                            var auditLog = new CreateAuditViewModel
                            {
                                ActivitySubject = "Updated Stage",
                                ActivityDesc = "Stage is Updated successfully",
                                ActivityType = (byte)AuditActivityType.RecordUpdates,
                                TaskID = CandStage.Id,
                                UserId = UserId
                            };
                            audList.Add(auditLog);
                            SaveAuditLog(audList);

                            respModel.Status = true;
                            respModel.SetResult(message);
                        }
                        else
                        {
                            respModel.Status = false;
                            message = "The Stage is not available";
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        }
                    }
                    else
                    {
                        message = "Color is already available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                    }
                }
                else
                {
                    message = "Name is already available";
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

        public async Task<DeleteResponseViewModel<string>> DeleteStage(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod()); 
            
            int UserId = Usr.Id;
            var respModel = new DeleteResponseViewModel<string>();
            string message = "Deleted Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.DeleteItem, "Start of method:", respModel.Meta.RequestID);

                var CandStage = await dbContext.PhCandStagesSes.Where(x => x.Id == Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (CandStage != null)
                {
                    CandStage.UpdatedBy = UserId;
                    CandStage.UpdatedDate = CurrentTime;
                    CandStage.Status = (byte)RecordStatus.Delete;

                    dbContext.PhCandStagesSes.Update(CandStage);
                    await dbContext.SaveChangesAsync();

                    var CandStageMap = await dbContext.PhCandStageMaps.Where(x => x.StageId == Id && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                    foreach (var item in CandStageMap)
                    {
                        item.UpdatedBy = UserId;
                        item.UpdatedDate = CurrentTime;
                        item.Status = (byte)RecordStatus.Delete;

                        dbContext.PhCandStageMaps.Update(item);
                        await dbContext.SaveChangesAsync();
                    }

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Deleted Stage",
                        ActivityDesc = "Stage is deleted successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = CandStage.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Stage is not found";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.DeleteItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<List<StageViewModel>>> Stages()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod()); 
            
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<StageViewModel>>();
            try
            {
                
                var Status = new List<StageViewModel>();

                Status = await (from stus in dbContext.PhCandStagesSes
                                where stus.Status != (byte)RecordStatus.Delete
                                select new StageViewModel
                                {
                                    ColorCode = stus.ColorCode,
                                    Id = stus.Id,
                                    StageDesc = stus.StageDesc,
                                    Status = stus.Status,
                                    Title = stus.Title
                                }).OrderBy(x => x.Title).ToListAsync();

                respModel.Status = true;
                respModel.SetResult(Status);
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

        public async Task<GetResponseViewModel<List<StageStatusListViewModel>>> StageCandidateStatus()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod()); 
            
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<StageStatusListViewModel>>();
            try
            {
                
                var Status = new List<StageStatusListViewModel>();

                Status = await (from stus in dbContext.PhCandStagesSes
                                where stus.Status != (byte)RecordStatus.Delete
                                select new StageStatusListViewModel
                                {
                                    Id = stus.Id,
                                    Title = stus.Title,
                                    StageDesc = stus.StageDesc,
                                    ColorCode = stus.ColorCode,
                                    Status = stus.Status,
                                    StageCandidateStatusListViewModel = (from MapStus in dbContext.PhCandStageMaps
                                                                         join CanStus in dbContext.PhCandStatusSes on MapStus.CandStatusId equals CanStus.Id
                                                                         where MapStus.StageId == stus.Id && MapStus.Status != (byte)RecordStatus.Delete
                                                                         select new StageCandidateStatusListViewModel
                                                                         {
                                                                             CandStageMapId = MapStus.Id,
                                                                             CandidateStatusId = CanStus.Id,
                                                                             CandidateStatusIdIsEnable = CanStus.Status,
                                                                             Status = MapStus.Status,
                                                                             Title = CanStus.Title,
                                                                             StatusDesc = CanStus.StatusDesc
                                                                         }).ToList()
                                }).ToListAsync();

                respModel.SetResult(Status);
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

        public async Task<CreateResponseViewModel<string>> MapCandidateStatus(MapCandidateStatusViewModel mapCandidateStatusViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            int UserId = Usr.Id;
            try
            {
                

                var CandStages = await dbContext.PhCandStagesSes.Where(x => x.Id == mapCandidateStatusViewModel.StageId
                && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (CandStages != null)
                {
                    var CandStatus = await dbContext.PhCandStatusSes.Where(x => mapCandidateStatusViewModel.CandStatusId.Contains(x.Id)
               && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                    if (CandStatus.Count == mapCandidateStatusViewModel.CandStatusId.Count)
                    {
                        var CandStageMap = await dbContext.PhCandStageMaps.Where(x => x.StageId == mapCandidateStatusViewModel.StageId &&
                mapCandidateStatusViewModel.CandStatusId.Contains(x.CandStatusId) && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                        if (CandStageMap == null)
                        {
                            foreach (var item in mapCandidateStatusViewModel.CandStatusId)
                            {
                                CandStageMap = new PhCandStageMap()
                                {
                                    CandStatusId = item,
                                    CreatedBy = UserId,
                                    CreatedDate = CurrentTime,
                                    Status = (byte)RecordStatus.Active,
                                    StageId = mapCandidateStatusViewModel.StageId
                                };
                                dbContext.PhCandStageMaps.Add(CandStageMap);
                                await dbContext.SaveChangesAsync();
                            }

                            respModel.Status = true;
                            respModel.SetResult(message);
                        }
                        else
                        {
                            respModel.Status = false;
                            message = "The selected status is already mapped to this stage";
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                        }
                    }
                    else
                    {
                        respModel.Status = false;
                        message = "The Candidate Status is not available";
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                }
                else
                {
                    respModel.Status = false;
                    message = "The Stage is not available";
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

        public async Task<UpdateResponseViewModel<string>> DeleteStageCandidateStatus(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Deleted Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.DeleteItem, "Start of method:", respModel.Meta.RequestID);

                var CandStageMap = dbContext.PhCandStageMaps.Where(x => x.Id == Id).FirstOrDefault();
                if (CandStageMap != null)
                {
                    CandStageMap.UpdatedDate = CurrentTime;
                    CandStageMap.UpdatedBy = UserId;
                    CandStageMap.Status = (byte)RecordStatus.Delete;

                    dbContext.PhCandStageMaps.Update(CandStageMap);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Map Candidate Status",
                        ActivityDesc = " Mapped Candidate Status to stage",
                        ActivityType = (byte)AuditActivityType.Critical,
                        TaskID = CandStageMap.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Candidate Status is not found";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.DeleteItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        #endregion

        #region - Display Rule

        public async Task<GetResponseViewModel<List<DisplayRuleViewmodel>>> DisplayRules()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<DisplayRuleViewmodel>>();
            try
            {
                
                var Status = new List<DisplayRuleViewmodel>();

                Status = await (from stus in dbContext.PhCandStatusConfigs
                                join canStus in dbContext.PhCandStatusSes on stus.StatusId equals canStus.Id
                                where stus.Status != (byte)RecordStatus.Delete && stus.DispOrder == 1
                                select new DisplayRuleViewmodel
                                {
                                    Id = stus.Id,
                                    Title = canStus.Title,
                                    Status = stus.Status,
                                    StatusId = stus.StatusId,
                                    StatusIdIsEnable = canStus.Status,
                                    DisplayRuleNextOrderViewmodels = (from nextStus in dbContext.PhCandStatusConfigs
                                                                      join canNextStus in dbContext.PhCandStatusSes on nextStus.NextStatusId equals canNextStus.Id
                                                                      where nextStus.Status != (byte)RecordStatus.Delete
                                                                      && nextStus.DispOrder != 1 && nextStus.StatusId == stus.StatusId
                                                                      select new DisplayRuleNextOrderViewmodel
                                                                      {
                                                                          Id = nextStus.Id,
                                                                          Title = canNextStus.Title,
                                                                          Status = nextStus.Status,
                                                                          StatusId = nextStus.StatusId,
                                                                          NextStatusId = nextStus.NextStatusId,
                                                                          NextStatusIdIsEnable = canNextStus.Status,
                                                                          DisplayOrder = nextStus.DispOrder
                                                                      }).OrderBy(x => x.DisplayOrder).ToList()
                                }).ToListAsync();

                respModel.Status = true;
                respModel.SetResult(Status);
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

        public async Task<CreateResponseViewModel<string>> CreateDisplayRule(CreateDisplayRuleViewmodel createDisplayRuleViewmodel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    
                    int auditId = 0;
                    var CandStatus = await dbContext.PhCandStatusSes.Where(x => (x.Id == createDisplayRuleViewmodel.StatusId
                    || createDisplayRuleViewmodel.NextStatusId.Contains(x.Id)) && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                    if (CandStatus.Count > 0)
                    {
                        byte DispOrder = 1;
                        var CandStatusConfig = dbContext.PhCandStatusConfigs.Where(x => x.StatusId == createDisplayRuleViewmodel.StatusId
                        && x.DispOrder == DispOrder && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                        if (CandStatusConfig == null)
                        {
                            if (!createDisplayRuleViewmodel.NextStatusId.Contains(createDisplayRuleViewmodel.StatusId))
                            {
                                CandStatusConfig = new PhCandStatusConfig()
                                {
                                    CreatedBy = UserId,
                                    DispOrder = DispOrder,
                                    CreatedDate = CurrentTime,
                                    Status = (byte)RecordStatus.Active,
                                    StatusId = createDisplayRuleViewmodel.StatusId,
                                    NextStatusId = 0
                                };

                                dbContext.PhCandStatusConfigs.Add(CandStatusConfig);
                                await dbContext.SaveChangesAsync();

                                auditId = CandStatusConfig.Id;

                                for (int i = 0; i < createDisplayRuleViewmodel.NextStatusId.Count; i++)
                                {
                                    CandStatusConfig = new PhCandStatusConfig()
                                    {
                                        CreatedBy = UserId,
                                        DispOrder = (byte)(CandStatusConfig.DispOrder + 1),
                                        CreatedDate = CurrentTime,
                                        Status = (byte)RecordStatus.Active,
                                        StatusId = createDisplayRuleViewmodel.StatusId,
                                        NextStatusId = createDisplayRuleViewmodel.NextStatusId[i]
                                    };
                                    dbContext.PhCandStatusConfigs.Add(CandStatusConfig);
                                    await dbContext.SaveChangesAsync();
                                }

                                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                                var auditLog = new CreateAuditViewModel
                                {
                                    ActivitySubject = " Display Rule",
                                    ActivityDesc = "  has created the Display Rule successfully",
                                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                                    TaskID = auditId,
                                    UserId = UserId
                                };
                                audList.Add(auditLog);
                                SaveAuditLog(audList);

                                respModel.Status = true;
                                respModel.SetResult(message);
                            }
                            else
                            {
                                respModel.Status = false;
                                message = "The Current status available in applicable status";
                                respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                            }
                        }
                        else
                        {
                            respModel.Status = false;
                            message = "The Selected status is already configured to display rules";
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                        }
                    }
                    else
                    {
                        respModel.Status = false;
                        message = "The Status is in Active";
                        respModel.Meta.SetError(ApiResponseErrorCodes.InvalidUrlParameter, message, true);
                    }
                    trans.Commit();

                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return respModel;
        }

        public async Task<UpdateResponseViewModel<string>> EditDisplayRule(EditDisplayRuleViewmodel editDisplayRuleViewmodel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);
                    int auditId = 0;
                    var CandStatus = await dbContext.PhCandStatusSes.Where(x => (x.Id == editDisplayRuleViewmodel.StatusId
                    || editDisplayRuleViewmodel.NextStatusId.Contains(x.Id)) && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                    if (CandStatus.Count > 0)
                    {
                        byte DispOrder = 1;
                        var CandStatusConfig = dbContext.PhCandStatusConfigs.Where(x => x.StatusId == editDisplayRuleViewmodel.StatusId
                        && x.DispOrder == DispOrder && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                        if (CandStatusConfig == null)
                        {
                            if (!editDisplayRuleViewmodel.NextStatusId.Contains(editDisplayRuleViewmodel.StatusId))
                            {
                                CandStatusConfig = new PhCandStatusConfig()
                                {
                                    CreatedBy = UserId,
                                    DispOrder = DispOrder,
                                    CreatedDate = CurrentTime,
                                    Status = (byte)RecordStatus.Active,
                                    StatusId = editDisplayRuleViewmodel.StatusId,
                                    NextStatusId = 0
                                };
                                dbContext.PhCandStatusConfigs.Add(CandStatusConfig);
                                await dbContext.SaveChangesAsync();

                                for (int i = 0; i < editDisplayRuleViewmodel.NextStatusId.Count; i++)
                                {
                                    CandStatusConfig = new PhCandStatusConfig()
                                    {
                                        CreatedBy = UserId,
                                        DispOrder = (byte)(CandStatusConfig.DispOrder + 1),
                                        CreatedDate = CurrentTime,
                                        Status = (byte)RecordStatus.Active,
                                        StatusId = editDisplayRuleViewmodel.StatusId,
                                        NextStatusId = editDisplayRuleViewmodel.NextStatusId[i]
                                    };
                                    dbContext.PhCandStatusConfigs.Add(CandStatusConfig);
                                    await dbContext.SaveChangesAsync();
                                }

                                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                                var auditLog = new CreateAuditViewModel
                                {
                                    ActivitySubject = " Display Rule",
                                    ActivityDesc = " has created the Display Rule successfully",
                                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                                    TaskID = auditId,
                                    UserId = UserId
                                };
                                audList.Add(auditLog);
                                SaveAuditLog(audList);


                                respModel.Status = true;
                                respModel.SetResult(message);
                            }
                            else
                            {
                                respModel.Status = false;
                                message = "The Current status available in applicable status";
                                respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                            }
                        }
                        else
                        {
                            var CandStatusNextConfig = await dbContext.PhCandStatusConfigs.Where(x => x.StatusId == editDisplayRuleViewmodel.StatusId
                        && x.DispOrder != DispOrder && x.Status != (byte)RecordStatus.Delete).ToListAsync();

                            if (CandStatusNextConfig.Count > 0)
                            {
                                dbContext.PhCandStatusConfigs.RemoveRange(CandStatusNextConfig);
                                await dbContext.SaveChangesAsync();
                            }

                            for (int i = 0; i < editDisplayRuleViewmodel.NextStatusId.Count; i++)
                            {
                                CandStatusConfig = new PhCandStatusConfig()
                                {
                                    CreatedBy = UserId,
                                    DispOrder = (byte)(CandStatusConfig.DispOrder + 1),
                                    CreatedDate = CurrentTime,
                                    Status = (byte)RecordStatus.Active,
                                    StatusId = editDisplayRuleViewmodel.StatusId,
                                    NextStatusId = editDisplayRuleViewmodel.NextStatusId[i]
                                };
                                dbContext.PhCandStatusConfigs.Add(CandStatusConfig);
                                await dbContext.SaveChangesAsync();
                            }

                            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                            var auditLog = new CreateAuditViewModel
                            {
                                ActivitySubject = " Display Rule",
                                ActivityDesc = " has created the Display Rule successfully",
                                ActivityType = (byte)AuditActivityType.RecordUpdates,
                                TaskID = 0,
                                UserId = UserId
                            };
                            audList.Add(auditLog);
                            SaveAuditLog(audList);

                            respModel.Status = true;
                            respModel.SetResult(message);
                        }
                    }
                    else
                    {
                        respModel.Status = false;
                        message = "The Status is in Active";
                        respModel.Meta.SetError(ApiResponseErrorCodes.InvalidUrlParameter, message, true);
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return respModel;
        }

        public async Task<UpdateResponseViewModel<string>> UpdateDisplayRule(UpdateDisplayRuleViewmodel updateDisplayRuleViewmodel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod()); 
            
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = string.Empty;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var CandStage = await dbContext.PhCandStatusConfigs.Where(x => x.StatusId == updateDisplayRuleViewmodel.StatusId && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                if (CandStage.Count > 0)
                {
                    foreach (var item in CandStage)
                    {
                        item.UpdatedBy = UserId;
                        item.UpdatedDate = CurrentTime;
                        item.Status = updateDisplayRuleViewmodel.Status;

                        dbContext.PhCandStatusConfigs.Update(item);
                        await dbContext.SaveChangesAsync();
                        if (updateDisplayRuleViewmodel.Status == 0)
                        {
                            message = "Disabled Successfully";
                        }
                        else
                        {
                            message = "Enabled Successfully";
                        }

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = " Display Rule",
                            ActivityDesc = " updated the display rule successfully",
                            ActivityType = (byte)AuditActivityType.StatusUpdates,
                            TaskID = 0,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }
                }
                else
                {
                    message = "Status is not found";
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

        public async Task<DeleteResponseViewModel<string>> DeleteDisplayRule(int StatusId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod()); 
            
            int UserId = Usr.Id;
            var respModel = new DeleteResponseViewModel<string>();
            string message = "Deleted Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.DeleteItem, "Start of method:", respModel.Meta.RequestID);

                var CandStatusConfig = await dbContext.PhCandStatusConfigs.Where(x => x.StatusId == StatusId && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                if (CandStatusConfig.Count > 0)
                {
                    foreach (var item in CandStatusConfig)
                    {
                        item.UpdatedBy = UserId;
                        item.UpdatedDate = CurrentTime;
                        item.Status = (byte)RecordStatus.Delete;
                    }

                    dbContext.PhCandStatusConfigs.UpdateRange(CandStatusConfig);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Deleted Display Rule",
                        ActivityDesc = " has deleted the Display Rule successfully",
                        ActivityType = (byte)AuditActivityType.Critical,
                        TaskID = 0,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult(message);

                }
                else
                {
                    message = "Status is not found";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.DeleteItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }



        public async Task<GetResponseViewModel<List<NextCandidateStatusViewmodel>>> GetNextCandidateStatus(int StatusId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod()); 
            
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<NextCandidateStatusViewmodel>>();
            try
            {
                
                var Status = new List<NextCandidateStatusViewmodel>();

                Status = await (from stus in dbContext.PhCandStatusConfigs
                                where stus.StatusId == StatusId && stus.NextStatusId != 0 && stus.Status != (byte)RecordStatus.Delete
                                select new NextCandidateStatusViewmodel
                                {
                                    StatusId = stus.NextStatusId,
                                    StatusName = dbContext.PhCandStatusSes.Where(x => x.Id == stus.NextStatusId).Select(x => x.Title).FirstOrDefault()
                                }).ToListAsync();

                respModel.Status = true;
                respModel.SetResult(Status);
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
    }
}
