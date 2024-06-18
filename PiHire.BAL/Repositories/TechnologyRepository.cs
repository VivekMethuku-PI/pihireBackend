using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PiHire.BAL.Repositories;
using PiHire.BAL.IRepositories;
using PiHire.BAL.Common.Types;
using PiHire.DAL.Entities;
using PiHire.DAL.Models;

namespace PiHire.BAL.Repositories
{
    public class TechnologyRepository : BaseRepository, ITechnologyRepository
    {
        readonly Logger logger;
        public TechnologyRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<TechnologyRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }


        
        public async Task<GetResponseViewModel<List<TechnologiesModel>>> GetTechnologies()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var Techs = new List<TechnologiesModel>();
            var respModel = new GetResponseViewModel<List<TechnologiesModel>>();
            int UserId = Usr.Id;
            try
            {
                
                Techs = await (from technologys in dbContext.PhTechnologysSes
                               where technologys.Status != (byte)RecordStatus.Delete
                               select new TechnologiesModel
                               {
                                   Id = technologys.Id,
                                   Status = technologys.Status,
                                   Title = technologys.Title
                               }).ToListAsync();


                respModel.SetResult(Techs);
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


        
        public async Task<GetResponseViewModel<TechnologyModel>> GetTechnologies(GetTechnologiesViewModel getTechnologiesViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<TechnologyModel>();
            int UserId = Usr.Id;
            try
            {
                
                getTechnologiesViewModel.CurrentPage = (getTechnologiesViewModel.CurrentPage.Value - 1) * getTechnologiesViewModel.PerPage.Value;
                var data = await dbContext.GetTechnologies(getTechnologiesViewModel.SearchKey, getTechnologiesViewModel.PerPage, getTechnologiesViewModel.CurrentPage);

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

        public async Task<CreateResponseViewModel<string>> CreateTechnology(CreateTechnologiesViewModel createTechnologiesViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                
                var Technologies = await dbContext.PhTechnologysSes.Where(x => x.Title == createTechnologiesViewModel.Title && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (Technologies == null)
                {
                    Technologies = new PhTechnologysS
                    {
                        Title = createTechnologiesViewModel.Title,
                        CreatedBy = UserId,
                        Status = (byte)RecordStatus.Active,
                        CreatedDate = CurrentTime
                    };

                    dbContext.PhTechnologysSes.Add(Technologies);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Created Technology",
                        ActivityDesc = "Technology is Created successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = Technologies.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult("Created Successfully");
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Technology is already available", true);
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

        public async Task<UpdateResponseViewModel<string>> UpdateTechnology(UpdateTechnologiesViewModel updateTechnologiesViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                
                var Technologies = await dbContext.PhTechnologysSes.Where(x => x.Id != updateTechnologiesViewModel.Id && x.Title == updateTechnologiesViewModel.Title && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (Technologies == null)
                {
                    Technologies = await dbContext.PhTechnologysSes.Where(x => x.Id == updateTechnologiesViewModel.Id).FirstOrDefaultAsync();
                    if (Technologies != null)
                    {
                        Technologies.Title = updateTechnologiesViewModel.Title;
                        Technologies.UpdatedDate = CurrentTime;
                        Technologies.UpdatedBy = UserId;

                        dbContext.PhTechnologysSes.Update(Technologies);
                        await dbContext.SaveChangesAsync();

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Updated Technology",
                            ActivityDesc = "Technology is updated successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = Technologies.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        respModel.Status = true;
                        respModel.SetResult("Updated Successfully");
                    }
                    else
                    {
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The Technology is not available", true);
                    }
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Technology is already available", true);
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

        public async Task<DeleteResponseViewModel<string>> DeleteTechnology(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new DeleteResponseViewModel<string>();
            string message = "Deleted Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.DeleteItem, "Start of method:", respModel.Meta.RequestID);
                var technologysS = await dbContext.PhTechnologysSes.Where(x => x.Id == Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (technologysS != null)
                {
                    technologysS.UpdatedBy = UserId;
                    technologysS.UpdatedDate = CurrentTime;
                    technologysS.Status = (byte)RecordStatus.Delete;

                    dbContext.PhTechnologysSes.Update(technologysS);
                    await dbContext.SaveChangesAsync();

                    respModel.Status = true;
                    respModel.SetResult(message);

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Deleted Technology",
                        ActivityDesc = "Technology is Deleted successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);
                }
                else
                {
                    message = "Technology not found";

                    respModel.Status = true;
                    respModel.SetResult(message);
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

        #region Technology Group 

        public async Task<GetResponseViewModel<List<TechnologiesViewModel>>> GetTechnologyGroup(int id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<TechnologiesViewModel>>();
            int UserId = Usr.Id;
            try
            {
                
                var Technologies = new List<TechnologiesViewModel>();

                Technologies = await dbContext.PhTechnologyGroupsSes.Where(da => da.TechnologyGroupId == id).Join(dbContext.PhTechnologysSes,
                    da => da.TechnologyId, da2 => da2.Id,
                    (da, da2) => new TechnologiesViewModel
                    {
                        Id = da2.Id,
                        Title = da2.Title
                    }).ToListAsync();

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

        public async Task<GetResponseViewModel<List<SkillProfilesViewModel>>> TechnologyGroups()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<SkillProfilesViewModel>>();
            int UserId = Usr.Id;
            try
            {
                
                var Technologies = new List<SkillProfilesViewModel>();

                var SkillGroups = await dbContext.PhTechnologyGroupsSes.Where(x => x.Status != (byte)RecordStatus.Delete).GroupBy(x => x.TechnologyGroupId).Select(x => x.Key).ToListAsync();
                foreach (var item in SkillGroups)
                {
                    var detls = new SkillProfilesViewModel
                    {
                        Id = item,
                        Name = dbContext.PhRefMasters.Where(x => x.Id == item).Select(x => x.Rmvalue).FirstOrDefault(),
                        TechnologiesViewModel = (from skillGroup in dbContext.PhTechnologyGroupsSes
                                                 join tech in dbContext.PhTechnologysSes on skillGroup.TechnologyId equals tech.Id
                                                 where skillGroup.TechnologyGroupId == item && skillGroup.Status != (byte)RecordStatus.Delete
                                                 select new TechnologiesViewModel
                                                 {
                                                     Id = skillGroup.TechnologyId,
                                                     Status = skillGroup.Status,
                                                     Title = tech.Title
                                                 }).ToList()
                    };
                    if (detls != null)
                    {
                        Technologies.Add(detls);
                    }
                }

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

        public async Task<CreateResponseViewModel<string>> CreateTechnologyGroup(CreateSkillProfileViewModel createSkillProfileViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            int UserId = Usr.Id;
            try
            {
                

                var RefValue = dbContext.PhRefMasters.Where(x => x.Rmvalue.ToLower() == createSkillProfileViewModel.SkillProfileName.Trim().ToLower() && x.GroupId == 186 && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (RefValue == null)
                {
                    var refData = new PhRefMaster
                    {
                        GroupId = 186,
                        CreatedBy = UserId,
                        CreatedDate = CurrentTime,
                        Rmdesc = createSkillProfileViewModel.SkillProfileName,
                        Rmtype = "SkillGroup",
                        Rmvalue = createSkillProfileViewModel.SkillProfileName,
                        Status = (byte)RecordStatus.Active
                    };

                    dbContext.PhRefMasters.Add(refData);
                    await dbContext.SaveChangesAsync();

                    foreach (var item in createSkillProfileViewModel.TechnologyId)
                    {
                        var technologyGroup = new PhTechnologyGroupsS
                        {
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            Status = (byte)RecordStatus.Active,
                            TechnologyGroupId = refData.Id,
                            TechnologyId = item
                        };
                        dbContext.PhTechnologyGroupsSes.Add(technologyGroup);
                        await dbContext.SaveChangesAsync();
                    }

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Created New Skill Profile Group",
                        ActivityDesc = " Skill Group is Created successfully",
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
                    message = "Skill Profile Name is already available";
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

        public async Task<UpdateResponseViewModel<string>> UpdateTechnologyGroup(UpdateSkillProfileViewModel updateSkillProfileViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod()); 
            
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);
                var RefValue = dbContext.PhRefMasters.Where(x => x.Id != updateSkillProfileViewModel.Id && x.Rmvalue.ToLower() == updateSkillProfileViewModel.SkillProfileName.Trim().ToLower() && x.GroupId == 186 && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (RefValue == null)
                {
                    RefValue = dbContext.PhRefMasters.Where(x => x.Id == updateSkillProfileViewModel.Id).FirstOrDefault();
                    if (RefValue != null)
                    {
                        RefValue.Rmvalue = updateSkillProfileViewModel.SkillProfileName;
                        RefValue.UpdatedBy = UserId;
                        RefValue.UpdatedDate = CurrentTime;

                        dbContext.PhRefMasters.Update(RefValue);
                        await dbContext.SaveChangesAsync();

                        var techGroups = await dbContext.PhTechnologyGroupsSes.Where(x => x.TechnologyGroupId == updateSkillProfileViewModel.Id && !updateSkillProfileViewModel.TechnologyId.Contains(x.TechnologyId) && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                        foreach (var groups in techGroups)
                        {
                            groups.Status = (byte)RecordStatus.Delete;
                            groups.UpdatedBy = UserId;
                            groups.UpdatedDate = CurrentTime;

                            dbContext.PhTechnologyGroupsSes.Update(groups);
                            await dbContext.SaveChangesAsync();
                        }

                        foreach (var item in updateSkillProfileViewModel.TechnologyId)
                        {
                            var checRerd = dbContext.PhTechnologyGroupsSes.Where(x => x.TechnologyGroupId == updateSkillProfileViewModel.Id && x.TechnologyId == item && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                            if (checRerd == null)
                            {
                                var technologyGroup = new PhTechnologyGroupsS
                                {
                                    CreatedBy = UserId,
                                    CreatedDate = CurrentTime,
                                    Status = (byte)RecordStatus.Active,
                                    TechnologyGroupId = updateSkillProfileViewModel.Id,
                                    TechnologyId = item
                                };
                                dbContext.PhTechnologyGroupsSes.Add(technologyGroup);
                                await dbContext.SaveChangesAsync();
                            }
                        }

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Updated Skill Profile Group",
                            ActivityDesc = " Skill Profile Group is Updated successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = RefValue.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }
                    else
                    {
                        message = "Skill Profile Name is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                }
                else
                {
                    message = "Skill Profile Name is already available";
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

        public async Task<DeleteResponseViewModel<string>> DeleteTechnologyGroup(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod()); 
            
            int UserId = Usr.Id;
            var respModel = new DeleteResponseViewModel<string>();
            string message = "Deleted Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.DeleteItem, "Start of method:", respModel.Meta.RequestID);

                var SkillGroup = await dbContext.PhTechnologyGroupsSes.Where(x => x.TechnologyGroupId == Id && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                if (SkillGroup.Count > 0)
                {
                    foreach (var item in SkillGroup)
                    {
                        item.UpdatedBy = UserId;
                        item.UpdatedDate = CurrentTime;
                        item.Status = (byte)RecordStatus.Delete;

                        dbContext.PhTechnologyGroupsSes.Update(item);
                        await dbContext.SaveChangesAsync();

                        respModel.Status = true;
                        respModel.SetResult(message);
                    }

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Deleted Skill Profile",
                        ActivityDesc = "Skill Profile is Deleted successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);
                }

                else
                {
                    message = "Skill Profile is not found";
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
    }
}
