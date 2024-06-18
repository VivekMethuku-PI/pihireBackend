using Microsoft.AspNetCore.Hosting;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class TestimonialRepository : BaseRepository, ITestimonialRepository
    {
        readonly Logger logger;
        
        private readonly IWebHostEnvironment _environment;

        
        public TestimonialRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<TestimonialRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<GetResponseViewModel<List<TestimonialsModel>>> GetActiveTestimonials()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<TestimonialsModel>>();
            try
            {
                

                var Testimonials = await (from mt in dbContext.PhTestimonials
                                          where mt.Status == (byte)RecordStatus.Active
                                          select new TestimonialsModel
                                          {
                                              Id = mt.Id,
                                              Status = mt.Status,
                                              CandidateId = mt.CandidateId,
                                              CreatedDate = mt.CreatedDate,
                                              Rating = mt.Rating,
                                              Tdesc = mt.Tdesc,
                                              Title = mt.Title,
                                              ProfilePic = mt.ProfilePic,
                                              Designation = mt.Designation
                                          }).ToListAsync();
                foreach (var tem in Testimonials)
                {
                    if (!string.IsNullOrEmpty(tem.ProfilePic))
                    {
                        tem.ProfilePic = this.appSettings.AppSettingsProperties.HireApiUrl + "/Testimonals/" + tem.ProfilePic;
                    }
                }

                respModel.SetResult(Testimonials);
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

        public async Task<GetResponseViewModel<List<TestimonialsModel>>> TestimonialsList()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<TestimonialsModel>>();
            int UserId = Usr.Id;
            try
            {
                

                var Testimonials = await (from mt in dbContext.PhTestimonials
                                          where mt.Status != (byte)RecordStatus.Delete
                                          select new TestimonialsModel
                                          {
                                              Id = mt.Id,
                                              Status = mt.Status,
                                              CandidateId = mt.CandidateId,
                                              CreatedDate = mt.CreatedDate,
                                              Rating = mt.Rating,
                                              Tdesc = mt.Tdesc,
                                              Title = mt.Title,
                                              ProfilePic = mt.ProfilePic,
                                              Designation = mt.Designation
                                          }).ToListAsync();
                foreach (var tem in Testimonials)
                {
                    if (!string.IsNullOrEmpty(tem.ProfilePic))
                    {
                        tem.ProfilePic = this.appSettings.AppSettingsProperties.HireApiUrl + "/Testimonals/" + tem.ProfilePic;
                    }
                }

                respModel.SetResult(Testimonials);
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

        public async Task<CreateResponseViewModel<string>> CreateTestimonial(CreateTestimonialModel createTestimonialsModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string message = "Saved Successfully";
            try
            {
                

                string fileName = string.Empty;
                string webRootPath = string.Empty;

                if (createTestimonialsModel.File != null)
                {
                    if (createTestimonialsModel.File.Length > 0)
                    {
                        webRootPath = _environment.ContentRootPath + "\\Testimonals";

                        // Checking for folder is available or not 
                        if (!Directory.Exists(webRootPath))
                        {
                            Directory.CreateDirectory(webRootPath);
                        }

                        fileName = Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + createTestimonialsModel.File.FileName);

                        fileName = fileName.Replace(" ", "_");
                        if (fileName.Length > 100)
                        {
                            fileName = fileName.Substring(0, 99);
                        }
                        var filePath = Path.Combine(webRootPath, string.Empty, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await createTestimonialsModel.File.CopyToAsync(fileStream);
                        }
                    }
                }

                var phTestimonials = new PhTestimonial
                {
                    CreatedBy = UserId,
                    CreatedDate = CurrentTime,
                    Status = (byte)RecordStatus.Active,
                    CandidateId = createTestimonialsModel.CandidateId,
                    ProfilePic = fileName,
                    Rating = createTestimonialsModel.Rating,
                    Tdesc = createTestimonialsModel.Tdesc,
                    Title = createTestimonialsModel.Title,
                    Designation = createTestimonialsModel.Designation
                };

                dbContext.PhTestimonials.Add(phTestimonials);
                await dbContext.SaveChangesAsync();

                message = "Saved Successfully";

                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                var auditLog = new CreateAuditViewModel
                {
                    ActivitySubject = "Created New Testimonal",
                    ActivityDesc = "Testimonal is Created successfully",
                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                    TaskID = phTestimonials.Id,
                    UserId = UserId
                };
                audList.Add(auditLog);
                SaveAuditLog(audList);

                respModel.SetResult(message);
                respModel.Status = true;

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

        
        public async Task<UpdateResponseViewModel<string>> UpdateTestimonial(UpdateTestimonialModel updateTestimonialsModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Saved Successfully";
            try
            {
                


                var phTestimonials = await dbContext.PhTestimonials.Where(x => x.Id == updateTestimonialsModel.Id).FirstOrDefaultAsync();
                if (phTestimonials != null)
                {
                    phTestimonials.UpdatedBy = UserId;
                    phTestimonials.UpdatedDate = CurrentTime;
                    phTestimonials.CandidateId = updateTestimonialsModel.CandidateId;
                    phTestimonials.Rating = updateTestimonialsModel.Rating;
                    phTestimonials.Title = updateTestimonialsModel.Title;
                    phTestimonials.Tdesc = updateTestimonialsModel.Tdesc;
                    phTestimonials.Designation = updateTestimonialsModel.Designation;

                    string fileName = string.Empty;

                    string webRootPath = string.Empty;

                    webRootPath = _environment.ContentRootPath + "\\Testimonals";

                    if (!string.IsNullOrEmpty(phTestimonials.ProfilePic))
                    {
                        var isFile = webRootPath + "\\" + phTestimonials.ProfilePic;
                        if ((System.IO.File.Exists(isFile)))
                        {
                            System.IO.File.Delete(isFile);
                        }
                    }

                    if (updateTestimonialsModel.File != null)
                    {
                        if (updateTestimonialsModel.File.Length > 0)
                        {
                            // Checking for folder is available or not 
                            if (!Directory.Exists(webRootPath))
                            {
                                Directory.CreateDirectory(webRootPath);
                            }

                            fileName = Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + updateTestimonialsModel.File.FileName);

                            fileName = fileName.Replace(" ", "_");
                            if (fileName.Length > 100)
                            {
                                fileName = fileName.Substring(0, 99);
                            }
                            var filePath = Path.Combine(webRootPath, string.Empty, fileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await updateTestimonialsModel.File.CopyToAsync(fileStream);
                            }

                            phTestimonials.ProfilePic = fileName;
                        }

                    }

                    dbContext.PhTestimonials.Update(phTestimonials);
                    await dbContext.SaveChangesAsync();

                    message = "Updated Successfully";
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Updated Testimonal",
                        ActivityDesc = "Testimonal details is Updated successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = phTestimonials.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.SetResult(message);
                    respModel.Status = true;

                }
                else
                {
                    respModel.Status = false;
                    message = "The Testimonial is not available";
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

        public async Task<UpdateResponseViewModel<string>> UpdateTestimonialStatus(UpdateStatusModel updateStatusModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Status Updated Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var Testimonials = dbContext.PhTestimonials.Where(x => x.Id == updateStatusModel.Id).FirstOrDefault();
                if (Testimonials != null)
                {
                    Testimonials.UpdatedBy = UserId;
                    Testimonials.UpdatedDate = CurrentTime;
                    Testimonials.Status = (byte)updateStatusModel.Status;

                    dbContext.PhTestimonials.Update(Testimonials);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Testimonial ",
                        ActivityDesc = " Updated Testimonial Status",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = Testimonials.Id,
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
                    message = "The Testimonial is not available";
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

        public async Task<GetResponseViewModel<TestimonialModel>> GetTestimonial(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<TestimonialModel>();
            try
            {
                {
                    var Testimonial = await (from mt in dbContext.PhTestimonials
                                             where mt.Id == Id
                                             select new TestimonialModel
                                             {
                                                 Id = mt.Id,
                                                 Status = mt.Status,
                                                 CandidateId = mt.CandidateId,
                                                 CreatedDate = mt.CreatedDate,
                                                 Rating = mt.Rating,
                                                 Tdesc = mt.Tdesc,
                                                 Title = mt.Title,
                                                 ProfilePic = mt.ProfilePic,
                                                 Designation = mt.Designation
                                             }).FirstOrDefaultAsync();
                    if (Testimonial != null)
                    {
                        if (!string.IsNullOrEmpty(Testimonial.ProfilePic))
                        {
                            Testimonial.ProfilePic = this.appSettings.AppSettingsProperties.HireApiUrl + "/Testimonals/" + Testimonial.ProfilePic;
                        }
                    }

                    respModel.SetResult(Testimonial);
                    respModel.Status = true;
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
    }
}
