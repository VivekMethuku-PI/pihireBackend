using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PiHire.BAL.Common;
using PiHire.BAL.Common.Http;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.Common.Types;
using PiHire.BAL.IRepositories;
using PiHire.BAL.ViewModels;
using PiHire.BAL.ViewModels.ApiBaseModels;
using PiHire.DAL.Entities;
using PiHire.DAL.Models;
using PiHire.Utilities.Communications.Emails;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class CandidateRepository : BaseRepository, ICandidateRepository
    {
        readonly Logger logger;

        private readonly IWebHostEnvironment _environment;


        public CandidateRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<CandidateRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        #region Candidate Portal
        public async Task<GetResponseViewModel<GetJobCandidatePortalViewModel>> GetJobCandidateAsync(int candProfileId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            var respModel = new GetResponseViewModel<GetJobCandidatePortalViewModel>();

            int loginUserId = Usr.Id;
            var loginUserType = (UserType)Usr.UserTypeId;

            if (loginUserType == UserType.Candidate)
            {
                int usrId = Usr.Id;
                var email = await dbContext.PiHireUsers.Where(da => da.Id == usrId).Select(da => da.EmailId).FirstOrDefaultAsync();
                candProfileId = await dbContext.PhCandidateProfiles.Where(da => da.EmailId == email).Select(da => da.Id).FirstOrDefaultAsync();
            }

            using (var trans = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted))
                try
                {
                    var model = new GetJobCandidatePortalViewModel();

                    var dbModel_jobOpeningExist = await dbContext.PhJobOpenings.Where(da => da.Id == JobId && da.Status != (byte)RecordStatus.Delete).CountAsync() > 0;
                    if (dbModel_jobOpeningExist)
                    {
                        model = await dbContext.PhCandidateProfiles.AsNoTracking().Where(da => da.Id == candProfileId)
                                      .Select(can => new GetJobCandidatePortalViewModel
                                      {
                                          SourceId = can.SourceId,
                                          CandProfId = can.Id,
                                          EmailAddress = can.EmailId,

                                          FullName = can.CandName,
                                          ValidPpflag = can.ValidPpflag,
                                          ContactNo = can.ContactNo,
                                          AlteContactNo = can.AlteContactNo,
                                          CandidateDOB = can.Dob,
                                          Gender = can.Gender,
                                          MaritalStatus = can.MaritalStatus,

                                          CurrEmplFlag = can.CurrEmplFlag,
                                          NoticePeriod = can.NoticePeriod,
                                          ReasonsForReloc = can.ReasonsForReloc,

                                          Nationality = can.Nationality,
                                          CountryID = can.CountryId,
                                          CurrLocation = can.CurrLocation,
                                          CurrLocationID = can.CurrLocationId,

                                          CurrOrganization = can.CurrOrganization,
                                          CurrDesignation = can.CurrDesignation,

                                          TotalExperiance = can.Experience,
                                          RelevantExperiance = can.RelevantExperience,

                                          CPCurrency = can.Cpcurrency,
                                          CPGrossPayPerAnnum = can.CpgrossPayPerAnnum,
                                          CPDeductionsPerAnnum = can.CpdeductionsPerAnnum,
                                          CPVariablePayPerAnnum = can.CpvariablePayPerAnnum,
                                          CPTakeHomeSalPerMonth = can.CptakeHomeSalPerMonth,

                                          CurrentPackage = can.CpdeductionsPerAnnum,



                                          //ReasonType = can.ReasonType,
                                          //Remarks = can.Remarks,


                                      }).FirstOrDefaultAsync();

                        if (model != null)
                        {
                            var dbModel_JobCandidate = await dbContext.PhJobCandidates.AsNoTracking().Where(da => da.Joid == JobId && da.CandProfId == candProfileId).FirstOrDefaultAsync();
                            if (dbModel_JobCandidate != null)
                            {
                                model.JobId = dbModel_JobCandidate.Joid;

                                model.EPCurrency = dbModel_JobCandidate.Epcurrency;
                                model.EPTakeHomeSalPerMonth = dbModel_JobCandidate.EptakeHomePerMonth;

                                model.ExpectedPackage = dbModel_JobCandidate.EpgrossPayPerAnnum;
                                model.SelfRating = dbModel_JobCandidate.SelfRating;
                                model.CandPrfStatus = dbModel_JobCandidate.CandProfStatus;
                                model.StageId = dbModel_JobCandidate.StageId;

                                model.CandidatePrefRegion = ToViewModel<int?>(dbModel_JobCandidate.CandidatePrefRegion, dbModel_JobCandidate.CandidatePrefRegionId);
                                model.JobCountryDrivingLicence = dbModel_JobCandidate.JobCountryDrivingLicence;
                                model.JobDesirableDomain = ToViewModel<int?>(dbModel_JobCandidate.JobDesirableDomain, dbModel_JobCandidate.JobDesirableDomainId);
                                model.JobDesirableCategory = ToViewModel<int?>(dbModel_JobCandidate.JobDesirableCategory, dbModel_JobCandidate.JobDesirableCategoryId);
                                model.JobDesirableTenure = ToViewModel<int?>(dbModel_JobCandidate.JobDesirableTenure, dbModel_JobCandidate.JobDesirableTenureId);
                                model.JobDesirableWorkPattern = ToViewModel<int?>(dbModel_JobCandidate.JobDesirableWorkPattern, dbModel_JobCandidate.JobDesirableWorkPatternId);
                                model.JobDesirableTeamRole = ToViewModel<int?>(dbModel_JobCandidate.JobDesirableTeamRole, dbModel_JobCandidate.JobDesirableTeamRoleId);
                                model.CandidatePrefLanguage = ToViewModel<int?>(dbModel_JobCandidate.CandidatePrefLanguage, dbModel_JobCandidate.CandidatePrefLanguageId);
                                model.CandidatePrefVisaPreference = ToViewModel<int?>(dbModel_JobCandidate.CandidatePrefVisaPreference, dbModel_JobCandidate.CandidatePrefVisaPreferenceId);

                                model.CandidatePrefEmployeeStatus = (CandidateEmployeeStatus?)dbModel_JobCandidate.CandidatePrefEmployeeStatus;
                                model.CandidateResignationAccepted = dbModel_JobCandidate.CandidateResignationAccepted;
                                model.CandidateLastWorkDate = dbModel_JobCandidate.CandidateLastWorkDate;
                                model.AnyOfferInHand = dbModel_JobCandidate.AnyOfferInHand;
                                model.CandidateCanJoinDate = dbModel_JobCandidate.CandidateCanJoinDate;

                                model.InterviewFaceToFace = dbModel_JobCandidate.InterviewFaceToFace;
                                model.InterviewFaceToFaceReason = dbModel_JobCandidate.InterviewFaceToFaceReason;
                            }
                            {
                                model.CandidateSkills = await (from Canskill in dbContext.PhCandidateSkillsets
                                                               join tech in dbContext.PhTechnologysSes on Canskill.TechnologyId equals tech.Id
                                                               where Canskill.CandProfId == candProfileId && Canskill.Status == (byte)RecordStatus.Active
                                                               select new UpdateJobCandidatePortalViewModel_skillRating
                                                               {
                                                                   //Id = Canskill.Id,
                                                                   ExpInMonths = Canskill.ExpInMonths,
                                                                   ExpInYears = Canskill.ExpInYears,
                                                                   SelfRating = Canskill.SelfRating,
                                                                   TechnologyId = Canskill.TechnologyId,
                                                                   //TechnologyName = tech.Title,
                                                                   //IsCanSkill = false
                                                               }).ToListAsync();
                                {
                                    var dbModel_jobCandidateSkills = await dbContext.PhJobCandidateSkillsets.AsNoTracking().Where(da => da.JoCandId == dbModel_JobCandidate.Id && da.Status == (byte)RecordStatus.Active).ToListAsync();

                                    {
                                        var GroupType = (byte)JobDesirableSkillGroupTypes.Specializations;
                                        model.JobDesirableSpecializations = dbModel_jobCandidateSkills.Where(da => da.GroupType == GroupType)
                                                                            .Select(da => new UpdateJobCandidatePortalViewModel_skill
                                                                            {
                                                                                TechnologyId = da.TechnologyId,
                                                                                ExpInYears = da.ExpInYears,
                                                                                ExpInMonths = da.ExpInMonths
                                                                            }).ToList();
                                    }
                                    {
                                        var GroupType = (byte)JobDesirableSkillGroupTypes.Implementations;
                                        model.JobDesirableImplementations = dbModel_jobCandidateSkills.Where(da => da.GroupType == GroupType)
                                                                             .Select(da => new UpdateJobCandidatePortalViewModel_skill
                                                                             {
                                                                                 TechnologyId = da.TechnologyId,
                                                                                 ExpInYears = da.ExpInYears,
                                                                                 ExpInMonths = da.ExpInMonths
                                                                             }).ToList();
                                    }
                                    {
                                        var GroupType = (byte)JobDesirableSkillGroupTypes.Designs;
                                        model.JobDesirableDesigns = dbModel_jobCandidateSkills.Where(da => da.GroupType == GroupType)
                                                                             .Select(da => new UpdateJobCandidatePortalViewModel_skill
                                                                             {
                                                                                 TechnologyId = da.TechnologyId,
                                                                                 ExpInYears = da.ExpInYears,
                                                                                 ExpInMonths = da.ExpInMonths
                                                                             }).ToList();
                                    }
                                    {
                                        var GroupType = (byte)JobDesirableSkillGroupTypes.Developments;
                                        model.JobDesirableDevelopments = dbModel_jobCandidateSkills.Where(da => da.GroupType == GroupType)
                                                                             .Select(da => new UpdateJobCandidatePortalViewModel_skill
                                                                             {
                                                                                 TechnologyId = da.TechnologyId,
                                                                                 ExpInYears = da.ExpInYears,
                                                                                 ExpInMonths = da.ExpInMonths
                                                                             }).ToList();
                                    }
                                    {
                                        var GroupType = (byte)JobDesirableSkillGroupTypes.Supports;
                                        model.JobDesirableSupports = dbModel_jobCandidateSkills.Where(da => da.GroupType == GroupType)
                                                                             .Select(da => new UpdateJobCandidatePortalViewModel_skill
                                                                             {
                                                                                 TechnologyId = da.TechnologyId,
                                                                                 ExpInYears = da.ExpInYears,
                                                                                 ExpInMonths = da.ExpInMonths
                                                                             }).ToList();
                                    }
                                    {
                                        var GroupType = (byte)JobDesirableSkillGroupTypes.Qualities;
                                        model.JobDesirableQualities = dbModel_jobCandidateSkills.Where(da => da.GroupType == GroupType)
                                                                             .Select(da => new UpdateJobCandidatePortalViewModel_skill
                                                                             {
                                                                                 TechnologyId = da.TechnologyId,
                                                                                 ExpInYears = da.ExpInYears,
                                                                                 ExpInMonths = da.ExpInMonths
                                                                             }).ToList();
                                    }
                                    {
                                        var GroupType = (byte)JobDesirableSkillGroupTypes.Documentations;
                                        model.JobDesirableDocumentations = dbModel_jobCandidateSkills.Where(da => da.GroupType == GroupType)
                                                                             .Select(da => new UpdateJobCandidatePortalViewModel_skill
                                                                             {
                                                                                 TechnologyId = da.TechnologyId,
                                                                                 ExpInYears = da.ExpInYears,
                                                                                 ExpInMonths = da.ExpInMonths
                                                                             }).ToList();
                                    }
                                }
                            }

                            //Qualifications
                            {
                                var dbModel_jobOpeningQualifications = await dbContext.PhJobCandidateOpeningsQualifications.AsNoTracking().Where(da => da.JoCandId == dbModel_JobCandidate.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();

                                //Candidate Education Qualification
                                {
                                    var GroupType = (byte)JobCandQualCertGroupTypes.JobCandiateEducationQualification;
                                    model.CandidateQualificationModel =
                                        dbModel_jobOpeningQualifications.Where(da => da.GroupType == GroupType)
                                            .Select(da => new GetJobCandidatePortalViewModel_CandidateEducation
                                            {
                                                QualificationId = da.QualificationId,
                                                Qualification = ToViewModel(da.PrefQualification, da.PrefQualificationId),
                                                CourseId = da.CourseId,
                                                Course = ToViewModel(da.PrefCourse, da.PrefCourseId)
                                            }).ToList();
                                }
                                //Opening Qualification
                                {
                                    var GroupType = (byte)JobCandQualCertGroupTypes.JobOpeningQualification;
                                    model.OpeningQualifications =
                                        dbModel_jobOpeningQualifications.Where(da => da.GroupType == GroupType)
                                            .Select(da => new UpdateJobCandidatePortalViewModel_OpeningEducation
                                            {
                                                QualificationId = da.QualificationId,
                                                Qualification = ToViewModel(da.PrefQualification, da.PrefQualificationId),
                                            }).ToList();
                                }
                            }

                            //certifications
                            {
                                var dbModel_jobOpeningCertifications = await dbContext.PhJobCandidateOpeningsCertifications.AsNoTracking().Where(da => da.JoCandId == dbModel_JobCandidate.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();

                                //Candidate Education Certification
                                {
                                    var GroupType = (byte)JobCandQualCertGroupTypes.JobCandidateEducationCertifications;
                                    model.CandidateCertificationModel =
                                        dbModel_jobOpeningCertifications.Where(da => da.GroupType == GroupType)
                                                    .Select(da => new UpdateJobCandidatePortalViewModel_CandidateCertification
                                                    {
                                                        CertificationId = da.CertificationId,
                                                        Certification = ToViewModel(da.PrefCertification, da.PrefCertificationId),
                                                    }).ToList();
                                }
                                //Opening Certification
                                {
                                    var GroupType = (byte)JobCandQualCertGroupTypes.JobOpeningCertification;
                                    model.OpeningCertifications =
                                        dbModel_jobOpeningCertifications.Where(da => da.GroupType == GroupType)
                                                    .Select(da => new UpdateJobCandidatePortalViewModel_CandidateCertification
                                                    {
                                                        CertificationId = da.CertificationId,
                                                        Certification = ToViewModel(da.PrefCertification, da.PrefCertificationId),
                                                    }).ToList();
                                }
                            }

                            var documents = await (from canDoc in dbContext.PhCandidateDocs.AsNoTracking()
                                                   where canDoc.CandProfId == candProfileId && canDoc.Joid == JobId && canDoc.Status != (byte)RecordStatus.Delete
                                                   select new CandidateFilesViewModel
                                                   {
                                                       DocType = canDoc.DocType,
                                                       FileGroup = canDoc.FileGroup,
                                                       FileName = canDoc.FileName,
                                                       DocStatus = canDoc.DocStatus,
                                                       CandProfId = canDoc.CandProfId,
                                                       UploadedFromDrive = false
                                                   }).ToListAsync();
                            foreach (var document in documents)
                            {
                                document.DocStatusName = Enum.GetName(typeof(DocStatus), document.DocStatus);
                                if (!string.IsNullOrEmpty(document.FileName))
                                {
                                    if (ValidHttpURL(document.FileName))
                                    {
                                        document.UploadedFromDrive = true;
                                        document.FilePath = document.FileName;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(document.FileName))
                                        {
                                            document.FileName = document.FileName.Replace("#", "%23");
                                            document.FilePath = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + document.CandProfId + "/" + document.FileName;
                                        }
                                    }
                                }
                            }

                            if (documents.Count() > 0)
                            {
                                //model.Photo = documents.Where(da => da.DocType == "Profile Photo" && da.FileGroup == (byte)FileGroup.Profile).Select(da => da.FilePath).FirstOrDefault();

                                model.ResumeUrl = documents.Where(da => da.DocType == "Candidate CV" && da.FileGroup == (byte)FileGroup.Profile).Select(da => da.FilePath).FirstOrDefault();
                                model.CandVideoProfileUrl = documents.Where(da => da.DocType == "Video Profile" && da.FileGroup == (byte)FileGroup.Profile).Select(da => da.FilePath).FirstOrDefault();
                                model.PaySlipUrls = documents.Where(da => (da.DocType == "Pay Slips" || da.DocType == "Payslip") && da.FileGroup == (byte)FileGroup.Profile).Select(da => da.FilePath).ToList();
                            }

                            respModel.SetResult(model);
                        }
                        else
                        {
                            logger.Log(LogLevel.Information, LoggingEvents.GetItem, $"Candidate: {candProfileId} is not available");
                            respModel.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The Candidate is not available", true);
                        }
                    }
                    else
                    {
                        logger.Log(LogLevel.Information, LoggingEvents.GetItem, $"Job: {JobId} is not available");
                        respModel.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The Job is not available", true);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"candProfileId:{candProfileId}, Jobid:{JobId}", respModel.Meta.RequestID, ex);

                    respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    trans.Rollback();
                }
            return respModel;
        }

        private UpdateJobCandidatePortalViewModel_value<T> ToViewModel<T>(bool? PreferenceType, T value)
        {
            if (PreferenceType.HasValue)
            {
                return new UpdateJobCandidatePortalViewModel_value<T> { PreferenceType = PreferenceType.Value, Value = value };
            }
            else return null;
        }

        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateJobCandidateAsync(UpdateJobCandidatePortalViewModel candidateModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            int loginUserId = Usr.Id;
            var loginUserType = (UserType)Usr.UserTypeId;

            int candProfileId = candidateModel.Id;
            if (loginUserType == UserType.Candidate)
            {
                int usrId = Usr.Id;
                var email = await dbContext.PiHireUsers.Where(da => da.Id == usrId).Select(da => da.EmailId).FirstOrDefaultAsync();
                candProfileId = await dbContext.PhCandidateProfiles.Where(da => da.EmailId == email).Select(da => da.Id).FirstOrDefaultAsync();
            }

            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    var isJobAvail = await dbContext.PhJobOpenings.Where(da => da.Id == candidateModel.JobId && da.Status != (byte)RecordStatus.Delete).CountAsync() > 0;
                    if (isJobAvail)
                    {
                        var dbModel_candidateProfile = await dbContext.PhCandidateProfiles.FirstOrDefaultAsync(da => da.Id == candProfileId);
                        if (dbModel_candidateProfile != null)
                        {
                            byte ProfCompStatus = 0;
                            if (candidateModel.MaritalStatus > 0 &&
                                !string.IsNullOrEmpty(candidateModel.TotalExperiance) &&
                                !string.IsNullOrEmpty(candidateModel.RelevantExperiance))
                            {
                                ProfCompStatus = 1;
                            }
                            if (!string.IsNullOrEmpty(candidateModel.CPCurrency) &&
                                candidateModel.CPGrossPayPerAnnum != 0 &&
                                candidateModel.EPTakeHomeSalPerMonth != 0 &&
                                !string.IsNullOrEmpty(candidateModel.EPCurrency) && candidateModel.EPTakeHomeSalPerMonth != 0)
                            {
                                ProfCompStatus = 2;
                            }

                            double jobSkillRating = 0;
                            if (candidateModel.CandidateSkills?.Count > 0)
                            {
                                var JobSkillCount = candidateModel.CandidateSkills.Count();
                                var JobSkillRating = candidateModel.CandidateSkills.Sum(da => da.SelfRating);

                                double value = (double)JobSkillRating / (double)JobSkillCount;
                                if (value > 0)
                                {
                                    jobSkillRating = Math.Round(value, 1, MidpointRounding.AwayFromZero);
                                }
                            }

                            dbModel_candidateProfile.CandName = candidateModel.FullName;
                            dbModel_candidateProfile.FullNameInPp = candidateModel.FullName;
                            dbModel_candidateProfile.ContactNo = candidateModel.ContactNo;
                            //dbModel_candidateProfile.Remarks = candidateModel.Remarks;

                            dbModel_candidateProfile.ValidPpflag = candidateModel.ValidPpflag;
                            dbModel_candidateProfile.AlteContactNo = candidateModel.AlteContactNo;
                            dbModel_candidateProfile.Dob = candidateModel.CandidateDOB;
                            dbModel_candidateProfile.Gender = candidateModel.Gender;
                            dbModel_candidateProfile.MaritalStatus = candidateModel.MaritalStatus;
                            dbModel_candidateProfile.NoticePeriod = candidateModel.NoticePeriod;
                            dbModel_candidateProfile.ReasonsForReloc = candidateModel.ReasonsForReloc;

                            dbModel_candidateProfile.Nationality = candidateModel.Nationality;
                            dbModel_candidateProfile.CountryId = candidateModel.CountryID;
                            dbModel_candidateProfile.CurrLocation = candidateModel.CurrLocation;
                            dbModel_candidateProfile.CurrLocationId = candidateModel.CurrLocationID;

                            dbModel_candidateProfile.CurrOrganization = candidateModel.CurrOrganization;
                            dbModel_candidateProfile.CurrEmplFlag = candidateModel.CurrEmplFlag;
                            dbModel_candidateProfile.CurrDesignation = candidateModel.CurrDesignation;

                            dbModel_candidateProfile.Experience = candidateModel.TotalExperiance;
                            dbModel_candidateProfile.ExperienceInMonths = candidateModel.TotalExperiance != string.Empty ? ConvertMonths(candidateModel.TotalExperiance) : 0;

                            dbModel_candidateProfile.RelevantExperience = candidateModel.RelevantExperiance;
                            dbModel_candidateProfile.ReleExpeInMonths = candidateModel.RelevantExperiance != string.Empty ? ConvertMonths(candidateModel.RelevantExperiance) : 0;

                            dbModel_candidateProfile.ProfCompStatus = ProfCompStatus;

                            dbModel_candidateProfile.Cpcurrency = candidateModel.CPCurrency;
                            dbModel_candidateProfile.CpgrossPayPerAnnum = candidateModel.CPGrossPayPerAnnum;
                            dbModel_candidateProfile.CpdeductionsPerAnnum = candidateModel.CPDeductionsPerAnnum;
                            dbModel_candidateProfile.CpvariablePayPerAnnum = candidateModel.CPVariablePayPerAnnum;
                            dbModel_candidateProfile.CptakeHomeSalPerMonth = candidateModel.CPTakeHomeSalPerMonth;

                            dbModel_candidateProfile.Epcurrency = candidateModel.EPCurrency;
                            dbModel_candidateProfile.EptakeHomeSalPerMonth = candidateModel.EPTakeHomeSalPerMonth;

                            dbModel_candidateProfile.UpdatedBy = loginUserId;
                            dbModel_candidateProfile.UpdatedDate = CurrentTime;

                            var dbModel_JobCandidate = await dbContext.PhJobCandidates.FirstOrDefaultAsync(da => da.Joid == candidateModel.JobId && da.CandProfId == dbModel_candidateProfile.Id && da.Status != (byte)RecordStatus.Delete);
                            if (dbModel_JobCandidate != null)
                            {
                                dbModel_JobCandidate.Epcurrency = candidateModel.EPCurrency;
                                dbModel_JobCandidate.EptakeHomePerMonth = candidateModel.EPTakeHomeSalPerMonth;
                                dbModel_JobCandidate.SelfRating = jobSkillRating;
                                dbModel_JobCandidate.EmailSentFlag = true;

                                dbModel_JobCandidate.CandidatePrefRegion = candidateModel.CandidatePrefRegion?.PreferenceType;
                                dbModel_JobCandidate.CandidatePrefRegionId = candidateModel.CandidatePrefRegion?.Value;

                                dbModel_JobCandidate.JobCountryDrivingLicence = candidateModel.JobCountryDrivingLicence;
                                dbModel_JobCandidate.JobDesirableDomain = candidateModel.JobDesirableDomain?.PreferenceType;
                                dbModel_JobCandidate.JobDesirableDomainId = candidateModel.JobDesirableDomain?.Value;

                                dbModel_JobCandidate.JobDesirableCategory = candidateModel.JobDesirableCategory?.PreferenceType;
                                dbModel_JobCandidate.JobDesirableCategoryId = candidateModel.JobDesirableCategory?.Value;

                                dbModel_JobCandidate.JobDesirableTenure = candidateModel.JobDesirableTenure?.PreferenceType;
                                dbModel_JobCandidate.JobDesirableTenureId = candidateModel.JobDesirableTenure?.Value;

                                dbModel_JobCandidate.JobDesirableWorkPattern = candidateModel.JobDesirableWorkPattern?.PreferenceType;
                                dbModel_JobCandidate.JobDesirableWorkPatternId = candidateModel.JobDesirableWorkPattern?.Value;

                                dbModel_JobCandidate.JobDesirableTeamRole = candidateModel.JobDesirableTeamRole?.PreferenceType;
                                dbModel_JobCandidate.JobDesirableTeamRoleId = candidateModel.JobDesirableTeamRole?.Value;

                                dbModel_JobCandidate.CandidatePrefLanguage = candidateModel.CandidatePrefLanguage?.PreferenceType;
                                dbModel_JobCandidate.CandidatePrefLanguageId = candidateModel.CandidatePrefLanguage?.Value;

                                dbModel_JobCandidate.CandidatePrefVisaPreference = candidateModel.CandidatePrefVisaPreference?.PreferenceType;
                                dbModel_JobCandidate.CandidatePrefVisaPreferenceId = candidateModel.CandidatePrefVisaPreference?.Value;

                                dbModel_JobCandidate.CandidatePrefEmployeeStatus = (int)candidateModel.CandidatePrefEmployeeStatus;
                                dbModel_JobCandidate.CandidateResignationAccepted = candidateModel.CandidateResignationAccepted;
                                dbModel_JobCandidate.CandidateLastWorkDate = candidateModel.CandidateLastWorkDate;
                                dbModel_JobCandidate.AnyOfferInHand = candidateModel.AnyOfferInHand;
                                dbModel_JobCandidate.CandidateCanJoinDate = candidateModel.CandidateCanJoinDate;

                                dbModel_JobCandidate.InterviewFaceToFace = candidateModel.InterviewFaceToFace;
                                dbModel_JobCandidate.InterviewFaceToFaceReason = candidateModel.InterviewFaceToFaceReason;

                                dbModel_JobCandidate.UpdatedBy = loginUserId;
                                dbModel_JobCandidate.UpdatedDate = CurrentTime;

                                dbContext.PhJobCandidates.Update(dbModel_JobCandidate);
                            }
                            else
                            {
                                PhJobAssignment dbModel_JobAssignment = null;
                                DateTime? ProfReceDate = CurrentTime;
                                if (loginUserType == UserType.Recruiter)
                                {
                                    dbModel_JobAssignment = await dbContext.PhJobAssignments.FirstOrDefaultAsync(da => da.Joid == candidateModel.JobId && da.AssignedTo == loginUserId);
                                    if (dbModel_JobAssignment != null)
                                    {
                                        if (dbModel_JobAssignment.ProfilesUploaded == null)
                                        {
                                            dbModel_JobAssignment.ProfilesUploaded = 0;
                                        }
                                        dbModel_JobAssignment.ProfilesUploaded = (short)(dbModel_JobAssignment.ProfilesUploaded + 1);
                                        dbModel_JobAssignment.UpdatedBy = loginUserId;
                                        dbModel_JobAssignment.UpdatedDate = CurrentTime;
                                        PhJobAssignmentsDayWise_records(ref dbModel_JobAssignment, CurrentTime, incrementCvsuploaded: 1);
                                        dbContext.PhJobAssignments.Update(dbModel_JobAssignment);
                                        await dbContext.SaveChangesAsync();
                                    }
                                }
                                if (dbModel_JobAssignment == null)
                                {
                                    dbModel_JobAssignment = await dbContext.PhJobAssignments.Where(da => da.Joid == candidateModel.JobId && da.DeassignDate.HasValue == false).OrderBy(da => da.CreatedDate).FirstOrDefaultAsync();
                                    if (dbModel_JobAssignment == null)
                                        dbModel_JobAssignment = await dbContext.PhJobAssignments.Where(da => da.Joid == candidateModel.JobId).OrderByDescending(da => da.DeassignDate).FirstOrDefaultAsync();
                                    if (dbModel_JobAssignment != null)
                                    {
                                        if (dbModel_JobAssignment.ProfilesUploaded == null)
                                        {
                                            dbModel_JobAssignment.ProfilesUploaded = 0;
                                        }
                                        dbModel_JobAssignment.ProfilesUploaded = (short)(dbModel_JobAssignment.ProfilesUploaded + 1);
                                        dbModel_JobAssignment.UpdatedBy = loginUserId;
                                        dbModel_JobAssignment.UpdatedDate = CurrentTime;
                                        PhJobAssignmentsDayWise_records(ref dbModel_JobAssignment, CurrentTime, incrementCvsuploaded: 1);
                                        dbContext.PhJobAssignments.Update(dbModel_JobAssignment);
                                        await dbContext.SaveChangesAsync();
                                    }
                                }

                                dbModel_JobCandidate = new PhJobCandidate
                                {
                                    CandProfId = candidateModel.Id,
                                    Joid = candidateModel.JobId,
                                    IsTagged = false,

                                    CandidatePrefRegion = candidateModel.CandidatePrefRegion?.PreferenceType,
                                    CandidatePrefRegionId = candidateModel.CandidatePrefRegion?.Value,
                                    JobCountryDrivingLicence = candidateModel.JobCountryDrivingLicence,
                                    JobDesirableDomain = candidateModel.JobDesirableDomain?.PreferenceType,
                                    JobDesirableDomainId = candidateModel.JobDesirableDomain?.Value,

                                    JobDesirableCategory = candidateModel.JobDesirableCategory?.PreferenceType,
                                    JobDesirableCategoryId = candidateModel.JobDesirableCategory?.Value,


                                    JobDesirableTenure = candidateModel.JobDesirableTenure?.PreferenceType,
                                    JobDesirableTenureId = candidateModel.JobDesirableTenure?.Value,

                                    JobDesirableWorkPattern = candidateModel.JobDesirableWorkPattern?.PreferenceType,
                                    JobDesirableWorkPatternId = candidateModel.JobDesirableWorkPattern?.Value,

                                    JobDesirableTeamRole = candidateModel.JobDesirableTeamRole?.PreferenceType,
                                    JobDesirableTeamRoleId = candidateModel.JobDesirableTeamRole?.Value,

                                    CandidatePrefLanguage = candidateModel.CandidatePrefLanguage?.PreferenceType,
                                    CandidatePrefLanguageId = candidateModel.CandidatePrefLanguage?.Value,


                                    CandidatePrefVisaPreference = candidateModel.CandidatePrefVisaPreference?.PreferenceType,
                                    CandidatePrefVisaPreferenceId = candidateModel.CandidatePrefVisaPreference?.Value,

                                    CandidatePrefEmployeeStatus = (int)candidateModel.CandidatePrefEmployeeStatus,
                                    CandidateResignationAccepted = candidateModel.CandidateResignationAccepted,
                                    CandidateLastWorkDate = candidateModel.CandidateLastWorkDate,
                                    AnyOfferInHand = candidateModel.AnyOfferInHand,
                                    CandidateCanJoinDate = candidateModel.CandidateCanJoinDate,

                                    InterviewFaceToFace = candidateModel.InterviewFaceToFace,
                                    InterviewFaceToFaceReason = candidateModel.InterviewFaceToFaceReason,

                                    //RecruiterId = dbModel_JobAssignment?.AssignedTo,
                                    RecruiterId = dbModel_JobAssignment?.AssignedTo,
                                    EmailSentFlag = true,
                                    StageId = 1,
                                    CandProfStatus = 2,

                                    //Epcurrency = candidateModel.EPCurrency,
                                    //EptakeHomePerMonth = candidateModel.EPTakeHomeSalPerMonth,
                                    Epcurrency = string.Empty,
                                    EptakeHomePerMonth = 0,
                                    EpdeductionsPerAnnum = 0,
                                    EpgrossPayPerAnnum = 0,

                                    OpconfirmDate = null,
                                    OpconfirmFlag = false,

                                    Opcurrency = string.Empty,
                                    OpdeductionsPerAnnum = 0,
                                    OpgrossPayPerAnnum = 0,
                                    OptakeHomePerMonth = 0,
                                    OpvarPayPerAnnum = 0,
                                    OpgrossPayPerMonth = 0,
                                    OpnetPayPerAnnum = 0,
                                    SelfRating = jobSkillRating,

                                    ProfileUpdateFlag = false,
                                    ProfReceDate = ProfReceDate,

                                    Status = (byte)RecordStatus.Active,
                                    CreatedBy = loginUserId,
                                    CreatedDate = CurrentTime
                                };
                                await dbContext.PhJobCandidates.AddAsync(dbModel_JobCandidate);
                                await dbContext.SaveChangesAsync();
                            }

                            if (candidateModel.CandidateSkills?.Count > 0)
                            {
                                foreach (var CandidateSkill in candidateModel.CandidateSkills)
                                {
                                    var dbModel_candidateSkill = await dbContext.PhCandidateSkillsets.FirstOrDefaultAsync(da => da.TechnologyId == CandidateSkill.TechnologyId && da.CandProfId == dbModel_candidateProfile.Id && da.Status == (byte)RecordStatus.Active);
                                    if (dbModel_candidateSkill == null)
                                    {
                                        dbModel_candidateSkill = new PhCandidateSkillset
                                        {
                                            CandProfId = dbModel_candidateProfile.Id,
                                            CreatedBy = loginUserId,
                                            CreatedDate = CurrentTime,

                                            SelfRating = CandidateSkill.SelfRating,
                                            ExpInMonths = CandidateSkill.ExpInMonths,
                                            ExpInYears = CandidateSkill.ExpInYears,
                                            TechnologyId = CandidateSkill.TechnologyId,
                                            Status = (byte)RecordStatus.Active
                                        };
                                        await dbContext.PhCandidateSkillsets.AddAsync(dbModel_candidateSkill);
                                        await dbContext.SaveChangesAsync();
                                    }
                                    else
                                    {
                                        dbModel_candidateSkill.SelfRating = CandidateSkill.SelfRating;
                                        dbModel_candidateSkill.ExpInMonths = CandidateSkill.ExpInMonths;
                                        dbModel_candidateSkill.ExpInYears = CandidateSkill.ExpInYears;

                                        dbModel_candidateSkill.UpdatedBy = loginUserId;
                                        dbModel_candidateSkill.UpdatedDate = CurrentTime;

                                        dbContext.PhCandidateSkillsets.Update(dbModel_candidateSkill);
                                    }
                                }

                                double overallCanRating = 0;
                                var canOverSkill = await dbContext.PhCandidateSkillsets.Where(da => da.CandProfId == dbModel_candidateProfile.Id && da.Status == (byte)RecordStatus.Active).ToListAsync();
                                var canSkillCount = canOverSkill.Count();
                                var canSkillRating = canOverSkill.Sum(da => da.SelfRating);
                                double value1 = (double)canSkillRating / (double)canSkillRating;
                                if (value1 > 0)
                                {
                                    overallCanRating = Math.Round(value1, 1, MidpointRounding.AwayFromZero);
                                }
                                dbModel_candidateProfile.OverallRating = overallCanRating;
                                dbContext.PhCandidateProfiles.Update(dbModel_candidateProfile);
                            }
                            {
                                var dbModel_jobCandidateSkills = await dbContext.PhJobCandidateSkillsets.Where(da => da.JoCandId == dbModel_JobCandidate.Id && da.CandProfId == dbModel_candidateProfile.Id && da.Status == (byte)RecordStatus.Active).ToListAsync();
                                var existingIds = dbModel_jobCandidateSkills.Select(da => da.Id).ToList();

                                if (candidateModel.JobDesirableSpecializations?.Count > 0)
                                {
                                    var GroupType = (byte)JobDesirableSkillGroupTypes.Specializations;
                                    foreach (var CandidateSkill in candidateModel.JobDesirableSpecializations)
                                    {
                                        var dbModel_jobCandidateSkill = dbModel_jobCandidateSkills.FirstOrDefault(da => da.GroupType == GroupType && da.TechnologyId == CandidateSkill.TechnologyId);
                                        if (dbModel_jobCandidateSkill == null)
                                        {
                                            dbModel_jobCandidateSkill = new PhJobCandidateSkillset
                                            {
                                                GroupType = GroupType,
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,

                                                TechnologyId = CandidateSkill.TechnologyId,
                                                ExpInMonths = CandidateSkill.ExpInMonths,
                                                ExpInYears = CandidateSkill.ExpInYears,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            };
                                            await dbContext.PhJobCandidateSkillsets.AddAsync(dbModel_jobCandidateSkill);
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobCandidateSkill.Id);
                                            dbModel_jobCandidateSkill.ExpInMonths = CandidateSkill.ExpInMonths;
                                            dbModel_jobCandidateSkill.ExpInYears = CandidateSkill.ExpInYears;

                                            dbModel_jobCandidateSkill.UpdatedBy = loginUserId;
                                            dbModel_jobCandidateSkill.UpdatedDate = CurrentTime;

                                            dbContext.PhJobCandidateSkillsets.Update(dbModel_jobCandidateSkill);
                                        }
                                    }
                                }
                                if (candidateModel.JobDesirableImplementations?.Count > 0)
                                {
                                    var GroupType = (byte)JobDesirableSkillGroupTypes.Implementations;
                                    foreach (var CandidateSkill in candidateModel.JobDesirableImplementations)
                                    {
                                        var dbModel_jobCandidateSkill = dbModel_jobCandidateSkills.FirstOrDefault(da => da.GroupType == GroupType && da.TechnologyId == CandidateSkill.TechnologyId);
                                        if (dbModel_jobCandidateSkill == null)
                                        {
                                            dbModel_jobCandidateSkill = new PhJobCandidateSkillset
                                            {
                                                GroupType = GroupType,
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,

                                                TechnologyId = CandidateSkill.TechnologyId,
                                                ExpInMonths = CandidateSkill.ExpInMonths,
                                                ExpInYears = CandidateSkill.ExpInYears,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            };
                                            await dbContext.PhJobCandidateSkillsets.AddAsync(dbModel_jobCandidateSkill);
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobCandidateSkill.Id);
                                            dbModel_jobCandidateSkill.ExpInMonths = CandidateSkill.ExpInMonths;
                                            dbModel_jobCandidateSkill.ExpInYears = CandidateSkill.ExpInYears;

                                            dbModel_jobCandidateSkill.UpdatedBy = loginUserId;
                                            dbModel_jobCandidateSkill.UpdatedDate = CurrentTime;

                                            dbContext.PhJobCandidateSkillsets.Update(dbModel_jobCandidateSkill);
                                        }
                                    }
                                }
                                if (candidateModel.JobDesirableDesigns?.Count > 0)
                                {
                                    var GroupType = (byte)JobDesirableSkillGroupTypes.Designs;
                                    foreach (var CandidateSkill in candidateModel.JobDesirableDesigns)
                                    {
                                        var dbModel_jobCandidateSkill = dbModel_jobCandidateSkills.FirstOrDefault(da => da.GroupType == GroupType && da.TechnologyId == CandidateSkill.TechnologyId);
                                        if (dbModel_jobCandidateSkill == null)
                                        {
                                            dbModel_jobCandidateSkill = new PhJobCandidateSkillset
                                            {
                                                GroupType = GroupType,
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,

                                                TechnologyId = CandidateSkill.TechnologyId,
                                                ExpInMonths = CandidateSkill.ExpInMonths,
                                                ExpInYears = CandidateSkill.ExpInYears,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            };
                                            await dbContext.PhJobCandidateSkillsets.AddAsync(dbModel_jobCandidateSkill);
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobCandidateSkill.Id);
                                            dbModel_jobCandidateSkill.ExpInMonths = CandidateSkill.ExpInMonths;
                                            dbModel_jobCandidateSkill.ExpInYears = CandidateSkill.ExpInYears;

                                            dbModel_jobCandidateSkill.UpdatedBy = loginUserId;
                                            dbModel_jobCandidateSkill.UpdatedDate = CurrentTime;

                                            dbContext.PhJobCandidateSkillsets.Update(dbModel_jobCandidateSkill);
                                        }
                                    }
                                }
                                if (candidateModel.JobDesirableDevelopments?.Count > 0)
                                {
                                    var GroupType = (byte)JobDesirableSkillGroupTypes.Developments;
                                    foreach (var CandidateSkill in candidateModel.JobDesirableDevelopments)
                                    {
                                        var dbModel_jobCandidateSkill = dbModel_jobCandidateSkills.FirstOrDefault(da => da.GroupType == GroupType && da.TechnologyId == CandidateSkill.TechnologyId);
                                        if (dbModel_jobCandidateSkill == null)
                                        {
                                            dbModel_jobCandidateSkill = new PhJobCandidateSkillset
                                            {
                                                GroupType = GroupType,
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,

                                                TechnologyId = CandidateSkill.TechnologyId,
                                                ExpInMonths = CandidateSkill.ExpInMonths,
                                                ExpInYears = CandidateSkill.ExpInYears,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            };
                                            await dbContext.PhJobCandidateSkillsets.AddAsync(dbModel_jobCandidateSkill);
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobCandidateSkill.Id);
                                            dbModel_jobCandidateSkill.ExpInMonths = CandidateSkill.ExpInMonths;
                                            dbModel_jobCandidateSkill.ExpInYears = CandidateSkill.ExpInYears;

                                            dbModel_jobCandidateSkill.UpdatedBy = loginUserId;
                                            dbModel_jobCandidateSkill.UpdatedDate = CurrentTime;

                                            dbContext.PhJobCandidateSkillsets.Update(dbModel_jobCandidateSkill);
                                        }
                                    }
                                }
                                if (candidateModel.JobDesirableSupports?.Count > 0)
                                {
                                    var GroupType = (byte)JobDesirableSkillGroupTypes.Supports;
                                    foreach (var CandidateSkill in candidateModel.JobDesirableSupports)
                                    {
                                        var dbModel_jobCandidateSkill = dbModel_jobCandidateSkills.FirstOrDefault(da => da.GroupType == GroupType && da.TechnologyId == CandidateSkill.TechnologyId);
                                        if (dbModel_jobCandidateSkill == null)
                                        {
                                            dbModel_jobCandidateSkill = new PhJobCandidateSkillset
                                            {
                                                GroupType = GroupType,
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,

                                                TechnologyId = CandidateSkill.TechnologyId,
                                                ExpInMonths = CandidateSkill.ExpInMonths,
                                                ExpInYears = CandidateSkill.ExpInYears,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            };
                                            await dbContext.PhJobCandidateSkillsets.AddAsync(dbModel_jobCandidateSkill);
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobCandidateSkill.Id);
                                            dbModel_jobCandidateSkill.ExpInMonths = CandidateSkill.ExpInMonths;
                                            dbModel_jobCandidateSkill.ExpInYears = CandidateSkill.ExpInYears;

                                            dbModel_jobCandidateSkill.UpdatedBy = loginUserId;
                                            dbModel_jobCandidateSkill.UpdatedDate = CurrentTime;

                                            dbContext.PhJobCandidateSkillsets.Update(dbModel_jobCandidateSkill);
                                        }
                                    }
                                }
                                if (candidateModel.JobDesirableQualities?.Count > 0)
                                {
                                    var GroupType = (byte)JobDesirableSkillGroupTypes.Qualities;
                                    foreach (var CandidateSkill in candidateModel.JobDesirableQualities)
                                    {
                                        var dbModel_jobCandidateSkill = dbModel_jobCandidateSkills.FirstOrDefault(da => da.GroupType == GroupType && da.TechnologyId == CandidateSkill.TechnologyId);
                                        if (dbModel_jobCandidateSkill == null)
                                        {
                                            dbModel_jobCandidateSkill = new PhJobCandidateSkillset
                                            {
                                                GroupType = GroupType,
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,

                                                TechnologyId = CandidateSkill.TechnologyId,
                                                ExpInMonths = CandidateSkill.ExpInMonths,
                                                ExpInYears = CandidateSkill.ExpInYears,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            };
                                            await dbContext.PhJobCandidateSkillsets.AddAsync(dbModel_jobCandidateSkill);
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobCandidateSkill.Id);
                                            dbModel_jobCandidateSkill.ExpInMonths = CandidateSkill.ExpInMonths;
                                            dbModel_jobCandidateSkill.ExpInYears = CandidateSkill.ExpInYears;

                                            dbModel_jobCandidateSkill.UpdatedBy = loginUserId;
                                            dbModel_jobCandidateSkill.UpdatedDate = CurrentTime;

                                            dbContext.PhJobCandidateSkillsets.Update(dbModel_jobCandidateSkill);
                                        }
                                    }
                                }
                                if (candidateModel.JobDesirableDocumentations?.Count > 0)
                                {
                                    var GroupType = (byte)JobDesirableSkillGroupTypes.Documentations;
                                    foreach (var CandidateSkill in candidateModel.JobDesirableDocumentations)
                                    {
                                        var dbModel_jobCandidateSkill = dbModel_jobCandidateSkills.FirstOrDefault(da => da.GroupType == GroupType && da.TechnologyId == CandidateSkill.TechnologyId);
                                        if (dbModel_jobCandidateSkill == null)
                                        {
                                            dbModel_jobCandidateSkill = new PhJobCandidateSkillset
                                            {
                                                GroupType = GroupType,
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,

                                                TechnologyId = CandidateSkill.TechnologyId,
                                                ExpInMonths = CandidateSkill.ExpInMonths,
                                                ExpInYears = CandidateSkill.ExpInYears,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            };
                                            await dbContext.PhJobCandidateSkillsets.AddAsync(dbModel_jobCandidateSkill);
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobCandidateSkill.Id);
                                            dbModel_jobCandidateSkill.ExpInMonths = CandidateSkill.ExpInMonths;
                                            dbModel_jobCandidateSkill.ExpInYears = CandidateSkill.ExpInYears;

                                            dbModel_jobCandidateSkill.UpdatedBy = loginUserId;
                                            dbModel_jobCandidateSkill.UpdatedDate = CurrentTime;

                                            dbContext.PhJobCandidateSkillsets.Update(dbModel_jobCandidateSkill);
                                        }
                                    }
                                }

                                foreach (var id in existingIds)
                                {
                                    var dbModel_jobCandidateSkill = dbModel_jobCandidateSkills.FirstOrDefault(da => da.Id == id);
                                    dbModel_jobCandidateSkill.Status = (byte)RecordStatus.Delete;
                                    dbModel_jobCandidateSkill.UpdatedBy = loginUserId;
                                    dbModel_jobCandidateSkill.UpdatedDate = CurrentTime;
                                }
                                await dbContext.SaveChangesAsync();
                            }

                            string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + dbModel_candidateProfile.Id + "";

                            // Checking for folder is available or not 
                            if (!Directory.Exists(webRootPath))
                            {
                                Directory.CreateDirectory(webRootPath);
                            }

                            // Resume
                            if (candidateModel.Resume?.Length > 0)
                            {
                                var dbModel_CandidateCv = await dbContext.PhCandidateDocs.Where(da => da.CandProfId == dbModel_candidateProfile.Id
                                                    && da.Joid == dbModel_JobCandidate.Id && da.FileGroup == (byte)FileGroup.Profile
                                                    && da.DocType == "Candidate CV" && da.Status != (byte)RecordStatus.Delete).OrderByDescending(da => da.CreatedDate).FirstOrDefaultAsync();

                                if (dbModel_CandidateCv != null)
                                {
                                    dbModel_CandidateCv.UpdatedBy = loginUserId;
                                    dbModel_CandidateCv.Status = (byte)RecordStatus.Delete;
                                    dbModel_CandidateCv.UpdatedDate = CurrentTime;
                                    dbContext.PhCandidateDocs.Update(dbModel_CandidateCv);
                                }

                                string fileName = Path.GetFileName(candidateModel.Resume.FileName);
                                fileName = fileName.Replace(" ", "_");

                                if (fileName.Length > 200)
                                {
                                    fileName = fileName.Substring(0, 199);
                                }
                                var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await candidateModel.Resume.CopyToAsync(fileStream);
                                }
                                string fileType = candidateModel.Resume.ContentType;

                                if (!string.IsNullOrEmpty(fileName))
                                {
                                    dbModel_CandidateCv = new PhCandidateDoc
                                    {
                                        //JoCandId = dbModel_JobCandidate.Id, 
                                        Joid = dbModel_JobCandidate.Joid,
                                        Status = (byte)RecordStatus.Active,
                                        CandProfId = dbModel_candidateProfile.Id,
                                        CreatedBy = loginUserId,
                                        CreatedDate = CurrentTime,
                                        UploadedBy = loginUserId,
                                        FileGroup = (byte)FileGroup.Profile,
                                        DocType = "Candidate CV",
                                        FileName = fileName,
                                        FileType = fileType,
                                        DocStatus = (byte)DocStatus.Notreviewd
                                    };

                                    dbContext.PhCandidateDocs.Add(dbModel_CandidateCv);
                                }

                                await dbContext.SaveChangesAsync();
                            }


                            // Payslips
                            {
                                var dbModel_candidatePaySlips = await dbContext.PhCandidateDocs.Where(da => da.CandProfId == dbModel_candidateProfile.Id
                                                                            && da.Joid == dbModel_JobCandidate.Joid && da.FileGroup == (byte)FileGroup.Profile
                                                                            && da.DocType == "Pay Slips" && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                                foreach (var dbModel_candidatePaySlip in dbModel_candidatePaySlips)
                                {
                                    if (candidateModel.PaySlipUrls.Contains(dbModel_candidatePaySlip.FileName) == false)
                                    {
                                        dbModel_candidatePaySlip.UpdatedBy = loginUserId;
                                        dbModel_candidatePaySlip.Status = (byte)RecordStatus.Delete;
                                        dbModel_candidatePaySlip.UpdatedDate = CurrentTime;
                                        dbContext.PhCandidateDocs.Update(dbModel_candidatePaySlip);                                        
                                    }
                                }
                                if (candidateModel.PaySlips?.Count > 0)
                                {
                                    foreach (var paySlip in candidateModel.PaySlips)
                                    {
                                        var fileName = Path.GetFileName(paySlip.FileName);
                                        fileName = fileName.Replace(" ", "_");
                                        if (fileName.Length > 200)
                                        {
                                            fileName = fileName.Substring(0, 199);
                                        }
                                        var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await paySlip.CopyToAsync(fileStream);
                                        }
                                        var dbModel_CandidatePaySlip = new PhCandidateDoc
                                        {
                                            //JoCandId = dbModel_JobCandidate.Id, 
                                            Joid = dbModel_JobCandidate.Joid,
                                            Status = (byte)RecordStatus.Active,
                                            CandProfId = dbModel_candidateProfile.Id,
                                            CreatedBy = loginUserId,
                                            CreatedDate = CurrentTime,
                                            UploadedBy = loginUserId,
                                            FileGroup = (byte)FileGroup.Profile,
                                            DocType = "Pay Slips",
                                            FileName = fileName,
                                            FileType = paySlip.ContentType,
                                            DocStatus = (byte)DocStatus.Notreviewd
                                        };

                                        dbContext.PhCandidateDocs.Add(dbModel_CandidatePaySlip);
                                        await dbContext.SaveChangesAsync();
                                    }
                                }
                                await dbContext.SaveChangesAsync();
                            }

                            // Video profile
                            if (candidateModel.CandVideoProfile?.Length > 0)
                            {
                                var dbModel_candidateVideoProfile = await dbContext.PhCandidateDocs.Where(da => da.CandProfId == dbModel_candidateProfile.Id
                                                                            && da.Joid == dbModel_JobCandidate.Joid && da.FileGroup == (byte)FileGroup.Profile
                                                                            && da.DocType == "Video Profile" && da.Status != (byte)RecordStatus.Delete).OrderByDescending(da => da.CreatedDate).FirstOrDefaultAsync();
                                if (dbModel_candidateVideoProfile != null)
                                {
                                    dbModel_candidateVideoProfile.UpdatedBy = loginUserId;
                                    dbModel_candidateVideoProfile.Status = (byte)RecordStatus.Delete;
                                    dbModel_candidateVideoProfile.UpdatedDate = CurrentTime;
                                    dbContext.PhCandidateDocs.Update(dbModel_candidateVideoProfile);
                                }

                                var fileName = Path.GetFileName(candidateModel.CandVideoProfile.FileName);
                                fileName = fileName.Replace(" ", "_");
                                if (fileName.Length > 200)
                                {
                                    fileName = fileName.Substring(0, 199);
                                }
                                var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await candidateModel.CandVideoProfile.CopyToAsync(fileStream);
                                }

                                dbModel_candidateVideoProfile = new PhCandidateDoc
                                {
                                    //JoCandId = dbModel_JobCandidate.Id, 
                                    Joid = dbModel_JobCandidate.Joid,
                                    Status = (byte)RecordStatus.Active,
                                    CandProfId = dbModel_candidateProfile.Id,
                                    CreatedBy = loginUserId,
                                    CreatedDate = CurrentTime,
                                    UploadedBy = loginUserId,
                                    FileGroup = (byte)FileGroup.Profile,
                                    DocType = "Video Profile",
                                    FileName = fileName,
                                    FileType = candidateModel.CandVideoProfile.ContentType,
                                    DocStatus = (byte)DocStatus.Notreviewd
                                };

                                dbContext.PhCandidateDocs.Add(dbModel_candidateVideoProfile);
                                await dbContext.SaveChangesAsync();
                            }

                            //Qualifications
                            {
                                var dbModel_jobOpeningQualifications = await dbContext.PhJobCandidateOpeningsQualifications.Where(da => da.JoCandId == dbModel_JobCandidate.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                                var existingIds = dbModel_jobOpeningQualifications.Select(da => da.Id).ToList();

                                //Candidate Education Qualification
                                if (candidateModel.CandidateQualificationModel?.Count > 0)
                                {
                                    var GroupType = (byte)JobCandQualCertGroupTypes.JobCandiateEducationQualification;
                                    foreach (var data in candidateModel.CandidateQualificationModel.Select(da => new { da.QualificationId, da.CourseId }).Distinct())
                                    {
                                        var _data = candidateModel.CandidateQualificationModel.FirstOrDefault(da => da.QualificationId == data.QualificationId && da.CourseId == data.CourseId);

                                        var dbModel_jobOpeningQualification = dbModel_jobOpeningQualifications.FirstOrDefault(da => da.GroupType == GroupType && da.QualificationId == data.QualificationId && da.CourseId == data.CourseId);
                                        if (dbModel_jobOpeningQualification == null)
                                        {
                                            dbContext.PhJobCandidateOpeningsQualifications.Add(new PhJobCandidateOpeningsQualification
                                            {
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,
                                                GroupType = GroupType,

                                                QualificationId = _data.QualificationId,
                                                PrefQualificationId = _data.Qualification?.Value,
                                                PrefQualification = _data.Qualification?.PreferenceType,

                                                CourseId = _data.CourseId,
                                                PrefCourseId = _data.Course?.Value,
                                                PrefCourse = _data.Course?.PreferenceType,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            });
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobOpeningQualification.Id);
                                        }
                                    }
                                }
                                //Opening Qualification
                                if (candidateModel.OpeningQualifications?.Count > 0)
                                {
                                    var GroupType = (byte)JobCandQualCertGroupTypes.JobOpeningQualification;
                                    foreach (var data in candidateModel.OpeningQualifications.Select(da => new { da.QualificationId }).Distinct())
                                    {
                                        var _data = candidateModel.OpeningQualifications.FirstOrDefault(da => da.QualificationId == data.QualificationId);

                                        var dbModel_jobOpeningQualification = dbModel_jobOpeningQualifications.FirstOrDefault(da => da.GroupType == GroupType && da.QualificationId == data.QualificationId);
                                        if (dbModel_jobOpeningQualification == null)
                                        {
                                            dbModel_jobOpeningQualification = new PhJobCandidateOpeningsQualification
                                            {
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,
                                                GroupType = GroupType,

                                                QualificationId = _data.QualificationId,
                                                PrefQualificationId = _data.Qualification?.Value,
                                                PrefQualification = _data.Qualification?.PreferenceType,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            };
                                            dbContext.PhJobCandidateOpeningsQualifications.Add(dbModel_jobOpeningQualification);
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobOpeningQualification.Id);
                                        }
                                    }
                                }

                                foreach (var id in existingIds)
                                {
                                    var dbModel_jobOpeningAssmt = dbModel_jobOpeningQualifications.FirstOrDefault(da => da.Id == id);
                                    dbModel_jobOpeningAssmt.Status = (byte)RecordStatus.Delete;
                                    dbModel_jobOpeningAssmt.UpdatedBy = loginUserId;
                                    dbModel_jobOpeningAssmt.UpdatedDate = CurrentTime;
                                }
                                await dbContext.SaveChangesAsync();
                            }

                            //certifications
                            {
                                var dbModel_jobOpeningCertifications = await dbContext.PhJobCandidateOpeningsCertifications.Where(da => da.JoCandId == dbModel_JobCandidate.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                                var existingIds = dbModel_jobOpeningCertifications.Select(da => da.Id).ToList();

                                //Candidate Education Certification
                                if (candidateModel.CandidateCertificationModel?.Count > 0)
                                {
                                    var GroupType = (byte)JobCandQualCertGroupTypes.JobCandidateEducationCertifications;
                                    foreach (var data in candidateModel.CandidateCertificationModel.Select(da => new { da.CertificationId }).Distinct())
                                    {
                                        var _data = candidateModel.CandidateCertificationModel.FirstOrDefault(da => da.CertificationId == data.CertificationId);

                                        var dbModel_jobOpeningCertification = dbModel_jobOpeningCertifications.FirstOrDefault(da => da.GroupType == GroupType && da.CertificationId == data.CertificationId);
                                        if (dbModel_jobOpeningCertification == null)
                                        {
                                            dbContext.PhJobCandidateOpeningsCertifications.Add(new PhJobCandidateOpeningsCertification
                                            {
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,
                                                GroupType = GroupType,

                                                CertificationId = _data.CertificationId,
                                                PrefCertificationId = _data.Certification?.Value,
                                                PrefCertification = _data.Certification?.PreferenceType,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            });
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobOpeningCertification.Id);
                                        }
                                    }
                                }
                                //Opening Certification
                                if (candidateModel.OpeningCertifications?.Count > 0)
                                {
                                    var GroupType = (byte)JobCandQualCertGroupTypes.JobOpeningCertification;
                                    foreach (var data in candidateModel.OpeningCertifications.Select(da => new { da.CertificationId }).Distinct())
                                    {
                                        var _data = candidateModel.OpeningCertifications.FirstOrDefault(da => da.CertificationId == data.CertificationId);

                                        var dbModel_jobOpeningCertification = dbModel_jobOpeningCertifications.FirstOrDefault(da => da.GroupType == GroupType && da.CertificationId == data.CertificationId);
                                        if (dbModel_jobOpeningCertification == null)
                                        {
                                            dbModel_jobOpeningCertification = new PhJobCandidateOpeningsCertification
                                            {
                                                JoCandId = dbModel_JobCandidate.Id,
                                                Joid = dbModel_JobCandidate.Joid,
                                                CandProfId = dbModel_candidateProfile.Id,
                                                GroupType = GroupType,

                                                CertificationId = _data.CertificationId,
                                                PrefCertificationId = _data.Certification?.Value,
                                                PrefCertification = _data.Certification?.PreferenceType,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = loginUserId,
                                                CreatedDate = CurrentTime,
                                            };
                                            dbContext.PhJobCandidateOpeningsCertifications.Add(dbModel_jobOpeningCertification);
                                        }
                                        else
                                        {
                                            existingIds.Remove(dbModel_jobOpeningCertification.Id);
                                        }
                                    }
                                }

                                foreach (var id in existingIds)
                                {
                                    var dbModel_jobOpeningCertification = dbModel_jobOpeningCertifications.FirstOrDefault(da => da.Id == id);
                                    dbModel_jobOpeningCertification.Status = (byte)RecordStatus.Delete;
                                    dbModel_jobOpeningCertification.UpdatedBy = loginUserId;
                                    dbModel_jobOpeningCertification.UpdatedDate = CurrentTime;
                                }
                                await dbContext.SaveChangesAsync();
                            }


                            // Audit
                            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                            var auditLog = new CreateAuditViewModel
                            {
                                ActivitySubject = "Updated Candidate",
                                ActivityDesc = " updated the Candidate details successfully",
                                ActivityType = (byte)AuditActivityType.RecordUpdates,
                                TaskID = dbModel_candidateProfile.Id,
                                UserId = loginUserId
                            };
                            audList.Add(auditLog);
                            SaveAuditLog(audList);

                            // activity
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Candidate,
                                ActivityOn = candidateModel.Id,
                                ActivityType = (byte)LogActivityType.RecordUpdates,
                                JobId = dbModel_JobCandidate.Id,
                                ActivityDesc = " has updated the Candidate details successfully",
                                UserId = loginUserId
                            };
                            activityList.Add(activityLog);
                            SaveActivity(activityList);

                            if (loginUserType == UserType.Candidate)
                            {
                                loginUserId = dbModel_JobCandidate.RecruiterId ?? loginUserId;
                            }

                            if (dbModel_JobCandidate != null)
                            {
                                // Applying workflow rule 
                                var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                                {
                                    ActionMode = (byte)WorkflowActionMode.Candidate,
                                    CanProfId = dbModel_JobCandidate.CandProfId,
                                    JobId = dbModel_JobCandidate.Joid,
                                    TaskCode = TaskCode.CAJ.ToString(),
                                    UserId = loginUserId,
                                    CurrentStatusId = dbModel_JobCandidate.CandProfStatus
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
                                            CreatedBy = loginUserId,
                                            IsAudioNotify = true
                                        };
                                        notificationPushedViewModel.Add(notificationPushed);
                                    }
                                }
                            }

                            await trans.CommitAsync();
                            respModel.SetResult(" Updated Successfully");
                        }
                        else
                        {
                            logger.Log(LogLevel.Warning, LoggingEvents.InsertItem, $"candProfileId:{candProfileId} is invalid." + Newtonsoft.Json.JsonConvert.SerializeObject(candidateModel), respModel.Meta.RequestID);
                            respModel.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, "The Email Address is already associated to other job", true);
                        }
                    }
                    else
                    {
                        logger.Log(LogLevel.Warning, LoggingEvents.InsertItem, $"Job:{candidateModel.JobId} is invalid." + Newtonsoft.Json.JsonConvert.SerializeObject(candidateModel), respModel.Meta.RequestID);
                        respModel.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The Job is not available", true);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",candidate update respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateModel), respModel.Meta.RequestID, ex);
                    respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        #endregion

        #region Sourcing Candidate
        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> CreateCandidate(CreateCandidateViewModel createCandidateViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = " Created Successfully";
            int UserId = Usr.Id;
            var pswd = string.Empty;
            int RecId = UserId;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    var Opening = await (from job in dbContext.PhJobOpenings
                                         join jobStatus in dbContext.PhJobStatusSes on job.JobOpeningStatus equals jobStatus.Id
                                         where job.Id == createCandidateViewModel.JobId
                                         select new
                                         {
                                             job.Id,
                                             job.JobOpeningStatus,
                                             job.JobTitle,
                                             jobStatus.Jscode
                                         }).FirstOrDefaultAsync();
                    if (Opening != null)
                    {
                        if (Opening.Jscode != JobStatusCodes.CLS.ToString())
                        {
                            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                            var EmailAddress = createCandidateViewModel.EmailAddress?.Trim();
                            var candidate = await dbContext.PhCandidateProfiles.Where(x => x.EmailId == EmailAddress).FirstOrDefaultAsync();
                            if (candidate == null)
                            {
                                byte ProfCompStatus = 0;
                                if (createCandidateViewModel.MaritalStatus > 0 &&
                                    !string.IsNullOrEmpty(createCandidateViewModel.TotalExperiance) && !string.IsNullOrEmpty(createCandidateViewModel.RelevantExperiance))
                                {
                                    ProfCompStatus = 1;
                                }
                                if (!string.IsNullOrEmpty(createCandidateViewModel.CPCurrency) &&
                                    createCandidateViewModel.CPGrossPayPerAnnum != 0 &&
                                    createCandidateViewModel.EPTakeHomeSalPerMonth != 0 &&
                                    !string.IsNullOrEmpty(createCandidateViewModel.EPCurrency))
                                {
                                    ProfCompStatus = 2;
                                }

                                double Rating = 0;
                                if (createCandidateViewModel.CreateCandidateSkillViewModel != null)
                                {
                                    if (createCandidateViewModel.CreateCandidateSkillViewModel.Count > 0)
                                    {
                                        var CanSkillCount = createCandidateViewModel.CreateCandidateSkillViewModel.Count();
                                        var CanSkillRating = createCandidateViewModel.CreateCandidateSkillViewModel.Sum(x => x.SelfRating);
                                        double value1 = (double)CanSkillRating / (double)CanSkillCount;
                                        if (value1 > 0)
                                        {
                                            Rating = Math.Round(value1, 1, MidpointRounding.AwayFromZero);
                                        }
                                    }
                                }

                                var Candidate_ = new PhCandidateProfile
                                {
                                    CandName = createCandidateViewModel.FullName,
                                    EmailId = EmailAddress,
                                    SourceId = createCandidateViewModel.SourceId,
                                    ContactNo = createCandidateViewModel.ContactNo,
                                    Remarks = createCandidateViewModel.Remarks,

                                    ValidPpflag = createCandidateViewModel.ValidPpflag,
                                    AlteContactNo = createCandidateViewModel.AlteContactNo,
                                    FullNameInPp = createCandidateViewModel.FullName,
                                    Dob = createCandidateViewModel.CandidateDOB,
                                    Gender = createCandidateViewModel.Gender,
                                    MaritalStatus = createCandidateViewModel.MaritalStatus,
                                    NoticePeriod = createCandidateViewModel.NoticePeriod,
                                    ReasonsForReloc = createCandidateViewModel.ReasonsForReloc,

                                    CurrLocation = createCandidateViewModel.CurrLocation,
                                    CurrLocationId = createCandidateViewModel.CurrLocationID,
                                    CountryId = createCandidateViewModel.CountryID,
                                    Nationality = createCandidateViewModel.Nationality,

                                    CurrOrganization = createCandidateViewModel.CurrOrganization,
                                    CurrEmplFlag = createCandidateViewModel.CurrEmplFlag,
                                    CurrDesignation = createCandidateViewModel.CurrDesignation,

                                    Experience = createCandidateViewModel.TotalExperiance,
                                    RelevantExperience = createCandidateViewModel.RelevantExperiance,
                                    ExperienceInMonths = createCandidateViewModel.TotalExperiance != string.Empty ? ConvertMonths(createCandidateViewModel.TotalExperiance) : 0,
                                    ReleExpeInMonths = createCandidateViewModel.RelevantExperiance != string.Empty ? ConvertMonths(createCandidateViewModel.RelevantExperiance) : 0,

                                    CandOverallStatus = (byte)CandOverallStatus.Available,
                                    ProfUpdateFlag = false,
                                    ProfCompStatus = ProfCompStatus,
                                    ProfTaggedFlag = false,

                                    Cpcurrency = createCandidateViewModel.CPCurrency,
                                    CpgrossPayPerAnnum = createCandidateViewModel.CPGrossPayPerAnnum,
                                    CpdeductionsPerAnnum = createCandidateViewModel.CPDeductionsPerAnnum,
                                    CpvariablePayPerAnnum = createCandidateViewModel.CPVariablePayPerAnnum,
                                    CptakeHomeSalPerMonth = createCandidateViewModel.CPTakeHomeSalPerMonth,

                                    Epcurrency = createCandidateViewModel.EPCurrency,
                                    EptakeHomeSalPerMonth = createCandidateViewModel.EPTakeHomeSalPerMonth,

                                    Status = (byte)RecordStatus.Active,
                                    CreatedBy = UserId,
                                    CreatedDate = CurrentTime,

                                    OverallRating = Rating
                                };

                                dbContext.PhCandidateProfiles.Add(Candidate_);
                                await dbContext.SaveChangesAsync();

                                var User_ = dbContext.PiHireUsers.Where(x => x.UserName == EmailAddress).FirstOrDefault();
                                if (User_ == null)
                                {
                                    var generator = new RandomGenerator();
                                    pswd = generator.RandomPassword(8);
                                    var HashPswd = Hashification.SHA(pswd);
                                    User_ = new PiHireUser
                                    {
                                        CreatedBy = UserId,
                                        CreatedDate = CurrentTime,
                                        FirstName = createCandidateViewModel.FullName,
                                        LastName = string.Empty,
                                        MobileNumber = createCandidateViewModel.ContactNo,
                                        Status = (byte)RecordStatus.Active,
                                        UserId = 0,
                                        UserName = EmailAddress,
                                        UserType = (byte)UserType.Candidate,
                                        UserRoleId = 0,
                                        UserRoleName = string.Empty,
                                        Location = createCandidateViewModel.CurrLocation,
                                        LocationId = createCandidateViewModel.CurrLocationID,
                                        PasswordHash = HashPswd,
                                        VerifiedFlag = true,
                                        EmailId = EmailAddress
                                    };
                                    dbContext.PiHireUsers.Add(User_);
                                    await dbContext.SaveChangesAsync();
                                }
                                else
                                {
                                    var generator = new RandomGenerator();
                                    pswd = generator.RandomPassword(8);
                                    var HashPswd = Hashification.SHA(pswd);
                                    User_.PasswordHash = HashPswd;
                                    User_.VerifiedFlag = true;

                                    dbContext.PiHireUsers.Update(User_);
                                    await dbContext.SaveChangesAsync();
                                }


                                DateTime? ProfReceDate = null;
                                ProfReceDate = CurrentTime;
                                var JobAsignment = new PhJobAssignment();
                                JobAsignment = null;

                                if (Usr.UserTypeId == (byte)UserType.Recruiter)
                                {
                                    JobAsignment = dbContext.PhJobAssignments.Where(x => x.Joid == createCandidateViewModel.JobId && x.AssignedTo == UserId).FirstOrDefault();
                                    if (JobAsignment != null)
                                    {
                                        if (JobAsignment.ProfilesUploaded == null)
                                        {
                                            JobAsignment.ProfilesUploaded = 0;
                                        }
                                        JobAsignment.ProfilesUploaded = (short)(JobAsignment.ProfilesUploaded + 1);
                                        JobAsignment.UpdatedBy = UserId;
                                        JobAsignment.UpdatedDate = CurrentTime;
                                        PhJobAssignmentsDayWise_records(ref JobAsignment, CurrentTime, incrementCvsuploaded: 1);
                                        dbContext.PhJobAssignments.Update(JobAsignment);
                                        await dbContext.SaveChangesAsync();
                                    }
                                }
                                if (JobAsignment == null)
                                {
                                    JobAsignment = dbContext.PhJobAssignments.Where(x => x.Joid == createCandidateViewModel.JobId).OrderBy(x => x.DeassignDate).FirstOrDefault();
                                    if (JobAsignment != null)
                                    {
                                        if (JobAsignment.ProfilesUploaded == null)
                                        {
                                            JobAsignment.ProfilesUploaded = 0;
                                        }
                                        JobAsignment.ProfilesUploaded = (short)(JobAsignment.ProfilesUploaded + 1);
                                        JobAsignment.UpdatedBy = UserId;
                                        JobAsignment.UpdatedDate = CurrentTime;
                                        PhJobAssignmentsDayWise_records(ref JobAsignment, CurrentTime, incrementCvsuploaded: 1);
                                        dbContext.PhJobAssignments.Update(JobAsignment);
                                        await dbContext.SaveChangesAsync();
                                    }
                                    if (Usr.UserTypeId == (byte)UserType.Candidate)
                                    {
                                        RecId = JobAsignment.AssignedTo;
                                    }
                                }

                                if (Candidate_.Id != 0)
                                {
                                    var JobCandidate_ = new PhJobCandidate
                                    {
                                        CandProfId = Candidate_.Id,
                                        Joid = createCandidateViewModel.JobId,

                                        //RecruiterId = JobAsignment == null ? 0 : JobAsignment.AssignedTo,
                                        RecruiterId = RecId,
                                        EmailSentFlag = false,

                                        Epcurrency = createCandidateViewModel.EPCurrency,
                                        EpdeductionsPerAnnum = 0,
                                        EpgrossPayPerAnnum = 0,
                                        EptakeHomePerMonth = createCandidateViewModel.EPTakeHomeSalPerMonth,

                                        OpconfirmDate = CurrentTime,
                                        OpconfirmFlag = false,

                                        Opcurrency = string.Empty,
                                        OpdeductionsPerAnnum = 0,
                                        OpgrossPayPerAnnum = 0,
                                        OptakeHomePerMonth = 0,
                                        OpvarPayPerAnnum = 0,
                                        OpgrossPayPerMonth = 0,
                                        OpnetPayPerAnnum = 0,
                                        ProfileUpdateFlag = false,
                                        ProfReceDate = ProfReceDate,
                                        SelfRating = Rating,
                                        BgvacceptedFlag = false,
                                        IsTagged = false,

                                        Status = (byte)RecordStatus.Active,
                                        CreatedBy = UserId,
                                        CreatedDate = CurrentTime
                                    };
                                    dbContext.PhJobCandidates.Add(JobCandidate_);
                                    await dbContext.SaveChangesAsync();

                                    if (createCandidateViewModel.CreateCandidateSkillViewModel != null)
                                    {
                                        if (createCandidateViewModel.CreateCandidateSkillViewModel.Count > 0)
                                        {
                                            foreach (var item in createCandidateViewModel.CreateCandidateSkillViewModel)
                                            {
                                                var candSkilSet = new PhCandidateSkillset
                                                {
                                                    CandProfId = Candidate_.Id,
                                                    CreatedBy = UserId,
                                                    CreatedDate = CurrentTime,
                                                    SelfRating = item.SelfRating,
                                                    ExpInMonths = item.ExpInMonths,
                                                    ExpInYears = item.ExpInYears,
                                                    TechnologyId = item.TechnologyId,
                                                    Status = (byte)RecordStatus.Active
                                                };
                                                dbContext.PhCandidateSkillsets.Add(candSkilSet);
                                                await dbContext.SaveChangesAsync();
                                            }
                                        }
                                    }

                                    string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + Candidate_.Id + "";
                                    //checking for folder is available or not 
                                    if (!Directory.Exists(webRootPath))
                                    {
                                        Directory.CreateDirectory(webRootPath);
                                    }
                                    //photo
                                    if (createCandidateViewModel.Photo != null)
                                    {
                                        if (createCandidateViewModel.Photo.Length > 0)
                                        {
                                            var fileName = Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + createCandidateViewModel.Photo.FileName);
                                            fileName = fileName.Replace(" ", "_");
                                            if (fileName.Length > 200)
                                            {
                                                fileName = fileName.Substring(0, 199);
                                            }

                                            var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                                            {
                                                await createCandidateViewModel.Photo.CopyToAsync(fileStream);
                                            }

                                            var candidateDoc = new PhCandidateDoc
                                            {
                                                Joid = createCandidateViewModel.JobId,
                                                Status = (byte)RecordStatus.Active,
                                                CandProfId = Candidate_.Id,
                                                CreatedBy = UserId,
                                                CreatedDate = CurrentTime,
                                                UploadedBy = UserId,
                                                FileGroup = (byte)FileGroup.Profile,
                                                DocType = "Profile Photo",
                                                FileName = fileName,
                                                FileType = createCandidateViewModel.Photo.ContentType,
                                                DocStatus = (byte)DocStatus.Accepted
                                            };

                                            var cand = await dbContext.PiHireUsers.Where(x => x.UserName == Candidate_.EmailId).FirstOrDefaultAsync();
                                            if (cand != null)
                                            {
                                                if (!string.IsNullOrEmpty(fileName))
                                                {
                                                    fileName = fileName.Replace("#", "%23");
                                                }
                                                cand.ProfilePhoto = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + Candidate_.Id + "/" + fileName;
                                                dbContext.PiHireUsers.Update(cand);
                                            }
                                            dbContext.PhCandidateDocs.Add(candidateDoc);

                                            await dbContext.SaveChangesAsync();
                                        }
                                    }

                                    //resume
                                    if (createCandidateViewModel.Resume != null || !string.IsNullOrEmpty(createCandidateViewModel.ResumeURL))
                                    {
                                        string fileName = string.Empty;
                                        string fileType = string.Empty;
                                        fileName = createCandidateViewModel.ResumeURL;
                                        fileType = createCandidateViewModel.ResumeURLType;
                                        if (createCandidateViewModel.Resume != null)
                                        {
                                            if (createCandidateViewModel.Resume.Length > 0)
                                            {
                                                fileName = Path.GetFileName(createCandidateViewModel.Resume.FileName);
                                                fileName = fileName.Replace(" ", "_");
                                                if (fileName.Length > 200)
                                                {
                                                    fileName = fileName.Substring(0, 199);
                                                }
                                                var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                                {
                                                    await createCandidateViewModel.Resume.CopyToAsync(fileStream);
                                                }
                                                fileType = createCandidateViewModel.Resume.ContentType;
                                            }
                                        }

                                        var CandDoc = dbContext.PhCandidateDocs.Where(x => x.CandProfId == Candidate_.Id
                                        && x.Joid == createCandidateViewModel.JobId && x.FileGroup == (byte)FileGroup.Profile && x.DocType == "Candidate CV" &&
                                       x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                                        if (CandDoc != null)
                                        {
                                            CandDoc.UpdatedBy = UserId;
                                            CandDoc.Status = (byte)RecordStatus.Delete;
                                            CandDoc.UpdatedDate = CurrentTime;
                                            dbContext.PhCandidateDocs.Update(CandDoc);
                                            await dbContext.SaveChangesAsync();
                                        }

                                        if (!string.IsNullOrEmpty(fileName))
                                        {
                                            string Doctype = string.Empty;
                                            if (Usr.UserTypeId == (byte)UserType.Recruiter)
                                            {
                                                Doctype = "Base CV";
                                            }
                                            else
                                            {
                                                Doctype = "Candidate CV";
                                            }
                                            var candidateDoc = new PhCandidateDoc
                                            {
                                                Joid = createCandidateViewModel.JobId,
                                                Status = (byte)RecordStatus.Active,
                                                CandProfId = Candidate_.Id,
                                                CreatedBy = UserId,
                                                CreatedDate = CurrentTime,
                                                UploadedBy = UserId,
                                                FileGroup = (byte)FileGroup.Profile,
                                                DocType = Doctype,
                                                FileName = fileName,
                                                FileType = fileType,
                                                DocStatus = (byte)DocStatus.Notreviewd
                                            };

                                            dbContext.PhCandidateDocs.Add(candidateDoc);
                                            await dbContext.SaveChangesAsync();
                                        }

                                    }

                                    //payslips
                                    if (createCandidateViewModel.PaySlips != null)
                                    {
                                        if (createCandidateViewModel.PaySlips.Count > 0)
                                        {
                                            foreach (var item in createCandidateViewModel.PaySlips)
                                            {
                                                var fileName = Path.GetFileName(item.FileName);
                                                fileName = fileName.Replace(" ", "_");
                                                if (fileName.Length > 200)
                                                {
                                                    fileName = fileName.Substring(0, 199);
                                                }
                                                var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                                {
                                                    await item.CopyToAsync(fileStream);
                                                }
                                                var candidateDoc = new PhCandidateDoc
                                                {
                                                    Joid = createCandidateViewModel.JobId,
                                                    Status = (byte)RecordStatus.Active,
                                                    CandProfId = Candidate_.Id,
                                                    CreatedBy = UserId,
                                                    CreatedDate = CurrentTime,
                                                    UploadedBy = UserId,
                                                    FileGroup = (byte)FileGroup.Profile,
                                                    DocType = "Pay Slips",
                                                    FileName = fileName,
                                                    FileType = item.ContentType,
                                                    DocStatus = (byte)DocStatus.Notreviewd
                                                };

                                                dbContext.PhCandidateDocs.Add(candidateDoc);
                                                await dbContext.SaveChangesAsync();
                                            }
                                        }
                                    }

                                    if (createCandidateViewModel.PaySlipsModel != null)
                                    {
                                        if (createCandidateViewModel.PaySlipsModel.Count > 0)
                                        {
                                            foreach (var item in createCandidateViewModel.PaySlipsModel)
                                            {
                                                var candidateDoc = new PhCandidateDoc
                                                {
                                                    Joid = createCandidateViewModel.JobId,
                                                    Status = (byte)RecordStatus.Active,
                                                    CandProfId = Candidate_.Id,
                                                    CreatedBy = UserId,
                                                    CreatedDate = CurrentTime,
                                                    UploadedBy = UserId,
                                                    FileGroup = (byte)FileGroup.Profile,
                                                    DocType = "Pay Slips",
                                                    FileName = item.URL,
                                                    FileType = item.Type,
                                                    DocStatus = (byte)DocStatus.Notreviewd
                                                };

                                                dbContext.PhCandidateDocs.Add(candidateDoc);
                                                await dbContext.SaveChangesAsync();
                                            }
                                        }
                                    }

                                    //videoprofile
                                    if (createCandidateViewModel.CandVideoProfile != null)
                                    {
                                        if (createCandidateViewModel.CandVideoProfile.Length > 0)
                                        {
                                            var fileName = Path.GetFileName(createCandidateViewModel.CandVideoProfile.FileName);
                                            fileName = fileName.Replace(" ", "_");
                                            if (fileName.Length > 200)
                                            {
                                                fileName = fileName.Substring(0, 199);
                                            }
                                            var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                                            {
                                                await createCandidateViewModel.CandVideoProfile.CopyToAsync(fileStream);
                                            }
                                            var candidateDoc = new PhCandidateDoc
                                            {
                                                Joid = createCandidateViewModel.JobId,
                                                Status = (byte)RecordStatus.Active,
                                                CandProfId = Candidate_.Id,
                                                CreatedBy = UserId,
                                                CreatedDate = CurrentTime,
                                                UploadedBy = UserId,
                                                FileGroup = (byte)FileGroup.Profile,
                                                DocType = "Video Profile",
                                                FileName = fileName,
                                                FileType = createCandidateViewModel.CandVideoProfile.ContentType,
                                                DocStatus = (byte)DocStatus.Notreviewd
                                            };


                                            dbContext.PhCandidateDocs.Add(candidateDoc);
                                            await dbContext.SaveChangesAsync();
                                        }
                                    }

                                    //qualification
                                    if (createCandidateViewModel.CandidateQualificationModel != null)
                                    {
                                        if (createCandidateViewModel.CandidateQualificationModel.Count > 0)
                                        {
                                            foreach (var qualification in createCandidateViewModel.CandidateQualificationModel)
                                            {
                                                var phCandidateEduDetails1 = dbContext.PhCandidateEduDetails.Where(x => x.CandProfId == Candidate_.Id && x.QualificationId == qualification.QualificationId && x.CourseId == qualification.CourseId && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                                                if (phCandidateEduDetails1 == null)
                                                {
                                                    if (qualification.Qualification.Length > 50)
                                                    {
                                                        qualification.Qualification = qualification.Qualification.Substring(0, 50);
                                                    }
                                                    if (qualification.Course.Length > 100)
                                                    {
                                                        qualification.Course = qualification.Course.Substring(0, 100);
                                                    }

                                                    var phCandidateEduDetails = new PhCandidateEduDetail
                                                    {
                                                        Status = (byte)RecordStatus.Active,
                                                        CandProfId = Candidate_.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = CurrentTime,
                                                        Qualification = qualification.Qualification,
                                                        Course = qualification.Course,
                                                        QualificationId = qualification.QualificationId,
                                                        CourseId = qualification.CourseId
                                                    };

                                                    dbContext.PhCandidateEduDetails.Add(phCandidateEduDetails);
                                                    await dbContext.SaveChangesAsync();
                                                }
                                            }
                                        }
                                    }

                                    //ceritification
                                    if (createCandidateViewModel.CandidateCertificationModel != null)
                                    {
                                        if (createCandidateViewModel.CandidateCertificationModel.Count > 0)
                                        {
                                            foreach (var certification in createCandidateViewModel.CandidateCertificationModel)
                                            {
                                                var phCandidateCertifications = dbContext.PhCandidateCertifications.Where(x => x.CandProfId == Candidate_.Id && x.CertificationId == certification.CertificationID && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                                                if (phCandidateCertifications == null)
                                                {
                                                    phCandidateCertifications = new PhCandidateCertification
                                                    {
                                                        Status = (byte)RecordStatus.Active,
                                                        CandProfId = Candidate_.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = CurrentTime,
                                                        CertificationId = certification.CertificationID
                                                    };

                                                    dbContext.PhCandidateCertifications.Add(phCandidateCertifications);
                                                    await dbContext.SaveChangesAsync();
                                                }
                                            }
                                        }
                                    }

                                    //questionResponse
                                    if (createCandidateViewModel.CandidateQuestionResponseModel != null)
                                    {
                                        if (createCandidateViewModel.CandidateQuestionResponseModel.Count > 0)
                                        {
                                            foreach (var qtnResponse in createCandidateViewModel.CandidateQuestionResponseModel)
                                            {

                                                var phJobCandidateStResponses = new PhJobCandidateStResponse
                                                {
                                                    Status = (byte)RecordStatus.Active,
                                                    CandProfId = Candidate_.Id,
                                                    CreatedBy = UserId,
                                                    CreatedDate = CurrentTime,
                                                    Joid = createCandidateViewModel.JobId,
                                                    Response = qtnResponse.Response,
                                                    StquestionId = qtnResponse.QuestionId
                                                };

                                                dbContext.PhJobCandidateStResponses.Add(phJobCandidateStResponses);
                                                await dbContext.SaveChangesAsync();
                                            }
                                        }
                                    }


                                    var jobAddlDtls = dbContext.PhJobOpeningsAddlDetails.Where(x => x.Joid == createCandidateViewModel.JobId).FirstOrDefault();
                                    if (jobAddlDtls != null)
                                    {
                                        if (jobAddlDtls.NoOfCvsFilled == null)
                                        {
                                            jobAddlDtls.NoOfCvsFilled = 0;
                                        }
                                        jobAddlDtls.NoOfCvsFilled += 1;
                                    }
                                }


                                // audit
                                var auditLog = new CreateAuditViewModel
                                {
                                    ActivitySubject = " Created Candidate",
                                    ActivityDesc = " Created new candidate - " + createCandidateViewModel.FullName + " ",
                                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                                    TaskID = Candidate_.Id,
                                    UserId = UserId
                                };
                                audList.Add(auditLog);
                                SaveAuditLog(audList);

                                if (Usr.UserTypeId == (byte)UserType.Candidate)
                                {
                                    UserId = RecId;
                                }

                                // Activity 
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Candidate,
                                    ActivityOn = Candidate_.Id,
                                    JobId = createCandidateViewModel.JobId,
                                    ActivityType = (byte)LogActivityType.RecordUpdates,
                                    ActivityDesc = " Created new candidate - " + createCandidateViewModel.FullName + " ",
                                    UserId = UserId
                                };
                                activityList.Add(activityLog);
                                SaveActivity(activityList);

                                // Applying workflow conditions 
                                var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                                {
                                    ActionMode = (byte)WorkflowActionMode.Candidate,
                                    CanProfId = Candidate_.Id,
                                    JobId = createCandidateViewModel.JobId,
                                    UserId = UserId
                                };
                                bool IsAudioNotify = false;
                                if (Usr.UserTypeId == (byte)UserType.Candidate)
                                {
                                    var curentStatus = dbContext.PhCandStatusSes.Where(x => x.Cscode == "CCV").Select(x => x.Id).FirstOrDefault();
                                    workFlowRuleSearchViewModel.TaskCode = TaskCode.CAJ.ToString();
                                    workFlowRuleSearchViewModel.CurrentStatusId = curentStatus;
                                    IsAudioNotify = true;
                                }
                                else
                                {
                                    workFlowRuleSearchViewModel.TaskCode = TaskCode.CCD.ToString();
                                    workFlowRuleSearchViewModel.CurrentStatusId = null;
                                    workFlowRuleSearchViewModel.UserPassword = pswd;
                                    workFlowRuleSearchViewModel.UserName = User_.UserName;
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
                                            CreatedBy = UserId,
                                            IsAudioNotify = IsAudioNotify
                                        };
                                        notificationPushedViewModel.Add(notificationPushed);
                                    }
                                }

                                respModel.Status = true;
                                respModel.SetResult(message);
                            }
                            else
                            {
                                var MapOpening = await dbContext.PhJobOpenings.Where(x => x.Id == createCandidateViewModel.JobId).FirstOrDefaultAsync();
                                if (MapOpening != null)
                                {

                                    var Mapcandidate = await dbContext.PhCandidateProfiles.Where(x => x.EmailId == EmailAddress).FirstOrDefaultAsync();
                                    if (Mapcandidate != null)
                                    {
                                        var MapjobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == createCandidateViewModel.JobId
                                        && x.CandProfId == Mapcandidate.Id).FirstOrDefaultAsync();

                                        if (MapjobCandidate == null)
                                        {
                                            string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + Mapcandidate.Id + "";
                                            // Checking for folder is available or not 
                                            if (!Directory.Exists(webRootPath))
                                            {
                                                Directory.CreateDirectory(webRootPath);
                                            }

                                            if (string.IsNullOrEmpty(createCandidateViewModel.EPCurrency))
                                            {
                                                createCandidateViewModel.EPCurrency = Mapcandidate.Epcurrency;
                                            }
                                            if (createCandidateViewModel.EPTakeHomeSalPerMonth == null || createCandidateViewModel.EPTakeHomeSalPerMonth == 0)
                                            {
                                                createCandidateViewModel.EPTakeHomeSalPerMonth = Mapcandidate.EptakeHomeSalPerMonth;
                                            }

                                            Mapcandidate.ProfTaggedFlag = true;
                                            Mapcandidate.UpdatedBy = UserId;
                                            Mapcandidate.UpdatedDate = CurrentTime;

                                            var jobAddlDtls = dbContext.PhJobOpeningsAddlDetails.Where(x => x.Joid == createCandidateViewModel.JobId).FirstOrDefault();
                                            if (jobAddlDtls != null)
                                            {
                                                if (jobAddlDtls.NoOfCvsFilled == null)
                                                {
                                                    jobAddlDtls.NoOfCvsFilled = 0;
                                                }
                                                jobAddlDtls.NoOfCvsFilled += 1;
                                            }

                                            var JobAsignment = new PhJobAssignment();
                                            JobAsignment = null;
                                            DateTime? ProfReceDate = null;
                                            ProfReceDate = CurrentTime;
                                            if (Usr.UserTypeId == (byte)UserType.Recruiter)
                                            {
                                                JobAsignment = dbContext.PhJobAssignments.Where(x => x.Joid == createCandidateViewModel.JobId && x.AssignedTo == UserId).FirstOrDefault();
                                                if (JobAsignment != null)
                                                {
                                                    if (JobAsignment.ProfilesUploaded == null)
                                                    {
                                                        JobAsignment.ProfilesUploaded = 0;
                                                    }
                                                    JobAsignment.ProfilesUploaded = (short)(JobAsignment.ProfilesUploaded + 1);
                                                    JobAsignment.UpdatedBy = UserId;
                                                    JobAsignment.UpdatedDate = CurrentTime;
                                                    PhJobAssignmentsDayWise_records(ref JobAsignment, CurrentTime, incrementCvsuploaded: 1);
                                                    dbContext.PhJobAssignments.Update(JobAsignment);
                                                    await dbContext.SaveChangesAsync();
                                                }
                                            }
                                            if (JobAsignment == null)
                                            {
                                                JobAsignment = dbContext.PhJobAssignments.Where(x => x.Joid == createCandidateViewModel.JobId).OrderByDescending(x => x.DeassignDate).FirstOrDefault();
                                                if (JobAsignment != null)
                                                {
                                                    if (JobAsignment.ProfilesUploaded == null)
                                                    {
                                                        JobAsignment.ProfilesUploaded = 0;
                                                    }
                                                    JobAsignment.ProfilesUploaded = (short)(JobAsignment.ProfilesUploaded + 1);
                                                    JobAsignment.UpdatedBy = UserId;
                                                    JobAsignment.UpdatedDate = CurrentTime;
                                                    PhJobAssignmentsDayWise_records(ref JobAsignment, CurrentTime, incrementCvsuploaded: 1);
                                                    dbContext.PhJobAssignments.Update(JobAsignment);
                                                    await dbContext.SaveChangesAsync();
                                                }
                                                if (Usr.UserTypeId == (byte)UserType.Candidate)
                                                {
                                                    RecId = JobAsignment.AssignedTo;
                                                }
                                            }

                                            var JobCandidate_ = new PhJobCandidate
                                            {
                                                CandProfId = Mapcandidate.Id,
                                                Joid = createCandidateViewModel.JobId,
                                                IsTagged = true,

                                                //RecruiterId = JobAsignment == null ? 0 : JobAsignment.AssignedTo,
                                                RecruiterId = RecId,
                                                EmailSentFlag = false,

                                                //Epcurrency = createCandidateViewModel.EPCurrency,
                                                //EptakeHomePerMonth = createCandidateViewModel.EPTakeHomeSalPerMonth,
                                                Epcurrency = string.Empty,
                                                EptakeHomePerMonth = 0,
                                                EpdeductionsPerAnnum = 0,
                                                EpgrossPayPerAnnum = 0,

                                                OpconfirmDate = null,
                                                OpconfirmFlag = false,

                                                Opcurrency = string.Empty,
                                                OpdeductionsPerAnnum = 0,
                                                OpgrossPayPerAnnum = 0,
                                                OptakeHomePerMonth = 0,
                                                OpvarPayPerAnnum = 0,
                                                OpgrossPayPerMonth = 0,
                                                OpnetPayPerAnnum = 0,

                                                ProfileUpdateFlag = false,
                                                ProfReceDate = ProfReceDate,
                                                BgvacceptedFlag = false,

                                                Status = (byte)RecordStatus.Active,
                                                CreatedBy = UserId,
                                                CreatedDate = CurrentTime
                                            };
                                            dbContext.PhJobCandidates.Add(JobCandidate_);
                                            await dbContext.SaveChangesAsync();


                                            // Skill
                                            if (createCandidateViewModel.CreateCandidateSkillViewModel != null)
                                            {
                                                if (createCandidateViewModel.CreateCandidateSkillViewModel.Count > 0)
                                                {
                                                    foreach (var item in createCandidateViewModel.CreateCandidateSkillViewModel)
                                                    {
                                                        var canSkil = dbContext.PhCandidateSkillsets.Where(x => x.TechnologyId
                                                             == item.TechnologyId && x.CandProfId == candidate.Id && x.Status == (byte)RecordStatus.Active).FirstOrDefault();
                                                        if (canSkil == null)
                                                        {
                                                            var candSkilSet = new PhCandidateSkillset
                                                            {
                                                                CandProfId = candidate.Id,
                                                                CreatedBy = UserId,
                                                                CreatedDate = CurrentTime,
                                                                SelfRating = item.SelfRating,
                                                                ExpInMonths = item.ExpInMonths,
                                                                ExpInYears = item.ExpInYears,
                                                                TechnologyId = item.TechnologyId,
                                                                Status = (byte)RecordStatus.Active
                                                            };
                                                            await dbContext.PhCandidateSkillsets.AddAsync(candSkilSet);
                                                            await dbContext.SaveChangesAsync();
                                                        }
                                                        else
                                                        {
                                                            canSkil.SelfRating = item.SelfRating;
                                                            canSkil.ExpInMonths = item.ExpInMonths;
                                                            canSkil.ExpInYears = item.ExpInYears;

                                                            dbContext.PhCandidateSkillsets.Update(canSkil);
                                                        }
                                                    }

                                                    double overallCanRating = 0;
                                                    var canOverSkill = dbContext.PhCandidateSkillsets.Where(x => x.CandProfId == candidate.Id && x.Status == (byte)RecordStatus.Active).ToList();
                                                    var canSkillCount = canOverSkill.Count();
                                                    var canSkillRating = canOverSkill.Sum(x => x.SelfRating);
                                                    double value1 = (double)canSkillRating / (double)canSkillRating;
                                                    if (value1 > 0)
                                                    {
                                                        overallCanRating = Math.Round(value1, 1, MidpointRounding.AwayFromZero);
                                                    }
                                                    candidate.OverallRating = overallCanRating;
                                                    dbContext.PhCandidateProfiles.Update(candidate);

                                                    await dbContext.SaveChangesAsync();
                                                }
                                            }

                                            // ResumeUrl
                                            if (createCandidateViewModel.Resume != null || !string.IsNullOrEmpty(createCandidateViewModel.ResumeURL))
                                            {
                                                string fileName = string.Empty;
                                                string fileType = string.Empty;

                                                fileName = createCandidateViewModel.ResumeURL;
                                                fileType = createCandidateViewModel.ResumeURLType;
                                                if (createCandidateViewModel.Resume != null)
                                                {
                                                    if (createCandidateViewModel.Resume.Length > 0)
                                                    {
                                                        fileName = Path.GetFileName(createCandidateViewModel.Resume.FileName);
                                                        fileName = fileName.Replace(" ", "_");
                                                        if (fileName.Length > 200)
                                                        {
                                                            fileName = fileName.Substring(0, 199);
                                                        }
                                                        var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                                                        {
                                                            await createCandidateViewModel.Resume.CopyToAsync(fileStream);
                                                        }
                                                        fileType = createCandidateViewModel.Resume.ContentType;
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(fileName))
                                                {
                                                    string Doctype = string.Empty;
                                                    if (Usr.UserTypeId == (byte)UserType.Recruiter)
                                                    {
                                                        Doctype = "Base CV";
                                                    }
                                                    else
                                                    {
                                                        Doctype = "Candidate CV";
                                                    }
                                                    var candidateDoc = new PhCandidateDoc
                                                    {
                                                        Joid = createCandidateViewModel.JobId,
                                                        Status = (byte)RecordStatus.Active,
                                                        CandProfId = Mapcandidate.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = CurrentTime,
                                                        UploadedBy = UserId,
                                                        FileGroup = (byte)FileGroup.Profile,
                                                        DocType = Doctype,
                                                        FileName = fileName,
                                                        FileType = fileType,
                                                        DocStatus = (byte)DocStatus.Notreviewd
                                                    };

                                                    dbContext.PhCandidateDocs.Add(candidateDoc);
                                                    await dbContext.SaveChangesAsync();
                                                }
                                            }

                                            // Payslips
                                            if (createCandidateViewModel.PaySlips != null)
                                            {
                                                if (createCandidateViewModel.PaySlips.Count > 0)
                                                {
                                                    foreach (var item in createCandidateViewModel.PaySlips)
                                                    {
                                                        var fileName = Path.GetFileName(item.FileName);
                                                        fileName = fileName.Replace(" ", "_");
                                                        if (fileName.Length > 200)
                                                        {
                                                            fileName = fileName.Substring(0, 199);
                                                        }
                                                        var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                                                        {
                                                            await item.CopyToAsync(fileStream);
                                                        }
                                                        var candidateDoc = new PhCandidateDoc
                                                        {
                                                            Joid = createCandidateViewModel.JobId,
                                                            Status = (byte)RecordStatus.Active,
                                                            CandProfId = Mapcandidate.Id,
                                                            CreatedBy = UserId,
                                                            CreatedDate = CurrentTime,
                                                            UploadedBy = UserId,
                                                            FileGroup = (byte)FileGroup.Profile,
                                                            DocType = "Pay Slips",
                                                            FileName = fileName,
                                                            FileType = item.ContentType,
                                                            DocStatus = (byte)DocStatus.Notreviewd
                                                        };

                                                        dbContext.PhCandidateDocs.Add(candidateDoc);
                                                        await dbContext.SaveChangesAsync();
                                                    }
                                                }
                                            }

                                            if (createCandidateViewModel.PaySlipsModel != null)
                                            {
                                                if (createCandidateViewModel.PaySlipsModel.Count > 0)
                                                {
                                                    foreach (var item in createCandidateViewModel.PaySlipsModel)
                                                    {
                                                        var candidateDoc = new PhCandidateDoc
                                                        {
                                                            Joid = createCandidateViewModel.JobId,
                                                            Status = (byte)RecordStatus.Active,
                                                            CandProfId = Mapcandidate.Id,
                                                            CreatedBy = UserId,
                                                            CreatedDate = CurrentTime,
                                                            UploadedBy = UserId,
                                                            FileGroup = (byte)FileGroup.Profile,
                                                            DocType = "Pay Slips",
                                                            FileName = item.URL,
                                                            FileType = item.Type,
                                                            DocStatus = (byte)DocStatus.Notreviewd
                                                        };

                                                        dbContext.PhCandidateDocs.Add(candidateDoc);
                                                        await dbContext.SaveChangesAsync();
                                                    }
                                                }
                                            }

                                            // Videoprofile
                                            if (createCandidateViewModel.CandVideoProfile != null)
                                            {
                                                if (createCandidateViewModel.CandVideoProfile.Length > 0)
                                                {
                                                    var fileName = Path.GetFileName(createCandidateViewModel.CandVideoProfile.FileName);
                                                    fileName = fileName.Replace(" ", "_");
                                                    if (fileName.Length > 200)
                                                    {
                                                        fileName = fileName.Substring(0, 199);
                                                    }
                                                    var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                                                    {
                                                        await createCandidateViewModel.CandVideoProfile.CopyToAsync(fileStream);
                                                    }
                                                    var candidateDoc = new PhCandidateDoc
                                                    {
                                                        Joid = createCandidateViewModel.JobId,
                                                        Status = (byte)RecordStatus.Active,
                                                        CandProfId = Mapcandidate.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = CurrentTime,
                                                        UploadedBy = UserId,
                                                        FileGroup = (byte)FileGroup.Profile,
                                                        DocType = "Video Profile",
                                                        FileName = fileName,
                                                        FileType = createCandidateViewModel.CandVideoProfile.ContentType,
                                                        DocStatus = (byte)DocStatus.Notreviewd
                                                    };


                                                    dbContext.PhCandidateDocs.Add(candidateDoc);
                                                    await dbContext.SaveChangesAsync();
                                                }
                                            }

                                            // Qualification
                                            if (createCandidateViewModel.CandidateQualificationModel != null)
                                            {
                                                if (createCandidateViewModel.CandidateQualificationModel.Count > 0)
                                                {
                                                    foreach (var qualification in createCandidateViewModel.CandidateQualificationModel)
                                                    {
                                                        var phCandidateEduDetails1 = dbContext.PhCandidateEduDetails.Where(x => x.CandProfId == Mapcandidate.Id && x.QualificationId == qualification.QualificationId && x.CourseId == qualification.CourseId && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                                                        if (phCandidateEduDetails1 == null)
                                                        {
                                                            if (qualification.Qualification.Length > 50)
                                                            {
                                                                qualification.Qualification = qualification.Qualification.Substring(0, 50);
                                                            }
                                                            if (qualification.Course.Length > 100)
                                                            {
                                                                qualification.Course = qualification.Course.Substring(0, 100);
                                                            }

                                                            var phCandidateEduDetails = new PhCandidateEduDetail
                                                            {
                                                                Status = (byte)RecordStatus.Active,
                                                                CandProfId = Mapcandidate.Id,
                                                                CreatedBy = UserId,
                                                                CreatedDate = CurrentTime,
                                                                Qualification = qualification.Qualification,
                                                                Course = qualification.Course,
                                                                QualificationId = qualification.QualificationId,
                                                                CourseId = qualification.CourseId
                                                            };

                                                            dbContext.PhCandidateEduDetails.Add(phCandidateEduDetails);
                                                            await dbContext.SaveChangesAsync();
                                                        }
                                                    }
                                                }
                                            }

                                            // Ceritification
                                            if (createCandidateViewModel.CandidateCertificationModel != null)
                                            {
                                                if (createCandidateViewModel.CandidateCertificationModel.Count > 0)
                                                {
                                                    foreach (var certification in createCandidateViewModel.CandidateCertificationModel)
                                                    {
                                                        var phCandidateCertifications = dbContext.PhCandidateCertifications.Where(x => x.CandProfId == Mapcandidate.Id && x.CertificationId == certification.CertificationID && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                                                        if (phCandidateCertifications == null)
                                                        {
                                                            phCandidateCertifications = new PhCandidateCertification
                                                            {
                                                                Status = (byte)RecordStatus.Active,
                                                                CandProfId = Mapcandidate.Id,
                                                                CreatedBy = UserId,
                                                                CreatedDate = CurrentTime,
                                                                CertificationId = certification.CertificationID
                                                            };

                                                            dbContext.PhCandidateCertifications.Add(phCandidateCertifications);
                                                            await dbContext.SaveChangesAsync();
                                                        }
                                                    }
                                                }
                                            }

                                            // QuestionResponse
                                            if (createCandidateViewModel.CandidateQuestionResponseModel != null)
                                            {
                                                if (createCandidateViewModel.CandidateQuestionResponseModel.Count > 0)
                                                {
                                                    foreach (var qtnResponse in createCandidateViewModel.CandidateQuestionResponseModel)
                                                    {

                                                        var phJobCandidateStResponses = new PhJobCandidateStResponse
                                                        {
                                                            Status = (byte)RecordStatus.Active,
                                                            CandProfId = Mapcandidate.Id,
                                                            CreatedBy = UserId,
                                                            CreatedDate = CurrentTime,
                                                            Joid = createCandidateViewModel.JobId,
                                                            Response = qtnResponse.Response,
                                                            StquestionId = qtnResponse.QuestionId
                                                        };

                                                        dbContext.PhJobCandidateStResponses.Add(phJobCandidateStResponses);
                                                        await dbContext.SaveChangesAsync();
                                                    }
                                                }
                                            }

                                            if (Usr.UserTypeId == (byte)UserType.Candidate)
                                            {
                                                UserId = RecId;
                                            }

                                            // Audit 
                                            var auditLog = new CreateAuditViewModel
                                            {
                                                ActivitySubject = "Map Candidate",
                                                ActivityDesc = Mapcandidate.CandName + " is mapped to new Job -" + Opening.JobTitle,
                                                ActivityType = (byte)AuditActivityType.StatusUpdates,
                                                TaskID = Opening.Id,
                                                UserId = UserId
                                            };
                                            audList.Add(auditLog);
                                            SaveAuditLog(audList);

                                            // activity
                                            var activityLog = new CreateActivityViewModel
                                            {
                                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                                ActivityOn = Mapcandidate.Id,
                                                ActivityType = (byte)LogActivityType.StatusUpdates,
                                                JobId = Opening.Id,
                                                ActivityDesc = Mapcandidate.CandName + " is mapped to new Job -" + Opening.JobTitle,
                                                UserId = UserId
                                            };
                                            activityList.Add(activityLog);
                                            SaveActivity(activityList);

                                            // Applying workflow conditions 
                                            var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                                            {
                                                ActionMode = (byte)WorkflowActionMode.Candidate,
                                                CanProfId = Mapcandidate.Id,
                                                CurrentStatusId = null,
                                                JobId = createCandidateViewModel.JobId,
                                                TaskCode = TaskCode.TJB.ToString(),
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

                                            respModel.Status = true;
                                            respModel.SetResult(message);
                                        }
                                        else
                                        {
                                            message = " Candidate is already associated to this job";
                                            respModel.Status = false;
                                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            message = "This job is already closed.";
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        }
                    }
                    else
                    {
                        message = "The Job is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }


                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",candidate create request respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(createCandidateViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateCandidate(UpdateCandidateViewModel updateCandidateViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            int UserId = Usr.Id;
            int RecId = UserId;
            string message = " Updated Successfully";
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    var Opening = await dbContext.PhJobOpenings.Where(x => x.Id == updateCandidateViewModel.JobId).FirstOrDefaultAsync();
                    if (Opening != null)
                    {
                        var candidate = await dbContext.PhCandidateProfiles.Where(x => x.Id == updateCandidateViewModel.Id).FirstOrDefaultAsync();
                        if (candidate != null)
                        {
                            byte ProfCompStatus = 0;
                            if (updateCandidateViewModel.MaritalStatus > 0 &&
                                !string.IsNullOrEmpty(updateCandidateViewModel.TotalExperiance) && !string.IsNullOrEmpty(updateCandidateViewModel.RelevantExperiance))
                            {
                                ProfCompStatus = 1;
                            }
                            if (!string.IsNullOrEmpty(updateCandidateViewModel.CPCurrency) &&
                                updateCandidateViewModel.CPGrossPayPerAnnum != 0 &&
                                updateCandidateViewModel.EPTakeHomeSalPerMonth != 0 &&
                                !string.IsNullOrEmpty(updateCandidateViewModel.EPCurrency) && updateCandidateViewModel.EPTakeHomeSalPerMonth != 0)
                            {
                                ProfCompStatus = 2;
                            }

                            double jobSkillRating = 0;
                            if (updateCandidateViewModel.UpdateCandidateSkillViewModel != null)
                            {
                                if (updateCandidateViewModel.UpdateCandidateSkillViewModel.Count > 0)
                                {
                                    var JobSkillCount = updateCandidateViewModel.UpdateCandidateSkillViewModel.Count();
                                    var JobSkillRating = updateCandidateViewModel.UpdateCandidateSkillViewModel.Sum(x => x.SelfRating);

                                    double value = (double)JobSkillRating / (double)JobSkillCount;
                                    if (value > 0)
                                    {
                                        jobSkillRating = Math.Round(value, 1, MidpointRounding.AwayFromZero);
                                    }
                                }
                            }

                            candidate.CandName = updateCandidateViewModel.FullName;
                            candidate.ContactNo = updateCandidateViewModel.ContactNo;
                            candidate.Remarks = updateCandidateViewModel.Remarks;

                            candidate.ValidPpflag = updateCandidateViewModel.ValidPpflag;
                            candidate.AlteContactNo = updateCandidateViewModel.AlteContactNo;
                            candidate.FullNameInPp = updateCandidateViewModel.FullName;
                            candidate.Dob = updateCandidateViewModel.CandidateDOB;
                            candidate.Gender = updateCandidateViewModel.Gender;
                            candidate.MaritalStatus = updateCandidateViewModel.MaritalStatus;
                            candidate.NoticePeriod = updateCandidateViewModel.NoticePeriod;
                            candidate.ReasonsForReloc = updateCandidateViewModel.ReasonsForReloc;

                            candidate.CurrLocation = updateCandidateViewModel.CurrLocation;
                            candidate.CurrLocationId = updateCandidateViewModel.CurrLocationID;
                            candidate.CountryId = updateCandidateViewModel.CountryID;
                            candidate.Nationality = updateCandidateViewModel.Nationality;

                            candidate.CurrOrganization = updateCandidateViewModel.CurrOrganization;
                            candidate.CurrEmplFlag = updateCandidateViewModel.CurrEmplFlag;
                            candidate.CurrDesignation = updateCandidateViewModel.CurrDesignation;

                            candidate.Experience = updateCandidateViewModel.TotalExperiance;
                            candidate.RelevantExperience = updateCandidateViewModel.RelevantExperiance;
                            candidate.ExperienceInMonths = updateCandidateViewModel.TotalExperiance != string.Empty ? ConvertMonths(updateCandidateViewModel.TotalExperiance) : 0;
                            candidate.ReleExpeInMonths = updateCandidateViewModel.RelevantExperiance != string.Empty ? ConvertMonths(updateCandidateViewModel.RelevantExperiance) : 0;

                            candidate.ProfCompStatus = ProfCompStatus;

                            candidate.Cpcurrency = updateCandidateViewModel.CPCurrency;
                            candidate.CpgrossPayPerAnnum = updateCandidateViewModel.CPGrossPayPerAnnum;
                            candidate.CpdeductionsPerAnnum = updateCandidateViewModel.CPDeductionsPerAnnum;
                            candidate.CpvariablePayPerAnnum = updateCandidateViewModel.CPVariablePayPerAnnum;
                            candidate.CptakeHomeSalPerMonth = updateCandidateViewModel.CPTakeHomeSalPerMonth;

                            candidate.Epcurrency = updateCandidateViewModel.EPCurrency;
                            candidate.EptakeHomeSalPerMonth = updateCandidateViewModel.EPTakeHomeSalPerMonth;

                            candidate.UpdatedBy = UserId;
                            candidate.UpdatedDate = CurrentTime;


                            var jobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == updateCandidateViewModel.JobId
                            && x.CandProfId == candidate.Id).FirstOrDefaultAsync();

                            if (jobCandidate != null)
                            {
                                jobCandidate.Epcurrency = updateCandidateViewModel.EPCurrency;
                                jobCandidate.EptakeHomePerMonth = updateCandidateViewModel.EPTakeHomeSalPerMonth;
                                jobCandidate.SelfRating = jobSkillRating;
                                jobCandidate.EmailSentFlag = true;

                                dbContext.PhJobCandidates.Update(jobCandidate);
                            }
                            else
                            {
                                var JobAsignment = new PhJobAssignment();
                                JobAsignment = null;
                                DateTime? ProfReceDate = null;
                                ProfReceDate = CurrentTime;
                                if (Usr.UserTypeId == (byte)UserType.Recruiter)
                                {
                                    JobAsignment = dbContext.PhJobAssignments.Where(x => x.Joid == updateCandidateViewModel.JobId && x.AssignedTo == UserId).FirstOrDefault();
                                    if (JobAsignment != null)
                                    {
                                        if (JobAsignment.ProfilesUploaded == null)
                                        {
                                            JobAsignment.ProfilesUploaded = 0;
                                        }
                                        JobAsignment.ProfilesUploaded = (short)(JobAsignment.ProfilesUploaded + 1);
                                        JobAsignment.UpdatedBy = UserId;
                                        JobAsignment.UpdatedDate = CurrentTime;
                                        PhJobAssignmentsDayWise_records(ref JobAsignment, CurrentTime, incrementCvsuploaded: 1);
                                        dbContext.PhJobAssignments.Update(JobAsignment);
                                        await dbContext.SaveChangesAsync();
                                    }
                                }
                                if (JobAsignment == null)
                                {
                                    JobAsignment = dbContext.PhJobAssignments.Where(x => x.Joid == updateCandidateViewModel.JobId).OrderBy(x => x.DeassignDate).FirstOrDefault();
                                    if (JobAsignment != null)
                                    {
                                        if (JobAsignment.ProfilesUploaded == null)
                                        {
                                            JobAsignment.ProfilesUploaded = 0;
                                        }
                                        JobAsignment.ProfilesUploaded = (short)(JobAsignment.ProfilesUploaded + 1);
                                        JobAsignment.UpdatedBy = UserId;
                                        JobAsignment.UpdatedDate = CurrentTime;
                                        PhJobAssignmentsDayWise_records(ref JobAsignment, CurrentTime, incrementCvsuploaded: 1);
                                        dbContext.PhJobAssignments.Update(JobAsignment);
                                        await dbContext.SaveChangesAsync();
                                    }
                                    if (Usr.UserTypeId == (byte)UserType.Candidate)
                                    {
                                        RecId = JobAsignment.AssignedTo;
                                    }
                                }

                                jobCandidate = new PhJobCandidate
                                {
                                    CandProfId = updateCandidateViewModel.Id,
                                    Joid = updateCandidateViewModel.JobId,
                                    IsTagged = false,

                                    //RecruiterId = JobAsignment?.AssignedTo,
                                    RecruiterId = RecId,
                                    EmailSentFlag = true,
                                    StageId = 1,
                                    CandProfStatus = 2,

                                    //Epcurrency = updateCandidateViewModel.EPCurrency,
                                    //EptakeHomePerMonth = updateCandidateViewModel.EPTakeHomeSalPerMonth,
                                    Epcurrency = string.Empty,
                                    EptakeHomePerMonth = 0,
                                    EpdeductionsPerAnnum = 0,
                                    EpgrossPayPerAnnum = 0,

                                    OpconfirmDate = null,
                                    OpconfirmFlag = false,

                                    Opcurrency = string.Empty,
                                    OpdeductionsPerAnnum = 0,
                                    OpgrossPayPerAnnum = 0,
                                    OptakeHomePerMonth = 0,
                                    OpvarPayPerAnnum = 0,
                                    OpgrossPayPerMonth = 0,
                                    OpnetPayPerAnnum = 0,
                                    SelfRating = jobSkillRating,

                                    ProfileUpdateFlag = false,
                                    ProfReceDate = ProfReceDate,

                                    Status = (byte)RecordStatus.Active,
                                    CreatedBy = UserId,
                                    CreatedDate = CurrentTime
                                };
                                await dbContext.PhJobCandidates.AddAsync(jobCandidate);
                                await dbContext.SaveChangesAsync();
                            }

                            if (updateCandidateViewModel.UpdateCandidateSkillViewModel != null)
                            {
                                if (updateCandidateViewModel.UpdateCandidateSkillViewModel.Count > 0)
                                {
                                    foreach (var item in updateCandidateViewModel.UpdateCandidateSkillViewModel)
                                    {
                                        var canSkil = dbContext.PhCandidateSkillsets.Where(x => x.TechnologyId
                                             == item.TechnologyId && x.CandProfId == candidate.Id && x.Status == (byte)RecordStatus.Active).FirstOrDefault();
                                        if (canSkil == null)
                                        {
                                            var candSkilSet = new PhCandidateSkillset
                                            {
                                                CandProfId = candidate.Id,
                                                CreatedBy = UserId,
                                                CreatedDate = CurrentTime,
                                                SelfRating = item.SelfRating,
                                                ExpInMonths = item.ExpInMonths,
                                                ExpInYears = item.ExpInYears,
                                                TechnologyId = item.TechnologyId,
                                                Status = (byte)RecordStatus.Active
                                            };
                                            await dbContext.PhCandidateSkillsets.AddAsync(candSkilSet);
                                            await dbContext.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            canSkil.SelfRating = item.SelfRating;
                                            canSkil.ExpInMonths = item.ExpInMonths;
                                            canSkil.ExpInYears = item.ExpInYears;

                                            dbContext.PhCandidateSkillsets.Update(canSkil);
                                        }
                                    }

                                    double overallCanRating = 0;
                                    var canOverSkill = dbContext.PhCandidateSkillsets.Where(x => x.CandProfId == candidate.Id && x.Status == (byte)RecordStatus.Active).ToList();
                                    var canSkillCount = canOverSkill.Count();
                                    var canSkillRating = canOverSkill.Sum(x => x.SelfRating);
                                    double value1 = (double)canSkillRating / (double)canSkillRating;
                                    if (value1 > 0)
                                    {
                                        overallCanRating = Math.Round(value1, 1, MidpointRounding.AwayFromZero);
                                    }
                                    candidate.OverallRating = overallCanRating;
                                    dbContext.PhCandidateProfiles.Update(candidate);
                                }
                            }

                            await dbContext.SaveChangesAsync();

                            string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + candidate.Id + "";

                            // Checking for folder is available or not 
                            if (!Directory.Exists(webRootPath))
                            {
                                Directory.CreateDirectory(webRootPath);
                            }
                            // Photo
                            if (updateCandidateViewModel.Photo != null)
                            {
                                if (updateCandidateViewModel.Photo.Length > 0)
                                {
                                    var fileName = Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + updateCandidateViewModel.Photo.FileName);
                                    fileName = fileName.Replace(" ", "_");
                                    if (fileName.Length > 200)
                                    {
                                        fileName = fileName.Substring(0, 199);
                                    }
                                    var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                                    {
                                        await updateCandidateViewModel.Photo.CopyToAsync(fileStream);
                                    }

                                    var CandDoc = dbContext.PhCandidateDocs.Where(x => x.CandProfId == candidate.Id && x.FileGroup == (byte)FileGroup.Profile
                                    && x.DocType == "Profile Photo" && x.Status != (byte)RecordStatus.Delete).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                                    if (CandDoc != null)
                                    {
                                        CandDoc.UpdatedBy = UserId;
                                        CandDoc.Status = (byte)RecordStatus.Delete;
                                        CandDoc.UpdatedDate = CurrentTime;
                                        dbContext.PhCandidateDocs.Update(CandDoc);
                                        await dbContext.SaveChangesAsync();
                                    }
                                    var candidateDoc = new PhCandidateDoc
                                    {
                                        Joid = updateCandidateViewModel.JobId,
                                        Status = (byte)RecordStatus.Active,
                                        CandProfId = candidate.Id,
                                        CreatedBy = UserId,
                                        CreatedDate = CurrentTime,
                                        UploadedBy = UserId,
                                        FileGroup = (byte)FileGroup.Profile,
                                        DocType = "Profile Photo",
                                        FileName = fileName,
                                        FileType = updateCandidateViewModel.Photo.ContentType,
                                        DocStatus = (byte)DocStatus.Accepted
                                    };

                                    var cand = await dbContext.PiHireUsers.Where(x => x.UserName == candidate.EmailId).FirstOrDefaultAsync();
                                    if (cand != null)
                                    {
                                        cand.ProfilePhoto = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + candidate.Id + "/" + fileName;
                                        dbContext.PiHireUsers.Update(cand);
                                    }

                                    dbContext.PhCandidateDocs.Add(candidateDoc);
                                    await dbContext.SaveChangesAsync();

                                }
                            }

                            // ResumeUrl
                            if (updateCandidateViewModel.Resume != null || !string.IsNullOrEmpty(updateCandidateViewModel.ResumeURL))
                            {
                                string fileName = string.Empty;
                                string fileType = string.Empty;
                                fileName = updateCandidateViewModel.ResumeURL;
                                fileType = updateCandidateViewModel.ResumeURLType;
                                if (updateCandidateViewModel.Resume != null)
                                {
                                    if (updateCandidateViewModel.Resume.Length > 0)
                                    {
                                        fileName = Path.GetFileName(updateCandidateViewModel.Resume.FileName);
                                        fileName = fileName.Replace(" ", "_");
                                        if (fileName.Length > 200)
                                        {
                                            fileName = fileName.Substring(0, 199);
                                        }
                                        var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await updateCandidateViewModel.Resume.CopyToAsync(fileStream);
                                        }
                                        fileType = updateCandidateViewModel.Resume.ContentType;
                                    }
                                }

                                var CandDoc = dbContext.PhCandidateDocs.Where(x => x.CandProfId == candidate.Id
                                && x.Joid == updateCandidateViewModel.JobId && x.FileGroup == (byte)FileGroup.Profile
                                && x.DocType == "Candidate CV" && x.Status != (byte)RecordStatus.Delete).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                                if (CandDoc != null)
                                {
                                    CandDoc.UpdatedBy = UserId;
                                    CandDoc.Status = (byte)RecordStatus.Delete;
                                    CandDoc.UpdatedDate = CurrentTime;
                                    dbContext.PhCandidateDocs.Update(CandDoc);
                                    await dbContext.SaveChangesAsync();
                                }

                                if (!string.IsNullOrEmpty(fileName))
                                {
                                    var candidateDoc = new PhCandidateDoc
                                    {
                                        Joid = updateCandidateViewModel.JobId,
                                        Status = (byte)RecordStatus.Active,
                                        CandProfId = candidate.Id,
                                        CreatedBy = UserId,
                                        CreatedDate = CurrentTime,
                                        UploadedBy = UserId,
                                        FileGroup = (byte)FileGroup.Profile,
                                        DocType = "Candidate CV",
                                        FileName = fileName,
                                        FileType = fileType,
                                        DocStatus = (byte)DocStatus.Notreviewd
                                    };

                                    dbContext.PhCandidateDocs.Add(candidateDoc);
                                    await dbContext.SaveChangesAsync();
                                }

                            }

                            // Educations
                            if (updateCandidateViewModel.PaySlips != null)
                            {
                                if (updateCandidateViewModel.PaySlips.Count > 0)
                                {
                                    foreach (var item in updateCandidateViewModel.PaySlips)
                                    {
                                        var fileName = Path.GetFileName(item.FileName);
                                        fileName = fileName.Replace(" ", "_");
                                        if (fileName.Length > 200)
                                        {
                                            fileName = fileName.Substring(0, 199);
                                        }
                                        var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await item.CopyToAsync(fileStream);
                                        }
                                        var candidateDoc = new PhCandidateDoc
                                        {
                                            Joid = updateCandidateViewModel.JobId,
                                            Status = (byte)RecordStatus.Active,
                                            CandProfId = candidate.Id,
                                            CreatedBy = UserId,
                                            CreatedDate = CurrentTime,
                                            UploadedBy = UserId,
                                            FileGroup = (byte)FileGroup.Profile,
                                            DocType = "Pay Slips",
                                            FileName = fileName,
                                            FileType = item.ContentType,
                                            DocStatus = (byte)DocStatus.Notreviewd
                                        };

                                        dbContext.PhCandidateDocs.Add(candidateDoc);
                                        await dbContext.SaveChangesAsync();
                                    }
                                }
                            }

                            if (updateCandidateViewModel.PaySlipsModel != null)
                            {
                                if (updateCandidateViewModel.PaySlipsModel.Count > 0)
                                {
                                    foreach (var item in updateCandidateViewModel.PaySlipsModel)
                                    {
                                        var candidateDoc = new PhCandidateDoc
                                        {
                                            Joid = updateCandidateViewModel.JobId,
                                            Status = (byte)RecordStatus.Active,
                                            CandProfId = candidate.Id,
                                            CreatedBy = UserId,
                                            CreatedDate = CurrentTime,
                                            UploadedBy = UserId,
                                            FileGroup = (byte)FileGroup.Profile,
                                            DocType = "Pay Slips",
                                            FileName = item.URL,
                                            FileType = item.Type,
                                            DocStatus = (byte)DocStatus.Notreviewd
                                        };

                                        dbContext.PhCandidateDocs.Add(candidateDoc);
                                        await dbContext.SaveChangesAsync();
                                    }
                                }
                            }

                            // Videoprofile
                            if (updateCandidateViewModel.CandVideoProfile != null)
                            {
                                if (updateCandidateViewModel.CandVideoProfile.Length > 0)
                                {
                                    var fileName = Path.GetFileName(updateCandidateViewModel.CandVideoProfile.FileName);
                                    fileName = fileName.Replace(" ", "_");
                                    if (fileName.Length > 200)
                                    {
                                        fileName = fileName.Substring(0, 199);
                                    }
                                    var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                                    {
                                        await updateCandidateViewModel.CandVideoProfile.CopyToAsync(fileStream);
                                    }
                                    //var CandDoc = dbContext.PhCandidateDocs.Where(da => da.CandProfId == candidate.Id
                                    //&& da.Joid == updateCandidateViewModel.JobId && da.FileGroup == (byte)FileGroup.Profile
                                    //&& da.DocType == "Video Profile" && da.Status != (byte)RecordStatus.Delete).OrderByDescending(da => da.CreatedDate).FirstOrDefault();
                                    //if (CandDoc != null)
                                    //{
                                    //    CandDoc.UpdatedBy = loginUserId;
                                    //    CandDoc.Status = (byte)RecordStatus.Delete;
                                    //    CandDoc.UpdatedDate = CurrentTime;
                                    //    dbContext.PhCandidateDocs.Update(CandDoc);
                                    //    await dbContext.SaveChangesAsync();
                                    //}

                                    var candidateDoc = new PhCandidateDoc
                                    {
                                        Joid = updateCandidateViewModel.JobId,
                                        Status = (byte)RecordStatus.Active,
                                        CandProfId = candidate.Id,
                                        CreatedBy = UserId,
                                        CreatedDate = CurrentTime,
                                        UploadedBy = UserId,
                                        FileGroup = (byte)FileGroup.Profile,
                                        DocType = "Video Profile",
                                        FileName = fileName,
                                        FileType = updateCandidateViewModel.CandVideoProfile.ContentType,
                                        DocStatus = (byte)DocStatus.Notreviewd
                                    };

                                    dbContext.PhCandidateDocs.Add(candidateDoc);
                                    await dbContext.SaveChangesAsync();
                                }
                            }

                            // Qualification
                            if (updateCandidateViewModel.CandidateQualificationModel != null)
                            {
                                if (updateCandidateViewModel.CandidateQualificationModel.Count > 0)
                                {
                                    foreach (var qualification in updateCandidateViewModel.CandidateQualificationModel)
                                    {
                                        if (qualification.Qualification?.Length > 50)
                                        {
                                            qualification.Qualification = qualification.Qualification.Substring(0, 50);
                                        }
                                        if (qualification.Course?.Length > 100)
                                        {
                                            qualification.Course = qualification.Course.Substring(0, 100);
                                        }
                                        var phCandidateEduDetails = dbContext.PhCandidateEduDetails.Where(x => x.CandProfId == candidate.Id && x.QualificationId == qualification.QualificationId && x.CourseId == qualification.CourseId).FirstOrDefault();
                                        if (phCandidateEduDetails == null)
                                        {
                                            phCandidateEduDetails = new PhCandidateEduDetail
                                            {
                                                Status = (byte)RecordStatus.Active,
                                                CandProfId = candidate.Id,
                                                CreatedBy = UserId,
                                                CreatedDate = CurrentTime,
                                                Qualification = qualification.Qualification,
                                                Course = qualification.Course,
                                                CourseId = qualification.CourseId,
                                                QualificationId = qualification.QualificationId
                                            };

                                            dbContext.PhCandidateEduDetails.Add(phCandidateEduDetails);
                                            await dbContext.SaveChangesAsync();
                                        }
                                    }
                                }
                            }

                            // Certification
                            if (updateCandidateViewModel.CandidateCertificationModel != null)
                            {
                                if (updateCandidateViewModel.CandidateCertificationModel.Count > 0)
                                {
                                    foreach (var certification in updateCandidateViewModel.CandidateCertificationModel)
                                    {
                                        if (certification.Id == 0)
                                        {
                                            var phCandidateCertifications = new PhCandidateCertification
                                            {
                                                Status = (byte)RecordStatus.Active,
                                                CandProfId = candidate.Id,
                                                CreatedBy = UserId,
                                                CreatedDate = CurrentTime,
                                                CertificationId = certification.CertificationID
                                            };

                                            dbContext.PhCandidateCertifications.Add(phCandidateCertifications);
                                            await dbContext.SaveChangesAsync();
                                        }
                                    }
                                }
                            }

                            // Questions
                            if (updateCandidateViewModel.CandidateQuestionResponseModel != null)
                            {
                                if (updateCandidateViewModel.CandidateQuestionResponseModel.Count > 0)
                                {
                                    foreach (var qtnResponse in updateCandidateViewModel.CandidateQuestionResponseModel)
                                    {
                                        if (qtnResponse.Id != 0)
                                        {
                                            var phJobCandidateStResponses = dbContext.PhJobCandidateStResponses.Where(x => x.Joid == updateCandidateViewModel.JobId && x.CandProfId == candidate.Id && x.Id == qtnResponse.Id).FirstOrDefault();
                                            if (phJobCandidateStResponses != null)
                                            {
                                                phJobCandidateStResponses.StquestionId = qtnResponse.QuestionId;
                                                phJobCandidateStResponses.Response = qtnResponse.Response;
                                                phJobCandidateStResponses.UpdatedDate = CurrentTime;
                                                phJobCandidateStResponses.UpdatedBy = UserId;

                                                dbContext.PhJobCandidateStResponses.Update(phJobCandidateStResponses);
                                                await dbContext.SaveChangesAsync();
                                            }
                                            else
                                            {
                                                phJobCandidateStResponses = new PhJobCandidateStResponse
                                                {
                                                    Status = (byte)RecordStatus.Active,
                                                    CandProfId = candidate.Id,
                                                    CreatedBy = UserId,
                                                    CreatedDate = CurrentTime,
                                                    Joid = updateCandidateViewModel.JobId,
                                                    StquestionId = qtnResponse.QuestionId,
                                                    Response = qtnResponse.Response
                                                };

                                                dbContext.PhJobCandidateStResponses.Add(phJobCandidateStResponses);
                                                await dbContext.SaveChangesAsync();
                                            }
                                        }
                                        else
                                        {
                                            var phJobCandidateStResponses = new PhJobCandidateStResponse
                                            {
                                                Status = (byte)RecordStatus.Active,
                                                CandProfId = candidate.Id,
                                                CreatedBy = UserId,
                                                CreatedDate = CurrentTime,
                                                Joid = updateCandidateViewModel.JobId,
                                                StquestionId = qtnResponse.QuestionId,
                                                Response = qtnResponse.Response
                                            };

                                            dbContext.PhJobCandidateStResponses.Add(phJobCandidateStResponses);
                                            await dbContext.SaveChangesAsync();
                                        }

                                    }
                                }
                            }

                            // Audit
                            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                            var auditLog = new CreateAuditViewModel
                            {
                                ActivitySubject = "Updated Candidate",
                                ActivityDesc = " updated the Candidate details successfully",
                                ActivityType = (byte)AuditActivityType.RecordUpdates,
                                TaskID = candidate.Id,
                                UserId = UserId
                            };
                            audList.Add(auditLog);
                            SaveAuditLog(audList);

                            // activity
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Candidate,
                                ActivityOn = updateCandidateViewModel.Id,
                                ActivityType = (byte)LogActivityType.RecordUpdates,
                                JobId = updateCandidateViewModel.JobId,
                                ActivityDesc = " has updated the Candidate details successfully",
                                UserId = UserId
                            };
                            activityList.Add(activityLog);
                            SaveActivity(activityList);

                            if (Usr.UserTypeId == (byte)UserType.Candidate)
                            {
                                UserId = RecId;
                            }

                            if (jobCandidate != null)
                            {
                                // Applying workflow rule 
                                var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                                {
                                    ActionMode = (byte)WorkflowActionMode.Candidate,
                                    CanProfId = jobCandidate.CandProfId,
                                    JobId = jobCandidate.Joid,
                                    TaskCode = TaskCode.CAJ.ToString(),
                                    UserId = UserId,
                                    CurrentStatusId = jobCandidate.CandProfStatus
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
                            }

                            respModel.Status = true;
                            respModel.SetResult(message);
                        }
                        else
                        {
                            message = "The Email Address is already associated to other job";
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                        }
                    }
                    else
                    {
                        message = "The Job is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",candidate update respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(updateCandidateViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> MapCandidateToJob(MapCandidateViewModel mapCandidateViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            int recId = UserId;

            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = "Updated Successfully";
            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    foreach (var item in mapCandidateViewModel.JobId)
                    {
                        var Opening = await dbContext.PhJobOpenings.Where(x => x.Id == item).FirstOrDefaultAsync();
                        if (Opening != null)
                        {
                            var candidate = await dbContext.PhCandidateProfiles.Where(x => x.Id == mapCandidateViewModel.CandidateId).FirstOrDefaultAsync();
                            if (candidate != null)
                            {
                                var jobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == item
                                && x.CandProfId == candidate.Id).FirstOrDefaultAsync();

                                if (jobCandidate == null)
                                {
                                    if (string.IsNullOrEmpty(mapCandidateViewModel.EPCurrency))
                                    {
                                        mapCandidateViewModel.EPCurrency = candidate.Epcurrency;
                                    }
                                    if (mapCandidateViewModel.EPTakeHomeSalPerMonth == null || mapCandidateViewModel.EPTakeHomeSalPerMonth == 0)
                                    {
                                        mapCandidateViewModel.EPTakeHomeSalPerMonth = candidate.EptakeHomeSalPerMonth;
                                    }

                                    candidate.ProfTaggedFlag = true;
                                    candidate.UpdatedBy = UserId;
                                    candidate.UpdatedDate = CurrentTime;

                                    var JobAsignment = dbContext.PhJobAssignments.Where(x => x.Joid == item && x.AssignedTo == UserId).OrderByDescending(x => x.DeassignDate
                                    ).FirstOrDefault();

                                    if (JobAsignment == null)
                                    {
                                        JobAsignment = dbContext.PhJobAssignments.Where(x => x.Joid == item).OrderByDescending(x => x.DeassignDate
                                    ).FirstOrDefault();
                                    }

                                    if (JobAsignment != null)
                                    {
                                        if (JobAsignment.ProfilesUploaded == null)
                                        {
                                            JobAsignment.ProfilesUploaded = 0;
                                        }
                                        JobAsignment.ProfilesUploaded = (short)(JobAsignment.ProfilesUploaded + 1);
                                        JobAsignment.UpdatedBy = UserId;
                                        JobAsignment.UpdatedDate = CurrentTime;
                                        PhJobAssignmentsDayWise_records(ref JobAsignment, CurrentTime, incrementCvsuploaded: 1);
                                        dbContext.PhJobAssignments.Update(JobAsignment);
                                        await dbContext.SaveChangesAsync();
                                    }

                                    var JobCandidate_ = new PhJobCandidate
                                    {
                                        CandProfId = candidate.Id,
                                        Joid = item,
                                        RecruiterId = recId,
                                        EmailSentFlag = false,

                                        Epcurrency = mapCandidateViewModel.EPCurrency,
                                        EpdeductionsPerAnnum = 0,
                                        EpgrossPayPerAnnum = mapCandidateViewModel.ExpectedPackage,
                                        EptakeHomePerMonth = mapCandidateViewModel.EPTakeHomeSalPerMonth,

                                        OpconfirmDate = null,
                                        OpconfirmFlag = false,

                                        Opcurrency = string.Empty,
                                        OpdeductionsPerAnnum = 0,
                                        OpgrossPayPerAnnum = 0,
                                        OptakeHomePerMonth = 0,
                                        OpvarPayPerAnnum = 0,
                                        OpgrossPayPerMonth = 0,
                                        OpnetPayPerAnnum = 0,

                                        ProfileUpdateFlag = false,
                                        ProfReceDate = CurrentTime,
                                        BgvacceptedFlag = false,

                                        Status = (byte)RecordStatus.Active,
                                        CreatedBy = UserId,
                                        CreatedDate = CurrentTime,
                                        IsTagged = true
                                    };
                                    dbContext.PhJobCandidates.Add(JobCandidate_);
                                    await dbContext.SaveChangesAsync();

                                    // applying workflow conditions 
                                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                                    {
                                        ActionMode = (byte)WorkflowActionMode.Candidate,
                                        CanProfId = candidate.Id,
                                        CurrentStatusId = null,
                                        JobId = item,
                                        TaskCode = TaskCode.TJB.ToString(),
                                        UserId = recId
                                    };
                                    var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
                                    if (wfResp.Status && wfResp.isNotification)
                                    {
                                        foreach (var itemN in wfResp.WFNotifications)
                                        {
                                            var notificationPushed = new NotificationPushedViewModel
                                            {
                                                JobId = wfResp.JoId,
                                                PushedTo = itemN.UserIds,
                                                NoteDesc = itemN.NoteDesc,
                                                Title = itemN.Title,
                                                CreatedBy = UserId
                                            };
                                            notificationPushedViewModel.Add(notificationPushed);
                                        }
                                    }

                                    if (wfResp.Status)
                                    {
                                        // audit 
                                        var auditLog = new CreateAuditViewModel
                                        {
                                            ActivitySubject = "NEW Job Mapping",
                                            ActivityDesc = candidate.CandName + " is Mapped to NEW Job - " + Opening.JobTitle,
                                            ActivityType = (byte)AuditActivityType.StatusUpdates,
                                            TaskID = Opening.Id,
                                            UserId = UserId
                                        };
                                        audList.Add(auditLog);

                                        // activity
                                        var activityLog = new CreateActivityViewModel
                                        {
                                            ActivityMode = (byte)WorkflowActionMode.Opening,
                                            ActivityOn = candidate.Id,
                                            ActivityType = (byte)LogActivityType.StatusUpdates,
                                            JobId = Opening.Id,
                                            ActivityDesc = candidate.CandName + " is Mapped to NEW Job - " + Opening.JobTitle,
                                            UserId = UserId
                                        };
                                        activityList.Add(activityLog);

                                    }

                                }
                            }
                        }
                    }

                    if (audList.Count > 0)
                    {
                        SaveAuditLog(audList);
                    }
                    if (activityList.Count > 0)
                    {
                        SaveActivity(activityList);
                    }

                    respModel.Status = true;
                    respModel.SetResult(message);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",Map candidate respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(mapCandidateViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        #endregion

        #region Candidate listing view actions
        public async Task<GetResponseViewModel<CandidateListModel>> CandidateList(CandidateListSearchViewModel candidateListSearchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            byte UsrType = Usr.UserTypeId;
            var respModel = new GetResponseViewModel<CandidateListModel>();
            try
            {

                logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "CandidateList Request:", Newtonsoft.Json.JsonConvert.SerializeObject(candidateListSearchViewModel));

                int? SalaryMinRange = null;
                int? SalaryMaxRange = null;
                int? MinAge = null;
                int? MaxAge = null;

                if (candidateListSearchViewModel.Availability == 0)
                {
                    candidateListSearchViewModel.Availability = null;
                }
                if (!string.IsNullOrEmpty(candidateListSearchViewModel.Age))
                {
                    string[] age_ = candidateListSearchViewModel.Age.Split('-');
                    if (age_.Length == 2)
                    {
                        MinAge = Convert.ToInt32(age_[0]);
                        MaxAge = Convert.ToInt32(age_[1]);
                    }
                }
                if (!string.IsNullOrEmpty(candidateListSearchViewModel.SalaryRange))
                {
                    string[] salary_ = candidateListSearchViewModel.SalaryRange.Split('-');
                    if (salary_.Length == 2)
                    {
                        SalaryMinRange = Convert.ToInt32(salary_[0]);
                        SalaryMaxRange = Convert.ToInt32(salary_[1]);
                    }
                }
                // Refer for Recruiters only
                if (candidateListSearchViewModel.MyCandidates && UsrType == (byte)UserType.Recruiter)
                {
                    candidateListSearchViewModel.Recruiter = UserId.ToString();
                }
                candidateListSearchViewModel.CurrentPage = (candidateListSearchViewModel.CurrentPage.Value - 1) * candidateListSearchViewModel.PerPage.Value;
                candidateListSearchViewModel.JobId = null;

                var dtls = await dbContext.GetCandidateList(candidateListSearchViewModel.SearchKey,
      candidateListSearchViewModel.Recruiter, candidateListSearchViewModel.Rating, candidateListSearchViewModel.ApplicationStatus,
      candidateListSearchViewModel.Gender, candidateListSearchViewModel.Nationality, candidateListSearchViewModel.CurrentLocaiton,
      candidateListSearchViewModel.Source, candidateListSearchViewModel.Currency, candidateListSearchViewModel.MaritalStatus, Usr.UserTypeId, UserId, candidateListSearchViewModel.PuId,
      candidateListSearchViewModel.Availability, SalaryMinRange, SalaryMaxRange,
      MinAge, MaxAge, candidateListSearchViewModel.JobId, candidateListSearchViewModel.fromDate, candidateListSearchViewModel.toDate,
      candidateListSearchViewModel.PerPage, candidateListSearchViewModel.CurrentPage);


                respModel.Status = true;
                respModel.SetResult(dtls);

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",candidate list respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateListSearchViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<JobCandidateListModel>> JobCandidateList(CandidateListSearchViewModel candidateListSearchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<JobCandidateListModel>();
            try
            {

                // logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "JobCandidateList Request:", Newtonsoft.Json.JsonConvert.SerializeObject(candidateListSearchViewModel));


                int? SalaryMinRange = null;
                int? SalaryMaxRange = null;
                int? MinAge = null;
                int? MaxAge = null;

                if (candidateListSearchViewModel.Availability == 0)
                {
                    candidateListSearchViewModel.Availability = null;
                }
                if (!string.IsNullOrEmpty(candidateListSearchViewModel.Age))
                {
                    string[] age_ = candidateListSearchViewModel.Age.Split('-');
                    if (age_.Length == 2)
                    {
                        MinAge = Convert.ToInt32(age_[0]);
                        MaxAge = Convert.ToInt32(age_[1]);
                    }
                }
                if (!string.IsNullOrEmpty(candidateListSearchViewModel.SalaryRange))
                {
                    string[] salary_ = candidateListSearchViewModel.SalaryRange.Split('-');
                    if (salary_.Length == 2)
                    {
                        SalaryMinRange = Convert.ToInt32(salary_[0]);
                        SalaryMaxRange = Convert.ToInt32(salary_[1]);
                    }
                }
                candidateListSearchViewModel.CurrentPage = (candidateListSearchViewModel.CurrentPage.Value - 1) * candidateListSearchViewModel.PerPage.Value;

                var dtls = await dbContext.GetJobCandidateList(candidateListSearchViewModel.SearchKey,
      candidateListSearchViewModel.Recruiter, candidateListSearchViewModel.Rating, candidateListSearchViewModel.ApplicationStatus,
      candidateListSearchViewModel.Gender, candidateListSearchViewModel.Nationality, candidateListSearchViewModel.CurrentLocaiton,
      candidateListSearchViewModel.Source, candidateListSearchViewModel.Currency, candidateListSearchViewModel.MaritalStatus, Usr.UserTypeId, UserId,
      candidateListSearchViewModel.Availability, SalaryMinRange, SalaryMaxRange,
      MinAge, MaxAge, candidateListSearchViewModel.JobId, candidateListSearchViewModel.MyCandidates, candidateListSearchViewModel.tlReview, candidateListSearchViewModel.dmReview, candidateListSearchViewModel.l1Review, candidateListSearchViewModel.fromDate, candidateListSearchViewModel.toDate,
      candidateListSearchViewModel.PerPage, candidateListSearchViewModel.CurrentPage);


                respModel.Status = true;
                respModel.SetResult(dtls);

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",job candidate list respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateListSearchViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<JobCandidateListFilterDataViewModel>> JobCandidateListFilterData(int? JobId = null)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<JobCandidateListFilterDataViewModel>();
            try
            {

                // logger.Log(LogLevel.Debug, LoggingEvents.ListItems, "JobCandidateList Request:JobId=>" + JobId);

                var dtls = await dbContext.GetJobCandidateListFilterData(Usr.UserTypeId, UserId, JobId);
                var data = new JobCandidateListFilterDataViewModel
                {
                    CountryIds = dtls.Where(da => da?.CountryID.HasValue ?? false).Select(da => da.CountryID.Value).Distinct().ToList(),
                    NationalityIds = dtls.Where(da => da?.Nationality.HasValue ?? false).Select(da => da.Nationality.Value).Distinct().ToList(),
                    CandProfStatusIds = dtls.Where(da => da?.CandProfStatus.HasValue ?? false).Select(da => da.CandProfStatus.Value).Distinct().ToList(),
                    OpCurrencyIds = dtls.Where(da => string.IsNullOrEmpty(da?.OpCurrency) == false).Select(da => da.OpCurrency).Distinct().ToList(),
                };

                respModel.Status = true;
                respModel.SetResult(data);
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
        public async Task<GetResponseViewModel<JobCandidateListFilterDataViewModel>> SuggestCandidateListFilterData(int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<JobCandidateListFilterDataViewModel>();
            try
            {

                var dtls = await dbContext.GetJobSuggestListFilterData(Usr.UserTypeId, UserId, JobId);
                var data = new JobCandidateListFilterDataViewModel
                {
                    CountryIds = dtls.Where(da => da?.CountryID.HasValue ?? false).Select(da => da.CountryID.Value).Distinct().ToList(),
                    NationalityIds = dtls.Where(da => da?.Nationality.HasValue ?? false).Select(da => da.Nationality.Value).Distinct().ToList(),
                    CandProfStatusIds = dtls.Where(da => da?.CandProfStatus.HasValue ?? false).Select(da => da.CandProfStatus.Value).Distinct().ToList()
                };

                respModel.Status = true;
                respModel.SetResult(data);
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
        public async Task<GetResponseViewModel<JobCandidateListModel>> SuggestCandidateList(SuggestCandidateListSearchViewModel suggestCandidateListSearchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<JobCandidateListModel>();
            try
            {


                int? SalaryMinRange = null;
                int? SalaryMaxRange = null;
                int? MinAge = null;
                int? MaxAge = null;
                if (suggestCandidateListSearchViewModel.Availability == 0)
                {
                    suggestCandidateListSearchViewModel.Availability = null;
                }
                if (!string.IsNullOrEmpty(suggestCandidateListSearchViewModel.Age))
                {
                    string[] age_ = suggestCandidateListSearchViewModel.Age.Split('-');
                    if (age_.Length == 2)
                    {
                        MinAge = Convert.ToInt32(age_[0]);
                        MaxAge = Convert.ToInt32(age_[1]);
                    }
                }
                if (!string.IsNullOrEmpty(suggestCandidateListSearchViewModel.SalaryRange))
                {
                    string[] salary_ = suggestCandidateListSearchViewModel.SalaryRange.Split('-');
                    if (salary_.Length == 2)
                    {
                        SalaryMinRange = Convert.ToInt32(salary_[0]);
                        SalaryMaxRange = Convert.ToInt32(salary_[1]);
                    }
                }


                suggestCandidateListSearchViewModel.CurrentPage = (suggestCandidateListSearchViewModel.CurrentPage.Value - 1) * suggestCandidateListSearchViewModel.PerPage.Value;

                var dtls = await dbContext.GetSuggestCandidateList(suggestCandidateListSearchViewModel.SearchKey,
            suggestCandidateListSearchViewModel.Recruiter, suggestCandidateListSearchViewModel.Rating, suggestCandidateListSearchViewModel.ApplicationStatus,
            suggestCandidateListSearchViewModel.Gender, suggestCandidateListSearchViewModel.Nationality, suggestCandidateListSearchViewModel.CurrentLocaiton,
            suggestCandidateListSearchViewModel.Source, suggestCandidateListSearchViewModel.Currency, suggestCandidateListSearchViewModel.MaritalStatus, suggestCandidateListSearchViewModel.JobId,
            suggestCandidateListSearchViewModel.Availability, SalaryMinRange, SalaryMaxRange,
            MinAge, MaxAge,
            suggestCandidateListSearchViewModel.PerPage, suggestCandidateListSearchViewModel.CurrentPage);


                respModel.Status = true;
                respModel.SetResult(dtls);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ", Suggest Candidate List Model respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(suggestCandidateListSearchViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<List<CandidatesViewModel>>> GetExportCandidates(CandidateExportSearchViewModel candidateExportSearchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<CandidatesViewModel>>();
            try
            {


                int? SalaryMinRange = null;
                int? SalaryMaxRange = null;
                int? MinAge = null;
                int? MaxAge = null;
                if (candidateExportSearchViewModel.JobId == 0)
                {
                    candidateExportSearchViewModel.JobId = null;
                }
                if (candidateExportSearchViewModel.Availability == 0)
                {
                    candidateExportSearchViewModel.Availability = null;
                }
                if (!string.IsNullOrEmpty(candidateExportSearchViewModel.Age))
                {
                    string[] age_ = candidateExportSearchViewModel.Age.Split('-');
                    if (age_.Length == 2)
                    {
                        MinAge = Convert.ToInt32(age_[0]);
                        MaxAge = Convert.ToInt32(age_[1]);
                    }
                }
                if (!string.IsNullOrEmpty(candidateExportSearchViewModel.SalaryRange))
                {
                    string[] salary_ = candidateExportSearchViewModel.SalaryRange.Split('-');
                    if (salary_.Length == 2)
                    {
                        SalaryMinRange = Convert.ToInt32(salary_[0]);
                        SalaryMaxRange = Convert.ToInt32(salary_[1]);
                    }
                }

                // audit              
                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
                var auditLog = new CreateAuditViewModel
                {
                    ActivitySubject = "Export Candidate List",
                    ActivityDesc = " Fetched details of Candidates to Export as Excel/CSV",
                    ActivityType = (byte)AuditActivityType.Other,
                    TaskID = candidateExportSearchViewModel.JobId,
                    UserId = UserId
                };
                audList.Add(auditLog);
                SaveAuditLog(audList);

                var dtls = await dbContext.GetCandidateList(candidateExportSearchViewModel.SearchKey,
            candidateExportSearchViewModel.Recruiter, candidateExportSearchViewModel.Rating, candidateExportSearchViewModel.ApplicationStatus,
            candidateExportSearchViewModel.Gender, candidateExportSearchViewModel.Nationality, candidateExportSearchViewModel.CurrentLocaiton,
            candidateExportSearchViewModel.Source, candidateExportSearchViewModel.Currency, candidateExportSearchViewModel.MaritalStatus, Usr.UserTypeId, UserId, 0, candidateExportSearchViewModel.Availability, SalaryMinRange, SalaryMaxRange,
            MinAge, MaxAge, candidateExportSearchViewModel.JobId, null, null, 1000, 0);

                respModel.Status = true;
                respModel.SetResult(dtls.CandidatesViewModel);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",Candidate Export respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateExportSearchViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateCandidatePrfStatus(UpdateCandidatePrfStatusViewModel updateCandidatePrfStatusViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = string.Empty;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var jobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == updateCandidatePrfStatusViewModel.JobId && x.CandProfId == updateCandidatePrfStatusViewModel.CandidateId).FirstOrDefaultAsync();
                var CandidateProfile = await dbContext.PhCandidateProfiles.Where(x => x.Id == updateCandidatePrfStatusViewModel.CandidateId).FirstOrDefaultAsync();
                var UpdateCandidateStatus = await dbContext.PhCandStatusSes.Select(x => new { x.Title, x.Id }).Where(x => x.Id == updateCandidatePrfStatusViewModel.CandidateStatuId).FirstOrDefaultAsync();

                if (UpdateCandidateStatus != null)
                {
                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        // Day Wise Actions 
                        var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == updateCandidatePrfStatusViewModel.JobId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                        if (IsDayWiseAction == null)
                        {
                            IsDayWiseAction = new PhDayWiseJobAction
                            {
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                Joid = updateCandidatePrfStatusViewModel.JobId,
                                CandStatus = true
                            };
                            dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            IsDayWiseAction.CandStatus = true;
                            dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                    }


                    if (!string.IsNullOrEmpty(updateCandidatePrfStatusViewModel.Remarks))
                    {
                        // activity
                        List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Opening,
                            ActivityOn = updateCandidatePrfStatusViewModel.CandidateId,
                            ActivityType = (byte)LogActivityType.RecordUpdates,
                            JobId = updateCandidatePrfStatusViewModel.JobId,
                            ActivityDesc = " has Updated Remarks for : " + updateCandidatePrfStatusViewModel.Remarks + "",
                            UserId = UserId
                        };
                        activityList.Add(activityLog);
                        SaveActivity(activityList);
                    }

                    // applying workflow conditions 
                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    {
                        ActionMode = (byte)WorkflowActionMode.Candidate,
                        CanProfId = CandidateProfile.Id,
                        CurrentStatusId = jobCandidate.CandProfStatus,
                        UpdateStatusId = updateCandidatePrfStatusViewModel.CandidateStatuId,
                        JobId = updateCandidatePrfStatusViewModel.JobId,
                        TaskCode = TaskCode.CUS.ToString(),
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
                    if (!wfResp.Status)
                    {
                        respModel.Status = false;
                        message = wfResp.Message.Count > 0 ? string.Join(",", wfResp.Message).ToString() : string.Empty;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                    else
                    {
                        respModel.Status = true;
                        message = "Updated Successfully";
                    }

                    respModel.SetResult(message);

                }
                else
                {
                    respModel.Status = true;
                    message = "The Candidate is not available";
                    respModel.SetResult(message);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(updateCandidatePrfStatusViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateCandidatePrfStatus(UpdateCandidateCVStatusViewModel updateCandidateCVStatusViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = string.Empty;
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var jobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == updateCandidateCVStatusViewModel.JobId && x.CandProfId == updateCandidateCVStatusViewModel.CandidateId).FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(updateCandidateCVStatusViewModel.Remarks))
                {
                    // activity
                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        ActivityOn = updateCandidateCVStatusViewModel.CandidateId,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        JobId = updateCandidateCVStatusViewModel.JobId,
                        ActivityDesc = " has Updated Remarks for : " + updateCandidateCVStatusViewModel.Remarks + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);
                }

                // applying workflow conditions 
                var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                {
                    ActionMode = (byte)WorkflowActionMode.Candidate,
                    CanProfId = updateCandidateCVStatusViewModel.CandidateId,
                    CurrentStatusId = jobCandidate.CandProfStatus,
                    JobId = updateCandidateCVStatusViewModel.JobId,
                    UserId = UserId
                };
                if (updateCandidateCVStatusViewModel.CandidateStatuCode == CandidateStatusCodes.SVD.ToString())
                {
                    workFlowRuleSearchViewModel.TaskCode = TaskCode.NSU.ToString();
                }
                else
                {
                    workFlowRuleSearchViewModel.TaskCode = TaskCode.RCR.ToString();
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
                    respModel.Status = true;
                    message = wfResp.Message.Count > 0 ? string.Join(",", wfResp.Message).ToString() : string.Empty;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
                else
                {
                    respModel.Status = true;
                    message = "Updated Successfully";
                }
                respModel.SetResult(message);

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(updateCandidateCVStatusViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateCandidateOtherStatus(CandidateOtherStatusViewModel candidateOtherStatusViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            string message = string.Empty;
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.UpdateItem, "Start of method:", respModel.Meta.RequestID);

                var CandStatus = dbContext.PhCandStatusSes.Where(x => x.Cscode == candidateOtherStatusViewModel.Code).FirstOrDefault();
                if (CandStatus != null)
                {
                    var jobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == candidateOtherStatusViewModel.JobId && x.CandProfId == candidateOtherStatusViewModel.CandidateId).FirstOrDefaultAsync();
                    if (jobCandidate != null)
                    {
                        if (jobCandidate.CandProfStatus != CandStatus.Id)
                        {
                            // Applying workflow conditions 
                            var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                            {
                                ActionMode = (byte)WorkflowActionMode.Other,
                                CanProfId = candidateOtherStatusViewModel.CandidateId,
                                JobId = candidateOtherStatusViewModel.JobId,
                                TaskCode = candidateOtherStatusViewModel.TaskCode,
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
                            if (!wfResp.Status)
                            {
                                respModel.Status = true;
                                message = wfResp.Message.Count > 0 ? string.Join(",", wfResp.Message).ToString() : string.Empty;
                                respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                            }
                            else
                            {
                                var changeStatusViewModel = new ChangeStatusViewModel
                                {
                                    ActionMode = (byte)WorkflowActionMode.Candidate,
                                    CanProfId = candidateOtherStatusViewModel.CandidateId,
                                    CurrentStatusId = jobCandidate.CandProfStatus,
                                    UpdatedStatusId = CandStatus.Id,
                                    JobId = candidateOtherStatusViewModel.JobId,
                                    UserId = UserId,
                                    Remarks = string.Empty
                                };
                                await ChangeStatusRules(changeStatusViewModel);

                                respModel.Status = true;
                                message = " Updated Successfully";
                            }
                            respModel.SetResult(message);
                        }
                        else
                        {
                            respModel.Status = true;
                            message = "The Candidate is already in same status";
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        }
                    }
                    else
                    {
                        respModel.Status = true;
                        message = "The Candidate is not available";
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                }
                else
                {
                    respModel.Status = false;
                    message = "The Candidate Status is not available";
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateOtherStatusViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        #endregion

        #region  Candidate view actions
        public async Task<GetResponseViewModel<GetCandidateViewModel>> GetCandidate(int CandId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<GetCandidateViewModel>();
            string message = string.Empty;
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {

                    var dtls = new GetCandidateViewModel();

                    var opening = await dbContext.PhJobOpenings.Where(x => x.Id == JobId).FirstOrDefaultAsync();
                    if (opening != null)
                    {
                        dtls = (from can in dbContext.PhCandidateProfiles
                                where can.Id == CandId
                                select new GetCandidateViewModel
                                {
                                    FullName = can.CandName,
                                    FullNameInPp = can.FullNameInPp,
                                    Gender = can.Gender,
                                    CandProfId = can.Id,
                                    EmailAddress = can.EmailId,
                                    AlteContactNo = can.AlteContactNo,
                                    CandidateDOB = can.Dob,
                                    ContactNo = can.ContactNo,
                                    MaritalStatus = can.MaritalStatus,

                                    CPDeductionsPerAnnum = can.CpdeductionsPerAnnum,
                                    CPGrossPayPerAnnum = can.CpgrossPayPerAnnum,
                                    CPTakeHomeSalPerMonth = can.CptakeHomeSalPerMonth,
                                    CPVariablePayPerAnnum = can.CpvariablePayPerAnnum,
                                    CurrentPackage = can.CpdeductionsPerAnnum,

                                    CountryID = can.CountryId,
                                    CurrLocation = can.CurrLocation,
                                    CurrLocationID = can.CurrLocationId,
                                    CurrOrganization = can.CurrOrganization,
                                    CPCurrency = can.Cpcurrency,

                                    EPCurrency = can.Epcurrency,
                                    EPTakeHomeSalPerMonth = can.EptakeHomeSalPerMonth,

                                    Nationality = can.Nationality,
                                    NoticePeriod = can.NoticePeriod,
                                    ReasonsForReloc = can.ReasonsForReloc,
                                    ReasonType = can.ReasonType,
                                    RelevantExperiance = can.RelevantExperience,
                                    CurrEmplFlag = can.CurrEmplFlag,
                                    Remarks = can.Remarks,
                                    SourceId = can.SourceId,
                                    TotalExperiance = can.Experience,
                                    ValidPpflag = can.ValidPpflag,
                                    CurrDesignation = can.CurrDesignation
                                }).FirstOrDefault();

                        if (dtls != null)
                        {
                            var jbAdtlDtls = dbContext.PhJobCandidates.Where(x => x.Joid == JobId && x.CandProfId == CandId).FirstOrDefault();
                            if (jbAdtlDtls != null)
                            {
                                dtls.ExpectedPackage = jbAdtlDtls.EpgrossPayPerAnnum;
                                dtls.JobId = jbAdtlDtls.Joid;
                                dtls.SelfRating = jbAdtlDtls.SelfRating;
                                dtls.CandPrfStatus = jbAdtlDtls.CandProfStatus;
                                dtls.StageId = jbAdtlDtls.StageId;
                            }

                            var CandidateSkillSet = (from Canskill in dbContext.PhCandidateSkillsets
                                                     join tech in dbContext.PhTechnologysSes on Canskill.TechnologyId equals tech.Id
                                                     where Canskill.CandProfId == CandId && Canskill.Status == (byte)RecordStatus.Active
                                                     select new GetCandidateSkillViewModel
                                                     {
                                                         Id = Canskill.Id,
                                                         ExpInMonths = Canskill.ExpInMonths,
                                                         ExpInYears = Canskill.ExpInYears,
                                                         SelfRating = Canskill.SelfRating,
                                                         TechnologyId = Canskill.TechnologyId,
                                                         TechnologyName = tech.Title,
                                                         IsCanSkill = false
                                                     }).ToList();

                            // start 
                            if (dtls.CandidateSkillViewModel == null)
                            {
                                dtls.CandidateSkillViewModel = new List<GetCandidateSkillViewModel>();
                            }
                            if (CandidateSkillSet.Count() > 0)
                            {
                                dtls.CandidateSkillViewModel.AddRange(CandidateSkillSet);
                            }
                            // end

                            var documents = await (from canDoc in dbContext.PhCandidateDocs
                                                   where canDoc.CandProfId == CandId && canDoc.Joid == JobId && canDoc.Status != (byte)RecordStatus.Delete
                                                   select new CandidateFilesViewModel
                                                   {
                                                       DocType = canDoc.DocType,
                                                       FileGroup = canDoc.FileGroup,
                                                       FileName = canDoc.FileName,
                                                       DocStatus = canDoc.DocStatus,
                                                       CandProfId = canDoc.CandProfId,
                                                       UploadedFromDrive = false
                                                   }).ToListAsync();
                            foreach (var item in documents)
                            {
                                item.DocStatusName = Enum.GetName(typeof(DocStatus), item.DocStatus);
                                if (!string.IsNullOrEmpty(item.FileName))
                                {
                                    if (ValidHttpURL(item.FileName))
                                    {
                                        item.UploadedFromDrive = true;
                                        item.FilePath = item.FileName;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(item.FileName))
                                        {
                                            item.FileName = item.FileName.Replace("#", "%23");
                                            item.FilePath = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + item.CandProfId + "/" + item.FileName;
                                        }
                                    }
                                }
                            }

                            if (documents.Count() > 0)
                            {
                                dtls.Photo = documents.Where(x => x.DocType == "Profile Photo" && x.FileGroup == (byte)FileGroup.Profile).Select(x => x.FilePath).FirstOrDefault();

                                dtls.Resume = documents.Where(x => x.DocType == "Candidate CV" && x.FileGroup == (byte)FileGroup.Profile).Select(x => x.FilePath).FirstOrDefault();

                                dtls.PaySlips = documents.Where(x => (x.DocType == "Pay Slips" || x.DocType == "Payslip") && x.FileGroup == (byte)FileGroup.Profile).Select(x => x.FilePath).ToList();
                            }


                            //qualification 
                            dtls.Qualifications = new List<GetCandQualifications>();
                            dtls.Qualifications = await (from canEdu in dbContext.PhCandidateEduDetails
                                                         where canEdu.CandProfId == CandId && canEdu.Status != (byte)RecordStatus.Delete
                                                         select new GetCandQualifications
                                                         {
                                                             Course = canEdu.Course,
                                                             CreatedDate = canEdu.CreatedDate,
                                                             Qualification = canEdu.Qualification,
                                                             QualificationId = canEdu.QualificationId,
                                                             CourseId = canEdu.CourseId,
                                                             Id = canEdu.Id
                                                         }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                            //certification 
                            dtls.Certififcates = new List<GetCandCertifications>();
                            dtls.Certififcates = await (from canCert in dbContext.PhCandidateCertifications
                                                        join refData in dbContext.PhRefMasters on canCert.CertificationId equals refData.Id
                                                        where canCert.CandProfId == CandId && canCert.Status != (byte)RecordStatus.Delete
                                                        select new GetCandCertifications
                                                        {
                                                            Id = canCert.CertificationId,
                                                            CertificationName = refData.Rmvalue,
                                                            CreatedDate = refData.CreatedDate,
                                                            CertificationId = canCert.CertificationId
                                                        }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                            //questions 
                            dtls.QuestionResponse = new List<GetCandQuestionResponse>();
                            dtls.QuestionResponse = await (from jobQtn in dbContext.PhJobOpeningStQtns
                                                           join canRes in dbContext.PhJobCandidateStResponses on jobQtn.Id equals canRes.StquestionId
                                                           where canRes.Joid == JobId && canRes.CandProfId == CandId && canRes.Status != (byte)RecordStatus.Delete
                                                           select new GetCandQuestionResponse
                                                           {
                                                               Id = canRes.Id,
                                                               QuestionText = jobQtn.QuestionText,
                                                               Response = canRes.Response,
                                                               CreatedDate = canRes.CreatedDate,
                                                               QuestionType = jobQtn.QuestionType
                                                           }).OrderBy(x => x.Id).ToListAsync();

                            respModel.Status = true;
                            respModel.SetResult(dtls);
                        }
                        else
                        {
                            message = "The Candidate is not available";
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        }


                    }
                    else
                    {
                        message = "The Job is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return respModel;
        }
        public async Task<GetResponseViewModel<CandidateOverViewModel>> GetCandidateOverview(int CandId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateOverViewModel>();
            try
            {

                var dtls = new CandidateOverViewModel();

                dtls = await (from canJob in dbContext.PhJobCandidates
                              join canPrf in dbContext.PhCandidateProfiles on canJob.CandProfId equals canPrf.Id
                              join job in dbContext.PhJobOpenings on canJob.Joid equals job.Id
                              join jobDtls in dbContext.PhJobOpeningsAddlDetails on job.Id equals jobDtls.Joid
                              where canJob.CandProfId == CandId && canJob.Joid == JobId
                              select new CandidateOverViewModel
                              {
                                  EmailAddress = canPrf.EmailId,
                                  FullName = canPrf.CandName,
                                  Gender = canPrf.Gender,
                                  MaritalStatus = canPrf.MaritalStatus,
                                  Nationality = canPrf.Nationality,
                                  CandProfId = CandId,
                                  AlteContactNo = canPrf.AlteContactNo,
                                  CandidateDOB = canPrf.Dob,
                                  ContactNo = canPrf.ContactNo,
                                  SelfRating = canJob.SelfRating,

                                  NoticePeriod = canPrf.NoticePeriod,
                                  ReasonsForReloc = canPrf.ReasonsForReloc,
                                  RelevantExperience = canPrf.RelevantExperience,
                                  TotalExperience = canPrf.Experience,

                                  SourceId = canPrf.SourceId,
                                  CreatedBy = canJob.RecruiterId,
                                  CreatedDate = canJob.CreatedDate,
                                  JobStartDate = job.PostedDate,
                                  JobClosedDate = job.ClosedDate,
                                  JobTitle = job.JobTitle,
                                  ClientId = job.ClientId,

                                  CountryID = canPrf.CountryId,
                                  CurrLocationID = canPrf.CurrLocationId,
                                  CurrLocation = dbContext.PhCities.Where(x => x.Id == canPrf.CurrLocationId).Select(x => x.Name).FirstOrDefault(),

                                  CPCurrency = canPrf.Cpcurrency,
                                  CPDeductionsPerAnnum = canPrf.CpdeductionsPerAnnum,
                                  CPGrossPayPerAnnum = canPrf.CpgrossPayPerAnnum,
                                  CPTakeHomeSalPerMonth = canPrf.CptakeHomeSalPerMonth,
                                  CPVariablePayPerAnnum = canPrf.CpvariablePayPerAnnum,

                                  PayslipCurrency = canJob.PayslipCurrency,
                                  PayslipSalary = canJob.PayslipSalary,
                                  IsPayslipVerified = canJob.IsPayslipVerified == null ? false : canJob.IsPayslipVerified,

                                  EPCurrency = canJob.Epcurrency,
                                  EPTakeHomeSalPerMonth = canJob.EptakeHomePerMonth,

                                  Opcurrency = canJob.Opcurrency,
                                  OpdeductionsPerAnnum = canJob.OpdeductionsPerAnnum,
                                  OpgrossPayPerAnnum = canJob.OpgrossPayPerAnnum,
                                  OpgrossPayPerMonth = canJob.OpgrossPayPerMonth,
                                  OpnetPayPerAnnum = canJob.OpnetPayPerAnnum,
                                  OptakeHomePerMonth = canJob.OptakeHomePerMonth,
                                  OpvarPayPerAnnum = canJob.OpvarPayPerAnnum,

                                  OpconfirmFlag = canJob.OpconfirmFlag == null ? false : canJob.OpconfirmFlag,
                                  BgvacceptedFlag = canJob.BgvacceptedFlag,
                                  CandProfStatus = canJob.CandProfStatus,
                                  StageId = canJob.StageId,
                                  PuId = jobDtls.Puid,
                                  Roles = canPrf.Roles,

                                  MinSalary = jobDtls.MinSalary,
                                  MaxSalary = jobDtls.MaxSalary,
                                  BudgetCurrencyId = jobDtls.CurrencyId,
                                  JobCountryId = job.CountryId,
                                  JobCityId = job.JobLocationId,

                                  SalaryVerifiedby = dbContext.PiHireUsers.Where(x => x.Id == canJob.SalaryVerifiedby && x.UserType != (byte)UserType.Candidate).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                  CandidateUserId = dbContext.PiHireUsers.Where(x => x.UserName == canPrf.EmailId).Select(x => x.Id).FirstOrDefault(),

                                  IsTagged = canJob.IsTagged,
                                  Sourced = string.Empty,
                                  CountryName = string.Empty,
                                  PaySlip = string.Empty,
                                  BudgetCurrency = string.Empty,
                                  ProfilePhoto = string.Empty,
                                  TLReview = canJob.Tlreview,
                                  DMReview = canJob.Mreview,
                                  L1Review = canJob.L1review
                              }).FirstOrDefaultAsync();

                if (dtls != null)
                {

                    if (dtls.StageId != null)
                    {
                        dtls.StageName = dbContext.PhCandStagesSes.Where(x => x.Id == dtls.StageId).Select(x => x.Title).FirstOrDefault();
                    }
                    dtls.ReasonsText = new List<string>();
                    var InterviewDtls = dbContext.PhCandidateProfilesShareds.Where(x => x.Joid == JobId && x.CandProfId == CandId
                    && x.ClreviewStatus == (byte)ClreviewStatus.Rejected).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                    if (InterviewDtls != null)
                    {
                        if (!string.IsNullOrEmpty(InterviewDtls.Reasons))
                        {
                            dtls.ReasonsText = InterviewDtls.Reasons.Split(',').ToList();
                        }
                        dtls.RejectOtherReason = InterviewDtls.Remarks;
                    }

                    if (!string.IsNullOrEmpty(dtls.Roles))
                    {
                        List<int> RoleIds = dtls.Roles.Split(',').Select(int.Parse).ToList();
                        dtls.RolesText = new List<string>();
                        dtls.RolesText = dbContext.PhRefMasters.Where(x => x.GroupId == 191 && RoleIds.Contains(x.Id)).Select(x => x.Rmvalue).ToList();
                    }
                    if (dtls.CandProfStatus != null)
                    {
                        var status = dbContext.PhCandStatusSes.Where(x => x.Id == dtls.CandProfStatus).FirstOrDefault();
                        if (status != null)
                        {
                            dtls.CandProfStatusName = status.Title;
                            dtls.CandProfStatusCode = status.Cscode;
                        }
                    }
                    if (dtls.CountryID != 0)
                    {
                        dtls.CountryName = dbContext.PhCountries.Where(x => x.Id == dtls.CountryID).Select(x => x.Nicename).FirstOrDefault();
                    }
                    if (dtls.BudgetCurrencyId != 0)
                    {
                        dtls.BudgetCurrency = dbContext.PhRefMasters.Where(x => x.GroupId == 13 && x.Id == dtls.BudgetCurrencyId).Select(x => x.Rmvalue).FirstOrDefault();
                    }
                    if (dtls.SourceId != 0)
                    {
                        if (dtls.CreatedBy == null || dtls.CreatedBy == 0)
                        {
                            if (dtls.SourceId == (byte)SourceType.Website || dtls.SourceId == (byte)SourceType.CandidatePortal)
                            {
                                dtls.Sourced = " Sourced from Website " + GetTimeDiff(dtls.CreatedDate.Value);
                            }
                            else if (dtls.SourceId == (byte)SourceType.Google)
                            {
                                dtls.Sourced = " Sourced from Google SignUp " + GetTimeDiff(dtls.CreatedDate.Value);
                            }
                            else if (dtls.SourceId == (byte)SourceType.Facebook)
                            {
                                dtls.Sourced = " Sourced from Facebook SignUp " + GetTimeDiff(dtls.CreatedDate.Value);
                            }
                        }
                        else
                        {
                            string SourceBy = Enum.GetName(typeof(SourceType), dtls.SourceId);
                            var user = dbContext.PiHireUsers.Where(x => x.Id == dtls.CreatedBy).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                            if (!string.IsNullOrEmpty(user))
                            {
                                if (dtls.IsTagged != null)
                                {
                                    if (dtls.IsTagged.Value)
                                    {
                                        dtls.Sourced = " Tagged by " + user + " " + GetTimeDiff(dtls.CreatedDate.Value);
                                    }
                                    else
                                    {
                                        dtls.Sourced = " Sourced from " + SourceBy + " by " + user + " " + GetTimeDiff(dtls.CreatedDate.Value);
                                    }
                                }
                                else
                                {
                                    dtls.Sourced = " Sourced from " + SourceBy + " by " + user + " " + GetTimeDiff(dtls.CreatedDate.Value);
                                }
                            }
                        }
                    }

                    var docs = await (from canDoc in dbContext.PhCandidateDocs
                                      where canDoc.CandProfId == CandId && canDoc.Status != (byte)RecordStatus.Delete &&
                                      (canDoc.DocType == "Pay Slips" || canDoc.DocType == "Payslip" || canDoc.DocType == "Profile Photo" || canDoc.DocType == "Final CV")
                                      && (canDoc.DocStatus == (byte)DocStatus.Accepted || canDoc.DocStatus == (byte)DocStatus.Notreviewd)
                                      select new CandidateFilesViewModel
                                      {
                                          DocType = canDoc.DocType,
                                          FileGroup = canDoc.FileGroup,
                                          CandProfId = canDoc.CandProfId,
                                          FileName = canDoc.FileName,
                                          CreatedDate = canDoc.CreatedDate,
                                          Joid = canDoc.Joid
                                      }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                    if (docs != null)
                    {
                        var profPhoto = docs.Where(x => x.DocType == "Profile Photo").FirstOrDefault();
                        if (profPhoto != null)
                        {
                            if (!string.IsNullOrEmpty(profPhoto.FileName))
                            {
                                profPhoto.FileName = profPhoto.FileName.Replace("#", "%23");
                                dtls.ProfilePhoto = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + profPhoto.CandProfId + "/" + profPhoto.FileName;
                            }
                        }

                        var finalCv = docs.Where(x => x.Joid == JobId && x.DocType == "Final CV").FirstOrDefault();
                        if (finalCv != null)
                        {
                            if (!string.IsNullOrEmpty(finalCv.FileName))
                            {
                                finalCv.FileName = finalCv.FileName.Replace("#", "%23");
                                dtls.FinalCVUrl = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + finalCv.CandProfId + "/" + finalCv.FileName;
                            }
                        }

                        //qualification 
                        dtls.Qualifications = new List<GetCandQualifications>();
                        dtls.Qualifications = await (from canEdu in dbContext.PhCandidateEduDetails
                                                     where canEdu.CandProfId == CandId && canEdu.Status != (byte)RecordStatus.Delete
                                                     select new GetCandQualifications
                                                     {
                                                         Course = canEdu.Course,
                                                         CreatedDate = canEdu.CreatedDate,
                                                         Qualification = canEdu.Qualification,
                                                         QualificationId = canEdu.QualificationId,
                                                         CourseId = canEdu.CourseId,
                                                         Id = canEdu.Id
                                                     }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                        //certification 
                        dtls.Certififcates = new List<GetCandCertifications>();
                        dtls.Certififcates = await (from canCert in dbContext.PhCandidateCertifications
                                                    join refData in dbContext.PhRefMasters on canCert.CertificationId equals refData.Id
                                                    where canCert.CandProfId == CandId && canCert.Status != (byte)RecordStatus.Delete
                                                    select new GetCandCertifications
                                                    {
                                                        Id = canCert.CertificationId,
                                                        CertificationName = refData.Rmvalue,
                                                        CreatedDate = refData.CreatedDate,
                                                        CertificationId = canCert.CertificationId
                                                    }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                        //questions 
                        dtls.QuestionResponse = new List<GetCandQuestionResponse>();
                        dtls.QuestionResponse = await (from jobQtn in dbContext.PhJobOpeningStQtns
                                                       join canRes in dbContext.PhJobCandidateStResponses on jobQtn.Id equals canRes.StquestionId
                                                       where canRes.Joid == JobId && canRes.CandProfId == CandId && canRes.Status != (byte)RecordStatus.Delete
                                                       select new GetCandQuestionResponse
                                                       {
                                                           Id = canRes.Id,
                                                           QuestionText = jobQtn.QuestionText,
                                                           Response = canRes.Response,
                                                           CreatedDate = canRes.CreatedDate,
                                                           QuestionType = jobQtn.QuestionType
                                                       }).OrderBy(x => x.Id).ToListAsync();

                        // re offer details
                        dtls.CandidateJobReOfferDtlsViewModel = await (from jobOffer in dbContext.PhCandReofferDetails
                                                                       where jobOffer.JoId == JobId && jobOffer.CandProfId == CandId && jobOffer.Status != (byte)RecordStatus.Delete
                                                                       select new CandidateJobReOfferDtlsViewModel
                                                                       {
                                                                           CandProfId = jobOffer.CandProfId,
                                                                           CreatedBy = jobOffer.CreatedBy,
                                                                           CreatedDate = jobOffer.CreatedDate,
                                                                           JoId = jobOffer.JoId,
                                                                           Opcurrency = jobOffer.Opcurrency,
                                                                           OpgrossPayPerMonth = jobOffer.OpgrossPayPerMonth
                                                                       }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                        var paySlips = docs.Where(x => x.Joid == JobId && (x.DocType == "Pay Slips" || x.DocType == "Payslip")).FirstOrDefault();
                        if (paySlips != null)
                        {
                            if (!string.IsNullOrEmpty(paySlips.FileName))
                            {
                                dtls.UploadedFromDrive = false;
                                if (ValidHttpURL(paySlips.FileName))
                                {
                                    dtls.UploadedFromDrive = true;
                                    dtls.PaySlip = paySlips.FileName;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(paySlips.FileName))
                                    {
                                        paySlips.FileName = paySlips.FileName.Replace("#", "%23");
                                        dtls.PaySlip = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + paySlips.CandProfId + "/" + paySlips.FileName;
                                    }
                                }
                            }
                        }
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
        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ValidateCandidateSalary(ValidateCanJobSalaryViewModel validateCanJobSalaryViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {

                    var Opening = await dbContext.PhJobOpenings.Where(x => x.Id == validateCanJobSalaryViewModel.JobId).FirstOrDefaultAsync();
                    if (Opening != null)
                    {
                        var JobCandidate = await dbContext.PhJobCandidates.Where(x => x.CandProfId == validateCanJobSalaryViewModel.CandProfId && x.Joid == validateCanJobSalaryViewModel.JobId).FirstOrDefaultAsync();

                        if (JobCandidate != null)
                        {
                            JobCandidate.UpdatedBy = UserId;
                            JobCandidate.UpdatedDate = CurrentTime;
                            JobCandidate.SalaryVerifiedby = UserId;
                            JobCandidate.PayslipCurrency = validateCanJobSalaryViewModel.Curreny;
                            JobCandidate.PayslipSalary = validateCanJobSalaryViewModel.Salary;
                            JobCandidate.IsPayslipVerified = true;


                            dbContext.PhJobCandidates.Update(JobCandidate);
                            await dbContext.SaveChangesAsync();

                            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                            var auditLog = new CreateAuditViewModel
                            {
                                ActivitySubject = " verified Payslip",
                                ActivityDesc = " has verified payslip for " + validateCanJobSalaryViewModel.JobId + " - " + validateCanJobSalaryViewModel.CandProfId + " ",
                                ActivityType = (byte)AuditActivityType.RecordUpdates,
                                TaskID = validateCanJobSalaryViewModel.CandProfId,
                                UserId = UserId
                            };
                            audList.Add(auditLog);
                            SaveAuditLog(audList);

                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Candidate,
                                ActivityOn = JobCandidate.CandProfId,
                                JobId = JobCandidate.Joid,
                                ActivityType = (byte)LogActivityType.RecordUpdates,
                                ActivityDesc = " has verified payslip for " + validateCanJobSalaryViewModel.JobId + " - " + validateCanJobSalaryViewModel.CandProfId + " ",
                                UserId = UserId
                            };
                            activityList.Add(activityLog);
                            SaveActivity(activityList);

                            // Applying workflow conditions 
                            var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                            {
                                ActionMode = (byte)WorkflowActionMode.Candidate,
                                CanProfId = validateCanJobSalaryViewModel.CandProfId,
                                CurrentStatusId = JobCandidate.CandProfStatus,
                                JobId = validateCanJobSalaryViewModel.JobId,
                                TaskCode = TaskCode.SVR.ToString(),
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
                                        CreatedBy = UserId,
                                        IsAudioNotify = true
                                    };
                                    notificationPushedViewModel.Add(notificationPushed);
                                }
                            }
                        }

                        respModel.SetResult(message);
                        respModel.Status = true;

                    }
                    else
                    {
                        message = "The Job is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(validateCanJobSalaryViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ReofferPackage(ReofferPackageViewModel reofferPackageViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {

                    var Opening = await dbContext.PhJobOpenings.Where(x => x.Id == reofferPackageViewModel.JobId).FirstOrDefaultAsync();
                    if (Opening != null)
                    {

                        var Candidate = await dbContext.PhCandidateProfiles.Where(x => x.Id == reofferPackageViewModel.CanPrfId).FirstOrDefaultAsync();
                        if (Candidate != null)
                        {
                            Candidate.ProfUpdateFlag = true;
                            Candidate.UpdatedBy = UserId;
                            Candidate.UpdatedDate = CurrentTime;

                            dbContext.PhCandidateProfiles.Update(Candidate);
                            await dbContext.SaveChangesAsync();

                            var JobCandidate = await dbContext.PhJobCandidates.Where(x => x.CandProfId == reofferPackageViewModel.CanPrfId
                            && x.Joid == reofferPackageViewModel.JobId).FirstOrDefaultAsync();
                            string CandStatusCode = string.Empty;
                            if (JobCandidate != null)
                            {
                                JobCandidate.UpdatedBy = UserId;
                                JobCandidate.UpdatedDate = CurrentTime;

                                JobCandidate.OpconfirmFlag = true;
                                JobCandidate.OpconfirmDate = CurrentTime;
                                JobCandidate.OpgrossPayPerMonth = reofferPackageViewModel.Salary;
                                JobCandidate.Opcurrency = JobCandidate.Epcurrency;

                                dbContext.PhJobCandidates.Update(JobCandidate);

                                CandStatusCode = dbContext.PhCandStatusSes.Where(x => x.Id == JobCandidate.CandProfStatus).Select(x => x.Cscode).FirstOrDefault();
                                var phCandReofferDetails = new PhCandReofferDetail
                                {
                                    JoId = reofferPackageViewModel.JobId,
                                    CreatedDate = CurrentTime,
                                    OpgrossPayPerMonth = reofferPackageViewModel.Salary,
                                    CandProfId = reofferPackageViewModel.CanPrfId,
                                    CreatedBy = UserId,
                                    Opcurrency = JobCandidate.Epcurrency
                                };
                                dbContext.PhCandReofferDetails.Add(phCandReofferDetails);

                                await dbContext.SaveChangesAsync();

                                List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                                var auditLog = new CreateAuditViewModel
                                {
                                    ActivitySubject = " Offered package ",
                                    ActivityDesc = " updated offered package for " + reofferPackageViewModel.JobId + " - " + reofferPackageViewModel.CanPrfId,
                                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                                    TaskID = reofferPackageViewModel.CanPrfId,
                                    UserId = UserId
                                };
                                audList.Add(auditLog);
                                SaveAuditLog(audList);

                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Candidate,
                                    ActivityOn = reofferPackageViewModel.CanPrfId,
                                    JobId = reofferPackageViewModel.JobId,
                                    ActivityType = (byte)LogActivityType.RecordUpdates,
                                    ActivityDesc = " has updated offered package for " + reofferPackageViewModel.JobId + " - " + reofferPackageViewModel.CanPrfId,
                                    UserId = UserId
                                };
                                activityList.Add(activityLog);
                                SaveActivity(activityList);

                                // Applying workflow conditions 
                                var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                                {
                                    CanProfId = reofferPackageViewModel.CanPrfId,
                                    CurrentStatusId = JobCandidate.CandProfStatus,
                                    JobId = reofferPackageViewModel.JobId,
                                    UserId = UserId
                                };
                                if (CandStatusCode == CandidateStatusCodes.YSP.ToString()) // Yet to Send Proposal
                                {
                                    workFlowRuleSearchViewModel.ActionMode = (byte)WorkflowActionMode.Candidate;
                                    workFlowRuleSearchViewModel.TaskCode = TaskCode.SLP.ToString(); // Salary Proposal
                                    workFlowRuleSearchViewModel.SalaryProposalOfferBenefits = reofferPackageViewModel.SalaryProposalOfferBenefits; // Salary Proposal
                                }
                                else
                                {
                                    workFlowRuleSearchViewModel.ActionMode = (byte)WorkflowActionMode.Other;
                                    workFlowRuleSearchViewModel.TaskCode = TaskCode.ROF.ToString(); // Re Offer
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
                                            CreatedBy = UserId,
                                            IsAudioNotify = true
                                        };
                                        notificationPushedViewModel.Add(notificationPushed);
                                    }
                                }
                            }

                            respModel.SetResult(message);
                            respModel.Status = true;

                        }
                        else
                        {
                            message = "The Candidate is not available";
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        }
                    }
                    else
                    {
                        message = "The Job is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(reofferPackageViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ReOfferApprove(ReOfferApproveViewModel reOfferApproveViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {

                    var Opening = await dbContext.PhJobOpenings.Where(x => x.Id == reOfferApproveViewModel.JobId).FirstOrDefaultAsync();
                    if (Opening != null)
                    {

                        var Candidate = await dbContext.PhCandidateProfiles.Where(x => x.Id == reOfferApproveViewModel.CanPrfId).FirstOrDefaultAsync();
                        if (Candidate != null)
                        {
                            Candidate.ProfUpdateFlag = true;
                            Candidate.UpdatedBy = UserId;
                            Candidate.UpdatedDate = CurrentTime;

                            dbContext.PhCandidateProfiles.Update(Candidate);
                            await dbContext.SaveChangesAsync();

                            var JobCandidate = await dbContext.PhJobCandidates.Where(x => x.CandProfId == reOfferApproveViewModel.CanPrfId && x.Joid == reOfferApproveViewModel.JobId).FirstOrDefaultAsync();

                            if (JobCandidate != null)
                            {
                                JobCandidate.UpdatedBy = UserId;
                                JobCandidate.UpdatedDate = CurrentTime;
                                JobCandidate.OpconfirmFlag = reOfferApproveViewModel.Approve == true ? false : true;
                                JobCandidate.OpconfirmDate = CurrentTime;

                                dbContext.PhJobCandidates.Update(JobCandidate);
                                await dbContext.SaveChangesAsync();

                                string Confirmation = reOfferApproveViewModel.Approve == true ? "Accepted" : "Rejected";

                                List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                                var auditLog = new CreateAuditViewModel
                                {
                                    ActivitySubject = " Salary confirmation",
                                    ActivityDesc = "" + Confirmation + " Salary confirmation",
                                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                                    TaskID = reOfferApproveViewModel.CanPrfId,
                                    UserId = UserId
                                };
                                audList.Add(auditLog);
                                SaveAuditLog(audList);

                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Candidate,
                                    ActivityOn = reOfferApproveViewModel.CanPrfId,
                                    JobId = reOfferApproveViewModel.JobId,
                                    ActivityType = (byte)LogActivityType.RecordUpdates,
                                    ActivityDesc = " has " + Confirmation + " the salary confirmation",
                                    UserId = UserId
                                };
                                activityList.Add(activityLog);
                                SaveActivity(activityList);

                                // applying workflow conditions 
                                var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                                {
                                    ActionMode = (byte)WorkflowActionMode.Candidate,
                                    CanProfId = JobCandidate.CandProfId,
                                    CurrentStatusId = JobCandidate.CandProfStatus,
                                    JobId = JobCandidate.Joid,
                                    UserId = UserId
                                };
                                if (Confirmation == "Accepted")
                                {
                                    workFlowRuleSearchViewModel.TaskCode = TaskCode.SAC.ToString();
                                }
                                else
                                {
                                    workFlowRuleSearchViewModel.TaskCode = TaskCode.SRJ.ToString();
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
                                            CreatedBy = UserId,
                                            IsAudioNotify = true
                                        };
                                        notificationPushedViewModel.Add(notificationPushed);
                                    }
                                }

                            }
                            respModel.SetResult(message);
                            respModel.Status = true;

                        }
                        else
                        {
                            message = "The Candidate is not available";
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        }
                    }
                    else
                    {
                        message = "The Job is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(reOfferApproveViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        public async Task<GetResponseViewModel<CandidateSkillViewModel>> GetCandidateSkills(int CandId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateSkillViewModel>();
            var response = new CandidateSkillViewModel
            {
                JobsSkillViewModel = new List<GetCandidateSkillViewModel>(),
                CandidatesSkillViewModel = new List<GetCandidateSkillViewModel>()
            };
            try
            {


                response.JobsSkillViewModel = await (from Jobskill in dbContext.PhJobOpeningSkills
                                                     join Canskill in dbContext.PhCandidateSkillsets on Jobskill.TechnologyId equals Canskill.TechnologyId
                                                     where Canskill.CandProfId == CandId
                                                     && Jobskill.Joid == JobId
                                                     && Jobskill.Status == (byte)RecordStatus.Active && Canskill.Status == (byte)RecordStatus.Active
                                                     select new GetCandidateSkillViewModel
                                                     {
                                                         Id = Canskill.Id,
                                                         ExpInMonths = Canskill.ExpInMonths,
                                                         ExpInYears = Canskill.ExpInYears,
                                                         SelfRating = Canskill.SelfRating,
                                                         TechnologyId = Canskill.TechnologyId,
                                                         TechnologyName = Jobskill.Technology
                                                     }).ToListAsync();
                List<int> JobSkillId = new List<int>();
                if (response.JobsSkillViewModel.Count > 0)
                {
                    JobSkillId = response.JobsSkillViewModel.Select(x => x.TechnologyId).ToList();
                }
                response.CandidatesSkillViewModel = await (from CandidateSkill in dbContext.PhCandidateSkillsets
                                                           join Skill in dbContext.PhTechnologysSes on CandidateSkill.TechnologyId equals Skill.Id
                                                           where CandidateSkill.CandProfId == CandId
                                                           && !JobSkillId.Contains(CandidateSkill.TechnologyId)
                                                           && CandidateSkill.Status == (byte)RecordStatus.Active
                                                           select new GetCandidateSkillViewModel
                                                           {
                                                               Id = CandidateSkill.Id,
                                                               ExpInMonths = CandidateSkill.ExpInMonths,
                                                               ExpInYears = CandidateSkill.ExpInYears,
                                                               SelfRating = CandidateSkill.SelfRating,
                                                               TechnologyId = CandidateSkill.TechnologyId,
                                                               TechnologyName = Skill.Title
                                                           }).ToListAsync();

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
        public async Task<GetResponseViewModel<CandidateJobOfferDtlsViewModel>> GetCandidateJobPackageDtls(int CandId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateJobOfferDtlsViewModel>();
            try
            {

                var dtls = new CandidateJobOfferDtlsViewModel();

                dtls = await (from canJob in dbContext.PhJobCandidates
                              join canPrf in dbContext.PhCandidateProfiles on canJob.CandProfId equals canPrf.Id
                              join stge in dbContext.PhCandStagesSes on canJob.StageId equals stge.Id
                              join canStatus in dbContext.PhCandStatusSes on canJob.CandProfStatus equals canStatus.Id
                              join job in dbContext.PhJobOpenings on canJob.Joid equals job.Id
                              join jobDtls in dbContext.PhJobOpeningsAddlDetails on canJob.Joid equals jobDtls.Joid
                              where canJob.CandProfId == CandId && canJob.Joid == JobId
                              select new CandidateJobOfferDtlsViewModel
                              {
                                  CPCurrency = canPrf.Cpcurrency,
                                  CPDeductionsPerAnnum = canPrf.CpdeductionsPerAnnum,
                                  CPGrossPayPerAnnum = canPrf.CpgrossPayPerAnnum,
                                  CPTakeHomeSalPerMonth = canPrf.CptakeHomeSalPerMonth,
                                  CPVariablePayPerAnnum = canPrf.CpvariablePayPerAnnum,
                                  EPCurrency = canJob.Epcurrency,
                                  EPTakeHomeSalPerMonth = canJob.EptakeHomePerMonth,
                                  Opcurrency = canJob.Opcurrency,
                                  OpdeductionsPerAnnum = canJob.OpdeductionsPerAnnum,
                                  OpgrossPayPerAnnum = canJob.OpgrossPayPerAnnum,
                                  OpgrossPayPerMonth = canJob.OpgrossPayPerMonth,
                                  OpnetPayPerAnnum = canJob.OpnetPayPerAnnum,
                                  OptakeHomePerMonth = canJob.OptakeHomePerMonth,
                                  OpvarPayPerAnnum = canJob.OpvarPayPerAnnum,
                                  MinSalary = jobDtls.MinSalary,
                                  MaxSalary = jobDtls.MaxSalary,
                                  BudgetCurrencyId = jobDtls.CurrencyId
                              }).FirstOrDefaultAsync();

                if (dtls != null)
                {
                    if (dtls.BudgetCurrencyId != 0)
                    {
                        dtls.BudgetCurrency = dbContext.PhRefMasters.Where(x => x.GroupId == 13 && x.Id == dtls.BudgetCurrencyId).Select(x => x.Rmvalue).FirstOrDefault();
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

        public async Task<GetResponseViewModel<List<CandidateJobsViewModel>>> GetCandidateTagJobs(int CandId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CandidateJobsViewModel>>();
            var userId = Usr.Id;
            var userType = Usr.UserTypeId;
            try
            {


                var JobTags = new List<CandidateJobsViewModel>();
                JobTags = await (from can in dbContext.PhJobCandidates
                                 join job in dbContext.PhJobOpenings on can.Joid equals job.Id
                                 join jobDtls in dbContext.PhJobOpeningsAddlDetails on job.Id equals jobDtls.Joid
                                 join stge in dbContext.PhCandStagesSes on can.StageId equals stge.Id
                                 join da in dbContext.PiHireUsers on can.RecruiterId equals da.Id into ps
                                 from da in ps.DefaultIfEmpty()
                                 join das in dbContext.PiHireUsers on job.BroughtBy equals das.Id into pss
                                 from das in pss.DefaultIfEmpty()
                                 join canStatus in dbContext.PhCandStatusSes on can.CandProfStatus equals canStatus.Id
                                 where can.CandProfId == CandId
                                 select new CandidateJobsViewModel
                                 {
                                     CandidateId = can.CandProfId,
                                     JobId = can.Joid,
                                     CreatedDate = can.CreatedDate,
                                     CandProfStatus = can.CandProfStatus,
                                     CandProfStatusName = canStatus.Title,
                                     JobTitle = job.JobTitle,
                                     RecId = can.RecruiterId,
                                     BroughtBy = job.BroughtBy,
                                     StageId = can.StageId,
                                     RecName = da.FirstName + " " + da.LastName,
                                     ProfilePhoto = da.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + can.RecruiterId + "/ProfilePhoto/" + da.ProfilePhoto : string.Empty,
                                     BdmProfilePhoto = das.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + job.BroughtBy + "/ProfilePhoto/" + das.ProfilePhoto : string.Empty,
                                     BdmName = das.FirstName + " " + das.LastName,
                                     StageName = stge.Title,
                                     IsTagged = can.IsTagged
                                 }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                //if (userType == (byte)UserType.BDM)
                //{
                //    JobTags = JobTags.Where(da => da.BroughtBy == userId).ToList();
                //}
                //else if (userType == (byte)UserType.Recruiter)
                //{
                //    JobTags = JobTags.Where(da => da.RecId == userId).ToList();
                //}

                respModel.SetResult(JobTags);
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

        public async Task<GetResponseViewModel<CandidateFilesViewModel>> GetCandidateResume(ResumeViewModel resumeViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateFilesViewModel>();
            try
            {


                var documents = await (from canDoc in dbContext.PhCandidateDocs
                                       where canDoc.CandProfId == resumeViewModel.CandPrfId
                                       && canDoc.Joid == resumeViewModel.JobId
                                       && canDoc.DocType == resumeViewModel.DocType
                                       && canDoc.FileGroup == (byte)FileGroup.Profile
                                       && canDoc.Status != (byte)RecordStatus.Delete
                                       select new CandidateFilesViewModel
                                       {
                                           FileName = canDoc.FileName,
                                           CandProfId = canDoc.CandProfId,
                                           Id = canDoc.Id,
                                           DocType = canDoc.DocType,
                                           FileGroup = canDoc.FileGroup,
                                           DocStatus = canDoc.DocStatus,
                                           Remarks = canDoc.Remerks,
                                           CreatedDate = canDoc.CreatedDate,
                                           UploadedFromDrive = false
                                       }).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                if (documents != null)
                {
                    if (!string.IsNullOrEmpty(documents.FileName))
                    {
                        if (ValidHttpURL(documents.FileName))
                        {
                            documents.FilePath = documents.FileName;
                            documents.UploadedFromDrive = true;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(documents.FileName))
                            {
                                documents.FileName = documents.FileName.Replace("#", "%23");
                                documents.FilePath = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + documents.CandProfId + "/" + documents.FileName;
                            }
                        }
                    }
                }

                respModel.SetResult(documents);
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

        public async Task<GetResponseViewModel<CandidateFilesViewModel>> GetCandidateVideoProfile(ResumeViewModel resumeViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateFilesViewModel>();
            try
            {


                var documents = await (from canDoc in dbContext.PhCandidateDocs
                                       where canDoc.CandProfId == resumeViewModel.CandPrfId
                                       && canDoc.Joid == resumeViewModel.JobId && canDoc.DocType == resumeViewModel.DocType
                                       && canDoc.FileGroup == (byte)FileGroup.Profile && canDoc.DocStatus == (byte)DocStatus.Accepted
                                       select new CandidateFilesViewModel
                                       {
                                           FileName = canDoc.FileName,
                                           CandProfId = canDoc.CandProfId,
                                           Id = canDoc.Id,
                                           DocType = canDoc.DocType,
                                           FileGroup = canDoc.FileGroup,
                                           DocStatus = canDoc.DocStatus,
                                           Remarks = canDoc.Remerks,
                                           CreatedDate = canDoc.CreatedDate,
                                           UploadedFromDrive = false
                                       }).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                if (documents != null)
                {
                    if (!string.IsNullOrEmpty(documents.FileName))
                    {
                        if (ValidHttpURL(documents.FileName))
                        {
                            documents.UploadedFromDrive = true;
                            documents.FilePath = documents.FileName;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(documents.FileName))
                            {
                                documents.FileName = documents.FileName.Replace("#", "%23");
                                documents.FilePath = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + documents.CandProfId + "/" + documents.FileName;
                            }
                        }
                    }
                }

                respModel.SetResult(documents);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(resumeViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        public async Task<GetResponseViewModel<JobActivities>> GetCandidateActivities(int JobId, int CanPrfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<JobActivities>();
            try
            {


                var JobActivities = new JobActivities();
                var response = await dbContext.GeJobActiviesList(JobId);
                if (response != null)
                {
                    response = response.Where(x => x.ActivityOn == CanPrfId).ToList();

                    var groupByActionTypes = response.Where(x => x.ActivityMode == (byte)WorkflowActionMode.Candidate).GroupBy(x => x.ActivityType).Select(x => x.Key).ToList();
                    JobActivities.ActionTypesViewModel = new List<ActionTypesViewModel>();
                    foreach (var item in groupByActionTypes)
                    {
                        var dtls = new ActionTypesViewModel
                        {
                            Id = item,
                            ActionTypeName = EnumKeyName(item, "ActivityType")
                        };
                        JobActivities.ActionTypesViewModel.Add(dtls);
                    }

                    var groupByUsers = response.Where(x => x.ActivityMode == (byte)WorkflowActionMode.Candidate).GroupBy(x => x.CreatedBy).Select(grp => grp.First()).ToList();
                    JobActivities.ActivityCreatedBy = new List<ActivityCreatedBy>();
                    foreach (var item in groupByUsers)
                    {
                        JobActivities.ActivityCreatedBy.Add(new ActivityCreatedBy
                        {
                            UserId = item.CreatedBy,
                            Name = item.CreatedByName,
                            ProfilePhoto = item.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + item.CreatedBy + "/ProfilePhoto/" + item.ProfilePhoto : string.Empty
                        });
                    }

                    var activies = (from actLog in response
                                    where actLog.ActivityMode == (byte)WorkflowActionMode.Candidate
                                    select new ActivitiesViewModel
                                    {
                                        Id = actLog.Id,
                                        ActivityDesc = actLog.ActivityDesc,
                                        CreatedBy = actLog.CreatedBy,
                                        CreatedByName = actLog.CreatedByName,
                                        CreatedDate = actLog.CreatedDate,
                                        GroupDate = actLog.CreatedDate.ToString("MM/dd/yyyy"),
                                        ActivityType = actLog.ActivityType,
                                        ProfilePhoto = actLog.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + actLog.CreatedBy + "/ProfilePhoto/" + actLog.ProfilePhoto : string.Empty
                                    }).OrderByDescending(x => x.CreatedDate).ToList();

                    JobActivities.ActivitiesViewModel = activies;
                }

                respModel.SetResult(JobActivities);
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

        public async Task<GetResponseViewModel<List<CandidateTagsViewModel>>> GetCandidateTags(int CandidateId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());


            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<CandidateTagsViewModel>>();
            try
            {

                var tags = new List<CandidateTagsViewModel>();

                tags = await (from tag in dbContext.PhCandidateTags
                              where tag.CandProfId == CandidateId && tag.Status != (byte)RecordStatus.Delete
                              select new CandidateTagsViewModel
                              {
                                  CandidateId = tag.CandProfId,
                                  JobId = tag.Joid,
                                  TagId = tag.Id,
                                  TagWord = tag.TaggingWord,
                                  CreatedDate = tag.CreatedDate
                              }).ToListAsync();

                respModel.SetResult(tags);
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

        public async Task<CreateResponseViewModel<string>> CreateCandidateTag(CreateTag createTag)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            try
            {


                var CandTags = dbContext.PhCandidateTags.Where(x => x.TaggingWord.ToLower() == createTag.TagWord.Trim().ToLower() && x.CandProfId == createTag.CandidateId && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (CandTags == null)
                {
                    var Tag = new PhCandidateTag()
                    {
                        CreatedBy = UserId,
                        CreatedDate = CurrentTime,
                        Status = (byte)RecordStatus.Active,
                        CandProfId = createTag.CandidateId,
                        TaggingWord = createTag.TagWord,
                        Joid = createTag.JobId,
                    };
                    dbContext.PhCandidateTags.Add(Tag);
                    await dbContext.SaveChangesAsync();

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " New Tag",
                        ActivityDesc = " has Created the " + createTag.TagWord + " tag for  " + createTag.CandidateId + "",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = Tag.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = createTag.CandidateId,
                        JobId = createTag.JobId,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has Created the " + createTag.TagWord + " tag for  " + createTag.CandidateId + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Tag is already available";
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

        public async Task<DeleteResponseViewModel<string>> DeleteCandidateTag(int TagId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new DeleteResponseViewModel<string>();
            string message = "Deleted Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.DeleteItem, "Start of method:", respModel.Meta.RequestID);

                var CandidateTag = await dbContext.PhCandidateTags.Where(x => x.Id == TagId && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (CandidateTag != null)
                {
                    CandidateTag.UpdatedBy = UserId;
                    CandidateTag.UpdatedDate = CurrentTime;
                    CandidateTag.Status = (byte)RecordStatus.Delete;

                    dbContext.PhCandidateTags.Update(CandidateTag);
                    await dbContext.SaveChangesAsync();


                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Delete Tag",
                        ActivityDesc = " has deleted the " + CandidateTag.TaggingWord + " tag for  " + CandidateTag.CandProfId + "",
                        ActivityType = (byte)AuditActivityType.Critical,
                        TaskID = CandidateTag.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = CandidateTag.CandProfId,
                        JobId = CandidateTag.Joid,
                        ActivityType = (byte)LogActivityType.Critical,
                        ActivityDesc = " has deleted the " + CandidateTag.TaggingWord + " tag for  " + CandidateTag.CandProfId + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);


                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Tag is not found";
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


        public async Task<CreateResponseViewModel<string>> CandidateStatusReview(CandidateStatusReviewViewModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            try
            {


                var JobCand = dbContext.PhJobCandidates.Where(x => x.Joid == model.JobId && x.CandProfId == model.CanProfId).FirstOrDefault();
                if (JobCand != null)
                {
                    if (model.ReviewBy == 1)
                    {
                        JobCand.Tlreview = model.Status;
                    }
                    else if (model.ReviewBy == 2)
                    {
                        JobCand.Mreview = model.Status;
                    }
                    else
                    {
                        JobCand.L1review = model.Status;
                    }

                    JobCand.UpdatedBy = UserId;
                    JobCand.UpdatedDate = CurrentTime;
                    dbContext.PhJobCandidates.Update(JobCand);

                    var review = new PhCandidateStatusLog()
                    {
                        CreatedBy = UserId,
                        CreatedDate = CurrentTime,
                        Status = model.Status == true ? (byte)RecordStatus.Active : (byte)RecordStatus.Inactive,
                        Joid = model.JobId,
                        CandProfStatus = model.CandProfStatus,
                        ActivityDesc = model.ActivityDesc,
                        CanProfId = model.CanProfId
                    };
                    dbContext.PhCandidateStatusLogs.Add(review);
                    await dbContext.SaveChangesAsync();

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Candidate review",
                        ActivityDesc = " has review the Candidate for  " + model.CanProfId + "",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = review.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = model.CanProfId,
                        JobId = model.JobId,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has review the Candidate for  " + model.CanProfId + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Candidate is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(model), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<GetResponseViewModel<List<CandidateStatusReviewListViewModel>>> CandidateStatusReviewList(int JobId, int CanPrfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<CandidateStatusReviewListViewModel>>();
            try
            {

                var items = new List<CandidateStatusReviewListViewModel>();

                items = await (from item in dbContext.PhCandidateStatusLogs
                               join user in dbContext.PiHireUsers on item.CreatedBy equals user.Id
                               where item.CanProfId == CanPrfId && item.Joid == JobId
                               select new CandidateStatusReviewListViewModel
                               {
                                   CanProfId = item.CanProfId,
                                   ActivityDesc = item.ActivityDesc,
                                   CandProfStatus = item.CandProfStatus,
                                   JobId = item.Joid,
                                   ReviewBy = item.CreatedBy,
                                   ReviewDate = item.CreatedDate,
                                   Status = item.Status == (byte)RecordStatus.Active ? true : false,
                                   ReviewByName = user.FirstName + " " + user.LastName
                               }).OrderByDescending(x => x.ReviewDate).ToListAsync();

                respModel.SetResult(items);
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

        #endregion

        #region Candidate file view actions

        public async Task<GetResponseViewModel<List<CandidateFilesViewModel>>> GetCandidateFiles(int CandPrfId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CandidateFilesViewModel>>();
            try
            {


                var documents = await (from canDoc in dbContext.PhCandidateDocs
                                       join user in dbContext.PiHireUsers on canDoc.CreatedBy equals user.Id
                                       where canDoc.CandProfId == CandPrfId && canDoc.Joid == JobId
                                       && canDoc.Status != (byte)RecordStatus.Delete
                                       select new CandidateFilesViewModel
                                       {
                                           Id = canDoc.Id,
                                           CandProfId = canDoc.CandProfId,
                                           DocType = canDoc.DocType.Trim(),
                                           FileGroup = canDoc.FileGroup,
                                           FileName = canDoc.FileName,
                                           FileType = canDoc.FileType,
                                           Joid = canDoc.Joid,
                                           UploadedBy = canDoc.UploadedBy,
                                           UploadedByName = user.FirstName,
                                           DocStatus = canDoc.DocStatus,
                                           Remarks = canDoc.Remerks,
                                           CreatedDate = canDoc.CreatedDate,
                                           UploadedFromDrive = false,
                                           ProfilePhoto = user.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + user.Id + "/ProfilePhoto/" + user.ProfilePhoto : string.Empty
                                       }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                documents = documents.Where(x => x.DocType != "Base CV" || x.DocType != "Final CV" || x.DocType != "Video Profile").ToList();
                foreach (var item in documents)
                {
                    if (item.FileGroup != 0)
                    {
                        item.FileGroupName = Enum.GetName(typeof(FileGroup), item.FileGroup);
                    }
                    if (!string.IsNullOrEmpty(item.FileName))
                    {
                        if (ValidHttpURL(item.FileName))
                        {
                            item.UploadedFromDrive = true;
                            item.FilePath = item.FileName;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(item.FileName))
                            {
                                item.FileName = item.FileName.Replace("#", "%23");
                                item.FilePath = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + item.CandProfId + "/" + item.FileName;
                            }
                        }
                    }
                }

                respModel.SetResult(documents);
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

        public async Task<GetResponseViewModel<List<CandidateDocumentsModel>>> GetCandidateRequestedFiles(int CandPrfId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CandidateDocumentsModel>>();
            try
            {


                var pendingDocuments = await (from docs in dbContext.PhCandidateDocs
                                              where docs.Joid == JobId && docs.CandProfId == CandPrfId && docs.Status != (byte)RecordStatus.Delete
                                              && docs.DocStatus == (byte)DocStatus.Requested
                                              select new CandidateDocumentsModel
                                              {
                                                  Id = docs.Id,
                                                  CandProfId = docs.CandProfId,
                                                  DocStatus = docs.DocStatus,
                                                  DocTypeName = docs.DocType,
                                                  FileGroup = docs.FileGroup,
                                                  Joid = docs.Joid,
                                                  DocStatusName = string.Empty,
                                                  DocType = string.IsNullOrEmpty(docs.DocType) ? 0 : dbContext.PhRefMasters.Where(x => x.Rmvalue == docs.DocType && x.GroupId == 15).Select(x => x.Id).FirstOrDefault(),
                                                  FileGroupName = string.Empty
                                              }).ToListAsync();
                foreach (var docItem in pendingDocuments)
                {
                    if (docItem.FileGroup != 0)
                    {
                        docItem.FileGroupName = Enum.GetName(typeof(FileGroup), docItem.FileGroup);
                    }
                    if (docItem.DocStatus != 0)
                    {
                        docItem.DocStatusName = Enum.GetName(typeof(DocStatus), docItem.DocStatus);
                    }
                }
                respModel.SetResult(pendingDocuments);
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


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> UploadCandidateFiles(UploadCandidateFileViewModel uploadCandidateFileViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = "Uploaded Successfully";
            try
            {

                var canDtls = (from CandidateProfile in dbContext.PhCandidateProfiles
                               join JobCand in dbContext.PhJobCandidates on CandidateProfile.Id equals JobCand.CandProfId
                               join CandStatu in dbContext.PhCandStatusSes on JobCand.CandProfStatus equals CandStatu.Id
                               where JobCand.Joid == uploadCandidateFileViewModel.JobId && JobCand.CandProfId == uploadCandidateFileViewModel.CandId
                               select new
                               {
                                   CandidateProfile.CandName,
                                   CandStatu.Cscode,
                                   JobCand.CandProfStatus
                               }).FirstOrDefault();
                var candidate = dbContext.PhCandidateProfiles.Where(x => x.Id == uploadCandidateFileViewModel.CandId).Select(x => x.CandName).FirstOrDefault();
                if (canDtls != null)
                {
                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    foreach (var item in uploadCandidateFileViewModel.Files)
                    {
                        if (uploadCandidateFileViewModel.DocType == "Base CV" || uploadCandidateFileViewModel.DocType == "Candidate CV"
                            || uploadCandidateFileViewModel.DocType == "Final CV" || uploadCandidateFileViewModel.DocType == "Video Profile")
                        {

                            var CandDocs = await dbContext.PhCandidateDocs.Where(x => x.CandProfId == uploadCandidateFileViewModel.CandId
                             && x.Joid == uploadCandidateFileViewModel.JobId && x.FileGroup == uploadCandidateFileViewModel.FileGroup
                             && x.DocType == uploadCandidateFileViewModel.DocType
                             && (x.Status != (byte)RecordStatus.Delete || x.DocStatus != (byte)DocStatus.Rejected)).OrderByDescending(x => x.CreatedDate).ToListAsync();
                            foreach (var CandDoc in CandDocs)
                            {
                                CandDoc.Status = (byte)RecordStatus.Delete;
                                CandDoc.UpdatedBy = UserId;
                                CandDoc.UpdatedDate = CurrentTime;
                                dbContext.PhCandidateDocs.Update(CandDoc);
                                await dbContext.SaveChangesAsync();
                            }

                            if (uploadCandidateFileViewModel.DocType == "Final CV")
                            {
                                var CandDoc = dbContext.PhCandidateDocs.Where(x => x.CandProfId == uploadCandidateFileViewModel.CandId
                                && x.Joid == uploadCandidateFileViewModel.JobId && x.DocType == uploadCandidateFileViewModel.DocType).FirstOrDefault();
                                if (CandDoc == null)
                                {
                                    var CanRecDtls = dbContext.PhJobCandidates.Where(x => x.Joid == uploadCandidateFileViewModel.JobId
                                        && x.CandProfId == uploadCandidateFileViewModel.CandId).FirstOrDefault();
                                    if (CanRecDtls != null)
                                    {
                                        var assignmentDtls = dbContext.PhJobAssignments.Where(x => x.Joid == uploadCandidateFileViewModel.JobId
                                        && x.AssignedTo == CanRecDtls.RecruiterId.Value).FirstOrDefault();
                                        if (assignmentDtls != null)
                                        {
                                            if (assignmentDtls.NoOfFinalCvsFilled == null)
                                            {
                                                assignmentDtls.NoOfFinalCvsFilled = 0;
                                            }
                                            assignmentDtls.NoOfFinalCvsFilled += 1;
                                            assignmentDtls.UpdatedBy = UserId;
                                            assignmentDtls.UpdatedDate = CurrentTime;
                                            PhJobAssignmentsDayWise_records(ref assignmentDtls, CurrentTime, incrementFinalCvsFilled: 1);
                                            dbContext.PhJobAssignments.Update(assignmentDtls);

                                            CanRecDtls.ProfileUpdateFlag = true;
                                            dbContext.PhJobCandidates.Update(CanRecDtls);
                                            dbContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }

                        string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + uploadCandidateFileViewModel.CandId + "";

                        // Checking for folder is available or not 
                        if (!Directory.Exists(webRootPath))
                        {
                            Directory.CreateDirectory(webRootPath);
                        }

                        var fileName = Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + item.FileName);
                        fileName = fileName.Replace(" ", "_");
                        if (fileName.Length > 200)
                        {
                            fileName = fileName.Substring(0, 199);
                        }
                        var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                        if ((System.IO.File.Exists(filePath)))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await item.CopyToAsync(fileStream);
                        }

                        var candidateDoc = new PhCandidateDoc
                        {
                            Joid = uploadCandidateFileViewModel.JobId,
                            Status = (byte)RecordStatus.Active,
                            CandProfId = uploadCandidateFileViewModel.CandId,
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            UploadedBy = UserId,
                            FileGroup = uploadCandidateFileViewModel.FileGroup,
                            DocType = uploadCandidateFileViewModel.DocType,
                            FileName = fileName,
                            FileType = item.ContentType,
                            DocStatus = (byte)DocStatus.Notreviewd
                        };

                        dbContext.PhCandidateDocs.Add(candidateDoc);
                        await dbContext.SaveChangesAsync();

                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = uploadCandidateFileViewModel.CandId,
                            JobId = uploadCandidateFileViewModel.JobId,
                            ActivityType = (byte)LogActivityType.RecordUpdates,
                            ActivityDesc = " has Uploaded the " + uploadCandidateFileViewModel.DocType + " document for " + candidate,
                            UserId = UserId
                        };
                        activityList.Add(activityLog);

                        if (uploadCandidateFileViewModel.DocType == "Final CV")//&& canDtls.Cscode == "CFD"
                        {
                            // applying workflow conditions 
                            var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                            {
                                ActionMode = (byte)WorkflowActionMode.Candidate,
                                CanProfId = uploadCandidateFileViewModel.CandId,
                                JobId = uploadCandidateFileViewModel.JobId,
                                TaskCode = TaskCode.FCV.ToString(), // Final CV
                                UserId = UserId,
                                CurrentStatusId = canDtls.CandProfStatus
                            };
                            var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
                            if (wfResp.Status && wfResp.isNotification)
                            {
                                foreach (var wFNotification in wfResp.WFNotifications)
                                {
                                    var notificationPushed = new NotificationPushedViewModel
                                    {
                                        JobId = wfResp.JoId,
                                        PushedTo = wFNotification.UserIds,
                                        NoteDesc = wFNotification.NoteDesc,
                                        Title = wFNotification.Title,
                                        CreatedBy = UserId
                                    };
                                    notificationPushedViewModel.Add(notificationPushed);
                                }
                            }
                        }
                    }

                    if (activityList.Count > 0)
                    {
                        SaveActivity(activityList);
                    }

                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Candidate Documents",
                        ActivityDesc = " has uploaded the Candidate Documents",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = uploadCandidateFileViewModel.CandId,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = "Candidate is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(uploadCandidateFileViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        public async Task<CreateResponseViewModel<string>> UpdateCandidateFile(UpdateCandidateFileViewModel uploadCandidateFileViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string message = "Uploaded Successfully";
            try
            {


                List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                foreach (var item in uploadCandidateFileViewModel.Files)
                {
                    string webRootPath = _environment.ContentRootPath + "\\Candidate" + "\\" + uploadCandidateFileViewModel.CandId + "";
                    // Checking for folder is available or not 
                    if (!Directory.Exists(webRootPath))
                    {
                        Directory.CreateDirectory(webRootPath);
                    }
                    var fileName = Path.GetFileName(CurrentTime.ToString("yyyyMMddHHmmss") + "_" + item.FileName);
                    fileName = fileName.Replace(" ", "_");
                    if (fileName.Length > 200)
                    {
                        fileName = fileName.Substring(0, 199);
                    }
                    var filePath = Path.Combine(webRootPath, string.Empty, fileName);
                    if ((System.IO.File.Exists(filePath)))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await item.CopyToAsync(fileStream);
                    }

                    var candidateDoc = dbContext.PhCandidateDocs.Where(x => x.CandProfId == uploadCandidateFileViewModel.CandId && x.Joid == uploadCandidateFileViewModel.JobId && x.Id == uploadCandidateFileViewModel.Id).FirstOrDefault();
                    if (candidateDoc != null)
                    {
                        candidateDoc.UploadedBy = UserId;
                        candidateDoc.FileType = item.ContentType;
                        candidateDoc.FileName = fileName;
                        candidateDoc.DocStatus = (byte)DocStatus.Notreviewd;
                        candidateDoc.FileGroup = uploadCandidateFileViewModel.FileGroup;
                        candidateDoc.DocType = uploadCandidateFileViewModel.DocType;

                        dbContext.PhCandidateDocs.Update(candidateDoc);
                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        if (uploadCandidateFileViewModel.DocType == "Final CV")
                        {
                            var CanRecDtls = dbContext.PhJobCandidates.Where(x => x.Joid == uploadCandidateFileViewModel.JobId
                            && x.CandProfId == uploadCandidateFileViewModel.CandId).FirstOrDefault();
                            if (CanRecDtls != null)
                            {
                                var assignmentDtls = dbContext.PhJobAssignments.Where(x => x.Joid == uploadCandidateFileViewModel.JobId
                                && x.AssignedTo == CanRecDtls.RecruiterId.Value).FirstOrDefault();
                                if (assignmentDtls != null)
                                {
                                    if (assignmentDtls.NoOfFinalCvsFilled == null)
                                    {
                                        assignmentDtls.NoOfFinalCvsFilled = 0;
                                    }
                                    assignmentDtls.NoOfFinalCvsFilled += 1;
                                    assignmentDtls.UpdatedBy = UserId;
                                    assignmentDtls.UpdatedDate = CurrentTime;
                                    PhJobAssignmentsDayWise_records(ref assignmentDtls, CurrentTime, incrementFinalCvsFilled: 1);
                                    dbContext.PhJobAssignments.Update(assignmentDtls);

                                    CanRecDtls.ProfileUpdateFlag = true;
                                    dbContext.PhJobCandidates.Update(CanRecDtls);

                                    dbContext.SaveChanges();
                                }
                            }
                        }

                        candidateDoc = new PhCandidateDoc
                        {
                            Joid = uploadCandidateFileViewModel.JobId,
                            Status = (byte)RecordStatus.Active,
                            CandProfId = uploadCandidateFileViewModel.CandId,
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            UploadedBy = UserId,
                            FileGroup = uploadCandidateFileViewModel.FileGroup,
                            DocType = uploadCandidateFileViewModel.DocType,
                            FileName = fileName,
                            FileType = item.ContentType,
                            DocStatus = (byte)DocStatus.Notreviewd
                        };

                        dbContext.PhCandidateDocs.Add(candidateDoc);
                        await dbContext.SaveChangesAsync();
                    }

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = uploadCandidateFileViewModel.CandId,
                        JobId = uploadCandidateFileViewModel.JobId,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has Uploaded the " + uploadCandidateFileViewModel.DocType + " document for " + uploadCandidateFileViewModel.CandId,
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                }

                if (activityList.Count > 0)
                {
                    SaveActivity(activityList);
                }

                var auditLog = new CreateAuditViewModel
                {
                    ActivitySubject = " Candidate Documents",
                    ActivityDesc = " has uploaded the " + uploadCandidateFileViewModel.DocType + " document for " + uploadCandidateFileViewModel.CandId,
                    ActivityType = (byte)AuditActivityType.RecordUpdates,
                    TaskID = uploadCandidateFileViewModel.CandId,
                    UserId = UserId
                };
                audList.Add(auditLog);
                SaveAuditLog(audList);

                respModel.SetResult(message);
                respModel.Status = true;

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(uploadCandidateFileViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<CreateResponseViewModel<string>> CandidateFileApproveReject(CandidateFileApproveRejectViewModel candidateFileApproveRejectViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string Message = " Updated Successfully";
            try
            {


                var docs = await dbContext.PhCandidateDocs.Where(canDoc => canDoc.Joid == candidateFileApproveRejectViewModel.Joid
                && canDoc.CandProfId == candidateFileApproveRejectViewModel.CandProfId && canDoc.Id == candidateFileApproveRejectViewModel.Id).FirstOrDefaultAsync();
                if (docs != null)
                {
                    string updatedDocStatus = string.Empty;
                    string currentDocStatus = string.Empty;
                    string ActivityDesc = string.Empty;

                    if (docs.DocStatus != 0)
                    {
                        currentDocStatus = Enum.GetName(typeof(DocStatus), docs.DocStatus);
                        updatedDocStatus = Enum.GetName(typeof(DocStatus), candidateFileApproveRejectViewModel.DocStatus);
                        ActivityDesc = " has updated file " + currentDocStatus + " to " + updatedDocStatus + ".";
                    }

                    if (!string.IsNullOrEmpty(candidateFileApproveRejectViewModel.Remarks))
                    {
                        docs.Remerks = candidateFileApproveRejectViewModel.Remarks;
                        ActivityDesc = ActivityDesc + " Remarks : " + candidateFileApproveRejectViewModel.Remarks + "";
                    }

                    docs.DocStatus = candidateFileApproveRejectViewModel.DocStatus;
                    docs.UpdatedBy = UserId;
                    docs.UpdatedDate = CurrentTime;

                    dbContext.PhCandidateDocs.Update(docs);
                    await dbContext.SaveChangesAsync();


                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();


                    if (candidateFileApproveRejectViewModel.DocStatus == (byte)DocStatus.Rejected)
                    {
                        var response = await dbContext.PhCandidateDocs.Where(x => x.Joid == docs.Joid
                    && x.CandProfId == docs.CandProfId
                     && x.DocType == docs.DocType && x.DocStatus == (byte)DocStatus.Requested && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                        if (response == null)
                        {
                            var candidateDoc = new PhCandidateDoc
                            {
                                Joid = docs.Joid,
                                Status = (byte)RecordStatus.Active,
                                CandProfId = docs.CandProfId,
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                UploadedBy = UserId,
                                FileGroup = docs.FileGroup,
                                DocType = docs.DocType,
                                FileName = string.Empty,
                                FileType = string.Empty,
                                DocStatus = (byte)DocStatus.Requested,
                                Remerks = candidateFileApproveRejectViewModel.Remarks
                            };

                            dbContext.PhCandidateDocs.Add(candidateDoc);
                            await dbContext.SaveChangesAsync();
                            // activity
                            var acivityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Candidate,
                                ActivityOn = candidateDoc.CandProfId,
                                JobId = candidateDoc.Joid,
                                ActivityType = (byte)LogActivityType.Other,
                                ActivityDesc = " has request to candidate (" + candidateDoc.CandProfId + ") for " + candidateDoc.DocType + " document ",
                                UserId = UserId,
                            };
                            activityList.Add(acivityLog);
                        }
                    }

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = docs.CandProfId,
                        JobId = docs.Joid,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = ActivityDesc,
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.SetResult(Message);
                    respModel.Status = true;
                }
                else
                {
                    Message = "Document is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, Message, true);
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateFileApproveRejectViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> CandidateFinalCVReject(CandidateFinalCVRejectViewModel candidateFinalCVRejectViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            string ActivityDesc = "";
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = " Updated Successfully";

            try
            {


                var canDtls = (from CandidateProfile in dbContext.PhCandidateProfiles
                               join JobCand in dbContext.PhJobCandidates on CandidateProfile.Id equals JobCand.CandProfId
                               join CandStatu in dbContext.PhCandStatusSes on JobCand.CandProfStatus equals CandStatu.Id
                               where JobCand.Joid == candidateFinalCVRejectViewModel.Joid && JobCand.CandProfId == candidateFinalCVRejectViewModel.CandProfId
                               select new
                               {
                                   CandidateProfile.CandName,
                                   CandStatu.Cscode,
                                   JobCand.CandProfStatus,
                                   JobCand.RecruiterId
                               }).FirstOrDefault();
                if (canDtls != null)
                {
                    ActivityDesc = " has Rejected " + canDtls.CandName + " final CV successfully";
                    var docs = await dbContext.PhCandidateDocs.Where(canDoc => canDoc.Joid == candidateFinalCVRejectViewModel.Joid
              && canDoc.CandProfId == candidateFinalCVRejectViewModel.CandProfId && canDoc.DocType == "Final CV").ToListAsync();
                    if (docs != null)
                    {
                        foreach (var item in docs)
                        {
                            item.DocStatus = (byte)DocStatus.Rejected;
                            item.UpdatedBy = UserId;
                            item.UpdatedDate = CurrentTime;
                            item.Status = (byte)RecordStatus.Delete;
                            item.Remerks = candidateFinalCVRejectViewModel.Remarks;

                            dbContext.PhCandidateDocs.Update(item);
                            await dbContext.SaveChangesAsync();
                        }
                        if (!string.IsNullOrEmpty(candidateFinalCVRejectViewModel.Remarks))
                        {
                            ActivityDesc = ActivityDesc + " Remarks : " + candidateFinalCVRejectViewModel.Remarks + "";
                        }
                        List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                        var UpdateCandidateStatus = dbContext.PhCandStatusSes.Select(x =>
                        new { x.Title, x.Id, x.Cscode }).Where(x => x.Cscode == CandidateStatusCodes.PIR.ToString()).FirstOrDefault();

                        // Applying workflow conditions 
                        var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                        {
                            ActionMode = (byte)WorkflowActionMode.Candidate,
                            CanProfId = candidateFinalCVRejectViewModel.CandProfId,
                            CurrentStatusId = canDtls.CandProfStatus,
                            UpdateStatusId = UpdateCandidateStatus.Id,
                            JobId = candidateFinalCVRejectViewModel.Joid,
                            TaskCode = TaskCode.CUS.ToString(),
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
                        if (!wfResp.Status)
                        {
                            respModel.Status = false;
                            message = wfResp.Message.Count > 0 ? string.Join(",", wfResp.Message).ToString() : string.Empty;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        }
                        else
                        {
                            var assignmentDtls = dbContext.PhJobAssignments.Where(x => x.Joid == candidateFinalCVRejectViewModel.Joid
                                       && x.AssignedTo == canDtls.RecruiterId.Value).FirstOrDefault();
                            if (assignmentDtls != null)
                            {
                                if (assignmentDtls.NoOfFinalCvsFilled != null && assignmentDtls.NoOfFinalCvsFilled.Value > 0)
                                {
                                    assignmentDtls.NoOfFinalCvsFilled -= 1;
                                }
                                assignmentDtls.UpdatedBy = UserId;
                                assignmentDtls.UpdatedDate = CurrentTime;
                                PhJobAssignmentsDayWise_records(ref assignmentDtls, CurrentTime, incrementFinalCvsFilled: -1);
                                dbContext.PhJobAssignments.Update(assignmentDtls);

                                dbContext.SaveChanges();
                            }


                            respModel.Status = true;
                            message = " Rejected Successfully";
                        }

                        // Activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = candidateFinalCVRejectViewModel.CandProfId,
                            JobId = candidateFinalCVRejectViewModel.Joid,
                            ActivityType = (byte)LogActivityType.RecordUpdates,
                            ActivityDesc = ActivityDesc,
                            UserId = UserId
                        };
                        activityList.Add(activityLog);
                        SaveActivity(activityList);

                        respModel.SetResult(message);
                        respModel.Status = true;
                    }
                    else
                    {
                        message = "Document is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                }
                else
                {
                    message = "Candidate is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateFinalCVRejectViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> CandidateDocumentRequest(CandidateDocumentRequestViewModel candidateDocumentRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string Message = " Requested Successfully";
            string RequestDocument = string.Empty;
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            try
            {


                List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                foreach (var item in candidateDocumentRequestViewModel.DocType)
                {
                    var response = await dbContext.PhCandidateDocs.Where(x => x.Joid == candidateDocumentRequestViewModel.Joid
                    && x.CandProfId == candidateDocumentRequestViewModel.CandProfId
                    && x.DocType == item && x.DocStatus == (byte)DocStatus.Requested && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                    if (response.Count == 0)
                    {
                        var candidateDoc = new PhCandidateDoc
                        {
                            Joid = candidateDocumentRequestViewModel.Joid,
                            Status = (byte)RecordStatus.Active,
                            CandProfId = candidateDocumentRequestViewModel.CandProfId,
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            UploadedBy = UserId,
                            FileGroup = candidateDocumentRequestViewModel.FileGroup,
                            DocType = item,
                            FileName = string.Empty,
                            FileType = string.Empty,
                            DocStatus = (byte)DocStatus.Requested,
                            Remerks = candidateDocumentRequestViewModel.Remarks
                        };

                        dbContext.PhCandidateDocs.Add(candidateDoc);
                        await dbContext.SaveChangesAsync();
                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = candidateDoc.CandProfId,
                            JobId = candidateDoc.Joid,
                            ActivityType = (byte)LogActivityType.Other,
                            ActivityDesc = " has request to candidate (" + candidateDoc.CandProfId + ") for " + candidateDoc.DocType + " document ",
                            UserId = UserId,
                        };
                        activityList.Add(activityLog);

                        if (string.IsNullOrEmpty(RequestDocument))
                        {
                            RequestDocument += "" + candidateDoc.DocType + "";
                        }
                        else
                        {
                            RequestDocument += ", " + candidateDoc.DocType + "";
                        }
                    }
                    else
                    {
                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = candidateDocumentRequestViewModel.CandProfId,
                            JobId = candidateDocumentRequestViewModel.Joid,
                            ActivityType = (byte)LogActivityType.Other,
                            ActivityDesc = " has request to candidate for " + item + " document is already available",
                            UserId = UserId
                        };
                        activityList.Add(activityLog);
                    }
                }

                if (activityList.Count > 0)
                {
                    SaveActivity(activityList);
                }

                if (!string.IsNullOrEmpty(RequestDocument))
                {
                    var recId = dbContext.PhJobCandidates.Where(x => x.Joid == candidateDocumentRequestViewModel.Joid && x.CandProfId == candidateDocumentRequestViewModel.CandProfId).Select(x => x.RecruiterId.Value).FirstOrDefault();

                    // Applying workflow conditions 
                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    {
                        ActionMode = (byte)WorkflowActionMode.Other,
                        CanProfId = candidateDocumentRequestViewModel.CandProfId,
                        JobId = candidateDocumentRequestViewModel.Joid,
                        TaskCode = TaskCode.RDC.ToString(),
                        AssignTo = recId,
                        RequestDocuments = RequestDocument,
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

                respModel.SetResult(Message);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateDocumentRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }



        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> CandidateMultiDocumentRequest(CandidateMultiDocumentRequestViewModel candidateDocumentRequestViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string Message = " Requested Successfully";
            string RequestDocument = string.Empty;
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            try
            {


                List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                foreach (var item in candidateDocumentRequestViewModel.DocumentsIdsViewModel)
                {
                    var response = await dbContext.PhCandidateDocs.Where(x => x.Joid == candidateDocumentRequestViewModel.Joid
                    && x.CandProfId == candidateDocumentRequestViewModel.CandProfId
                     && x.DocType == item.DocType && x.DocStatus == (byte)DocStatus.Requested && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                    if (response.Count == 0)
                    {
                        var candidateDoc = new PhCandidateDoc
                        {
                            Joid = candidateDocumentRequestViewModel.Joid,
                            Status = (byte)RecordStatus.Active,
                            CandProfId = candidateDocumentRequestViewModel.CandProfId,
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            UploadedBy = UserId,
                            FileGroup = item.FileGroup,
                            DocType = item.DocType,
                            FileName = string.Empty,
                            FileType = string.Empty,
                            DocStatus = (byte)DocStatus.Requested,
                            Remerks = candidateDocumentRequestViewModel.Remarks
                        };

                        dbContext.PhCandidateDocs.Add(candidateDoc);
                        await dbContext.SaveChangesAsync();
                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = candidateDoc.CandProfId,
                            JobId = candidateDoc.Joid,
                            ActivityType = (byte)LogActivityType.Other,
                            ActivityDesc = " has request to candidate (" + candidateDoc.CandProfId + ") for "
                            + candidateDoc.DocType + " document ",
                            UserId = UserId,
                        };
                        activityList.Add(activityLog);

                        if (string.IsNullOrEmpty(RequestDocument))
                        {
                            RequestDocument += "" + candidateDoc.DocType + "";
                        }
                        else
                        {
                            RequestDocument += ", " + candidateDoc.DocType + "";
                        }
                    }
                    else
                    {
                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = candidateDocumentRequestViewModel.CandProfId,
                            JobId = candidateDocumentRequestViewModel.Joid,
                            ActivityType = (byte)LogActivityType.Other,
                            ActivityDesc = " has request to candidate for " + item + " document is already available",
                            UserId = UserId
                        };
                        activityList.Add(activityLog);
                    }
                }

                if (activityList.Count > 0)
                {
                    SaveActivity(activityList);
                }

                if (!string.IsNullOrEmpty(RequestDocument))
                {
                    var recId = dbContext.PhJobCandidates.Where(x => x.Joid == candidateDocumentRequestViewModel.Joid && x.CandProfId == candidateDocumentRequestViewModel.CandProfId).Select(x => x.RecruiterId.Value).FirstOrDefault();

                    // Applying workflow conditions 
                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    {
                        ActionMode = (byte)WorkflowActionMode.Other,
                        CanProfId = candidateDocumentRequestViewModel.CandProfId,
                        JobId = candidateDocumentRequestViewModel.Joid,
                        TaskCode = TaskCode.RDC.ToString(),
                        AssignTo = recId,
                        RequestDocuments = RequestDocument,
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

                respModel.SetResult(Message);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateDocumentRequestViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }

        #endregion

        #region Assessments

        public async Task<GetResponseViewModel<List<CandidateAssessmentViewModel>>> CandidateAssessments(int CandPrfId, int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CandidateAssessmentViewModel>>();
            try
            {
                var data = new List<AssessmentViewModel>();
                data = await GetAssessmentList();


                var Assessments = new List<CandidateAssessmentViewModel>();

                var jobOpenings = dbContext.PhJobOpenings.Where(x => x.Id == JobId).FirstOrDefault();
                if (jobOpenings != null)
                {
                    Assessments = await (from asstmnt in dbContext.PhJobCandidateAssemts
                                         where asstmnt.Joid == JobId && asstmnt.CandProfId == CandPrfId && asstmnt.Status != (byte)RecordStatus.Delete
                                         select new CandidateAssessmentViewModel
                                         {
                                             Id = asstmnt.Id,
                                             CanAssessmentId = asstmnt.AssmtId,
                                             JobId = asstmnt.Joid,
                                             CanAssessmentName = string.Empty,
                                             ResponseDate = asstmnt.ResponseDate,
                                             DistributionId = asstmnt.DistributionId,
                                             ContactId = asstmnt.ContactId,
                                             ResponseURL = asstmnt.ResponseUrl,
                                             ResponseStatus = asstmnt.ResponseStatus,
                                             Status = asstmnt.Status,
                                             JobAssessmentId = asstmnt.JoassmtId,
                                             AssessmentSentDate = asstmnt.CreatedDate
                                         }).ToListAsync();
                    if (Assessments.Count > 0)
                    {
                        foreach (var item in Assessments)
                        {
                            if (!string.IsNullOrEmpty(item.ResponseURL))
                            {
                                item.ResponseURL += "&isPihire=true";
                            }
                            var surveyDtls = data.Where(x => x.Id == item.CanAssessmentId).FirstOrDefault();
                            if (surveyDtls != null)
                            {
                                item.CanAssessmentName = surveyDtls.SurveyName;
                                item.PreviewURL = surveyDtls.PreviewUrl;
                            }
                            if (item.ResponseStatus == 0 || item.ResponseStatus == 1 || item.ResponseStatus == 2 || item.ResponseStatus == 3)
                            {
                                item.ResponseStatusName = EnumKeyName(item.ResponseStatus, "ResponseStatus");
                            }
                        }
                    }

                    respModel.Status = true;
                    respModel.SetResult(Assessments);
                }
                else
                {
                    string message = "Job is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
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

        public async Task<CreateResponseViewModel<string>> CreateCandidateAssessment(CreateCandidateAssessmentViewModel createCandidateAssessmentViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string message = " Created Successfully";
            try
            {


                var candidate = dbContext.PhCandidateProfiles.Where(x => x.Id == createCandidateAssessmentViewModel.CandProfId).FirstOrDefault();
                var Jobcandidate = dbContext.PhJobCandidates.Where(x => x.Joid == createCandidateAssessmentViewModel.JobId && x.CandProfId == createCandidateAssessmentViewModel.CandProfId).FirstOrDefault();
                if (Jobcandidate != null)
                {
                    var CandidateAssemt = dbContext.PhJobCandidateAssemts.Where(x => x.Joid == createCandidateAssessmentViewModel.JobId &&
                    x.CandProfId == createCandidateAssessmentViewModel.CandProfId && x.JoassmtId == createCandidateAssessmentViewModel.JobAssessmentId &
                    x.AssmtId == createCandidateAssessmentViewModel.CanAssessmentId
                    && x.ResponseStatus != (byte)ContactDistributionResponseStatus.Interepted).FirstOrDefault();

                    var JobTitle = dbContext.PhJobOpenings.Where(x => x.Id == createCandidateAssessmentViewModel.JobId).Select(x => x.JobTitle).FirstOrDefault();
                    if (CandidateAssemt == null)
                    {
                        var Assessment = new PhJobCandidateAssemt
                        {
                            CandProfId = createCandidateAssessmentViewModel.CandProfId,
                            AssmtId = createCandidateAssessmentViewModel.CanAssessmentId,
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            JoassmtId = createCandidateAssessmentViewModel.JobAssessmentId,
                            Joid = createCandidateAssessmentViewModel.JobId,
                            ResponseDate = null,
                            DistributionId = string.Empty,
                            ContactId = string.Empty,
                            ResponseStatus = (byte)ContactDistributionResponseStatus.NotStarted,
                            Status = (byte)RecordStatus.Active,
                            ResponseUrl = string.Empty
                        };
                        dbContext.PhJobCandidateAssemts.Add(Assessment);
                        await dbContext.SaveChangesAsync();

                        var sendAssessmentViewModel = new SendAssessmentViewModel
                        {
                            EmailTemplateID = this.appSettings.AppSettingsProperties.AssessmentTemplateId,
                            FromEmail = string.Empty,
                            FromName = string.Empty,
                            GroupID = string.Empty,
                            ReplyToEmail = string.Empty,
                            Subject = " Assessment for the " + createCandidateAssessmentViewModel.JobId + " - " + JobTitle + " position",
                            SurveyExpiryDays = this.appSettings.AppSettingsProperties.SurveyExpiryDays,
                            SurveyID = createCandidateAssessmentViewModel.CanAssessmentId,
                            SurveyInstanceID = string.Empty,
                            WhenToSend = DateTime.UtcNow.AddMinutes(this.appSettings.AppSettingsProperties.WhenToSend).ToString("dd-MMM-yyyy HH:mm:ss")
                        };
                        sendAssessmentViewModel.Contacts = new List<AssessmentContacts>();
                        var AssessmentContacts = new AssessmentContacts
                        {
                            Email = candidate.EmailId,
                            FirstName = candidate.CandName,
                            LastName = string.Empty
                        };
                        AssessmentContacts.AdditionalData = new AssessmentAdditionalData();
                        var AssessmentAdditionalData = new AssessmentAdditionalData
                        {
                            JOB_TITLE = JobTitle
                        };
                        AssessmentContacts.AdditionalData = AssessmentAdditionalData;
                        sendAssessmentViewModel.Contacts.Add(AssessmentContacts);


                        List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                        var resp = await SendAssessmentRules(sendAssessmentViewModel);

                        var candidateName = dbContext.PhCandidateProfiles.Where(x => x.Id == createCandidateAssessmentViewModel.CandProfId).Select(x => x.CandName).FirstOrDefault();
                        if (!string.IsNullOrEmpty(resp.DistributionId))
                        {
                            Assessment.DistributionId = resp.DistributionId;

                            dbContext.PhJobCandidateAssemts.Update(Assessment);
                            dbContext.SaveChanges();

                            var phJobOpeningActvCounter = dbContext.PhJobOpeningActvCounters.Where(x => x.Joid == createCandidateAssessmentViewModel.JobId && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                            if (phJobOpeningActvCounter != null)
                            {
                                phJobOpeningActvCounter.AsmtCounter += 1;
                                dbContext.PhJobOpeningActvCounters.Update(phJobOpeningActvCounter);
                                await dbContext.SaveChangesAsync();
                            }

                            // activity
                            var activityLog1 = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Candidate,
                                ActivityOn = createCandidateAssessmentViewModel.CandProfId,
                                JobId = createCandidateAssessmentViewModel.JobId,
                                ActivityType = (byte)LogActivityType.AssessementUpdates,
                                ActivityDesc = " has shared Assessment for  " + candidateName + " ",
                                UserId = UserId
                            };
                            activityList.Add(activityLog1);


                            // audit 
                            var auditLog = new CreateAuditViewModel
                            {
                                ActivitySubject = " Created Assessment",
                                ActivityDesc = " has created the Assessment successfully",
                                ActivityType = (byte)AuditActivityType.RecordUpdates,
                                TaskID = Assessment.Id,
                                UserId = UserId
                            };
                            audList.Add(auditLog);
                            SaveAuditLog(audList);

                            respModel.Status = true;
                            respModel.SetResult(message);
                        }
                        else
                        {
                            Assessment.ResponseStatus = (byte)ContactDistributionResponseStatus.Interepted;
                            Assessment.UpdatedDate = CurrentTime;

                            dbContext.PhJobCandidateAssemts.Update(Assessment);
                            dbContext.SaveChanges();

                            message = " Failed to share Assessment, Please re-check survey settings";
                            respModel.Status = false;
                            respModel.SetResult(message);
                        }
                    }
                    else
                    {
                        message = " Assessment is already available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                    }
                }
                else
                {
                    message = " Candidate is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(createCandidateAssessmentViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> MapAssessmentResponse(PostVM postVM)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, "Start of method assessmentResponseViewModel:", Newtonsoft.Json.JsonConvert.SerializeObject(postVM));

                if (!string.IsNullOrEmpty(postVM.data))
                {
                    var assessmentResponseViewModel = JsonConvert.DeserializeObject<AssessmentResponseViewModel>(postVM.data);

                    var assessmentResponse = dbContext.PhJobCandidateAssemts.Where(x => x.DistributionId == assessmentResponseViewModel.DistributionId).FirstOrDefault();
                    if (assessmentResponse != null)
                    {
                        assessmentResponse.ResponseUrl = assessmentResponseViewModel.ResponseAnonymousUrl;
                        assessmentResponse.ResponseDate = CurrentTime; // receiving on utc time
                        if (!string.IsNullOrEmpty(assessmentResponseViewModel.ResponseStatus))
                        {
                            var resStatus = (ContactDistributionResponseStatus)Enum.Parse(typeof(ContactDistributionResponseStatus), assessmentResponseViewModel.ResponseStatus);
                            assessmentResponse.ResponseStatus = (byte)resStatus;
                        }
                        if (assessmentResponseViewModel.ContactDetails != null)
                        {
                            assessmentResponse.ContactId = assessmentResponseViewModel.ContactDetails.Id.ToString();
                        }

                        dbContext.PhJobCandidateAssemts.Update(assessmentResponse);
                        await dbContext.SaveChangesAsync();

                        // applying workflow conditions 
                        var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                        {
                            ActionMode = (byte)WorkflowActionMode.Other,
                            JobId = assessmentResponse.Joid,
                            TaskCode = TaskCode.RAR.ToString(),
                            UserId = assessmentResponse.CreatedBy
                        };
                        var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
                        if (wfResp.Status && wfResp.isNotification)
                        {
                            foreach (var itemN in wfResp.WFNotifications)
                            {
                                var notificationPushed = new NotificationPushedViewModel
                                {
                                    JobId = wfResp.JoId,
                                    PushedTo = itemN.UserIds,
                                    NoteDesc = itemN.NoteDesc,
                                    Title = itemN.Title,
                                    CreatedBy = assessmentResponse.CreatedBy
                                };
                                notificationPushedViewModel.Add(notificationPushed);
                            }
                        }

                        var candidate = dbContext.PhCandidateProfiles.Where(x => x.Id == assessmentResponse.CandProfId).FirstOrDefault();

                        List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Assessment Response",
                            ActivityDesc = " " + candidate?.CandName + " updated the Assessment Response ",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = assessmentResponse.CandProfId,
                            UserId = assessmentResponse.CreatedBy
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = assessmentResponse.CandProfId,
                            JobId = assessmentResponse.Joid,
                            ActivityType = (byte)LogActivityType.AssessementUpdates,
                            ActivityDesc = " " + candidate?.CandName + " updated the Assessment Response ",
                            UserId = assessmentResponse.CreatedBy
                        };
                        activityList.Add(activityLog);
                        SaveActivity(activityList);
                    }

                    respModel.SetResult(message);
                    respModel.Status = true;
                }
                else
                {
                    message = "No data Available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(postVM), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        #endregion

        #region Candidate qualifications and certifications and employments 

        public async Task<CreateResponseViewModel<string>> CreateCandidateCertification(CreateCandidateCertificationModel createCandidateCertificationModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new NotificationPushedViewModel();
            string message = "Saved Successfully";
            int UserId = Usr.Id;
            try
            {


                var candidate = await dbContext.PhCandidateCertifications.Where(x => x.CandProfId == createCandidateCertificationModel.CandProfId
                && x.CertificationId == createCandidateCertificationModel.CertificationID && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (candidate == null)
                {

                    var phCandidateCertifications = new PhCandidateCertification
                    {
                        Status = (byte)RecordStatus.Active,
                        CandProfId = createCandidateCertificationModel.CandProfId,
                        CreatedBy = UserId,
                        CreatedDate = CurrentTime,
                        CertificationId = createCandidateCertificationModel.CertificationID
                    };

                    dbContext.PhCandidateCertifications.Add(phCandidateCertifications);
                    await dbContext.SaveChangesAsync();

                    respModel.SetResult(message);
                    respModel.Status = true;

                }
                else
                {
                    message = "Certification is already available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(createCandidateCertificationModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        #endregion 

        #region Candidate Rating 

        public async Task<GetResponseViewModel<CandidateCSATViewModel>> GetTeamCandidateRating(int JoId, int CandProfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateCSATViewModel>();
            var response = new CandidateCSATViewModel
            {
                CandidateRatingModel = new List<CandidateRatingModel>()
            };
            try
            {


                response.CandidateRatingModel = await (from evaluation in dbContext.PhJobCandidateEvaluations
                                                       join user in dbContext.PiHireUsers on evaluation.CreatedBy equals user.Id
                                                       where evaluation.Joid == JoId && evaluation.CandProfId == CandProfId && evaluation.RatingType == (byte)ScheduledBy.PiTeam
                                                       select new CandidateRatingModel
                                                       {
                                                           CanPrfId = evaluation.CandProfId,
                                                           CreatedByName = user.FirstName + " " + user.LastName,
                                                           CreatedBy = evaluation.CreatedBy,
                                                           CreatedDate = evaluation.CreatedDate,
                                                           JobId = evaluation.Joid,
                                                           Rating = evaluation.Rating,
                                                           Remarks = evaluation.Remakrs,
                                                           RatingType = evaluation.RatingType
                                                       }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                if (response.CandidateRatingModel != null)
                {
                    if (response.CandidateRatingModel.Count > 0)
                    {
                        int castOne = response.CandidateRatingModel.Where(x => x.Rating == 1).Count();
                        int castTwo = response.CandidateRatingModel.Where(x => x.Rating == 2).Count();
                        int castThree = response.CandidateRatingModel.Where(x => x.Rating == 3).Count();
                        int castFour = response.CandidateRatingModel.Where(x => x.Rating == 4).Count();
                        int castFive = response.CandidateRatingModel.Where(x => x.Rating == 5).Count();
                        int ttlCnt = response.CandidateRatingModel.Count();

                        decimal ragTtlCnt = (1 * castOne) + (2 * castTwo) + (3 * castThree) + (4 * castFour) + (5 * castFive);
                        decimal csatScre = (ragTtlCnt / (5 * ttlCnt)) * 5;
                        response.CSATScore = Math.Round(csatScre, 2, MidpointRounding.AwayFromZero);
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

        public async Task<GetResponseViewModel<CandidateCSATViewModel>> GetClientCandidateRating(int JoId, int CandProfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateCSATViewModel>();
            var response = new CandidateCSATViewModel
            {
                CandidateRatingModel = new List<CandidateRatingModel>()
            };
            try
            {


                response.CandidateRatingModel = await (from evaluation in dbContext.PhJobCandidateEvaluations
                                                       join user in dbContext.PiHireUsers on evaluation.CreatedBy equals user.Id
                                                       join da in dbContext.PhCandidateProfilesShareds on evaluation.RefId equals da.Id into ps
                                                       from da in ps.DefaultIfEmpty()
                                                       where evaluation.Joid == JoId && evaluation.CandProfId == CandProfId && evaluation.RatingType == (byte)ScheduledBy.Client
                                                       select new CandidateRatingModel
                                                       {
                                                           CanPrfId = evaluation.CandProfId,
                                                           CreatedDate = evaluation.CreatedDate,
                                                           JobId = evaluation.Joid,
                                                           Rating = evaluation.Rating,
                                                           Remarks = evaluation.Remakrs,
                                                           CreatedBy = evaluation.CreatedBy,
                                                           CreatedByName = da.Clname,
                                                           RatingType = evaluation.RatingType
                                                       }).OrderByDescending(x => x.CreatedDate).ToListAsync();

                if (response.CandidateRatingModel != null)
                {
                    if (response.CandidateRatingModel.Count > 0)
                    {
                        int castOne = response.CandidateRatingModel.Where(x => x.Rating == 1).Count();
                        int castTwo = response.CandidateRatingModel.Where(x => x.Rating == 2).Count();
                        int castThree = response.CandidateRatingModel.Where(x => x.Rating == 3).Count();
                        int castFour = response.CandidateRatingModel.Where(x => x.Rating == 4).Count();
                        int castFive = response.CandidateRatingModel.Where(x => x.Rating == 5).Count();
                        int ttlCnt = response.CandidateRatingModel.Count();

                        decimal ragTtlCnt = (1 * castOne) + (2 * castTwo) + (3 * castThree) + (4 * castFour) + (5 * castFive);
                        decimal csatScre = (ragTtlCnt / (5 * ttlCnt)) * 5;

                        response.CSATScore = Math.Round(csatScre, 2, MidpointRounding.AwayFromZero);
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

        public async Task<CreateResponseViewModel<string>> UpdateCandidateRating(UpdateCandidateRatingModel candidateRatingModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new NotificationPushedViewModel();
            string message = "Updated Successfully";
            try
            {


                var candidate = await dbContext.PhJobCandidates.Where(x => x.Joid == candidateRatingModel.JoId
                && x.CandProfId == candidateRatingModel.CandProfId).FirstOrDefaultAsync();
                if (candidate != null)
                {
                    var candidateName = await dbContext.PhCandidateProfiles.Where(x => x.Id == candidateRatingModel.CandProfId).Select(x => x.CandName).FirstOrDefaultAsync();
                    var JobCandidateEvaluation = new PhJobCandidateEvaluation();
                    //if (candidateRatingModel.ScheduledBy == (byte)ScheduledBy.PiTeam)
                    //{
                    //    JobCandidateEvaluation = dbContext.PhJobCandidateEvaluation.Where(da => da.CandProfId == candidateRatingModel.CandProfId && da.Joid == candidateRatingModel.JoId && da.RatingType == (byte)ScheduledBy.PiTeam && da.CreatedBy == candidateRatingModel.loginUserId).FirstOrDefault();
                    //}
                    //else
                    //{
                    //    JobCandidateEvaluation = null;
                    //}
                    //if (JobCandidateEvaluation != null)
                    //{
                    //    JobCandidateEvaluation.UpdatedBy = candidateRatingModel.loginUserId;
                    //    JobCandidateEvaluation.UpdatedDate = CurrentTime;
                    //    JobCandidateEvaluation.Rating = candidateRatingModel.Rating;
                    //    JobCandidateEvaluation.Remakrs = candidateRatingModel.Remarks;
                    //    dbContext.PhJobCandidateEvaluation.Update(JobCandidateEvaluation);
                    //    await dbContext.SaveChangesAsync();
                    //}
                    //else
                    //{
                    var CandidateEvaluation = new PhJobCandidateEvaluation()
                    {
                        CandProfId = candidateRatingModel.CandProfId,
                        Remakrs = candidateRatingModel.Remarks,
                        CreatedBy = candidateRatingModel.UserId,
                        CreatedDate = CurrentTime,
                        Joid = candidateRatingModel.JoId,
                        Rating = candidateRatingModel.Rating,
                        RefId = candidateRatingModel.CanShareId,
                        Status = (byte)RecordStatus.Active,
                        RatingType = candidateRatingModel.ScheduledBy
                    };
                    dbContext.PhJobCandidateEvaluations.Add(CandidateEvaluation);

                    await dbContext.SaveChangesAsync();
                    // }

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    //Audit
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Candidate Rating",
                        ActivityDesc = " updated Rating for " + candidateName + "",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = candidateRatingModel.CandProfId,
                        UserId = candidateRatingModel.UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    //Activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = candidateRatingModel.CandProfId,
                        JobId = candidateRatingModel.JoId,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has updated Rating for " + candidateName + "",
                        UserId = candidateRatingModel.UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.SetResult(message);
                    respModel.Status = true;

                }
                else
                {
                    message = "Candidate is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateRatingModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        public async Task<CreateResponseViewModel<string>> UpdateCandidateRecruiter(UpdateCandidateRecModel candidateRatingModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new NotificationPushedViewModel();
            string message = "Updated Successfully";
            try
            {
                int UserId = Usr.Id;
                int? oldRec = null;

                var jobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == candidateRatingModel.JoId
                && x.CandProfId == candidateRatingModel.CandProfId).FirstOrDefaultAsync();
                if (jobCandidate != null)
                {
                    //var oldRecDtls = dbContext.PiHireUsers.Where(da => da.Id == candidate.CreatedBy && da.Status == (byte)RecordStatus.Active).FirstOrDefault();
                    //if (oldRecDtls == null)
                    //{
                    var candidateName = await dbContext.PhCandidateProfiles.Where(x => x.Id == candidateRatingModel.CandProfId).Select(x => x.CandName).FirstOrDefaultAsync();
                    oldRec = jobCandidate.RecruiterId;

                    jobCandidate.RecruiterId = candidateRatingModel.RecruiterId;
                    jobCandidate.IsTagged = true;
                    jobCandidate.UpdatedBy = UserId;
                    jobCandidate.UpdatedDate = CurrentTime;

                    if (oldRec != null)
                    {
                        var candidateRecHis = new PhCandidateRecruitersHistory()
                        {
                            CandProfId = candidateRatingModel.CandProfId,
                            Joid = candidateRatingModel.JoId,
                            Remarks = candidateRatingModel.Remarks,
                            RecruiterId = oldRec.Value,
                            CreatedBy = UserId
                        };
                        dbContext.PhCandidateRecruitersHistories.Add(candidateRecHis);

                        await dbContext.SaveChangesAsync();
                    }

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    //Audit
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Candidate Recruiter update",
                        ActivityDesc = " updated Recruiter for " + candidateName + "",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = candidateRatingModel.CandProfId,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    //Activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = candidateRatingModel.CandProfId,
                        JobId = candidateRatingModel.JoId,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has updated Recruiter for " + candidateName + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.SetResult(message);
                    respModel.Status = true;
                    //}
                    //else
                    //{
                    //    message = "This request could not be processed because candidate recruiter is in active";
                    //    respModel.Status = false;
                    //    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    //}
                }
                else
                {
                    message = "Candidate is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateRatingModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }



        public async Task<GetResponseViewModel<List<CandidateRecHistoryViewModel>>> GetCandidateRecHistory(int JoId, int CandProfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CandidateRecHistoryViewModel>>();
            var resp = new List<CandidateRecHistoryViewModel>();
            try
            {


                resp = await (from evaluation in dbContext.PhCandidateRecruitersHistories
                              join user in dbContext.PiHireUsers on evaluation.CreatedBy equals user.Id
                              join da in dbContext.PiHireUsers on evaluation.RecruiterId equals da.Id
                              where evaluation.Joid == JoId && evaluation.CandProfId == CandProfId
                              select new CandidateRecHistoryViewModel
                              {
                                  Remarks = evaluation.Remarks,
                                  CreatedDate = evaluation.CreatedDate,
                                  UpdatedBy = evaluation.CreatedBy,
                                  UpdatedName = user.FirstName + " " + user.LastName,
                                  RecruiterId = evaluation.RecruiterId,
                                  RecruiterName = da.FirstName + " " + da.LastName
                              }).ToListAsync();


                respModel.SetResult(resp);
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

        public async Task<GetResponseViewModel<CandidateCSATViewModel>> GetCandidateJobEvaluationSummary(int JoId, int CandProfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateCSATViewModel>();
            var response = new CandidateCSATViewModel
            {
                CandidateRatingModel = new List<CandidateRatingModel>()
            };
            try
            {


                var clientRating = await (from evaluation in dbContext.PhJobCandidateEvaluations
                                          join user in dbContext.PiHireUsers on evaluation.CreatedBy equals user.Id
                                          join da in dbContext.PhCandidateProfilesShareds on evaluation.RefId equals da.Id into ps
                                          from da in ps.DefaultIfEmpty()
                                          where evaluation.Joid == JoId && evaluation.CandProfId == CandProfId && evaluation.RatingType == (byte)ScheduledBy.Client
                                          select new CandidateRatingModel
                                          {
                                              CanPrfId = evaluation.CandProfId,
                                              CreatedDate = evaluation.CreatedDate,
                                              CreatedBy = evaluation.CreatedBy,
                                              JobId = evaluation.Joid,
                                              Rating = evaluation.Rating,
                                              Remarks = evaluation.Remakrs,
                                              CreatedByName = da.Clname,
                                              RatingType = evaluation.RatingType
                                          }).ToListAsync();
                if (clientRating.Count() > 0)
                {
                    response.CandidateRatingModel.AddRange(clientRating);
                }

                var teamRating = await (from evaluation in dbContext.PhJobCandidateEvaluations
                                        join user in dbContext.PiHireUsers on evaluation.CreatedBy equals user.Id
                                        where evaluation.Joid == JoId && evaluation.CandProfId == CandProfId && evaluation.RatingType == (byte)ScheduledBy.PiTeam
                                        select new CandidateRatingModel
                                        {
                                            CanPrfId = evaluation.CandProfId,
                                            CreatedByName = user.FirstName + " " + user.LastName,
                                            CreatedBy = evaluation.CreatedBy,
                                            CreatedDate = evaluation.CreatedDate,
                                            JobId = evaluation.Joid,
                                            Rating = evaluation.Rating,
                                            Remarks = evaluation.Remakrs,
                                            RatingType = evaluation.RatingType
                                        }).ToListAsync();

                if (teamRating.Count() > 0)
                {
                    response.CandidateRatingModel.AddRange(teamRating);
                }

                if (response.CandidateRatingModel != null)
                {
                    if (response.CandidateRatingModel.Count > 0)
                    {
                        int castOne = response.CandidateRatingModel.Where(x => x.Rating == 1).Count();
                        int castTwo = response.CandidateRatingModel.Where(x => x.Rating == 2).Count();
                        int castThree = response.CandidateRatingModel.Where(x => x.Rating == 3).Count();
                        int castFour = response.CandidateRatingModel.Where(x => x.Rating == 4).Count();
                        int castFive = response.CandidateRatingModel.Where(x => x.Rating == 5).Count();
                        int ttlCnt = response.CandidateRatingModel.Count();

                        decimal ragTtlCnt = (1 * castOne) + (2 * castTwo) + (3 * castThree) + (4 * castFour) + (5 * castFive);
                        decimal csatScre = (ragTtlCnt / (5 * ttlCnt)) * 5;

                        response.CSATScore = Math.Round(csatScre, 2, MidpointRounding.AwayFromZero);
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

        public async Task<GetResponseViewModel<CandidateCSATViewModel>> GetCandidateOverallEvaluationSummary(int CandProfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<CandidateCSATViewModel>();
            var response = new CandidateCSATViewModel
            {
                CandidateRatingModel = new List<CandidateRatingModel>()
            };
            try
            {


                var clientRating = await (from evaluation in dbContext.PhJobCandidateEvaluations
                                          join user in dbContext.PiHireUsers on evaluation.CreatedBy equals user.Id
                                          join da in dbContext.PhCandidateProfilesShareds on evaluation.RefId equals da.Id into ps
                                          from da in ps.DefaultIfEmpty()
                                          where evaluation.CandProfId == CandProfId && evaluation.RatingType == (byte)ScheduledBy.Client
                                          select new CandidateRatingModel
                                          {
                                              CanPrfId = evaluation.CandProfId,
                                              CreatedDate = evaluation.CreatedDate,
                                              CreatedBy = evaluation.CreatedBy,
                                              JobId = evaluation.Joid,
                                              Rating = evaluation.Rating,
                                              Remarks = evaluation.Remakrs,
                                              CreatedByName = da.Clname,
                                              RatingType = evaluation.RatingType
                                          }).ToListAsync();
                if (clientRating.Count() > 0)
                {
                    response.CandidateRatingModel.AddRange(clientRating);
                }

                var teamRating = await (from evaluation in dbContext.PhJobCandidateEvaluations
                                        join user in dbContext.PiHireUsers on evaluation.CreatedBy equals user.Id
                                        where evaluation.CandProfId == CandProfId && evaluation.RatingType == (byte)ScheduledBy.PiTeam
                                        select new CandidateRatingModel
                                        {
                                            CanPrfId = evaluation.CandProfId,
                                            CreatedByName = user.FirstName + " " + user.LastName,
                                            CreatedBy = evaluation.CreatedBy,
                                            CreatedDate = evaluation.CreatedDate,
                                            JobId = evaluation.Joid,
                                            Rating = evaluation.Rating,
                                            Remarks = evaluation.Remakrs,
                                            RatingType = evaluation.RatingType
                                        }).ToListAsync();

                if (teamRating.Count() > 0)
                {
                    response.CandidateRatingModel.AddRange(teamRating);
                }

                if (response.CandidateRatingModel != null)
                {
                    if (response.CandidateRatingModel.Count > 0)
                    {
                        int castOne = response.CandidateRatingModel.Where(x => x.Rating == 1).Count();
                        int castTwo = response.CandidateRatingModel.Where(x => x.Rating == 2).Count();
                        int castThree = response.CandidateRatingModel.Where(x => x.Rating == 3).Count();
                        int castFour = response.CandidateRatingModel.Where(x => x.Rating == 4).Count();
                        int castFive = response.CandidateRatingModel.Where(x => x.Rating == 5).Count();
                        int ttlCnt = response.CandidateRatingModel.Count();

                        decimal ragTtlCnt = (1 * castOne) + (2 * castTwo) + (3 * castThree) + (4 * castFour) + (5 * castFive);
                        decimal csatScre = (ragTtlCnt / (5 * ttlCnt)) * 5;

                        response.CSATScore = Math.Round(csatScre, 2, MidpointRounding.AwayFromZero);
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

        #endregion

        #region Un Subscribe

        public async Task<UpdateResponseViewModel<string>> Unsubscribe(CandidateUnSubscribeRequestModel candidateUnSubscribeRequestModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, " Start of method:", respModel.Meta.RequestID);
                SimpleEncrypt simpleEncrypt = new SimpleEncrypt();
                if (!string.IsNullOrEmpty(candidateUnSubscribeRequestModel.CandProfId))
                {
                    string respVal = System.Web.HttpUtility.UrlDecode(candidateUnSubscribeRequestModel.CandProfId);
                    var _CandId = simpleEncrypt.passwordDecrypt(respVal);
                    int CandId = Convert.ToInt32(_CandId);
                    var candidate = await dbContext.PhCandidateProfiles.Where(x => x.Id == CandId && x.EmailId == candidateUnSubscribeRequestModel.EmailId).FirstOrDefaultAsync();
                    if (candidate != null)
                    {
                        candidate.UpdatedDate = CurrentTime;
                        candidate.Status = (byte)RecordStatus.Unsubscribe;
                        dbContext.PhCandidateProfiles.Update(candidate);

                        await dbContext.SaveChangesAsync();

                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = " " + candidate?.CandName + " Unsubscribed Successfully.",
                            ActivityDesc = " " + candidate?.CandName + " Unsubscribed Successfully.",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = candidate.Id,
                            UserId = candidate.CreatedBy == null ? 0 : candidate.CreatedBy.Value
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        string Remarks = string.Empty;
                        if (!string.IsNullOrEmpty(candidateUnSubscribeRequestModel.Remarks))
                        {
                            Remarks = " Remarks : " + candidateUnSubscribeRequestModel.Remarks + "";
                        }

                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            JobId = 0,
                            ActivityMode = (byte)WorkflowActionMode.Candidate,
                            ActivityOn = candidate.Id,
                            ActivityType = (byte)LogActivityType.Other,
                            ActivityDesc = " " + candidate?.CandName + " Unsubscribed Successfully. " + Remarks + "",
                            UserId = candidate.CreatedBy == null ? 0 : candidate.CreatedBy.Value
                        };

                        activityList.Add(activityLog);
                        SaveActivity(activityList);


                        respModel.SetResult(message);
                        respModel.Status = true;

                    }
                    else
                    {
                        message = " Candidate is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                    }
                }
                else
                {
                    message = " Candidate id is mandatory";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateUnSubscribeRequestModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        #endregion

        #region  Currency Exchange

        public GetResponseViewModel<string> CurrConv(CurrConvViewModel currConvViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<string>();
            string responseRate = string.Empty;
            try
            {

                var curyExcRate = dbContext.PhCurrencyExchangeRates.Where(x => x.FromCurrency == currConvViewModel.FrmCurn && x.ToCurrency == currConvViewModel.ToCurn).FirstOrDefault();
                if (curyExcRate != null)
                {
                    if (curyExcRate.ExchangeRate > 0)
                    {
                        responseRate = curyExcRate.ExchangeRate.ToString();
                    }
                }
                respModel.SetResult(responseRate);
                respModel.Status = true;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(currConvViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        #endregion

        #region Resend User Cred Candidate

        public async Task<UpdateResponseViewModel<string>> ResendUserCred(ResendUserCredViewModel resendUserCredViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Resend Successfully";
            int UserId = Usr.Id;
            try
            {
                logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, " Start of method:", respModel.Meta.RequestID);

                var candidate = await dbContext.PhCandidateProfiles.Where(x => x.Id == resendUserCredViewModel.CandProfId).FirstOrDefaultAsync();
                if (candidate != null)
                {
                    string pswd = string.Empty;
                    var canUser = await dbContext.PiHireUsers.Where(x => x.UserName == candidate.EmailId && x.UserType == (byte)UserType.Candidate).FirstOrDefaultAsync();
                    if (canUser != null)
                    {
                        var generator = new RandomGenerator();
                        pswd = generator.RandomPassword(8);
                        var HashPswd = Hashification.SHA(pswd);
                        var JobCandidate = await dbContext.PhJobCandidates.Where(x => x.Joid == resendUserCredViewModel.JoId && x.CandProfId == resendUserCredViewModel.CandProfId).FirstOrDefaultAsync();
                        if (JobCandidate != null)
                        {
                            canUser.PasswordHash = HashPswd;
                            canUser.VerifiedFlag = true;
                            dbContext.PiHireUsers.Update(canUser);
                            await dbContext.SaveChangesAsync();

                            string redirectURL = appSettings.AppSettingsProperties.CandidateAppUrl + "/login?jobid=" + resendUserCredViewModel.JoId + "";
                            var mailBody = EmailTemplates.User_EmailCredentials_Template(candidate.CandName, candidate.EmailId, pswd,
                                redirectURL, this.appSettings.AppSettingsProperties.HireAppUrl, this.appSettings.AppSettingsProperties.HireApiUrl);

                            SmtpMailing smtp = new SmtpMailing(appSettings.smtpEmailConfig.SmtpAddress, appSettings.smtpEmailConfig.SmtpPort,
                            appSettings.smtpEmailConfig.SmtpLoginName, appSettings.smtpEmailConfig.SmtpLoginPassword, appSettings.smtpEmailConfig.SmtpEnableSsl,
                            appSettings.smtpEmailConfig.SmtpFromEmail, appSettings.smtpEmailConfig.SmtpFromName);
                            _ = smtp.SendMail(candidate.EmailId, EmailTemplates.GetSubject(EmailTypes.AppUser_Create), mailBody, string.Empty);


                            var activityList = new List<CreateActivityViewModel>();
                            var audList = new List<CreateAuditViewModel>();

                            //Audit
                            var auditLog = new CreateAuditViewModel
                            {
                                ActivitySubject = " Resend Candidate Credentails",
                                ActivityDesc = " Sent new credentials for " + candidate.CandName + " ",
                                ActivityType = (byte)AuditActivityType.Authentication,
                                TaskID = candidate.Id,
                                UserId = UserId
                            };
                            audList.Add(auditLog);
                            SaveAuditLog(audList);

                            //Activity
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Candidate,
                                ActivityOn = resendUserCredViewModel.CandProfId,
                                JobId = resendUserCredViewModel.JoId,
                                ActivityType = (byte)LogActivityType.Critical,
                                ActivityDesc = " Sent new credentials for " + candidate.CandName + "",
                                UserId = UserId
                            };
                            activityList.Add(activityLog);
                            SaveActivity(activityList);


                            respModel.SetResult(message);
                            respModel.Status = true;
                        }
                        else
                        {
                            message = "Candidate is not associated with this job";
                            respModel.Status = false;
                            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                        }
                    }
                    else
                    {
                        message = "Candidate is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                    }
                }
                else
                {
                    message = "Candidate is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(resendUserCredViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        #endregion
    }
}


