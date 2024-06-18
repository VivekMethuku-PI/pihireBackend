using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PiHire.BAL.Common;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.Common.Types;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.Utilities.Communications.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class WorkSheduleRepository : BaseRepository, IWorkSheduleRepository
    {
        readonly Logger logger;
        
        private readonly IWebHostEnvironment _environment;

        
        public WorkSheduleRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<WorkSheduleRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        #region Leaves

        public async Task<GetResponseViewModel<List<LeavesViewModel>>> GetLeaves()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<LeavesViewModel>>();
            int UserId = Usr.Id;
            try
            {
                
                var Leaves = new List<LeavesViewModel>();

                Leaves = await (from stus in dbContext.PhEmpLeaveRequests
                                select new LeavesViewModel
                                {
                                    ApprovedBy = stus.ApprovedBy,
                                    LeaveType = stus.LeaveType,
                                    EmpId = stus.EmpId,
                                    ApprovedDate = stus.ApprovedDate,
                                    ApproveRemarks = stus.ApproveRemarks,
                                    RejectRemarks = stus.RejectRemarks,
                                    CancelDate = stus.CancelDate,
                                    CancelRemarks = stus.CancelRemarks,
                                    CreatedBy = stus.CreatedBy,
                                    CreatedDate = stus.CreatedDate,
                                    Id = stus.Id,
                                    LeaveEndDate = stus.LeaveEndDate,
                                    LeaveReason = stus.LeaveReason,
                                    LeaveStartDate = stus.LeaveStartDate,
                                    RejectedDate = stus.RejectedDate,
                                    Status = stus.Status,
                                    LeaveTypeName = string.Empty,
                                    EmpName = string.Empty,
                                    StatusName = string.Empty,
                                    ApprovedByName = string.Empty
                                }).ToListAsync();

                if (Usr.UserTypeId == (byte)UserType.Admin)
                {
                    Leaves = Leaves.Where(x => x.ApprovedBy == UserId || x.EmpId == UserId).ToList();
                }
                else if (Usr.UserTypeId == (byte)UserType.BDM || Usr.UserTypeId == (byte)UserType.Recruiter)
                {
                    Leaves = Leaves.Where(x => x.EmpId == UserId).ToList();
                }

                var users = dbContext.PiHireUsers.Where(x => x.UserType != (byte)UserType.Candidate).ToList();
                var refData = dbContext.PhRefMasters.Where(x => x.GroupId == 98).ToList();

                foreach (var item in Leaves)
                {
                    var ApprovedByDtls = users.Where(x => x.Id == item.ApprovedBy).FirstOrDefault();
                    if (ApprovedByDtls != null)
                    {
                        item.ApprovedByName = ApprovedByDtls.FirstName + " " + ApprovedByDtls.LastName;
                        item.ApprovedByRole = ApprovedByDtls.UserRoleName;
                        item.ApprovedByProfilePic = ApprovedByDtls.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + ApprovedByDtls.Id + "/ProfilePhoto/" + ApprovedByDtls.ProfilePhoto : string.Empty;
                    }
                    var EmpDtls = users.Where(x => x.Id == item.EmpId).FirstOrDefault();
                    if (EmpDtls != null)
                    {
                        item.EmpName = EmpDtls.FirstName + " " + EmpDtls.LastName;
                        item.EmpNameRole = EmpDtls.UserRoleName;
                        item.EmpNameProfilePic = EmpDtls.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + EmpDtls.Id + "/ProfilePhoto/" + EmpDtls.ProfilePhoto : string.Empty;
                    }

                    item.LeaveTypeName = refData.Where(x => x.Id == item.LeaveType).Select(x => x.Rmvalue).FirstOrDefault();
                    item.StatusName = Enum.GetName(typeof(LeaveStatus), item.Status);
                }

                respModel.SetResult(Leaves);
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

        public async Task<GetResponseViewModel<LeavesViewModel>> GetLeave(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<LeavesViewModel>();
            int UserId = Usr.Id;
            try
            {
                
                var Leaves = new LeavesViewModel();

                Leaves = await (from stus in dbContext.PhEmpLeaveRequests
                                where stus.Id == Id
                                select new LeavesViewModel
                                {
                                    ApprovedBy = stus.ApprovedBy,
                                    LeaveType = stus.LeaveType,
                                    EmpId = stus.EmpId,
                                    ApprovedDate = stus.ApprovedDate,
                                    ApproveRemarks = stus.ApproveRemarks,
                                    RejectRemarks = stus.RejectRemarks,
                                    CancelDate = stus.CancelDate,
                                    CancelRemarks = stus.CancelRemarks,
                                    CreatedBy = stus.CreatedBy,
                                    CreatedDate = stus.CreatedDate,
                                    Id = stus.Id,
                                    LeaveEndDate = stus.LeaveEndDate,
                                    LeaveReason = stus.LeaveReason,
                                    LeaveStartDate = stus.LeaveStartDate,
                                    LeaveTypeName = string.Empty,
                                    EmpName = string.Empty,
                                    StatusName = string.Empty,
                                    ApprovedByName = string.Empty,
                                    RejectedDate = stus.RejectedDate,
                                    Status = stus.Status
                                }).FirstOrDefaultAsync();

                var users = dbContext.PiHireUsers.Where(x => x.UserType != (byte)UserType.Candidate).ToList();
                var refData = dbContext.PhRefMasters.Where(x => x.GroupId == 98).ToList();
                if (Leaves != null)
                {
                    Leaves.ApprovedByName = users.Where(x => x.Id == Leaves.ApprovedBy).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                    Leaves.EmpName = users.Where(x => x.Id == Leaves.EmpId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                    Leaves.LeaveTypeName = refData.Where(x => x.Id == Leaves.LeaveType).Select(x => x.Rmvalue).FirstOrDefault();
                    Leaves.StatusName = Enum.GetName(typeof(LeaveStatus), Leaves.Status);
                }

                respModel.SetResult(Leaves);
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

        public async Task<CreateResponseViewModel<string>> CreateLeave(CreateLeaveViewModel createLeaveViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                

                var Leaves = dbContext.PhEmpLeaveRequests.Where(s => s.EmpId == createLeaveViewModel.EmpId && s.Status == (byte)LeaveStatus.Accepted).ToList();
                foreach (var item in Leaves)
                {
                    if (createLeaveViewModel.LeaveStartDate >= item.LeaveStartDate.Date && createLeaveViewModel.LeaveEndDate <= item.LeaveEndDate)
                    {
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "Can't apply the leave to selected date.", true);
                        return respModel;
                    }
                }

                var phEmpLeaveRequest = new PiHire.DAL.Entities.PhEmpLeaveRequest
                {
                    CreatedBy = UserId,
                    Status = (byte)LeaveStatus.Requested,
                    CreatedDate = CurrentTime,
                    LeaveReason = createLeaveViewModel.LeaveReason,
                    LeaveEndDate = createLeaveViewModel.LeaveEndDate,
                    LeaveStartDate = createLeaveViewModel.LeaveStartDate,
                    EmpId = createLeaveViewModel.EmpId,
                    LeaveType = createLeaveViewModel.LeaveType,
                    ApprovedBy = createLeaveViewModel.ApprovedBy,
                    LeaveCategory = createLeaveViewModel.LeaveCategory
                };

                dbContext.PhEmpLeaveRequests.Add(phEmpLeaveRequest);
                await dbContext.SaveChangesAsync();

                var EmpDtls = dbContext.PiHireUsers.Where(x => x.Id == createLeaveViewModel.EmpId && x.UserType != (byte)UserType.Candidate).FirstOrDefault();
                var ApprovedDtls = dbContext.PiHireUsers.Where(x => x.Id == createLeaveViewModel.ApprovedBy && x.UserType != (byte)UserType.Candidate).FirstOrDefault();
                var RefData = dbContext.PhRefMasters.Where(x => x.GroupId == 98 && x.Id == createLeaveViewModel.LeaveType).FirstOrDefault();

                string Approvedby = ApprovedDtls?.FirstName + " " + ApprovedDtls?.LastName;
                string ApproverEmail = ApprovedDtls?.EmailId;
                string RecipientName = EmpDtls?.FirstName + " " + EmpDtls?.LastName;
                string LeaveType = RefData?.Rmvalue;
                string Signature = EmpDtls?.EmailSignature;

                if (!string.IsNullOrEmpty(Signature))
                {
                    Signature = "Talent Acquisition Group, <br /> ParamInfo";
                }

                var mailBody = EmailTemplates.LeaveRequest(Approvedby, LeaveType, createLeaveViewModel.LeaveStartDate, createLeaveViewModel.LeaveEndDate, createLeaveViewModel.LeaveReason, Signature, string.Empty);

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

                await smtp.SendMail(ApproverEmail, "ParamInfo: Application for Leave", mailBody, string.Empty);

                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                var auditLog = new CreateAuditViewModel
                {
                    ActivitySubject = " Leave Request",
                    ActivityDesc = " Leave Request is Created successfully ",
                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                    TaskID = phEmpLeaveRequest.Id,
                    UserId = UserId
                };
                audList.Add(auditLog);
                SaveAuditLog(audList);

                respModel.Status = true;

                string message = "Created Succesfully";
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


        public async Task<CreateResponseViewModel<string>> CreateLeaveInstead(CreateLeaveInsteadViewModel createLeaveViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                

                var Leaves = dbContext.PhEmpLeaveRequests.Where(s => s.EmpId == createLeaveViewModel.EmpId && s.Status == (byte)LeaveStatus.Accepted).ToList();
                foreach (var item in Leaves)
                {
                    if (createLeaveViewModel.LeaveStartDate >= item.LeaveStartDate.Date && createLeaveViewModel.LeaveEndDate <= item.LeaveEndDate)
                    {
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "Can't apply the leave to selected date.", true);
                        return respModel;
                    }
                }

                var phEmpLeaveRequest = new PiHire.DAL.Entities.PhEmpLeaveRequest
                {
                    CreatedBy = UserId,
                    Status = (byte)LeaveStatus.Accepted,
                    CreatedDate = CurrentTime,
                    LeaveReason = createLeaveViewModel.LeaveReason,
                    LeaveEndDate = createLeaveViewModel.LeaveEndDate,
                    LeaveStartDate = createLeaveViewModel.LeaveStartDate,
                    EmpId = createLeaveViewModel.EmpId,
                    LeaveType = createLeaveViewModel.LeaveType,
                    ApprovedBy = UserId,
                    LeaveCategory = createLeaveViewModel.LeaveCategory
                };

                dbContext.PhEmpLeaveRequests.Add(phEmpLeaveRequest);
                await dbContext.SaveChangesAsync();

                var ApprovedDtls = dbContext.PiHireUsers.Where(x => x.Id == UserId && x.UserType != (byte)UserType.Candidate).FirstOrDefault();
                var EmplDtls = dbContext.PiHireUsers.Where(x => x.Id == createLeaveViewModel.EmpId && x.UserType != (byte)UserType.Candidate).FirstOrDefault();

                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                var auditLog = new CreateAuditViewModel
                {
                    ActivitySubject = " Leave is Approved ",
                    ActivityDesc = " Leave is Approved by " + ApprovedDtls?.FirstName + " " + ApprovedDtls?.LastName + " due to " + EmplDtls?.FirstName + " " + EmplDtls?.LastName + " informed over the call",
                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                    TaskID = phEmpLeaveRequest.Id,
                    UserId = UserId
                };
                audList.Add(auditLog);
                SaveAuditLog(audList);

                respModel.Status = true;

                string message = "Created Successfully";
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

        public async Task<UpdateResponseViewModel<string>> UpdateLeave(UpdateLeaveViewModel updateLeaveViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                

                var phEmpLeaveRequest = await dbContext.PhEmpLeaveRequests.Where(x => x.Id == updateLeaveViewModel.Id).FirstOrDefaultAsync();

                string LeaveReason = string.Empty;
                if (phEmpLeaveRequest != null)
                {
                    if (updateLeaveViewModel.Status == (byte)LeaveStatus.Accepted)
                    {
                        phEmpLeaveRequest.ApprovedDate = CurrentTime;
                        phEmpLeaveRequest.ApproveRemarks = updateLeaveViewModel.ApproveRemarks;
                        phEmpLeaveRequest.Status = (byte)LeaveStatus.Accepted;

                        LeaveReason = updateLeaveViewModel.ApproveRemarks;
                    }
                    if (updateLeaveViewModel.Status == (byte)LeaveStatus.Rejected)
                    {
                        phEmpLeaveRequest.RejectedDate = CurrentTime;
                        phEmpLeaveRequest.RejectRemarks = updateLeaveViewModel.RejectRemarks;
                        phEmpLeaveRequest.Status = (byte)LeaveStatus.Rejected;

                        LeaveReason = updateLeaveViewModel.RejectRemarks;
                    }
                    if (updateLeaveViewModel.Status == (byte)LeaveStatus.Canceled)
                    {
                        phEmpLeaveRequest.CancelDate = CurrentTime;
                        phEmpLeaveRequest.CancelRemarks = updateLeaveViewModel.CancelRemarks;
                        phEmpLeaveRequest.Status = (byte)LeaveStatus.Canceled;

                        LeaveReason = updateLeaveViewModel.CancelRemarks;
                    }

                    dbContext.PhEmpLeaveRequests.Update(phEmpLeaveRequest);
                    await dbContext.SaveChangesAsync();

                    var EmpDtls = dbContext.PiHireUsers.Where(x => x.Id == phEmpLeaveRequest.EmpId
                    && x.UserType != (byte)UserType.Candidate).FirstOrDefault();
                    var ApprovedDtls = dbContext.PiHireUsers.Where(x => x.Id == phEmpLeaveRequest.ApprovedBy
                    && x.UserType != (byte)UserType.Candidate).FirstOrDefault();
                    var RefData = dbContext.PhRefMasters.Where(x => x.GroupId == 98 && x.Id == phEmpLeaveRequest.LeaveType).FirstOrDefault();

                    string Approvedby = ApprovedDtls?.FirstName + " " + ApprovedDtls?.LastName;
                    string ApproverEmail = ApprovedDtls?.EmailId;
                    string RecipientName = EmpDtls?.FirstName + " " + EmpDtls?.LastName;
                    string RecipientEmail = EmpDtls?.EmailId;
                    string LeaveType = RefData?.Rmvalue;
                    string Signature = ApprovedDtls?.EmailSignature;

                    if (!string.IsNullOrEmpty(Signature))
                    {
                        Signature = "Talent Acquisition Group, <br /> ParamInfo";
                    }

                    var mailBody = EmailTemplates.LeaveApproval(RecipientName, LeaveType, phEmpLeaveRequest.LeaveStartDate,
                        phEmpLeaveRequest.LeaveEndDate, LeaveReason, Signature, string.Empty);

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

                    await smtp.SendMail(RecipientEmail, "ParamInfo: Approval of Leave application", mailBody, string.Empty);


                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Updated Leave Request",
                        ActivityDesc = "Leave Request is Updated  successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = phEmpLeaveRequest.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    string message = "Updated Succesfully";
                    respModel.SetResult(message);
                }
                else
                {
                    string message = "Leave is not found";
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

        #endregion


        #region Work Shifts 

        public async Task<GetResponseViewModel<List<WorkShiftViewModel>>> GetWorkShifts()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<WorkShiftViewModel>>();
            int UserId = Usr.Id;
            try
            {
                
                var Shifts = new List<WorkShiftViewModel>();

                Shifts = await (from stus in dbContext.PhShifts
                                join user in dbContext.PiHireUsers on stus.CreatedBy equals user.Id
                                where stus.Status != (byte)RecordStatus.Delete
                                select new WorkShiftViewModel
                                {
                                    Id = stus.Id,
                                    CreatedBy = stus.CreatedBy,
                                    CreatedDate = stus.CreatedDate,
                                    ShiftName = stus.ShiftName,
                                    CreatedByName = user.FirstName + " " + user.LastName,
                                    Status = stus.Status,
                                    WorkShiftDtlsViewModel = (from dtls in dbContext.PhShiftDetls
                                                              where dtls.ShiftId == stus.Id
                                                              select new WorkShiftDtlsViewModel
                                                              {
                                                                  Id = dtls.Id,
                                                                  ShiftId = dtls.ShiftId,
                                                                  AlternativeStart = dtls.AlternativeStart,
                                                                  AlternativeWeekStartDate = dtls.AlternativeWeekStartDate,
                                                                  CreatedBy = dtls.CreatedBy,
                                                                  CreatedDate = dtls.CreatedDate,
                                                                  From = dtls.From,
                                                                  FromMeridiem = dtls.FromMeridiem,
                                                                  FromMinutes = dtls.FromMinutes,
                                                                  IsAlternateWeekend = dtls.IsAlternateWeekend,
                                                                  IsWeekend = dtls.IsWeekend,
                                                                  Status = dtls.Status,
                                                                  To = dtls.To,
                                                                  ToMeridiem = dtls.ToMeridiem,
                                                                  ToMinutes = dtls.ToMinutes,
                                                                  UpdatedBy = dtls.UpdatedBy,
                                                                  UpdatedDate = dtls.UpdatedDate,
                                                                  WeekendModel = dtls.WeekendModel,
                                                                  DayName = dtls.DayName
                                                              }).ToList()
                                }).ToListAsync();


                respModel.SetResult(Shifts);
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

        public async Task<GetResponseViewModel<WorkShiftViewModel>> GetWorkShift(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<WorkShiftViewModel>();
            int UserId = Usr.Id;
            try
            {
                
                var Shifts = new WorkShiftViewModel();

                Shifts = await (from stus in dbContext.PhShifts
                                join user in dbContext.PiHireUsers on stus.CreatedBy equals user.Id
                                where stus.Id == Id && stus.Status != (byte)RecordStatus.Delete
                                select new WorkShiftViewModel
                                {
                                    Id = stus.Id,
                                    CreatedBy = stus.CreatedBy,
                                    CreatedDate = stus.CreatedDate,
                                    ShiftName = stus.ShiftName,
                                    CreatedByName = user.FirstName + " " + user.LastName,
                                    Status = stus.Status,
                                    WorkShiftDtlsViewModel = (from dtls in dbContext.PhShiftDetls
                                                              where dtls.ShiftId == stus.Id
                                                              select new WorkShiftDtlsViewModel
                                                              {
                                                                  Id = dtls.Id,
                                                                  ShiftId = dtls.ShiftId,
                                                                  AlternativeStart = dtls.AlternativeStart,
                                                                  AlternativeWeekStartDate = dtls.AlternativeWeekStartDate,
                                                                  CreatedBy = dtls.CreatedBy,
                                                                  CreatedDate = dtls.CreatedDate,
                                                                  From = dtls.From,
                                                                  FromMeridiem = dtls.FromMeridiem,
                                                                  FromMinutes = dtls.FromMinutes,
                                                                  IsAlternateWeekend = dtls.IsAlternateWeekend,
                                                                  IsWeekend = dtls.IsWeekend,
                                                                  Status = dtls.Status,
                                                                  To = dtls.To,
                                                                  ToMeridiem = dtls.ToMeridiem,
                                                                  ToMinutes = dtls.ToMinutes,
                                                                  UpdatedBy = dtls.UpdatedBy,
                                                                  UpdatedDate = dtls.UpdatedDate,
                                                                  WeekendModel = dtls.WeekendModel,
                                                                  DayName = dtls.DayName
                                                              }).ToList()
                                }).FirstOrDefaultAsync();

                respModel.SetResult(Shifts);
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

        public async Task<CreateResponseViewModel<string>> CreateWorkShifts(CreateWorkShiftDtlsViewModel createWorkShiftDtlsViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                

                var Shift = await dbContext.PhShifts.Where(s => s.ShiftName == createWorkShiftDtlsViewModel.ShiftName && s.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (Shift == null)
                {
                    var phShift = new PiHire.DAL.Entities.PhShift
                    {
                        CreatedBy = UserId,
                        Status = (byte)RecordStatus.Active,
                        CreatedDate = CurrentTime,
                        ShiftName = createWorkShiftDtlsViewModel.ShiftName
                    };

                    dbContext.PhShifts.Add(phShift);
                    await dbContext.SaveChangesAsync();

                    foreach (var item in createWorkShiftDtlsViewModel.createWorkShiftViewModels)
                    {
                        var phShiftDetl = new PiHire.DAL.Entities.PhShiftDetl
                        {
                            CreatedBy = UserId,
                            Status = (byte)RecordStatus.Active,
                            CreatedDate = CurrentTime,
                            AlternativeStart = item.AlternativeStart,
                            IsAlternateWeekend = item.IsAlternateWeekend,
                            AlternativeWeekStartDate = item.AlternativeWeekStartDate,
                            From = item.From,
                            FromMeridiem = item.FromMeridiem,
                            FromMinutes = item.FromMinutes,
                            IsWeekend = item.IsWeekend,
                            ShiftId = phShift.Id,
                            To = item.To,
                            ToMeridiem = item.ToMeridiem,
                            ToMinutes = item.ToMinutes,
                            WeekendModel = item.WeekendModel,
                            DayName = item.DayName
                        };

                        dbContext.PhShiftDetls.Add(phShiftDetl);
                        await dbContext.SaveChangesAsync();

                        if (item.IsAlternateWeekend != null)
                        {
                            if (item.IsAlternateWeekend.Value)
                            {
                                int weekDay = ConvertToDayNumber(item.DayName);
                                if (item.AlternativeStart != null)
                                {
                                    if (item.AlternativeStart.Value)
                                    {
                                        int dC = (int)System.DateTime.Now.DayOfWeek;
                                        DateTime today = DateTime.Today;
                                        int daysUntilWeekOff = ((int)weekDay - (int)today.DayOfWeek + 7) % 7;
                                        DateTime nextWeekOff = today.AddDays(daysUntilWeekOff);
                                        item.AlternativeWeekStartDate = nextWeekOff.AddDays(7);

                                    }
                                    else
                                    {
                                        int dC = (int)System.DateTime.Now.DayOfWeek;
                                        DateTime today = DateTime.Today;
                                        int daysUntilWeekOff = ((int)weekDay - (int)today.DayOfWeek + 7) % 7;
                                        DateTime nextWeekOff = today.AddDays(daysUntilWeekOff);
                                        item.AlternativeWeekStartDate = nextWeekOff;
                                    }

                                    phShiftDetl.AlternativeWeekStartDate = item.AlternativeWeekStartDate;

                                    dbContext.PhShiftDetls.Update(phShiftDetl);
                                    await dbContext.SaveChangesAsync();

                                }
                            }
                        }
                    }

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Created Shift Name",
                        ActivityDesc = " Shift Name is Created successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = phShift.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    string message = "Created Succesfully";
                    respModel.SetResult(message);
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "Shift Name is already available", true);
                    return respModel;
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

        public async Task<UpdateResponseViewModel<string>> UpdateWorkShifts(UpdateWorkShiftDtlsViewModel updateWorkShiftDtlsViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                

                var Shift = await dbContext.PhShifts.Where(s => s.Id != updateWorkShiftDtlsViewModel.Id && s.ShiftName == updateWorkShiftDtlsViewModel.ShiftName && s.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (Shift == null)
                {
                    Shift = dbContext.PhShifts.Where(s => s.Id == updateWorkShiftDtlsViewModel.Id).FirstOrDefault();
                    Shift.ShiftName = updateWorkShiftDtlsViewModel.ShiftName;
                    Shift.UpdatedBy = UserId;
                    Shift.UpdatedDate = CurrentTime;

                    dbContext.PhShifts.Update(Shift);
                    await dbContext.SaveChangesAsync();


                    foreach (var item in updateWorkShiftDtlsViewModel.UpdateWorkShiftViewModels)
                    {
                        var phShiftDetl = await dbContext.PhShiftDetls.Where(s => s.Id == item.Id).FirstOrDefaultAsync();
                        if (phShiftDetl != null)
                        {
                            phShiftDetl.AlternativeStart = item.AlternativeStart;
                            phShiftDetl.IsAlternateWeekend = item.IsAlternateWeekend;
                            phShiftDetl.AlternativeWeekStartDate = item.AlternativeWeekStartDate;
                            phShiftDetl.From = item.From;
                            phShiftDetl.FromMeridiem = item.FromMeridiem;
                            phShiftDetl.FromMinutes = item.FromMinutes;
                            phShiftDetl.IsWeekend = item.IsWeekend;
                            phShiftDetl.To = item.To;
                            phShiftDetl.ToMeridiem = item.ToMeridiem;
                            phShiftDetl.ToMinutes = item.ToMinutes;
                            phShiftDetl.WeekendModel = item.WeekendModel;
                            phShiftDetl.DayName = item.DayName;
                            phShiftDetl.UpdatedBy = UserId;
                            phShiftDetl.UpdatedDate = CurrentTime;

                            dbContext.PhShiftDetls.Update(phShiftDetl);
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Updated Shift Name",
                        ActivityDesc = "Shift Name is Updated successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = Shift.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    string message = "Updated Succesfully";
                    respModel.SetResult(message);
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "Shift Name is already available", true);
                    return respModel;
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


        public async Task<UpdateResponseViewModel<string>> UpdateWorkShiftStatus(int Id, byte status)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                

                var Shift = await dbContext.PhShifts.Where(s => s.Id == Id).FirstOrDefaultAsync();
                if (Shift != null)
                {
                    Shift.Status = status;
                    Shift.UpdatedBy = UserId;
                    Shift.UpdatedDate = CurrentTime;

                    dbContext.PhShifts.Update(Shift);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Update Shift Status",
                        ActivityDesc = "Shift Status is Updated successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = Shift.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    string message = "Updated Succesfully";
                    respModel.SetResult(message);
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "Shift Name is not available", true);
                    return respModel;
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


        #endregion


        #region Work Schedule 

        
        public async Task<GetResponseViewModel<List<WorkScheduleDtlsViewModel>>> GetWorkScheduleDtls(WorkScheduleSearchViewModel workScheduleSearchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<WorkScheduleDtlsViewModel>>();

            try
            {
                

                var workScheduleDtlsViewModel1 = new List<WorkScheduleDtlsViewModel>();
                var users = await dbContext.GetRecuiters();

                users = users.Where(x => x.puId == workScheduleSearchViewModel.PuId).GroupBy(x => x.UserId).Select(grp => grp.First()).ToList();

                var shiftDetl = dbContext.PhShiftDetls.Where(x => x.Status != (byte)RecordStatus.Delete).ToList();

                foreach (var item in users)
                {
                    var workScheduleDtlsViewModel = new WorkScheduleDtlsViewModel
                    {
                        Location = item.Location,
                        LocationId = item.LocationId,
                        Name = item.Name,
                        UserId = item.UserId,
                        ShiftId = item.ShiftId,
                        Weekend = shiftDetl.Where(x => x.ShiftId == item.ShiftId && x.IsAlternateWeekend == false && x.IsWeekend == true).Select(x => x.DayName).ToList(),
                        WeekModelObj = shiftDetl.Where(x => x.ShiftId == item.ShiftId && x.IsAlternateWeekend == true && x.IsWeekend == true).Select(x => new SubClassWeekend { WeekendModel = x.WeekendModel, DayName = x.DayName }).ToList()
                    };
                    workScheduleDtlsViewModel1.Add(workScheduleDtlsViewModel);
                }

                workScheduleSearchViewModel.ToDate = workScheduleSearchViewModel.ToDate.AddDays(1);



                for (int i = 0; i < workScheduleDtlsViewModel1.Count; i++)
                {
                    var shifts = shiftDetl.Where(x => x.ShiftId == workScheduleDtlsViewModel1[i].ShiftId).ToList();

                    if (shifts.Count > 0)
                    {
                        List<DateTime> weekends = new List<DateTime>();
                        List<SubClassWeekend> listOfweekModelObj = workScheduleDtlsViewModel1[i].WeekModelObj;
                        foreach (var item in workScheduleDtlsViewModel1[i].Weekend)
                        {
                            int weekNumber = 0;
                            DateTime dt = DateTime.ParseExact(this.appSettings.AppSettingsProperties.GatewayMigratedToHireOn, @"d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);// Hire Start Date
                            DateTime weekendDate = dt;
                            for (DateTime k = workScheduleSearchViewModel.FromDate; k < workScheduleSearchViewModel.ToDate; k = k.AddDays(1))
                            {
                                if (k.DayOfWeek.ToString() == item)
                                {
                                    weekNumber = (int)k.DayOfWeek;
                                    weekendDate = k;
                                    break;
                                }
                            }
                            if (dt < weekendDate)
                            {
                                for (DateTime l = weekendDate; l < workScheduleSearchViewModel.ToDate; l = l.AddDays(7))
                                {
                                    weekends.Add(l.Date);
                                }
                            }                          
                        }

                        var shiftDataModels = new List<ShiftDataModel>();

                        for (DateTime m = workScheduleSearchViewModel.FromDate; m < workScheduleSearchViewModel.ToDate; m = m.AddDays(1))
                        {
                            var shiftDataModel = new ShiftDataModel();
                            shiftDataModel.UserId = workScheduleDtlsViewModel1[i].UserId;

                            if (!weekends.Contains(m.Date))
                            {
                                if ((CheckAlternativeWeekend(m.Date, listOfweekModelObj)))
                                {
                                    shiftDataModel.Date = m;
                                    shiftDataModel.DayName = m.DayOfWeek.ToString();
                                    shiftDataModel.FromHour = (int)shifts.Where(y => y.DayName == m.DayOfWeek.ToString()).Select(y => y.From).FirstOrDefault();
                                    shiftDataModel.ToHour = (int)shifts.Where(y => y.DayName == m.DayOfWeek.ToString()).Select(y => y.To).FirstOrDefault();
                                    shiftDataModel.IsOnLeave = false;
                                    shiftDataModel.IsWeekEnd = false;

                                    var accptLeaveObj = (from stus in dbContext.PhEmpLeaveRequests
                                                         join refd in dbContext.PhRefMasters on stus.LeaveType equals refd.Id
                                                         where stus.EmpId == workScheduleDtlsViewModel1[i].UserId && m.Date >= stus.LeaveStartDate.Date && m.Date <= stus.LeaveEndDate.Date
                                                         && stus.Status == (byte)LeaveStatus.Accepted
                                                         select new
                                                         {
                                                             stus.LeaveType,
                                                             refd.Rmvalue
                                                         }).FirstOrDefault();

                                    if (accptLeaveObj != null)
                                    {
                                        shiftDataModel.LeaveName = accptLeaveObj.Rmvalue;
                                        shiftDataModel.IsOnLeave = true;
                                        shiftDataModel.FromHour = 0;
                                        shiftDataModel.ToHour = 0;
                                    }
                                    if (CurrentTime.Day <= m.Date.Day)
                                    {
                                        shiftDataModel.IsLeaveRequest = true;
                                    }
                                    else
                                    {
                                        shiftDataModel.IsLeaveRequest = false;
                                    }
                                    shiftDataModels.Add(shiftDataModel);
                                }
                                else
                                {
                                    shiftDataModel.Date = m;
                                    shiftDataModel.FromHour = 0;
                                    shiftDataModel.ToHour = 0;
                                    shiftDataModel.IsOnLeave = false;
                                    shiftDataModel.IsWeekEnd = true;
                                    shiftDataModel.DayName = m.DayOfWeek.ToString();

                                    shiftDataModels.Add(shiftDataModel);
                                }
                            }
                            else
                            {

                                shiftDataModel.Date = m;
                                shiftDataModel.DayName = m.DayOfWeek.ToString();
                                shiftDataModel.FromHour = 0;
                                shiftDataModel.ToHour = 0;
                                shiftDataModel.IsOnLeave = false;
                                shiftDataModel.IsWeekEnd = true;

                                shiftDataModels.Add(shiftDataModel);
                            }
                        }

                        workScheduleDtlsViewModel1[i].ShiftData = new List<ShiftDataModel>();
                        workScheduleDtlsViewModel1[i].ShiftData = shiftDataModels;

                    }
                }

                respModel.SetResult(workScheduleDtlsViewModel1);
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

    }
}
