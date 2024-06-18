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
    public class BlogsRepository : BaseRepository, IBlogsRepository
    {
        readonly Logger logger;
        private readonly IWebHostEnvironment _environment;
        public BlogsRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<BlogsRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<GetResponseViewModel<List<BlogsModel>>> GetActiveBlogs()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<BlogsModel>>();
            try
            {
                

                var Blogs = await (from mt in dbContext.PhBlogs
                                   where mt.Status == (byte)RecordStatus.Active
                                   select new BlogsModel
                                   {
                                       AuthorName = mt.AuthorName,
                                       BlogDesc = mt.BlogDesc,
                                       BlogPic = mt.BlogPic,
                                       CreatedDate = mt.CreatedDate,
                                       BlogShortDesc = mt.BlogShortDesc,
                                       Title = mt.Title,
                                       Status = mt.Status,
                                       Tags = mt.Tags,
                                       Id = mt.Id
                                   }).ToListAsync();
                foreach (var tem in Blogs)
                {
                    if (!string.IsNullOrEmpty(tem.BlogPic))
                    {
                        tem.BlogPic = this.appSettings.AppSettingsProperties.HireApiUrl + "/Blogs/" + tem.BlogPic;
                    }
                }

                respModel.SetResult(Blogs);
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

        public async Task<GetResponseViewModel<List<BlogsModel>>> BlogsList()
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<BlogsModel>>();
            int UserId = Usr.Id;
            try
            {
                

                var Blogs = await (from mt in dbContext.PhBlogs
                                   where mt.Status != (byte)RecordStatus.Delete
                                   select new BlogsModel
                                   {
                                       AuthorName = mt.AuthorName,
                                       BlogShortDesc = mt.BlogShortDesc,
                                       BlogDesc = mt.BlogDesc,
                                       BlogPic = mt.BlogPic,
                                       CreatedDate = mt.CreatedDate,
                                       Title = mt.Title,
                                       Status = mt.Status,
                                       Tags = mt.Tags,
                                       Id = mt.Id
                                   }).ToListAsync();
                foreach (var tem in Blogs)
                {
                    if (!string.IsNullOrEmpty(tem.BlogPic))
                    {
                        tem.BlogPic = this.appSettings.AppSettingsProperties.HireApiUrl + "/Blogs/" + tem.BlogPic;
                    }
                }

                respModel.SetResult(Blogs);
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

        public async Task<GetResponseViewModel<BlogModel>> GetBlog(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<BlogModel>();
            try
            {
                {
                    var Blog = await (from mt in dbContext.PhBlogs
                                      where mt.Id == Id
                                      select new BlogModel
                                      {
                                          Id = mt.Id,
                                          AuthorName = mt.AuthorName,
                                          BlogShortDesc = mt.BlogShortDesc,
                                          BlogDesc = mt.BlogDesc,
                                          BlogPic = mt.BlogPic,
                                          CreatedDate = mt.CreatedDate,
                                          Title = mt.Title,
                                          Status = mt.Status,
                                          Tags = mt.Tags
                                      }).FirstOrDefaultAsync();
                    if (Blog != null)
                    {
                        if (!string.IsNullOrEmpty(Blog.BlogPic))
                        {
                            Blog.BlogPic = this.appSettings.AppSettingsProperties.HireApiUrl + "/Blogs/" + Blog.BlogPic;
                        }
                    }

                    respModel.SetResult(Blog);
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

        public async Task<CreateResponseViewModel<string>> CreateBlog(CreateBlogModel createBlogModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string message = "Saved Successfully";
            try
            {
                

                string fileName = string.Empty;
                string webRootPath = string.Empty;

                if (createBlogModel.File != null)
                {
                    if (createBlogModel.File.Length > 0)
                    {
                        webRootPath = _environment.ContentRootPath + "\\Blogs";

                        // Checking for folder is available or not 
                        if (!Directory.Exists(webRootPath))
                        {
                            Directory.CreateDirectory(webRootPath);
                        }

                        fileName = Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + createBlogModel.File.FileName);

                        fileName = fileName.Replace(" ", "_");
                        if (fileName.Length > 100)
                        {
                            fileName = fileName.Substring(0, 99);
                        }
                        var filePath = Path.Combine(webRootPath, string.Empty, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await createBlogModel.File.CopyToAsync(fileStream);
                        }
                    }
                }

                var phBlogs = new PhBlog
                {
                    CreatedBy = UserId,
                    CreatedDate = CurrentTime,
                    Status = (byte)RecordStatus.Active,
                    AuthorName = createBlogModel.AuthorName,
                    BlogPic = fileName,
                    BlogShortDesc = createBlogModel.BlogShortDesc,
                    BlogDesc = createBlogModel.BlogDesc,
                    Tags = createBlogModel.Tags,
                    Title = createBlogModel.Title
                };

                dbContext.PhBlogs.Add(phBlogs);
                await dbContext.SaveChangesAsync();

                message = "Saved Successfully";
                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                var auditLog = new CreateAuditViewModel
                {
                    ActivitySubject = "Created New Blog",
                    ActivityDesc = "Created New Blog",
                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                    TaskID = phBlogs.Id,
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

        public async Task<UpdateResponseViewModel<string>> UpdateBlog(UpdateBlogModel updateBlogModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Saved Successfully";
            try
            {
                


                var phBlogs = await dbContext.PhBlogs.Where(x => x.Id == updateBlogModel.Id).FirstOrDefaultAsync();
                if (phBlogs != null)
                {
                    phBlogs.UpdatedBy = UserId;
                    phBlogs.UpdatedDate = CurrentTime;
                    phBlogs.Tags = updateBlogModel.Tags;
                    phBlogs.AuthorName = updateBlogModel.AuthorName;
                    phBlogs.Title = updateBlogModel.Title;
                    phBlogs.BlogShortDesc = updateBlogModel.BlogShortDesc;
                    phBlogs.BlogDesc = updateBlogModel.BlogDesc;

                    string fileName = string.Empty;

                    string webRootPath = string.Empty;

                    webRootPath = _environment.ContentRootPath + "\\Blogs";

                    if (!string.IsNullOrEmpty(phBlogs.BlogPic))
                    {
                        var isFile = webRootPath + "\\" + phBlogs.BlogPic;
                        if ((System.IO.File.Exists(isFile)))
                        {
                            System.IO.File.Delete(isFile);
                        }
                    }

                    if (updateBlogModel.File != null)
                    {
                        if (updateBlogModel.File.Length > 0)
                        {
                            // Checking for folder is available or not 
                            if (!Directory.Exists(webRootPath))
                            {
                                Directory.CreateDirectory(webRootPath);
                            }

                            fileName = Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + updateBlogModel.File.FileName);

                            fileName = fileName.Replace(" ", "_");
                            if (fileName.Length > 100)
                            {
                                fileName = fileName.Substring(0, 99);
                            }
                            var filePath = Path.Combine(webRootPath, string.Empty, fileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await updateBlogModel.File.CopyToAsync(fileStream);
                            }

                            phBlogs.BlogPic = fileName;
                        }

                    }

                    dbContext.PhBlogs.Update(phBlogs);
                    await dbContext.SaveChangesAsync();

                    message = "Updated Successfully";
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Updated Blog",
                        ActivityDesc = " Blog details is Updated successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = phBlogs.Id,
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
                    message = "The Blog is not available";
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

        public async Task<UpdateResponseViewModel<string>> UpdateBlogStatus(UpdateStatusModel updateStatusModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Status Updated Successfully";
            int UserId = Usr.Id;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var phBlogs = dbContext.PhBlogs.Where(x => x.Id == updateStatusModel.Id).FirstOrDefault();
                if (phBlogs != null)
                {
                    phBlogs.UpdatedBy = UserId;
                    phBlogs.UpdatedDate = CurrentTime;
                    phBlogs.Status = (byte)updateStatusModel.Status;

                    dbContext.PhBlogs.Update(phBlogs);
                    await dbContext.SaveChangesAsync();

                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Updated Blog",
                        ActivityDesc = "Blog status is Updated successfully",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = phBlogs.Id,
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
                    message = "The Blog is not available";
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

    }
}
