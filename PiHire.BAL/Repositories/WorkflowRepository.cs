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
using PiHire.BAL.Common.Types;

namespace PiHire.BAL.Repositories
{
    public class WorkflowRepository : BaseRepository, IWorkflowRepository
    {
        readonly Logger logger;
        public WorkflowRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<AutomationRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }

        
        public async Task<GetResponseViewModel<List<WorkflowRuleViewmodel>>> GetWorkflowRules(byte sort)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<WorkflowRuleViewmodel>>();
            int UserId = Usr.Id;
            try
            {
                
                var workRules = new List<WorkflowRuleViewmodel>();

                var taskList = await TasksList();
                var phJobStatusS = await dbContext.PhJobStatusSes.ToListAsync();
                var phCandStatusS = await dbContext.PhCandStatusSes.ToListAsync();
                var refData = await dbContext.PhRefMasters.Where(x => x.GroupId == 15).ToListAsync();

                workRules = (from workFlow in dbContext.PhWorkflows
                             where workFlow.Status != (byte)RecordStatus.Delete
                             select new WorkflowRuleViewmodel
                             {
                                 Id = workFlow.Id,
                                 ActionMode = workFlow.ActionMode,
                                 CreatedDate = workFlow.CreatedDate,
                                 Status = workFlow.Status,
                                 TaskCode = workFlow.TaskCode,
                                 TaskId = workFlow.TaskId,
                                 ActionModeName = string.Empty,
                                 TaskName = string.Empty,
                                 WorkflowRuleDetailsViewmodel = (from WorkFlowdtls in dbContext.PhWorkflowsDets
                                                                 where WorkFlowdtls.WorkflowId == workFlow.Id && WorkFlowdtls.Status != (byte)RecordStatus.Delete
                                                                 select new WorkflowRuleDetailsViewmodel
                                                                 {
                                                                     Id = WorkFlowdtls.Id,
                                                                     WorkflowId = WorkFlowdtls.WorkflowId,
                                                                     ActionType = WorkFlowdtls.ActionType,
                                                                     AsmtOrTplId = WorkFlowdtls.AsmtOrTplId,
                                                                     CurrentStatusId = WorkFlowdtls.CurrentStatusId,
                                                                     UpdateStatusId = WorkFlowdtls.UpdateStatusId,
                                                                     SendMode = WorkFlowdtls.SendMode,
                                                                     SendTo = WorkFlowdtls.SendTo,
                                                                     Status = WorkFlowdtls.Status,
                                                                     DocsReqstdIds = WorkFlowdtls.DocsReqstdIds,
                                                                     ActionTypeName = string.Empty,
                                                                     CurrentStatusName = string.Empty,
                                                                     UpdateStatusName = string.Empty,
                                                                     AsmtOrTplName = string.Empty,
                                                                     SendModeName = string.Empty,
                                                                     SendToName = string.Empty,
                                                                     DocsReqstdNames = string.Empty
                                                                 }).ToList()
                             }).ToList();
                foreach (var item in workRules)
                {
                    var name = taskList.Where(x => x.Id == item.TaskId).Select(x => x.Description).FirstOrDefault();
                    if (!string.IsNullOrEmpty(name))
                    {
                        item.TaskName = name;
                    }
                    item.ActionModeName = Enum.GetName(typeof(WorkflowActionMode), item.ActionMode);
                    foreach (var workflow in item.WorkflowRuleDetailsViewmodel)
                    {
                        workflow.ActionTypeName = Enum.GetName(typeof(WorkflowActionTypes), workflow.ActionType);
                        workflow.SendModeName = Enum.GetName(typeof(SendMode), workflow.SendMode);
                        if (workflow.SendTo != null)
                        {
                            workflow.SendToName = Enum.GetName(typeof(UserType), workflow.SendTo);
                        }
                        if (workflow.CurrentStatusId != null)
                        {
                            if (item.ActionMode == (byte)WorkflowActionMode.Opening)
                            {
                                var dtls = phJobStatusS.Where(x => x.Id == workflow.CurrentStatusId).Select(x => new { x.Title, x.Status }).FirstOrDefault();
                                if (dtls != null)
                                {
                                    workflow.CurrentStatusIdIsEnable = dtls.Status;
                                    workflow.CurrentStatusName = dtls.Title;
                                }
                            }
                            else
                            {
                                var dtls = phCandStatusS.Where(x => x.Id == workflow.CurrentStatusId).Select(x => new { x.Title, x.Status }).FirstOrDefault();
                                if (dtls != null)
                                {
                                    workflow.CurrentStatusIdIsEnable = dtls.Status;
                                    workflow.CurrentStatusName = dtls.Title;
                                }
                            }
                        }
                        if (workflow.UpdateStatusId != null)
                        {
                            if (item.ActionMode == (byte)WorkflowActionMode.Opening)
                            {
                                var dtls = phJobStatusS.Where(x => x.Id == workflow.UpdateStatusId).Select(x => new { x.Title, x.Status }).FirstOrDefault();
                                if (dtls != null)
                                {

                                    workflow.UpdateStatusIdEnable = dtls.Status;
                                    workflow.UpdateStatusName = dtls.Title;
                                }
                            }
                            else
                            {
                                var dtls = phCandStatusS.Where(x => x.Id == workflow.UpdateStatusId).Select(x => new { x.Title, x.Status }).FirstOrDefault();
                                if (dtls != null)
                                {

                                    workflow.UpdateStatusIdEnable = dtls.Status;
                                    workflow.UpdateStatusName = dtls.Title;
                                }
                            }
                        }
                        if (workflow.AsmtOrTplId != null)
                        {
                            workflow.AsmtOrTplName = dbContext.PhMessageTemplates.Where(x => x.Id == workflow.AsmtOrTplId).Select(x => x.TplTitle).FirstOrDefault();
                        }
                        if (workflow.DocsReqstdIds != null)
                        {
                            List<int> DocsReqstdIds = workflow.DocsReqstdIds.Split(",").Select(int.Parse).ToList();
                            workflow.DocsReqstdNames = string.Join(",", refData.Where(x => DocsReqstdIds.Contains(x.Id)).Select(x => x.Rmvalue));
                        }
                    }
                }

                if (workRules.Count > 0 && sort != 0)
                {
                    workRules = workRules.Where(x => x.ActionMode == sort).ToList();
                }

                respModel.Status = true;
                respModel.SetResult(workRules);
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

        
        public async Task<GetResponseViewModel<EditWorkflowRuleViewmodel>> GetWorkflowRule(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<EditWorkflowRuleViewmodel>();
            int UserId = Usr.Id;
            try
            {
                
                var workRules = new EditWorkflowRuleViewmodel();

                var TaskList = await TasksList();
                workRules = await (from workFlow in dbContext.PhWorkflows
                                   where workFlow.Id == Id && workFlow.Status != (byte)RecordStatus.Delete
                                   select new EditWorkflowRuleViewmodel
                                   {
                                       ActionMode = workFlow.ActionMode,
                                       TaskCode = workFlow.TaskCode,
                                       TaskId = workFlow.TaskId,
                                       TaskName = string.Empty,
                                       Id = workFlow.Id,
                                       WorkflowRuleDetailsViewmodel = (from WorkFlowdtls in dbContext.PhWorkflowsDets
                                                                       where
                                                                       WorkFlowdtls.WorkflowId == workFlow.Id &&
                                                                       WorkFlowdtls.Status != (byte)RecordStatus.Delete
                                                                       select new EditWorkflowRuleDetailsViewmodel
                                                                       {
                                                                           Id = WorkFlowdtls.Id,
                                                                           ActionType = WorkFlowdtls.ActionType,
                                                                           AsmtOrTplId = WorkFlowdtls.AsmtOrTplId,
                                                                           CurrentStatusId = WorkFlowdtls.CurrentStatusId,
                                                                           UpdateStatusId = WorkFlowdtls.UpdateStatusId,
                                                                           SendMode = WorkFlowdtls.SendMode,
                                                                           SendTo = WorkFlowdtls.SendTo,
                                                                           DocsReqstdIds = WorkFlowdtls.DocsReqstdIds
                                                                       }).ToList()
                                   }).FirstOrDefaultAsync();
                if (workRules != null)
                {
                    workRules.TaskName = TaskList.Where(x => x.Id == workRules.TaskId).Select(x => x.Description).FirstOrDefault();
                }

                respModel.Status = true;
                respModel.SetResult(workRules);
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

        public async Task<CreateResponseViewModel<string>> CreateWorkflowRule(CreateWorkflowRuleViewmodel workflowRuleViewmodel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    
                    var Workflow = new PhWorkflow
                    {
                        CreatedBy = UserId,
                        CreatedDate = CurrentTime,
                        Status = (byte)RecordStatus.Active,
                        ActionMode = workflowRuleViewmodel.ActionMode,
                        TaskCode = workflowRuleViewmodel.TaskCode,
                        TaskId = workflowRuleViewmodel.TaskId
                    };

                    dbContext.PhWorkflows.Add(Workflow);
                    await dbContext.SaveChangesAsync();

                    foreach (var item in workflowRuleViewmodel.WorkflowRuleDetailsViewmodel)
                    {
                        var WorkflowDetails = dbContext.PhWorkflowsDets.Where(x =>
                        x.ActionType == item.ActionType && x.CurrentStatusId == item.CurrentStatusId &&
                        x.UpdateStatusId == item.UpdateStatusId && x.AsmtOrTplId == item.AsmtOrTplId
                        && x.SendMode == item.SendMode && x.SendTo == item.SendTo && x.WorkflowId == Workflow.Id &&
                        x.Status != (byte)RecordStatus.Delete).FirstOrDefault();

                        if (WorkflowDetails == null)
                        {
                            WorkflowDetails = new PhWorkflowsDet
                            {
                                ActionType = item.ActionType,
                                SendTo = item.SendTo,
                                SendMode = item.SendMode,
                                AsmtOrTplId = item.AsmtOrTplId,
                                CurrentStatusId = item.ActionType == (byte)WorkflowActionTypes.ChangeStatus ? item.CurrentStatusId : null,
                                UpdateStatusId = item.ActionType == (byte)WorkflowActionTypes.ChangeStatus ? item.UpdateStatusId : null,
                                WorkflowId = Workflow.Id,
                                Status = (byte)RecordStatus.Active,
                                DocsReqstdIds = item.DocsReqstdIds
                            };

                            dbContext.PhWorkflowsDets.Add(WorkflowDetails);
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Created New Workflow Rule",
                        ActivityDesc = " Workflow is Created successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = Workflow.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult(message);

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

        public async Task<UpdateResponseViewModel<string>> UpdateWorkflowRule(EditWorkflowRuleViewmodel workflowRuleViewmodel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);
                    var Workflow = await dbContext.PhWorkflows.Where(x => x.Id == workflowRuleViewmodel.Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                    if (Workflow != null)
                    {
                        Workflow.TaskId = workflowRuleViewmodel.TaskId;
                        Workflow.TaskCode = workflowRuleViewmodel.TaskCode;
                        Workflow.ActionMode = workflowRuleViewmodel.ActionMode;
                        await dbContext.SaveChangesAsync();

                        var workFlowIds = new List<int>();
                        if (workflowRuleViewmodel.WorkflowRuleDetailsViewmodel != null)
                        {
                            workFlowIds = workflowRuleViewmodel.WorkflowRuleDetailsViewmodel.Select(x => x.Id).ToList();
                        }
                        var isExistWorkFlowRule = await dbContext.PhWorkflowsDets.Where(x => x.WorkflowId == workflowRuleViewmodel.Id && !workFlowIds.Contains(x.Id) && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                        foreach (var workflow in isExistWorkFlowRule)
                        {
                            workflow.Status = (byte)RecordStatus.Delete;

                            dbContext.PhWorkflowsDets.Update(workflow);
                            await dbContext.SaveChangesAsync();
                        }

                        foreach (var item in workflowRuleViewmodel.WorkflowRuleDetailsViewmodel)
                        {
                            if (item.Id == 0)
                            {
                                var WorkflowDetails = dbContext.PhWorkflowsDets.Where(x =>
                           x.ActionType == item.ActionType && x.CurrentStatusId == item.CurrentStatusId &&
                           x.UpdateStatusId == item.UpdateStatusId && x.AsmtOrTplId == item.AsmtOrTplId
                           && x.SendMode == item.SendMode && x.SendTo == item.SendTo && x.WorkflowId == Workflow.Id &&
                           x.Status != (byte)RecordStatus.Delete).FirstOrDefault();

                                if (WorkflowDetails == null)
                                {
                                    WorkflowDetails = new PhWorkflowsDet
                                    {
                                        ActionType = item.ActionType,
                                        SendTo = item.SendTo,
                                        SendMode = item.SendMode,
                                        AsmtOrTplId = item.AsmtOrTplId,
                                        CurrentStatusId = item.CurrentStatusId,
                                        UpdateStatusId = item.UpdateStatusId,
                                        WorkflowId = Workflow.Id,
                                        Status = (byte)RecordStatus.Active,
                                        DocsReqstdIds = item.DocsReqstdIds
                                    };

                                    dbContext.PhWorkflowsDets.Add(WorkflowDetails);
                                    await dbContext.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                var WorkflowDetails = dbContext.PhWorkflowsDets.Where(x => x.Id != item.Id &&
                          x.ActionType == item.ActionType && x.CurrentStatusId == item.CurrentStatusId &&
                          x.UpdateStatusId == item.UpdateStatusId && x.AsmtOrTplId == item.AsmtOrTplId
                          && x.SendMode == item.SendMode && x.SendTo == item.SendTo && x.WorkflowId == Workflow.Id &&
                          x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                                if (WorkflowDetails == null)
                                {
                                    WorkflowDetails = dbContext.PhWorkflowsDets.Where(x => x.Id == item.Id).FirstOrDefault();
                                    if (WorkflowDetails != null)
                                    {
                                        WorkflowDetails.ActionType = item.ActionType;
                                        WorkflowDetails.SendTo = item.SendTo;
                                        WorkflowDetails.SendMode = item.SendMode;
                                        WorkflowDetails.AsmtOrTplId = item.AsmtOrTplId;
                                        WorkflowDetails.CurrentStatusId = item.ActionType == (byte)WorkflowActionTypes.ChangeStatus ? item.CurrentStatusId : null;
                                        WorkflowDetails.UpdateStatusId = item.ActionType == (byte)WorkflowActionTypes.ChangeStatus ? item.UpdateStatusId : null;
                                        WorkflowDetails.WorkflowId = Workflow.Id;
                                        WorkflowDetails.DocsReqstdIds = item.DocsReqstdIds;

                                        dbContext.PhWorkflowsDets.Update(WorkflowDetails);
                                        await dbContext.SaveChangesAsync();
                                    }
                                }
                            }
                        }

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Updated Workflow Rule",
                            ActivityDesc = " Workflow is Updated successfully ",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = Workflow.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }
                    else
                    {
                        message = "Workflow rule is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
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

        public async Task<DeleteResponseViewModel<string>> DeleteWorkflowDetails(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new DeleteResponseViewModel<string>();
            int UserId = Usr.Id;
            string message = "Deleted Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.DeleteItem, "Start of method:", respModel.Meta.RequestID);
                var WorkflowDtls = await dbContext.PhWorkflowsDets.Where(x => x.Id == Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (WorkflowDtls != null)
                {

                    WorkflowDtls.Status = (byte)RecordStatus.Delete;

                    dbContext.PhWorkflowsDets.UpdateRange(WorkflowDtls);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Deleted Workflow details",
                        ActivityDesc = "Workflow details is Deleted successfully",
                        ActivityType = (byte)AuditActivityType.Critical,
                        TaskID = WorkflowDtls.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Workflow Details is not found";
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

        public async Task<DeleteResponseViewModel<string>> DeleteWorkflowRule(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new DeleteResponseViewModel<string>();
            string message = "Deleted Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.DeleteItem, "Start of method:", respModel.Meta.RequestID);

                var Workflow = await dbContext.PhWorkflows.Where(x => x.Id == Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (Workflow != null)
                {
                    Workflow.UpdatedBy = UserId;
                    Workflow.Status = (byte)RecordStatus.Delete;
                    Workflow.UpdatedDate = CurrentTime;

                    dbContext.PhWorkflows.UpdateRange(Workflow);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Deleted Workflow Rule",
                        ActivityDesc = " Workflow is Deleted successfully",
                        ActivityType = (byte)AuditActivityType.Critical,
                        TaskID = Workflow.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Workflow Rule is not found";
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
    }
}
