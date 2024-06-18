using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PiHire.BAL.IRepositories;
using PiHire.DAL.Entities;
using PiHire.DAL.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using PiHire.BAL.Common.Http;
using Newtonsoft.Json;

using iText.Layout.Element;
using iText.Html2pdf;
using iText.Layout;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Events;
using iText.Kernel.Font;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace PiHire.BAL.Repositories
{
    public class OffersRepository : BaseRepository, IOffersRepository
    {
        readonly Logger logger;
        
        private readonly IWebHostEnvironment _environment;
        
        public OffersRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<OffersRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        #region SALARY Slab
        public async Task<CreateResponseViewModel<string>> CreateSlab(CreateSlabViewModel createSlabViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                
                var slabsS = await dbContext.PhSalarySlabsSes.Where(x => x.Title == createSlabViewModel.Title && x.Puid == createSlabViewModel.Puid && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (slabsS == null)
                {
                    var phSalarySlabsS = new PiHire.DAL.Entities.PhSalarySlabsS
                    {
                        Title = createSlabViewModel.Title,
                        CreatedBy = UserId,
                        Status = (byte)RecordStatus.Active,
                        CreatedDate = CurrentTime,
                        Puid = createSlabViewModel.Puid,
                        SlabDesc = createSlabViewModel.SlabDesc
                    };

                    dbContext.PhSalarySlabsSes.Add(phSalarySlabsS);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Created NEW Slab",
                        ActivityDesc = "Slab is Created successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = phSalarySlabsS.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult("Saved Successfully");
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Slab is already available", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(createSlabViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<UpdateResponseViewModel<string>> UpdateSlab(UpdateSlabViewModel updateSlabViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                
                var slabsS = await dbContext.PhSalarySlabsSes.Where(x => x.Id != updateSlabViewModel.Id && x.Title == updateSlabViewModel.Title && x.Puid == updateSlabViewModel.Puid && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (slabsS == null)
                {
                    slabsS = await dbContext.PhSalarySlabsSes.Where(x => x.Id == updateSlabViewModel.Id).FirstOrDefaultAsync();
                    if (slabsS != null)
                    {
                        slabsS.Title = updateSlabViewModel.Title;
                        slabsS.UpdatedBy = UserId;
                        slabsS.UpdatedDate = CurrentTime;
                        slabsS.Puid = updateSlabViewModel.Puid;
                        slabsS.SlabDesc = updateSlabViewModel.SlabDesc;

                        dbContext.PhSalarySlabsSes.Update(slabsS);
                        await dbContext.SaveChangesAsync();

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Updated Slab",
                            ActivityDesc = " Slab is Updated successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = slabsS.Id,
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
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The Slab isn't available", true);
                    }
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Slab is already available", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(updateSlabViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<DeleteResponseViewModel<string>> DeleteSlab(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new DeleteResponseViewModel<string>();
            string message = "Deleted Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var phSalarySlabsS = dbContext.PhSalarySlabsSes.Where(x => x.Id == Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (phSalarySlabsS != null)
                {
                    phSalarySlabsS.UpdatedBy = UserId;
                    phSalarySlabsS.UpdatedDate = CurrentTime;
                    phSalarySlabsS.Status = (byte)RecordStatus.Delete;

                    dbContext.PhSalarySlabsSes.Update(phSalarySlabsS);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Deleted salary slab",
                        ActivityDesc = " Salary Slab is deleted successfully",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = phSalarySlabsS.Id,
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
                    message = "The Salary Slab isn't available";
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

        public async Task<GetResponseViewModel<SlabViewModel>> GetSlab(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<SlabViewModel>();
            int UserId = Usr.Id;
            try
            {
                
                var slabViewModel = new SlabViewModel();

                slabViewModel = await (from stus in dbContext.PhSalarySlabsSes
                                       where stus.Id == Id
                                       select new SlabViewModel
                                       {
                                           Id = stus.Id,
                                           SlabDesc = stus.SlabDesc,
                                           Puid = stus.Puid,
                                           Title = stus.Title
                                       }).FirstOrDefaultAsync();

                respModel.SetResult(slabViewModel);
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

        public async Task<GetResponseViewModel<List<SlabViewModel>>> GetSlabs()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<SlabViewModel>>();
            int UserId = Usr.Id;
            try
            {
                
                var slabViewModels = new List<SlabViewModel>();

                slabViewModels = await (from stus in dbContext.PhSalarySlabsSes
                                        where stus.Status != (byte)RecordStatus.Delete
                                        select new SlabViewModel
                                        {
                                            Id = stus.Id,
                                            SlabDesc = stus.SlabDesc,
                                            Puid = stus.Puid,
                                            Title = stus.Title,
                                            CreatedDate = stus.CreatedDate
                                        }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                respModel.SetResult(slabViewModels);
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

        #region SALARY Componet
        public async Task<CreateResponseViewModel<string>> CreateSalaryComponet(CreateSalayComponentViewModel createSalayComponentViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                
                var phSalaryCompS = await dbContext.PhSalaryComps.Where(x => x.Title == createSalayComponentViewModel.Title && x.CompType == createSalayComponentViewModel.CompType && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (phSalaryCompS == null)
                {
                    phSalaryCompS = new PhSalaryComp
                    {
                        Title = createSalayComponentViewModel.Title,
                        CreatedBy = UserId,
                        Status = (byte)RecordStatus.Active,
                        CreatedDate = CurrentTime,
                        CompType = createSalayComponentViewModel.CompType,
                        CompDesc = createSalayComponentViewModel.CompDesc
                    };

                    dbContext.PhSalaryComps.Add(phSalaryCompS);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Created Componet",
                        ActivityDesc = " Componet is Created successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = phSalaryCompS.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult("Saved Successfully");
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Componet is already available", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(createSalayComponentViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<UpdateResponseViewModel<string>> UpdateSalaryComponet(UpdateSalayComponentViewModel updateSalayComponentViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                
                var phSalaryCompS = await dbContext.PhSalaryComps.Where(x => x.Id != updateSalayComponentViewModel.Id && x.Title == updateSalayComponentViewModel.Title && x.CompType == updateSalayComponentViewModel.CompType && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (phSalaryCompS == null)
                {
                    phSalaryCompS = await dbContext.PhSalaryComps.Where(x => x.Id == updateSalayComponentViewModel.Id).FirstOrDefaultAsync();
                    if (phSalaryCompS != null)
                    {
                        phSalaryCompS.Title = updateSalayComponentViewModel.Title;
                        phSalaryCompS.UpdatedBy = UserId;
                        phSalaryCompS.UpdatedDate = CurrentTime;
                        phSalaryCompS.CompType = updateSalayComponentViewModel.CompType;
                        phSalaryCompS.CompDesc = updateSalayComponentViewModel.CompDesc;

                        dbContext.PhSalaryComps.Update(phSalaryCompS);
                        await dbContext.SaveChangesAsync();

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Updated Component",
                            ActivityDesc = " Component is updated successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = phSalaryCompS.Id,
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
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The Component isn't available", true);
                    }
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Component is already available", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(updateSalayComponentViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<DeleteResponseViewModel<string>> DeleteSalaryComponet(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new DeleteResponseViewModel<string>();
            string message = "Deleted Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var phSalaryCompS = dbContext.PhSalaryComps.Where(x => x.Id == Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (phSalaryCompS != null)
                {
                    phSalaryCompS.UpdatedBy = UserId;
                    phSalaryCompS.UpdatedDate = CurrentTime;
                    phSalaryCompS.Status = (byte)RecordStatus.Delete;

                    dbContext.PhSalaryComps.Update(phSalaryCompS);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Salary Component",
                        ActivityDesc = " Salary component is deleted successfully",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = phSalaryCompS.Id,
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
                    message = "The Salary Component isn't available";
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

        public async Task<GetResponseViewModel<SalayComponentViewModel>> GetSalaryComponet(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<SalayComponentViewModel>();
            int UserId = Usr.Id;
            try
            {
                
                var salayComponentViewModel = new SalayComponentViewModel();

                salayComponentViewModel = await (from stus in dbContext.PhSalaryComps
                                                 join Ref in dbContext.PhRefMasters on stus.CompType equals Ref.Id
                                                 where stus.Id == Id && stus.Status != (byte)RecordStatus.Delete
                                                 select new SalayComponentViewModel
                                                 {
                                                     Id = stus.Id,
                                                     CompDesc = stus.CompDesc,
                                                     CompType = stus.CompType,
                                                     Title = stus.Title,
                                                     CompTypeName = Ref.Rmvalue
                                                 }).FirstOrDefaultAsync();

                respModel.SetResult(salayComponentViewModel);
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

        public async Task<GetResponseViewModel<List<SalayComponentViewModel>>> GetSalaryComponets()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<SalayComponentViewModel>>();
            int UserId = Usr.Id;
            try
            {
                
                var salayComponentViewModels = new List<SalayComponentViewModel>();

                salayComponentViewModels = await (from stus in dbContext.PhSalaryComps
                                                  join Ref in dbContext.PhRefMasters on stus.CompType equals Ref.Id
                                                  where stus.Status != (byte)RecordStatus.Delete
                                                  select new SalayComponentViewModel
                                                  {
                                                      Id = stus.Id,
                                                      CompDesc = stus.CompDesc,
                                                      CompType = stus.CompType,
                                                      CompTypeName = Ref.Rmvalue,
                                                      Title = stus.Title,
                                                      CreatedDate = stus.CreatedDate
                                                  }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                respModel.SetResult(salayComponentViewModels);
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

        #region  SALARY SLABS WISE COMPS
        public async Task<CreateResponseViewModel<string>> CreateSlabComponet(CreateSlabComponentViewModel createSlabComponentViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                
                var phSalarySlabsWiseCompsS = await dbContext.PhSalarySlabsWiseCompsSes.Where(x => x.Puid == createSlabComponentViewModel.Puid
                && x.CompId == createSlabComponentViewModel.CompId
                && x.SlabId == createSlabComponentViewModel.SlabId && x.PercentageFlag == x.PercentageFlag && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (phSalarySlabsWiseCompsS == null)
                {
                    phSalarySlabsWiseCompsS = new PiHire.DAL.Entities.PhSalarySlabsWiseCompsS
                    {
                        CreatedBy = UserId,
                        Status = (byte)RecordStatus.Active,
                        CreatedDate = CurrentTime,
                        Amount = createSlabComponentViewModel.Amount,
                        SlabId = createSlabComponentViewModel.SlabId,
                        CompId = createSlabComponentViewModel.CompId,
                        Puid = createSlabComponentViewModel.Puid,
                        PercentageFlag = createSlabComponentViewModel.PercentageFlag
                    };

                    dbContext.PhSalarySlabsWiseCompsSes.Add(phSalarySlabsWiseCompsS);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Created Slab Componet",
                        ActivityDesc = " Slab Componet is Created successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = phSalarySlabsWiseCompsS.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.Status = true;
                    respModel.SetResult("Saved Successfully");
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Slab Component is already available", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(createSlabComponentViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<UpdateResponseViewModel<string>> UpdateSlabComponet(UpdateSlabComponentViewModel updateSlabComponentViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                
                var phSalarySlabsWiseCompsS = await dbContext.PhSalarySlabsWiseCompsSes.Where(x => x.Id != updateSlabComponentViewModel.Id && x.Puid == updateSlabComponentViewModel.Puid && x.CompId == updateSlabComponentViewModel.CompId && x.SlabId == updateSlabComponentViewModel.SlabId
                && x.PercentageFlag == x.PercentageFlag && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (phSalarySlabsWiseCompsS == null)
                {
                    phSalarySlabsWiseCompsS = await dbContext.PhSalarySlabsWiseCompsSes.Where(x => x.Id == updateSlabComponentViewModel.Id).FirstOrDefaultAsync();
                    if (phSalarySlabsWiseCompsS != null)
                    {
                        phSalarySlabsWiseCompsS.Puid = updateSlabComponentViewModel.Puid;
                        phSalarySlabsWiseCompsS.UpdatedBy = UserId;
                        phSalarySlabsWiseCompsS.UpdatedDate = CurrentTime;
                        phSalarySlabsWiseCompsS.SlabId = updateSlabComponentViewModel.SlabId;
                        phSalarySlabsWiseCompsS.PercentageFlag = updateSlabComponentViewModel.PercentageFlag;
                        phSalarySlabsWiseCompsS.Amount = updateSlabComponentViewModel.Amount;
                        phSalarySlabsWiseCompsS.CompId = updateSlabComponentViewModel.CompId;

                        dbContext.PhSalarySlabsWiseCompsSes.Update(phSalarySlabsWiseCompsS);
                        await dbContext.SaveChangesAsync();

                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Updated Slab Component",
                            ActivityDesc = "Slab Component is Updated successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = phSalarySlabsWiseCompsS.Id,
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
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The Slab Component isn't available", true);
                    }
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Slab Component is already available", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(updateSlabComponentViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<DeleteResponseViewModel<string>> DeleteSlabComponet(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new DeleteResponseViewModel<string>();
            string message = "Deleted Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var phSalarySlabsWiseCompsS = dbContext.PhSalarySlabsWiseCompsSes.Where(x => x.Id == Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (phSalarySlabsWiseCompsS != null)
                {
                    phSalarySlabsWiseCompsS.UpdatedBy = UserId;
                    phSalarySlabsWiseCompsS.UpdatedDate = CurrentTime;
                    phSalarySlabsWiseCompsS.Status = (byte)RecordStatus.Delete;

                    dbContext.PhSalarySlabsWiseCompsSes.Update(phSalarySlabsWiseCompsS);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Deleted Salary Slab Component",
                        ActivityDesc = "Salary Slab Component is deleted",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = phSalarySlabsWiseCompsS.Id,
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
                    message = "The Salary Slab Component isn't available";
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

        public async Task<GetResponseViewModel<SlabComponentViewModel>> GetSlabComponet(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<SlabComponentViewModel>();
            int UserId = Usr.Id;
            try
            {
                
                var slabComponentViewModel = new SlabComponentViewModel();

                slabComponentViewModel = await (from stus in dbContext.PhSalarySlabsWiseCompsSes
                                                join comp in dbContext.PhSalaryComps on stus.CompId equals comp.Id
                                                join slab in dbContext.PhSalarySlabsSes on stus.SlabId equals slab.Id
                                                join setn in dbContext.PhRefMasters on comp.CompType equals setn.Id
                                                where stus.Id == Id && stus.Status != (byte)RecordStatus.Delete
                                                select new SlabComponentViewModel
                                                {
                                                    Id = stus.Id,
                                                    Amount = stus.Amount,
                                                    CompId = stus.CompId,
                                                    CompName = comp.Title,
                                                    ComType = comp.CompType,
                                                    ComTypeNam = setn.Rmvalue,
                                                    PercentageFlag = stus.PercentageFlag,
                                                    Puid = stus.Puid,
                                                    SlabId = stus.SlabId,
                                                    SlabName = slab.Title,
                                                    CreatedDate = stus.CreatedDate
                                                }).FirstOrDefaultAsync();

                respModel.SetResult(slabComponentViewModel);
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

        public async Task<GetResponseViewModel<List<SlabComponentViewModel>>> GetSlabComponets()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<SlabComponentViewModel>>();
            int UserId = Usr.Id;
            try
            {
                
                var slabComponentViewModels = new List<SlabComponentViewModel>();

                slabComponentViewModels = await (from stus in dbContext.PhSalarySlabsWiseCompsSes
                                                 join comp in dbContext.PhSalaryComps on stus.CompId equals comp.Id
                                                 join slab in dbContext.PhSalarySlabsSes on stus.SlabId equals slab.Id
                                                 join setn in dbContext.PhRefMasters on comp.CompType equals setn.Id
                                                 where stus.Status != (byte)RecordStatus.Delete
                                                 select new SlabComponentViewModel
                                                 {
                                                     Id = stus.Id,
                                                     Amount = stus.Amount,
                                                     CompId = stus.CompId,
                                                     CompName = comp.Title,
                                                     ComType = comp.CompType,
                                                     ComTypeNam = setn.Rmvalue,
                                                     PercentageFlag = stus.PercentageFlag,
                                                     Puid = stus.Puid,
                                                     SlabId = stus.SlabId,
                                                     SlabName = slab.Title,
                                                     CreatedDate = stus.CreatedDate
                                                 }).OrderBy(x => x.CreatedDate).ToListAsync();

                respModel.SetResult(slabComponentViewModels);
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

        #region OFFER LETTER 

        public async Task<GetResponseViewModel<List<GrpBySlabModel>>> GetSlabComponetDtls(int Id, int PuId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<GrpBySlabModel>>();
            int UserId = Usr.Id;
            try
            {
                
                var slabComponentDtlsViewModel = new List<SlabComponentDtlsViewModel>();

                slabComponentDtlsViewModel = await (from stus in dbContext.PhSalarySlabsWiseCompsSes
                                                    join comp in dbContext.PhSalaryComps on stus.CompId equals comp.Id
                                                    join refData in dbContext.PhRefMasters on comp.CompType equals refData.Id
                                                    join slab in dbContext.PhSalarySlabsSes on stus.SlabId equals slab.Id
                                                    where stus.SlabId == Id && stus.Puid == PuId && stus.Status != (byte)RecordStatus.Delete
                                                    select new SlabComponentDtlsViewModel
                                                    {
                                                        Id = stus.Id,
                                                        Amount = stus.Amount,
                                                        CompId = stus.CompId,
                                                        CompName = comp.Title,
                                                        CompType = comp.CompType,
                                                        CompTypeName = refData.Rmvalue,
                                                        PercentageFlag = stus.PercentageFlag,
                                                        Puid = stus.Puid,
                                                        SlabId = stus.SlabId,
                                                        SlabName = slab.Title,
                                                        CreatedDate = stus.CreatedDate
                                                    }).OrderBy(x => x.CreatedDate).ToListAsync();

                List<GrpBySlabModel> componentsGrpBy = slabComponentDtlsViewModel.GroupBy(x => x.CompTypeName).Select(grp => new GrpBySlabModel { CompTypeName = grp.Key, slabComponentDtlsViewModels = grp.ToList() }).ToList();

                respModel.SetResult(componentsGrpBy);
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

        
        public async Task<CreateResponseViewModel<string>> CreateOfferLetterWithSlab(CreateOfferLetterSlabViewModel createOfferLetterSlabViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                

                var phJobOfferLetters = new PiHire.DAL.Entities.PhJobOfferLetter
                {
                    CreatedBy = UserId,
                    Status = (byte)RecordStatus.Active,
                    CreatedDate = CurrentTime,
                    BasicSalary = createOfferLetterSlabViewModel.OfferLetterViewModel.BasicSalary,
                    CandProfId = createOfferLetterSlabViewModel.OfferLetterViewModel.CandProfId,
                    Conveyance = createOfferLetterSlabViewModel.OfferLetterViewModel.Conveyance,
                    CurrencyId = createOfferLetterSlabViewModel.OfferLetterViewModel.CurrencyId,
                    DepartmentId = createOfferLetterSlabViewModel.OfferLetterViewModel.DepartmentId,
                    DesignationId = createOfferLetterSlabViewModel.OfferLetterViewModel.DesignationId,
                    Gratuity = createOfferLetterSlabViewModel.OfferLetterViewModel.Gratuity,
                    GrossSalary = createOfferLetterSlabViewModel.OfferLetterViewModel.GrossSalary,
                    GrossSalaryPerAnnum = createOfferLetterSlabViewModel.OfferLetterViewModel.GrossSalaryPerAnnum,
                    Hra = createOfferLetterSlabViewModel.OfferLetterViewModel.Hra,
                    Ita = createOfferLetterSlabViewModel.OfferLetterViewModel.DesignationId,
                    Joid = createOfferLetterSlabViewModel.OfferLetterViewModel.JobId,
                    JoiningDate = createOfferLetterSlabViewModel.OfferLetterViewModel.JoiningDate,
                    NetSalary = createOfferLetterSlabViewModel.OfferLetterViewModel.NetSalary,
                    Otbonus = createOfferLetterSlabViewModel.OfferLetterViewModel.Otbonus,
                    ProcessUnitId = createOfferLetterSlabViewModel.OfferLetterViewModel.ProcessUnitId,
                    Sickness = createOfferLetterSlabViewModel.OfferLetterViewModel.Sickness,
                    SpecId = createOfferLetterSlabViewModel.OfferLetterViewModel.SpecId,
                    SignatureAuthority = createOfferLetterSlabViewModel.OfferLetterViewModel.SignatureAuthority,
                    EmployeeType = createOfferLetterSlabViewModel.OfferLetterViewModel.EmployeeType,
                    CompanyId = createOfferLetterSlabViewModel.OfferLetterViewModel.LocationId  // will reffer this as location Id 
                };

                dbContext.PhJobOfferLetters.Add(phJobOfferLetters);
                await dbContext.SaveChangesAsync();

                foreach (var item in createOfferLetterSlabViewModel.SlabComponentValuesModel)
                {
                    var phJobOfferSlabDetails = new PhJobOfferSlabDetail
                    {
                        Amount = item.Amount,
                        CandProfId = createOfferLetterSlabViewModel.OfferLetterViewModel.CandProfId,
                        ComponentId = item.ComponentId,
                        CreatedBy = UserId,
                        CreatedDate = CurrentTime,
                        JobOfferId = phJobOfferLetters.Id,
                        Joid = createOfferLetterSlabViewModel.OfferLetterViewModel.JobId,
                        SlabId = item.SlabId,
                        Status = (byte)RecordStatus.Active
                    };
                    dbContext.PhJobOfferSlabDetails.Add(phJobOfferSlabDetails);
                    await dbContext.SaveChangesAsync();
                }

                var downloadOfferLetter = await DownloadOfferLetter(phJobOfferLetters.Id, true);
                if (downloadOfferLetter.Status)
                {
                    var result = downloadOfferLetter.Result;
                    if (result != null)
                    {
                        phJobOfferLetters.FileUrl = result.FileURL;
                        phJobOfferLetters.FileType = result.FileType;
                        phJobOfferLetters.FileName = result.FileName;

                        var fileUpload = new PhCandidateDoc()
                        {
                            DocType = "OfferLetter",
                            FileGroup = (byte)FileGroup.Other,
                            FileName = result.FileName,
                            DocStatus = (byte)DocStatus.Accepted,
                            Joid = createOfferLetterSlabViewModel.OfferLetterViewModel.JobId,
                            CandProfId = createOfferLetterSlabViewModel.OfferLetterViewModel.CandProfId,
                            FileType = result.FileType,
                            CreatedBy = UserId
                        };

                        dbContext.PhCandidateDocs.Add(fileUpload);
                        await dbContext.SaveChangesAsync();
                    }
                }

                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                var auditLog = new CreateAuditViewModel
                {
                    ActivitySubject = "Created Offer letter",
                    ActivityDesc = " Offer letter is Created successfully",
                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                    TaskID = phJobOfferLetters.Id,
                    UserId = UserId
                };
                audList.Add(auditLog);
                SaveAuditLog(audList);

                respModel.Status = true;
                respModel.SetResult("Created Successfully");

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(createOfferLetterSlabViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        
        public async Task<CreateResponseViewModel<string>> CreateOfferLetterWithOutSlab(CreateOfferLetterWithSlabViewModel createOfferLetterWithSlabViewModel)
        {

            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            try
            {
                

                var phJobOfferLetters = new PiHire.DAL.Entities.PhJobOfferLetter
                {
                    CreatedBy = UserId,
                    Status = (byte)RecordStatus.Active,
                    CreatedDate = CurrentTime,
                    BasicSalary = createOfferLetterWithSlabViewModel.OfferLetterViewModel.BasicSalary,
                    CandProfId = createOfferLetterWithSlabViewModel.OfferLetterViewModel.CandProfId,
                    Conveyance = createOfferLetterWithSlabViewModel.OfferLetterViewModel.Conveyance,
                    CurrencyId = createOfferLetterWithSlabViewModel.OfferLetterViewModel.CurrencyId,
                    DepartmentId = createOfferLetterWithSlabViewModel.OfferLetterViewModel.DepartmentId,
                    DesignationId = createOfferLetterWithSlabViewModel.OfferLetterViewModel.DesignationId,
                    Gratuity = createOfferLetterWithSlabViewModel.OfferLetterViewModel.Gratuity,
                    GrossSalary = createOfferLetterWithSlabViewModel.OfferLetterViewModel.GrossSalary,
                    GrossSalaryPerAnnum = createOfferLetterWithSlabViewModel.OfferLetterViewModel.GrossSalaryPerAnnum,
                    Hra = createOfferLetterWithSlabViewModel.OfferLetterViewModel.Hra,
                    Ita = createOfferLetterWithSlabViewModel.OfferLetterViewModel.DesignationId,
                    Joid = createOfferLetterWithSlabViewModel.OfferLetterViewModel.JobId,
                    JoiningDate = createOfferLetterWithSlabViewModel.OfferLetterViewModel.JoiningDate,
                    NetSalary = createOfferLetterWithSlabViewModel.OfferLetterViewModel.NetSalary,
                    Otbonus = createOfferLetterWithSlabViewModel.OfferLetterViewModel.Otbonus,
                    ProcessUnitId = createOfferLetterWithSlabViewModel.OfferLetterViewModel.ProcessUnitId,
                    Sickness = createOfferLetterWithSlabViewModel.OfferLetterViewModel.Sickness,
                    SpecId = createOfferLetterWithSlabViewModel.OfferLetterViewModel.SpecId,
                    SignatureAuthority = createOfferLetterWithSlabViewModel.OfferLetterViewModel.SignatureAuthority,
                    EmployeeType = createOfferLetterWithSlabViewModel.OfferLetterViewModel.EmployeeType,
                    CompanyId = createOfferLetterWithSlabViewModel.OfferLetterViewModel.LocationId // will reffer this as location Id 
                   
                };

                dbContext.PhJobOfferLetters.Add(phJobOfferLetters);
                await dbContext.SaveChangesAsync();


                foreach (var item in createOfferLetterWithSlabViewModel.OfferLetterAllowanceViewModel)
                {
                    var phJobOfferAllowances = new PhJobOfferAllowance
                    {
                        AllowanceTitle = item.AllowanceTitle,
                        Status = (byte)RecordStatus.Active,
                        Amount = item.Amount,
                        CandProfId = createOfferLetterWithSlabViewModel.OfferLetterViewModel.CandProfId,
                        CreatedBy = UserId,
                        CreatedDate = CurrentTime,
                        JobOfferId = phJobOfferLetters.Id,
                        Joid = createOfferLetterWithSlabViewModel.OfferLetterViewModel.JobId
                    };
                    dbContext.PhJobOfferAllowances.Add(phJobOfferAllowances);
                    await dbContext.SaveChangesAsync();
                }


                var downloadOfferLetter = await DownloadOfferLetter(phJobOfferLetters.Id, true);
                if (downloadOfferLetter.Status)
                {
                    var result = downloadOfferLetter.Result;
                    if (result != null)
                    {
                        phJobOfferLetters.FileUrl = result.FileURL;
                        phJobOfferLetters.FileType = result.FileType;
                        phJobOfferLetters.FileName = result.FileName;

                        var fileUpload = new PhCandidateDoc()
                        {
                            DocType = "OfferLetter",
                            FileGroup = (byte)FileGroup.Other,
                            FileName = result.FileName,
                            DocStatus = (byte)DocStatus.Accepted,
                            Joid = createOfferLetterWithSlabViewModel.OfferLetterViewModel.JobId,
                            CandProfId = createOfferLetterWithSlabViewModel.OfferLetterViewModel.CandProfId,
                            FileType = result.FileType,
                            CreatedBy = UserId
                        };

                        dbContext.PhCandidateDocs.Add(fileUpload);
                        await dbContext.SaveChangesAsync();
                    }
                }

                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                var auditLog = new CreateAuditViewModel
                {
                    ActivitySubject = "Created Offer letter",
                    ActivityDesc = " Offer letter is created successfully",
                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                    TaskID = phJobOfferLetters.Id,
                    UserId = UserId
                };
                audList.Add(auditLog);
                SaveAuditLog(audList);

                respModel.Status = true;
                respModel.SetResult("Created Successfully");

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(createOfferLetterWithSlabViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<List<JobOfferLetterViewModel>>> GetJobOfferLetter(int JobId, int CanProfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<JobOfferLetterViewModel>>();
            int UserId = Usr.Id;
            try
            {
                
                var jobOfferLetterViewModels = new List<JobOfferLetterViewModel>();

                jobOfferLetterViewModels = await (from letter in dbContext.PhJobOfferLetters
                                                  join cand in dbContext.PhCandidateBgvDetails on letter.CandProfId equals cand.CandProfId
                                                  join curency in dbContext.PhRefMasters on letter.CurrencyId equals curency.Id
                                                  where letter.CandProfId == CanProfId && letter.Joid == JobId && letter.Status != (byte)RecordStatus.Delete && curency.GroupId == 13
                                                  select new JobOfferLetterViewModel
                                                  {
                                                      Id = letter.Id,
                                                      CandName = cand.FirstName + " "
                                                      + cand.MiddleName + " " + cand.LastName,
                                                      BasicSalary = letter.BasicSalary,
                                                      CandProfId = letter.CandProfId,
                                                      Conveyance = letter.Conveyance,
                                                      CreatedBy = letter.CreatedBy,
                                                      CreatedByName = dbContext.PiHireUsers.Where(x => x.Id == letter.CreatedBy).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                      CreatedDate = letter.CreatedDate,
                                                      CurrencyId = letter.CurrencyId,
                                                      CurrencyName = curency.Rmvalue,
                                                      DepartmentId = letter.DepartmentId,
                                                      DesignationId = letter.DesignationId,
                                                      Gratuity = letter.Gratuity,
                                                      GrossSalary = letter.GrossSalary,
                                                      GrossSalaryPerAnnum = letter.GrossSalaryPerAnnum,
                                                      Hra = letter.Hra,
                                                      Ita = letter.Ita,
                                                      JobId = letter.Joid,
                                                      JoiningDate = letter.JoiningDate,
                                                      NetSalary = letter.NetSalary,
                                                      Otbonus = letter.Otbonus,
                                                      ProcessUnitId = letter.ProcessUnitId,
                                                      Sickness = letter.Sickness,
                                                      SpecId = letter.SpecId,
                                                      EmployeeType = letter.EmployeeType
                                                  }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                foreach (var item in jobOfferLetterViewModels)
                {
                    if (item.EmployeeType > 0)
                    {
                        item.EmployeeTypeName = dbContext.PhRefMasters.Where(x => x.Id == item.EmployeeType).Select(x => x.Rmvalue).FirstOrDefault();
                    }
                }

                respModel.SetResult(jobOfferLetterViewModels);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        
        public async Task<GetResponseViewModel<FileURLViewModel>> DownloadOfferLetter(int offerId, bool Islogo)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<FileURLViewModel>();
            var fileDownloadViewModel = new FileDownloadViewModel();
            var offerletterURLViewModel = new FileURLViewModel();
            int UserId = Usr.Id;

            try
            {               

                var offerDtls = dbContext.PhJobOfferLetters.Where(x => x.Id == offerId).FirstOrDefault();
                var msgDtls = await GetOfferletterMessageBody(offerId, offerDtls, Islogo);
                if (!string.IsNullOrEmpty(msgDtls.Item1))
                {
                    string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + offerDtls.CandProfId + "\\temp";

                    string fileName = offerDtls.Joid + "_" + offerId + "_" + msgDtls.Item2 + ".pdf";
                    fileName = fileName.Replace(" ", "_");

                    string fileLocation = System.IO.Path.Combine(webRootPath, fileName);
                    if (!Directory.Exists(webRootPath))
                    {
                        Directory.CreateDirectory(webRootPath);
                        FileStream file1 = File.Create(fileLocation);
                        file1.Write(fileDownloadViewModel.File, 0, fileDownloadViewModel.File.Length);
                        file1.Close();
                    }

                    offerletterURLViewModel.FileURL = _environment.ContentRootPath + "/Candidate/" + offerDtls.CandProfId + "/" + fileName;
                    offerletterURLViewModel.FileName = fileName;
                    offerletterURLViewModel.FileType = "application/pdf";


                    string imageUrl = msgDtls.Item6;
                    if (string.IsNullOrEmpty(imageUrl) || imageUrl == "false")
                    {
                        imageUrl = _environment.ContentRootPath + "\\TemplateGallery\\" + "p-logo.png";
                    }
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (var writer = new iText.Kernel.Pdf.PdfWriter(fileLocation))
                        {
                            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                            {
                                ConverterProperties converterProperties = new ConverterProperties();
                                converterProperties.SetCharset(Encoding.UTF8.WebName);
                                HtmlConverter.ConvertToPdf(msgDtls.Item1, pdf, converterProperties);

                                PdfDocument pdfDocument = new PdfDocument(new PdfReader(fileLocation), new PdfWriter(offerletterURLViewModel.FileURL));
                                Document doc = new Document(pdfDocument, PageSize.A4);

                                if (Islogo && !string.IsNullOrEmpty(imageUrl))
                                {
                                    // Create an event handler to add the logo to the header
                                    pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new CustomEventHandler(imageUrl));
                                }

                                PdfFont font = PdfFontFactory.CreateFont("Helvetica");
                                int numberOfPages = pdfDocument.GetNumberOfPages();
                                for (int i = 1; i <= numberOfPages; i++)
                                {
                                    if (!string.IsNullOrEmpty(msgDtls.Item4))
                                    {
                                        doc.ShowTextAligned(new Paragraph(msgDtls.Item4).SetFont(font).SetFontSize(9),
                                     300, 45, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
                                    }

                                    if (!string.IsNullOrEmpty(msgDtls.Item5))
                                    {
                                        doc.ShowTextAligned(new Paragraph(msgDtls.Item5).SetFont(font).SetFontSize(9),
                                     300, 30, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
                                    }
                                    doc.ShowTextAligned(new iText.Layout.Element.Paragraph("Page " + i + " of " + numberOfPages).SetFont(font).SetFontSize(9),
                                        547, 10, i, TextAlignment.RIGHT, VerticalAlignment.BOTTOM, 0);

                                }

                                doc.Close();
                            }

                            fileDownloadViewModel.File = memoryStream.ToArray();
                        }
                    }

                    if (fileDownloadViewModel.FileName != null && fileDownloadViewModel.FileName.Length > 0 && fileDownloadViewModel.File != null)
                    {
                        // Checking for folder is available or not 
                        if (!Directory.Exists(webRootPath))
                        {
                            Directory.CreateDirectory(webRootPath);
                        }
                        FileStream file = File.Create(fileLocation);

                        file.Write(fileDownloadViewModel.File, 0, fileDownloadViewModel.File.Length);

                        file.Close();
                    }

                    offerletterURLViewModel.FileURL = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + offerDtls.CandProfId + "/" + fileName;
                    respModel.SetResult(offerletterURLViewModel);
                    respModel.Status = true;

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Download offer letter",
                        ActivityDesc = " downloaded offer letter for " + msgDtls.Item2 + "",
                        ActivityType = (byte)AuditActivityType.Other,
                        TaskID = offerId,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = offerDtls.CandProfId,
                        JobId = offerDtls.Joid,
                        ActivityType = (byte)LogActivityType.Other,
                        ActivityDesc = " has download offer letter for " + msgDtls.Item2 + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                }
                else
                {
                    string message = "Candidate BGV Details are not available or not fully submitted";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public class CustomEventHandler : IEventHandler
        {
            private string logoPath;

            public CustomEventHandler(string logoPath)
            {
                this.logoPath = logoPath;
            }

            public void HandleEvent(Event @event)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;

                // Create a Canvas at the top of each page
                PdfCanvas canvas = new PdfCanvas(
                    docEvent.GetPage().NewContentStreamBefore(),
                    docEvent.GetPage().GetResources(),
                    docEvent.GetDocument()
                );

                ImageData imageData = ImageDataFactory.Create(logoPath);

                // Load the logo image
                iText.Layout.Element.Image logo = new iText.Layout.Element.Image(imageData); // Adjust the size as needed

                logo.ScaleToFit(40, 40);
                logo.SetMargins(25, 15, 30, 60);

                // Position the logo at the top-right corner
                float logoX = docEvent.GetPage().GetPageSize().GetWidth() - 200;
                float logoY = docEvent.GetPage().GetPageSize().GetHeight() - 80;

                // Add the logo to the Canvas
                canvas.AddImageAt(imageData, logoX, logoY, false);
            }
        }



        
        public async Task<Tuple<List<NotificationPushedViewModel>, GetResponseViewModel<string>>> ReleaseIntentOffer(ReleaseIntentOfferViewModel releaseIntentOfferViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<string>();
            var fileDownloadViewModel = new FileDownloadViewModel();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            var offerletterURLViewModel = new FileURLViewModel();
            int UserId = Usr.Id;
            try
            {              

                var offerDtls = dbContext.PhJobOfferLetters.Where(x => x.Id == releaseIntentOfferViewModel.JobCandOfferId).FirstOrDefault();
                var msgDtls = await GetOfferletterMessageBody(releaseIntentOfferViewModel.JobCandOfferId, offerDtls, true);

                if (!string.IsNullOrEmpty(msgDtls.Item1))
                {

                    string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + offerDtls.CandProfId + "\\temp";

                    string fileName = offerDtls.Joid + "_" + releaseIntentOfferViewModel.JobCandOfferId + "_" + CurrentTime.ToString("yyyyMMddHHmmss") + msgDtls.Item2 + ".pdf";
                    fileName = fileName.Replace(" ", "_");

                    string fileLocation = System.IO.Path.Combine(webRootPath, fileName);
                    if (!Directory.Exists(webRootPath))
                    {
                        Directory.CreateDirectory(webRootPath);
                        FileStream file1 = File.Create(fileLocation);
                        file1.Write(fileDownloadViewModel.File, 0, fileDownloadViewModel.File.Length);
                        file1.Close();
                    }

                    offerletterURLViewModel.FileURL = _environment.ContentRootPath + "/Candidate/" + offerDtls.CandProfId + "/" + fileName;

                    offerletterURLViewModel.FileName = fileName;
                    offerletterURLViewModel.FileType = "application/pdf";

                    string imageUrl = msgDtls.Item6;
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        imageUrl = _environment.ContentRootPath + "\\TemplateGallery\\" + "p-logo.png";
                    }

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (var writer = new iText.Kernel.Pdf.PdfWriter(fileLocation))
                        {
                            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                            {
                                ConverterProperties converterProperties = new ConverterProperties();
                                converterProperties.SetCharset(Encoding.UTF8.WebName);
                                HtmlConverter.ConvertToPdf(msgDtls.Item1, pdf, converterProperties);

                                PdfDocument pdfDocument = new PdfDocument(new PdfReader(fileLocation), new PdfWriter(offerletterURLViewModel.FileURL));
                                Document doc = new Document(pdfDocument);

                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    // Create an event handler to add the logo to the header
                                    pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new CustomEventHandler(imageUrl));
                                }

                                PdfFont font = PdfFontFactory.CreateFont("Helvetica");
                                int numberOfPages = pdfDocument.GetNumberOfPages();
                                for (int i = 1; i <= numberOfPages; i++)
                                {
                                    if (!string.IsNullOrEmpty(msgDtls.Item4))
                                    {
                                        doc.ShowTextAligned(new Paragraph(msgDtls.Item4).SetFont(font).SetFontSize(9),
                                     300, 45, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
                                    }

                                    if (!string.IsNullOrEmpty(msgDtls.Item5))
                                    {
                                        doc.ShowTextAligned(new Paragraph(msgDtls.Item5).SetFont(font).SetFontSize(9),
                                     300, 30, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
                                    }
                                    doc.ShowTextAligned(new iText.Layout.Element.Paragraph("Page " + i + " of " + numberOfPages).SetFont(font).SetFontSize(9),
                                        547, 10, i, TextAlignment.RIGHT, VerticalAlignment.BOTTOM, 0);

                                }

                                doc.Close();
                            }

                            fileDownloadViewModel.File = memoryStream.ToArray();
                        }
                    }

                    if (fileDownloadViewModel.FileName != null && fileDownloadViewModel.FileName.Length > 0 && fileDownloadViewModel.File != null)
                    {
                        // Checking for folder is available or not 
                        if (!Directory.Exists(webRootPath))
                        {
                            Directory.CreateDirectory(webRootPath);
                        }
                        FileStream file = File.Create(fileLocation);
                        file.Write(fileDownloadViewModel.File, 0, fileDownloadViewModel.File.Length);
                        file.Close();
                    }

                    // Applying workflow rule 
                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    {
                        ActionMode = (byte)WorkflowActionMode.Other,
                        CanProfId = offerDtls.CandProfId,
                        JobId = offerDtls.Joid,
                        TaskCode = TaskCode.RIO.ToString(),
                        UserId = UserId,
                        IntentOfferContent = fileDownloadViewModel.File,
                        IntentOfferRemarks = releaseIntentOfferViewModel.Remarks,
                        DOJ = offerDtls.JoiningDate,
                        UserIds = releaseIntentOfferViewModel.UserIds
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
                                CreatedBy = UserId,
                                IsAudioNotify = true
                            };
                            notificationPushedViewModel.Add(notificationPushed);
                        }
                    }

                    respModel.SetResult("Successfully sent");
                    respModel.Status = true;

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();

                    // Activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = offerDtls.CandProfId,
                        JobId = offerDtls.Joid,
                        ActivityType = (byte)LogActivityType.Other,
                        ActivityDesc = " has sent intent offer letter to " + msgDtls.Item2 + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                }
                else
                {
                    string message = "Candidate BGV Details are not available or not fully submitted";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + JsonConvert.SerializeObject(releaseIntentOfferViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


       
        //public async Task<HtmlMessageBodyViewModel> DownloadIntentLetter(int offerId, bool isLogo)
        //{
        //    logger.SetMethodName(MethodBase.GetCurrentMethod());
        //    var htmlMessageBodyViewModel = new HtmlMessageBodyViewModel();
        //    try
        //    {
        //        var offerDtls = dbContext.PhJobOfferLetters.Where(x => x.Id == offerId).FirstOrDefault();
        //        var msgDtls = await GetOfferletterMessageBody(offerId, offerDtls, isLogo);

        //        htmlMessageBodyViewModel.OfferHtml = msgDtls?.Item1;
        //        htmlMessageBodyViewModel.Address1 = msgDtls?.Item4;
        //        htmlMessageBodyViewModel.Address2 = msgDtls?.Item5;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + JsonConvert.SerializeObject(htmlMessageBodyViewModel), ex);
        //    }
        //    return htmlMessageBodyViewModel;
        //}

        
        public async Task<Tuple<string, string, PuLocationsModel, string, string, string>> GetOfferletterMessageBody(int OfferId, PhJobOfferLetter offerDtls, bool Islogo)
        {
            DateTime dt = CurrentTime;
            var tDay = dt.Day;
            var tMonth = dt.Month;
            var tYear = dt.Year;
            string Title = string.Empty;
            string CandName = string.Empty;
            string messageBody = string.Empty;
            string companyName = string.Empty;
            string shortName = "ParamInfo";
            string address1 = string.Empty;
            string address2 = string.Empty;
            string logo = string.Empty;

            var puLocationsModel = new PuLocationsModel();
            var canDtls = dbContext.PhCandidateBgvDetails.Where(x => x.CandProfId == offerDtls.CandProfId && x.BgcompStatus == (byte)(BGCompStatus.Completed)).FirstOrDefault();
            if (canDtls != null)
            {
                CandName = canDtls.FirstName + " " + canDtls.MiddleName + " " + canDtls.LastName;
                string cityName = canDtls.PermAddrCityId != null ? dbContext.PhCities.Where(b => b.Id == canDtls.PermAddrCityId).Select(x => x.Name).FirstOrDefault() : string.Empty;
                string countryName = canDtls.PermAddrCountryId != null ? dbContext.PhCountries.Where(b => b.Id == canDtls.PermAddrCountryId).Select(x => x.Nicename).FirstOrDefault() : string.Empty;
                //pending
                //if (canDtls.Gender == "M")
                //{
                //    Title = "Mr.";
                //}
                //else if (canDtls.Gender == "F")
                //{
                //    Title = "Ms.";
                //}
                var signatureDtls = dbContext.PiHireUsers.Where(x => x.Id == offerDtls.SignatureAuthority).FirstOrDefault();
                DateTime? joiningDate = offerDtls.JoiningDate;
                var joiningDay = joiningDate.Value.Day;
                var joiningMonth = joiningDate.Value.Month;
                var joiningYear = joiningDate.Value.Year;

                var refDtls = dbContext.PhRefMasters.ToList();
                var designation = offerDtls.DesignationId != null ? refDtls.Where(x => x.Id == offerDtls.DesignationId).Select(x => x.Rmvalue).FirstOrDefault() : string.Empty;
                if (offerDtls.CompanyId != null)
                {
                    var dtls = await dbContext.GetCompanyLocation(offerDtls.CompanyId.Value);
                    if (dtls.Count > 0)
                    {
                        var lctnDtls = dtls[0];
                        if (lctnDtls != null)
                        {
                            address1 = lctnDtls.address1 + ", " + lctnDtls.address2 + ", " + lctnDtls.address3 + ", " + lctnDtls.city_name + ", " + lctnDtls.country + ".";
                            address2 = " " + lctnDtls.mobile_number + " " + lctnDtls.website;
                            logo = lctnDtls.PuLogo;
                        }
                    }
                }
                if (offerDtls.ProcessUnitId != null)
                {
                    var dtls = await dbContext.GetPUs();
                    if (dtls.Count > 0)
                    {
                        var puDtls = dtls.Where(x => x.Id == offerDtls.ProcessUnitId).FirstOrDefault();
                        if (puDtls != null)
                        {
                            companyName = puDtls.Name;
                            if (offerDtls.ProcessUnitId == 596)
                            {
                                shortName = puDtls.ShortName;
                                logo = puDtls.logo;
                            }
                        }
                    }
                }
                var currency = refDtls.Where(x => x.Id == offerDtls.CurrencyId).Select(x => x.Rmvalue).FirstOrDefault();
                var specialization = offerDtls.SpecId != null ? refDtls.Where(x => x.Id == offerDtls.SpecId).Select(x => x.Rmvalue).FirstOrDefault() : string.Empty;
                var jobAllowsDetails = (from jobAllows in dbContext.PhJobOfferAllowances
                                        where jobAllows.JobOfferId == OfferId && jobAllows.Status != (byte)RecordStatus.Active
                                        select new
                                        {
                                            ID = jobAllows.Id,
                                            allow_Description = jobAllows.AllowanceTitle,
                                            amount = jobAllows.Amount
                                        }).ToList();
                if (Islogo)
                {
                    messageBody = System.IO.File.ReadAllText("EmailTemplates/OfferLetterSampleWithSlab.html");
                }
                else
                {
                    messageBody = System.IO.File.ReadAllText("EmailTemplates/OfferLetterSamplewithoutSlab.html");
                }

                messageBody = messageBody.Replace("!todayDay", GetDaySuffix(tDay));
                messageBody = messageBody.Replace("!Specilization", specialization);
                messageBody = messageBody.Replace("!todayMonth", GetMonthName(tMonth));
                messageBody = messageBody.Replace("!todayYear", tYear.ToString());
                messageBody = messageBody.Replace("!joiningDay", GetDaySuffix(joiningDay));
                messageBody = messageBody.Replace("!joiningMonth", GetMonthName(joiningMonth));
                messageBody = messageBody.Replace("!joiningYear", joiningYear.ToString());
                messageBody = messageBody.Replace("!ComnyShrtName", shortName);
                messageBody = messageBody.Replace("!CandName", CandName);
                messageBody = messageBody.Replace("!Position", designation);
                messageBody = messageBody.Replace("!GrossTotal", string.Format("{0:n0}", (offerDtls.GrossSalaryPerAnnum)));
                messageBody = messageBody.Replace("!GrossWords", NumbersToWords((offerDtls.GrossSalaryPerAnnum) ?? default(int)));
                messageBody = messageBody.Replace("!Currency", currency);
                messageBody = messageBody.Replace("!mBasic", string.Format("{0:n0}", offerDtls.BasicSalary));
                messageBody = messageBody.Replace("!aBasic", string.Format("{0:n0}", (offerDtls.BasicSalary * 12)));
                messageBody = messageBody.Replace("!City", cityName ?? " ");
                messageBody = messageBody.Replace("!Country", countryName ?? " ");
                messageBody = messageBody.Replace("!Title", Title);

                messageBody = messageBody.Replace("!Comapny", companyName);
                messageBody = messageBody.Replace("!ownerName", signatureDtls?.FirstName + " " + signatureDtls?.LastName);
                messageBody = messageBody.Replace("!ownerDesignation", signatureDtls?.UserRoleName);


                messageBody = messageBody.Replace("!mHRA", string.Format("{0:n0}", (offerDtls.Hra)) ?? "NA");
                messageBody = messageBody.Replace("!aHRA", string.Format("{0:n0}", (offerDtls.Hra * 12)) ?? "NA");

                messageBody = messageBody.Replace("!mConveyance", string.Format("{0:n0}", (offerDtls.Conveyance)) ?? "NA");
                messageBody = messageBody.Replace("!aConveyance", string.Format("{0:n0}", (offerDtls.Conveyance * 12)) ?? "NA");

                messageBody = messageBody.Replace("!mOverTimeBonus", string.Format("{0:n0}", (offerDtls.Otbonus)) ?? "NA");
                messageBody = messageBody.Replace("!aOverTimeBonus", string.Format("{0:n0}", (offerDtls.Otbonus * 12)) ?? "NA");

                messageBody = messageBody.Replace("!mGSBonus", string.Format("{0:n0}", (offerDtls.Gratuity)) ?? "NA");
                messageBody = messageBody.Replace("!aGSBonus", string.Format("{0:n0}", (offerDtls.Gratuity * 12)) ?? "NA");

                messageBody = messageBody.Replace("!mTotalNet", string.Format("{0:n0}", (offerDtls.NetSalary)) ?? "NA");
                messageBody = messageBody.Replace("!aTotalNet", string.Format("{0:n0}", (offerDtls.NetSalary * 12)) ?? "NA");

                messageBody = messageBody.Replace("!mSicknessAllowance", string.Format("{0:n0}", (offerDtls.Sickness)) ?? "NA");
                messageBody = messageBody.Replace("!aSicknessAllowance", string.Format("{0:n0}", (offerDtls.Sickness * 12)) ?? "NA");

                if (jobAllowsDetails.Count > 0)
                {
                    string jobAllowdiv = string.Empty;
                    for (int j = 0; j < jobAllowsDetails.Count; j++)
                    {
                        jobAllowdiv = jobAllowdiv + " " +
                           "<tr><td style='height: 20px;padding-left: 7px;font-size: 12px;font-weight:normal;'>" + jobAllowsDetails[j].allow_Description +
                          "</td><td style='height: 20px; text-align:right; padding-left: 7px;font-size: 12px;font-weight:normal;'>" + string.Format("{0:n0}", "") +
                          "</td><td style='text-align:right; height: 20px;padding-left: 7px;font-size: 12px;font-weight:normal;'>" + string.Format("{0:n0}", jobAllowsDetails[j].amount * 1) +
                          "</td></tr>";
                    }
                    messageBody = messageBody.Replace("!allowancecomponent", jobAllowdiv ?? "NA");
                }
                else
                {
                    messageBody = messageBody.Replace("!mtotalGross", string.Format("{0:n0}", (offerDtls.GrossSalary)) ?? "NA");
                    messageBody = messageBody.Replace("!aTotalGross", string.Format("{0:n0}", (offerDtls.GrossSalaryPerAnnum)) ?? "NA");

                    var defaultValues = dbContext.PhJobOfferSlabDetails.Where(x => x.JobOfferId == OfferId && x.ComponentId == 0 && x.SlabId == 0 && x.Status != (byte)RecordStatus.Delete).Select(x => new { x.Id, x.Amount }).ToArray();


                    List<DownloadSlabComponentDtlsViewModel> offerValues = (from stus in dbContext.PhJobOfferSlabDetails
                                                                            join comp in dbContext.PhSalaryComps on stus.ComponentId equals comp.Id
                                                                            join refData in dbContext.PhRefMasters on comp.CompType equals refData.Id
                                                                            join slabs in dbContext.PhSalarySlabsSes on stus.SlabId equals slabs.Id
                                                                            where stus.JobOfferId == OfferId && stus.Status != (byte)RecordStatus.Delete
                                                                            select new DownloadSlabComponentDtlsViewModel
                                                                            {
                                                                                Id = stus.Id,
                                                                                Amount = stus.Amount,
                                                                                CompName = comp.Title,
                                                                                CompTypeName = refData.Rmvalue,
                                                                                SlabName = slabs.Title
                                                                            }).ToList();
                    foreach (var item in defaultValues)
                    {
                        int chkIndex = item.Id - 1;
                        var set_dtls = dbContext.PhJobOfferSlabDetails.Where(x => x.Id == chkIndex).FirstOrDefault();
                        if (set_dtls != null)
                        {
                            var dtls = dbContext.PhSalaryComps.Where(x => x.Id == set_dtls.ComponentId).FirstOrDefault();
                            if (dtls != null)
                            {
                                var dtls1 = dbContext.PhRefMasters.Where(x => x.Id == dtls.CompType).FirstOrDefault();
                                if (dtls1 != null)
                                {
                                    var dtls2 = dbContext.PhSalarySlabsSes.Where(x => x.Id == set_dtls.SlabId).FirstOrDefault();
                                    if (dtls2 != null)
                                    {
                                        if (dtls1.Rmvalue.ToUpper().Contains("SECTION C"))
                                        {
                                            offerValues.Add(new DownloadSlabComponentDtlsViewModel
                                            {
                                                Amount = item.Amount,
                                                CompName = "ESIC",
                                                CompTypeName = dtls1.Rmvalue,
                                                SlabName = dtls2.Title
                                            });
                                        }
                                        else
                                        {
                                            offerValues.Add(new DownloadSlabComponentDtlsViewModel
                                            {
                                                Amount = item.Amount,
                                                CompName = "Special Allowance",
                                                CompTypeName = dtls1.Rmvalue,
                                                SlabName = dtls2.Title
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }


                    List<DownloadGrpBySlabModel> offerComponentsGrpBy = offerValues.GroupBy(x => x.CompTypeName).Select(grp => new DownloadGrpBySlabModel { CompTypeName = grp.Key, slabComponentDtlsViewModels = grp.ToList() }).ToList();

                    string Additions = string.Empty;
                    var snoParticulars = 0;
                    var snoCompType = 0;
                    foreach (var item in offerComponentsGrpBy)
                    {
                        snoCompType = snoCompType + 1;
                        Additions = Additions + "" + "<tr><td colspan='2' style='height: 20px;padding-left: 7px;font-size: 14px;font-weight:bold;'>" + item.CompTypeName +
                         "</td><td style='height: 20px; text-align:right; padding-left: 7px;font-size: 12px;font-weight:normal;'>" +
                         "</td><td style='text-align:right; height: 20px;padding-left: 7px;font-size: 12px;font-weight:normal;'>" +
                         "</td></tr>";
                        decimal subTotal = 0;
                        foreach (var itemSub in item.slabComponentDtlsViewModels)
                        {
                            snoParticulars = snoParticulars + 1;
                            Additions = Additions + " " +
                       "<tr><td style='height: 20px;padding-left: 7px;font-size: 12px;font-weight:normal;width:20px;'>" + snoParticulars +
                       "</td><td style='height: 20px;padding-left: 7px;font-size: 12px;font-weight:normal;'>" + itemSub.CompName +
                       "</td><td style='height: 20px; text-align:right; padding-left: 7px;font-size: 12px;font-weight:normal;'>" + string.Format("{0:n0}", itemSub.Amount) +
                       "</td><td style='text-align:right; height: 20px;padding-left: 7px;font-size: 12px;font-weight:normal;'>" + string.Format("{0:n0}", itemSub.Amount * 12) +
                       "</td></tr>";

                            subTotal = subTotal + itemSub.Amount;
                        }

                        string Subtotal = string.Empty;
                        string ccmnpntName = item.CompTypeName;
                        if (ccmnpntName.Contains("Section"))
                        {
                            Subtotal = "Total";
                        }
                        else
                        {
                            if (snoCompType == 1)
                            {
                                Subtotal = "Monthly Net Salary";
                            }
                            else
                            {
                                Subtotal = "Total";
                            }
                        }
                        Additions = Additions + " " +
                     "<tr><td align='right' colspan='2' style='height: 20px;padding-left: 7px;font-size: 12px;font-weight:normal;'>" + Subtotal +
                       "</td><td style='height: 20px; text-align:right; padding-left: 7px;font-size: 12px;font-weight:normal;'>" + string.Format("{0:n0}", subTotal) +
                       "</td><td style='text-align:right; height: 20px;padding-left: 7px;font-size: 12px;font-weight:normal;'>" + string.Format("{0:n0}", subTotal * 12) +
                       "</td></tr>";
                    }

                    string totalGross = string.Empty;
                    messageBody = messageBody.Replace("!offerDetailsAdditions", Additions ?? "NA");

                    totalGross = totalGross + " " +
                  "<tr><td colspan='2'  style='text-align:right;height: 20px;padding-left: 7px;font-size: 12px;font-weight:bold;'>" + " Annual CTC " +
                    "</td><td style='height: 20px; text-align:right; padding-left: 7px;font-size: 12px;font-weight:normal;'>" + string.Format("{0:n0}", (offerDtls.GrossSalary)) +
                    "</td><td style='text-align:right; height: 20px;padding-left: 7px;font-size: 12px;font-weight:normal;'>" + string.Format("{0:n0}", (offerDtls.GrossSalaryPerAnnum)) +
                    "</td></tr>";

                    messageBody = messageBody.Replace("!offerDetailsGross", totalGross ?? "NA");

                    messageBody = messageBody.Replace("!mtotalGross", string.Format("{0:n0}", (offerDtls.GrossSalary)) ?? "NA");
                    messageBody = messageBody.Replace("!aTotalGross", string.Format("{0:n0}", (offerDtls.GrossSalaryPerAnnum)) ?? "NA");

                }
            }
            return Tuple.Create(messageBody, CandName, puLocationsModel, address1, address2, logo);
        }

        #endregion

        #region  OFFERS (PRE JOINS + ON BOARDS)

        public async Task<GetResponseViewModel<OfferedCandidatesModel>> GetOfferdCandidates(OfferdCandidateSearchModel offerdCandidateSearchModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<OfferedCandidatesModel>();
            int UserId = Usr.Id;
            try
            {
                
                var offeredCandidatesModel = new OfferedCandidatesModel();

                offerdCandidateSearchModel.CurrentPage = (offerdCandidateSearchModel.CurrentPage.Value - 1) * offerdCandidateSearchModel.PerPage.Value;

                offeredCandidatesModel = await dbContext.GetOfferdCandidatesList(offerdCandidateSearchModel.SearchKey, offerdCandidateSearchModel.PerPage, offerdCandidateSearchModel.CurrentPage);

                respModel.SetResult(offeredCandidatesModel);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(offerdCandidateSearchModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        #endregion

    }
}
