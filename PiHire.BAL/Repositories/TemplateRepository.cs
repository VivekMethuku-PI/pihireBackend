using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.Common.Types;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Entities;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class TemplateRepository : BaseRepository, ITemplateRepository
    {
        readonly Logger logger;
        public TemplateRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<TemplateRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }

        public async Task<GetResponseViewModel<List<TemplatesViewModel>>> GetTemplateList(TemplateSerachViewModel templateSerachViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<TemplatesViewModel>>();
            int UserId = Usr.Id;
            try
            {
                

                var Tempaltes = await (from mt in dbContext.PhMessageTemplates
                                       where mt.MessageType == templateSerachViewModel.MessageType && mt.Status != (byte)RecordStatus.Delete
                                       select new TemplatesViewModel
                                       {
                                           Id = mt.Id,
                                           Status = mt.Status,
                                           MessageType = mt.MessageType,
                                           Code = mt.Code,
                                           CreatedDate = mt.CreatedDate,
                                           DynamicLabels = mt.DynamicLabels,
                                           ProfileType = mt.ProfileType,
                                           PublishStatus = mt.PublishStatus,
                                           SentBy = mt.SentBy,
                                           TemplateName = mt.TplTitle,
                                           TplBody = mt.TplBody,
                                           TplDesc = mt.TplDesc,
                                           TplSubject = mt.TplSubject,
                                           TplFullBody = mt.TplFullBody,
                                           IndustryId = mt.IndustryId,
                                           IndustryName = string.Empty
                                       }).ToListAsync();

                if (templateSerachViewModel.IndustryId != null && templateSerachViewModel.IndustryId != 0)
                {
                    Tempaltes = Tempaltes.Where(x => x.IndustryId == templateSerachViewModel.IndustryId).ToList();
                }

                foreach (var item in Tempaltes)
                {
                    item.SentByName = Enum.GetName(typeof(SentBy), item.SentBy);
                    item.MessageTypeName = Enum.GetName(typeof(MessageType), item.MessageType);
                    item.MessageTypeName = Enum.GetName(typeof(ProfileType), item.ProfileType);
                    if (item.IndustryId != null)
                    {
                        item.IndustryName = dbContext.PhRefMasters.Where(x => x.GroupId == 50 && x.Id == item.IndustryId).Select(x => x.Rmvalue).FirstOrDefault();
                    }
                }

                respModel.SetResult(Tempaltes);
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
        public async Task<GetResponseViewModel<TemplatesViewModel>> GetTemplate(int tempId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<TemplatesViewModel>();
            int UserId = Usr.Id; try

            {
                
                var Tempalte = await (from mt in dbContext.PhMessageTemplates
                                      where mt.Id == tempId && mt.Status != (byte)RecordStatus.Delete
                                      select new TemplatesViewModel
                                      {
                                          Id = mt.Id,
                                          Status = mt.Status,
                                          MessageType = mt.MessageType,
                                          Code = mt.Code,
                                          CreatedDate = mt.CreatedDate,
                                          DynamicLabels = mt.DynamicLabels,
                                          ProfileType = mt.ProfileType,
                                          PublishStatus = mt.PublishStatus,
                                          SentBy = mt.SentBy,
                                          TemplateName = mt.TplTitle,
                                          TplBody = mt.TplBody,
                                          TplDesc = mt.TplDesc,
                                          TplSubject = mt.TplSubject,
                                          IndustryId = mt.IndustryId,
                                          IndustryName = string.Empty
                                      }).FirstOrDefaultAsync();

                if (Tempalte != null)
                {
                    Tempalte.SentByName = Tempalte.SentBy != 0 ? Enum.GetName(typeof(SentBy), Tempalte.SentBy) : string.Empty;
                    Tempalte.MessageTypeName = Tempalte.MessageType != 0 ? Enum.GetName(typeof(MessageType), Tempalte.MessageType) : string.Empty;
                    Tempalte.ProfileTypeName = Tempalte.ProfileType != 0 ? Enum.GetName(typeof(ProfileType), Tempalte.ProfileType) : string.Empty;
                    if (Tempalte.IndustryId != null)
                    {
                        Tempalte.IndustryName = dbContext.PhRefMasters.Where(x => x.GroupId == 50 && x.Id == Tempalte.IndustryId).Select(x => x.Rmvalue).FirstOrDefault();
                    }
                }

                respModel.SetResult(Tempalte);
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

        public async Task<CreateResponseViewModel<string>> CreateTemplate(CreateJobTemplateViewModel createJobTemplateViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try

                {
                    

                    var template = await dbContext.PhMessageTemplates.Where(x => x.TplTitle == createJobTemplateViewModel.TplTitle && x.MessageType == createJobTemplateViewModel.MessageType && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                    if (template == null)
                    {

                        var phMessageTemplates = new PhMessageTemplate
                        {
                            Code = "ET", // it will replace after insertion
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            DynamicLabels = createJobTemplateViewModel.DynamicLabels,
                            MessageType = createJobTemplateViewModel.MessageType,
                            ProfileType = createJobTemplateViewModel.ProfileType,
                            PublishStatus = false, // 0 - Unpublished, 1 - Published,
                            SentBy = createJobTemplateViewModel.SentBy,
                            Status = (byte)RecordStatus.Active,
                            TplBody = createJobTemplateViewModel.TplBody,
                            TplDesc = createJobTemplateViewModel.TplDesc,
                            TplSubject = createJobTemplateViewModel.TplSubject,
                            TplTitle = createJobTemplateViewModel.TplTitle,
                            TplFullBody = createJobTemplateViewModel.TplFullBody,
                            IndustryId = createJobTemplateViewModel.IndustryId
                        };

                        dbContext.PhMessageTemplates.Add(phMessageTemplates);
                        await dbContext.SaveChangesAsync();

                        phMessageTemplates.Code = GetTemplateCode(phMessageTemplates.Id, phMessageTemplates.MessageType);
                        await dbContext.SaveChangesAsync();

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Created New Template",
                            ActivityDesc = " has created the Template successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = phMessageTemplates.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }
                    else
                    {
                        message = "Template is already available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
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

        public async Task<UpdateResponseViewModel<string>> UpdateTemplate(UpdateJobTemplateViewModel updateJobTemplateViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    

                    var template = await dbContext.PhMessageTemplates.Where(x => x.Id != updateJobTemplateViewModel.Id && x.TplTitle == updateJobTemplateViewModel.TplTitle && x.MessageType == updateJobTemplateViewModel.MessageType && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                    if (template == null)
                    {
                        template = await dbContext.PhMessageTemplates.Where(x => x.Id == updateJobTemplateViewModel.Id).FirstOrDefaultAsync();

                        template.DynamicLabels = updateJobTemplateViewModel.DynamicLabels;
                        template.MessageType = updateJobTemplateViewModel.MessageType;
                        template.ProfileType = updateJobTemplateViewModel.ProfileType;
                        template.SentBy = updateJobTemplateViewModel.SentBy;
                        template.TplBody = updateJobTemplateViewModel.TplBody;
                        template.TplDesc = updateJobTemplateViewModel.TplDesc;
                        template.TplSubject = updateJobTemplateViewModel.TplSubject;
                        template.TplTitle = updateJobTemplateViewModel.TplTitle;
                        template.TplFullBody = updateJobTemplateViewModel.TplFullBody;
                        template.IndustryId = updateJobTemplateViewModel.IndustryId;

                        dbContext.PhMessageTemplates.Update(template);
                        await dbContext.SaveChangesAsync();

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Edited Template",
                            ActivityDesc = " updated the Template successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = template.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }
                    else
                    {
                        message = "Template is already available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
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

        public async Task<UpdateResponseViewModel<string>> UnPublishTemplate(UpdateJobTemplateStatus updateJobTemplateStatus)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Unpublished Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var MessageTemplate = dbContext.PhMessageTemplates.Where(x => x.Id == updateJobTemplateStatus.Id && x.Status != (byte)RecordStatus.Delete &&
                x.PublishStatus != false).FirstOrDefault();
                if (MessageTemplate != null)
                {
                    MessageTemplate.UpdatedBy = UserId;
                    MessageTemplate.UpdatedDate = CurrentTime;
                    MessageTemplate.PublishStatus = false;

                    dbContext.PhMessageTemplates.Update(MessageTemplate);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Template Status",
                        ActivityDesc = " updated the " + MessageTemplate.TplTitle + " Template Unpublished successfully",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = MessageTemplate.Id,
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
                    message = "The Message template is already Unpublished";
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

        public async Task<UpdateResponseViewModel<string>> UpdateTemplateStatus(UpdateJobTemplateStatus updateJobTemplateStatus)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var MessageTemplate = dbContext.PhMessageTemplates.Where(x => x.Id == updateJobTemplateStatus.Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (MessageTemplate != null)
                {
                    MessageTemplate.UpdatedBy = UserId;
                    MessageTemplate.UpdatedDate = CurrentTime;
                    MessageTemplate.Status = updateJobTemplateStatus.Status;

                    dbContext.PhMessageTemplates.Update(MessageTemplate);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " template updated successfully",
                        ActivityDesc = " updated the " + MessageTemplate.TplTitle + " template updated successfully",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = MessageTemplate.Id,
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
                    message = "The Message template is not available";
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

        public async Task<UpdateResponseViewModel<string>> PublishTemplate(UpdateJobTemplateStatus updateJobTemplateStatus)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Published Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var MessageTemplate = dbContext.PhMessageTemplates.Where(x => x.Id == updateJobTemplateStatus.Id && x.Status != (byte)RecordStatus.Delete &&
               x.PublishStatus != true).FirstOrDefault();
                if (MessageTemplate != null)
                {
                    MessageTemplate.UpdatedBy = UserId;
                    MessageTemplate.UpdatedDate = CurrentTime;
                    MessageTemplate.PublishStatus = true;

                    dbContext.PhMessageTemplates.Update(MessageTemplate);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Template status updated successfully",
                        ActivityDesc = " updated the " + MessageTemplate.TplTitle + " template published successfully",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = MessageTemplate.Id,
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
                    message = "The Message template is already published";
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


    }
}
