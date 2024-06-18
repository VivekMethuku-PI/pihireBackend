using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PiHire.BAL.Common.Http;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;
using PiHire.BAL.Common.Types;

namespace PiHire.BAL.Repositories
{
    public class RefRepository : BaseRepository, IRefRepository
    {
        readonly Logger logger;
        public RefRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<RefRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }

        public async Task<GetResponseViewModel<List<RefMasterViewModel>>> GetRefList()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<RefMasterViewModel>>();
            int UserId = Usr.Id;
            try
            {


                var RefData = new List<RefMasterViewModel>();

                RefData = await (from data in dbContext.PhRefMasters
                                 where data.Status != (byte)RecordStatus.Delete
                                 select new RefMasterViewModel
                                 {
                                     Id = data.Id,
                                     GroupId = data.GroupId,
                                     Rmdesc = data.Rmdesc,
                                     Rmtype = data.Rmtype,
                                     Rmvalue = data.Rmvalue,
                                     Status = data.Status,
                                     CreatedDate = data.CreatedDate
                                 }).OrderByDescending(x => x.CreatedDate).ToListAsync();

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
        public async Task<GetResponseViewModel<RefMasterViewModel>> GetRefData(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<RefMasterViewModel>();
            int UserId = Usr.Id;
            try
            {


                var RefData = new RefMasterViewModel();

                RefData = await (from data in dbContext.PhRefMasters
                                 where data.Id == Id
                                 select new RefMasterViewModel
                                 {
                                     Id = data.Id,
                                     GroupId = data.GroupId,
                                     Rmdesc = data.Rmdesc,
                                     Rmtype = data.Rmtype,
                                     Rmvalue = data.Rmvalue,
                                     Status = data.Status,
                                     CreatedDate = data.CreatedDate
                                 }).FirstOrDefaultAsync();

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
            int UserId = Usr.Id;
            try
            {


                var RefData = new List<RefDataViewModel>();

                RefData = await (from data in dbContext.PhRefMasters
                                 where GroupId.Contains(data.GroupId)
                                 select new RefDataViewModel
                                 {
                                     Id = data.Id,
                                     GroupId = data.GroupId,
                                     Rmdesc = data.Rmdesc,
                                     Rmtype = data.Rmtype,
                                     Rmvalue = data.Rmvalue
                                 }).ToListAsync();

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
        public async Task<GetResponseViewModel<List<ReferenceDataViewModel>>> GetRefData(ReferenceGroupType referenceGroup)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<ReferenceDataViewModel>>();
            //int UserId = Usr.Id;
            try
            {
                var RefData = await (from data in dbContext.PhRefMasters
                                     where data.GroupId == (int)referenceGroup && data.Status == (byte)RecordStatus.Active
                                     select new ReferenceDataViewModel
                                     {
                                         Id = data.Id,
                                         Rmdesc = data.Rmdesc,
                                         Rmvalue = data.Rmvalue
                                     }).ToListAsync();
                respModel.SetResult(RefData);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, $"referenceGroup:{referenceGroup}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }

        public async Task<CreateResponseViewModel<string>> CreateRefValue(CreateRefValuesViewModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string saveEndPoint = string.Empty;
            int UserId = Usr.Id;
            saveEndPoint = "api/ReferenceMaster/updateReferenceRecord";

            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {


                    var createRefValueViewModel = new CreateRefValueViewModel
                    {
                        groupid = model.GroupId,
                        value = model.Rmvalue,
                        description = model.Rmdesc,
                        type = model.Rmtype
                    };

                    if (model.Id == 0)
                    {
                        var refData = await dbContext.PhRefMasters.Where(x => x.GroupId == model.GroupId && x.Rmtype == model.Rmtype && x.Rmvalue == model.Rmvalue && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();

                        if (refData == null)
                        {
                            refData = new PhRefMaster
                            {
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                Status = (byte)RecordStatus.Active,
                                GroupId = model.GroupId,
                                Rmvalue = model.Rmvalue,
                                Rmtype = model.Rmtype,
                                Rmdesc = model.Rmdesc
                            };

                            dbContext.PhRefMasters.Add(refData);
                            await dbContext.SaveChangesAsync();

                            createRefValueViewModel.id = refData.Id;

                            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                            var auditLog = new CreateAuditViewModel
                            {
                                ActivitySubject = "Created new ref data",
                                ActivityDesc = " Added new record in ref master for groupId - " + model.GroupId + " ",
                                ActivityType = (byte)AuditActivityType.RecordUpdates,
                                TaskID = refData.Id,
                                UserId = UserId
                            };
                            audList.Add(auditLog);
                            SaveAuditLog(audList);

                            respModel.Status = true;
                            respModel.SetResult("Created Successfully");
                        }
                        else
                        {
                            respModel.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Value is already available");
                            return respModel;
                        }
                    }
                    else
                    {

                        var refData = await dbContext.PhRefMasters.Where(x => x.Id != x.Id && x.GroupId == model.GroupId && x.Rmtype == model.Rmtype && x.Rmvalue == model.Rmvalue && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();

                        if (refData == null)
                        {
                            refData = await dbContext.PhRefMasters.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                            createRefValueViewModel.oldvalue = refData.Rmvalue;

                            if (refData != null)
                            {
                                refData.Rmdesc = model.Rmdesc;
                                refData.Rmtype = model.Rmtype;
                                refData.Rmvalue = model.Rmvalue;

                                refData.UpdatedBy = UserId;
                                refData.UpdatedDate = CurrentTime;

                                dbContext.PhRefMasters.Update(refData);
                                await dbContext.SaveChangesAsync();

                                createRefValueViewModel.id = refData.Id;


                                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                                var auditLog = new CreateAuditViewModel
                                {
                                    ActivitySubject = "Updated Ref data",
                                    ActivityDesc = " Updated record in ref master for groupId - " + createRefValueViewModel.groupid + " ",
                                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                                    TaskID = refData.Id,
                                    UserId = UserId
                                };
                                audList.Add(auditLog);
                                SaveAuditLog(audList);

                                respModel.Status = true;
                                respModel.SetResult("Updated Successfully");
                            }
                            else
                            {
                                respModel.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The Value is not available");
                                return respModel;
                            }
                        }
                        else
                        {
                            respModel.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Value is already available");
                            return respModel;
                        }
                    }

                    using var client1 = new HttpClientService();

                    var response = await client1.PostAsync(appSettings.AppSettingsProperties.GatewayUrl, saveEndPoint, createRefValueViewModel);
                    if (response.IsSuccessStatusCode)
                    {
                        logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ", gateway ref save success:" + Newtonsoft.Json.JsonConvert.SerializeObject(response), respModel.Meta.RequestID);
                    }
                    else
                    {
                        logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, ", gateway ref save fail:" + Newtonsoft.Json.JsonConvert.SerializeObject(response), respModel.Meta.RequestID);
                    }


                    //using var client = new HttpClientService();
                    //var odooAccessResponse = client.Get(appSettings.AppSettingsProperties.OdooBaseURL,
                    //              appSettings.AppSettingsProperties.OdooLoginURL, appSettings.AppSettingsProperties.OdooDb, appSettings.AppSettingsProperties.OdooUsername, appSettings.AppSettingsProperties.OdooPassword);

                    //var responseContent = await odooAccessResponse.Content.ReadAsStringAsync();
                    //string access_token = JObject.Parse(responseContent)["access_token"].ToString();

                    //var cnvtEmployee = await client.PostAsync(appSettings.AppSettingsProperties.OdooBaseURL, appSettings.AppSettingsProperties.CreateUpdateRefData, access_token, createRefValueViewModel);
                    //if (cnvtEmployee.IsSuccessStatusCode)
                    //{
                    //    var cnvtEmployeeResponseContent = await cnvtEmployee.Content.ReadAsStringAsync();
                    //    var result = JsonConvert.DeserializeObject<int>(cnvtEmployeeResponseContent);

                    //    logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, ", odoo ref save success:" + Newtonsoft.Json.JsonConvert.SerializeObject(cnvtEmployeeResponseContent), respModel.Meta.RequestID);
                    //}
                    //else
                    //{
                    //    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ", odoo ref save fail:" + Newtonsoft.Json.JsonConvert.SerializeObject(cnvtEmployee), respModel.Meta.RequestID);
                    //}

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = string.Empty;
                    trans.Rollback();
                }
            return respModel;
        }
        public async Task<UpdateResponseViewModel<string>> UpdateRefValue(UpdateRefValuesViewModel updateRefValuesViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            int UserId = Usr.Id;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {


                    var refData = await dbContext.PhRefMasters.Where(x => x.Id != updateRefValuesViewModel.Id && x.GroupId == updateRefValuesViewModel.GroupId && x.Rmtype == updateRefValuesViewModel.Rmtype && x.Rmvalue == updateRefValuesViewModel.Rmvalue && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                    if (refData == null)
                    {
                        refData = await dbContext.PhRefMasters.Where(x => x.Id == updateRefValuesViewModel.Id).FirstOrDefaultAsync();

                        refData.Rmdesc = updateRefValuesViewModel.Rmdesc;
                        refData.Rmtype = updateRefValuesViewModel.Rmtype;
                        refData.Rmvalue = updateRefValuesViewModel.Rmvalue;
                        refData.UpdatedBy = UserId;
                        refData.UpdatedDate = CurrentTime;

                        dbContext.PhRefMasters.Update(refData);
                        await dbContext.SaveChangesAsync();

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Updated Ref data",
                            ActivityDesc = " Updated record in ref master for groupId - " + updateRefValuesViewModel.GroupId + " ",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = refData.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }
                    else
                    {
                        message = "Rm Value is already available";
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
        public async Task<UpdateResponseViewModel<string>> UpdateTemplateStatus(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var RefData = dbContext.PhRefMasters.Where(x => x.Id == Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (RefData != null)
                {
                    RefData.UpdatedBy = UserId;
                    RefData.UpdatedDate = CurrentTime;
                    RefData.Status = (byte)RecordStatus.Delete;

                    dbContext.PhRefMasters.Update(RefData);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Deleted Ref Data",
                        ActivityDesc = " Ref Master record is deleted successfully",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = RefData.Id,
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
                    message = "No record is found";
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

        //public async Task<GetResponseViewModel<List<ReferenceDataViewModel>>> GetCandidateEducationQualifications_Graduations()
        //{
        //    logger.SetMethodName(MethodBase.GetCurrentMethod());
        //    var respModel = new GetResponseViewModel<List<ReferenceDataViewModel>>();
        //    //int UserId = Usr.Id;
        //    try
        //    {
        //        var RefData = await (from data in dbContext.PhEducationQualificationMasters
        //                             where data.GroupType == (byte)EducationQualificationGroup.Graduation && data.Status == (byte)RecordStatus.Active
        //                             select new ReferenceDataViewModel
        //                             {
        //                                 Id = data.Id,
        //                                 Rmdesc = data.Desc,
        //                                 Rmvalue = data.Title
        //                             }).ToListAsync();
        //        respModel.SetResult(RefData);
        //        respModel.Status = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log(LogLevel.Error, LoggingEvents.ListItems, $"", respModel.Meta.RequestID, ex);
        //        respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
        //    }
        //    return respModel;
        //}
        //public async Task<GetResponseViewModel<List<ReferenceDataViewModel>>> GetCandidateEducationQualifications_GraduationSpecializations(int graduationId)
        //{
        //    logger.SetMethodName(MethodBase.GetCurrentMethod());
        //    var respModel = new GetResponseViewModel<List<ReferenceDataViewModel>>();
        //    //int UserId = Usr.Id;
        //    try
        //    {
        //        var RefData = await (from data in dbContext.PhEducationQualificationMasters
        //                             where data.GroupType == (byte)EducationQualificationGroup.GraduationSpecialization && data.GroupId == graduationId && data.Status == (byte)RecordStatus.Active
        //                             select new ReferenceDataViewModel
        //                             {
        //                                 Id = data.Id,
        //                                 Rmdesc = data.Desc,
        //                                 Rmvalue = data.Title
        //                             }).ToListAsync();
        //        respModel.SetResult(RefData);
        //        respModel.Status = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log(LogLevel.Error, LoggingEvents.ListItems, $"graduationId:{graduationId}", respModel.Meta.RequestID, ex);
        //        respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
        //    }
        //    return respModel;
        //}
    }
}
