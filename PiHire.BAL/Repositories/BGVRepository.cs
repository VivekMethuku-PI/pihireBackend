using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PiHire.BAL.Common;
using PiHire.BAL.Common.Http;
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
using System.Text;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using Microsoft.IdentityModel.Tokens;

namespace PiHire.BAL.Repositories
{
    public class BGVRepository : BaseRepository, IBGVRepository
    {
        readonly Logger logger;

        private readonly IWebHostEnvironment _environment;


        public BGVRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<BGVRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }



        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateCandidateInfo(UpdateCandidateInfoViewModel candidateInfoViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {

                    var candidateProfile = await dbContext.PhCandidateProfiles.Where(x => x.Id == candidateInfoViewModel.CanPrfId).FirstOrDefaultAsync();
                    if (candidateProfile != null)
                    {
                        candidateProfile.CountryId = candidateInfoViewModel.CountryId;
                        candidateProfile.CurrLocationId = candidateInfoViewModel.CityId;
                        candidateProfile.CurrLocation = candidateInfoViewModel.CityName;
                        candidateProfile.ProfUpdateFlag = true;
                        candidateProfile.CandName = candidateInfoViewModel.Name;
                        candidateProfile.UpdatedDate = CurrentTime;
                        candidateProfile.MaritalStatus = candidateInfoViewModel.MaritalStatus;
                        candidateProfile.Nationality = candidateInfoViewModel.Nationality;
                        candidateProfile.NoticePeriod = candidateInfoViewModel.NoticePeriod;
                        candidateProfile.Gender = candidateInfoViewModel.Gender;
                        candidateProfile.ContactNo = candidateInfoViewModel.PhoneNumber;
                        candidateProfile.AlteContactNo = candidateInfoViewModel.AlterPhoneNumber;
                        candidateProfile.Dob = candidateInfoViewModel.DateofBirth;
                        candidateProfile.Roles = candidateInfoViewModel.Roles;

                        dbContext.PhCandidateProfiles.Update(candidateProfile);

                        int CandNameLegth = candidateInfoViewModel.Name.Length;
                        var user = dbContext.PiHireUsers.Where(x => x.UserName == candidateProfile.EmailId).FirstOrDefault();
                        if (user != null)
                        {
                            if (candidateProfile.CandName.Length > 50)
                            {
                                user.FirstName = candidateInfoViewModel.Name.Substring(0, 50);
                                user.LastName = candidateInfoViewModel.Name.Substring(50, CandNameLegth - 50);
                            }
                            else
                            {
                                user.FirstName = candidateInfoViewModel.Name;
                                user.LastName = string.Empty;
                            }

                            user.MobileNumber = candidateProfile.ContactNo;
                            dbContext.PiHireUsers.Update(user);
                        }
                        await dbContext.SaveChangesAsync();

                        var BgvDtls = dbContext.PhCandidateBgvDetails.Where(x => x.CandProfId == candidateProfile.Id).FirstOrDefault();
                        if (BgvDtls != null)
                        {
                            if (candidateProfile.CandName.Length > 50)
                            {
                                BgvDtls.FirstName = candidateInfoViewModel.Name.Substring(0, 50);
                                BgvDtls.LastName = candidateInfoViewModel.Name.Substring(50, CandNameLegth - 50);
                            }
                            else
                            {
                                BgvDtls.FirstName = candidateInfoViewModel.Name;
                                BgvDtls.LastName = string.Empty;
                            }
                            BgvDtls.MaritalStatus = candidateInfoViewModel.MaritalStatus;
                            dbContext.PhCandidateBgvDetails.Update(BgvDtls);
                            await dbContext.SaveChangesAsync();
                        }

                        foreach (var SkillItem in candidateInfoViewModel.CandidatesSkillViewModel)
                        {
                            if (SkillItem.Id == 0)
                            {
                                var candSkilSet = new PhCandidateSkillset
                                {
                                    CandProfId = candidateProfile.Id,
                                    CreatedBy = UserId,
                                    CreatedDate = CurrentTime,
                                    SelfRating = SkillItem.SelfRating,
                                    ExpInMonths = SkillItem.ExpInMonths,
                                    ExpInYears = SkillItem.ExpInYears,
                                    TechnologyId = SkillItem.TechnologyId,
                                    Status = (byte)RecordStatus.Active
                                };
                                dbContext.PhCandidateSkillsets.Add(candSkilSet);
                                await dbContext.SaveChangesAsync();
                            }
                            else
                            {
                                var canSkil = dbContext.PhCandidateSkillsets.Where(x => x.TechnologyId == SkillItem.TechnologyId
                                && x.CandProfId == candidateProfile.Id && x.Status == (byte)RecordStatus.Active).FirstOrDefault();
                                if (canSkil != null)
                                {
                                    canSkil.SelfRating = SkillItem.SelfRating;
                                    canSkil.ExpInMonths = SkillItem.ExpInMonths;
                                    canSkil.ExpInYears = SkillItem.ExpInYears;

                                    dbContext.PhCandidateSkillsets.Update(canSkil);
                                    await dbContext.SaveChangesAsync();
                                }
                            }
                        }

                        if (candidateInfoViewModel.CandidateSocialReferenceViewModel != null)
                        {
                            foreach (var RefItem in candidateInfoViewModel.CandidateSocialReferenceViewModel)
                            {
                                var CanSocialReference = dbContext.PhCandSocialPrefs.Where(x => x.CandProfId
                                         == candidateProfile.Id && x.ProfileType == RefItem.ProfileType).FirstOrDefault();
                                if (CanSocialReference != null)
                                {
                                    CanSocialReference.ProfileUrl = RefItem.ProfileURL;
                                    CanSocialReference.UpdatedBy = UserId;
                                    CanSocialReference.UpdatedDate = CurrentTime;

                                    dbContext.PhCandSocialPrefs.Update(CanSocialReference);
                                    await dbContext.SaveChangesAsync();
                                }
                                else
                                {
                                    var phCandSocialPref = new PhCandSocialPref
                                    {
                                        CandProfId = candidateProfile.Id,
                                        CreatedBy = UserId,
                                        CreatedDate = CurrentTime,
                                        Status = (byte)RecordStatus.Active,
                                        ProfileUrl = RefItem.ProfileURL,
                                        ProfileType = RefItem.ProfileType
                                    };
                                    dbContext.PhCandSocialPrefs.Add(phCandSocialPref);
                                    await dbContext.SaveChangesAsync();
                                }

                            }
                        }

                        string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + candidateProfile.Id + "";
                        // Checking for folder is available or not 
                        if (!Directory.Exists(webRootPath))
                        {
                            Directory.CreateDirectory(webRootPath);
                        }
                        //photo
                        if (candidateInfoViewModel.ProfilePhoto != null)
                        {
                            if (candidateInfoViewModel.ProfilePhoto.Length > 0)
                            {
                                var fileName = System.IO.Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + candidateInfoViewModel.ProfilePhoto.FileName);
                                fileName = fileName.Replace(" ", "_");

                                if (fileName.Length > 200)
                                {
                                    fileName = fileName.Substring(0, 199);
                                }

                                var filePath = System.IO.Path.Combine(webRootPath, string.Empty, fileName);
                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await candidateInfoViewModel.ProfilePhoto.CopyToAsync(fileStream);
                                }

                                var CandDoc = dbContext.PhCandidateDocs.Where(x => x.CandProfId == candidateProfile.Id
                                && x.FileGroup == (byte)FileGroup.Profile && x.DocType == "Profile Photo" && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                                if (CandDoc != null)
                                {
                                    CandDoc.UpdatedBy = UserId;
                                    CandDoc.Status = (byte)RecordStatus.Delete;
                                    CandDoc.UpdatedDate = CurrentTime;
                                    dbContext.PhCandidateDocs.Update(CandDoc);
                                }
                                var candidateDoc = new PhCandidateDoc
                                {
                                    Joid = 0,
                                    Status = (byte)RecordStatus.Active,
                                    CandProfId = candidateProfile.Id,
                                    CreatedBy = UserId,
                                    CreatedDate = CurrentTime,
                                    UploadedBy = UserId,
                                    FileGroup = (byte)FileGroup.Profile,
                                    DocType = "Profile Photo",
                                    FileName = fileName,
                                    FileType = candidateInfoViewModel.ProfilePhoto.ContentType,
                                    DocStatus = (byte)DocStatus.Accepted
                                };

                                var cand = await dbContext.PiHireUsers.Where(x => x.UserName == candidateProfile.EmailId).FirstOrDefaultAsync();
                                if (cand != null)
                                {
                                    cand.ProfilePhoto = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + candidateProfile.Id + "/" + fileName;
                                    dbContext.PiHireUsers.Update(cand);
                                }

                                dbContext.PhCandidateDocs.Add(candidateDoc);
                                await dbContext.SaveChangesAsync();

                            }
                        }

                        List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                        // audit 
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = " Candidate Profile details",
                            ActivityDesc = " updated Candidate profile successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = candidateProfile.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = candidateInfoViewModel.CanPrfId,
                            JobId = 0,
                            ActivityType = (byte)LogActivityType.RecordUpdates,
                            ActivityDesc = " updated Candidate profile successfully",
                            UserId = UserId
                        };
                        activityList.Add(activityLog);
                        SaveActivity(activityList);


                        respModel.SetResult(message);
                        respModel.Status = true;
                    }
                    else
                    {
                        message = "The Candidate is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",UpdateCandidateInfo respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateInfoViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }

        public async Task<GetResponseViewModel<List<RecruiterViewModel>>> GetRecruiterDetails(int CandId, int jobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<RecruiterViewModel>>();
            var response = new List<RecruiterViewModel>();
            try
            {

                var RecIds = new List<int?>();
                if (jobId == 0)
                {
                    RecIds = await dbContext.PhJobCandidates.Where(x => x.CandProfId == CandId).Select(x => x.RecruiterId).ToListAsync();
                }
                else
                {
                    RecIds = await dbContext.PhJobCandidates.Where(x => x.CandProfId == CandId && x.Joid == jobId).Select(x => x.RecruiterId).ToListAsync();
                }
                foreach (var item in RecIds)
                {
                    var recDtls = dbContext.PiHireUsers.Where(x => x.Id == item).FirstOrDefault();
                    if (recDtls != null)
                    {
                        response.Add(new RecruiterViewModel
                        {
                            UserId = recDtls.Id,
                            ContactNo = recDtls.MobileNumber,
                            EmailId = recDtls.UserName,
                            Name = recDtls.FirstName + " " + recDtls.LastName,
                            ProfilePhoto = string.Empty
                        });
                    }
                }

                respModel.SetResult(response);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",GetRecruiterDetails respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<CandidateInfoViewModel>> GetCandidateInfo(string EmailId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<CandidateInfoViewModel>();
            var response = new CandidateInfoViewModel();
            try
            {


                var candidateProfile = await dbContext.PhCandidateProfiles.Where(x => x.EmailId == EmailId).FirstOrDefaultAsync();
                if (candidateProfile != null)
                {
                    response.CanPrfId = candidateProfile.Id;
                    response.CountryId = candidateProfile.CountryId;
                    response.CityId = candidateProfile.CurrLocationId;
                    response.CityName = candidateProfile.CurrLocation;
                    response.CountryName = candidateProfile.CountryId == null ? string.Empty : dbContext.PhCountries.Where(x => x.Id == candidateProfile.CountryId.Value).Select(x => x.Nicename).FirstOrDefault();
                    response.EmailId = candidateProfile.EmailId;
                    response.Name = candidateProfile.CandName;

                    response.AlterPhoneNumber = candidateProfile.AlteContactNo;
                    response.PhoneNumber = candidateProfile.ContactNo;
                    response.NoticePeriod = candidateProfile.NoticePeriod == null ? 0 : candidateProfile.NoticePeriod;
                    response.MaritalStatus = candidateProfile.MaritalStatus;
                    response.Gender = candidateProfile.Gender;
                    response.DateofBirth = candidateProfile.Dob;
                    response.Nationality = candidateProfile.Nationality;
                    response.Roles = candidateProfile.Roles;

                    response.CandidatesSkillViewModel = new List<GetCandidateSkillViewModel>();
                    response.CandidatesSkillViewModel = await (from Canskill in dbContext.PhCandidateSkillsets
                                                               join Skill in dbContext.PhTechnologysSes on Canskill.TechnologyId equals Skill.Id
                                                               where Canskill.CandProfId == candidateProfile.Id && Canskill.Status == (byte)RecordStatus.Active
                                                               select new GetCandidateSkillViewModel
                                                               {
                                                                   Id = Canskill.Id,
                                                                   ExpInMonths = Canskill.ExpInMonths,
                                                                   ExpInYears = Canskill.ExpInYears,
                                                                   SelfRating = Canskill.SelfRating,
                                                                   TechnologyId = Canskill.TechnologyId,
                                                                   TechnologyName = Skill.Title
                                                               }).ToListAsync();

                    response.CandidateSocialReferenceViewModel = new List<CandidateSocialReferenceViewModel>();
                    response.CandidateSocialReferenceViewModel = await (from SocialReference in dbContext.PhCandSocialPrefs
                                                                        where SocialReference.CandProfId == candidateProfile.Id
                                                                        select new CandidateSocialReferenceViewModel
                                                                        {
                                                                            ProfileType = SocialReference.ProfileType,
                                                                            ProfileURL = SocialReference.ProfileUrl
                                                                        }).ToListAsync();
                    foreach (var item in response.CandidateSocialReferenceViewModel)
                    {
                        item.ProfileTypeName = Enum.GetName(typeof(SocialProfileType), item.ProfileType);
                    }


                    var documents = await (from canDoc in dbContext.PhCandidateDocs
                                           where canDoc.CandProfId == candidateProfile.Id && canDoc.DocType == "Profile Photo"
                                           && canDoc.FileGroup == (byte)FileGroup.Profile && canDoc.Status != (byte)RecordStatus.Delete
                                           select new CandidateFilesViewModel
                                           {
                                               DocType = canDoc.DocType,
                                               FileGroup = canDoc.FileGroup,
                                               CandProfId = canDoc.CandProfId,
                                               FileName = canDoc.FileName
                                           }).FirstOrDefaultAsync();
                    if (documents != null)
                    {
                        response.ProfilePhoto = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + documents.CandProfId + "/" + documents.FileName;
                    }
                }
                else
                {
                    var user = dbContext.PiHireUsers.Where(x => x.UserName == EmailId && x.UserType == (byte)UserType.Candidate).FirstOrDefault();
                    if (user != null)
                    {
                        response.PhoneNumber = user.MobileNumber;
                        response.EmailId = user.UserName;
                        response.Name = user.FirstName + " " + user.LastName;
                    }
                }

                respModel.SetResult(response);
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


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> SaveCandidateBGV(SaveCandidateBGVViewModel bgvDtls)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();

            int UserId = Usr.Id;
            string message = "Updated Successfully";

            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    var dtls = dbContext.PhCandidateBgvDetails.Where(x => x.CandProfId == bgvDtls.CandProfId).FirstOrDefault();
                    if (dtls != null)
                    {

                        dtls.FirstName = bgvDtls.FirstName;
                        dtls.AnotherName = bgvDtls.OtherName;
                        dtls.MiddleName = bgvDtls.MiddleName;
                        dtls.LastName = bgvDtls.LastName;
                        dtls.DateOfBirth = bgvDtls.DateOfBirth != null ? bgvDtls.DateOfBirth.Value : bgvDtls.DateOfBirth;
                        dtls.PlaceOfBirth = bgvDtls.PlaceOfBirth;
                        dtls.Gender = bgvDtls.Gender;
                        dtls.MaritalStatus = bgvDtls.MaritalStatus;
                        dtls.Nationality = bgvDtls.Nationality;

                        dtls.ExpeInYears = bgvDtls.ExpeInYears;
                        dtls.ExpeInMonths = bgvDtls.ExpeInMonths;

                        dtls.FatherName = bgvDtls.FatherName;
                        dtls.MotherName = bgvDtls.MotherName;
                        dtls.SpouseName = bgvDtls.SpouseName;
                        dtls.NoOfKids = bgvDtls.NoOfKids;
                        dtls.HomePhone = bgvDtls.HomePhone;
                        dtls.MobileNo = bgvDtls.MobileNo;

                        dtls.EmerContactPerson = bgvDtls.EmerContactPerson;
                        dtls.EmerContactNo = bgvDtls.EmerContactNo;
                        dtls.EmerContactRelation = bgvDtls.EmerContactRelation;

                        dtls.Ppnumber = bgvDtls.Ppnumber;
                        dtls.EmiratesId = bgvDtls.EmiratesId;
                        dtls.UgmedicalTreaDetails = bgvDtls.UGMedicalTreaDetails;
                        dtls.UgmedicalTreaFlag = bgvDtls.UGMedicalTreaFlag;
                        dtls.PpexpiryDate = bgvDtls.PPExpiryDate != null ? bgvDtls.PPExpiryDate.Value : bgvDtls.PPExpiryDate;

                        dtls.PresAddress = bgvDtls.PresAddress;
                        dtls.PresAddrResiSince = bgvDtls.PresAddrResiSince != null ? bgvDtls.PresAddrResiSince.Value : bgvDtls.PresAddrResiSince;
                        dtls.PresAddrResiType = bgvDtls.PresAddrResiType;

                        dtls.PresAddrCountryId = bgvDtls.PresAddrCountryId;
                        dtls.PresAddrCityId = bgvDtls.PresAddrCityId;

                        dtls.PresAddrContactPerson = bgvDtls.PresAddrContactPerson;
                        dtls.PresAddrContactNo = bgvDtls.PresAddrContactNo;
                        dtls.PresAddrLandMark = bgvDtls.PresAddrLandMark;
                        dtls.PresAddrPrefTimeForVerification = bgvDtls.PresAddrPrefTimeForVerification;

                        dtls.PermAddress = bgvDtls.PermAddress;
                        dtls.PermAddrLandMark = bgvDtls.PermAddrLandMark;
                        dtls.PermAddrCityId = bgvDtls.PermAddrCityID;
                        dtls.PermAddrCountryId = bgvDtls.PermAddrCountryID;
                        dtls.PermAddrResiType = bgvDtls.PermAddrResiType;
                        dtls.PermAddrResiSince = bgvDtls.PermAddrResiSince != null ? bgvDtls.PermAddrResiSince.Value : bgvDtls.PermAddrResiSince;
                        dtls.PermAddrResiTill = bgvDtls.PermAddrResiTill != null ? bgvDtls.PermAddrResiTill.Value : bgvDtls.PermAddrResiTill;
                        dtls.PermAddrContactPerson = bgvDtls.PermAddrContactPerson;
                        dtls.PermAddrContactNo = bgvDtls.PermAddrContactNo;
                        dtls.PermContactRelation = bgvDtls.PermContactRelation;
                        dtls.PermPinCode = bgvDtls.PermPinCode;
                        dtls.PresPinCode = bgvDtls.PresPinCode;

                        dtls.UpdatedBy = UserId;
                        dtls.UpdatedDate = CurrentTime;
                        dtls.BloodGroup = bgvDtls.BloodGroup;
                        dtls.MiddleName = bgvDtls.MiddleName;

                        if (bgvDtls.FinalSubmit && !string.IsNullOrEmpty(dtls.Ppnumber) && dtls.PpexpiryDate != null)
                        {
                            dtls.BgcompStatus = (byte)BGCompStatus.Completed;
                        }

                        dbContext.PhCandidateBgvDetails.Update(dtls);
                        await dbContext.SaveChangesAsync();

                        if (bgvDtls.FinalSubmit && !string.IsNullOrEmpty(dtls.Ppnumber) && dtls.PpexpiryDate != null)
                        {
                            var JoCanDtls = dbContext.PhJobCandidates.Where(x => x.Joid == bgvDtls.JoId && x.CandProfId == bgvDtls.CandProfId).FirstOrDefault();
                            if (JoCanDtls != null)
                            {
                                // applying workflow ruls                           
                                var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                                {
                                    ActionMode = (byte)WorkflowActionMode.Candidate,
                                    CanProfId = bgvDtls.CandProfId,
                                    JobId = bgvDtls.JoId,
                                    TaskCode = TaskCode.SUBG.ToString(), // Submission BGV 
                                    CurrentStatusId = JoCanDtls.CandProfStatus,
                                    UserId = UserId
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
                                            CreatedBy = UserId
                                        };
                                        notificationPushedViewModel.Add(notificationPushed);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var model = new PhCandidateBgvDetail
                        {
                            AnotherName = bgvDtls.OtherName,
                            PermAddrCountryId = bgvDtls.PermAddrCountryID,
                            CandProfId = bgvDtls.CandProfId,
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            DateOfBirth = bgvDtls.DateOfBirth != null ? bgvDtls.DateOfBirth.Value : bgvDtls.DateOfBirth,
                            BgacceptFlag = false,
                            BloodGroup = bgvDtls.BloodGroup,
                            MiddleName = bgvDtls.MiddleName,
                            PresContactRelation = string.Empty,
                            EmerContactNo = bgvDtls.EmerContactNo,
                            EmerContactPerson = bgvDtls.EmerContactPerson,
                            EmerContactRelation = bgvDtls.EmerContactRelation,
                            EmiratesId = bgvDtls.EmiratesId,
                            ExpeInMonths = bgvDtls.ExpeInMonths,
                            ExpeInYears = bgvDtls.ExpeInYears,
                            FatherName = bgvDtls.FatherName,
                            FirstName = bgvDtls.FirstName,
                            Gender = bgvDtls.Gender,
                            HomePhone = bgvDtls.HomePhone,
                            LastName = bgvDtls.LastName,
                            MaritalStatus = bgvDtls.MaritalStatus,
                            MobileNo = bgvDtls.MobileNo,
                            MotherName = bgvDtls.MotherName,
                            Nationality = bgvDtls.Nationality,
                            NoOfKids = bgvDtls.NoOfKids,
                            PermAddrCityId = bgvDtls.PermAddrCityID,
                            PermAddrContactNo = bgvDtls.PermAddrContactNo,
                            PermAddrContactPerson = bgvDtls.PermAddrContactPerson,
                            PermAddress = bgvDtls.PermAddress,
                            PermAddrLandMark = bgvDtls.PermAddrLandMark,
                            PermAddrResiSince = bgvDtls.PermAddrResiSince,
                            PermAddrResiTill = bgvDtls.PermAddrResiTill != null ? bgvDtls.PermAddrResiTill.Value : bgvDtls.PermAddrResiTill,
                            PermAddrResiType = bgvDtls.PermAddrResiType,
                            PermContactRelation = bgvDtls.PermContactRelation,
                            PlaceOfBirth = bgvDtls.PlaceOfBirth,
                            PpexpiryDate = bgvDtls.PPExpiryDate != null ? bgvDtls.PPExpiryDate.Value : bgvDtls.PPExpiryDate,
                            Ppnumber = bgvDtls.Ppnumber,
                            PresAddrCityId = bgvDtls.PresAddrCityId,
                            PresAddrContactNo = bgvDtls.PresAddrContactNo,
                            PresAddrContactPerson = bgvDtls.PresAddrContactPerson,
                            PresAddrCountryId = bgvDtls.PresAddrCountryId,
                            PresAddress = bgvDtls.PresAddress,
                            PresAddrLandMark = bgvDtls.PresAddrLandMark,
                            PresAddrPrefTimeForVerification = bgvDtls.PresAddrPrefTimeForVerification,
                            PresAddrResiSince = bgvDtls.PresAddrResiSince != null ? bgvDtls.PresAddrResiSince.Value : bgvDtls.PresAddrResiSince,
                            PresAddrResiType = bgvDtls.PresAddrResiType != null ? bgvDtls.PresAddrResiType.Value : bgvDtls.PresAddrResiType,
                            SpouseName = bgvDtls.SpouseName,
                            Status = (byte)RecordStatus.Active,
                            UgmedicalTreaDetails = bgvDtls.UGMedicalTreaDetails,
                            UgmedicalTreaFlag = bgvDtls.UGMedicalTreaFlag,
                            BgcompStatus = (byte)BGCompStatus.PersonalDetails,
                            PermPinCode = bgvDtls.PermPinCode,
                            PresPinCode = bgvDtls.PresPinCode,
                        };

                        dbContext.PhCandidateBgvDetails.Add(model);
                        await dbContext.SaveChangesAsync();
                    }


                    trans.Commit();

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(bgvDtls), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }

        public async Task<CreateResponseViewModel<string>> SaveCandidateEmpBGV(SaveCandidateBGVEmpViewModel saveCandidateBGVEmpViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            string message = "Updated Successfully";
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {

                    if (saveCandidateBGVEmpViewModel.Id == 0)
                    {
                        var CandidateEmpmtDetails = dbContext.PhCandidateEmpmtDetails.Where(x => x.CandProfId == saveCandidateBGVEmpViewModel.CandProfId && x.CurrentWorkingFlag == true).FirstOrDefault();
                        if (CandidateEmpmtDetails != null && saveCandidateBGVEmpViewModel.CurrentWorkingFlag == true)
                        {
                            message = "The Candidate's current employment is already tied to your existence records";
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        }
                        else
                        {
                            var model = new PhCandidateEmpmtDetail
                            {
                                CandProfId = saveCandidateBGVEmpViewModel.CandProfId,
                                Address = saveCandidateBGVEmpViewModel.Address,
                                CityId = saveCandidateBGVEmpViewModel.CityId,
                                CountryId = saveCandidateBGVEmpViewModel.CountryId,
                                Cpdesignation = saveCandidateBGVEmpViewModel.Cpdesignation,
                                CpemailId = saveCandidateBGVEmpViewModel.CpemailId,
                                Cpname = saveCandidateBGVEmpViewModel.Cpname,
                                Cpnumber = saveCandidateBGVEmpViewModel.Cpnumber,
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                CurrentWorkingFlag = saveCandidateBGVEmpViewModel.CurrentWorkingFlag,
                                Designation = saveCandidateBGVEmpViewModel.Designation,
                                DesignationId = saveCandidateBGVEmpViewModel.DesignationId,
                                EmployerName = saveCandidateBGVEmpViewModel.EmployerName,
                                EmployId = saveCandidateBGVEmpViewModel.EmployId,
                                EmptToDate = saveCandidateBGVEmpViewModel.EmptToDate != null ? saveCandidateBGVEmpViewModel.EmptToDate.Value : saveCandidateBGVEmpViewModel.EmptToDate,
                                EmptFromDate = saveCandidateBGVEmpViewModel.EmptFromDate != null ? saveCandidateBGVEmpViewModel.EmptFromDate.Value : saveCandidateBGVEmpViewModel.EmptFromDate,
                                HrcontactNo = saveCandidateBGVEmpViewModel.HrcontactNo,
                                HremailId = saveCandidateBGVEmpViewModel.HremailId,
                                OfficialEmailId = saveCandidateBGVEmpViewModel.OfficialEmailId,
                                PhoneNumber = saveCandidateBGVEmpViewModel.PhoneNumber,
                                Status = (byte)RecordStatus.Active
                            };
                            dbContext.PhCandidateEmpmtDetails.Add(model);
                            await dbContext.SaveChangesAsync();


                            var dtls = dbContext.PhCandidateBgvDetails.Where(x => x.CandProfId == saveCandidateBGVEmpViewModel.CandProfId).FirstOrDefault();
                            if (dtls != null)
                            {
                                if (dtls.BgcompStatus == (byte)BGCompStatus.PersonalDetails)
                                {
                                    dtls.BgcompStatus = (byte)BGCompStatus.Employmentdetails;
                                    dtls.UpdatedBy = UserId;
                                    dtls.UpdatedDate = CurrentTime;

                                    dbContext.PhCandidateBgvDetails.Update(dtls);
                                }
                            }

                            respModel.SetResult(message);
                            respModel.Status = true;
                        }

                    }
                    else
                    {
                        var CandidateEmpmtDetails = dbContext.PhCandidateEmpmtDetails.Where(x => x.Id == saveCandidateBGVEmpViewModel.Id && x.CandProfId == saveCandidateBGVEmpViewModel.CandProfId).FirstOrDefault();
                        if (CandidateEmpmtDetails != null)
                        {
                            if (saveCandidateBGVEmpViewModel.CurrentWorkingFlag == true)
                            {
                                var dtls = dbContext.PhCandidateEmpmtDetails.Where(x => x.Id != saveCandidateBGVEmpViewModel.Id && x.CandProfId == saveCandidateBGVEmpViewModel.CandProfId).ToList();
                                foreach (var item in dtls)
                                {
                                    item.CurrentWorkingFlag = false;
                                    dbContext.PhCandidateEmpmtDetails.Update(item);
                                    await dbContext.SaveChangesAsync();
                                }
                            }

                            CandidateEmpmtDetails.Address = saveCandidateBGVEmpViewModel.Address;
                            CandidateEmpmtDetails.CityId = saveCandidateBGVEmpViewModel.CityId;
                            CandidateEmpmtDetails.CountryId = saveCandidateBGVEmpViewModel.CountryId;
                            CandidateEmpmtDetails.Cpdesignation = saveCandidateBGVEmpViewModel.Cpdesignation;
                            CandidateEmpmtDetails.CpemailId = saveCandidateBGVEmpViewModel.CpemailId;
                            CandidateEmpmtDetails.Cpname = saveCandidateBGVEmpViewModel.Cpname;
                            CandidateEmpmtDetails.Cpnumber = saveCandidateBGVEmpViewModel.Cpnumber;
                            CandidateEmpmtDetails.UpdatedBy = UserId;
                            CandidateEmpmtDetails.UpdatedDate = CurrentTime;
                            CandidateEmpmtDetails.CurrentWorkingFlag = saveCandidateBGVEmpViewModel.CurrentWorkingFlag;
                            CandidateEmpmtDetails.Designation = saveCandidateBGVEmpViewModel.Designation;
                            CandidateEmpmtDetails.DesignationId = saveCandidateBGVEmpViewModel.DesignationId;
                            CandidateEmpmtDetails.EmployerName = saveCandidateBGVEmpViewModel.EmployerName;
                            CandidateEmpmtDetails.EmployId = saveCandidateBGVEmpViewModel.EmployId;
                            CandidateEmpmtDetails.EmptToDate = saveCandidateBGVEmpViewModel.EmptToDate != null ? saveCandidateBGVEmpViewModel.EmptToDate.Value : saveCandidateBGVEmpViewModel.EmptToDate;
                            CandidateEmpmtDetails.EmptFromDate = saveCandidateBGVEmpViewModel.EmptFromDate != null ? saveCandidateBGVEmpViewModel.EmptFromDate.Value : saveCandidateBGVEmpViewModel.EmptFromDate;
                            CandidateEmpmtDetails.HrcontactNo = saveCandidateBGVEmpViewModel.HrcontactNo;
                            CandidateEmpmtDetails.HremailId = saveCandidateBGVEmpViewModel.HremailId;
                            CandidateEmpmtDetails.OfficialEmailId = saveCandidateBGVEmpViewModel.OfficialEmailId;
                            CandidateEmpmtDetails.PhoneNumber = saveCandidateBGVEmpViewModel.PhoneNumber;

                            dbContext.PhCandidateEmpmtDetails.Update(CandidateEmpmtDetails);
                            await dbContext.SaveChangesAsync();

                            respModel.SetResult(message);
                            respModel.Status = true;
                        }
                        else
                        {
                            message = "The Candidate is not available for Update Employment details";
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(saveCandidateBGVEmpViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return respModel;
        }

        public async Task<CreateResponseViewModel<string>> SaveCandidateEduBGV(SaveCandidateBGVEduViewModel saveCandidateBGVEduViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            int UserId = Usr.Id;
            string message = "Updated Successfully";
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {

                    if (saveCandidateBGVEduViewModel.Id == 0)
                    {
                        var dtls = dbContext.PhCandidateBgvDetails.Where(x => x.CandProfId == saveCandidateBGVEduViewModel.CandProfId).FirstOrDefault();
                        if (dtls != null)
                        {
                            if (dtls.BgcompStatus == (byte)BGCompStatus.PersonalDetails || dtls.BgcompStatus == (byte)BGCompStatus.Employmentdetails)
                            {
                                dtls.BgcompStatus = (byte)BGCompStatus.Educationdetails;
                                dtls.UpdatedBy = UserId;
                                dtls.UpdatedDate = CurrentTime;

                                dbContext.PhCandidateBgvDetails.Update(dtls);
                            }
                        }
                        var model = new PhCandidateEduDetail
                        {
                            CandProfId = saveCandidateBGVEduViewModel.CandProfId,
                            YearofPassing = saveCandidateBGVEduViewModel.YearofPassing,
                            UnivOrInstitution = saveCandidateBGVEduViewModel.UnivOrInstitution,
                            Course = saveCandidateBGVEduViewModel.Course,
                            CourseId = saveCandidateBGVEduViewModel.CourseId,
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            DurationFrom = saveCandidateBGVEduViewModel.DurationFrom != null ? saveCandidateBGVEduViewModel.DurationFrom.Value : saveCandidateBGVEduViewModel.DurationFrom,
                            DurationTo = saveCandidateBGVEduViewModel.DurationTo != null ? saveCandidateBGVEduViewModel.DurationTo.Value : saveCandidateBGVEduViewModel.DurationTo,
                            Grade = saveCandidateBGVEduViewModel.Grade,
                            Percentage = saveCandidateBGVEduViewModel.Percentage,
                            Qualification = saveCandidateBGVEduViewModel.Qualification,
                            QualificationId = saveCandidateBGVEduViewModel.QualificationId,
                            Status = (byte)RecordStatus.Active
                        };
                        dbContext.PhCandidateEduDetails.Add(model);
                        await dbContext.SaveChangesAsync();

                        respModel.SetResult(message);
                        respModel.Status = true;
                    }
                    else
                    {
                        var CandidateEduDetails = dbContext.PhCandidateEduDetails.Where(x => x.Id == saveCandidateBGVEduViewModel.Id && x.CandProfId == saveCandidateBGVEduViewModel.CandProfId).FirstOrDefault();
                        if (CandidateEduDetails != null)
                        {
                            CandidateEduDetails.YearofPassing = saveCandidateBGVEduViewModel.YearofPassing;
                            CandidateEduDetails.UnivOrInstitution = saveCandidateBGVEduViewModel.UnivOrInstitution;
                            CandidateEduDetails.Course = saveCandidateBGVEduViewModel.Course;
                            CandidateEduDetails.CourseId = saveCandidateBGVEduViewModel.CourseId;
                            CandidateEduDetails.UpdatedBy = UserId;
                            CandidateEduDetails.UpdatedDate = CurrentTime;
                            CandidateEduDetails.DurationFrom = saveCandidateBGVEduViewModel.DurationFrom != null ? saveCandidateBGVEduViewModel.DurationFrom.Value : saveCandidateBGVEduViewModel.DurationFrom;
                            CandidateEduDetails.DurationTo = saveCandidateBGVEduViewModel.DurationTo != null ? saveCandidateBGVEduViewModel.DurationTo.Value : saveCandidateBGVEduViewModel.DurationTo;
                            CandidateEduDetails.Grade = saveCandidateBGVEduViewModel.Grade;
                            CandidateEduDetails.Percentage = saveCandidateBGVEduViewModel.Percentage;
                            CandidateEduDetails.Qualification = saveCandidateBGVEduViewModel.Qualification;
                            CandidateEduDetails.QualificationId = saveCandidateBGVEduViewModel.QualificationId;

                            dbContext.PhCandidateEduDetails.Update(CandidateEduDetails);
                            await dbContext.SaveChangesAsync();

                            respModel.SetResult(message);
                            respModel.Status = true;
                        }
                        else
                        {
                            message = "The Candidate is not available for Update Education details";
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(saveCandidateBGVEduViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return respModel;
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> AcceptCandidateBGV(AcceptCandidateBGVViewModel acceptCandidateBGVViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    var phJobCandidates = dbContext.PhJobCandidates.Where(x => x.Joid == acceptCandidateBGVViewModel.JoId && x.CandProfId == acceptCandidateBGVViewModel.CandProfId).FirstOrDefault();
                    if (phJobCandidates != null)
                    {
                        phJobCandidates.BgvacceptedFlag = acceptCandidateBGVViewModel.Accept;
                        phJobCandidates.Bgvcomments = acceptCandidateBGVViewModel.Remarks;
                        phJobCandidates.UpdatedBy = UserId;
                        phJobCandidates.UpdatedDate = CurrentTime;
                        dbContext.PhJobCandidates.Update(phJobCandidates);

                        var dtls = dbContext.PhCandidateBgvDetails.Where(x => x.CandProfId == acceptCandidateBGVViewModel.CandProfId).FirstOrDefault();
                        if (dtls != null)
                        {
                            dtls.BgacceptFlag = acceptCandidateBGVViewModel.Accept;
                            dtls.UpdatedBy = UserId;
                            dtls.UpdatedDate = CurrentTime;

                            dbContext.PhCandidateBgvDetails.Update(dtls);
                        }
                        await dbContext.SaveChangesAsync();

                        string Accept = string.Empty;
                        if (acceptCandidateBGVViewModel.Accept)
                        {
                            Accept = "Accept.";

                            var resp = await DownloadCandidateBGVForm(acceptCandidateBGVViewModel.CandProfId, acceptCandidateBGVViewModel.JoId);

                            if (resp.Status)
                            {
                                var result = resp.Result;
                                if (result != null)
                                {
                                    var fileUpload = new PhCandidateDoc()
                                    {
                                        DocType = "BGV",
                                        FileGroup = (byte)FileGroup.Other,
                                        FileName = result.FileName,
                                        DocStatus = (byte)DocStatus.Accepted,
                                        Joid = acceptCandidateBGVViewModel.JoId,
                                        CandProfId = acceptCandidateBGVViewModel.CandProfId,
                                        FileType = result.FileType,
                                        CreatedBy = UserId
                                    };

                                    dbContext.PhCandidateDocs.Add(fileUpload);
                                    await dbContext.SaveChangesAsync();

                                }
                            }

                        }
                        else
                        {
                            Accept = "Modifications.";
                        }

                        string Remarks = string.Empty;
                        if (!string.IsNullOrEmpty(acceptCandidateBGVViewModel.Remarks))
                        {
                            Remarks = " Remarks :" + acceptCandidateBGVViewModel.Remarks;
                        }
                        List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                        // Audit 
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Request BGV " + Accept + "",
                            ActivityDesc = "Requested for BGV form for " + Accept + "",
                            ActivityType = (byte)AuditActivityType.StatusUpdates,
                            TaskID = phJobCandidates.CandProfId,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        // Activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = phJobCandidates.CandProfId,
                            JobId = phJobCandidates.Joid,
                            ActivityType = (byte)LogActivityType.RecordUpdates,
                            ActivityDesc = " has Requested for BGV form for " + Accept + " " + Remarks + "",
                            UserId = UserId
                        };
                        activityList.Add(activityLog);
                        SaveActivity(activityList);

                        // Applying workflow Rules
                        var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                        {
                            ActionMode = (byte)WorkflowActionMode.Candidate,
                            CanProfId = acceptCandidateBGVViewModel.CandProfId,
                            CurrentStatusId = phJobCandidates.CandProfStatus,
                            JobId = acceptCandidateBGVViewModel.JoId,
                            UserId = UserId
                        };
                        if (acceptCandidateBGVViewModel.Accept == true)
                        {
                            workFlowRuleSearchViewModel.TaskCode = TaskCode.ABG.ToString(); // Accept BGV 
                        }
                        else
                        {
                            workFlowRuleSearchViewModel.TaskCode = TaskCode.BGC.ToString(); //  BGV Clarification due
                        }

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
                                    CreatedBy = UserId
                                };
                                notificationPushedViewModel.Add(notificationPushed);
                            }
                        }
                        if (!wfResp.Status)
                        {
                            message = wfResp.Message.Count > 0 ? string.Join(",", wfResp.Message).ToString() : string.Empty;
                            respModel.Status = false;
                        }
                        else
                        {
                            respModel.Status = true;
                        }
                        respModel.SetResult(message);
                    }
                    else
                    {
                        message = "The Candidate is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(acceptCandidateBGVViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        public async Task<GetResponseViewModel<CandidateBGVViewModel>> GetCandidateBGVDtls(int CanId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<CandidateBGVViewModel>();
            string message = string.Empty;

            try
            {


                var dtls = await CandidateBGVDtls(CanId, JobId);

                respModel.SetResult(dtls);
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


        public async Task<CandidateBGVViewModel> CandidateBGVDtls(int CanId, int JobId)
        {
            var dtls = new CandidateBGVViewModel();
            var bgvDtls = dbContext.PhCandidateBgvDetails.Where(x => x.CandProfId == CanId).FirstOrDefault();
            var Countrys = await dbContext.GeCountryList();
            if (bgvDtls != null)
            {
                var Cities = await dbContext.GeCityList();

                var canDtls = (from can in dbContext.PhCandidateProfiles
                               join canJob in dbContext.PhJobCandidates on can.Id equals canJob.CandProfId
                               join job in dbContext.PhJobOpeningsAddlDetails on canJob.Joid equals job.Joid
                               join canJobStus in dbContext.PhCandStatusSes on canJob.CandProfStatus equals canJobStus.Id
                               where canJob.CandProfId == CanId && canJob.Joid == JobId
                               select new
                               {
                                   can.Id,
                                   can.EmailId,
                                   canJob.CandProfStatus,
                                   canJobStus.Cscode,
                                   canJob.Bgvcomments,
                                   canJob.BgvacceptedFlag,
                                   job.Puid

                               }).FirstOrDefault();

                dtls.PuId = canDtls?.Puid;
                dtls.CadidateStatusCode = canDtls?.Cscode;
                dtls.EmailId = canDtls?.EmailId;
                dtls.CandProfId = bgvDtls.CandProfId;
                dtls.FirstName = bgvDtls.FirstName;
                dtls.LastName = bgvDtls.LastName;
                dtls.MiddleName = bgvDtls.MiddleName;
                dtls.MotherName = bgvDtls.MotherName;
                dtls.OtherName = bgvDtls.AnotherName;
                dtls.DateOfBirth = bgvDtls.DateOfBirth;
                dtls.PlaceOfBirth = bgvDtls.PlaceOfBirth;
                dtls.BloodGroup = bgvDtls.BloodGroup;
                dtls.BloodGroupName = bgvDtls.BloodGroup != null ? dbContext.PhRefMasters.Where(x => x.GroupId == 5 && x.Id == bgvDtls.BloodGroup).Select(x => x.Rmvalue).FirstOrDefault() : string.Empty;
                dtls.Gender = bgvDtls.Gender;
                dtls.MaritalStatus = bgvDtls.MaritalStatus;
                dtls.Nationality = bgvDtls.Nationality;
                dtls.GenderName = bgvDtls.Gender > 0 ? dbContext.PhRefMasters.Where(da => da.Id == bgvDtls.Gender).Select(da => da.Rmvalue).FirstOrDefault() : "";
                dtls.MaritalStatusName = bgvDtls.MaritalStatus > 0 ? dbContext.PhRefMasters.Where(da => da.Id == bgvDtls.MaritalStatus).Select(da => da.Rmvalue).FirstOrDefault() : "";
                dtls.NationalityName = bgvDtls.Nationality != null ? Countrys.Where(x => x.Id == bgvDtls.Nationality).Select(x => x.Name).FirstOrDefault() : string.Empty;




                dtls.ExpeInYears = bgvDtls.ExpeInYears;
                dtls.ExpeInMonths = bgvDtls.ExpeInMonths;



                dtls.FatherName = bgvDtls.FatherName;
                dtls.FatherName = bgvDtls.FatherName;
                dtls.SpouseName = bgvDtls.SpouseName;
                dtls.NoOfKids = bgvDtls.NoOfKids;
                dtls.HomePhone = bgvDtls.HomePhone;
                dtls.MobileNo = bgvDtls.MobileNo;

                dtls.EmerContactPerson = bgvDtls.EmerContactPerson;
                dtls.EmerContactNo = bgvDtls.EmerContactNo;
                dtls.EmerContactRelation = bgvDtls.EmerContactRelation;

                dtls.Ppnumber = bgvDtls.Ppnumber;
                dtls.EmiratesId = bgvDtls.EmiratesId;
                dtls.UGMedicalTreaDetails = bgvDtls.UgmedicalTreaDetails;
                dtls.UGMedicalTreaFlag = bgvDtls.UgmedicalTreaFlag;
                dtls.PPExpiryDate = bgvDtls.PpexpiryDate;

                dtls.PresAddress = bgvDtls.PresAddress;
                dtls.PresAddrResiSince = bgvDtls.PresAddrResiSince;
                dtls.PresAddrResiType = bgvDtls.PresAddrResiType;
                dtls.PresAddrResiTypeName = bgvDtls.PresAddrResiType != null ? Enum.GetName(typeof(ResiType), bgvDtls.PresAddrResiType) : string.Empty;
                dtls.PresAddrCountryId = bgvDtls.PresAddrCountryId;
                dtls.PresAddrCountryName = bgvDtls.PresAddrCountryId != null ? Countrys.Where(x => x.Id == bgvDtls.PresAddrCountryId).Select(x => x.Name).FirstOrDefault() : string.Empty;
                dtls.PresAddrCityId = bgvDtls.PresAddrCityId;
                dtls.PresAddrCityName = bgvDtls.PresAddrCityId != null ? Cities.Where(x => x.Id == bgvDtls.PresAddrCityId).Select(x => x.Name).FirstOrDefault() : string.Empty;

                dtls.PresAddrContactPerson = bgvDtls.PresAddrContactPerson;
                dtls.PresAddrContactNo = bgvDtls.PresAddrContactNo;
                dtls.PresAddrLandMark = bgvDtls.PresAddrLandMark;
                dtls.PresAddrPrefTimeForVerification = bgvDtls.PresAddrPrefTimeForVerification;

                dtls.PermAddress = bgvDtls.PermAddress;
                dtls.PermAddrLandMark = bgvDtls.PermAddrLandMark;

                dtls.PermAddrCityID = bgvDtls.PermAddrCityId;
                dtls.PermAddrCountryID = bgvDtls.PermAddrCountryId;
                dtls.PermAddrCountryName = bgvDtls.PermAddrCountryId != null ? Countrys.Where(x => x.Id == bgvDtls.PermAddrCountryId).Select(x => x.Name).FirstOrDefault() : string.Empty;
                dtls.PermAddrCityName = bgvDtls.PermAddrCityId != null ? Cities.Where(x => x.Id == bgvDtls.PermAddrCityId).Select(x => x.Name).FirstOrDefault() : string.Empty;

                dtls.PermAddrResiType = bgvDtls.PermAddrResiType;
                dtls.PermAddrResiTypeName = bgvDtls.PermAddrResiType != null ? Enum.GetName(typeof(ResiType), bgvDtls.PermAddrResiType) : string.Empty;
                dtls.PermAddrResiSince = bgvDtls.PermAddrResiSince;
                dtls.PermAddrResiTill = bgvDtls.PermAddrResiTill;
                dtls.PermAddrContactPerson = bgvDtls.PermAddrContactPerson;
                dtls.PermAddrContactNo = bgvDtls.PermAddrContactNo;
                dtls.PermContactRelation = bgvDtls.PermContactRelation;
                dtls.PresAddrPrefTimeForVerification = bgvDtls.PresAddrPrefTimeForVerification;
                dtls.BgacceptFlag = bgvDtls.BgacceptFlag;
                dtls.Bgvcomments = canDtls.Bgvcomments;
                dtls.JoId = JobId;
                dtls.PresPinCode = bgvDtls.PresPinCode;
                dtls.PermPinCode = bgvDtls.PermPinCode;


                var eduDtls = dbContext.PhCandidateEduDetails.Where(x => x.CandProfId == CanId && x.Status != (byte)RecordStatus.Delete).ToList();
                if (eduDtls.Count > 0)
                {
                    dtls.CandidateBGVEduViewModel = new List<CandidateBGVEduViewModel>();
                    foreach (var item in eduDtls)
                    {
                        var model = new CandidateBGVEduViewModel
                        {
                            Id = item.Id,
                            QualificationId = item.QualificationId,
                            Qualification = item.Qualification,
                            Course = item.Course,
                            CourseId = item.CourseId,
                            UnivOrInstitution = item.UnivOrInstitution,
                            YearofPassing = item.YearofPassing,
                            DurationFrom = item.DurationFrom,
                            DurationTo = item.DurationTo,
                            Grade = item.Grade,
                            Percentage = item.Percentage
                        };
                        dtls.CandidateBGVEduViewModel.Add(model);
                    }
                }

                var empDtls = dbContext.PhCandidateEmpmtDetails.Where(x => x.CandProfId == CanId && x.Status != (byte)RecordStatus.Delete).ToList();
                if (empDtls.Count > 0)
                {
                    dtls.CandidateBGVEmployeeViewModel = new List<CandidateBGVEmployeeViewModel>();
                    foreach (var item in empDtls)
                    {
                        var model = new CandidateBGVEmployeeViewModel
                        {
                            Id = item.Id,
                            Address = item.Address,
                            CityId = item.CityId,
                            CityName = item.CityId != null ? Cities.Where(x => x.Id == item.CityId).Select(x => x.Name).FirstOrDefault() : string.Empty,
                            CountryId = item.CountryId,
                            CountryName = item.CountryId != null ? Countrys.Where(x => x.Id == item.CountryId).Select(x => x.Name).FirstOrDefault() : string.Empty,
                            Cpdesignation = item.Cpdesignation,
                            CpemailId = item.CpemailId,
                            Cpname = item.Cpname,
                            Cpnumber = item.Cpnumber,
                            CurrentWorkingFlag = item.CurrentWorkingFlag,
                            Designation = item.Designation,
                            DesignationId = item.DesignationId,
                            EmployerName = item.EmployerName,
                            EmployId = item.EmployId,
                            EmptFromDate = item.EmptFromDate,
                            EmptToDate = item.EmptToDate,
                            OfficialEmailId = item.OfficialEmailId,
                            HrcontactNo = item.HrcontactNo,
                            HremailId = item.HremailId,
                            PhoneNumber = item.PhoneNumber
                        };
                        dtls.CandidateBGVEmployeeViewModel.Add(model);
                    }
                }

            }
            else
            {
                var canDtls = (from can in dbContext.PhCandidateProfiles
                               join canJob in dbContext.PhJobCandidates on can.Id equals canJob.CandProfId
                               where canJob.CandProfId == CanId && canJob.Joid == JobId
                               select new
                               {
                                   can.Id,
                                   can.Dob,
                                   can.Gender,
                                   can.MaritalStatus,
                                   can.Nationality,
                                   can.CandName,
                                   can.ExperienceInMonths,
                                   can.ContactNo,
                                   can.Experience
                               }).FirstOrDefault();
                if (canDtls != null)
                {

                    string[] expList = null;
                    if (!string.IsNullOrEmpty(canDtls.Experience))
                    {
                        expList = canDtls.Experience.Split(".");
                    }
                    if (expList != null)
                    {
                        dtls.ExpeInYears = Convert.ToInt32(expList[0]);
                        if (expList.Length > 0)
                        {
                            dtls.ExpeInMonths = Convert.ToInt32(expList[1]);
                        }
                    }


                    dtls.CandProfId = canDtls.Id;
                    dtls.FirstName = canDtls.CandName;
                    dtls.DateOfBirth = canDtls.Dob;
                    dtls.Gender = canDtls.Gender;
                    dtls.MaritalStatus = canDtls.MaritalStatus;
                    dtls.GenderName = canDtls.Gender > 0 ? dbContext.PhRefMasters.Where(da => da.Id == canDtls.Gender).Select(da => da.Rmvalue).FirstOrDefault() : "";
                    dtls.MaritalStatusName = canDtls.MaritalStatus > 0 ? dbContext.PhRefMasters.Where(da => da.Id == canDtls.MaritalStatus).Select(da => da.Rmvalue).FirstOrDefault() : "";
                    dtls.NationalityName = canDtls.Nationality > 0 ? Countrys.Where(da => da.Id == canDtls.Nationality).Select(da => da.Nicename).FirstOrDefault() : "";
                    dtls.Nationality = canDtls.Nationality;

                    dtls.MobileNo = canDtls.ContactNo;
                }
            }

            return dtls;
        }


        public async Task<GetResponseViewModel<FileURLViewModel>> DownloadCandidateBGVForm(int CandId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<FileURLViewModel>();
            var fileDownloadViewModel = new FileDownloadViewModel();
            var fileURLViewModel = new FileURLViewModel();
            int UserId = Usr.Id;
            try
            {


                var dtls = await CandidateBGVDtls(CandId, JobId);
                var pudtls = await dbContext.TblParamProcessUnitMasters.Where(x => x.Id == dtls.PuId).FirstOrDefaultAsync();

                if (dtls.BgacceptFlag.Value)
                {
                    string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + CandId + "\\temp";
                    string fileName = JobId + "_" + CurrentTime.ToString("yyyyMMddHHmmss") + dtls.FirstName + "_" + dtls.LastName + "_BGV.pdf";

                    fileName = fileName.Replace(" ", "_");

                    string fileLocation = System.IO.Path.Combine(webRootPath, fileName);
                    if (!Directory.Exists(webRootPath))
                    {
                        Directory.CreateDirectory(webRootPath);
                        FileStream file1 = File.Create(fileLocation);
                        file1.Write(fileDownloadViewModel.File, 0, fileDownloadViewModel.File.Length);
                        file1.Close();
                    }

                    fileURLViewModel.FileURL = _environment.ContentRootPath + "/Candidate/" + CandId + "/" + fileName;
                    fileURLViewModel.FileName = fileName;
                    fileURLViewModel.FileType = "application/pdf";

                    var messageBody = GetBGVMessageBody(dtls);

                    string imageUrl = pudtls?.Logo;
                    string address = string.Empty;
                    string address1 = string.Empty;

                    var addDtls = await dbContext.GetCompanyLocation(dtls.PuId.Value);
                    if (addDtls.Count > 0)
                    {
                        var lctnDtls = addDtls[0];
                        if (lctnDtls != null)
                        {
                            address = lctnDtls.address1 + ", " + lctnDtls.address2 + ", " + lctnDtls.address3 + ", " + lctnDtls.city_name + ", " + lctnDtls.country + ".";
                            address1 = " " + lctnDtls.mobile_number + " " + lctnDtls.website;
                        }
                    }

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
                                HtmlConverter.ConvertToPdf(messageBody.Item1, pdf, converterProperties);

                                PdfDocument pdfDocument = new PdfDocument(new PdfReader(fileLocation), new PdfWriter(fileURLViewModel.FileURL));
                                Document doc = new Document(pdfDocument, PageSize.A4);

                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    // Create an event handler to add the logo to the header
                                    pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new CustomEventHandler(imageUrl));
                                }

                                PdfFont font = PdfFontFactory.CreateFont("Helvetica");
                                int numberOfPages = pdfDocument.GetNumberOfPages();
                                for (int i = 1; i <= numberOfPages; i++)
                                {
                                    if (!string.IsNullOrEmpty(address))
                                    {
                                        doc.ShowTextAligned(new Paragraph(address).SetFont(font).SetFontSize(9),
                                     300, 45, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
                                    }

                                    if (!string.IsNullOrEmpty(address1))
                                    {
                                        doc.ShowTextAligned(new Paragraph(address1).SetFont(font).SetFontSize(9),
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
                        // checking for folder is available or not 
                        if (!Directory.Exists(webRootPath))
                        {
                            Directory.CreateDirectory(webRootPath);
                        }
                        FileStream file = File.Create(fileLocation);
                        file.Write(fileDownloadViewModel.File, 0, fileDownloadViewModel.File.Length);
                        file.Close();
                    }

                    fileURLViewModel.FileURL = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + CandId + "/" + fileName;
                    respModel.SetResult(fileURLViewModel);
                    respModel.Status = true;

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    //audit
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Download offer letter",
                        ActivityDesc = " downloaded BGV Form for " + dtls.FirstName + " " + dtls.LastName + "",
                        ActivityType = (byte)AuditActivityType.Other,
                        TaskID = CandId,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    //activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = dtls.CandProfId,
                        JobId = JobId,
                        ActivityType = (byte)LogActivityType.Other,
                        ActivityDesc = " has download BGV Form for " + dtls.FirstName + " " + dtls.LastName + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                }
                else
                {
                    string message = "Candidate BGV Details are not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }

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
                float logoY = docEvent.GetPage().GetPageSize().GetHeight() - 75;

                // Add the logo to the Canvas
                canvas.AddImageAt(imageData, logoX, logoY, false);
            }
        }


        public async Task<GetResponseViewModel<FileURLViewModel>> DownloadCandidateAcknowledgementForm(AcknowledgementDwndViewModel acknowledgementDwndViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<FileURLViewModel>();
            var fileDownloadViewModel = new FileDownloadViewModel();
            var fileURLViewModel = new FileURLViewModel();
            int UserId = Usr.Id;
            try
            {
                Guid g = Guid.NewGuid();
                string guId = g.ToString();



                var dtls = GetAcknowledgementMessageBody(acknowledgementDwndViewModel);
                var jobDtls = await dbContext.PhJobOpeningsAddlDetails.Where(x => x.Joid == acknowledgementDwndViewModel.JobId).FirstOrDefaultAsync();
                var pudtls = await dbContext.TblParamProcessUnitMasters.Where(x => x.Id == jobDtls.Puid.Value).FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(dtls.Item1))
                {
                    string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + acknowledgementDwndViewModel.CandId + "\\temp";
                    string fileName = CurrentTime.ToString("yyyyMMddHHmmss") + "_" + dtls.Item2 + "_Acknowledgement.pdf";
                    fileName = fileName.Replace(" ", "_");

                    string fileLocation = System.IO.Path.Combine(webRootPath, fileName);
                    if (!Directory.Exists(webRootPath))
                    {
                        Directory.CreateDirectory(webRootPath);
                        FileStream file1 = File.Create(fileLocation);
                        file1.Write(fileDownloadViewModel.File, 0, fileDownloadViewModel.File.Length);
                        file1.Close();
                    }

                    fileURLViewModel.FileURL = _environment.ContentRootPath + "/Candidate/" + acknowledgementDwndViewModel.CandId + "/" + fileName;
                    fileURLViewModel.FileName = fileName;
                    fileURLViewModel.FileType = "application/pdf";


                    string imageUrl = pudtls?.Logo;
                    string address = string.Empty;
                    string address1 = string.Empty;

                    var addDtls = await dbContext.GetCompanyLocation(jobDtls.Puid.Value);
                    if (addDtls.Count > 0)
                    {
                        var lctnDtls = addDtls[0];
                        if (lctnDtls != null)
                        {
                            address = lctnDtls.address1 + ", " + lctnDtls.address2 + ", " + lctnDtls.address3 + ", " + lctnDtls.city_name + ", " + lctnDtls.country + ".";
                            address1 = " " + lctnDtls.mobile_number + " " + lctnDtls.website;
                        }
                    }

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
                                HtmlConverter.ConvertToPdf(dtls.Item1, pdf, converterProperties);

                                PdfDocument pdfDocument = new PdfDocument(new PdfReader(fileLocation), new PdfWriter(fileURLViewModel.FileURL));
                                Document doc = new Document(pdfDocument, PageSize.A4);

                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    // Create an event handler to add the logo to the header
                                    pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new CustomEventHandler(imageUrl));
                                }

                                PdfFont font = PdfFontFactory.CreateFont("Helvetica");
                                int numberOfPages = pdfDocument.GetNumberOfPages();
                                for (int i = 1; i <= numberOfPages; i++)
                                {
                                    if (!string.IsNullOrEmpty(address))
                                    {
                                        doc.ShowTextAligned(new Paragraph(address).SetFont(font).SetFontSize(9),
                                     300, 45, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
                                    }

                                    if (!string.IsNullOrEmpty(address1))
                                    {
                                        doc.ShowTextAligned(new Paragraph(address1).SetFont(font).SetFontSize(9),
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
                        // checking for folder is available or not 
                        if (!Directory.Exists(webRootPath))
                        {
                            Directory.CreateDirectory(webRootPath);
                        }
                        FileStream file = File.Create(fileLocation);
                        file.Write(fileDownloadViewModel.File, 0, fileDownloadViewModel.File.Length);
                        file.Close();
                    }

                    fileURLViewModel.FileURL = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + acknowledgementDwndViewModel.CandId + "/" + fileName;
                    respModel.SetResult(fileURLViewModel);
                    respModel.Status = true;


                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    //audit
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Download offer letter",
                        ActivityDesc = " downloaded Acknowledgement letter for " + dtls.Item2 + "",
                        ActivityType = (byte)AuditActivityType.Other,
                        TaskID = acknowledgementDwndViewModel.CandId,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    //activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = acknowledgementDwndViewModel.CandId,
                        JobId = acknowledgementDwndViewModel.JobId,
                        ActivityType = (byte)LogActivityType.Other,
                        ActivityDesc = " has download Acknowledgement letter for " + dtls.Item2 + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                }
                else
                {
                    string message = "Candidate Details are not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(acknowledgementDwndViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public Tuple<string> GetBGVMessageBody(CandidateBGVViewModel candidateBGVViewModel)
        {
            DateTime dt = CurrentTime;
            string messageBody = string.Empty;

            messageBody = File.ReadAllText("EmailTemplates/CandidateBGVform.html");
            messageBody = messageBody.Replace("!BGVSummitedon", dt.ToString("dd/MM/yyyy"));

            messageBody = messageBody.Replace("!CandidateName", candidateBGVViewModel.FirstName + " " + candidateBGVViewModel.LastName);
            messageBody = messageBody.Replace("!CandidateProfId", candidateBGVViewModel.CandProfId.ToString());
            messageBody = messageBody.Replace("!firstName", candidateBGVViewModel.FirstName);
            messageBody = messageBody.Replace("!lastName", candidateBGVViewModel.LastName);
            messageBody = messageBody.Replace("!otherName", candidateBGVViewModel.OtherName);
            if (candidateBGVViewModel.DateOfBirth != null)
            {
                messageBody = messageBody.Replace("!dateOfBirth", candidateBGVViewModel.DateOfBirth.Value.ToString("dd/MM/yyyy"));
            }
            else
            {
                messageBody = messageBody.Replace("!dateOfBirth", string.Empty);
            }
            messageBody = messageBody.Replace("!placeOfBirth", candidateBGVViewModel.PlaceOfBirth);
            messageBody = messageBody.Replace("!bloodGroupName", candidateBGVViewModel.BloodGroupName);

            //if (!string.IsNullOrEmpty(candidateBGVViewModel.GenderName))
            //{
            //    string gender;
            //    if (candidateBGVViewModel.Gender.ToLower() == "m")
            //    {
            //        gender = "Male";
            //    }
            //    else if (candidateBGVViewModel.Gender.ToLower() == "f")
            //    {
            //        gender = "Female";
            //    }
            //    else
            //    {
            //        gender = "Other";
            //    }
            //    messageBody = messageBody.Replace("!gender", gender);
            //}
            //if (!string.IsNullOrEmpty(candidateBGVViewModel.MaritalStatus))
            //{
            //    string maritalStatus;
            //    if (candidateBGVViewModel.MaritalStatus.ToLower() == "s")
            //    {
            //        maritalStatus = "Single";
            //    }
            //    else if (candidateBGVViewModel.MaritalStatus.ToLower() == "m")
            //    {
            //        maritalStatus = "Married";
            //    }
            //    else
            //    {
            //        maritalStatus = "Divorce";
            //    }
            //    messageBody = messageBody.Replace("!maritalStatus", maritalStatus);
            //}
            messageBody = messageBody.Replace("!gender", candidateBGVViewModel.GenderName);
            messageBody = messageBody.Replace("!maritalStatus", candidateBGVViewModel.MaritalStatusName);
            messageBody = messageBody.Replace("!nationalityName", candidateBGVViewModel.NationalityName);
            messageBody = messageBody.Replace("!expeInYears", candidateBGVViewModel.ExpeInYears.ToString());
            messageBody = messageBody.Replace("!mobileNo", candidateBGVViewModel.MobileNo);
            messageBody = messageBody.Replace("!emailAddress", candidateBGVViewModel.EmailId);
            messageBody = messageBody.Replace("!emiratesId", candidateBGVViewModel.EmiratesId);
            if (candidateBGVViewModel.PPExpiryDate != null)
            {
                messageBody = messageBody.Replace("!ppExpiryDate", candidateBGVViewModel.PPExpiryDate.Value.ToString("dd/MM/yyyy"));
            }
            else
            {
                messageBody = messageBody.Replace("!ppExpiryDate", string.Empty);
            }

            messageBody = messageBody.Replace("!ppnumber", candidateBGVViewModel.Ppnumber);
            messageBody = messageBody.Replace("!undergoinganyMedicalTreatment", candidateBGVViewModel.UGMedicalTreaFlag == true ? "YES" : "NO");

            if (candidateBGVViewModel.UGMedicalTreaFlag == true)
            {
                string ugMedicalTreaDetailTrTag = "<tr> <td> Medical Treatment Details </td> <td> !ugMedicalTreaDetail </td> </tr> ";
                ugMedicalTreaDetailTrTag = ugMedicalTreaDetailTrTag.Replace("!ugMedicalTreaDetail", candidateBGVViewModel.UGMedicalTreaDetails);
                messageBody = messageBody.Replace("!ugMedicalTrDetailTr", ugMedicalTreaDetailTrTag);
            }


            messageBody = messageBody.Replace("!spouseName", candidateBGVViewModel.SpouseName);
            messageBody = messageBody.Replace("!fatherName", candidateBGVViewModel.FatherName);
            messageBody = messageBody.Replace("!noOfKids", candidateBGVViewModel.NoOfKids.ToString());
            messageBody = messageBody.Replace("!motherName", candidateBGVViewModel.MotherName);
            messageBody = messageBody.Replace("!emerContactPerson", candidateBGVViewModel.EmerContactPerson);
            messageBody = messageBody.Replace("!emerContactRelation", candidateBGVViewModel.EmerContactRelation);
            messageBody = messageBody.Replace("!emerContactNo", candidateBGVViewModel.EmerContactNo);
            messageBody = messageBody.Replace("!homePhone", candidateBGVViewModel.HomePhone);

            messageBody = messageBody.Replace("!permAddrCountryName", candidateBGVViewModel.PermAddrCountryName);
            messageBody = messageBody.Replace("!permAddrCityName", candidateBGVViewModel.PermAddrCityName);
            messageBody = messageBody.Replace("!permAddrLandMark", candidateBGVViewModel.PermAddrLandMark);
            messageBody = messageBody.Replace("!permAddrContactPerson", candidateBGVViewModel.PermAddrContactPerson);
            messageBody = messageBody.Replace("!permAddrResiTypeName", candidateBGVViewModel.PermAddrResiTypeName);
            messageBody = messageBody.Replace("!permAddress", candidateBGVViewModel.PermAddress);
            messageBody = messageBody.Replace("!permAddrContactNo", candidateBGVViewModel.PermAddrContactNo);

            if (candidateBGVViewModel.PermAddrResiSince != null)
            {
                messageBody = messageBody.Replace("!permAddrResiSince", candidateBGVViewModel.PermAddrResiSince.Value.ToString("dd/MM/yyyy"));
            }
            else
            {
                messageBody = messageBody.Replace("!permAddrResiSince", string.Empty);
            }
            if (candidateBGVViewModel.PermAddrResiTill != null)
            {
                messageBody = messageBody.Replace("!permAddrResiTill", candidateBGVViewModel.PermAddrResiTill.Value.ToString("dd/MM/yyyy"));
            }
            else
            {
                messageBody = messageBody.Replace("!permAddrResiTill", string.Empty);
            }


            messageBody = messageBody.Replace("!presAddrCountryName", candidateBGVViewModel.PresAddrCountryName);
            messageBody = messageBody.Replace("!presAddrCityName", candidateBGVViewModel.PresAddrCityName);
            messageBody = messageBody.Replace("!presAddrLandMark", candidateBGVViewModel.PresAddrLandMark);
            messageBody = messageBody.Replace("!presAddrResiTypeName", candidateBGVViewModel.PresAddrResiTypeName);
            messageBody = messageBody.Replace("!presAddrContactPerson", candidateBGVViewModel.PresAddrContactPerson);
            messageBody = messageBody.Replace("!presAddress", candidateBGVViewModel.PresAddress);
            messageBody = messageBody.Replace("!presAddrContactNo", candidateBGVViewModel.PresAddrContactNo);

            if (candidateBGVViewModel.PresAddrResiSince != null)
            {
                messageBody = messageBody.Replace("!presAddrResiSince", candidateBGVViewModel.PresAddrResiSince.Value.ToString("dd/MM/yyyy"));
            }
            else
            {
                messageBody = messageBody.Replace("!presAddrResiSince", string.Empty);
            }

            if (!string.IsNullOrEmpty(candidateBGVViewModel.PresAddrPrefTimeForVerification))
            {
                messageBody = messageBody.Replace("!presAddrPrefTimeForVerification", " Preferred time of the day for conducting the verification, if any  :" + candidateBGVViewModel.PresAddrPrefTimeForVerification);
            }
            else
            {
                messageBody = messageBody.Replace("!presAddrPrefTimeForVerification", " <br />");
            }

            string employementRecords = string.Empty;
            string employementRecord = "<tr> <td> !employerNam </td> <td> !address </td> <td> !countryName </td> <td> !cityName </td> <td> !emptFromDate </td>  <td> !emptToDate </td>  <td> !employId </td> <td> !currentlyworking </td> </tr>";
            foreach (var item in candidateBGVViewModel.CandidateBGVEmployeeViewModel)
            {
                string empCopy = employementRecord;
                empCopy = empCopy.Replace("!employerNam", item.EmployerName);
                empCopy = empCopy.Replace("!address", item.Address);
                empCopy = empCopy.Replace("!countryName", item.CountryName);
                empCopy = empCopy.Replace("!cityName", item.CityName);
                if (item.EmptFromDate != null)
                {
                    empCopy = empCopy.Replace("!emptFromDate", item.EmptFromDate.Value.ToString("dd/MM/yyyy"));
                }
                else
                {
                    empCopy = empCopy.Replace("!emptFromDate", string.Empty);
                }
                if (item.EmptToDate != null)
                {
                    empCopy = empCopy.Replace("!emptToDate", item.EmptToDate.Value.ToString("dd/MM/yyyy"));
                }
                else
                {
                    empCopy = empCopy.Replace("!emptToDate", string.Empty);
                }

                empCopy = empCopy.Replace("!employId", item.EmployId);
                empCopy = empCopy.Replace("!currentlyworking", item.CurrentWorkingFlag == true ? "YES" : "NO");
                employementRecords += empCopy;
            }
            messageBody = messageBody.Replace("!employementRecords", employementRecords);

            string educationRecords = string.Empty;
            string educationRecord = "<tr><td> !qualification </td> <td> !course </td> <td> !univOrInstitution </td> <td> !durationFro </td> <td> !durationTo </td> <td> !percentage </td> </tr>";
            foreach (var item in candidateBGVViewModel.CandidateBGVEduViewModel)
            {
                string eduCopy = educationRecord;
                eduCopy = eduCopy.Replace("!qualification", item.Qualification);
                eduCopy = eduCopy.Replace("!course", item.Course);
                eduCopy = eduCopy.Replace("!univOrInstitution", item.UnivOrInstitution);
                if (item.DurationFrom != null)
                {
                    eduCopy = eduCopy.Replace("!durationFro", item.DurationFrom.Value.ToString("dd/MM/yyyy"));
                }
                else
                {
                    eduCopy = eduCopy.Replace("!durationFro", string.Empty);
                }
                if (item.DurationTo != null)
                {
                    eduCopy = eduCopy.Replace("!durationTo", item.DurationTo.Value.ToString("dd/MM/yyyy"));
                }
                else
                {
                    eduCopy = eduCopy.Replace("!durationTo", string.Empty);
                }
                eduCopy = eduCopy.Replace("!percentage", item.Percentage.ToString());
                educationRecords += eduCopy;
            }
            messageBody = messageBody.Replace("!educationRecords", educationRecords);

            return Tuple.Create(messageBody);
        }

        public Tuple<string, string> GetAcknowledgementMessageBody(AcknowledgementDwndViewModel acknowledgementDwndViewModel)
        {
            DateTime dt = CurrentTime;
            string messageBody = string.Empty;
            string candName = string.Empty;

            var candidateDtls = dbContext.PhCandidateBgvDetails.Where(x => x.CandProfId == acknowledgementDwndViewModel.CandId && x.BgcompStatus == (byte)BGCompStatus.Completed).FirstOrDefault();
            if (candidateDtls != null)
            {
                if (acknowledgementDwndViewModel.ReceiptsViewModel != null)
                {
                    candName = candidateDtls.FirstName + " " + candidateDtls.LastName;
                    messageBody = File.ReadAllText("EmailTemplates/CandidateAcknowledgement.html");

                    messageBody = messageBody.Replace("!date", dt.ToString("dd/MM/yyyy"));
                    messageBody = messageBody.Replace("!candidateName", candidateDtls.FirstName + " " + candidateDtls.LastName);
                    messageBody = messageBody.Replace("!todaysDate", dt.ToString("dd/MM/yyyy"));

                    string PassportTr = string.Empty;
                    string EducationTr = string.Empty;
                    if (acknowledgementDwndViewModel.ReceiptsViewModel != null)
                    {
                        string pTrStartTag = "<tr>  <td colspan='2'> <p>  !ppdiv </p> </td> </tr>";
                        var isPassAval = acknowledgementDwndViewModel.ReceiptsViewModel.Where(x => x.ReceiptType == 1).FirstOrDefault();
                        if (isPassAval != null)
                        {
                            if (candidateDtls.PpexpiryDate != null)
                            {
                                pTrStartTag = pTrStartTag.Replace("!ppdiv", "Passport # " + candidateDtls.Ppnumber + ", Valid till " + candidateDtls.PpexpiryDate.Value.ToString("dd/MM/yyyy") + "");
                            }
                            else
                            {
                                pTrStartTag = pTrStartTag.Replace("!ppdiv", string.Empty);
                            }
                            messageBody = messageBody.Replace("!passportRecord", pTrStartTag);
                        }
                    }

                    if (acknowledgementDwndViewModel.ReceiptsViewModel != null)
                    {
                        string eTrStartTag = "<tr>  <td colspan='2'> !edutndiv </td> </tr>";
                        string qultCourseMultiTag = "";
                        string qultCourseSingleTag = "<p> !qualification : !course </p>";
                        var bgvEductnDtls = dbContext.PhCandidateEduDetails.Where(x => x.CandProfId == candidateDtls.CandProfId).ToList();
                        foreach (var item in acknowledgementDwndViewModel.ReceiptsViewModel)
                        {
                            if (item.ReceiptType != 1) // 1 - passportNumber, 2 - Education
                            {
                                string copyQultCourseSingleTag = qultCourseSingleTag;
                                var bgvEductnDtl = bgvEductnDtls.Where(x => x.Id == item.ReceiptId).FirstOrDefault();
                                if (bgvEductnDtls != null)
                                {
                                    copyQultCourseSingleTag = copyQultCourseSingleTag.Replace("!qualification", bgvEductnDtl.Qualification);
                                    copyQultCourseSingleTag = copyQultCourseSingleTag.Replace("!course", bgvEductnDtl.Course);
                                }
                                qultCourseMultiTag = qultCourseMultiTag + copyQultCourseSingleTag;
                            }
                        }
                        eTrStartTag = eTrStartTag.Replace("!edutndiv", qultCourseMultiTag);
                        messageBody = messageBody.Replace("!educationRecords", eTrStartTag);
                    }
                }

            }

            return Tuple.Create(messageBody, candName);
        }

        public async Task<GetResponseViewModel<CandidateEmploymentEducationCertificationViewModel>> GetCandidateEmpEduCertVDtls(int CanId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<CandidateEmploymentEducationCertificationViewModel>();
            string message = string.Empty;

            try
            {

                var dtls = new CandidateEmploymentEducationCertificationViewModel();


                var Countrys = dbContext.PhCountries.ToList();
                var Cities = dbContext.PhCities.ToList();

                var eduDtls = await dbContext.PhCandidateEduDetails.Where(x => x.CandProfId == CanId && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                if (eduDtls.Count > 0)
                {
                    dtls.CandidateEduViewModel = new List<CandidateBGVEduViewModel>();
                    foreach (var item in eduDtls)
                    {
                        var model = new CandidateBGVEduViewModel
                        {
                            Id = item.Id,
                            QualificationId = item.QualificationId,
                            Qualification = item.Qualification,
                            Course = item.Course,
                            CourseId = item.CourseId,
                            UnivOrInstitution = item.UnivOrInstitution,
                            YearofPassing = item.YearofPassing,
                            DurationFrom = item.DurationFrom,
                            DurationTo = item.DurationTo,
                            Grade = item.Grade,
                            Percentage = item.Percentage
                        };
                        dtls.CandidateEduViewModel.Add(model);
                    }
                }

                dtls.CandidateCertificationViewModel = new List<CandidateCertificationViewModel>();
                dtls.CandidateCertificationViewModel = (from Certifications in dbContext.PhCandidateCertifications
                                                        join refData in dbContext.PhRefMasters on Certifications.CertificationId equals refData.Id
                                                        where Certifications.CandProfId == CanId && Certifications.Status != (byte)RecordStatus.Delete
                                                        select new CandidateCertificationViewModel
                                                        {
                                                            CertificationId = Certifications.CertificationId,
                                                            CertificationName = refData.Rmvalue
                                                        }).ToList();

                var empDtls = dbContext.PhCandidateEmpmtDetails.Where(x => x.CandProfId == CanId && x.Status != (byte)RecordStatus.Delete).ToList();
                if (empDtls.Count > 0)
                {
                    dtls.CandidateEmpMentModel = new List<CandidateBGVEmployeeViewModel>();
                    foreach (var item in empDtls)
                    {
                        var model = new CandidateBGVEmployeeViewModel
                        {
                            Id = item.Id,
                            Address = item.Address,
                            CityId = item.CityId,
                            CityName = item.CityId != null ? Cities.Where(x => x.Id == item.CityId).Select(x => x.Name).FirstOrDefault() : string.Empty,
                            CountryId = item.CountryId,
                            CountryName = item.CountryId != null ? Countrys.Where(x => x.Id == item.CountryId).Select(x => x.Name).FirstOrDefault() : string.Empty,
                            Cpdesignation = item.Cpdesignation,
                            CpemailId = item.CpemailId,
                            Cpname = item.Cpname,
                            Cpnumber = item.Cpnumber,
                            CurrentWorkingFlag = item.CurrentWorkingFlag,
                            Designation = item.Designation,
                            DesignationId = item.DesignationId,
                            EmployerName = item.EmployerName,
                            EmployId = item.EmployId,
                            EmptFromDate = item.EmptFromDate,
                            EmptToDate = item.EmptToDate,
                            OfficialEmailId = item.OfficialEmailId,
                            HrcontactNo = item.HrcontactNo,
                            HremailId = item.HremailId,
                            PhoneNumber = item.PhoneNumber
                        };
                        dtls.CandidateEmpMentModel.Add(model);
                    }
                }



                respModel.SetResult(dtls);
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


        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ConvertToEmployee(int CandId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = string.Empty;

            try
            {

                var dtls = new CandidateToEmployeeViewModel();
                var status = dbContext.PhCandStatusSes.ToList();

                var UserDtls = await dbContext.PiHireUsers.Where(x => x.UserType != (byte)UserType.Candidate).ToListAsync();
                var bgvDtls = dbContext.PhCandidateBgvDetails.Where(x => x.CandProfId == CandId && x.BgcompStatus == (byte)BGCompStatus.Completed).FirstOrDefault();

                if (bgvDtls != null)
                {
                    var _candEmpId = await dbContext.PiHireUsers.Where(x => x.UserId == CandId).Select(x => x.EmployId).FirstOrDefaultAsync();
                    if (_candEmpId == null || _candEmpId == 0)
                    {
                        var candDtls = await (from canJob in dbContext.PhJobCandidates
                                              join canPrf in dbContext.PhCandidateProfiles
                                              on canJob.CandProfId equals canPrf.Id
                                              join job in dbContext.PhJobOpenings on canJob.Joid equals job.Id
                                              join jobDtls in dbContext.PhJobOpeningsAddlDetails
                                              on job.Id equals jobDtls.Joid
                                              join cunty in dbContext.PhCountries on job.CountryId equals cunty.Id
                                              where canJob.CandProfId == CandId && canJob.Joid == JobId
                                              select new
                                              {
                                                  jobDtls.Puid,
                                                  jobDtls.Buid,
                                                  job.BroughtBy,
                                                  canJob.RecruiterId,
                                                  canPrf.EmailId,
                                                  canJob.CandProfStatus,
                                                  cunty.Name
                                              }).FirstOrDefaultAsync();

                        int prejoinsScuess = status.Where(x => x.Cscode == "PNS").Select(x => x.Id).FirstOrDefault();
                        if (candDtls.CandProfStatus == prejoinsScuess)
                        {
                            dtls.CandSkills = new List<CandSkills>();
                            dtls.CandSkills = await (from canSkill in dbContext.PhCandidateSkillsets
                                                     join skill in dbContext.PhTechnologysSes on canSkill.TechnologyId
                                                     equals skill.Id
                                                     where canSkill.CandProfId == CandId
                                                     && canSkill.Status == (byte)RecordStatus.Active
                                                     select new CandSkills
                                                     {
                                                         Title = skill.Title,
                                                         ExpInYears = canSkill.ExpInYears,
                                                         ExpInMonths = canSkill.ExpInMonths,
                                                         SelfRating = canSkill.SelfRating
                                                     }).ToListAsync();
                            dtls.SkillSet = string.Join(", ", dtls.CandSkills.Select(c => c.Title));


                            dtls.CandDocuments = new List<CandDocuments>();
                            dtls.CandDocuments = await (from canDoc in dbContext.PhCandidateDocs
                                                        where canDoc.CandProfId == CandId &&
                                                        canDoc.Joid == JobId &&
                                                        canDoc.FileName != null
                                                        select new CandDocuments
                                                        {
                                                            Id = canDoc.Id,
                                                            FileGroup = canDoc.FileGroup,
                                                            FileType = canDoc.FileType,
                                                            FileName = canDoc.FileName,
                                                            DocType = canDoc.DocType
                                                        }).OrderByDescending(x => x.Id).ToListAsync();
                            foreach (var item in dtls.CandDocuments)
                            {
                                if (!string.IsNullOrEmpty(item.FileName))
                                {
                                    item.FileName = item.FileName.Replace("#", "%23");
                                    item.FileName = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + CandId + "/" + item.FileName;
                                }
                            }

                            var refData = dbContext.PhRefMasters.ToList();

                            dtls.FirstName = bgvDtls.FirstName;
                            dtls.LastName = bgvDtls.LastName;
                            dtls.Middlename = bgvDtls.MiddleName;
                            dtls.Gender = refData.FirstOrDefault(da => da.Id == bgvDtls.Gender)?.Rmvalue?.Substring(0, 1); // Required
                            dtls.DateOfBirth = bgvDtls.DateOfBirth; // Required
                            dtls.PlaceOfBirth = bgvDtls.PlaceOfBirth; // Required
                            dtls.FatherName = bgvDtls.FatherName;
                            dtls.MotherName = bgvDtls.MotherName;
                            dtls.MaritalStatus = refData.FirstOrDefault(da => da.Id == bgvDtls.MaritalStatus)?.Rmvalue?.Substring(0, 1); // Required
                            dtls.NoOfKids = bgvDtls.NoOfKids;
                            dtls.SpouseName = bgvDtls.SpouseName;
                            dtls.BloodGroup = bgvDtls.BloodGroup;
                            dtls.Nationality = bgvDtls.Nationality;
                            dtls.PassportNumber = bgvDtls.Ppnumber;
                            dtls.PassportValidTill = bgvDtls.PpexpiryDate;
                            dtls.JobLocation = candDtls?.Name; // country name


                            // Contact
                            dtls.EmailId = candDtls.EmailId;
                            dtls.MobileNum = bgvDtls.MobileNo;
                            dtls.EmergencyContactNum = bgvDtls.EmerContactNo;
                            dtls.EmergencyContactPerson = bgvDtls.EmerContactPerson;
                            dtls.ContactPerson = bgvDtls.EmerContactPerson;
                            dtls.HomePhone = bgvDtls.HomePhone;

                            // Present Address 
                            dtls.PresentAddress = bgvDtls.PresAddress;
                            dtls.PresentAddressCity = bgvDtls.PresAddrCityId;
                            dtls.PresentCountry = bgvDtls.PresAddrCountryId;
                            if (dtls.PresentCountry == null)
                            {
                                message = "Present Country is not available";
                                respModel.SetResult(message);
                                respModel.Status = false;
                                return Tuple.Create(notificationPushedViewModel, respModel);
                            }
                            dtls.PresentResidingSince = bgvDtls.PresAddrResiSince;
                            dtls.PresentResidingTill = null;
                            dtls.PresentAddressLandMark = bgvDtls.PresAddrLandMark;
                            dtls.PresentContactPerson = bgvDtls.PresAddrContactNo;
                            dtls.PermPinCode = bgvDtls.PermPinCode;
                            dtls.PresPinCode = bgvDtls.PresPinCode;

                            // Present Address 
                            dtls.PermanentAddress = bgvDtls.PermAddress;
                            dtls.PermanentAddressCity = bgvDtls.PermAddrCityId;
                            dtls.PermanentCountry = bgvDtls.PermAddrCountryId;
                            if (dtls.PermanentCountry == null)
                            {
                                message = "Permanent Country is not available";
                                respModel.SetResult(message);
                                respModel.Status = false;
                                return Tuple.Create(notificationPushedViewModel, respModel);
                            }
                            dtls.PermanentResidingSince = bgvDtls.PermAddrResiSince;
                            dtls.PermanentResidingTill = bgvDtls.PermAddrResiTill;
                            dtls.PermanentAddressLandMark = bgvDtls.PermAddrLandMark;
                            dtls.PermanentContactPerson = bgvDtls.PermAddrContactNo;

                            dtls.PermanentAddressCityName = dbContext.PhCities.Where(x => x.Id == bgvDtls.PermAddrCityId).Select(x => x.Name).FirstOrDefault();

                            dtls.AnotherName = bgvDtls.AnotherName;
                            dtls.EmiratesId = bgvDtls.EmiratesId;
                            dtls.ProcessUnit = candDtls.Puid;
                            dtls.BusinessUnit = candDtls.Buid;

                            dtls.BroughtBy = candDtls.BroughtBy;
                            dtls.RecruiterId = candDtls.RecruiterId;

                            // Gateway Id
                            dtls.UpdatedBy = UserDtls.Where(x => x.Id == UserId).Select(x => x.EmployId).FirstOrDefault();

                            dtls.Candoffers = new List<CTECandidateOffers>();
                            var Offers = dbContext.PhJobOfferLetters.Where(x => x.CandProfId == CandId && x.Joid == JobId).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                            if (Offers != null)
                            {
                                dtls.JoiningDate = Offers.JoiningDate;
                                dtls.DepartmentId = Offers.DepartmentId;
                                dtls.SpecializationId = Offers.SpecId;
                                dtls.EmployeeType = Offers.EmployeeType;
                            }
                            else
                            {
                                message = "Candidate not having intent offer, to proceed";
                                respModel.SetResult(message);
                                respModel.Status = false;
                                return Tuple.Create(notificationPushedViewModel, respModel);
                            }
                            if (dtls.JoiningDate == null)
                            {
                                message = "Joining Date is not available, check once in intent release";
                                respModel.SetResult(message);
                                respModel.Status = false;
                                return Tuple.Create(notificationPushedViewModel, respModel);
                            }
                            if (dtls.DepartmentId == null)
                            {
                                message = "Department is not available, check once in intent release";
                                respModel.SetResult(message);
                                respModel.Status = false;
                                return Tuple.Create(notificationPushedViewModel, respModel);
                            }
                            if (dtls.SpecializationId == null)
                            {
                                message = "Specialization is not available, check once in intent release";
                                respModel.SetResult(message);
                                respModel.Status = false;
                                return Tuple.Create(notificationPushedViewModel, respModel);
                            }

                            var UpdateUserDtls = UserDtls.Where(x => x.Id == Offers?.CreatedBy).FirstOrDefault();
                            var SignatureAuthoriy = UserDtls.Where(x => x.Id == Offers?.SignatureAuthority).Select(x => x.EmployId).FirstOrDefault();

                            if (SignatureAuthoriy != null && UpdateUserDtls != null && UpdateUserDtls.EmployId != 0)
                            {
                                double AnulBasic = Offers.GrossSalaryPerAnnum.Value / 2;
                                double MonthBasic = AnulBasic / 12;
                                var basic = Convert.ToInt32(Math.Round(MonthBasic, 0, MidpointRounding.AwayFromZero));
                                var cTECandidateOffers = new CTECandidateOffers
                                {
                                    IntentOfferId = Offers.Id,
                                    Basic = basic,
                                    Hra = Offers?.Hra,
                                    Conveyance = Offers.Conveyance,
                                    Otbonus = Offers.Otbonus,
                                    Sickness = Offers.Sickness,
                                    Gratuity = Offers.Gratuity,
                                    TotalNet = Offers.GrossSalary, // Monthly Net
                                    TotalGrass = Offers.GrossSalaryPerAnnum, // Monthly Net * 12
                                    UpdateDate = Offers.UpdatedDate == null ? CurrentTime : Offers.UpdatedDate,
                                    JoiningDate = Offers.JoiningDate,
                                    DesignationId = Offers.DesignationId,
                                    DepartmentId = Offers.DepartmentId,
                                    SpecializationId = Offers.SpecId,
                                    CompanyDisplayName = refData.Where(x => x.Id == Offers.CompanyId).Select(x => x.Rmvalue).FirstOrDefault(),
                                    Currency = Offers.CurrencyId,
                                    UpdatedBy = UpdateUserDtls?.EmployId,// GW Employee Id
                                    UpdatedByName = UpdateUserDtls?.FirstName + " " + UpdateUserDtls?.LastName, // GW Employee Name
                                    EmployeeDesignation = SignatureAuthoriy, // GW Signature Authority (Employee Id)                                       
                                    FileName = Offers.FileName,
                                    FileType = Offers.FileType,
                                    FileURL = Offers.FileUrl
                                };
                                dtls.Candoffers.Add(cTECandidateOffers);

                                dtls.AllowanceDetails = new List<CTEAllowanceDetails>();
                                var AllowanceDtls = dbContext.PhJobOfferAllowances.Where(x => x.CandProfId == CandId
                                && x.Joid == JobId && x.Id == Offers.Id).OrderByDescending(x => x.CreatedDate).ToList();
                                foreach (var item in AllowanceDtls)
                                {
                                    var cTEAllowanceDetails = new CTEAllowanceDetails
                                    {
                                        AllowDescription = item.AllowanceTitle,
                                        Amount = item.Amount
                                    };
                                    dtls.AllowanceDetails.Add(cTEAllowanceDetails);
                                }
                            }


                            dtls.EmpQualification = new List<CTEEmpQualification>();
                            var eduDtls = dbContext.PhCandidateEduDetails.Where(x => x.CandProfId == CandId && x.Status != (byte)RecordStatus.Delete).ToList();
                            if (eduDtls.Count > 0)
                            {
                                foreach (var item in eduDtls)
                                {
                                    var model = new CTEEmpQualification
                                    {
                                        Course = item.Course,
                                        Degree = item.Qualification,
                                        DurationFrom = item.DurationFrom,
                                        DurationTo = item.DurationTo,
                                        Grade = item.Grade,
                                        Percentage = item.Percentage != null ? Convert.ToInt32(item.Percentage) : 0,
                                        UniversityOrInstitution = item.UnivOrInstitution,
                                        YearofPassing = item.YearofPassing == null ? 0 : item.YearofPassing
                                    };
                                    dtls.EmpQualification.Add(model);
                                }
                            }

                            dtls.EmpReference = new List<CTEEmpReference>();
                            var empDtls = dbContext.PhCandidateEmpmtDetails.Where(x => x.CandProfId == CandId && x.Status != (byte)RecordStatus.Delete).ToList();
                            if (empDtls.Count > 0)
                            {
                                foreach (var item in empDtls)
                                {
                                    var model = new CTEEmpReference
                                    {
                                        ContactPerson = item.Cpname,
                                        ContactPersonDesignation = item.Cpdesignation,
                                        ContactPersonEmail = item.CpemailId,
                                        ContactPersonPhoneNum = item.Cpnumber,
                                        CompanyName = item.EmployerName,
                                        StartDate = item.EmptFromDate,
                                        EndDate = item.EmptToDate,
                                        Designation = item.Designation
                                    };
                                    dtls.EmpReference.Add(model);
                                }
                            }


                            using var client = new HttpClientService();

                            #region oddo 


                            if (bgvDtls.IsOdooSync == null)
                            {
                                bgvDtls.IsOdooSync = false;
                            }
                            dbContext.PhCandidateBgvDetails.Update(bgvDtls);
                            await dbContext.SaveChangesAsync();

                            logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, ", Odoo ConvertToEmployee request model :" + Newtonsoft.Json.JsonConvert.SerializeObject(dtls), "Request body: " + Newtonsoft.Json.JsonConvert.SerializeObject(dtls));

                            var odooAccessResponse = client.Get(appSettings.AppSettingsProperties.OdooBaseURL,
                                  appSettings.AppSettingsProperties.OdooLoginURL, appSettings.AppSettingsProperties.OdooDb, appSettings.AppSettingsProperties.OdooUsername, appSettings.AppSettingsProperties.OdooPassword);

                            if (odooAccessResponse.IsSuccessStatusCode)
                            {
                                var responseContent = await odooAccessResponse.Content.ReadAsStringAsync();
                                string access_token = JObject.Parse(responseContent)["access_token"].ToString();

                                var cnvtEmployee = await client.PostAsync(appSettings.AppSettingsProperties.OdooBaseURL, appSettings.AppSettingsProperties.OdooConvtEmp, access_token, dtls, string.Empty);
                                var cnvtEmployeeResponseContent = await cnvtEmployee.Content.ReadAsStringAsync();
                                var result = JsonConvert.DeserializeObject<OddoLoginResponseViewModel>(cnvtEmployeeResponseContent);

                                if (cnvtEmployee.IsSuccessStatusCode)
                                {

                                    if (result.data != null && result.data.status)
                                    {
                                        var userDtls = dbContext.PiHireUsers.Where(x => x.UserType == (byte)UserType.Candidate && x.UserName == candDtls.EmailId).FirstOrDefault();
                                        if (userDtls != null)
                                        {
                                            userDtls.EmployId = result.data.emp_id;
                                            dbContext.PiHireUsers.Update(userDtls);
                                            dbContext.SaveChanges();

                                            bgvDtls.IsOdooSync = true;
                                        }
                                    }
                                    else
                                    {

                                        logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, ",Odoo ConvertToEmployee respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(cnvtEmployeeResponseContent), "Request body: " + Newtonsoft.Json.JsonConvert.SerializeObject(dtls));

                                        respModel.SetResult(result.data.message);
                                        respModel.Status = false;

                                        return Tuple.Create(notificationPushedViewModel, respModel);
                                    }
                                }
                                else
                                {

                                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",Odoo api failed respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(cnvtEmployeeResponseContent), "Request body: " + Newtonsoft.Json.JsonConvert.SerializeObject(dtls));

                                    respModel.SetResult(result.data.message);
                                    respModel.Status = false;

                                    return Tuple.Create(notificationPushedViewModel, respModel);
                                }
                            }
                            else
                            {
                                var responseContent = await odooAccessResponse.Content.ReadAsStringAsync();
                                logger.Log(LogLevel.Error, LoggingEvents.GenerateItems, ", Odoo authentication failed respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(responseContent), "Request body: " + Newtonsoft.Json.JsonConvert.SerializeObject(dtls));

                                message = "Failed to authentication in Odoo. Please contact administrator";
                                respModel.SetResult(message);
                                respModel.Status = false;

                                return Tuple.Create(notificationPushedViewModel, respModel);
                            }
                            #endregion


                            #region Gateway      

                            var response = await client.PostAsync(appSettings.AppSettingsProperties.GatewayUrl, "/api/ConsToEmp/SaveEmployee", dtls);
                            if (response.IsSuccessStatusCode)
                            {
                                var responseContent = await response.Content.ReadAsStringAsync();
                                var result = JsonConvert.DeserializeObject<CtoEResponseModel>(responseContent);

                                bgvDtls.IsGatewaySync = true;

                                logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, ", Gateway ConvertToEmployee request model:" + Newtonsoft.Json.JsonConvert.SerializeObject(dtls), "Response: " + Newtonsoft.Json.JsonConvert.SerializeObject(respModel));
                            }
                            else
                            {
                                bgvDtls.IsGatewaySync = false;

                                logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, ", Gateway ConvertToEmployee request model:" + Newtonsoft.Json.JsonConvert.SerializeObject(dtls), "Response body: " + Newtonsoft.Json.JsonConvert.SerializeObject(respModel));

                                message = "Failed to save in gateway. Please contact administrator";
                                respModel.SetResult(message);
                                respModel.Status = false;
                            }

                            #endregion


                            if (bgvDtls.IsGatewaySync == true && bgvDtls.IsOdooSync == true)
                            {
                                var jobAssignment = await dbContext.PhJobAssignments.Where(x => x.Joid == JobId
                           && x.AssignedTo == candDtls.RecruiterId).FirstOrDefaultAsync();
                                if (jobAssignment != null)
                                {
                                    jobAssignment.IsJoinerSuc = true;

                                    dbContext.PhJobAssignments.Update(jobAssignment);
                                    await dbContext.SaveChangesAsync();
                                }

                                var UpdateStatusId = status.Where(x => x.Cscode == "SUC").Select(x => x.Id).FirstOrDefault();
                                // Applying workflow rules                           
                                var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                                {
                                    ActionMode = (byte)WorkflowActionMode.Candidate,
                                    CanProfId = CandId,
                                    JobId = JobId,
                                    TaskCode = TaskCode.CUS.ToString(), // Convert Employee 
                                    CurrentStatusId = candDtls.CandProfStatus,
                                    UpdateStatusId = UpdateStatusId,
                                    UserId = UserId
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
                                            CreatedBy = UserId
                                        };
                                        notificationPushedViewModel.Add(notificationPushed);
                                    }
                                }

                                respModel.SetResult("Candidate conversion to employee is successful");
                                respModel.Status = true;
                            }

                            dbContext.PhCandidateBgvDetails.Update(bgvDtls);
                            await dbContext.SaveChangesAsync();

                        }
                        else
                        {
                            message = "To convert employee candidate status should be PREJOIN SUCCESS";
                            respModel.SetResult(message);
                            respModel.Status = false;
                        }
                    }
                    else
                    {
                        message = "This candidate already converted to employee";
                        respModel.SetResult(message);
                        respModel.Status = false;
                    }
                }
                else
                {
                    message = "The candidate pre employment form is not reviewed";
                    respModel.SetResult(message);
                    respModel.Status = false;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",ConvertToEmployee respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, "Exception occured while processing your request, Please contact administrator", true);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }

        public async Task<dynamic> Odoologin()
        {
            using var client = new HttpClientService();

            var odResponse = client.Get(appSettings.AppSettingsProperties.OdooBaseURL,
                                      appSettings.AppSettingsProperties.OdooLoginURL, appSettings.AppSettingsProperties.OdooDb, appSettings.AppSettingsProperties.OdooUsername, appSettings.AppSettingsProperties.OdooPassword);

            if (odResponse.IsSuccessStatusCode)
            {
                var responseContent = await odResponse.Content.ReadAsStringAsync();
                string data = JObject.Parse(responseContent)["access_token"].ToString();
                return data;
            }
            return false;
        }
    }
}
