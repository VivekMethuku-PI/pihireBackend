using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Repositories
{
    public class OpeningRepository : BaseRepository, IOpeningRepository
    {
        readonly Logger logger;
        public OpeningRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<OpeningRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> CreateJobsync(CreateOpeningViewModel createOpeningViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = " Created Successfully";
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    var JobOpeningStatusId = await dbContext.PhJobStatusSes.Where(da => da.Jscode == "NEW").Select(da => da.Id).FirstOrDefaultAsync();
                    if (createOpeningViewModel.BroughtBy == null || createOpeningViewModel.BroughtBy == 0)
                    {
                        if (Usr.UserTypeId == (byte)UserType.BDM)
                        {
                            createOpeningViewModel.BroughtBy = UserId;
                        }
                    }

                    var dbModel_jobOpening = new PhJobOpening
                    {
                        BroughtBy = createOpeningViewModel.BroughtBy,

                        ClientId = createOpeningViewModel.ClientId,
                        ClientName = createOpeningViewModel.ClientName,

                        JobTitle = createOpeningViewModel.JobRole,//createOpeningViewModel.JobTitle,
                        JobRole = createOpeningViewModel.JobRole,
                        ShortJobDesc = createOpeningViewModel.ShortJobDesc,
                        JobDescription = createOpeningViewModel.JobDescription,

                        CountryId = createOpeningViewModel.CountryId,
                        JobLocationId = createOpeningViewModel.CityId,
                        JobLocationLocal = createOpeningViewModel.LocalAnyWhere,

                        PostedDate = createOpeningViewModel.StartDate,
                        ClosedDate = createOpeningViewModel.ClosedDate,

                        JobCategoryId = createOpeningViewModel.JobCategoryId,
                        JobCategory = createOpeningViewModel.JobCategory,

                        Priority = createOpeningViewModel.Priority,
                        NoOfPositions = createOpeningViewModel.NoOfPositions,
                        MaxExpeInMonths = ConvertMonths(createOpeningViewModel.MaxYears?.Value),
                        ExpeInMonthsPrefTyp = ToValue(createOpeningViewModel?.MaxYears?.PreferenceType),
                        MinExpeInMonths = ConvertMonths(createOpeningViewModel.MinYears?.Value),
                        MaxReleventExpInMonths = ConvertMonths(createOpeningViewModel.ReleventExpMaxYears?.Value),
                        ReleventExpInMonthsPrefTyp = ToValue(createOpeningViewModel?.ReleventExpMaxYears?.PreferenceType),
                        MinReleventExpInMonths = ConvertMonths(createOpeningViewModel.ReleventExpMinYears?.Value),

                        JobOpeningStatus = JobOpeningStatusId,
                        Status = (byte)RecordStatus.Active,
                        CreatedDate = CurrentTime,
                        KeyRequirements = string.Empty,
                        ExpeInMonths = 0,// not using
                        ExpeInYears = 0, // not using
                        CreatedByName = string.Empty,
                        CreatedBy = UserId,
                    };
                    dbContext.PhJobOpenings.Add(dbModel_jobOpening);
                    await dbContext.SaveChangesAsync();

                    var dbModel_jobOpeningAddlDtls = new PhJobOpeningsAddlDetail
                    {
                        Joid = dbModel_jobOpening.Id,
                        Puid = createOpeningViewModel.Puid,
                        Buid = createOpeningViewModel.Buid,

                        ApprJoinDate = createOpeningViewModel.ApprJoinDate,

                        Spocid = createOpeningViewModel.Spocid,
                        ClientReviewFlag = createOpeningViewModel.ClientReviewFlag,
                        ClientBilling = createOpeningViewModel.ClientBilling,
                        CurrencyId = createOpeningViewModel.CurrencyId,
                        MinSalary = createOpeningViewModel.MinSalary,
                        MaxSalary = createOpeningViewModel.MaxSalary,

                        JobTenure = createOpeningViewModel.JobTenure,
                        JobWorkPattern = createOpeningViewModel?.JobWorkPattern?.Value,
                        JobWorkPatternPrefTyp = ToValue(createOpeningViewModel?.JobWorkPattern?.PreferenceType),
                        NoOfCvsRequired = createOpeningViewModel.NoOfCvsRequired,
                        NoticePeriod = createOpeningViewModel.NoticePeriod?.Value,
                        NoticePeriodPrefTyp = ToValue(createOpeningViewModel?.NoticePeriod?.PreferenceType),

                        ReceivedDate = CurrentTime,
                        SalaryPackage = string.Empty,
                        SalaryRemarks = string.Empty,
                        NoOfCvsFilled = 0,
                        NoOfCvsToBeFilled = 0,
                        AccessToAll = false,
                        AddlComments = string.Empty,
                        AnnualSalary = null,
                        CandPrefLcation = 0,
                        CrmoppoId = 0,
                        CvAsPdf = false,
                        CvWithLogo = false,
                    };
                    dbContext.PhJobOpeningsAddlDetails.Add(dbModel_jobOpeningAddlDtls);
                    await dbContext.SaveChangesAsync();

                    dbContext.PhJobOpeningsDesirables.Add(new PhJobOpeningsDesirables
                    {
                        Joid = dbModel_jobOpening.Id,

                        JobDomain = createOpeningViewModel.JobDesirableDomain?.Value,
                        JobDomainPrefType = ToValue(createOpeningViewModel.JobDesirableDomain?.PreferenceType),

                        JobTeamRole = createOpeningViewModel.JobDesirableTeamRole?.Value,
                        JobTeamRolePrefType = ToValue(createOpeningViewModel.JobDesirableTeamRole?.PreferenceType),

                        CandidateValidPassport = createOpeningViewModel.CandidatePrefValidPassport?.Value,
                        CandidateValidPassportPrefType = ToValue(createOpeningViewModel.CandidatePrefValidPassport?.PreferenceType),

                        CandidateDOB = createOpeningViewModel.CandidatePrefDOB?.Value,
                        CandidateDOBPrefType = ToValue(createOpeningViewModel.CandidatePrefDOB?.PreferenceType),

                        CandidateGender = createOpeningViewModel.CandidatePrefGender?.Value,
                        CandidateGenderPrefType = ToValue(createOpeningViewModel.CandidatePrefGender?.PreferenceType),

                        CandidateMaritalStatus = createOpeningViewModel.CandidatePrefMaritalStatus?.Value,
                        CandidateMaritalStatusPrefType = ToValue(createOpeningViewModel.CandidatePrefMaritalStatus?.PreferenceType),

                        CandidateLanguage = createOpeningViewModel.CandidatePrefLanguage?.Value,
                        CandidateLanguagePrefType = ToValue(createOpeningViewModel.CandidatePrefLanguage?.PreferenceType),

                        CandidateVisaPreference = createOpeningViewModel.CandidatePrefVisaPreference?.Value,
                        CandidateVisaPreferencePrefType = ToValue(createOpeningViewModel.CandidatePrefVisaPreference?.PreferenceType),

                        CandidateRegion = createOpeningViewModel.CandidatePrefRegion?.Value,
                        CandidateRegionPrefType = ToValue(createOpeningViewModel.CandidatePrefRegion?.PreferenceType),

                        CandidateNationality = createOpeningViewModel.CandidatePrefNationality?.Value,
                        CandidateNationalityPrefType = ToValue(createOpeningViewModel.CandidatePrefNationality?.PreferenceType),

                        CandidateResidingCountry = createOpeningViewModel.CandidatePrefResidingCountry?.Value,
                        CandidateResidingCountryPrefType = ToValue(createOpeningViewModel.CandidatePrefResidingCountry?.PreferenceType),

                        CandidateResidingCity = createOpeningViewModel.CandidatePrefResidingCity?.Value,
                        CandidateResidingCityPrefType = ToValue(createOpeningViewModel.CandidatePrefResidingCity?.PreferenceType),

                        CandidateDrivingLicence = createOpeningViewModel.CandidatePrefDrivingLicence?.Value,
                        CandidateDrivingLicencePrefType = ToValue(createOpeningViewModel.CandidatePrefDrivingLicence?.PreferenceType),

                        CandidateEmployeeStatus = createOpeningViewModel.CandidatePrefEmployeeStatus?.Value,
                        CandidateEmployeeStatusPrefType = ToValue(createOpeningViewModel.CandidatePrefEmployeeStatus?.PreferenceType),

                        CandidateResume = createOpeningViewModel.CandidatePrefResume?.Value,
                        CandidateResumePrefType = ToValue(createOpeningViewModel.CandidatePrefResume?.PreferenceType),

                        CandidateVidPrfl = createOpeningViewModel.CandidatePrefVidPrfl?.Value,
                        CandidateVidPrflPrefType = ToValue(createOpeningViewModel.CandidatePrefVidPrfl?.PreferenceType),

                        CandidatePaySlp = createOpeningViewModel.CandidatePrefPaySlp?.Value,
                        CandidatePaySlpPrefType = ToValue(createOpeningViewModel.CandidatePrefPaySlp?.PreferenceType),

                        CandidateNoticePeriod = createOpeningViewModel.CandidatePrefNoticePeriod?.Value,
                        CandidateNoticePeriodPrefType = ToValue(createOpeningViewModel.CandidatePrefNoticePeriod?.PreferenceType),
                    });
                    if (createOpeningViewModel.JobDesirableSpecializations?.Count > 0)
                    {
                        foreach (var TechnologyId in createOpeningViewModel.JobDesirableSpecializations.Select(da => da.TechnologyId).Distinct())
                        {
                            var techData = createOpeningViewModel.JobDesirableSpecializations.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                            dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobDesirableSkillGroupTypes.Specializations,

                                TechnologyId = techData.TechnologyId,
                                ExpYears = techData.ExpYears,
                                PreferenceType = ToValue(techData.PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            });
                        }
                    }
                    if (createOpeningViewModel.JobDesirableImplementations?.Count > 0)
                    {
                        foreach (var TechnologyId in createOpeningViewModel.JobDesirableImplementations.Select(da => da.TechnologyId).Distinct())
                        {
                            var techData = createOpeningViewModel.JobDesirableImplementations.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                            dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobDesirableSkillGroupTypes.Implementations,

                                TechnologyId = techData.TechnologyId,
                                ExpYears = techData.ExpYears,
                                PreferenceType = ToValue(techData.PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            });
                        }
                    }
                    if (createOpeningViewModel.JobDesirableDesigns?.Count > 0)
                    {
                        foreach (var TechnologyId in createOpeningViewModel.JobDesirableDesigns.Select(da => da.TechnologyId).Distinct())
                        {
                            var techData = createOpeningViewModel.JobDesirableDesigns.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                            dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobDesirableSkillGroupTypes.Designs,

                                TechnologyId = techData.TechnologyId,
                                ExpYears = techData.ExpYears,
                                PreferenceType = ToValue(techData.PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            });
                        }
                    }
                    if (createOpeningViewModel.JobDesirableDevelopments?.Count > 0)
                    {
                        foreach (var TechnologyId in createOpeningViewModel.JobDesirableDevelopments.Select(da => da.TechnologyId).Distinct())
                        {
                            var techData = createOpeningViewModel.JobDesirableDevelopments.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                            dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobDesirableSkillGroupTypes.Developments,

                                TechnologyId = techData.TechnologyId,
                                ExpYears = techData.ExpYears,
                                PreferenceType = ToValue(techData.PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            });
                        }
                    }
                    if (createOpeningViewModel.JobDesirableSupports?.Count > 0)
                    {
                        foreach (var TechnologyId in createOpeningViewModel.JobDesirableSupports.Select(da => da.TechnologyId).Distinct())
                        {
                            var techData = createOpeningViewModel.JobDesirableSupports.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                            dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobDesirableSkillGroupTypes.Supports,

                                TechnologyId = techData.TechnologyId,
                                ExpYears = techData.ExpYears,
                                PreferenceType = ToValue(techData.PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            });
                        }
                    }
                    if (createOpeningViewModel.JobDesirableQualities?.Count > 0)
                    {
                        foreach (var TechnologyId in createOpeningViewModel.JobDesirableQualities.Select(da => da.TechnologyId).Distinct())
                        {
                            var techData = createOpeningViewModel.JobDesirableQualities.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                            dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobDesirableSkillGroupTypes.Qualities,

                                TechnologyId = techData.TechnologyId,
                                ExpYears = techData.ExpYears,
                                PreferenceType = ToValue(techData.PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            });
                        }
                    }
                    if (createOpeningViewModel.JobDesirableDocumentations?.Count > 0)
                    {
                        foreach (var TechnologyId in createOpeningViewModel.JobDesirableDocumentations.Select(da => da.TechnologyId).Distinct())
                        {
                            var techData = createOpeningViewModel.JobDesirableDocumentations.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                            dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobDesirableSkillGroupTypes.Documentations,

                                TechnologyId = techData.TechnologyId,
                                ExpYears = techData.ExpYears,
                                PreferenceType = ToValue(techData.PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            });
                        }
                    }

                    //Skill set 
                    if (createOpeningViewModel.OpeningSkillSet?.Count > 0)
                    {
                        foreach (var TechnologyId in createOpeningViewModel.OpeningSkillSet.Select(da => da.TechnologyId).Distinct())
                        {
                            var item = createOpeningViewModel.OpeningSkillSet.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                            {
                                var TotalExpeInMonths = item.ExpMonth + ConvertMonths(item.ExpYears);
                                dbContext.PhJobOpeningSkills.Add(new PhJobOpeningSkill
                                {
                                    Joid = dbModel_jobOpening.Id,

                                    TechnologyId = item.TechnologyId,
                                    Technology = item.Technology,
                                    ExpYears = item.ExpYears,
                                    ExpMonth = item.ExpMonth,
                                    TotalExpeInMonths = TotalExpeInMonths,

                                    PreferenceType = ToValue(item.PreferenceType),

                                    Status = (byte)RecordStatus.Active,
                                    CreatedBy = dbModel_jobOpening.CreatedBy,
                                    CreatedDate = CurrentTime,
                                });
                            }
                        }
                        await dbContext.SaveChangesAsync();
                    }
                    //Candidate Education Qualification
                    if (createOpeningViewModel.CandidateEducationQualifications?.Count > 0)
                    {
                        foreach (var data in createOpeningViewModel.CandidateEducationQualifications.Select(da => new { da.Qualification, da.Course }).Distinct())
                        {
                            dbContext.PhJobOpeningsQualifications.Add(new PhJobOpeningsQualification
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobCandQualCertGroupTypes.JobCandiateEducationQualification,

                                QualificationId = data.Qualification,
                                CourseId = data.Course,
                                PreferenceType = ToValue(createOpeningViewModel.CandidateEducationQualifications.FirstOrDefault(da => da.Qualification == data.Qualification && da.Course == data.Course).PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            });
                        }
                        await dbContext.SaveChangesAsync();
                    }
                    //Candidate Education Certification
                    if (createOpeningViewModel.CandidateEducationCertifications?.Count > 0)
                    {
                        foreach (var data in createOpeningViewModel.CandidateEducationCertifications.Select(da => new { da.Value }).Distinct())
                        {
                            dbContext.PhJobOpeningsCertifications.Add(new PhJobOpeningsCertification
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobCandQualCertGroupTypes.JobCandidateEducationCertifications,

                                CertificationId = data.Value,
                                PreferenceType = ToValue(createOpeningViewModel.CandidateEducationCertifications.FirstOrDefault(da => da.Value == data.Value).PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            });
                        }
                        await dbContext.SaveChangesAsync();
                    }
                    //Opening Qualification
                    if (createOpeningViewModel.OpeningQualifications?.Count > 0)
                    {
                        foreach (var data in createOpeningViewModel.OpeningQualifications.Select(da => new { da.Value }).Distinct())
                        {
                            var phJobOpeningSkills = new PhJobOpeningsQualification
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobCandQualCertGroupTypes.JobOpeningQualification,

                                QualificationId = data.Value,
                                PreferenceType = ToValue(createOpeningViewModel.OpeningQualifications.FirstOrDefault(da => da.Value == data.Value).PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            };
                            dbContext.PhJobOpeningsQualifications.Add(phJobOpeningSkills);
                        }
                        await dbContext.SaveChangesAsync();
                    }
                    //Opening Certification
                    if (createOpeningViewModel.OpeningCertifications?.Count > 0)
                    {
                        foreach (var data in createOpeningViewModel.OpeningCertifications.Select(da => new { da.Value }).Distinct())
                        {
                            var phJobOpeningSkills = new PhJobOpeningsCertification
                            {
                                Joid = dbModel_jobOpening.Id,
                                GroupType = (byte)JobCandQualCertGroupTypes.JobOpeningCertification,

                                CertificationId = data.Value,
                                PreferenceType = ToValue(createOpeningViewModel.OpeningCertifications.FirstOrDefault(da => da.Value == data.Value).PreferenceType),

                                Status = (byte)RecordStatus.Active,
                                CreatedBy = dbModel_jobOpening.CreatedBy,
                                CreatedDate = CurrentTime,
                            };
                            dbContext.PhJobOpeningsCertifications.Add(phJobOpeningSkills);
                        }
                        await dbContext.SaveChangesAsync();
                    }

                    //// Preferences 
                    //if (createOpeningViewModel.OpeningPREFViewModel.Count > 0)
                    //{
                    //    foreach (var OpeningAssessment in createOpeningViewModel.OpeningPREFViewModel)
                    //    {
                    //        var phJobOpeningPref = new PhJobOpeningPref
                    //        {
                    //            CreatedBy = UserId,
                    //            CreatedDate = CurrentTime,
                    //            DisplayFlag = OpeningAssessment.DisplayFlag,
                    //            FieldCode = OpeningAssessment.FieldCode,
                    //            Joid = dbModel_jobOpening.Id,
                    //            Status = (byte)RecordStatus.Active
                    //        };
                    //        dbContext.PhJobOpeningPrefs.Add(phJobOpeningPref);
                    //        await dbContext.SaveChangesAsync();
                    //    }
                    //}

                    // Assessments
                    if (createOpeningViewModel.OpeningAssessments != null)
                    {
                        if (createOpeningViewModel.OpeningAssessments.Count > 0)
                        {
                            //int AssessmentCounter = createOpeningViewModel.OpeningAssessments.Count;
                            foreach (var CandStatusId in createOpeningViewModel.OpeningAssessments.Select(da => da.CandStatusId).Distinct())
                            {
                                var OpeningAssessment = createOpeningViewModel.OpeningAssessments.FirstOrDefault(da => da.CandStatusId == CandStatusId);
                                var phJobOpeningAssmts = new PhJobOpeningAssmt
                                {
                                    CandStatusId = OpeningAssessment.CandStatusId,
                                    Status = (byte)RecordStatus.Active,
                                    AssessmentId = OpeningAssessment.AssessmentId,
                                    CreatedBy = UserId,
                                    CreatedDate = CurrentTime,
                                    Joid = dbModel_jobOpening.Id,
                                    StageId = OpeningAssessment.StageId
                                };
                                dbContext.PhJobOpeningAssmts.Add(phJobOpeningAssmts);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }

                    //// Questions
                    //if (createOpeningViewModel.OpeningQtns != null)
                    //{
                    //    if (createOpeningViewModel.OpeningQtns.Count > 0)
                    //    {
                    //        foreach (var OpeningAssessment in createOpeningViewModel.OpeningQtns)
                    //        {
                    //            var phJobOpeningStQtns = new PhJobOpeningStQtn
                    //            {
                    //                CreatedBy = UserId,
                    //                CreatedDate = CurrentTime,
                    //                Joid = dbModel_jobOpening.Id,
                    //                Status = (byte)RecordStatus.Active,
                    //                IsMandatory = false,
                    //                QuestionSlno = OpeningAssessment.Slno,
                    //                QuestionText = OpeningAssessment.QuestionText,
                    //                QuestionType = OpeningAssessment.QuestionType
                    //            };
                    //            dbContext.PhJobOpeningStQtns.Add(phJobOpeningStQtns);
                    //            await dbContext.SaveChangesAsync();
                    //        }
                    //    }
                    //}

                    var PhJobOpeningActvCounter = new PhJobOpeningActvCounter
                    {
                        AsmtCounter = 0,
                        ClientViewsCounter = 0,
                        EmailsCounter = 0,
                        JobPostingCounter = 0,
                        Joid = dbModel_jobOpening.Id,
                        Status = (byte)RecordStatus.Active
                    };
                    dbContext.PhJobOpeningActvCounters.Add(PhJobOpeningActvCounter);
                    await dbContext.SaveChangesAsync();

                    // Auto assignment
                    //if (createOpeningViewModel.JobCategory != "Internal")
                    //{
                    //    var AutoAssignmentSearchViewModel = new AutoAssignmentSearchViewModel
                    //    {
                    //        Enddate = createOpeningViewModel.ClosedDate,
                    //        StartDate = createOpeningViewModel.StartDate,
                    //        Puid = createOpeningViewModel.Puid.Value,
                    //        OfficeId = createOpeningViewModel.OfficeId,
                    //        CreatedBy = loginUserId,
                    //        Joid = dbModel_jobOpening.Id,
                    //        NoOfCvsRequired = createOpeningViewModel.NoOfCvsRequired.Value
                    //    };
                    //    var autoAssgnmt = await JobAutoAssignment(AutoAssignmentSearchViewModel);
                    //}

                    // Activity
                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        ActivityOn = dbModel_jobOpening.Id,
                        JobId = dbModel_jobOpening.Id,
                        ActivityType = (byte)LogActivityType.JobEditUpdates,
                        ActivityDesc = " has Created the " + dbModel_jobOpening.JobTitle + " Job",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    // Applying work flow conditions 
                    //var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    //{
                    //    TaskCode = TaskCode.CJB.ToString(),
                    //    CanProfId = null,
                    //    JobId = dbModel_jobOpening.Id,
                    //    loginUserId = loginUserId
                    //};
                    //if (createOpeningViewModel.JobCategory != "Internal")
                    //{
                    //    workFlowRuleSearchViewModel.ActionMode = (byte)WorkflowActionMode.Opening;
                    //    workFlowRuleSearchViewModel.CurrentStatusId = JobOpeningStatusId;
                    //}
                    //else
                    //{
                    //    workFlowRuleSearchViewModel.ActionMode = (byte)WorkflowActionMode.Other;
                    //    workFlowRuleSearchViewModel.CurrentStatusId = null;
                    //}

                    //var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
                    //if (wfResp.Status && wfResp.isNotification)
                    //{
                    //    foreach (var OpeningAssessment in wfResp.WFNotifications)
                    //    {
                    //        var notificationPushed = new NotificationPushedViewModel
                    //        {
                    //            JobId = wfResp.JoId,
                    //            PushedTo = OpeningAssessment.UserIds,
                    //            NoteDesc = OpeningAssessment.NoteDesc,
                    //            Title = OpeningAssessment.Title,
                    //            CreatedBy = loginUserId
                    //        };
                    //        notificationPushedViewModel.Add(notificationPushed);
                    //    }
                    //}

                    #region CRM                                  
                    {
                        using var client = new HttpClientService();
                        var reqDtls = new CRM_createJobOpeningViewModel
                        {
                            account_id = dbModel_jobOpening.ClientId,
                            spocId = dbModel_jobOpeningAddlDtls.Spocid ?? 0,
                            business_Unit = dbModel_jobOpeningAddlDtls.Buid,
                            process_Unit = dbModel_jobOpeningAddlDtls.Puid,
                            negotiated_price = dbModel_jobOpeningAddlDtls.ClientBilling,
                            opportunity_name = dbModel_jobOpening.JobTitle,
                            quantity = dbModel_jobOpening.NoOfPositions,
                            opp_start_date = dbModel_jobOpening.PostedDate,
                            opp_end_date = dbModel_jobOpening.ClosedDate
                        };

                        logger.Log(LogLevel.Debug, LoggingEvents.InsertItem, ", Saving Opening as Opportunity model:" + Newtonsoft.Json.JsonConvert.SerializeObject(reqDtls), respModel.Meta.RequestID);

                        var response = await client.PostAsync(appSettings.AppSettingsProperties.CRMUrl, "/api/Opportunity/Hire/CreateOpportunity", reqDtls);
                    }
                    #endregion

                    respModel.SetResult(message);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(createOpeningViewModel), respModel.Meta.RequestID, ex);

                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    respModel.Result = null;
                    trans.Rollback();
                }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }
        public async Task<UpdateResponseViewModel<string>> EditJob(EditOpeningViewModel editOpeningViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UpdatedBy = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
            using (var trans = await dbContext.Database.BeginTransactionAsync())
                try
                {
                    var dbModel_jobOpening = await dbContext.PhJobOpenings.FirstOrDefaultAsync(da => da.Id == editOpeningViewModel.Id && da.Status != (byte)RecordStatus.Delete);
                    if (dbModel_jobOpening != null)
                    {
                        //List<int> GroupIds = new List<int> { 34, 59, 13, 185 };
                        //var refData = dbContext.PhRefMasters.Where(da => GroupIds.Contains(da.GroupId)).ToList();

                        dbModel_jobOpening.UpdatedBy = UpdatedBy;
                        dbModel_jobOpening.UpdatedDate = CurrentTime;

                        dbModel_jobOpening.JobDescription = editOpeningViewModel.JobDescription;
                        dbModel_jobOpening.PostedDate = editOpeningViewModel.StartDate;
                        dbModel_jobOpening.JobTitle = //editOpeningViewModel.JobTitle;
                        dbModel_jobOpening.JobRole = editOpeningViewModel.JobRole;

                        dbModel_jobOpening.CountryId = editOpeningViewModel.CountryId;
                        dbModel_jobOpening.JobLocationId = editOpeningViewModel.CityId;
                        dbModel_jobOpening.JobLocationLocal = editOpeningViewModel.LocalAnyWhere;

                        if (dbModel_jobOpening.NoOfPositions != editOpeningViewModel.NoOfPositions)
                        {
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated No Of Positions from " + dbModel_jobOpening.NoOfPositions + " to " + editOpeningViewModel.NoOfPositions + "",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.NoOfPositions = editOpeningViewModel.NoOfPositions;
                        }
                        if (dbModel_jobOpening.Priority != editOpeningViewModel.Priority)
                        {
                            string currentValue = await dbContext.PhRefMasters.Where(da => da.Id == dbModel_jobOpening.Priority).Select(da => da.Rmvalue).FirstOrDefaultAsync();
                            string newValue = await dbContext.PhRefMasters.Where(da => da.Id == editOpeningViewModel.Priority).Select(da => da.Rmvalue).FirstOrDefaultAsync();
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated Job Priority from " + currentValue + " to " + newValue + "",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.Priority = editOpeningViewModel.Priority;
                        }
                        if (dbModel_jobOpening.JobCategory != editOpeningViewModel.JobCategory)
                        {
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated Job Category from " + dbModel_jobOpening.JobCategory + " to " + editOpeningViewModel.JobCategory + "",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.JobCategory = editOpeningViewModel.JobCategory;
                        }
                        if (dbModel_jobOpening.ClosedDate.Date != editOpeningViewModel.ClosedDate.Date)
                        {
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated Closed Date from " + dbModel_jobOpening.ClosedDate.Date + " to " + editOpeningViewModel.ClosedDate.Date + "",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.ClosedDate = editOpeningViewModel.ClosedDate;
                        }
                        if (dbModel_jobOpening.ShortJobDesc != editOpeningViewModel.ShortJobDesc)
                        {
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated Short Job Desc from " + dbModel_jobOpening.ShortJobDesc + " to " + editOpeningViewModel.ShortJobDesc + "",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.ShortJobDesc = editOpeningViewModel.ShortJobDesc;
                        }
                        //if (editOpeningViewModel.BroughtBy != null)
                        //{
                        //    dbModel_jobOpening.BroughtBy = editOpeningViewModel.BroughtBy;
                        //}

                        var MaxExpeInMonths = ConvertMonths(editOpeningViewModel.MaxYears?.Value);
                        if (dbModel_jobOpening.MaxExpeInMonths != MaxExpeInMonths)
                        {
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated Job Max Experience from " + dbModel_jobOpening.MaxExpeInMonths + " month's to " + MaxExpeInMonths + " month's ",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.MaxExpeInMonths = MaxExpeInMonths;
                        }
                        var MinExpeInMonths = ConvertMonths(editOpeningViewModel.MinYears?.Value);
                        if (dbModel_jobOpening.MinExpeInMonths != MinExpeInMonths)
                        {
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated Job Min Experience from " + dbModel_jobOpening.MinExpeInMonths + " month's to " + MinExpeInMonths + " month's",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.MinExpeInMonths = MinExpeInMonths;
                        }
                        if (dbModel_jobOpening.ExpeInMonthsPrefTyp != ToValue(editOpeningViewModel?.MaxYears?.PreferenceType))
                        {
                            var oldValue = ToModel(dbModel_jobOpening.ExpeInMonthsPrefTyp);
                            var newValue = editOpeningViewModel?.MaxYears?.PreferenceType;

                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated Job Experience Preference Type from " + oldValue + " to " + newValue + "",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.ExpeInMonthsPrefTyp = ToValue(editOpeningViewModel?.MaxYears?.PreferenceType);
                        }

                        var MaxReleventExpInMonths = ConvertMonths(editOpeningViewModel.ReleventExpMaxYears?.Value);
                        if (dbModel_jobOpening.MaxReleventExpInMonths != MaxReleventExpInMonths)
                        {
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated Job Relevant Max Experience from " + dbModel_jobOpening.MaxReleventExpInMonths + " month's to " + MaxReleventExpInMonths + " month's ",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.MaxReleventExpInMonths = MaxReleventExpInMonths;
                        }
                        var MinReleventExpInMonths = ConvertMonths(editOpeningViewModel.ReleventExpMinYears?.Value);
                        if (dbModel_jobOpening.MinReleventExpInMonths != MinReleventExpInMonths)
                        {
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated Job Relevant Min Experience from " + dbModel_jobOpening.MinReleventExpInMonths + " month's to " + MinReleventExpInMonths + " month's",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.MinReleventExpInMonths = MinReleventExpInMonths;
                        }
                        if (dbModel_jobOpening.ReleventExpInMonthsPrefTyp != ToValue(editOpeningViewModel?.ReleventExpMaxYears?.PreferenceType))
                        {
                            var oldValue = ToModel(dbModel_jobOpening.ReleventExpInMonthsPrefTyp);
                            var newValue = editOpeningViewModel?.ReleventExpMaxYears?.PreferenceType;

                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = dbModel_jobOpening.Id,
                                JobId = dbModel_jobOpening.Id,
                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                ActivityDesc = " has updated Job Relevant Experience Preference Type from " + oldValue + " to " + newValue + "",
                                UserId = UpdatedBy
                            };
                            activityList.Add(activityLog);
                            dbModel_jobOpening.ReleventExpInMonthsPrefTyp = ToValue(editOpeningViewModel?.ReleventExpMaxYears?.PreferenceType);
                        }
                        dbContext.PhJobOpenings.Update(dbModel_jobOpening);

                        var dbModel_jobOpeningAddlDtls = await dbContext.PhJobOpeningsAddlDetails.FirstOrDefaultAsync(da => da.Joid == editOpeningViewModel.Id);
                        if (dbModel_jobOpeningAddlDtls == null)
                        {
                            dbModel_jobOpeningAddlDtls = new PhJobOpeningsAddlDetail
                            {
                                Joid = dbModel_jobOpening.Id,
                                Puid = editOpeningViewModel.Puid,
                                Buid = editOpeningViewModel.Buid,

                                ApprJoinDate = editOpeningViewModel.ApprJoinDate,

                                Spocid = editOpeningViewModel.Spocid,
                                ClientReviewFlag = editOpeningViewModel.ClientReviewFlag,
                                ClientBilling = editOpeningViewModel.ClientBilling,
                                CurrencyId = editOpeningViewModel.CurrencyId,
                                MinSalary = editOpeningViewModel.MinSalary,
                                MaxSalary = editOpeningViewModel.MaxSalary,

                                JobTenure = editOpeningViewModel.JobTenure,
                                JobWorkPattern = editOpeningViewModel?.JobWorkPattern?.Value,
                                JobWorkPatternPrefTyp = ToValue(editOpeningViewModel?.JobWorkPattern?.PreferenceType),
                                NoOfCvsRequired = editOpeningViewModel.NoOfCvsRequired,
                                NoticePeriod = editOpeningViewModel.NoticePeriod?.Value,
                                NoticePeriodPrefTyp = ToValue(editOpeningViewModel?.NoticePeriod?.PreferenceType),

                                ReceivedDate = CurrentTime,
                                SalaryPackage = string.Empty,
                                SalaryRemarks = string.Empty,
                                NoOfCvsFilled = 0,
                                NoOfCvsToBeFilled = 0,
                                AccessToAll = false,
                                AddlComments = string.Empty,
                                AnnualSalary = null,
                                CandPrefLcation = 0,
                                CrmoppoId = 0,
                                CvAsPdf = false,
                                CvWithLogo = false,
                            };
                            dbContext.PhJobOpeningsAddlDetails.Add(dbModel_jobOpeningAddlDtls);
                        }
                        else
                        {
                            if (editOpeningViewModel.Spocid != editOpeningViewModel.Spocid)
                            {
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Account manager name from " + editOpeningViewModel.CurrentSpocName + " to " + editOpeningViewModel.NewSpocName + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.Spocid = editOpeningViewModel.Spocid;
                            }
                            if (dbModel_jobOpeningAddlDtls.ApprJoinDate.Value.Date != editOpeningViewModel.ApprJoinDate.Value.Date)
                            {
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Tentative Joining Date from " + dbModel_jobOpeningAddlDtls.ApprJoinDate.Value.Date + " to " + editOpeningViewModel.ApprJoinDate.Value.Date + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.ApprJoinDate = editOpeningViewModel.ApprJoinDate;
                            }

                            if (dbModel_jobOpeningAddlDtls.ClientBilling != editOpeningViewModel.ClientBilling)
                            {
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Client Billing Amount from " + dbModel_jobOpeningAddlDtls.ClientBilling + " to " + editOpeningViewModel.ClientBilling + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.ClientBilling = editOpeningViewModel.ClientBilling;
                            }
                            if (dbModel_jobOpeningAddlDtls.ClientReviewFlag != editOpeningViewModel.ClientReviewFlag)
                            {
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Client Review Flag from " + dbModel_jobOpeningAddlDtls.ClientReviewFlag + " to " + editOpeningViewModel.ClientReviewFlag + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.ClientReviewFlag = editOpeningViewModel.ClientReviewFlag;
                            }
                            if (dbModel_jobOpeningAddlDtls.CurrencyId != editOpeningViewModel.CurrencyId)
                            {
                                string currentValue = await dbContext.PhRefMasters.Where(da => da.Id == dbModel_jobOpeningAddlDtls.CurrencyId).Select(da => da.Rmvalue).FirstOrDefaultAsync();
                                string newValue = await dbContext.PhRefMasters.Where(da => da.Id == editOpeningViewModel.CurrencyId).Select(da => da.Rmvalue).FirstOrDefaultAsync();
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Billing Currency from " + currentValue + " to " + newValue + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.CurrencyId = editOpeningViewModel.CurrencyId;
                            }
                            if (dbModel_jobOpeningAddlDtls.MinSalary != editOpeningViewModel.MinSalary)
                            {
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Job Min Salary from " + dbModel_jobOpeningAddlDtls.MinSalary + " to " + editOpeningViewModel.MinSalary + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.MinSalary = editOpeningViewModel.MinSalary;
                            }
                            if (dbModel_jobOpeningAddlDtls.MaxSalary != editOpeningViewModel.MaxSalary)
                            {
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Job Max Salary from " + dbModel_jobOpeningAddlDtls.MaxSalary + " to " + editOpeningViewModel.MaxSalary + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.MaxSalary = editOpeningViewModel.MaxSalary;
                            }

                            if (dbModel_jobOpeningAddlDtls.JobTenure != editOpeningViewModel.JobTenure)
                            {
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Job Tenure from " + dbModel_jobOpeningAddlDtls.JobTenure + " to  " + editOpeningViewModel.JobTenure + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.JobTenure = editOpeningViewModel.JobTenure;
                            }
                            if (dbModel_jobOpeningAddlDtls.NoOfCvsRequired != editOpeningViewModel.NoOfCvsRequired)
                            {
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated No Of Cvs Required from " + dbModel_jobOpeningAddlDtls.NoOfCvsRequired + " to " + editOpeningViewModel.NoOfCvsRequired + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.NoOfCvsRequired = editOpeningViewModel.NoOfCvsRequired;
                            }
                            if (dbModel_jobOpeningAddlDtls.JobWorkPattern != editOpeningViewModel.JobWorkPattern?.Value)
                            {
                                string currentValue = await dbContext.PhRefMasters.Where(da => da.Id == dbModel_jobOpeningAddlDtls.JobWorkPattern).Select(da => da.Rmvalue).FirstOrDefaultAsync();
                                string newValue = editOpeningViewModel.JobWorkPattern?.Value > 0 ? await dbContext.PhRefMasters.Where(da => da.Id == editOpeningViewModel.JobWorkPattern.Value).Select(da => da.Rmvalue).FirstOrDefaultAsync() : "";
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Job Work Pattern from " + currentValue + " to " + newValue + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.JobWorkPattern = editOpeningViewModel.JobWorkPattern?.Value;
                            }
                            if (dbModel_jobOpeningAddlDtls.JobWorkPatternPrefTyp != ToValue(editOpeningViewModel?.JobWorkPattern?.PreferenceType))
                            {
                                var currentValue = ToModel(dbModel_jobOpeningAddlDtls.JobWorkPatternPrefTyp);
                                var newValue = editOpeningViewModel.JobWorkPattern.PreferenceType;
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Job Work Pattern PreferenceType from " + currentValue + " to " + newValue + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.JobWorkPatternPrefTyp = ToValue(editOpeningViewModel?.JobWorkPattern?.PreferenceType);
                            }

                            if (dbModel_jobOpeningAddlDtls.NoticePeriod != editOpeningViewModel.NoticePeriod?.Value)
                            {
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Notice Period from " + dbModel_jobOpeningAddlDtls.NoticePeriod + " days to " + editOpeningViewModel.NoticePeriod?.Value + " days",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.NoticePeriod = editOpeningViewModel.NoticePeriod?.Value;
                            }
                            if (dbModel_jobOpeningAddlDtls.NoticePeriodPrefTyp != ToValue(editOpeningViewModel?.NoticePeriod?.PreferenceType))
                            {
                                var oldValue = ToModel(dbModel_jobOpeningAddlDtls.NoticePeriodPrefTyp);
                                var newValue = editOpeningViewModel?.NoticePeriod?.PreferenceType;

                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = dbModel_jobOpening.Id,
                                    JobId = dbModel_jobOpening.Id,
                                    ActivityType = (byte)LogActivityType.JobEditUpdates,
                                    ActivityDesc = " has updated Notice Period Preference Type from " + oldValue + " to " + newValue + "",
                                    UserId = UpdatedBy
                                };
                                activityList.Add(activityLog);
                                dbModel_jobOpeningAddlDtls.NoticePeriodPrefTyp = ToValue(editOpeningViewModel?.NoticePeriod?.PreferenceType);
                            }

                            dbContext.PhJobOpeningsAddlDetails.Update(dbModel_jobOpeningAddlDtls);
                        }


                        var dbModel_jobOpeningDesirables = await dbContext.PhJobOpeningsDesirables.FirstOrDefaultAsync(da => da.Joid == editOpeningViewModel.Id);
                        if (dbModel_jobOpeningDesirables == null)
                        {
                            dbContext.PhJobOpeningsDesirables.Add(new PhJobOpeningsDesirables
                            {
                                Joid = dbModel_jobOpening.Id,

                                JobDomain = editOpeningViewModel.JobDesirableDomain?.Value,
                                JobDomainPrefType = ToValue(editOpeningViewModel.JobDesirableDomain?.PreferenceType),

                                JobTeamRole = editOpeningViewModel.JobDesirableTeamRole?.Value,
                                JobTeamRolePrefType = ToValue(editOpeningViewModel.JobDesirableTeamRole?.PreferenceType),

                                CandidateValidPassport = editOpeningViewModel.CandidatePrefValidPassport?.Value,
                                CandidateValidPassportPrefType = ToValue(editOpeningViewModel.CandidatePrefValidPassport?.PreferenceType),

                                CandidateDOB = editOpeningViewModel.CandidatePrefDOB?.Value,
                                CandidateDOBPrefType = ToValue(editOpeningViewModel.CandidatePrefDOB?.PreferenceType),

                                CandidateGender = editOpeningViewModel.CandidatePrefGender?.Value,
                                CandidateGenderPrefType = ToValue(editOpeningViewModel.CandidatePrefGender?.PreferenceType),

                                CandidateMaritalStatus = editOpeningViewModel.CandidatePrefMaritalStatus?.Value,
                                CandidateMaritalStatusPrefType = ToValue(editOpeningViewModel.CandidatePrefMaritalStatus?.PreferenceType),

                                CandidateLanguage = editOpeningViewModel.CandidatePrefLanguage?.Value,
                                CandidateLanguagePrefType = ToValue(editOpeningViewModel.CandidatePrefLanguage?.PreferenceType),

                                CandidateVisaPreference = editOpeningViewModel.CandidatePrefVisaPreference?.Value,
                                CandidateVisaPreferencePrefType = ToValue(editOpeningViewModel.CandidatePrefVisaPreference?.PreferenceType),

                                CandidateRegion = editOpeningViewModel.CandidatePrefRegion?.Value,
                                CandidateRegionPrefType = ToValue(editOpeningViewModel.CandidatePrefRegion?.PreferenceType),

                                CandidateNationality = editOpeningViewModel.CandidatePrefNationality?.Value,
                                CandidateNationalityPrefType = ToValue(editOpeningViewModel.CandidatePrefNationality?.PreferenceType),

                                CandidateResidingCountry = editOpeningViewModel.CandidatePrefResidingCountry?.Value,
                                CandidateResidingCountryPrefType = ToValue(editOpeningViewModel.CandidatePrefResidingCountry?.PreferenceType),

                                CandidateResidingCity = editOpeningViewModel.CandidatePrefResidingCity?.Value,
                                CandidateResidingCityPrefType = ToValue(editOpeningViewModel.CandidatePrefResidingCity?.PreferenceType),

                                CandidateDrivingLicence = editOpeningViewModel.CandidatePrefDrivingLicence?.Value,
                                CandidateDrivingLicencePrefType = ToValue(editOpeningViewModel.CandidatePrefDrivingLicence?.PreferenceType),

                                CandidateEmployeeStatus = editOpeningViewModel.CandidatePrefEmployeeStatus?.Value,
                                CandidateEmployeeStatusPrefType = ToValue(editOpeningViewModel.CandidatePrefEmployeeStatus?.PreferenceType),

                                CandidateResume = editOpeningViewModel.CandidatePrefResume?.Value,
                                CandidateResumePrefType = ToValue(editOpeningViewModel.CandidatePrefResume?.PreferenceType),

                                CandidateVidPrfl = editOpeningViewModel.CandidatePrefVidPrfl?.Value,
                                CandidateVidPrflPrefType = ToValue(editOpeningViewModel.CandidatePrefVidPrfl?.PreferenceType),

                                CandidatePaySlp = editOpeningViewModel.CandidatePrefPaySlp?.Value,
                                CandidatePaySlpPrefType = ToValue(editOpeningViewModel.CandidatePrefPaySlp?.PreferenceType),

                                CandidateNoticePeriod = editOpeningViewModel.CandidatePrefNoticePeriod?.Value,
                                CandidateNoticePeriodPrefType = ToValue(editOpeningViewModel.CandidatePrefNoticePeriod?.PreferenceType),
                            });
                        }
                        else
                        {
                            dbModel_jobOpeningDesirables.JobDomain = editOpeningViewModel.JobDesirableDomain?.Value;
                            dbModel_jobOpeningDesirables.JobDomainPrefType = ToValue(editOpeningViewModel.JobDesirableDomain?.PreferenceType);

                            dbModel_jobOpeningDesirables.JobTeamRole = editOpeningViewModel.JobDesirableTeamRole?.Value;
                            dbModel_jobOpeningDesirables.JobTeamRolePrefType = ToValue(editOpeningViewModel.JobDesirableTeamRole?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateValidPassport = editOpeningViewModel.CandidatePrefValidPassport?.Value;
                            dbModel_jobOpeningDesirables.CandidateValidPassportPrefType = ToValue(editOpeningViewModel.CandidatePrefValidPassport?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateDOB = editOpeningViewModel.CandidatePrefDOB?.Value;
                            dbModel_jobOpeningDesirables.CandidateDOBPrefType = ToValue(editOpeningViewModel.CandidatePrefDOB?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateGender = editOpeningViewModel.CandidatePrefGender?.Value;
                            dbModel_jobOpeningDesirables.CandidateGenderPrefType = ToValue(editOpeningViewModel.CandidatePrefGender?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateMaritalStatus = editOpeningViewModel.CandidatePrefMaritalStatus?.Value;
                            dbModel_jobOpeningDesirables.CandidateMaritalStatusPrefType = ToValue(editOpeningViewModel.CandidatePrefMaritalStatus?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateLanguage = editOpeningViewModel.CandidatePrefLanguage?.Value;
                            dbModel_jobOpeningDesirables.CandidateLanguagePrefType = ToValue(editOpeningViewModel.CandidatePrefLanguage?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateVisaPreference = editOpeningViewModel.CandidatePrefVisaPreference?.Value;
                            dbModel_jobOpeningDesirables.CandidateVisaPreferencePrefType = ToValue(editOpeningViewModel.CandidatePrefVisaPreference?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateRegion = editOpeningViewModel.CandidatePrefRegion?.Value;
                            dbModel_jobOpeningDesirables.CandidateRegionPrefType = ToValue(editOpeningViewModel.CandidatePrefRegion?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateNationality = editOpeningViewModel.CandidatePrefNationality?.Value;
                            dbModel_jobOpeningDesirables.CandidateNationalityPrefType = ToValue(editOpeningViewModel.CandidatePrefNationality?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateResidingCountry = editOpeningViewModel.CandidatePrefResidingCountry?.Value;
                            dbModel_jobOpeningDesirables.CandidateResidingCountryPrefType = ToValue(editOpeningViewModel.CandidatePrefResidingCountry?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateResidingCity = editOpeningViewModel.CandidatePrefResidingCity?.Value;
                            dbModel_jobOpeningDesirables.CandidateResidingCityPrefType = ToValue(editOpeningViewModel.CandidatePrefResidingCity?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateDrivingLicence = editOpeningViewModel.CandidatePrefDrivingLicence?.Value;
                            dbModel_jobOpeningDesirables.CandidateDrivingLicencePrefType = ToValue(editOpeningViewModel.CandidatePrefDrivingLicence?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateEmployeeStatus = editOpeningViewModel.CandidatePrefEmployeeStatus?.Value;
                            dbModel_jobOpeningDesirables.CandidateEmployeeStatusPrefType = ToValue(editOpeningViewModel.CandidatePrefEmployeeStatus?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateResume = editOpeningViewModel.CandidatePrefResume?.Value;
                            dbModel_jobOpeningDesirables.CandidateResumePrefType = ToValue(editOpeningViewModel.CandidatePrefResume?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateVidPrfl = editOpeningViewModel.CandidatePrefVidPrfl?.Value;
                            dbModel_jobOpeningDesirables.CandidateVidPrflPrefType = ToValue(editOpeningViewModel.CandidatePrefVidPrfl?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidatePaySlp = editOpeningViewModel.CandidatePrefPaySlp?.Value;
                            dbModel_jobOpeningDesirables.CandidatePaySlpPrefType = ToValue(editOpeningViewModel.CandidatePrefPaySlp?.PreferenceType);

                            dbModel_jobOpeningDesirables.CandidateNoticePeriod = editOpeningViewModel.CandidatePrefNoticePeriod?.Value;
                            dbModel_jobOpeningDesirables.CandidateNoticePeriodPrefType = ToValue(editOpeningViewModel.CandidatePrefNoticePeriod?.PreferenceType);
                        }

                        //skill set 
                        {
                            var dbModel_jobOpeningSkills = await dbContext.PhJobOpeningSkills.Where(da => da.Joid == editOpeningViewModel.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                            var existingIds = dbModel_jobOpeningSkills.Select(da => da.Id).ToList();

                            if (editOpeningViewModel.OpeningSkillSet?.Count > 0)
                            {
                                foreach (var TechnologyId in editOpeningViewModel.OpeningSkillSet.Select(da => da.TechnologyId).Distinct())
                                {
                                    var item = editOpeningViewModel.OpeningSkillSet.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                                    var dbModel_jobOpeningSkill = dbModel_jobOpeningSkills.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                                    var TotalExpeInMonths = item.ExpMonth + ConvertMonths(item.ExpYears);
                                    if (dbModel_jobOpeningSkill == null)
                                    {
                                        dbContext.PhJobOpeningSkills.Add(new PhJobOpeningSkill
                                        {
                                            Joid = dbModel_jobOpening.Id,

                                            TechnologyId = item.TechnologyId,
                                            Technology = item.Technology,
                                            ExpYears = item.ExpYears,
                                            ExpMonth = item.ExpMonth,
                                            TotalExpeInMonths = TotalExpeInMonths,

                                            PreferenceType = ToValue(item.PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        });

                                        var activityLog = new CreateActivityViewModel
                                        {
                                            ActivityMode = (byte)WorkflowActionMode.Opening,
                                            ActivityOn = dbModel_jobOpening.Id,
                                            JobId = dbModel_jobOpening.Id,
                                            ActivityType = (byte)LogActivityType.JobEditUpdates,
                                            ActivityDesc = " has added new skill " + item.Technology + " with exp of " + TotalExpeInMonths + " month's ",
                                            UserId = UpdatedBy
                                        };
                                        activityList.Add(activityLog);
                                    }
                                    else
                                    {
                                        existingIds.Remove(dbModel_jobOpeningSkill.Id);
                                        dbModel_jobOpeningSkill.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningSkill.UpdatedDate = CurrentTime;

                                        dbModel_jobOpeningSkill.Technology = item.Technology;
                                        dbModel_jobOpeningSkill.TechnologyId = item.TechnologyId;
                                        dbModel_jobOpeningSkill.TotalExpeInMonths = TotalExpeInMonths;

                                        dbModel_jobOpeningSkill.PreferenceType = ToValue(item.PreferenceType);

                                        if (dbModel_jobOpeningSkill.ExpMonth != item.ExpMonth)
                                        {
                                            var activityLog = new CreateActivityViewModel
                                            {
                                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                                ActivityOn = dbModel_jobOpening.Id,
                                                JobId = dbModel_jobOpening.Id,
                                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                                ActivityDesc = " has updated month's of exp for " + item.Technology + " skill  from  " + dbModel_jobOpeningSkill.ExpMonth + " to " + item.ExpMonth + " ",
                                                UserId = UpdatedBy
                                            };
                                            activityList.Add(activityLog);
                                            dbModel_jobOpeningSkill.ExpMonth = item.ExpMonth;
                                        }
                                        if (dbModel_jobOpeningSkill.ExpYears != item.ExpYears)
                                        {
                                            var activityLog = new CreateActivityViewModel
                                            {
                                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                                ActivityOn = dbModel_jobOpening.Id,
                                                JobId = dbModel_jobOpening.Id,
                                                ActivityType = (byte)LogActivityType.JobEditUpdates,
                                                ActivityDesc = " has updated year's of exp for " + item.Technology + " skill from  " + dbModel_jobOpeningSkill.ExpYears + " to " + item.ExpYears + " ",
                                                UserId = UpdatedBy
                                            };
                                            activityList.Add(activityLog);
                                            dbModel_jobOpeningSkill.ExpYears = item.ExpYears;
                                        }
                                    }
                                }
                            }
                            foreach (var id in existingIds)
                            {
                                var dbModel_jobOpeningSkill = dbModel_jobOpeningSkills.FirstOrDefault(da => da.Id == id);
                                dbModel_jobOpeningSkill.Status = (byte)RecordStatus.Delete;
                                dbModel_jobOpeningSkill.UpdatedBy = UpdatedBy;
                                dbModel_jobOpeningSkill.UpdatedDate = CurrentTime;
                            }
                        }



                        {
                            var dbModel_jobOpeningDesirableSkills = await dbContext.PhJobOpeningsDesirableSkills.Where(da => da.Joid == dbModel_jobOpening.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                            var existingIds = dbModel_jobOpeningDesirableSkills.Select(da => da.Id).ToList();

                            if (editOpeningViewModel.JobDesirableSpecializations?.Count > 0)
                            {
                                var groupType = (byte)JobDesirableSkillGroupTypes.Specializations;
                                foreach (var TechnologyId in editOpeningViewModel.JobDesirableSpecializations.Select(da => da.TechnologyId).Distinct())
                                {
                                    var techData = editOpeningViewModel.JobDesirableSpecializations.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                                    var dbModel_jobOpeningDesirableSkill = dbModel_jobOpeningDesirableSkills.FirstOrDefault(da => da.GroupType == groupType && da.TechnologyId == TechnologyId);
                                    if (dbModel_jobOpeningDesirableSkill == null)
                                    {
                                        dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = groupType,

                                            TechnologyId = techData.TechnologyId,
                                            ExpYears = techData.ExpYears,
                                            PreferenceType = ToValue(techData.PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        });
                                    }
                                    else
                                    {
                                        existingIds.Remove(dbModel_jobOpeningDesirableSkill.Id);
                                        dbModel_jobOpeningDesirableSkill.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningDesirableSkill.UpdatedDate = CurrentTime;
                                        dbModel_jobOpeningDesirableSkill.ExpYears = techData.ExpYears;
                                        dbModel_jobOpeningDesirableSkill.TechnologyId = techData.TechnologyId;
                                        dbModel_jobOpeningDesirableSkill.PreferenceType = ToValue(techData.PreferenceType);
                                    }
                                }
                            }
                            if (editOpeningViewModel.JobDesirableImplementations?.Count > 0)
                            {
                                var groupType = (byte)JobDesirableSkillGroupTypes.Implementations;
                                foreach (var TechnologyId in editOpeningViewModel.JobDesirableImplementations.Select(da => da.TechnologyId).Distinct())
                                {
                                    var techData = editOpeningViewModel.JobDesirableImplementations.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                                    var dbModel_jobOpeningDesirableSkill = dbModel_jobOpeningDesirableSkills.FirstOrDefault(da => da.GroupType == groupType && da.TechnologyId == TechnologyId);
                                    if (dbModel_jobOpeningDesirableSkill == null)
                                    {
                                        dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = groupType,

                                            TechnologyId = techData.TechnologyId,
                                            ExpYears = techData.ExpYears,
                                            PreferenceType = ToValue(techData.PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        });
                                    }
                                    else
                                    {
                                        existingIds.Remove(dbModel_jobOpeningDesirableSkill.Id);
                                        dbModel_jobOpeningDesirableSkill.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningDesirableSkill.UpdatedDate = CurrentTime;
                                        dbModel_jobOpeningDesirableSkill.ExpYears = techData.ExpYears;
                                        dbModel_jobOpeningDesirableSkill.TechnologyId = techData.TechnologyId;
                                        dbModel_jobOpeningDesirableSkill.PreferenceType = ToValue(techData.PreferenceType);
                                    }
                                }
                            }
                            if (editOpeningViewModel.JobDesirableDesigns?.Count > 0)
                            {
                                var groupType = (byte)JobDesirableSkillGroupTypes.Designs;
                                foreach (var TechnologyId in editOpeningViewModel.JobDesirableDesigns.Select(da => da.TechnologyId).Distinct())
                                {
                                    var techData = editOpeningViewModel.JobDesirableDesigns.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                                    var dbModel_jobOpeningDesirableSkill = dbModel_jobOpeningDesirableSkills.FirstOrDefault(da => da.GroupType == groupType && da.TechnologyId == TechnologyId);
                                    if (dbModel_jobOpeningDesirableSkill == null)
                                    {
                                        dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = groupType,

                                            TechnologyId = techData.TechnologyId,
                                            ExpYears = techData.ExpYears,
                                            PreferenceType = ToValue(techData.PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        });
                                    }
                                    else
                                    {
                                        existingIds.Remove(dbModel_jobOpeningDesirableSkill.Id);
                                        dbModel_jobOpeningDesirableSkill.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningDesirableSkill.UpdatedDate = CurrentTime;
                                        dbModel_jobOpeningDesirableSkill.ExpYears = techData.ExpYears;
                                        dbModel_jobOpeningDesirableSkill.TechnologyId = techData.TechnologyId;
                                        dbModel_jobOpeningDesirableSkill.PreferenceType = ToValue(techData.PreferenceType);
                                    }
                                }
                            }
                            if (editOpeningViewModel.JobDesirableDevelopments?.Count > 0)
                            {
                                var groupType = (byte)JobDesirableSkillGroupTypes.Developments;
                                foreach (var TechnologyId in editOpeningViewModel.JobDesirableDevelopments.Select(da => da.TechnologyId).Distinct())
                                {
                                    var techData = editOpeningViewModel.JobDesirableDevelopments.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                                    var dbModel_jobOpeningDesirableSkill = dbModel_jobOpeningDesirableSkills.FirstOrDefault(da => da.GroupType == groupType && da.TechnologyId == TechnologyId);
                                    if (dbModel_jobOpeningDesirableSkill == null)
                                    {
                                        dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = groupType,

                                            TechnologyId = techData.TechnologyId,
                                            ExpYears = techData.ExpYears,
                                            PreferenceType = ToValue(techData.PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = dbModel_jobOpening.CreatedBy,
                                            CreatedDate = CurrentTime,
                                        });
                                    }
                                    else
                                    {
                                        existingIds.Remove(dbModel_jobOpeningDesirableSkill.Id);
                                        dbModel_jobOpeningDesirableSkill.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningDesirableSkill.UpdatedDate = CurrentTime;
                                        dbModel_jobOpeningDesirableSkill.ExpYears = techData.ExpYears;
                                        dbModel_jobOpeningDesirableSkill.TechnologyId = techData.TechnologyId;
                                        dbModel_jobOpeningDesirableSkill.PreferenceType = ToValue(techData.PreferenceType);
                                    }
                                }
                            }
                            if (editOpeningViewModel.JobDesirableSupports?.Count > 0)
                            {
                                var groupType = (byte)JobDesirableSkillGroupTypes.Supports;
                                foreach (var TechnologyId in editOpeningViewModel.JobDesirableSupports.Select(da => da.TechnologyId).Distinct())
                                {
                                    var techData = editOpeningViewModel.JobDesirableSupports.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                                    var dbModel_jobOpeningDesirableSkill = dbModel_jobOpeningDesirableSkills.FirstOrDefault(da => da.GroupType == groupType && da.TechnologyId == TechnologyId);
                                    if (dbModel_jobOpeningDesirableSkill == null)
                                    {
                                        dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = groupType,

                                            TechnologyId = techData.TechnologyId,
                                            ExpYears = techData.ExpYears,
                                            PreferenceType = ToValue(techData.PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        });
                                    }
                                    else
                                    {
                                        existingIds.Remove(dbModel_jobOpeningDesirableSkill.Id);
                                        dbModel_jobOpeningDesirableSkill.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningDesirableSkill.UpdatedDate = CurrentTime;
                                        dbModel_jobOpeningDesirableSkill.ExpYears = techData.ExpYears;
                                        dbModel_jobOpeningDesirableSkill.TechnologyId = techData.TechnologyId;
                                        dbModel_jobOpeningDesirableSkill.PreferenceType = ToValue(techData.PreferenceType);
                                    }
                                }
                            }
                            if (editOpeningViewModel.JobDesirableQualities?.Count > 0)
                            {
                                var groupType = (byte)JobDesirableSkillGroupTypes.Qualities;
                                foreach (var TechnologyId in editOpeningViewModel.JobDesirableQualities.Select(da => da.TechnologyId).Distinct())
                                {
                                    var techData = editOpeningViewModel.JobDesirableQualities.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                                    var dbModel_jobOpeningDesirableSkill = dbModel_jobOpeningDesirableSkills.FirstOrDefault(da => da.GroupType == groupType && da.TechnologyId == TechnologyId);
                                    if (dbModel_jobOpeningDesirableSkill == null)
                                    {
                                        dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = groupType,

                                            TechnologyId = techData.TechnologyId,
                                            ExpYears = techData.ExpYears,
                                            PreferenceType = ToValue(techData.PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        });
                                    }
                                    else
                                    {
                                        existingIds.Remove(dbModel_jobOpeningDesirableSkill.Id);
                                        dbModel_jobOpeningDesirableSkill.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningDesirableSkill.UpdatedDate = CurrentTime;
                                        dbModel_jobOpeningDesirableSkill.ExpYears = techData.ExpYears;
                                        dbModel_jobOpeningDesirableSkill.TechnologyId = techData.TechnologyId;
                                        dbModel_jobOpeningDesirableSkill.PreferenceType = ToValue(techData.PreferenceType);
                                    }
                                }
                            }
                            if (editOpeningViewModel.JobDesirableDocumentations?.Count > 0)
                            {
                                var groupType = (byte)JobDesirableSkillGroupTypes.Documentations;
                                foreach (var TechnologyId in editOpeningViewModel.JobDesirableDocumentations.Select(da => da.TechnologyId).Distinct())
                                {
                                    var techData = editOpeningViewModel.JobDesirableDocumentations.FirstOrDefault(da => da.TechnologyId == TechnologyId);
                                    var dbModel_jobOpeningDesirableSkill = dbModel_jobOpeningDesirableSkills.FirstOrDefault(da => da.GroupType == groupType && da.TechnologyId == TechnologyId);
                                    if (dbModel_jobOpeningDesirableSkill == null)
                                    {
                                        dbContext.PhJobOpeningsDesirableSkills.Add(new PhJobOpeningsDesirableSkill
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = groupType,

                                            TechnologyId = techData.TechnologyId,
                                            ExpYears = techData.ExpYears,
                                            PreferenceType = ToValue(techData.PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        });
                                    }
                                    else
                                    {
                                        existingIds.Remove(dbModel_jobOpeningDesirableSkill.Id);
                                        dbModel_jobOpeningDesirableSkill.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningDesirableSkill.UpdatedDate = CurrentTime;
                                        dbModel_jobOpeningDesirableSkill.ExpYears = techData.ExpYears;
                                        dbModel_jobOpeningDesirableSkill.TechnologyId = techData.TechnologyId;
                                        dbModel_jobOpeningDesirableSkill.PreferenceType = ToValue(techData.PreferenceType);
                                    }
                                }
                            }

                            foreach (var id in existingIds)
                            {
                                var dbModel_jobOpeningDesirableSkill = dbModel_jobOpeningDesirableSkills.FirstOrDefault(da => da.Id == id);
                                dbModel_jobOpeningDesirableSkill.Status = (byte)RecordStatus.Delete;
                                dbModel_jobOpeningDesirableSkill.UpdatedBy = UpdatedBy;
                                dbModel_jobOpeningDesirableSkill.UpdatedDate = CurrentTime;
                            }
                        }

                        //Qualifications
                        {
                            var dbModel_jobOpeningQualifications = await dbContext.PhJobOpeningsQualifications.Where(da => da.Joid == editOpeningViewModel.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                            var existingIds = dbModel_jobOpeningQualifications.Select(da => da.Id).ToList();

                            //Candidate Education Qualification
                            if (editOpeningViewModel.CandidateEducationQualifications?.Count > 0)
                            {
                                var GroupType = (byte)JobCandQualCertGroupTypes.JobCandiateEducationQualification;
                                foreach (var data in editOpeningViewModel.CandidateEducationQualifications.Select(da => new { da.Qualification, da.Course }).Distinct())
                                {
                                    var dbModel_jobOpeningQualification = dbModel_jobOpeningQualifications.FirstOrDefault(da => da.GroupType == GroupType && da.QualificationId == data.Qualification && da.CourseId == data.Course);
                                    if (dbModel_jobOpeningQualification == null)
                                    {
                                        dbContext.PhJobOpeningsQualifications.Add(new PhJobOpeningsQualification
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = GroupType,

                                            QualificationId = data.Qualification,
                                            CourseId = data.Course,
                                            PreferenceType = ToValue(editOpeningViewModel.CandidateEducationQualifications.FirstOrDefault(da => da.Qualification == data.Qualification && da.Course == data.Course).PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        });
                                    }
                                    else
                                    {
                                        dbModel_jobOpeningQualification.PreferenceType = ToValue(editOpeningViewModel.CandidateEducationQualifications.FirstOrDefault(da => da.Qualification == data.Qualification && da.Course == data.Course).PreferenceType);
                                        dbModel_jobOpeningQualification.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningQualification.UpdatedDate = CurrentDate;
                                        existingIds.Remove(dbModel_jobOpeningQualification.Id);
                                    }
                                }
                            }
                            //Opening Qualification
                            if (editOpeningViewModel.OpeningQualifications?.Count > 0)
                            {
                                var GroupType = (byte)JobCandQualCertGroupTypes.JobOpeningQualification;
                                foreach (var data in editOpeningViewModel.OpeningQualifications.Select(da => new { da.Value }).Distinct())
                                {
                                    var dbModel_jobOpeningQualification = dbModel_jobOpeningQualifications.FirstOrDefault(da => da.GroupType == GroupType && da.QualificationId == data.Value);
                                    if (dbModel_jobOpeningQualification == null)
                                    {
                                        dbModel_jobOpeningQualification = new PhJobOpeningsQualification
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = GroupType,

                                            QualificationId = data.Value,
                                            PreferenceType = ToValue(editOpeningViewModel.OpeningQualifications.FirstOrDefault(da => da.Value == data.Value).PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        };
                                        dbContext.PhJobOpeningsQualifications.Add(dbModel_jobOpeningQualification);
                                    }
                                    else
                                    {
                                        dbModel_jobOpeningQualification.PreferenceType = ToValue(editOpeningViewModel.OpeningQualifications.FirstOrDefault(da => da.Value == data.Value).PreferenceType);
                                        dbModel_jobOpeningQualification.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningQualification.UpdatedDate = CurrentDate;
                                        existingIds.Remove(dbModel_jobOpeningQualification.Id);
                                    }
                                }
                            }

                            foreach (var id in existingIds)
                            {
                                var dbModel_jobOpeningAssmt = dbModel_jobOpeningQualifications.FirstOrDefault(da => da.Id == id);
                                dbModel_jobOpeningAssmt.Status = (byte)RecordStatus.Delete;
                                dbModel_jobOpeningAssmt.UpdatedBy = UpdatedBy;
                                dbModel_jobOpeningAssmt.UpdatedDate = CurrentTime;
                            }
                        }

                        //certifications
                        {
                            var dbModel_jobOpeningCertifications = await dbContext.PhJobOpeningsCertifications.Where(da => da.Joid == editOpeningViewModel.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                            var existingIds = dbModel_jobOpeningCertifications.Select(da => da.Id).ToList();

                            //Candidate Education Certification
                            if (editOpeningViewModel.CandidateEducationCertifications?.Count > 0)
                            {
                                var GroupType = (byte)JobCandQualCertGroupTypes.JobCandidateEducationCertifications;
                                foreach (var data in editOpeningViewModel.CandidateEducationCertifications.Select(da => new { da.Value }).Distinct())
                                {
                                    var dbModel_jobOpeningCertification = dbModel_jobOpeningCertifications.FirstOrDefault(da => da.GroupType == GroupType && da.CertificationId == data.Value);
                                    if (dbModel_jobOpeningCertification == null)
                                    {
                                        dbContext.PhJobOpeningsCertifications.Add(new PhJobOpeningsCertification
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = GroupType,

                                            CertificationId = data.Value,
                                            PreferenceType = ToValue(editOpeningViewModel.CandidateEducationCertifications.FirstOrDefault(da => da.Value == data.Value).PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        });
                                    }
                                    else
                                    {
                                        dbModel_jobOpeningCertification.PreferenceType = ToValue(editOpeningViewModel.CandidateEducationCertifications.FirstOrDefault(da => da.Value == data.Value).PreferenceType);
                                        dbModel_jobOpeningCertification.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningCertification.UpdatedDate = CurrentDate;

                                        existingIds.Remove(dbModel_jobOpeningCertification.Id);
                                    }
                                }
                            }
                            //Opening Certification
                            if (editOpeningViewModel.OpeningCertifications?.Count > 0)
                            {
                                var GroupType = (byte)JobCandQualCertGroupTypes.JobOpeningCertification;
                                foreach (var data in editOpeningViewModel.OpeningCertifications.Select(da => new { da.Value }).Distinct())
                                {
                                    var dbModel_jobOpeningCertification = dbModel_jobOpeningCertifications.FirstOrDefault(da => da.GroupType == GroupType && da.CertificationId == data.Value);
                                    if (dbModel_jobOpeningCertification == null)
                                    {
                                        dbModel_jobOpeningCertification = new PhJobOpeningsCertification
                                        {
                                            Joid = dbModel_jobOpening.Id,
                                            GroupType = GroupType,

                                            CertificationId = data.Value,
                                            PreferenceType = ToValue(editOpeningViewModel.OpeningCertifications.FirstOrDefault(da => da.Value == data.Value).PreferenceType),

                                            Status = (byte)RecordStatus.Active,
                                            CreatedBy = UpdatedBy,
                                            CreatedDate = CurrentTime,
                                        };
                                        dbContext.PhJobOpeningsCertifications.Add(dbModel_jobOpeningCertification);
                                    }
                                    else
                                    {
                                        dbModel_jobOpeningCertification.PreferenceType = ToValue(editOpeningViewModel.OpeningCertifications.FirstOrDefault(da => da.Value == data.Value).PreferenceType);
                                        dbModel_jobOpeningCertification.UpdatedBy = UpdatedBy;
                                        dbModel_jobOpeningCertification.UpdatedDate = CurrentDate;
                                        existingIds.Remove(dbModel_jobOpeningCertification.Id);
                                    }
                                }
                            }

                            foreach (var id in existingIds)
                            {
                                var dbModel_jobOpeningCertification = dbModel_jobOpeningCertifications.FirstOrDefault(da => da.Id == id);
                                dbModel_jobOpeningCertification.Status = (byte)RecordStatus.Delete;
                                dbModel_jobOpeningCertification.UpdatedBy = UpdatedBy;
                                dbModel_jobOpeningCertification.UpdatedDate = CurrentTime;
                            }
                        }

                        // Assessments
                        {
                            var dbModel_jobOpeningAssmts = await dbContext.PhJobOpeningAssmts.Where(da => da.Joid == editOpeningViewModel.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                            var existingIds = dbModel_jobOpeningAssmts.Select(da => da.Id).ToList();
                            foreach (var CandStatusId in editOpeningViewModel.OpeningAssessments.Select(da => da.CandStatusId).Distinct())
                            {
                                var OpeningAssessment = editOpeningViewModel.OpeningAssessments.FirstOrDefault(da => da.CandStatusId == CandStatusId);
                                var dbModel_jobOpeningAssmt = dbModel_jobOpeningAssmts.FirstOrDefault(da => da.CandStatusId == CandStatusId);
                                if (dbModel_jobOpeningAssmt == null)
                                {
                                    dbModel_jobOpeningAssmt = new PhJobOpeningAssmt
                                    {
                                        CandStatusId = OpeningAssessment.CandStatusId,
                                        Status = (byte)RecordStatus.Active,
                                        AssessmentId = OpeningAssessment.AssessmentId,
                                        CreatedBy = UpdatedBy,
                                        CreatedDate = CurrentTime,
                                        Joid = dbModel_jobOpening.Id,
                                        StageId = OpeningAssessment.StageId
                                    };
                                    dbContext.PhJobOpeningAssmts.Add(dbModel_jobOpeningAssmt);
                                    var activityLog = new CreateActivityViewModel
                                    {
                                        ActivityMode = (byte)WorkflowActionMode.Opening,
                                        ActivityOn = dbModel_jobOpening.Id,
                                        JobId = dbModel_jobOpening.Id,
                                        ActivityType = (byte)LogActivityType.JobEditUpdates,
                                        ActivityDesc = " has added new " + OpeningAssessment.AssessmentName + " assessment for " + OpeningAssessment.StageName + " stage ",
                                        UserId = UpdatedBy
                                    };
                                    activityList.Add(activityLog);
                                }
                                else
                                {
                                    existingIds.Remove(dbModel_jobOpeningAssmt.Id);
                                    var activityLog = new CreateActivityViewModel
                                    {
                                        ActivityMode = (byte)WorkflowActionMode.Opening,
                                        ActivityOn = dbModel_jobOpening.Id,
                                        JobId = dbModel_jobOpening.Id,
                                        ActivityType = (byte)LogActivityType.JobEditUpdates,
                                        ActivityDesc = " has updated " + OpeningAssessment.AssessmentName + "  assessment for " + OpeningAssessment.StageName + " stage ",
                                        UserId = UpdatedBy
                                    };
                                    activityList.Add(activityLog);

                                    dbModel_jobOpeningAssmt.CandStatusId = OpeningAssessment.CandStatusId;
                                    dbModel_jobOpeningAssmt.AssessmentId = OpeningAssessment.AssessmentId;
                                    dbModel_jobOpeningAssmt.StageId = OpeningAssessment.StageId;
                                    dbModel_jobOpeningAssmt.UpdatedBy = UpdatedBy;
                                    dbModel_jobOpeningAssmt.UpdatedDate = CurrentTime;
                                    dbContext.PhJobOpeningAssmts.Update(dbModel_jobOpeningAssmt);
                                }
                            }
                            foreach (var id in existingIds)
                            {
                                var dbModel_jobOpeningAssmt = dbModel_jobOpeningAssmts.FirstOrDefault(da => da.Id == id);
                                dbModel_jobOpeningAssmt.Status = (byte)RecordStatus.Delete;
                                dbModel_jobOpeningAssmt.UpdatedBy = UpdatedBy;
                                dbModel_jobOpeningAssmt.UpdatedDate = CurrentTime;
                            }
                        }

                        ////preferences 
                        //if (editOpeningViewModel.OpeningPREFViewModel.Count > 0)
                        //{
                        //    foreach (var PREFitem in editOpeningViewModel.OpeningPREFViewModel)
                        //    {
                        //        if (PREFitem.Id == 0)
                        //        {
                        //            var phJobOpeningPref = new PhJobOpeningPref
                        //            {
                        //                CreatedBy = UpdatedBy,
                        //                CreatedDate = CurrentTime,
                        //                DisplayFlag = PREFitem.DisplayFlag,
                        //                FieldCode = PREFitem.FieldCode,
                        //                Joid = dbModel_jobOpening.Id,
                        //                Status = (byte)RecordStatus.Active
                        //            };
                        //            dbContext.PhJobOpeningPrefs.Add(phJobOpeningPref);

                        //            // activity
                        //            var currentValue = Enum.GetName(typeof(DisplayFlag), PREFitem.DisplayFlag);
                        //            var fieldValue = await dbContext.PhRefMasters.Where(da => da.Rmtype == PREFitem.FieldCode).Select(da => da.Rmdesc).FirstOrDefaultAsync();
                        //            var activityLog = new CreateActivityViewModel
                        //            {
                        //                ActivityMode = (byte)WorkflowActionMode.Opening,
                        //                ActivityOn = dbModel_jobOpening.Id,
                        //                JobId = dbModel_jobOpening.Id,
                        //                ActivityType = (byte)LogActivityType.JobEditUpdates,
                        //                ActivityDesc = " has added new " + fieldValue + " application wizard as " + currentValue + "",
                        //                UserId = UpdatedBy
                        //            };
                        //            activityList.Add(activityLog);
                        //        }
                        //        else
                        //        {
                        //            var phJobOpeningPref = await dbContext.PhJobOpeningPrefs.Where(da => da.Id == PREFitem.Id).FirstOrDefaultAsync();
                        //            if (phJobOpeningPref != null)
                        //            {

                        //                // activity
                        //                if (phJobOpeningPref.DisplayFlag != PREFitem.DisplayFlag)
                        //                {
                        //                    var fieldValue = await dbContext.PhRefMasters.Where(da => da.Rmvalue == PREFitem.FieldCode).Select(da => da.Rmdesc).FirstOrDefaultAsync();
                        //                    var currentValue = Enum.GetName(typeof(DisplayFlag), phJobOpeningPref.DisplayFlag);
                        //                    var newValue = Enum.GetName(typeof(DisplayFlag), PREFitem.DisplayFlag);
                        //                    var activityLog = new CreateActivityViewModel
                        //                    {
                        //                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        //                        ActivityOn = dbModel_jobOpening.Id,
                        //                        JobId = dbModel_jobOpening.Id,
                        //                        ActivityType = (byte)LogActivityType.JobEditUpdates,
                        //                        ActivityDesc = " has updated " + fieldValue + " application wizard from " + currentValue + " to  " + newValue + " ",
                        //                        UserId = UpdatedBy
                        //                    };
                        //                    activityList.Add(activityLog);
                        //                }

                        //                phJobOpeningPref.DisplayFlag = PREFitem.DisplayFlag;
                        //                phJobOpeningPref.FieldCode = PREFitem.FieldCode;
                        //                phJobOpeningPref.UpdatedBy = UpdatedBy;
                        //                phJobOpeningPref.UpdatedDate = CurrentTime;


                        //                dbContext.PhJobOpeningPrefs.Update(phJobOpeningPref);
                        //            }
                        //        }
                        //    }
                        //}



                        //var phJobOpeningActvCounter = dbContext.PhJobOpeningActvCounter.Where(da => da.Joid == dbModel_jobOpening.Id && da.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                        //if (phJobOpeningActvCounter != null)
                        //{
                        //    phJobOpeningActvCounter.AsmtCounter = AsmtCounter;
                        //    dbContext.PhJobOpeningActvCounter.Update(phJobOpeningActvCounter);
                        //}

                        // questions
                        //if (editOpeningViewModel.OpeningQtns != null)
                        //{
                        //    if (editOpeningViewModel.OpeningQtns.Count > 0)
                        //    {
                        //        foreach (var OpeningAssessment in editOpeningViewModel.OpeningQtns)
                        //        {
                        //            if (OpeningAssessment.Id == 0)
                        //            {
                        //                var phJobOpeningStQtn = dbContext.PhJobOpeningStQtns.Where(da => da.Joid == dbModel_jobOpening.Id
                        //           && da.QuestionText == OpeningAssessment.QuestionText && da.QuestionType == OpeningAssessment.QuestionType && da.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                        //                if (phJobOpeningStQtn == null)
                        //                {
                        //                    var phJobOpeningStQtns = new PhJobOpeningStQtn
                        //                    {
                        //                        CreatedBy = UpdatedBy,
                        //                        CreatedDate = CurrentTime,
                        //                        Joid = dbModel_jobOpening.Id,
                        //                        Status = (byte)RecordStatus.Active,
                        //                        IsMandatory = false,
                        //                        QuestionSlno = OpeningAssessment.Slno,
                        //                        QuestionText = OpeningAssessment.QuestionText,
                        //                        QuestionType = OpeningAssessment.QuestionType,
                        //                    };
                        //                    dbContext.PhJobOpeningStQtns.Add(phJobOpeningStQtns);

                        //                    var activityLog = new CreateActivityViewModel
                        //                    {
                        //                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        //                        ActivityOn = dbModel_jobOpening.Id,
                        //                        JobId = dbModel_jobOpening.Id,
                        //                        ActivityType = (byte)LogActivityType.JobEditUpdates,
                        //                        ActivityDesc = " has added new question : " + OpeningAssessment.QuestionText + " ",
                        //                        UserId = UpdatedBy
                        //                    };
                        //                    activityList.Add(activityLog);
                        //                }
                        //            }
                        //            else
                        //            {
                        //                var respQtn = dbContext.PhJobOpeningStQtns.Where(da => da.Joid == dbModel_jobOpening.Id && da.Id == OpeningAssessment.Id).FirstOrDefault();
                        //                if (respQtn != null)
                        //                {
                        //                    respQtn.QuestionSlno = OpeningAssessment.Slno;
                        //                    respQtn.QuestionType = OpeningAssessment.QuestionType;

                        //                    if (respQtn.QuestionText != OpeningAssessment.QuestionText)
                        //                    {
                        //                        var activityLog = new CreateActivityViewModel
                        //                        {
                        //                            ActivityMode = (byte)WorkflowActionMode.Opening,
                        //                            ActivityOn = dbModel_jobOpening.Id,
                        //                            JobId = dbModel_jobOpening.Id,
                        //                            ActivityType = (byte)LogActivityType.JobEditUpdates,
                        //                            ActivityDesc = " has updated question text from  " + respQtn.QuestionText + " to " + OpeningAssessment.QuestionText + " ",
                        //                            UserId = UpdatedBy
                        //                        };
                        //                        activityList.Add(activityLog);
                        //                    }
                        //                    respQtn.QuestionText = OpeningAssessment.QuestionText;

                        //                    dbContext.PhJobOpeningStQtns.Update(respQtn);
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                        await dbContext.SaveChangesAsync();

                        // audit 
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Update on Job",
                            ActivityDesc = " updated Job (" + dbModel_jobOpening.Id + ") successfully",
                            ActivityType = (byte)AuditActivityType.RecordUpdates,
                            TaskID = dbModel_jobOpening.Id,
                            UserId = UpdatedBy
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        if (activityList.Count > 0)
                        {
                            SaveActivity(activityList);
                        }
                        trans.Commit();
                        respModel.SetResult(message);
                    }
                    else
                    {
                        message = "Opening is not available";
                        respModel.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                        trans.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(editOpeningViewModel), respModel.Meta.RequestID, ex);

                    respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                    trans.Rollback();
                }
            return respModel;
        }
        private byte? ToValue(JobOpeningPreferenceTypes? preferenceType)
        {
            if (preferenceType.HasValue)
            {
                return (byte)preferenceType;
            }
            else return null;
        }

        public async Task<GetResponseViewModel<GetOpeningViewModel>> GetJobAsync(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<GetOpeningViewModel>();
            try
            {
                var dbModel_jobOpening = await dbContext.PhJobOpenings.AsNoTracking().FirstOrDefaultAsync(da => da.Id == Id && da.Status != (byte)RecordStatus.Delete);
                if (dbModel_jobOpening != null)
                {
                    //var refData = dbContext.PhRefMasters.Where(da => da.Status != (byte)RecordStatus.Delete).ToList();

                    var dbModel_jobOpeningAddlDtls = await dbContext.PhJobOpeningsAddlDetails.AsNoTracking().FirstOrDefaultAsync(da => da.Joid == Id);
                    var dbModel_jobOpeningDesirables = await dbContext.PhJobOpeningsDesirables.AsNoTracking().FirstOrDefaultAsync(da => da.Joid == Id);

                    var dtls = new GetOpeningViewModel
                    {
                        Id = dbModel_jobOpening.Id,

                        BroughtBy = dbModel_jobOpening.BroughtBy,

                        ClientId = dbModel_jobOpening.ClientId,
                        ClientName = dbModel_jobOpening.ClientName,

                        JobRole = dbModel_jobOpening.JobRole,
                        ShortJobDesc = dbModel_jobOpening.ShortJobDesc,
                        JobDescription = dbModel_jobOpening.JobDescription,

                        CountryId = dbModel_jobOpening.CountryId,
                        CityId = dbModel_jobOpening.JobLocationId,
                        LocalAnyWhere = dbModel_jobOpening.JobLocationLocal,

                        StartDate = dbModel_jobOpening.PostedDate,
                        ClosedDate = dbModel_jobOpening.ClosedDate,

                        JobCategoryId = dbModel_jobOpening.JobCategoryId,
                        JobCategory = dbModel_jobOpening.JobCategory,

                        Priority = dbModel_jobOpening.Priority,
                        NoOfPositions = dbModel_jobOpening.NoOfPositions,
                        MaxYears = ToGetOpeningModel(ConvertYearsNullable(dbModel_jobOpening.MaxExpeInMonths), dbModel_jobOpening.ExpeInMonthsPrefTyp),
                        MinYears = ToGetOpeningModel(ConvertYearsNullable(dbModel_jobOpening.MinExpeInMonths), dbModel_jobOpening.ExpeInMonthsPrefTyp),
                        ReleventExpMaxYears = ToGetOpeningModel(ConvertYearsNullable(dbModel_jobOpening.MaxReleventExpInMonths), dbModel_jobOpening.ReleventExpInMonthsPrefTyp),
                        ReleventExpMinYears = ToGetOpeningModel(ConvertYearsNullable(dbModel_jobOpening.MinReleventExpInMonths), dbModel_jobOpening.ReleventExpInMonthsPrefTyp),

                        JobOpeningStatus = dbModel_jobOpening.JobOpeningStatus,
                        JobOpeningStatusName = await dbContext.PhJobStatusSes.Where(da => da.Id == dbModel_jobOpening.JobOpeningStatus).Select(da => da.Title).FirstOrDefaultAsync(),


                        Puid = dbModel_jobOpeningAddlDtls.Puid,
                        Buid = dbModel_jobOpeningAddlDtls.Buid,

                        ApprJoinDate = dbModel_jobOpeningAddlDtls.ApprJoinDate,

                        Spocid = dbModel_jobOpeningAddlDtls.Spocid,
                        ClientReviewFlag = dbModel_jobOpeningAddlDtls.ClientReviewFlag,
                        ClientBilling = dbModel_jobOpeningAddlDtls.ClientBilling,
                        CurrencyId = dbModel_jobOpeningAddlDtls.CurrencyId,
                        MinSalary = dbModel_jobOpeningAddlDtls.MinSalary,
                        MaxSalary = dbModel_jobOpeningAddlDtls.MaxSalary,

                        JobTenure = dbModel_jobOpeningAddlDtls.JobTenure,
                        JobWorkPattern = ToGetOpeningModel(dbModel_jobOpeningAddlDtls?.JobWorkPattern, dbModel_jobOpeningAddlDtls?.JobWorkPatternPrefTyp),
                        NoOfCvsRequired = dbModel_jobOpeningAddlDtls.NoOfCvsRequired,
                        NoticePeriod = ToGetOpeningModel(dbModel_jobOpeningAddlDtls?.NoticePeriod, dbModel_jobOpeningAddlDtls?.NoticePeriodPrefTyp),


                        JobDesirableDomain = ToGetOpeningModel(dbModel_jobOpeningDesirables?.JobDomain, dbModel_jobOpeningDesirables?.JobDomainPrefType),
                        JobDesirableTeamRole = ToGetOpeningModel(dbModel_jobOpeningDesirables?.JobTeamRole, dbModel_jobOpeningDesirables?.JobTeamRolePrefType),
                        CandidatePrefValidPassport = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateValidPassport, dbModel_jobOpeningDesirables?.CandidateValidPassportPrefType),
                        CandidatePrefDOB = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateDOB, dbModel_jobOpeningDesirables?.CandidateDOBPrefType),
                        CandidatePrefGender = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateGender, dbModel_jobOpeningDesirables?.CandidateGenderPrefType),
                        CandidatePrefMaritalStatus = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateMaritalStatus, dbModel_jobOpeningDesirables?.CandidateMaritalStatusPrefType),
                        CandidatePrefLanguage = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateLanguage, dbModel_jobOpeningDesirables?.CandidateLanguagePrefType),
                        CandidatePrefVisaPreference = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateVisaPreference, dbModel_jobOpeningDesirables?.CandidateVisaPreferencePrefType),
                        CandidatePrefRegion = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateRegion, dbModel_jobOpeningDesirables?.CandidateRegionPrefType),
                        CandidatePrefNationality = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateNationality, dbModel_jobOpeningDesirables?.CandidateNationalityPrefType),
                        CandidatePrefResidingCountry = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateResidingCountry, dbModel_jobOpeningDesirables?.CandidateResidingCountryPrefType),
                        CandidatePrefResidingCity = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateResidingCity, dbModel_jobOpeningDesirables?.CandidateResidingCityPrefType),
                        CandidatePrefDrivingLicence = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateDrivingLicence, dbModel_jobOpeningDesirables?.CandidateDrivingLicencePrefType),
                        CandidatePrefEmployeeStatus = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateEmployeeStatus, dbModel_jobOpeningDesirables?.CandidateEmployeeStatusPrefType),
                        CandidatePrefResume = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateResume, dbModel_jobOpeningDesirables?.CandidateResumePrefType),
                        CandidatePrefVidPrfl = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateVidPrfl, dbModel_jobOpeningDesirables?.CandidateVidPrflPrefType),
                        CandidatePrefPaySlp = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidatePaySlp, dbModel_jobOpeningDesirables?.CandidatePaySlpPrefType),
                        CandidatePrefNoticePeriod = ToGetOpeningModel(dbModel_jobOpeningDesirables?.CandidateNoticePeriod, dbModel_jobOpeningDesirables?.CandidateNoticePeriodPrefType),

                        JobDesirableSpecializations = new List<CreateOpeningKnowledgePrefViewModel>(),
                        JobDesirableImplementations = new List<CreateOpeningKnowledgePrefViewModel>(),
                        JobDesirableDesigns = new List<CreateOpeningKnowledgePrefViewModel>(),
                        JobDesirableDevelopments = new List<CreateOpeningKnowledgePrefViewModel>(),
                        JobDesirableSupports = new List<CreateOpeningKnowledgePrefViewModel>(),
                        JobDesirableQualities = new List<CreateOpeningKnowledgePrefViewModel>(),
                        JobDesirableDocumentations = new List<CreateOpeningKnowledgePrefViewModel>(),

                        OpeningSkillSet = new List<CreateOpeningSkillSetViewModel>(),

                        CandidateEducationCertifications = new List<GetOpeningViewModel_PrefType<int>>(),
                        OpeningCertifications = new List<GetOpeningViewModel_PrefType<int>>(),
                        CandidateEducationQualifications = new List<CreateOpeningCandidateEducationQualificationViewModel>(),
                        OpeningQualifications = new List<GetOpeningViewModel_PrefType<int>>(),

                        OpeningAssessments = new List<CreateOpeningAssessmentViewModel>()
                    };

                    var dbModel_jobOpeningDesirableSkills = await dbContext.PhJobOpeningsDesirableSkills.AsNoTracking().Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Specializations))
                    {
                        dtls.JobDesirableSpecializations.Add(new CreateOpeningKnowledgePrefViewModel
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            ExpYears = dbModel_jobOpeningDesirableSkill.ExpYears,
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Implementations))
                    {
                        dtls.JobDesirableImplementations.Add(new CreateOpeningKnowledgePrefViewModel
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            ExpYears = dbModel_jobOpeningDesirableSkill.ExpYears,
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Designs))
                    {
                        dtls.JobDesirableDesigns.Add(new CreateOpeningKnowledgePrefViewModel
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            ExpYears = dbModel_jobOpeningDesirableSkill.ExpYears,
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Developments))
                    {
                        dtls.JobDesirableDevelopments.Add(new CreateOpeningKnowledgePrefViewModel
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            ExpYears = dbModel_jobOpeningDesirableSkill.ExpYears,
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Supports))
                    {
                        dtls.JobDesirableSupports.Add(new CreateOpeningKnowledgePrefViewModel
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            ExpYears = dbModel_jobOpeningDesirableSkill.ExpYears,
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Qualities))
                    {
                        dtls.JobDesirableQualities.Add(new CreateOpeningKnowledgePrefViewModel
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            ExpYears = dbModel_jobOpeningDesirableSkill.ExpYears,
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Documentations))
                    {
                        dtls.JobDesirableDocumentations.Add(new CreateOpeningKnowledgePrefViewModel
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            ExpYears = dbModel_jobOpeningDesirableSkill.ExpYears,
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }

                    //Skill set 
                    var dbModel_jobOpeningSkills = await dbContext.PhJobOpeningSkills.AsNoTracking().Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                    foreach (var dbModel_jobOpeningSkill in dbModel_jobOpeningSkills)
                    {
                        dtls.OpeningSkillSet.Add(new CreateOpeningSkillSetViewModel
                        {
                            TechnologyId = dbModel_jobOpeningSkill.TechnologyId,
                            Technology = dbModel_jobOpeningSkill.Technology,
                            ExpYears = dbModel_jobOpeningSkill.ExpYears,
                            ExpMonth = dbModel_jobOpeningSkill.ExpMonth,
                            PreferenceType = ToModel(dbModel_jobOpeningSkill.PreferenceType),
                        });
                    }

                    //certification 
                    var dbModel_jobOpeningCertifications = await dbContext.PhJobOpeningsCertifications.AsNoTracking().Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                    //Candidate Education Certification
                    foreach (var dbModel_jobOpeningCertification in dbModel_jobOpeningCertifications.Where(da => da.GroupType == (byte)JobCandQualCertGroupTypes.JobCandidateEducationCertifications))
                    {
                        dtls.CandidateEducationCertifications.Add(new GetOpeningViewModel_PrefType<int>
                        {
                            Value = dbModel_jobOpeningCertification.CertificationId,
                            PreferenceType = ToModel(dbModel_jobOpeningCertification.PreferenceType),
                        });
                    }
                    //Opening Certification
                    foreach (var dbModel_jobOpeningCertification in dbModel_jobOpeningCertifications.Where(da => da.GroupType == (byte)JobCandQualCertGroupTypes.JobOpeningCertification))
                    {
                        dtls.OpeningCertifications.Add(new GetOpeningViewModel_PrefType<int>
                        {
                            Value = dbModel_jobOpeningCertification.CertificationId,
                            PreferenceType = ToModel(dbModel_jobOpeningCertification.PreferenceType),
                        });
                    }

                    //Qualification
                    var dbModel_jobOpeningQualifications = await dbContext.PhJobOpeningsQualifications.AsNoTracking().Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                    //Candidate Education Qualification
                    foreach (var dbModel_jobOpeningQualification in dbModel_jobOpeningQualifications.Where(da => da.GroupType == (byte)JobCandQualCertGroupTypes.JobCandiateEducationQualification))
                    {
                        dtls.CandidateEducationQualifications.Add(new CreateOpeningCandidateEducationQualificationViewModel
                        {
                            Qualification = dbModel_jobOpeningQualification.QualificationId,
                            Course = dbModel_jobOpeningQualification.CourseId ?? 0,
                            PreferenceType = ToModel(dbModel_jobOpeningQualification.PreferenceType),
                        });
                    }
                    //Opening Qualification
                    foreach (var dbModel_jobOpeningQualification in dbModel_jobOpeningQualifications.Where(da => da.GroupType == (byte)JobCandQualCertGroupTypes.JobOpeningQualification))
                    {
                        dtls.OpeningQualifications.Add(new GetOpeningViewModel_PrefType<int>
                        {
                            Value = dbModel_jobOpeningQualification.QualificationId,
                            PreferenceType = ToModel(dbModel_jobOpeningQualification.PreferenceType),
                        });
                    }

                    // Assessments
                    dtls.OpeningAssessments = await dbContext.PhJobOpeningAssmts.Where(da => da.Joid == dbModel_jobOpening.Id && da.Status != (byte)RecordStatus.Delete)
                        .Select(da => new CreateOpeningAssessmentViewModel
                        {
                            AssessmentId = da.AssessmentId,
                            CandStatusId = da.CandStatusId,
                            StageId = da.StageId,
                        }).ToListAsync();

                    ////questions 
                    //dtls.OpeningQtns = new List<GetOpeningQtns>();
                    //dtls.OpeningQtns = await dbContext.PhJobOpeningStQtns.Select(da => new GetOpeningQtns
                    //{
                    //    Id = da.Id,
                    //    QuestionText = da.QuestionText,
                    //    QuestionType = da.QuestionType,
                    //    Slno = da.QuestionSlno,
                    //    JoId = da.Joid,
                    //    Status = da.Status
                    //}).Where(da => da.JoId == dbModel_jobOpening.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();

                    ////working recruiters 
                    //dtls.RecruiterViewModel = new List<RecruiterViewModel>();
                    //var recs = (from jobAsmt in dbContext.PhJobAssignments
                    //            join usr in dbContext.PiHireUsers on jobAsmt.AssignedTo equals usr.Id
                    //            where jobAsmt.Joid == Id && jobAsmt.DeassignDate == null && jobAsmt.Status != (byte)RecordStatus.Delete
                    //            select new RecruiterViewModel
                    //            {
                    //                Name = usr.FirstName + " " + usr.LastName,
                    //                UserId = usr.Id,
                    //                ContactNo = usr.MobileNumber,
                    //                EmailId = usr.UserName,
                    //                ProfilePhoto = usr.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + usr.Id + "/ProfilePhoto/" + usr.ProfilePhoto : string.Empty
                    //            }).ToList();
                    //if (recs.Count > 0)
                    //{
                    //    dtls.RecruiterViewModel = recs;
                    //}

                    respModel.SetResult(dtls);
                }
                else
                {
                    respModel.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The Opening is not available", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }
        private JobOpeningPreferenceTypes ToModel(byte? preferenceType)
        {
            if (preferenceType.Equals(null))
                //return null;
                return JobOpeningPreferenceTypes.Desirable;
            else
                return (JobOpeningPreferenceTypes)preferenceType;
        }
        private CreateOpeningPrefTypeViewModel<int> ToModel(int? data, byte? prefTyp)
        {
            if (data.Equals(null))
                return null;
            else
                return new CreateOpeningPrefTypeViewModel<int> { Value = data.Value, PreferenceType = (JobOpeningPreferenceTypes)prefTyp };
        }
        private CreateOpeningPrefTypeViewModel<bool> ToModel(bool? data, byte? prefTyp)
        {
            if (data.Equals(null))
                return null;
            else
                return new CreateOpeningPrefTypeViewModel<bool> { Value = data.Value, PreferenceType = (JobOpeningPreferenceTypes)prefTyp };
        }
        private GetOpeningViewModel_PrefType<int> ToGetOpeningModel(int? data, byte? prefTyp)
        {
            if (data.Equals(null))
                return null;
            else
                return new GetOpeningViewModel_PrefType<int> { Value = data.Value, PreferenceType = (JobOpeningPreferenceTypes?)prefTyp };
        }
        private GetOpeningViewModel_PrefType<bool> ToGetOpeningModel(bool? data, byte? prefTyp)
        {
            if (data.Equals(null))
                return null;
            else
                return new GetOpeningViewModel_PrefType<bool> { Value = data.Value, PreferenceType = (JobOpeningPreferenceTypes?)prefTyp };
        }
        private JobInfoViewModel_PrefType<int> ToCandidateModel(int? data, byte? prefTyp)
        {
            if (data.Equals(null))
                return null;
            else
                return new JobInfoViewModel_PrefType<int> { Value = data.Value, PreferenceType = (JobOpeningPreferenceTypes)prefTyp };
        }
        private JobInfoViewModel_PrefType<bool> ToCandidateModel(bool? data, byte? prefTyp)
        {
            if (data.Equals(null))
                return null;
            else
                return new JobInfoViewModel_PrefType<bool> { Value = data.Value, PreferenceType = (JobOpeningPreferenceTypes)prefTyp };
        }

        public async Task<GetResponseViewModel<GetOpeningViewModel>> GetPortalJobs(int Id)
        {
            return await GetJobAsync(Id);
            //pending
            //logger.SetMethodName(MethodBase.GetCurrentMethod());
            //var respModel = new GetResponseViewModel<GetOpeningViewModel>();
            //try
            //{

            //    var dtls = new GetOpeningViewModel();

            //    var dbModel_jobOpening = await dbContext.PhJobOpenings.Where(da => da.Id == Id).FirstOrDefaultAsync();
            //    if (dbModel_jobOpening != null)
            //    {
            //        dtls.Id = dbModel_jobOpening.Id;
            //        dtls.BroughtBy = dbModel_jobOpening.BroughtBy;
            //        dtls.JobTitle = dbModel_jobOpening.JobTitle;
            //        dtls.CountryId = dbModel_jobOpening.CountryId;
            //        dtls.CountryName = dbContext.PhCountries.Where(da => da.Id == dbModel_jobOpening.CountryId).Select(da => da.Nicename).FirstOrDefault();
            //        dtls.CityId = dbModel_jobOpening.JobLocationId;
            //        dtls.CityName = dbContext.PhCities.Where(da => da.Id == dbModel_jobOpening.JobLocationId).Select(da => da.Name).FirstOrDefault();
            //        dtls.JobRole = dbModel_jobOpening.JobRole;
            //        dtls.Priority = dbModel_jobOpening.Priority;
            //        dtls.NoOfPositions = dbModel_jobOpening.NoOfPositions;
            //        dtls.JobDescription = dbModel_jobOpening.JobDescription;
            //        dtls.JobCategory = dbModel_jobOpening.JobCategory;
            //        dtls.ClosedDate = dbModel_jobOpening.ClosedDate;
            //        dtls.StartDate = dbModel_jobOpening.PostedDate;
            //        dtls.ShortJobDesc = dbModel_jobOpening.ShortJobDesc;
            //        dtls.MaxYears = ConvertYears(dbModel_jobOpening.MaxExpeInMonths);
            //        dtls.MinYears = ConvertYears(dbModel_jobOpening.MinExpeInMonths);
            //        dtls.JobOpeningStatus = dbModel_jobOpening.JobOpeningStatus;
            //        dtls.JobOpeningStatusName = dbContext.PhJobStatusSes.Where(da => da.Id == dbModel_jobOpening.JobOpeningStatus).Select(da => da.Title).FirstOrDefault();

            //        var refData = dbContext.PhRefMasters.Where(da => da.Status != (byte)RecordStatus.Delete).ToList();
            //        var dbModel_jobOpeningAddlDtls = await dbContext.PhJobOpeningsAddlDetails.Where(da => da.Joid == Id).FirstOrDefaultAsync();
            //        if (dbModel_jobOpeningAddlDtls != null)
            //        {
            //            dtls.ApprJoinDate = dbModel_jobOpeningAddlDtls.ApprJoinDate;
            //            dtls.Buid = dbModel_jobOpeningAddlDtls.Buid;
            //            // dtls.ClientBilling = (int)dbModel_jobOpeningAddlDtls.ClientBilling;
            //            //  dtls.ClientReviewFlag = dbModel_jobOpeningAddlDtls.ClientReviewFlag;
            //            //  dtls.CurrencyId = dbModel_jobOpeningAddlDtls.CurrencyId;
            //            //  dtls.CurrencyName = dbModel_jobOpeningAddlDtls.CurrencyId == null ? string.Empty : refData.Where(da => da.GroupId == 13 && da.Id == dbModel_jobOpeningAddlDtls.CurrencyId.Value).Select(da => da.Rmvalue).FirstOrDefault();
            //            dtls.JobTenure = dbModel_jobOpeningAddlDtls.JobTenure;
            //            // dtls.MaxSalary = dbModel_jobOpeningAddlDtls.MaxSalary;
            //            // dtls.MinSalary = dbModel_jobOpeningAddlDtls.MinSalary;
            //            //  dtls.NoOfCvsRequired = dbModel_jobOpeningAddlDtls.NoOfCvsRequired;
            //            dtls.NoticePeriod = dbModel_jobOpeningAddlDtls.NoticePeriod;
            //            dtls.Puid = dbModel_jobOpeningAddlDtls.Puid;
            //            dtls.Spocid = dbModel_jobOpeningAddlDtls.Spocid;
            //        }


            //        //skill set 
            //        dtls.OpeningSkillSet = new List<GetOpeningSkillSetViewModel>();
            //        dtls.OpeningSkillSet = (from OpeningSkill in dbContext.PhJobOpeningSkills
            //                                join tech in dbContext.PhTechnologysSes on OpeningSkill.TechnologyId equals tech.Id
            //                                where OpeningSkill.Joid == dbModel_jobOpening.Id && OpeningSkill.Status == (byte)RecordStatus.Active
            //                                select new GetOpeningSkillSetViewModel
            //                                {
            //                                    Id = OpeningSkill.Id,
            //                                    ExpMonth = OpeningSkill.ExpMonth,
            //                                    ExpYears = OpeningSkill.ExpYears,
            //                                    SkillName = OpeningSkill.SkillName,
            //                                    SkillLevelId = OpeningSkill.SkillLevelId,
            //                                    Technology = OpeningSkill.Technology,
            //                                    TechnologyId = OpeningSkill.TechnologyId,
            //                                    JobId = OpeningSkill.Joid
            //                                }).ToList();


            //        //qualification 
            //        dtls.OpeningQualifications = new List<GetOpeningQualification>();
            //        dtls.OpeningQualifications = await dbContext.PhJobOpeningsQualifications.Select(da => new GetOpeningQualification
            //        {
            //            Id = da.Id,
            //            JoId = da.Joid,
            //            Status = da.Status,
            //            Course = da.CourseId,
            //            Qualification = da.QualificationId
            //        }).Where(da => da.JoId == dbModel_jobOpening.Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();

            //        foreach (var qtn in dtls.OpeningQualifications)
            //        {
            //            qtn.QualificationName = refData.Where(y => y.Id == qtn.Qualification).Select(y => y.Rmvalue).FirstOrDefault();
            //            qtn.CourseName = refData.Where(y => y.Id == qtn.Course).Select(y => y.Rmvalue).FirstOrDefault();
            //        }


            //        //working recruiters 
            //        dtls.RecruiterViewModel = new List<RecruiterViewModel>();
            //        var recs = (from jobAsmt in dbContext.PhJobAssignments
            //                    join usr in dbContext.PiHireUsers on jobAsmt.AssignedTo equals usr.Id
            //                    where jobAsmt.Joid == Id && jobAsmt.DeassignDate == null && jobAsmt.Status != (byte)RecordStatus.Delete
            //                    select new RecruiterViewModel
            //                    {
            //                        Name = usr.FirstName + " " + usr.LastName,
            //                        UserId = usr.Id,
            //                        ContactNo = usr.MobileNumber,
            //                        EmailId = usr.UserName,
            //                        ProfilePhoto = usr.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + usr.Id + "/ProfilePhoto/" + usr.ProfilePhoto : string.Empty
            //                    }).ToList();
            //        if (recs.Count > 0)
            //        {
            //            dtls.RecruiterViewModel = recs;
            //        }

            //        respModel.Status = true;
            //        respModel.SetResult(dtls);
            //    }
            //    else
            //    {
            //        respModel.Status = false;
            //        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "The Opening is not available", true);
            //    }

            //}
            //catch (Exception ex)
            //{
            //    logger.Log(LogLevel.Error, LoggingEvents.GetItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

            //    respModel.Status = false;
            //    respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            //    respModel.Result = null;

            //}
            //return respModel;
        }


        public async Task<GetResponseViewModel<JobListViewModel>> JobsList(OpeningListSearchViewModel openingListSearchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            string FilterKey = string.Empty;
            string Sort = string.Empty;
            string SortDirection = string.Empty;
            int UserId = Usr.Id;

            var respModel = new GetResponseViewModel<JobListViewModel>();
            try
            {

                var Openings = new JobListViewModel();

                openingListSearchViewModel.CurrentPage = (openingListSearchViewModel.CurrentPage.Value - 1) * openingListSearchViewModel.PerPage.Value;

                var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == UserId).ToListAsync();
                if ((byte)UserType.SuperAdmin != Usr.UserTypeId)
                {
                    var pusId = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                    var pusIds = String.Join(",", pusId);
                    if (pusIds.Length > 0)
                    {
                        FilterKey += " and jobAddl.PUID in (" + pusIds + ") ";
                    }
                }

                if (openingListSearchViewModel.FilterType == (byte)JobFilterType.AllJobs)
                {
                    if (Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        FilterKey += "  and JobStatus.JSCode !='CLS' and (Job.CreatedBy != " + UserId + " or Job.BroughtBy != " + UserId + ") ";
                    }
                    else
                    {
                        FilterKey += " and JobStatus.JSCode !='CLS' ";
                    }
                }

                if (openingListSearchViewModel.FilterType == (byte)JobFilterType.MyJobs)
                {
                    if (Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        FilterKey += "  and JobStatus.JSCode !='CLS' and (Job.CreatedBy = " + UserId + " or Job.BroughtBy = " + UserId + ") ";
                    }
                    else
                    {
                        FilterKey += " and JobStatus.JSCode !='CLS' ";
                    }
                }
                if (openingListSearchViewModel.FilterType == (byte)JobFilterType.New)
                {
                    if (Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        FilterKey += "  and JobStatus.JSCode ='WIP' and (Job.CreatedBy = " + UserId + " or Job.BroughtBy = " + UserId + ") ";
                    }
                    else
                    {
                        FilterKey += " and JobStatus.JSCode ='WIP' ";
                    }
                }
                if (openingListSearchViewModel.FilterType == (byte)JobFilterType.Closed)
                {
                    if (Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        FilterKey += "  and JobStatus.JSCode ='CLS' and (Job.CreatedBy = " + UserId + " or Job.BroughtBy = " + UserId + ") ";
                    }
                    else
                    {
                        FilterKey += " and JobStatus.JSCode ='CLS' ";
                    }
                }
                if (openingListSearchViewModel.FilterType == (byte)JobFilterType.MoreCvsRequired)
                {
                    if (Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        FilterKey += "  and JobStatus.JSCode ='MCV' and (Job.CreatedBy = " + UserId + " or Job.BroughtBy = " + UserId + ") ";
                    }
                    else
                    {
                        FilterKey += " and JobStatus.JSCode ='MCV' ";
                    }
                }
                if (openingListSearchViewModel.FilterType == (byte)JobFilterType.OnHold)
                {
                    if (Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        FilterKey += "  and JobStatus.JSCode ='HLD' and (Job.CreatedBy = " + UserId + " or Job.BroughtBy = " + UserId + ") ";
                    }
                    else
                    {
                        FilterKey += " and JobStatus.JSCode ='HLD' ";
                    }
                }
                if (openingListSearchViewModel.FilterType == (byte)JobFilterType.Overdue)
                {
                    if (Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        FilterKey += "  and JobStatus.JSCode !='CLS' and Job.ClosedDate <= GETDATE() and (Job.CreatedBy = " + UserId + " or Job.BroughtBy = " + UserId + ") ";
                    }
                    else
                    {
                        FilterKey += " and JobStatus.JSCode !='CLS' and Job.ClosedDate <= GETDATE()";
                    }
                }
                if (openingListSearchViewModel.FilterType == (byte)JobFilterType.ReOpen)
                {
                    if (Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        FilterKey += "  and JobStatus.JSCode ='RPN' and (Job.CreatedBy = " + UserId + " or Job.BroughtBy = " + UserId + ") ";
                    }
                    else
                    {
                        FilterKey += " and JobStatus.JSCode ='RPN'";
                    }
                }
                if (openingListSearchViewModel.FilterType == (byte)JobFilterType.Submit)
                {
                    if (Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        FilterKey += "  and JobStatus.JSCode ='SUB' and (Job.CreatedBy = " + UserId + " or Job.BroughtBy = " + UserId + ") ";
                    }
                    else
                    {
                        FilterKey += " and JobStatus.JSCode ='SUB'";
                    }
                }

                if (!string.IsNullOrEmpty(openingListSearchViewModel.SearchKey))
                {
                    openingListSearchViewModel.SearchKey = openingListSearchViewModel.SearchKey.Replace("'", " ");
                    string @search = '%' + openingListSearchViewModel.SearchKey + '%';
                    FilterKey += " and (Job.Id like '" + @search + "' or Job.ClientName like '" + @search + "' or Job.JobTitle like '" + @search + "' or Job.JobRole like '" + @search + "') ";
                }
                if (!string.IsNullOrWhiteSpace(openingListSearchViewModel.Sort))
                {
                    Sort = "Job.CreatedDate";
                    SortDirection = openingListSearchViewModel.Sort;
                }
                else
                {
                    Sort = "Job.CreatedDate";
                    SortDirection = "desc";
                }
                var dtls = await dbContext.GetJobsList(FilterKey, Sort, SortDirection, UserId, Usr.UserTypeId, openingListSearchViewModel.FilterType, openingListSearchViewModel.PerPage, openingListSearchViewModel.CurrentPage);


                if (dtls.JobList.Count > 0)
                {
                    Openings.OpeningList = new List<JobsList>();
                    Openings.OpeningCount = dtls.JobCount;

                    var joIds = dtls.JobList.Select(x => x.Id).ToArray();
                    var stages = await dbContext.PhCandStagesSes.Where(x => x.Status != (byte)RecordStatus.Delete).ToListAsync();
                    var counters = await dbContext.PhJobOpeningStatusCounters.Where(x => joIds.Contains(x.Joid) && x.Status != (byte)RecordStatus.Delete).ToListAsync();

                    foreach (var item in dtls.JobList)
                    {

                        item.JobOpeningStatusCounter = new List<JobOpeningStatusCounterViewModel>();
                        foreach (var stats in stages)
                        {
                            item.JobOpeningStatusCounter.Add(new JobOpeningStatusCounterViewModel
                            {
                                StageID = stats.Id,
                                StageColor = stats.ColorCode,
                                StageName = stats.Title,
                                Counter = counters.Where(x => x.Joid == item.Id && x.StageId == stats.Id).Sum(x => x.Counter)
                            });
                        }
                        Openings.OpeningList.Add(item);
                    }
                }

                respModel.Status = true;
                respModel.SetResult(Openings);
            }
            catch (Exception ex)
            {

                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(openingListSearchViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }


        public async Task<GetResponseViewModel<JobListViewModel>> JobsListToAssignRecruiters(JobsListToAssignRecruitersSearchViewModel jobsListToAssignRecruitersSearchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<JobListViewModel>();
            try
            {

                var Openings = new JobListViewModel();

                jobsListToAssignRecruitersSearchViewModel.CurrentPage = (jobsListToAssignRecruitersSearchViewModel.CurrentPage.Value - 1) * jobsListToAssignRecruitersSearchViewModel.PerPage.Value;

                var dtls = await dbContext.GetJobsToAssignRecruiters(jobsListToAssignRecruitersSearchViewModel.SearchKey,
                    jobsListToAssignRecruitersSearchViewModel.UserId, jobsListToAssignRecruitersSearchViewModel.PerPage, jobsListToAssignRecruitersSearchViewModel.CurrentPage);

                if (dtls.JobList.Count > 0)
                {
                    Openings.OpeningList = new List<JobsList>();
                    Openings.OpeningList = dtls.JobList;
                    Openings.OpeningCount = dtls.JobCount;
                }

                respModel.Status = true;
                respModel.SetResult(Openings);
            }
            catch (Exception ex)
            {

                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(jobsListToAssignRecruitersSearchViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<RecruitersJobListToAssignViewModel>> RecruiterTodayAssignmentsAsync(JobsListToAssignRecruitersSearchViewModel searchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<RecruitersJobListToAssignViewModel>();
            try
            {

                var Openings = new RecruitersJobListToAssignViewModel();

                searchViewModel.CurrentPage = (searchViewModel.CurrentPage.Value - 1) * searchViewModel.PerPage.Value;

                var dtls = await dbContext.GetJobsToAssignRecruiters(searchViewModel.SearchKey,
                                            searchViewModel.UserId, searchViewModel.PerPage, searchViewModel.CurrentPage);

                if (dtls.JobList.Count > 0)
                {
                    var joIds = dtls.JobList.Where(da => da.isAssigned > 0).Select(da => da.Id).ToHashSet();
                    var cvTargetDates = await dbContext.PhJobAssignments.Where(da => da.AssignedTo == searchViewModel.UserId && da.DeassignDate.HasValue == false && joIds.Contains(da.Joid) && da.Status == (byte)RecordStatus.Active)
                                        .ToDictionaryAsync(da => da.Joid, da2 => da2.CvTargetDate);
                    var currentDt = CurrentTime.Date;
                    var todayCvsRequired = await dbContext.PhJobAssignmentsDayWises.Where(da => da.AssignedTo == searchViewModel.UserId && da.AssignmentDate.Value.Date == currentDt && joIds.Contains(da.Joid) && da.Status == (byte)RecordStatus.Active)
                                        .ToDictionaryAsync(da => da.Joid, da2 => da2.NoCvsrequired);

                    Openings.OpeningList = RecruitersJobToAssignViewModel.ToModel(dtls.JobList, cvTargetDates, todayCvsRequired);
                    Openings.OpeningsCount = dtls.JobCount;
                }

                respModel.Status = true;
                respModel.SetResult(Openings);
            }
            catch (Exception ex)
            {

                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(searchViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }
        public async Task<GetResponseViewModel<RecruiterJobAssignmentSearchResponseViewModel>> RecruiterJobAssignmentDayWiseAsync_search(RecruiterJobAssignmentSearchViewModel srchModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new GetResponseViewModel<RecruiterJobAssignmentSearchResponseViewModel>();
            try
            {
                var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.Recruiter && da.Id == srchModel.RecruiterId);

                switch ((UserType)Usr.UserTypeId)
                {
                    case UserType.SuperAdmin:
                        {

                        }
                        break;
                    case UserType.Admin:
                        {
                            var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                            var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                            //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                            var allowedUsers = await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync();

                            hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                        }
                        break;
                    case UserType.BDM:
                        {
                            HashSet<int> allowedUsers = new HashSet<int>();
                            {
                                var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                allowedUsers = (await qry.Where(da => da.BdmId > 0).Select(da => da.BdmId.Value).Distinct().ToListAsync()).ToHashSet();
                            }
                            hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                        }
                        break;
                    case UserType.Recruiter:
                        {
                            hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                        }
                        break;
                    default:
                        {
                            hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                        }
                        break;
                }

                var recId = await hireUsrQry.Select(da => da.Id).FirstOrDefaultAsync();

                if (recId > 0)
                {
                    var jobId = await dbContext.PhJobOpenings.AsNoTracking()
                                .Where(da => da.Status != (byte)RecordStatus.Delete && da.Id == srchModel.JoId)
                                .Select(da => da.Id).FirstOrDefaultAsync();

                    if (jobId > 0)
                    {
                        var model = new RecruiterJobAssignmentSearchResponseViewModel
                        {
                            AssignmentDate = srchModel.AssignmentDate.Date,
                            JoId = srchModel.JoId,
                            RecruiterId = srchModel.RecruiterId,
                        };
                        var jobAsgmnt = await dbContext.PhJobAssignments.Where(da => da.Joid == jobId && da.AssignedTo == recId && da.Status != (byte)RecordStatus.Delete)
                                            .Select(da => new { da.NoCvsrequired, da.DeassignDate }).FirstOrDefaultAsync();
                        if (jobAsgmnt != null)
                        {
                            model.DeassignDate = jobAsgmnt.DeassignDate;
                            model.CumulativeNoOfCvs = jobAsgmnt.NoCvsrequired;
                            var jobAsgmntDayWise = await dbContext.PhJobAssignmentsDayWises.Where(da => da.Joid == jobId && da.AssignedTo == recId && da.AssignmentDate == model.AssignmentDate && da.Status != (byte)RecordStatus.Delete)
                                            .Select(da => new { da.NoCvsrequired }).FirstOrDefaultAsync();
                            model.NoOfCvs = jobAsgmntDayWise?.NoCvsrequired;
                        }
                        respModel.SetResult(model);
                    }
                    else
                    {
                        respModel.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Invalid job details", true);
                    }
                }
                else
                {
                    respModel.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Unauthorized recruiter details", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, " model:" + Newtonsoft.Json.JsonConvert.SerializeObject(srchModel), respModel.Meta.RequestID, ex);

                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> RecruiterJobAssignmentDayWiseAsync(RecruiterJobAssignmentViewModel model)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
            try
            {
                var hireUsrQry = dbContext.PiHireUsers.AsNoTracking()
                                .Where(da => da.Status == (byte)RecordStatus.Active && da.UserType == (byte)UserType.Recruiter && da.Id == model.RecruiterId);

                switch ((UserType)Usr.UserTypeId)
                {
                    case UserType.SuperAdmin:
                        {

                        }
                        break;
                    case UserType.Admin:
                        {
                            var PuBus = await dbContext.VwUserPuBus.AsNoTracking().Where(da => da.UserId == loginUserId).ToListAsync();
                            var pusIds = PuBus.Select(da => da.ProcessUnit).Distinct().ToArray();
                            //var busIds = PuBus.Select(da => da.BusinessUnit).Distinct().ToArray();
                            var allowedUsers = await dbContext.VwUserPuBus.AsNoTracking().Where(da => pusIds.Contains(da.ProcessUnit) /*&& busIds.Contains(da.BusinessUnit)*/).Select(da => da.UserId).ToArrayAsync();

                            hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                        }
                        break;
                    case UserType.BDM:
                        {
                            HashSet<int> allowedUsers = new HashSet<int>();
                            {
                                var qry = dbContext.VwDashboardDaywiseFilterData.AsNoTracking().Where(da => da.BdmId == loginUserId);
                                allowedUsers = (await qry.Where(da => da.BdmId > 0).Select(da => da.BdmId.Value).Distinct().ToListAsync()).ToHashSet();
                            }
                            hireUsrQry = hireUsrQry.Where(da => allowedUsers.Contains(da.Id));
                        }
                        break;
                    case UserType.Recruiter:
                        {
                            hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                        }
                        break;
                    default:
                        {
                            hireUsrQry = hireUsrQry.Where(da => 1 != 1);
                        }
                        break;
                }

                var recId = await hireUsrQry.Select(da => da.Id).FirstOrDefaultAsync();

                if (recId > 0)
                {
                    var jobOpenings = await dbContext.PhJobOpenings.Where(da => da.Status != (byte)RecordStatus.Delete && da.Id == model.JoId).FirstOrDefaultAsync();


                    if (jobOpenings != null)
                    {
                        var statusDtls = dbContext.PhJobStatusSes.ToList();
                        var assign = dbContext.PiHireUsers.Where(x => x.Id == recId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

                        // Day Wise Actions 
                        if (Usr.UserTypeId != (byte)UserType.Recruiter)
                        {
                            var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == model.JoId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                            if (IsDayWiseAction == null)
                            {
                                IsDayWiseAction = new PhDayWiseJobAction
                                {
                                    CreatedBy = loginUserId,
                                    CreatedDate = CurrentTime,
                                    Joid = model.JoId,
                                    Assign = true
                                };
                                dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                                dbContext.SaveChanges();
                            }
                            else
                            {
                                IsDayWiseAction.Assign = true;
                                dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                                dbContext.SaveChanges();
                            }
                        }


                        var jobAsgmnt = await dbContext.PhJobAssignments.FirstOrDefaultAsync(da => da.Joid == model.JoId && da.AssignedTo == recId && da.Status != (byte)RecordStatus.Delete);
                        if (jobAsgmnt == null)
                        {
                            jobAsgmnt = new PhJobAssignment
                            {
                                Joid = model.JoId,
                                AssignedTo = recId,

                                CvTarget = model.NoOfCvs,
                                NoCvsrequired = model.NoOfCvs,
                                ProfilesRejected = 0,
                                ProfilesUploaded = 0,

                                CvTargetDate = model.AssignmentDate.Date,

                                AssignBy = (byte)JobAssignBy.Manual,
                                CreatedBy = loginUserId,
                                CreatedDate = CurrentTime,

                                Status = (byte)RecordStatus.Active,
                            };
                            dbContext.PhJobAssignments.Add(jobAsgmnt);
                            await dbContext.SaveChangesAsync();
                            if (model.IsReplacement == true)
                                PhJobAssignmentsDayWise_records(ref jobAsgmnt, model.AssignmentDate, updateCvsrequired: model.NoOfCvs);
                            else
                                PhJobAssignmentsDayWise_records(ref jobAsgmnt, model.AssignmentDate, incrementCvsrequired: model.NoOfCvs);

                            dbContext.PhJobAssignmentHistories.Add(new PhJobAssignmentHistory
                            {
                                Joid = jobAsgmnt.Joid,
                                AssignedTo = jobAsgmnt.AssignedTo,
                                CreatedBy = jobAsgmnt.CreatedBy,
                                CreatedDate = jobAsgmnt.CreatedDate,
                                Status = (byte)RecordStatus.Active
                            });
                            await dbContext.SaveChangesAsync();



                            // activity
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = recId,
                                JobId = model.JoId,
                                ActivityType = (byte)LogActivityType.RecordUpdates,
                                ActivityDesc = " has Assigned " + jobOpenings.JobTitle + " job to " + assign + " successfully",
                                UserId = loginUserId
                            };
                            activityList.Add(activityLog);

                            // applying work flow conditions 
                            var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                            {
                                ActionMode = (byte)WorkflowActionMode.Other,
                                JobId = model.JoId,
                                TaskCode = TaskCode.AGJ.ToString(),
                                UserId = loginUserId,
                                AssignTo = recId
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
                                        CreatedBy = loginUserId
                                    };
                                    notificationPushedViewModel.Add(notificationPushed);
                                }
                            }
                        }
                        else if (jobAsgmnt.DeassignDate.HasValue)
                        {
                            jobAsgmnt.ReassignDate = CurrentTime;
                            jobAsgmnt.ReassignBy = loginUserId;
                            jobAsgmnt.DeassignBy = null;
                            jobAsgmnt.DeassignDate = null;
                            jobAsgmnt.AssignBy = (byte)JobAssignBy.Manual;

                            jobAsgmnt.UpdatedBy = loginUserId;
                            jobAsgmnt.UpdatedDate = CurrentTime;

                            if (model.IsReplacement == true)
                                PhJobAssignmentsDayWise_records(ref jobAsgmnt, model.AssignmentDate, updateCvsrequired: model.NoOfCvs);
                            else
                                PhJobAssignmentsDayWise_records(ref jobAsgmnt, model.AssignmentDate, incrementCvsrequired: model.NoOfCvs);
                            dbContext.PhJobAssignments.Update(jobAsgmnt);

                            dbContext.PhJobAssignmentHistories.Add(new PhJobAssignmentHistory
                            {
                                Joid = jobAsgmnt.Joid,
                                AssignedTo = jobAsgmnt.AssignedTo,
                                CreatedBy = jobAsgmnt.ReassignBy.Value,
                                CreatedDate = jobAsgmnt.ReassignDate.Value,
                                Status = (byte)RecordStatus.Active
                            });
                            await dbContext.SaveChangesAsync();

                            // Activity
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = model.JoId,
                                JobId = model.JoId,
                                ActivityType = (byte)LogActivityType.RecordUpdates,
                                ActivityDesc = " has deAssigned " + jobOpenings.JobTitle + " job to " + assign + " successfully",
                                UserId = loginUserId
                            };
                            activityList.Add(activityLog);
                        }
                        else
                        {
                            jobAsgmnt.UpdatedBy = loginUserId;
                            jobAsgmnt.UpdatedDate = CurrentTime;

                            if (model.IsReplacement == true)
                                PhJobAssignmentsDayWise_records(ref jobAsgmnt, model.AssignmentDate, updateCvsrequired: model.NoOfCvs);
                            else
                                PhJobAssignmentsDayWise_records(ref jobAsgmnt, model.AssignmentDate, incrementCvsrequired: model.NoOfCvs);

                            dbContext.PhJobAssignments.Update(jobAsgmnt);
                            await dbContext.SaveChangesAsync();

                            // Activity
                            var activityLog = new CreateActivityViewModel
                            {
                                ActivityMode = (byte)WorkflowActionMode.Opening,
                                ActivityOn = model.JoId,
                                JobId = model.JoId,
                                ActivityType = (byte)LogActivityType.RecordUpdates,
                                ActivityDesc = " has Increased CV to - " + jobOpenings.JobTitle + " job for " + assign + " successfully",
                                UserId = loginUserId
                            };
                            activityList.Add(activityLog);
                        }

                        var newStatusId = statusDtls.FirstOrDefault(x => x.Jscode == "NEW").Id;

                        if (jobOpenings.JobOpeningStatus == newStatusId) // Only New jobs
                        {
                            var WipStatusId = statusDtls.FirstOrDefault(x => x.Jscode == "WIP").Id;

                            jobOpenings.JobOpeningStatus = WipStatusId;
                            dbContext.PhJobOpenings.Update(jobOpenings);
                            await dbContext.SaveChangesAsync();
                        }

                        respModel.SetResult(string.Empty);
                    }
                    else
                    {
                        respModel.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Invalid job details", true);
                    }
                }
                else
                {
                    respModel.SetError(ApiResponseErrorCodes.InvalidUrlParameter, "Unauthorized recruiter details", true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, " model:" + Newtonsoft.Json.JsonConvert.SerializeObject(model), respModel.Meta.RequestID, ex);

                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }

        public async Task<GetResponseViewModel<TagJobModel>> GetJobsListToTagCandidate(TagJobListSearchViewModel tagJobListSearchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<TagJobModel>();

            try
            {
                tagJobListSearchViewModel.CurrentPage = (tagJobListSearchViewModel.CurrentPage.Value - 1) * tagJobListSearchViewModel.PerPage.Value;
                var dtls = await dbContext.GetJobsListToTagCandidate(tagJobListSearchViewModel.SearchKey, tagJobListSearchViewModel.CandidateId.Value,
          tagJobListSearchViewModel.PerPage, tagJobListSearchViewModel.CurrentPage);

                respModel.Status = true;
                respModel.SetResult(dtls);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(tagJobListSearchViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> ReOpenJob(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = "Re-Opened Successfully";
            try
            {

                var JobStatus = await dbContext.PhJobStatusSes.Where(x => x.Status != (byte)RecordStatus.Delete && x.Jscode == JobStatusCodes.RPN.ToString()).FirstOrDefaultAsync();
                if (JobStatus != null)
                {
                    int currentStatusId = 0;
                    var jobOpenings = dbContext.PhJobOpenings.Where(x => x.Id == Id).FirstOrDefault();
                    if (jobOpenings != null)
                    {

                        currentStatusId = jobOpenings.JobOpeningStatus;

                        jobOpenings.UpdatedBy = loginUserId;
                        jobOpenings.UpdatedDate = CurrentTime;
                        jobOpenings.JobOpeningStatus = JobStatus.Id;
                        jobOpenings.ReopenedDate = CurrentTime;

                        dbContext.PhJobOpenings.Update(jobOpenings);
                        await dbContext.SaveChangesAsync();

                        // assigning recruiters
                        var jobAssmtns = await dbContext.PhJobAssignments.Where(x => x.Joid == Id && x.DeassignDate != null).ToListAsync();
                        foreach (var jobAsgmnt in jobAssmtns)
                        {
                            jobAsgmnt.ReassignDate = CurrentTime;
                            jobAsgmnt.ReassignBy = loginUserId;
                            jobAsgmnt.DeassignDate = null;
                            jobAsgmnt.UpdatedBy = loginUserId;
                            jobAsgmnt.UpdatedDate = CurrentTime;

                            dbContext.PhJobAssignments.Update(jobAsgmnt);
                            dbContext.PhJobAssignmentHistories.Add(new PhJobAssignmentHistory
                            {
                                Joid = jobAsgmnt.Joid,
                                AssignedTo = jobAsgmnt.AssignedTo,
                                CreatedBy = jobAsgmnt.ReassignBy.Value,
                                CreatedDate = jobAsgmnt.ReassignDate.Value,
                                Status = (byte)RecordStatus.Active
                            });
                            await dbContext.SaveChangesAsync();
                        }

                        List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                        // audit 
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "The Job Reopened Successfully",
                            ActivityDesc = "The Job (" + jobOpenings.Id + ") Reopened Successfully",
                            ActivityType = (byte)AuditActivityType.StatusUpdates,
                            TaskID = jobOpenings.Id,
                            UserId = loginUserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Opening,
                            ActivityOn = jobOpenings.Id,
                            JobId = jobOpenings.Id,
                            ActivityType = (byte)LogActivityType.StatusUpdates,
                            ActivityDesc = " has updated to Job (" + jobOpenings.Id + ") Reopened Successfully",
                            UserId = loginUserId,
                            CurrentStatusId = currentStatusId,
                            UpdateStatusId = JobStatus.Id
                        };
                        activityList.Add(activityLog);
                        SaveActivity(activityList);

                        if (Usr.UserTypeId != (byte)UserType.Recruiter)
                        {
                            // Day Wise Actions 
                            var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == Id && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                            if (IsDayWiseAction == null)
                            {
                                IsDayWiseAction = new PhDayWiseJobAction
                                {
                                    CreatedBy = loginUserId,
                                    CreatedDate = CurrentTime,
                                    Joid = Id,
                                    JobStatus = true
                                };
                                dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                                dbContext.SaveChanges();
                            }
                            else
                            {
                                IsDayWiseAction.JobStatus = true;
                                dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                                dbContext.SaveChanges();
                            }
                        }


                        // Applying work flow conditions 
                        var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                        {
                            ActionMode = (byte)WorkflowActionMode.Other,
                            JobId = Id,
                            UserId = loginUserId,
                            TaskCode = TaskCode.REJB.ToString()
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
                                    CreatedBy = loginUserId
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
                        }
                        respModel.SetResult(message);
                        //}
                        //else
                        //{
                        //    message = "   Only jobs that have been posted using the Hire system can be Re-Opened. This position was published before the Gateway's migration. Please start over with a New Opening if necessary";
                        //    respModel.Status = false;
                        //    respModel.Meta.SetError(ApiResponseErrorCodes.UserPermissionNotGranted, message, true);
                        //}
                    }
                    else
                    {
                        message = "ReOpen status Id is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                }
                else
                {
                    respModel.Status = false;
                    message = "The Job is not available";
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
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> UpdateJobStatus(UpdateOpeningViewModel updateOpeningViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Status Updated Successfully";
            try
            {

                var jobOpenings = dbContext.PhJobOpenings.Where(x => x.Id == updateOpeningViewModel.JobId && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                if (jobOpenings != null)
                {
                    // applying work flow conditions 
                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    {
                        ActionMode = (byte)WorkflowActionMode.Opening,
                        JobId = updateOpeningViewModel.JobId,
                        UserId = UserId,
                        TaskCode = TaskCode.USU.ToString(),
                        CurrentStatusId = jobOpenings.JobOpeningStatus,
                        UpdateStatusId = updateOpeningViewModel.JobStatusId
                    };

                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        // Day Wise Actions 
                        var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == updateOpeningViewModel.JobId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                        if (IsDayWiseAction == null)
                        {
                            IsDayWiseAction = new PhDayWiseJobAction
                            {
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                Joid = updateOpeningViewModel.JobId,
                                JobStatus = true
                            };
                            dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            IsDayWiseAction.JobStatus = true;
                            dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
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
                    }
                    respModel.SetResult(message);
                }
                else
                {
                    respModel.Status = false;
                    message = "The Job is not available";
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(updateOpeningViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> HoldJob(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = "Updated Successfully";
            try
            {

                var JobStatus = await dbContext.PhJobStatusSes.Where(x => x.Status != (byte)RecordStatus.Delete && x.Jscode == JobStatusCodes.HLD.ToString()).FirstOrDefaultAsync();
                if (JobStatus != null)
                {
                    int CurrentStatusId = 0;
                    var jobOpenings = dbContext.PhJobOpenings.Where(x => x.Id == Id).FirstOrDefault();
                    if (jobOpenings != null)
                    {
                        CurrentStatusId = jobOpenings.JobOpeningStatus;

                        jobOpenings.UpdatedBy = UserId;
                        jobOpenings.UpdatedDate = CurrentTime;
                        jobOpenings.JobOpeningStatus = JobStatus.Id;

                        dbContext.PhJobOpenings.Update(jobOpenings);
                        await dbContext.SaveChangesAsync();

                        // deassing recruiters
                        var jobAssmtns = await dbContext.PhJobAssignments.Where(x => x.Joid == Id && x.DeassignDate == null
                        && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                        foreach (var jobAssmtn in jobAssmtns)
                        {
                            jobAssmtn.DeassignBy = UserId;
                            jobAssmtn.DeassignDate = CurrentTime;
                            jobAssmtn.UpdatedBy = UserId;
                            jobAssmtn.UpdatedDate = CurrentTime;

                            dbContext.PhJobAssignments.Update(jobAssmtn);
                            var jobAssmntHis = await dbContext.PhJobAssignmentHistories.FirstOrDefaultAsync(da => da.AssignedTo == jobAssmtn.AssignedTo && da.Joid == jobAssmtn.Joid && da.Status == (byte)RecordStatus.Active && da.DeassignDate.HasValue == false);
                            if (jobAssmntHis != null)
                            {
                                jobAssmntHis.DeassignDate = jobAssmtn.DeassignDate;
                                jobAssmntHis.DeassignBy = jobAssmtn.DeassignBy;
                                jobAssmntHis.UpdatedDate = jobAssmtn.UpdatedDate;
                                jobAssmntHis.UpdatedBy = jobAssmtn.UpdatedBy;
                            }
                            await dbContext.SaveChangesAsync();
                        }

                        List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                        // audit 
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Hold Job",
                            ActivityDesc = "Job update to Hold successfully",
                            ActivityType = (byte)AuditActivityType.StatusUpdates,
                            TaskID = jobOpenings.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Opening,
                            ActivityOn = jobOpenings.Id,
                            JobId = jobOpenings.Id,
                            ActivityType = (byte)LogActivityType.StatusUpdates,
                            ActivityDesc = " has update Job to Hold successfully",
                            UserId = UserId,
                            UpdateStatusId = CurrentStatusId,
                            CurrentStatusId = JobStatus.Id
                        };
                        activityList.Add(activityLog);
                        SaveActivity(activityList);

                        if (Usr.UserTypeId != (byte)UserType.Recruiter)
                        {
                            // Day Wise Actions 
                            var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == Id && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                            if (IsDayWiseAction == null)
                            {
                                IsDayWiseAction = new PhDayWiseJobAction
                                {
                                    CreatedBy = UserId,
                                    CreatedDate = CurrentTime,
                                    Joid = Id,
                                    JobStatus = true
                                };
                                dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                                dbContext.SaveChanges();
                            }
                            else
                            {
                                IsDayWiseAction.JobStatus = true;
                                dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                                dbContext.SaveChanges();
                            }
                        }


                        // Applying work flow conditions 
                        var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                        {
                            ActionMode = (byte)WorkflowActionMode.Other,
                            JobId = Id,
                            UserId = UserId,
                            TaskCode = TaskCode.HJB.ToString()
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
                            respModel.Status = true;
                        }
                        respModel.SetResult(message);
                    }
                    else
                    {
                        message = "The Job is not available";
                        respModel.Status = false;
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                    }
                }
                else
                {
                    respModel.Status = false;
                    message = "Hold status Id is not available";
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
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> CloseJob(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new UpdateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            string message = "Closed Successfully";
            try
            {

                var JobStatus = await dbContext.PhJobStatusSes.Select(x => new { x.Id, x.Title, x.Status, x.Jscode }).Where(x => x.Status != (byte)RecordStatus.Delete && x.Jscode == JobStatusCodes.CLS.ToString()).FirstOrDefaultAsync();
                if (JobStatus != null)
                {
                    int CurrentStatusId = 0;
                    var jobOpenings = dbContext.PhJobOpenings.Where(x => x.Id == Id).FirstOrDefault();
                    if (jobOpenings != null)
                    {
                        CurrentStatusId = jobOpenings.JobOpeningStatus;

                        jobOpenings.UpdatedBy = UserId;
                        jobOpenings.UpdatedDate = CurrentTime;
                        jobOpenings.JobOpeningStatus = JobStatus.Id;

                        dbContext.PhJobOpenings.Update(jobOpenings);
                        await dbContext.SaveChangesAsync();

                        // deassing recruiters
                        var jobAssmtns = await dbContext.PhJobAssignments.Where(x => x.Joid == Id && x.DeassignDate == null && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                        foreach (var jobAssmtn in jobAssmtns)
                        {
                            jobAssmtn.DeassignBy = UserId;
                            jobAssmtn.DeassignDate = CurrentTime;
                            jobAssmtn.UpdatedBy = UserId;
                            jobAssmtn.UpdatedDate = CurrentTime;

                            dbContext.PhJobAssignments.Update(jobAssmtn);
                            var jobAssmntHis = await dbContext.PhJobAssignmentHistories.FirstOrDefaultAsync(da => da.AssignedTo == jobAssmtn.AssignedTo && da.Joid == jobAssmtn.Joid && da.Status == (byte)RecordStatus.Active && da.DeassignDate.HasValue == false);
                            if (jobAssmntHis != null)
                            {
                                jobAssmntHis.DeassignDate = jobAssmtn.DeassignDate;
                                jobAssmntHis.DeassignBy = jobAssmtn.DeassignBy;
                                jobAssmntHis.UpdatedDate = jobAssmtn.UpdatedDate;
                                jobAssmntHis.UpdatedBy = jobAssmtn.UpdatedBy;
                            }
                            await dbContext.SaveChangesAsync();
                        }

                        List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                        List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                        // audit 
                        var auditLog = new CreateAuditViewModel
                        {
                            ActivitySubject = "Closed Job",
                            ActivityDesc = "Closed Job Successfully",
                            ActivityType = (byte)AuditActivityType.StatusUpdates,
                            TaskID = jobOpenings.Id,
                            UserId = UserId
                        };
                        audList.Add(auditLog);
                        SaveAuditLog(audList);

                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Opening,
                            ActivityOn = jobOpenings.Id,
                            JobId = jobOpenings.Id,
                            ActivityType = (byte)LogActivityType.StatusUpdates,
                            ActivityDesc = " has Job closed successfully",
                            UserId = UserId,
                            CurrentStatusId = CurrentStatusId,
                            UpdateStatusId = JobStatus.Id
                        };
                        activityList.Add(activityLog);
                        SaveActivity(activityList);

                        if (Usr.UserTypeId != (byte)UserType.Recruiter)
                        {
                            // Day Wise Actions 
                            var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == Id && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                            if (IsDayWiseAction == null)
                            {
                                IsDayWiseAction = new PhDayWiseJobAction
                                {
                                    CreatedBy = UserId,
                                    CreatedDate = CurrentTime,
                                    Joid = Id,
                                    JobStatus = true
                                };
                                dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                                dbContext.SaveChanges();
                            }
                            else
                            {
                                IsDayWiseAction.JobStatus = true;
                                dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                                dbContext.SaveChanges();
                            }
                        }



                        // Applying work flow conditions 
                        var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                        {
                            ActionMode = (byte)WorkflowActionMode.Other,
                            JobId = Id,
                            UserId = UserId,
                            TaskCode = TaskCode.CLJB.ToString()
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
                            respModel.Status = true;
                        }
                        respModel.SetResult(message);
                    }
                    else
                    {
                        respModel.Status = false;
                        message = "The Job is not available";
                        respModel.Meta.SetError(ApiResponseErrorCodes.ResourceAlreadyExist, message, true);
                    }
                }
                else
                {
                    message = "The Job Close status Id is not available";
                    respModel.Status = false;
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
            return Tuple.Create(notificationPushedViewModel, respModel);
        }

        public async Task<CreateResponseViewModel<CloneOpeningViewModel>> CloneJob(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<CloneOpeningViewModel>();
            //pending
            //string message = "Cloned Successfully";
            //using (var trans = await dbContext.Database.BeginTransactionAsync())
            //    try
            //    {

            //        var cloneOpeningViewModel = new CloneOpeningViewModel();
            //        var opening = dbContext.PhJobOpenings.Where(da => da.Id == Id).FirstOrDefault();
            //        var newJobeStatus = dbContext.PhJobStatusSes.FirstOrDefault(da => da.Jscode == JobStatusCodes.NEW.ToString()).Id;
            //        if (opening != null)
            //        {
            //            var opening_ = new PhJobOpening
            //            {
            //                BroughtBy = opening.BroughtBy,
            //                ClientId = opening.ClientId,
            //                ClientName = opening.ClientName,
            //                JobTitle = opening.JobTitle,
            //                CountryId = opening.CountryId,
            //                JobLocationId = opening.JobLocationId,
            //                CreatedDate = CurrentTime,
            //                JobRole = opening.JobRole,
            //                Status = (byte)RecordStatus.Active,
            //                Priority = opening.Priority,
            //                NoOfPositions = opening.NoOfPositions,
            //                JobDescription = opening.JobDescription,
            //                JobCategory = opening.JobCategory,
            //                KeyRequirements = string.Empty,
            //                ClosedDate = opening.ClosedDate,
            //                PostedDate = opening.PostedDate,
            //                JobOpeningStatus = newJobeStatus,
            //                CreatedByName = string.Empty,
            //                CreatedBy = UserId,
            //                MaxExpeInMonths = opening.MaxExpeInMonths,
            //                MinExpeInMonths = opening.MinExpeInMonths,
            //                Remarks = opening.Remarks,
            //                ShortJobDesc = opening.ShortJobDesc
            //            };
            //            dbContext.PhJobOpenings.Add(opening_);
            //            await dbContext.SaveChangesAsync();

            //            var openingDtls = dbContext.PhJobOpeningsAddlDetails.Where(da => da.Joid == Id).FirstOrDefault();
            //            if (openingDtls != null)
            //            {
            //                var openingDtls_ = new PhJobOpeningsAddlDetail
            //                {
            //                    ApprJoinDate = openingDtls.ApprJoinDate,
            //                    Buid = openingDtls.Buid,
            //                    ClientBilling = openingDtls.ClientBilling,
            //                    ClientReviewFlag = openingDtls.ClientReviewFlag,
            //                    CurrencyId = openingDtls.CurrencyId,
            //                    JobTenure = openingDtls.JobTenure,
            //                    Joid = opening_.Id,
            //                    MaxSalary = openingDtls.MaxSalary,
            //                    MinSalary = openingDtls.MinSalary,
            //                    NoOfCvsRequired = openingDtls.NoOfCvsRequired,
            //                    NoticePeriod = openingDtls.NoticePeriod,
            //                    Puid = openingDtls.Puid,
            //                    ReceivedDate = CurrentTime,
            //                    Spocid = openingDtls.Spocid
            //                };
            //                dbContext.PhJobOpeningsAddlDetails.Add(openingDtls_);
            //                await dbContext.SaveChangesAsync();
            //            }

            //            //skill set 

            //            var JobOpeningSkills = await dbContext.PhJobOpeningSkills.Where(da => da.Joid == Id && da.Status == (byte)RecordStatus.Active).ToListAsync();
            //            if (JobOpeningSkills.Count > 0)
            //            {
            //                foreach (var item in JobOpeningSkills)
            //                {
            //                    var phJobOpeningSkills = new PhJobOpeningSkill
            //                    {
            //                        CreatedBy = UserId,
            //                        CreatedDate = CurrentTime,
            //                        ExpMonth = item.ExpMonth,
            //                        ExpYears = item.ExpYears,
            //                        Joid = opening_.Id,
            //                        SkillName = item.SkillName,
            //                        Status = item.Status,
            //                        Technology = item.Technology,
            //                        TechnologyId = item.TechnologyId,
            //                        TotalExpeInMonths = item.TotalExpeInMonths
            //                    };
            //                    dbContext.PhJobOpeningSkills.Add(phJobOpeningSkills);
            //                    await dbContext.SaveChangesAsync();
            //                }
            //            }

            //            //preferences 
            //            var phJobOpeningPrefs = await dbContext.PhJobOpeningPrefs.Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
            //            if (phJobOpeningPrefs.Count > 0)
            //            {
            //                foreach (var item in phJobOpeningPrefs)
            //                {
            //                    var phJobOpeningPref = new PhJobOpeningPref
            //                    {
            //                        CreatedBy = UserId,
            //                        CreatedDate = CurrentTime,
            //                        DisplayFlag = item.DisplayFlag,
            //                        FieldCode = item.FieldCode,
            //                        Joid = opening_.Id,
            //                        Status = item.Status
            //                    };
            //                    dbContext.PhJobOpeningPrefs.Add(phJobOpeningPref);
            //                    await dbContext.SaveChangesAsync();
            //                }
            //            }

            //            // assessments
            //            var phJobOpeningAssmts = await dbContext.PhJobOpeningAssmts.Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
            //            if (phJobOpeningAssmts.Count > 0)
            //            {
            //                foreach (var item in phJobOpeningAssmts)
            //                {
            //                    var phJobOpeningAssmt = new PhJobOpeningAssmt
            //                    {
            //                        CandStatusId = item.CandStatusId,
            //                        Status = item.Status,
            //                        AssessmentId = item.AssessmentId,
            //                        CreatedBy = UserId,
            //                        CreatedDate = CurrentTime,
            //                        Joid = opening_.Id,
            //                        StageId = item.StageId
            //                    };
            //                    dbContext.PhJobOpeningAssmts.Add(phJobOpeningAssmt);
            //                    await dbContext.SaveChangesAsync();
            //                }
            //            }

            //            //qualifications
            //            var phJobReqdQualifications1 = await dbContext.PhJobReqdQualifications.Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
            //            foreach (var item in phJobReqdQualifications1)
            //            {
            //                var phJobReqdQualifications = new PhJobReqdQualification
            //                {
            //                    CourseId = item.CourseId,
            //                    QualificationId = item.QualificationId,
            //                    CreatedBy = UserId,
            //                    CreatedDate = CurrentTime,
            //                    Joid = opening.Id,
            //                    Status = (byte)RecordStatus.Active
            //                };
            //                dbContext.PhJobReqdQualifications.Add(phJobReqdQualifications);
            //                await dbContext.SaveChangesAsync();
            //            }


            //            // Certifications
            //            var phJobReqdCertifications1 = await dbContext.PhJobReqdCertifications.Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
            //            foreach (var item in phJobReqdCertifications1)
            //            {
            //                var phJobReqdCertifications = new PhJobReqdCertification
            //                {
            //                    CreatedBy = UserId,
            //                    CreatedDate = CurrentTime,
            //                    Joid = opening.Id,
            //                    Status = (byte)RecordStatus.Active,
            //                    CertificationId = item.CertificationId,
            //                    IsMandatory = false,
            //                };
            //                dbContext.PhJobReqdCertifications.Add(phJobReqdCertifications);
            //                await dbContext.SaveChangesAsync();
            //            }


            //            // Questions
            //            var phJobOpeningStQtns1 = await dbContext.PhJobOpeningStQtns.Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
            //            foreach (var item in phJobOpeningStQtns1)
            //            {
            //                var phJobOpeningStQtns = new PhJobOpeningStQtn
            //                {
            //                    CreatedBy = UserId,
            //                    CreatedDate = CurrentTime,
            //                    Joid = opening.Id,
            //                    Status = (byte)RecordStatus.Active,
            //                    IsMandatory = false,
            //                    QuestionSlno = item.QuestionSlno,
            //                    QuestionText = item.QuestionText,
            //                    QuestionType = item.QuestionType
            //                };
            //                dbContext.PhJobOpeningStQtns.Add(phJobOpeningStQtns);
            //                await dbContext.SaveChangesAsync();
            //            }

            //            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
            //            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
            //            // audit 
            //            var auditLog = new CreateAuditViewModel
            //            {
            //                ActivitySubject = "Cloned jobInfoModel",
            //                ActivityDesc = "Cloned jobInfoModel Succesfully",
            //                ActivityType = (byte)AuditActivityType.Other,
            //                TaskID = opening_.Id,
            //                UserId = UserId
            //            };
            //            audList.Add(auditLog);
            //            SaveAuditLog(audList);

            //            // activity
            //            var activityLog = new CreateActivityViewModel
            //            {
            //                ActivityMode = (byte)WorkflowActionMode.Opening,
            //                ActivityOn = opening_.Id,
            //                JobId = opening_.Id,
            //                ActivityType = (byte)LogActivityType.RecordUpdates,
            //                ActivityDesc = "has cloned job successfully",
            //                UserId = UserId
            //            };
            //            activityList.Add(activityLog);
            //            SaveActivity(activityList);

            //            if (Usr.UserTypeId != (byte)UserType.Recruiter)
            //            {
            //                // Day Wise Actions 
            //                var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(da => da.Joid == opening_.Id && da.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
            //                if (IsDayWiseAction == null)
            //                {
            //                    IsDayWiseAction = new PhDayWiseJobAction
            //                    {
            //                        CreatedBy = UserId,
            //                        CreatedDate = CurrentTime,
            //                        Joid = Id,
            //                        JobStatus = true
            //                    };
            //                    dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
            //                    dbContext.SaveChanges();
            //                }
            //                else
            //                {
            //                    IsDayWiseAction.JobStatus = true;
            //                    dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
            //                    dbContext.SaveChanges();
            //                }
            //            }


            //            cloneOpeningViewModel.Id = opening_.Id;
            //            respModel.Status = true;
            //            respModel.SetResult(cloneOpeningViewModel);
            //        }
            //        else
            //        {
            //            message = "Selected Opening is not available";
            //            respModel.Status = false;
            //            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
            //        }

            //        trans.Commit();
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

            //        respModel.Status = false;
            //        respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            //        respModel.Result = null;
            //        trans.Rollback();
            //    }
            return respModel;
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> AddMoreCVPerJob(MoreCVPerJobViewModel moreCVPerJobViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            try
            {


                var job = dbContext.PhJobOpenings.Where(x => x.Id == moreCVPerJobViewModel.JobId).FirstOrDefault();
                var jobOpenings = dbContext.PhJobOpeningsAddlDetails.Where(x => x.Joid == moreCVPerJobViewModel.JobId).FirstOrDefault();
                if (jobOpenings != null)
                {

                    jobOpenings.NoOfCvsRequired += moreCVPerJobViewModel.CvRequired;

                    dbContext.PhJobOpeningsAddlDetails.Update(jobOpenings);
                    await dbContext.SaveChangesAsync();

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " No.CVS Required for Job",
                        ActivityDesc = " No.CVS Required updated successfully",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = jobOpenings.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        ActivityOn = jobOpenings.Id,
                        JobId = jobOpenings.Id,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has Updated No.CVS (" + moreCVPerJobViewModel.CvRequired + ") for this job",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.SetResult(message);
                }
                else
                {
                    message = "Job is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(moreCVPerJobViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }



        public async Task<Tuple<List<NotificationPushedViewModel>, UpdateResponseViewModel<string>>> AddMoreCVPerJobRecruiter(MoreCVPerJobRecruiterViewModel moreCVPerJobViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            var respModel = new UpdateResponseViewModel<string>();
            string message = "Updated Successfully";
            try
            {


                var job = dbContext.PhJobOpenings.Where(x => x.Id == moreCVPerJobViewModel.JobId).FirstOrDefault();
                var jobOpenings = dbContext.PhJobOpeningsAddlDetails.Where(x => x.Joid == moreCVPerJobViewModel.JobId).FirstOrDefault();
                if (jobOpenings != null)
                {
                    var CVAssigntoTeamMembers = new List<CVAssigntoTeamMembers>();

                    int noCVS = 0;
                    foreach (var item in moreCVPerJobViewModel.CVAssigntoTeamMembers)
                    {
                        if (item.NoOfCvs > 0)
                        {
                            var CVAssigntoTeamMember = new CVAssigntoTeamMembers
                            {
                                UserId = item.UserId,
                                NoOfCvs = item.NoOfCvs
                            };
                            CVAssigntoTeamMembers.Add(CVAssigntoTeamMember);

                            jobOpenings.NoOfCvsRequired += item.NoOfCvs;
                            noCVS += item.NoOfCvs;

                            var jobAssmtns = await dbContext.PhJobAssignments.Where(x => x.Joid == moreCVPerJobViewModel.JobId && x.AssignedTo == item.UserId).FirstOrDefaultAsync();

                            if (jobAssmtns != null)
                            {
                                jobAssmtns.NoCvsrequired += item.NoOfCvs;
                                jobAssmtns.CvTarget = item.NoOfCvs;
                                jobAssmtns.UpdatedBy = UserId;
                                jobAssmtns.UpdatedDate = CurrentTime;
                                jobAssmtns.AssignBy = 1;
                                jobAssmtns.CvTargetDate = CurrentTime;

                                PhJobAssignmentsDayWise_records(ref jobAssmtns, CurrentTime, incrementCvsrequired: item.NoOfCvs);

                                dbContext.PhJobAssignments.Update(jobAssmtns);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }


                    dbContext.PhJobOpeningsAddlDetails.Update(jobOpenings);
                    await dbContext.SaveChangesAsync();

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " No.CVS Required for Job",
                        ActivityDesc = " No.CVS Required updated successfully",
                        ActivityType = (byte)AuditActivityType.StatusUpdates,
                        TaskID = jobOpenings.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        ActivityOn = jobOpenings.Id,
                        JobId = jobOpenings.Id,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has Updated No.CVS (" + noCVS + ") for this job",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    if (CVAssigntoTeamMembers.Count > 0)
                    {
                        // Applying work flow conditions 
                        var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                        {
                            ActionMode = (byte)WorkflowActionMode.Other,
                            JobId = moreCVPerJobViewModel.JobId,
                            TaskCode = TaskCode.MCV.ToString(),
                            UserId = UserId,
                            AssignTo_CV = CVAssigntoTeamMembers,
                            NoOfCvs = noCVS
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
                            respModel.Status = true;
                        }
                    }
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Job is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.UpdateItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(moreCVPerJobViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }




        #region Notes 


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> AddJobNote(CreateJobNotesViewModel createJobNotesViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            var respModel = new CreateResponseViewModel<string>();
            string message = "Created Successfully";
            try
            {


                var jobOpenings = dbContext.PhJobOpenings.Where(x => x.Id == createJobNotesViewModel.JobId).FirstOrDefault();
                if (jobOpenings != null)
                {
                    var note = new PhNote()
                    {
                        CreatedBy = UserId,
                        CreatedDate = CurrentTime,
                        Status = (byte)RecordStatus.Active,
                        Joid = createJobNotesViewModel.JobId,
                        NoteId = createJobNotesViewModel.NoteId,
                        NotesDesc = createJobNotesViewModel.NotesDesc,
                        Title = createJobNotesViewModel.Title,
                        CandId = createJobNotesViewModel.CandId
                    };
                    dbContext.PhNotes.Add(note);
                    await dbContext.SaveChangesAsync();

                    if (createJobNotesViewModel.TagId.Count > 0)
                    {
                        foreach (var item in createJobNotesViewModel.TagId)
                        {
                            if (item != 0)
                            {
                                var sendList = new PhNotesSendList()
                                {
                                    NotesId = note.Id,
                                    SendTo = item,
                                    Status = (byte)RecordStatus.Active
                                };

                                dbContext.PhNotesSendLists.Add(sendList);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }

                    // Applying work flow conditions 
                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                    {
                        ActionMode = (byte)WorkflowActionMode.Other,
                        CanProfId = createJobNotesViewModel.CandId,
                        JobId = createJobNotesViewModel.JobId,
                        TaskCode = TaskCode.SNT.ToString(),
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


                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    string CanNote = string.Empty;
                    if (createJobNotesViewModel.CandId != 0)
                    {
                        CanNote = "- " + createJobNotesViewModel.CandId;
                    }

                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        // Day Wise Actions 
                        var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == createJobNotesViewModel.JobId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                        if (IsDayWiseAction == null)
                        {
                            IsDayWiseAction = new PhDayWiseJobAction
                            {
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                Joid = createJobNotesViewModel.JobId,
                                Note = true
                            };
                            dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            IsDayWiseAction.Note = true;
                            dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                    }



                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = " Note Created",
                        ActivityDesc = "  has created a Note -" + jobOpenings.Id + " " + CanNote,
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = note.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Candidate,
                        ActivityOn = jobOpenings.Id,
                        JobId = jobOpenings.Id,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = "  has created a Note -" + jobOpenings.Id + " " + CanNote,
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Job is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(createJobNotesViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }


        public async Task<GetResponseViewModel<JobNotesViewModel>> GetJobNotes(int JobId, int CandId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<JobNotesViewModel>();
            try
            {


                var respNotes = new JobNotesViewModel();
                var parentNotes = new List<NotesViewModel>();
                var noteCreatedBy = new List<NoteCreatedBy>();

                List<int> noteUsersId = null;
                List<int> noteId = null;
                List<int> phNotesSendList = null;
                List<int> phNotesSendAllList = null;

                var AllNotes = await dbContext.GeJobNoteList(JobId);
                AllNotes = AllNotes.Where(x => x.CandId == CandId).ToList();

                noteUsersId = AllNotes.Select(x => x.CreatedBy.Value).ToList();
                noteId = AllNotes.Select(x => x.Id).ToList();

                var groupByUsers = AllNotes.GroupBy(x => x.CreatedBy).Select(grp => grp.First()).ToList();
                foreach (var item in groupByUsers)
                {
                    noteCreatedBy.Add(new NoteCreatedBy
                    {
                        Name = item.CreatedByName,
                        UserId = item.CreatedBy,
                        Role = item.Role,
                        ProfilePhoto = item.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + item.CreatedBy + "/ProfilePhoto/" + item.ProfilePhoto : string.Empty
                    });
                }

                parentNotes = (from nte in AllNotes
                               where nte.NoteId == 0
                               select new NotesViewModel
                               {
                                   Id = nte.Id,
                                   CreatedBy = nte.CreatedBy,
                                   CreatedByName = nte.CreatedByName,
                                   JobId = nte.Joid,
                                   NoteId = nte.NoteId,
                                   TimeDiff = string.Empty,
                                   NotesDesc = nte.NotesDesc,
                                   Status = nte.Status,
                                   CreatedDate = nte.CreatedDate,
                                   Title = nte.Title,
                                   ProfilePhoto = nte.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + nte.CreatedBy + "/ProfilePhoto/" + nte.ProfilePhoto : string.Empty
                               }).OrderByDescending(x => x.CreatedDate).ToList();

                var dtls = dbContext.PhNotesSendLists.Where(x => noteId.Contains(x.NotesId) && x.Status != 5).ToList();
                if (Usr.UserTypeId == (byte)UserType.Recruiter)
                {
                    phNotesSendAllList = dtls.Where(x => x.SendTo != UserId).Select(x => x.NotesId).ToList();
                    phNotesSendList = dtls.Where(x => x.SendTo == UserId).Select(x => x.NotesId).ToList();

                    phNotesSendAllList.RemoveAll(r => phNotesSendList.Any(a => a == r));
                    parentNotes = parentNotes.Where(x => x.CreatedBy == UserId || !phNotesSendAllList.Contains(x.Id)).ToList();
                }
                foreach (var item in parentNotes)
                {
                    item.TimeDiff = GetTimeDiff(item.CreatedDate);
                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        item.NotesReplyViewModel = (from nte in AllNotes
                                                    where nte.NoteId == item.Id
                                                    select new NotesReplyViewModel
                                                    {
                                                        Id = nte.Id,
                                                        CreatedBy = nte.CreatedBy,
                                                        CreatedByName = nte.CreatedByName,
                                                        NotesDesc = nte.NotesDesc,
                                                        CreatedDate = nte.CreatedDate,
                                                        Status = nte.Status,
                                                        Title = nte.Title,
                                                        ProfilePhoto = nte.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + nte.CreatedBy + "/ProfilePhoto/" + nte.ProfilePhoto : string.Empty
                                                    }).OrderByDescending(x => x.CreatedDate).ToList();
                    }
                    else
                    {
                        item.NotesReplyViewModel = (from nte in AllNotes
                                                    where nte.NoteId == item.Id && (nte.CreatedBy == UserId || !phNotesSendAllList.Contains(nte.Id))
                                                    select new NotesReplyViewModel
                                                    {
                                                        Id = nte.Id,
                                                        CreatedBy = nte.CreatedBy,
                                                        CreatedByName = nte.CreatedByName,
                                                        NotesDesc = nte.NotesDesc,
                                                        CreatedDate = nte.CreatedDate,
                                                        Status = nte.Status,
                                                        Title = nte.Title,
                                                        ProfilePhoto = nte.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + nte.CreatedBy + "/ProfilePhoto/" + nte.ProfilePhoto : string.Empty
                                                    }).OrderByDescending(x => x.CreatedDate).ToList();
                    }
                    foreach (var item1 in item.NotesReplyViewModel)
                    {
                        item1.TimeDiff = GetTimeDiff(item1.CreatedDate);
                    }
                }

                respNotes.NoteCreatedBy = noteCreatedBy;
                respNotes.NotesViewModel = parentNotes;

                respModel.Status = true;
                respModel.SetResult(respNotes);
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

        public async Task<DeleteResponseViewModel<string>> DeleteJobNote(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new DeleteResponseViewModel<string>();
            string message = "Deleted Successfully";
            try
            {
                //logger.Log(LogLevel.Debug, LoggingEvents.DeleteItem, "Start of method:", respModel.Meta.RequestID);
                List<int> noteIds = new List<int>();
                var PhNote = await dbContext.PhNotes.Where(x => x.Id == Id && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                if (PhNote != null)
                {
                    PhNote.UpdatedBy = UserId;
                    PhNote.UpdatedDate = CurrentTime;
                    PhNote.Status = (byte)RecordStatus.Delete;
                    noteIds.Add(PhNote.Id); // reference delete

                    dbContext.PhNotes.Update(PhNote);
                    if (PhNote.NoteId == 0)
                    {
                        var PhNotes = await dbContext.PhNotes.Where(x => x.NoteId == PhNote.Id && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                        foreach (var item in PhNotes)
                        {
                            item.UpdatedBy = UserId;
                            item.UpdatedDate = CurrentTime;
                            item.Status = (byte)RecordStatus.Delete;

                            noteIds.Add(item.Id); // reference delete
                            dbContext.PhNotes.Update(item);
                        }

                    }

                    var SendToList = await dbContext.PhNotesSendLists.Where(x => noteIds.Contains(x.NotesId) && x.Status != (byte)RecordStatus.Delete).ToListAsync();
                    foreach (var item in SendToList)
                    {
                        item.Status = (byte)RecordStatus.Delete;
                        dbContext.PhNotesSendLists.Update(item);
                    }

                    await dbContext.SaveChangesAsync();

                    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
                    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Deleted Note",
                        ActivityDesc = "  has deleted a Note " + PhNote.Joid + "",
                        ActivityType = (byte)AuditActivityType.Critical,
                        TaskID = PhNote.Id,
                        UserId = UserId
                    };
                    audList.Add(auditLog);
                    SaveAuditLog(audList);

                    // activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        ActivityOn = PhNote.Joid,
                        JobId = PhNote.Joid,
                        ActivityType = (byte)LogActivityType.Critical,
                        ActivityDesc = "  has deleted a Note " + PhNote.Joid + "",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);
                    SaveActivity(activityList);

                    respModel.Status = true;
                    respModel.SetResult(message);
                }
                else
                {
                    message = "Note is not found";
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


        #region Job View
        public async Task<GetResponseViewModel<JobInfoViewModel>> GetJobInfo(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<JobInfoViewModel>();
            try
            {
                var dbModel_jobOpening = await dbContext.PhJobOpenings.AsNoTracking().Where(da => da.Id == Id && da.Status == (byte)RecordStatus.Active).FirstOrDefaultAsync();
                if (dbModel_jobOpening != null)
                {
                    var dbModel_jobOpeningAddlDtls = await dbContext.PhJobOpeningsAddlDetails.AsNoTracking().FirstOrDefaultAsync(da => da.Joid == Id);
                    var dbModel_jobOpeningDesirables = await dbContext.PhJobOpeningsDesirables.AsNoTracking().FirstOrDefaultAsync(da => da.Joid == Id);
                    var dbModel_jobOpeningDesirableSkills = await dbContext.PhJobOpeningsDesirableSkills.AsNoTracking().Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete)
                                                               .Select(da => new { da.TechnologyId, da.PreferenceType, da.GroupType }).ToListAsync();
                    var techIds = dbModel_jobOpeningDesirableSkills.Select(da => da.TechnologyId).ToHashSet();
                    var techNames = await dbContext.PhTechnologysSes.Where(da => techIds.Contains(da.Id)).Select(da => new { da.Id, da.Title }).ToListAsync();


                    var dbModel_jobOpeningSkills = await dbContext.PhJobOpeningSkills.AsNoTracking().Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete).ToListAsync();
                    //certification 
                    var dbModel_jobOpeningCertifications = await dbContext.PhJobOpeningsCertifications.AsNoTracking().Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete)
                                                            .Select(da => new { da.CertificationId, da.GroupType, da.PreferenceType }).ToListAsync();
                    //Qualification
                    var dbModel_jobOpeningQualifications = await dbContext.PhJobOpeningsQualifications.AsNoTracking().Where(da => da.Joid == Id && da.Status != (byte)RecordStatus.Delete)
                                                            .Select(da => new { da.QualificationId, da.CourseId, da.GroupType, da.PreferenceType }).ToListAsync();
                    var refIds = dbModel_jobOpeningCertifications.Where(da => da.CertificationId > 0).Select(da => da.CertificationId).ToList();
                    refIds.AddRange(dbModel_jobOpeningQualifications.Where(da => da.QualificationId > 0).Select(da => da.QualificationId).ToList());
                    refIds.AddRange(dbModel_jobOpeningQualifications.Where(da => da.CourseId > 0).Select(da => da.CourseId.Value).ToList());
                    var _refIds = refIds.ToHashSet();
                    var refName = await dbContext.PhRefMasters.Where(da => _refIds.Contains(da.Id)).Select(da => new { da.Id, da.Rmvalue }).ToListAsync();

                    var jobInfoModel = new JobInfoViewModel()
                    {
                        JobId = dbModel_jobOpening.Id,
                        JobRole = dbModel_jobOpening.JobRole,
                        Country = dbModel_jobOpening.CountryId > 0 ? dbContext.PhCountries.Where(da => da.Id == dbModel_jobOpening.CountryId).Select(da => da.Nicename).FirstOrDefault() : string.Empty,
                        City = dbModel_jobOpening.JobLocationId > 0 ? dbContext.PhCities.Where(da => da.Id == dbModel_jobOpening.JobLocationId).Select(da => da.Name).FirstOrDefault() : string.Empty,
                        JobDescription = dbModel_jobOpening.JobDescription,
                        JobCategory = dbModel_jobOpening.JobCategory,
                        JobCategoryId = dbModel_jobOpening.JobCategoryId,


                        JobDesirableSpecializations = new List<JobInfoViewModel_KnowledgePref>(),
                        JobDesirableImplementations = new List<JobInfoViewModel_KnowledgePref>(),
                        JobDesirableDesigns = new List<JobInfoViewModel_KnowledgePref>(),
                        JobDesirableDevelopments = new List<JobInfoViewModel_KnowledgePref>(),
                        JobDesirableSupports = new List<JobInfoViewModel_KnowledgePref>(),
                        JobDesirableQualities = new List<JobInfoViewModel_KnowledgePref>(),
                        JobDesirableDocumentations = new List<JobInfoViewModel_KnowledgePref>(),

                        OpeningSkillSet = new List<JobInfoViewModel_SkillSet>(),

                        CandidateEducationCertifications = new List<JobInfoViewModel_PrefTypeWithName<int>>(),
                        OpeningCertifications = new List<JobInfoViewModel_PrefTypeWithName<int>>(),
                        CandidateEducationQualifications = new List<JobInfoViewModel_CandidateEducationQualification>(),
                        OpeningQualifications = new List<JobInfoViewModel_PrefTypeWithName<int>>(),
                    };
                    if ((dbModel_jobOpeningDesirables?.CandidateValidPassport).HasValue && (dbModel_jobOpeningDesirables?.CandidateValidPassportPrefType).HasValue)
                        jobInfoModel.CandidatePrefValidPassport = ToModel(dbModel_jobOpeningDesirables?.CandidateValidPassportPrefType);
                    if ((dbModel_jobOpeningDesirables?.CandidateDOB).HasValue && (dbModel_jobOpeningDesirables?.CandidateDOBPrefType).HasValue)
                        jobInfoModel.CandidatePrefDOB = ToModel(dbModel_jobOpeningDesirables?.CandidateDOBPrefType);
                    if ((dbModel_jobOpeningDesirables?.CandidateGender).HasValue && (dbModel_jobOpeningDesirables?.CandidateGenderPrefType).HasValue)
                        jobInfoModel.CandidatePrefGender = ToModel(dbModel_jobOpeningDesirables?.CandidateGenderPrefType);
                    if ((dbModel_jobOpeningDesirables?.CandidateMaritalStatus).HasValue && (dbModel_jobOpeningDesirables?.CandidateMaritalStatusPrefType).HasValue)
                        jobInfoModel.CandidatePrefMaritalStatus = ToModel(dbModel_jobOpeningDesirables?.CandidateMaritalStatusPrefType);
                    jobInfoModel.CandidatePrefRegion = ToCandidateModel(dbModel_jobOpeningDesirables?.CandidateRegion, dbModel_jobOpeningDesirables?.CandidateRegionPrefType);
                    if ((dbModel_jobOpeningDesirables?.CandidateNationality).HasValue && (dbModel_jobOpeningDesirables?.CandidateNationalityPrefType).HasValue)
                        jobInfoModel.CandidatePrefNationality = ToModel(dbModel_jobOpeningDesirables?.CandidateNationalityPrefType);
                    if ((dbModel_jobOpeningDesirables?.CandidateResidingCountry).HasValue && (dbModel_jobOpeningDesirables?.CandidateResidingCountryPrefType).HasValue)
                        jobInfoModel.CandidatePrefResidingCountry = ToModel(dbModel_jobOpeningDesirables?.CandidateResidingCountryPrefType);
                    if ((dbModel_jobOpeningDesirables?.CandidateResidingCity).HasValue && (dbModel_jobOpeningDesirables?.CandidateResidingCityPrefType).HasValue)
                        jobInfoModel.CandidatePrefResidingCity = ToModel(dbModel_jobOpeningDesirables?.CandidateResidingCityPrefType);
                    if ((dbModel_jobOpeningDesirables?.CandidateDrivingLicence).HasValue && (dbModel_jobOpeningDesirables?.CandidateDrivingLicencePrefType).HasValue)
                        jobInfoModel.CandidatePrefDrivingLicence = ToModel(dbModel_jobOpeningDesirables?.CandidateDrivingLicencePrefType);
                    jobInfoModel.JobDesirableDomain = ToCandidateModel(dbModel_jobOpeningDesirables?.JobDomain, dbModel_jobOpeningDesirables?.JobDomainPrefType);

                    jobInfoModel.CandidatePrefLanguage = ToCandidateModel(dbModel_jobOpeningDesirables?.CandidateLanguage, dbModel_jobOpeningDesirables?.CandidateLanguagePrefType);
                    if ((dbModel_jobOpeningDesirables?.CandidateVisaPreference).HasValue && (dbModel_jobOpeningDesirables?.CandidateVisaPreferencePrefType).HasValue)
                        jobInfoModel.CandidatePrefVisaPreference = ToModel(dbModel_jobOpeningDesirables?.CandidateVisaPreferencePrefType);

                    if ((dbModel_jobOpeningAddlDtls?.JobTenure).HasValue)
                        jobInfoModel.JobTenure = dbModel_jobOpeningAddlDtls?.JobTenure;
                    jobInfoModel.JobWorkPattern = ToCandidateModel(dbModel_jobOpeningAddlDtls?.JobWorkPattern, dbModel_jobOpeningAddlDtls?.JobWorkPatternPrefTyp);
                    jobInfoModel.JobDesirableTeamRole = ToCandidateModel(dbModel_jobOpeningDesirables?.JobTeamRole, dbModel_jobOpeningDesirables?.JobTeamRolePrefType);

                    if ((dbModel_jobOpening?.MaxExpeInMonths).HasValue && (dbModel_jobOpening?.MinExpeInMonths).HasValue && (dbModel_jobOpening?.ExpeInMonthsPrefTyp).HasValue)
                        jobInfoModel.ExpeInMonthsPrefTyp = ToModel(dbModel_jobOpening?.ExpeInMonthsPrefTyp);
                    if ((dbModel_jobOpening?.MaxReleventExpInMonths).HasValue && (dbModel_jobOpening?.MinReleventExpInMonths).HasValue && (dbModel_jobOpening?.ReleventExpInMonthsPrefTyp).HasValue)
                        jobInfoModel.ReleventExpInMonthsPrefTyp = ToModel(dbModel_jobOpening?.ReleventExpInMonthsPrefTyp);


                    if ((dbModel_jobOpeningDesirables?.CandidatePaySlp).HasValue && (dbModel_jobOpeningDesirables?.CandidatePaySlpPrefType).HasValue)
                        jobInfoModel.CandidatePrefPaySlp = ToModel(dbModel_jobOpeningDesirables?.CandidatePaySlpPrefType);

                    //question of notice period and employee status removed no-preference
                    if ((dbModel_jobOpeningDesirables?.CandidateEmployeeStatus).HasValue && (dbModel_jobOpeningDesirables?.CandidateEmployeeStatusPrefType).HasValue)
                        jobInfoModel.CandidatePrefEmployeeStatus = ToModel(dbModel_jobOpeningDesirables?.CandidateEmployeeStatusPrefType);
                    if ((dbModel_jobOpeningDesirables?.CandidateNoticePeriod).HasValue && (dbModel_jobOpeningDesirables?.CandidateNoticePeriodPrefType).HasValue)
                        jobInfoModel.CandidatePrefNoticePeriod = ToModel(dbModel_jobOpeningDesirables?.CandidateNoticePeriodPrefType);
                    //not required
                    //jobInfoModel.NoticePeriod = ToModel(dbModel_jobOpeningAddlDtls?.NoticePeriod, dbModel_jobOpeningAddlDtls?.NoticePeriodPrefTyp),

                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Specializations))
                    {
                        jobInfoModel.JobDesirableSpecializations.Add(new JobInfoViewModel_KnowledgePref
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            Technology = techNames.Where(da => da.Id == dbModel_jobOpeningDesirableSkill.TechnologyId).Select(da => da.Title).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Implementations))
                    {
                        jobInfoModel.JobDesirableImplementations.Add(new JobInfoViewModel_KnowledgePref
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            Technology = techNames.Where(da => da.Id == dbModel_jobOpeningDesirableSkill.TechnologyId).Select(da => da.Title).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Designs))
                    {
                        jobInfoModel.JobDesirableDesigns.Add(new JobInfoViewModel_KnowledgePref
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            Technology = techNames.Where(da => da.Id == dbModel_jobOpeningDesirableSkill.TechnologyId).Select(da => da.Title).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Developments))
                    {
                        jobInfoModel.JobDesirableDevelopments.Add(new JobInfoViewModel_KnowledgePref
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            Technology = techNames.Where(da => da.Id == dbModel_jobOpeningDesirableSkill.TechnologyId).Select(da => da.Title).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Supports))
                    {
                        jobInfoModel.JobDesirableSupports.Add(new JobInfoViewModel_KnowledgePref
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            Technology = techNames.Where(da => da.Id == dbModel_jobOpeningDesirableSkill.TechnologyId).Select(da => da.Title).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Qualities))
                    {
                        jobInfoModel.JobDesirableQualities.Add(new JobInfoViewModel_KnowledgePref
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            Technology = techNames.Where(da => da.Id == dbModel_jobOpeningDesirableSkill.TechnologyId).Select(da => da.Title).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    foreach (var dbModel_jobOpeningDesirableSkill in dbModel_jobOpeningDesirableSkills.Where(da => da.GroupType == (byte)JobDesirableSkillGroupTypes.Documentations))
                    {
                        jobInfoModel.JobDesirableDocumentations.Add(new JobInfoViewModel_KnowledgePref
                        {
                            TechnologyId = dbModel_jobOpeningDesirableSkill.TechnologyId,
                            Technology = techNames.Where(da => da.Id == dbModel_jobOpeningDesirableSkill.TechnologyId).Select(da => da.Title).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningDesirableSkill.PreferenceType),
                        });
                    }
                    //Skill set 
                    foreach (var dbModel_jobOpeningSkill in dbModel_jobOpeningSkills)
                    {
                        jobInfoModel.OpeningSkillSet.Add(new JobInfoViewModel_SkillSet
                        {
                            TechnologyId = dbModel_jobOpeningSkill.TechnologyId,
                            Technology = dbModel_jobOpeningSkill.Technology,
                            PreferenceType = ToModel(dbModel_jobOpeningSkill.PreferenceType),
                        });
                    }

                    //Candidate Education Qualification
                    foreach (var dbModel_jobOpeningQualification in dbModel_jobOpeningQualifications.Where(da => da.GroupType == (byte)JobCandQualCertGroupTypes.JobCandiateEducationQualification))
                    {
                        jobInfoModel.CandidateEducationQualifications.Add(new JobInfoViewModel_CandidateEducationQualification
                        {
                            Qualification = dbModel_jobOpeningQualification.QualificationId,
                            QualificationName = refName.Where(da => da.Id == dbModel_jobOpeningQualification.QualificationId).Select(da => da.Rmvalue).FirstOrDefault(),
                            Course = dbModel_jobOpeningQualification.CourseId ?? 0,
                            CourseName = refName.Where(da => da.Id == dbModel_jobOpeningQualification.CourseId).Select(da => da.Rmvalue).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningQualification.PreferenceType),
                        });
                    }
                    //Candidate Education Certification
                    foreach (var dbModel_jobOpeningCertification in dbModel_jobOpeningCertifications.Where(da => da.GroupType == (byte)JobCandQualCertGroupTypes.JobCandidateEducationCertifications))
                    {
                        jobInfoModel.CandidateEducationCertifications.Add(new JobInfoViewModel_PrefTypeWithName<int>
                        {
                            Value = dbModel_jobOpeningCertification.CertificationId,
                            ValueName = refName.Where(da => da.Id == dbModel_jobOpeningCertification.CertificationId).Select(da => da.Rmvalue).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningCertification.PreferenceType),
                        });
                    }
                    //Opening Certification
                    foreach (var dbModel_jobOpeningCertification in dbModel_jobOpeningCertifications.Where(da => da.GroupType == (byte)JobCandQualCertGroupTypes.JobOpeningCertification))
                    {
                        jobInfoModel.OpeningCertifications.Add(new JobInfoViewModel_PrefTypeWithName<int>
                        {
                            Value = dbModel_jobOpeningCertification.CertificationId,
                            ValueName = refName.Where(da => da.Id == dbModel_jobOpeningCertification.CertificationId).Select(da => da.Rmvalue).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningCertification.PreferenceType),
                        });
                    }
                    //Opening Qualification
                    foreach (var dbModel_jobOpeningQualification in dbModel_jobOpeningQualifications.Where(da => da.GroupType == (byte)JobCandQualCertGroupTypes.JobOpeningQualification))
                    {
                        jobInfoModel.OpeningQualifications.Add(new JobInfoViewModel_PrefTypeWithName<int>
                        {
                            Value = dbModel_jobOpeningQualification.QualificationId,
                            ValueName = refName.Where(da => da.Id == dbModel_jobOpeningQualification.QualificationId).Select(da => da.Rmvalue).FirstOrDefault(),
                            PreferenceType = ToModel(dbModel_jobOpeningQualification.PreferenceType),
                        });
                    }


                    if ((dbModel_jobOpeningDesirables?.CandidateResume).HasValue && (dbModel_jobOpeningDesirables?.CandidateResumePrefType).HasValue)
                        jobInfoModel.CandidatePrefResume = ToModel(dbModel_jobOpeningDesirables?.CandidateResumePrefType);
                    if ((dbModel_jobOpeningDesirables?.CandidateVidPrfl).HasValue && (dbModel_jobOpeningDesirables?.CandidateVidPrflPrefType).HasValue)
                        jobInfoModel.CandidatePrefVidPrfl = ToModel(dbModel_jobOpeningDesirables?.CandidateVidPrflPrefType);

                    respModel.SetResult(jobInfoModel);
                }
                else
                {
                    string message = "Job is not available";
                    respModel.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.GetItem, $"Id:{Id}", respModel.Meta.RequestID, ex);
                respModel.SetError(ApiResponseErrorCodes.Exception, string.Empty);
            }
            return respModel;
        }


        public async Task<GetResponseViewModel<JobDescriptionViewModel>> GetJobDescription(int Id)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            byte Usertype = Usr.UserTypeId;
            int? EmpId = Usr.EmpId;
            var respModel = new GetResponseViewModel<JobDescriptionViewModel>();
            try
            {

                var Job = new JobDescriptionViewModel();
                bool AddCadidate = true;
                Job = await (from opn in dbContext.PhJobOpenings
                             join Stus in dbContext.PhJobStatusSes on opn.JobOpeningStatus equals Stus.Id
                             join opnDtls in dbContext.PhJobOpeningsAddlDetails on opn.Id equals opnDtls.Joid
                             join cunty in dbContext.PhCountries on opn.CountryId equals cunty.Id
                             join user in dbContext.PiHireUsers on opn.CreatedBy equals user.Id
                             join cur in dbContext.PhRefMasters on opnDtls.CurrencyId equals cur.Id into ps
                             from cur in ps.DefaultIfEmpty()
                             join tnr in dbContext.PhRefMasters on opnDtls.JobTenure equals tnr.Id into tnr_
                             from tnr in tnr_.DefaultIfEmpty()
                             join ct in dbContext.PhCities on opn.JobLocationId equals ct.Id into pp
                             from ct in pp.DefaultIfEmpty()
                             join brt in dbContext.PiHireUsers on opn.BroughtBy equals brt.Id into pr
                             from brt in pr.DefaultIfEmpty()
                             where opn.Id == Id
                             select new JobDescriptionViewModel
                             {
                                 CreatedByName = user.FirstName + " " + user.LastName,
                                 CreatedByProfilePhoto = user.ProfilePhoto,
                                 BroughtByProfilePhoto = brt.ProfilePhoto,
                                 CreatedByRole = user.UserRoleName,
                                 NoticePeriod = opnDtls.NoticePeriod,
                                 JobTenure = tnr.Rmvalue,//opnDtls.JobTenure,
                                 Country = cunty.Nicename,
                                 ClientName = opn.ClientName,
                                 ClientId = opn.ClientId,
                                 SpocId = opnDtls.Spocid,
                                 CreatedDate = opn.CreatedDate,
                                 JobRole = opn.JobRole,
                                 CurrencyId = opnDtls.CurrencyId,
                                 CurrencyName = cur.Rmvalue,
                                 JobId = opn.Id,
                                 JobTitle = opn.JobTitle,
                                 JobStatus = opn.JobOpeningStatus,
                                 JobStatusCode = Stus.Jscode,
                                 JobStatusName = Stus.Title,
                                 NoOfPositions = opn.NoOfPositions,
                                 NoOfCvsRequired = opnDtls.NoOfCvsRequired,
                                 TargetDate = opn.ClosedDate,
                                 JoiningDate = opnDtls.ApprJoinDate,
                                 StartDate = opn.PostedDate,
                                 MaxSalary = opnDtls.MaxSalary,
                                 MinSalary = opnDtls.MinSalary,
                                 EmploymentType = opn.JobCategory,
                                 JobDescription = opn.JobDescription,
                                 ShortJobDesc = opn.ShortJobDesc,
                                 MaxExpeInMonths = opn.MaxExpeInMonths,
                                 MinExpeInMonths = opn.MinExpeInMonths,
                                 BroughtBy = opn.BroughtBy,
                                 BroughtByName = brt.FirstName + " " + brt.LastName,
                                 BroughtByRole = brt.UserRoleName,
                                 AccountManagerName = string.Empty,
                                 City = ct.Name,
                                 PuId = opnDtls.Puid,
                                 BuId = opnDtls.Buid,
                                 BuName = string.Empty,
                                 CreatedBy = opn.CreatedBy,
                                 JobDescSkillViewModel = (from tech in dbContext.PhJobOpeningSkills
                                                          where tech.Joid == opn.Id && tech.Status == (byte)RecordStatus.Active
                                                          select new JobSkillViewModel
                                                          {
                                                              ExpInMonths = tech.ExpMonth,
                                                              ExpInYears = tech.ExpYears,
                                                              TechnologyName = tech.Technology
                                                          }).ToList(),

                                 JobTeam = (from jobAsmt in dbContext.PhJobAssignments
                                            join usr in dbContext.PiHireUsers on jobAsmt.AssignedTo equals usr.Id
                                            where jobAsmt.Joid == opn.Id
                                            select new JobWorkingTeamViewModl
                                            {
                                                Name = usr.FirstName + " " + usr.LastName,
                                                UserId = usr.Id,
                                                Location = usr.Location,
                                                Role = usr.UserRoleName,
                                                NoOfCv = jobAsmt.NoCvsrequired,
                                                NoOfFinalCvsFilled = jobAsmt.NoOfFinalCvsFilled,
                                                DeAssignDate = jobAsmt.DeassignDate,
                                                ProfilePhoto = usr.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + usr.Id + "/ProfilePhoto/" + usr.ProfilePhoto : string.Empty
                                            }).ToList()
                             }).FirstOrDefaultAsync();
                if (Job != null)
                {
                    Job.MaxYear = ConvertYears(Job.MaxExpeInMonths);
                    Job.MinYear = ConvertYears(Job.MinExpeInMonths);
                    var GetClient = await dbContext.GetClient(Job.ClientId);
                    if (GetClient.Count > 0)
                    {
                        Job.AccountManagerName = GetClient.Where(x => x.Id == Job.ClientId).Select(x => x.AccountManagerName).FirstOrDefault();
                    }
                    var Spocs = await dbContext.GetClientSpocs(Job.ClientId);
                    if (Spocs.Count > 0)
                    {
                        Job.SpocName = Spocs.Where(x => x.Id == Job.SpocId).Select(x => x.Name).FirstOrDefault();
                    }

                    var getPUs = await dbContext.GetPUs();
                    if (getPUs.Count > 0)
                    {
                        var pudtls = getPUs.Where(x => x.Id == Job.PuId).FirstOrDefault();
                        if (pudtls != null)
                        {
                            Job.PuName = pudtls.Name;
                            Job.PuShortName = pudtls.ShortName;
                        }
                    }

                    string PuId = Job.PuId.ToString();
                    var getBUs = await dbContext.GetBUs(PuId);
                    if (getBUs.Count > 0)
                    {
                        var budtls = getBUs.Where(x => x.Id == Job.BuId).FirstOrDefault();
                        if (budtls != null)
                        {
                            Job.BuName = budtls.Name;
                        }
                    }

                    if (Usr.UserTypeId == (byte)UserType.Recruiter || Usr.UserTypeId == (byte)UserType.BDM)
                    {
                        if (Job.JobTeam.Count > 0)
                        {
                            AddCadidate = true;

                            Job.JobTeam = Job.JobTeam.Where(x => x.DeAssignDate == null).ToList();

                        }
                        else
                        {
                            AddCadidate = false;
                        }
                    }

                    Job.AddCadidate = AddCadidate;
                    Job.CreatedByProfilePhoto = Job.CreatedByProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + Job.CreatedBy + "/ProfilePhoto/" + Job.CreatedByProfilePhoto : string.Empty;
                    Job.BroughtByProfilePhoto = Job.BroughtByProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + Job.BroughtBy + "/ProfilePhoto/" + Job.BroughtByProfilePhoto : string.Empty;

                    var modifiedDtls = (from activityLog in dbContext.PhActivityLogs
                                        join hireUser in dbContext.PiHireUsers on activityLog.CreatedBy equals hireUser.Id
                                        where activityLog.Joid == Id && activityLog.ActivityMode == 2
                                        select new
                                        {
                                            activityLog.CreatedDate,
                                            Name = hireUser.FirstName + " " + hireUser.LastName,
                                            hireUser.Id
                                        }).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                    if (modifiedDtls != null)
                    {
                        Job.LastModified = modifiedDtls.Name;
                        Job.LastModifiedOn = modifiedDtls.CreatedDate;
                    }
                }

                respModel.Status = true;
                respModel.SetResult(Job);
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

        public async Task<GetResponseViewModel<List<JobAssessmentViewModel>>> GetJobAssessments(int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());

            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<JobAssessmentViewModel>>();
            try
            {
                var data = new List<AssessmentViewModel>();
                data = await GetAssessmentList();


                var Assessments = new List<JobAssessmentViewModel>();
                var jobOpenings = dbContext.PhJobOpenings.Where(x => x.Id == JobId).FirstOrDefault();
                if (jobOpenings != null)
                {
                    Assessments = await (from asstmnt in dbContext.PhJobOpeningAssmts
                                         join stage in dbContext.PhCandStagesSes on asstmnt.StageId equals stage.Id
                                         join cstatus in dbContext.PhCandStatusSes on asstmnt.CandStatusId equals cstatus.Id
                                         where asstmnt.Joid == JobId && asstmnt.Status != (byte)RecordStatus.Delete
                                         select new JobAssessmentViewModel
                                         {
                                             Id = asstmnt.Id,
                                             AssessmentId = asstmnt.AssessmentId,
                                             CandStatusId = asstmnt.CandStatusId,
                                             CandStatusName = cstatus.Title,
                                             StageId = asstmnt.StageId,
                                             JobId = asstmnt.Joid,
                                             StageColor = stage.ColorCode,
                                             StageName = stage.Title
                                         }).ToListAsync();

                    if (Assessments.Count > 0)
                    {
                        foreach (var item in Assessments)
                        {
                            var dtls = data.Where(x => x.Id == item.AssessmentId).Select(x => new { x.SurveyName, x.PreviewUrl }).FirstOrDefault();
                            if (dtls != null)
                            {
                                item.AssessmentName = dtls.SurveyName;
                                item.PreviewUrl = dtls.PreviewUrl;
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
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        public async Task<CreateResponseViewModel<string>> MapAssessmenttoJob(MapAssessmentToJobViewModel mapAssessmentViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());


            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string message = "Updated Successfully";
            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

            try
            {


                List<int> AssmtIds = mapAssessmentViewModel.AssessmentIdViewModel.Select(x => x.Id).ToList();

                var phJobOpeningAssmtList = await dbContext.PhJobOpeningAssmts.Where(x => !AssmtIds.Contains(x.Id) && x.Joid == mapAssessmentViewModel.JobId
                && x.Status != (byte)RecordStatus.Delete).ToListAsync();

                if (phJobOpeningAssmtList.Count > 0)
                {
                    foreach (var item in phJobOpeningAssmtList)
                    {
                        item.UpdatedBy = UserId;
                        item.Status = (byte)RecordStatus.Delete;
                        item.UpdatedDate = CurrentTime;

                        dbContext.PhJobOpeningAssmts.Update(item);
                        await dbContext.SaveChangesAsync();
                    }
                }

                foreach (var item in mapAssessmentViewModel.AssessmentIdViewModel)
                {
                    var jobOpeningAssmts = dbContext.PhJobOpeningAssmts.Where(x => x.Joid == mapAssessmentViewModel.JobId
                  && x.AssessmentId == item.AssessmentId && x.StageId == item.StageId
                  && x.CandStatusId == item.CandStatusId
                  && x.Status != (byte)RecordStatus.Delete).FirstOrDefault();
                    if (jobOpeningAssmts == null)
                    {
                        var assmt = new PhJobOpeningAssmt()
                        {
                            AssessmentId = item.AssessmentId,
                            CandStatusId = item.CandStatusId,
                            CreatedBy = UserId,
                            CreatedDate = CurrentTime,
                            StageId = item.StageId,
                            Joid = mapAssessmentViewModel.JobId,
                            Status = (byte)RecordStatus.Active
                        };

                        dbContext.PhJobOpeningAssmts.Add(assmt);
                        await dbContext.SaveChangesAsync();

                        var opening = dbContext.PhJobOpenings.Where(x => x.Id == mapAssessmentViewModel.JobId).Select(x => x.JobTitle).FirstOrDefault();
                        // activity
                        var activityLog = new CreateActivityViewModel
                        {
                            ActivityMode = (byte)WorkflowActionMode.Opening,
                            ActivityOn = assmt.Id,
                            JobId = mapAssessmentViewModel.JobId,
                            ActivityType = (byte)LogActivityType.AssessementUpdates,
                            ActivityDesc = " has Assessment Mapped to " + opening + " job successfully",
                            UserId = UserId
                        };
                        activityList.Add(activityLog);
                    }

                    // audit 
                    var auditLog = new CreateAuditViewModel
                    {
                        ActivitySubject = "Assessment Mapped to Job",
                        ActivityDesc = " Assessment Mapped to Job successfully",
                        ActivityType = (byte)AuditActivityType.RecordUpdates,
                        TaskID = 0,
                        UserId = UserId
                    };
                    audList.Add(auditLog);

                }

                if (audList.Count > 0)
                {
                    SaveAuditLog(audList);
                }
                if (activityList.Count > 0)
                {
                    SaveActivity(activityList);
                }

                var phJobOpeningActvCounter = await dbContext.PhJobOpeningActvCounters.Where(x => x.Joid == mapAssessmentViewModel.JobId
               && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();

                if (phJobOpeningActvCounter != null)
                {
                    phJobOpeningActvCounter.AsmtCounter = AssmtIds.Count;

                    dbContext.PhJobOpeningActvCounters.Update(phJobOpeningActvCounter);
                    await dbContext.SaveChangesAsync();
                }

                respModel.Status = true;
                respModel.SetResult(message);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(mapAssessmentViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        public async Task<GetResponseViewModel<List<GetJobAssignedTeamMembersViewModel>>> GetJobTeamMembers(int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<GetJobAssignedTeamMembersViewModel>>();
            try
            {

                var jodDtls = dbContext.PhJobOpeningsAddlDetails.Where(x => x.Joid == JobId).FirstOrDefault();
                if (jodDtls != null)
                {
                    List<UsersViewModel> RecData = null;
                    List<int> UserTypes = new List<int>
                {
                    (int)UserType.Recruiter
                };
                    RecData = await GetUserbyTypes(UserTypes, jodDtls.Puid.Value);

                    var Members = new List<GetJobAssignedTeamMembersViewModel>();
                    foreach (var item in RecData)
                    {
                        var jobAssmtns = await dbContext.PhJobAssignments.Where(x => x.AssignedTo == item.UserId && x.DeassignDate == null).ToListAsync();
                        var jobAssinged = jobAssmtns.Where(x => x.Joid == JobId).FirstOrDefault() != null ? true : false;
                        var asstn = new GetJobAssignedTeamMembersViewModel
                        {
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            FullName = item.FirstName + " " + item.LastName,
                            JobAssinged = jobAssinged,
                            CopyJobAssinged = jobAssinged,
                            MobileNo = item.MobileNo,
                            UserId = item.UserId,
                            Email = item.Email,
                            ActiveJobs = jobAssmtns.Count(),
                            Location = item.Location,
                            LocationId = item.LocationId,
                            RoleName = item.RoleName,
                            ProfilePhoto = item.ProfilePhoto
                        };
                        Members.Add(asstn);
                    }

                    respModel.Status = true;
                    respModel.SetResult(Members);
                }
                else
                {
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, "Job is not available", true);
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

        public async Task<GetResponseViewModel<List<UsersViewModel>>> GetJobAssociatedPannel(int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<UsersViewModel>>();
            try
            {

                List<UsersViewModel> RecData = null;

                var jobPannel = new List<int>();
                jobPannel = await dbContext.PhJobAssignments.Where(x => x.Joid == JobId && x.DeassignDate == null).Select(x => x.AssignedTo).ToListAsync();

                var jobDtls = dbContext.PhJobOpenings.Where(x => x.Id == JobId).FirstOrDefault();
                if (jobDtls != null)
                {
                    jobPannel.Add(jobDtls.CreatedBy);
                    if (jobDtls.BroughtBy != null)
                    {
                        if (jobDtls.CreatedBy != jobDtls.BroughtBy.Value)
                        {
                            jobPannel.Add(jobDtls.BroughtBy.Value);
                        }
                    }
                }
                RecData = dbContext.PiHireUsers.Select(x => new UsersViewModel
                {
                    Status = x.Status,
                    UserType = x.UserType,
                    Email = x.UserName,
                    Name = x.FirstName + " " + x.LastName,
                    MobileNo = x.MobileNumber,
                    UserId = x.Id,
                    RoleName = x.UserRoleName,
                    ProfilePhoto = x.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + x.UserId + "/ProfilePhoto/" + x.ProfilePhoto : string.Empty
                }).Where(x => UserId != x.UserId && (jobPannel.Contains(x.UserId) || x.UserType == (byte)UserType.Admin) && x.Status == (byte)RecordStatus.Active).ToList();

                respModel.Status = true;
                respModel.SetResult(RecData);
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

        public async Task<GetResponseViewModel<List<AssignedJobsViewModel>>> AssignedJobs(AssignedJobsReqViewModel assignedJobsViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<AssignedJobsViewModel>>();
            try
            {

                var AssignedJobs = new List<AssignedJobsViewModel>();
                var jobStatusClosedId = await dbContext.PhJobStatusSes.AsNoTracking().Where(da => da.Jscode == "CLS" && da.Status != (byte)RecordStatus.Delete).Select(da => da.Id).FirstOrDefaultAsync();

                AssignedJobs = await (from JobAssignment in dbContext.PhJobAssignments
                                      join JobOpening in dbContext.PhJobOpenings on JobAssignment.Joid equals JobOpening.Id
                                      join User in dbContext.PiHireUsers on JobOpening.BroughtBy == null ? JobOpening.CreatedBy : JobOpening.BroughtBy equals User.Id
                                      where JobAssignment.DeassignDate == null && JobAssignment.AssignedTo == assignedJobsViewModel.RecUserId && User.UserType != (byte)UserType.Candidate
                                      && JobOpening.JobOpeningStatus != jobStatusClosedId
                                      select new AssignedJobsViewModel
                                      {
                                          AccountName = JobOpening.ClientName,
                                          BDMName = User.FirstName + " " + User.LastName,
                                          CreatedDate = JobOpening.CreatedDate,
                                          EndDate = JobOpening.ClosedDate,
                                          JobName = JobOpening.JobTitle,
                                          JoId = JobOpening.Id,
                                          RequiredCv = JobAssignment.NoCvsrequired,
                                          SubmittedCV = JobAssignment.ProfilesUploaded
                                      }).ToListAsync();

                respModel.Status = true;
                respModel.SetResult(AssignedJobs);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(assignedJobsViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }


        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> AssignJobToTeamMember(JobAssignedMembersViewModel jobAssignedMembersViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int loginUserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
            try
            {

                var jobDtls = await dbContext.PhJobOpenings.Where(x => x.Id == jobAssignedMembersViewModel.JobId).FirstOrDefaultAsync();
                string message = string.Empty;
                int noOfCVSInct = 0;
                if (jobDtls != null)
                {
                    var jobStatus = dbContext.PhJobStatusSes.ToList();
                    var newStatusId = jobStatus.Where(x => x.Jscode == "NEW").Select(x => x.Id).FirstOrDefault();
                    if (jobDtls.JobOpeningStatus == newStatusId) // NEW 
                    {
                        var WipStatusId = jobStatus.Where(x => x.Jscode == "WIP").Select(x => x.Id).FirstOrDefault();
                        jobDtls.JobOpeningStatus = WipStatusId;
                        jobDtls.UpdatedDate = CurrentTime;

                        dbContext.PhJobOpenings.Update(jobDtls);
                        await dbContext.SaveChangesAsync();
                    }

                    var jobAdlDtls = await dbContext.PhJobOpeningsAddlDetails.Where(x => x.Joid == jobAssignedMembersViewModel.JobId).FirstOrDefaultAsync();

                    foreach (var item in jobAssignedMembersViewModel.assignMembers)
                    {
                        if (item.Assign)
                        {
                            var jobAsgmnt = await dbContext.PhJobAssignments.Where(x => x.AssignedTo == item.UserId && x.Joid == jobAssignedMembersViewModel.JobId
                            && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                            if (jobAsgmnt == null)
                            {
                                if (item.NoOfCvs != 0)
                                {
                                    noOfCVSInct += item.NoOfCvs;
                                    jobAsgmnt = new PhJobAssignment
                                    {
                                        Joid = jobAssignedMembersViewModel.JobId,
                                        Status = (byte)RecordStatus.Active,
                                        AssignedTo = item.UserId,
                                        CreatedBy = loginUserId,
                                        ProfilesRejected = 0,
                                        ProfilesUploaded = 0,
                                        AssignBy = 1,
                                        CvTarget = item.NoOfCvs,
                                        NoCvsrequired = item.NoOfCvs,
                                        CreatedDate = CurrentTime,
                                        CvTargetDate = CurrentTime
                                    };
                                    dbContext.PhJobAssignments.Add(jobAsgmnt);
                                    dbContext.PhJobAssignmentsDayWises.Add(new PhJobAssignmentsDayWise
                                    {
                                        Status = (byte)RecordStatus.Active,
                                        Joid = jobAsgmnt.Joid,
                                        AssignedTo = jobAsgmnt.AssignedTo,
                                        CreatedBy = jobAsgmnt.CreatedBy,
                                        NoCvsuploadded = jobAsgmnt.ProfilesUploaded,
                                        AssignBy = jobAsgmnt.AssignBy,
                                        NoCvsrequired = jobAsgmnt.NoCvsrequired,
                                        AssignmentDate = jobAsgmnt.CreatedDate
                                    });
                                    await dbContext.SaveChangesAsync();
                                    dbContext.PhJobAssignmentHistories.Add(new PhJobAssignmentHistory
                                    {
                                        Joid = jobAsgmnt.Joid,
                                        AssignedTo = jobAsgmnt.AssignedTo,
                                        CreatedBy = jobAsgmnt.CreatedBy,
                                        CreatedDate = jobAsgmnt.CreatedDate,
                                        Status = (byte)RecordStatus.Active
                                    });

                                    var assign = dbContext.PiHireUsers.Where(x => x.Id == item.UserId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

                                    // activity
                                    var activityLog = new CreateActivityViewModel
                                    {
                                        ActivityMode = (byte)WorkflowActionMode.Opening,
                                        ActivityOn = item.UserId,
                                        JobId = jobAssignedMembersViewModel.JobId,
                                        ActivityType = (byte)LogActivityType.RecordUpdates,
                                        ActivityDesc = " has Assigned " + jobDtls.JobTitle + " job to " + assign + " successfully",
                                        UserId = loginUserId
                                    };
                                    activityList.Add(activityLog);

                                    // applying work flow conditions 
                                    var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
                                    {
                                        ActionMode = (byte)WorkflowActionMode.Other,
                                        JobId = jobAssignedMembersViewModel.JobId,
                                        TaskCode = TaskCode.AGJ.ToString(),
                                        UserId = loginUserId,
                                        AssignTo = item.UserId
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
                                                CreatedBy = loginUserId
                                            };
                                            notificationPushedViewModel.Add(notificationPushed);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (jobAsgmnt.DeassignBy != null && item.NoOfCvs != 0)
                                {
                                    noOfCVSInct += item.NoOfCvs;
                                    noOfCVSInct += (int)jobAsgmnt.NoCvsrequired;

                                    jobAsgmnt.ReassignDate = CurrentTime;
                                    jobAsgmnt.ReassignBy = loginUserId;
                                    jobAsgmnt.DeassignBy = null;
                                    jobAsgmnt.DeassignDate = null;
                                    jobAsgmnt.UpdatedBy = loginUserId;
                                    jobAsgmnt.UpdatedDate = CurrentTime;
                                    jobAsgmnt.NoCvsrequired += item.NoOfCvs;
                                    jobAsgmnt.CvTargetDate = CurrentTime;
                                    jobAsgmnt.AssignBy = 1;
                                    jobAsgmnt.CvTarget = item.NoOfCvs;
                                    PhJobAssignmentsDayWise_records(ref jobAsgmnt, CurrentTime, incrementCvsrequired: item.NoOfCvs);
                                    dbContext.PhJobAssignments.Update(jobAsgmnt);
                                    dbContext.PhJobAssignmentHistories.Add(new PhJobAssignmentHistory
                                    {
                                        Joid = jobAsgmnt.Joid,
                                        AssignedTo = jobAsgmnt.AssignedTo,
                                        CreatedBy = jobAsgmnt.ReassignBy.Value,
                                        CreatedDate = jobAsgmnt.ReassignDate.Value,
                                        Status = (byte)RecordStatus.Active
                                    });
                                    await dbContext.SaveChangesAsync();

                                    var assign = dbContext.PiHireUsers.Where(x => x.Id == item.UserId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

                                    // Activity
                                    var activityLog = new CreateActivityViewModel
                                    {
                                        ActivityMode = (byte)WorkflowActionMode.Opening,
                                        ActivityOn = item.UserId,
                                        JobId = jobAssignedMembersViewModel.JobId,
                                        ActivityType = (byte)LogActivityType.RecordUpdates,
                                        ActivityDesc = " has deAssigned " + jobDtls.JobTitle + " job to " + assign + " successfully",
                                        UserId = loginUserId
                                    };
                                    activityList.Add(activityLog);
                                }
                            }
                        }
                        else
                        {
                            var jobAssmtns = await dbContext.PhJobAssignments.Where(x => x.AssignedTo == item.UserId && x.Joid == jobAssignedMembersViewModel.JobId && x.Status != (byte)RecordStatus.Delete).FirstOrDefaultAsync();
                            if (jobAssmtns != null)
                            {
                                if (jobAdlDtls != null && jobAssmtns.DeassignDate == null)
                                {
                                    jobAdlDtls.NoOfCvsRequired -= jobAssmtns.NoCvsrequired;
                                    dbContext.PhJobOpeningsAddlDetails.Update(jobAdlDtls);
                                    await dbContext.SaveChangesAsync();
                                }

                                jobAssmtns.AssignBy = 1;
                                jobAssmtns.DeassignBy = loginUserId;
                                jobAssmtns.DeassignDate = CurrentTime;
                                jobAssmtns.UpdatedBy = loginUserId;
                                jobAssmtns.UpdatedDate = CurrentTime;

                                dbContext.PhJobAssignments.Update(jobAssmtns);
                                var jobAssmntHis = await dbContext.PhJobAssignmentHistories.FirstOrDefaultAsync(da => da.AssignedTo == jobAssmtns.AssignedTo && da.Joid == jobAssmtns.Joid && da.Status == (byte)RecordStatus.Active && da.DeassignDate.HasValue == false);
                                if (jobAssmntHis != null)
                                {
                                    jobAssmntHis.DeassignDate = jobAssmtns.DeassignDate;
                                    jobAssmntHis.DeassignBy = jobAssmtns.DeassignBy;
                                    jobAssmntHis.UpdatedDate = jobAssmtns.UpdatedDate;
                                    jobAssmntHis.UpdatedBy = jobAssmtns.UpdatedBy;
                                }
                                await dbContext.SaveChangesAsync();

                                var assign = dbContext.PiHireUsers.Where(x => x.Id == item.UserId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                                // activity
                                var activityLog = new CreateActivityViewModel
                                {
                                    ActivityMode = (byte)WorkflowActionMode.Opening,
                                    ActivityOn = item.UserId,
                                    JobId = jobAssignedMembersViewModel.JobId,
                                    ActivityType = (byte)LogActivityType.RecordUpdates,
                                    ActivityDesc = " has deAssigned " + jobDtls.JobTitle + " job to " + assign + " successfully",
                                    UserId = loginUserId
                                };
                                activityList.Add(activityLog);
                            }
                        }
                    }
                    if (noOfCVSInct > 0)
                    {
                        if (jobAdlDtls != null)
                        {
                            jobAdlDtls.NoOfCvsRequired += noOfCVSInct;
                            dbContext.PhJobOpeningsAddlDetails.Update(jobAdlDtls);
                            await dbContext.SaveChangesAsync();
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
                }
                else
                {
                    message = " Job is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(jobAssignedMembersViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }

        //[Obsolete]
        //public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> AssignMultipleJobToTeamMember(MultipleJobAssignmentMembersViewModel multipleJobAssignmentMembersViewModel)
        //{
        //    logger.SetMethodName(MethodBase.GetCurrentMethod());
        //    int loginUserId = Usr.Id;
        //    var respModel = new CreateResponseViewModel<string>();
        //    string message = string.Empty;
        //    var notificationPushedViewModel = new List<NotificationPushedViewModel>();
        //    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
        //    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
        //    try
        //    {
        //        
        //        if (multipleJobAssignmentMembersViewModel.MultipleJobAssignmentMembers != null)
        //        {
        //            var jobStatus = dbContext.PhJobStatusSes.ToList();

        //            foreach (var item in multipleJobAssignmentMembersViewModel.MultipleJobAssignmentMembers)
        //            {
        //                int noOfCVS = 0;
        //                var jobDtls = await dbContext.PhJobOpenings.Where(da => da.Id == item.JoId).FirstOrDefaultAsync();
        //                var jobAdlDtls = await dbContext.PhJobOpeningsAddlDetails.Where(da => da.Joid == item.JoId).FirstOrDefaultAsync();
        //                if (item.UserId != 0 && item.JoId != 0 && item.NoOfCvs != 0)
        //                {
        //                    noOfCVS += item.NoOfCvs;
        //                    var jobAsgmnt = await dbContext.PhJobAssignments.Where(da => da.AssignedTo == item.UserId && da.Joid == item.JoId).FirstOrDefaultAsync();
        //                    if (jobAsgmnt == null)
        //                    {
        //                        var asstn = new PhJobAssignment
        //                        {
        //                            Joid = item.JoId,
        //                            Status = (byte)RecordStatus.Active,
        //                            AssignedTo = item.UserId,
        //                            CreatedBy = loginUserId,
        //                            ProfilesRejected = 0,
        //                            ProfilesUploaded = 0,
        //                            AssignBy = 1,
        //                            CvTarget = item.NoOfCvs,
        //                            NoCvsrequired = item.NoOfCvs,
        //                            CreatedDate = CurrentTime,
        //                            CvTargetDate = item.cvTargetDate == null ? jobDtls.ClosedDate : jobAsgmnt.cvTargetDate
        //                        };
        //                        dbContext.PhJobAssignments.Add(asstn);
        //                        dbContext.PhJobAssignmentsDayWises.Add(new PhJobAssignmentsDayWise
        //                        {
        //                            Status = (byte)RecordStatus.Active,
        //                            Joid = asstn.Joid,
        //                            AssignedTo = asstn.AssignedTo,
        //                            CreatedBy = asstn.CreatedBy,
        //                            NoCvsuploadded = asstn.ProfilesUploaded,
        //                            AssignBy = asstn.AssignBy,
        //                            NoCvsrequired = asstn.NoCvsrequired,
        //                            AssignmentDate = asstn.CreatedDate
        //                        });
        //                        await dbContext.SaveChangesAsync();
        //                        dbContext.PhJobAssignmentHistories.Add(new PhJobAssignmentHistory
        //                                          {
        //                                              Joid = asstn.Joid,
        //                                              AssignedTo = asstn.AssignedTo,
        //                                              CreatedBy = asstn.CreatedBy,
        //                                              CreatedDate = asstn.CreatedDate,
        //                                              Status = (byte) RecordStatus.Active
        //                        });

        //                        var assign = dbContext.PiHireUsers.Where(da => da.Id == item.UserId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault();

        //                        // Activity
        //                        var activityLog = new CreateActivityViewModel
        //                        {
        //                            ActivityMode = (byte)WorkflowActionMode.Opening,
        //                            ActivityOn = item.UserId,
        //                            JobId = item.JoId,
        //                            ActivityType = (byte)LogActivityType.RecordUpdates,
        //                            ActivityDesc = " has Assigned " + jobDtls.JobTitle + " job to " + assign + " successfully",
        //                            UserId = loginUserId
        //                        };
        //                        activityList.Add(activityLog);

        //                        // Applying work flow conditions 
        //                        var workFlowRuleSearchViewModel = new WorkFlowRuleSearchViewModel
        //                        {
        //                            ActionMode = (byte)WorkflowActionMode.Other,
        //                            JobId = item.JoId,
        //                            TaskCode = TaskCode.AGJ.ToString(),
        //                            UserId = loginUserId,
        //                            AssignTo = item.UserId
        //                        };
        //                        var wfResp = await ExecuteWorkFlowConditions(workFlowRuleSearchViewModel);
        //                        if (wfResp.Status && wfResp.isNotification)
        //                        {
        //                            foreach (var itemN in wfResp.WFNotifications)
        //                            {
        //                                var notificationPushed = new NotificationPushedViewModel
        //                                {
        //                                    JobId = wfResp.JoId,
        //                                    PushedTo = itemN.UserIds,
        //                                    NoteDesc = itemN.NoteDesc,
        //                                    Title = itemN.Title,
        //                                    CreatedBy = loginUserId
        //                                };
        //                                notificationPushedViewModel.Add(notificationPushed);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        noOfCVS += (int)jobAsgmnt.NoCvsrequired;

        //                        jobAsgmnt.AssignBy = 1;
        //                        jobAsgmnt.NoCvsrequired += item.NoOfCvs;
        //                        jobAsgmnt.ReassignDate = CurrentTime;
        //                        jobAsgmnt.ReassignBy = loginUserId;
        //                        jobAsgmnt.DeassignBy = null;
        //                        jobAsgmnt.DeassignDate = null;
        //                        jobAsgmnt.UpdatedBy = loginUserId;
        //                        jobAsgmnt.UpdatedDate = CurrentTime;
        //                        jobAsgmnt.CvTargetDate = item.cvTargetDate == null ? jobDtls.ClosedDate : item.cvTargetDate;
        //                        jobAsgmnt.CvTarget = item.NoOfCvs;
        //                        PhJobAssignmentsDayWise_records(ref jobAsgmnt, CurrentTime, incrementCvsrequired: item.NoOfCvs);
        //                        dbContext.PhJobAssignments.Update(jobAsgmnt);
        //                        await dbContext.SaveChangesAsync();

        //                        var assign = dbContext.PiHireUsers.Where(da => da.Id == item.UserId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault();
        //                        // Activity
        //                        var activityLog = new CreateActivityViewModel
        //                        {
        //                            ActivityMode = (byte)WorkflowActionMode.Opening,
        //                            ActivityOn = item.UserId,
        //                            JobId = item.JoId,
        //                            ActivityType = (byte)LogActivityType.RecordUpdates,
        //                            ActivityDesc = " has deAssigned " + jobDtls.JobTitle + " job to " + assign + " successfully",
        //                            UserId = loginUserId
        //                        };
        //                        activityList.Add(activityLog);
        //                    }

        //                    if (noOfCVS > 0)
        //                    {
        //                        if (jobAdlDtls != null)
        //                        {
        //                            jobAdlDtls.NoOfCvsRequired += noOfCVS;
        //                            dbContext.PhJobOpeningsAddlDetails.Update(jobAdlDtls);
        //                            await dbContext.SaveChangesAsync();
        //                        }
        //                    }
        //                }
        //            }

        //            if (audList.Count > 0)
        //            {
        //                SaveAuditLog(audList);
        //            }
        //            if (activityList.Count > 0)
        //            {
        //                SaveActivity(activityList);
        //            }

        //            message = "Updated Successfully";
        //            respModel.Status = true;
        //            respModel.SetResult(message);
        //        }
        //        else
        //        {
        //            message = "jobInfoModel is not available";
        //            respModel.Status = false;
        //            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(multipleJobAssignmentMembersViewModel), respModel.Meta.RequestID, ex);

        //        respModel.Status = false;
        //        respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
        //        respModel.Result = null;
        //    }
        //    return Tuple.Create(notificationPushedViewModel, respModel);
        //}



        public async Task<Tuple<List<NotificationPushedViewModel>, CreateResponseViewModel<string>>> DeAssignJobToTeamMember(DeAssignJobViewmodel DeAssignJobViewmodel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new CreateResponseViewModel<string>();
            string message = string.Empty;
            var notificationPushedViewModel = new List<NotificationPushedViewModel>();
            List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
            List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();

            try
            {
                var jobStatus = dbContext.PhJobStatusSes.ToList();
                var jobDtls = await dbContext.PhJobOpenings.Where(x => x.Id == DeAssignJobViewmodel.JobId).FirstOrDefaultAsync();

                var jobAssmtns = await dbContext.PhJobAssignments.Where(x => x.AssignedTo == DeAssignJobViewmodel.RecId && x.Joid == DeAssignJobViewmodel.JobId).FirstOrDefaultAsync();
                if (jobAssmtns != null)
                {
                    jobAssmtns.DeassignBy = UserId;
                    jobAssmtns.DeassignDate = CurrentTime;
                    jobAssmtns.UpdatedBy = UserId;
                    jobAssmtns.UpdatedDate = CurrentTime;

                    dbContext.PhJobAssignments.Update(jobAssmtns);
                    var jobAssmntHis = await dbContext.PhJobAssignmentHistories.FirstOrDefaultAsync(da => da.AssignedTo == jobAssmtns.AssignedTo && da.Joid == jobAssmtns.Joid && da.Status == (byte)RecordStatus.Active && da.DeassignDate.HasValue == false);
                    if (jobAssmntHis != null)
                    {
                        jobAssmntHis.DeassignDate = jobAssmtns.DeassignDate;
                        jobAssmntHis.DeassignBy = jobAssmtns.DeassignBy;
                        jobAssmntHis.UpdatedDate = jobAssmtns.UpdatedDate;
                        jobAssmntHis.UpdatedBy = jobAssmtns.UpdatedBy;
                    }
                    await dbContext.SaveChangesAsync();

                    var assign = dbContext.PiHireUsers.Where(x => x.Id == DeAssignJobViewmodel.RecId && x.UserType != (byte)UserType.Candidate).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                    // Activity
                    var activityLog = new CreateActivityViewModel
                    {
                        ActivityMode = (byte)WorkflowActionMode.Opening,
                        ActivityOn = DeAssignJobViewmodel.RecId,
                        JobId = DeAssignJobViewmodel.JobId,
                        ActivityType = (byte)LogActivityType.RecordUpdates,
                        ActivityDesc = " has deAssigned " + jobDtls.JobTitle + " job to " + assign + " successfully",
                        UserId = UserId
                    };
                    activityList.Add(activityLog);

                    if (audList.Count > 0)
                    {
                        SaveAuditLog(audList);
                    }
                    if (activityList.Count > 0)
                    {
                        SaveActivity(activityList);
                    }

                    if (Usr.UserTypeId != (byte)UserType.Recruiter)
                    {
                        // Day Wise Actions 
                        var IsDayWiseAction = dbContext.PhDayWiseJobActions.Where(x => x.Joid == DeAssignJobViewmodel.JobId && x.CreatedDate.Date == CurrentTime.Date).FirstOrDefault();
                        if (IsDayWiseAction == null)
                        {
                            IsDayWiseAction = new PhDayWiseJobAction
                            {
                                CreatedBy = UserId,
                                CreatedDate = CurrentTime,
                                Joid = DeAssignJobViewmodel.JobId,
                                Assign = true
                            };
                            dbContext.PhDayWiseJobActions.Add(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            IsDayWiseAction.Assign = true;
                            dbContext.PhDayWiseJobActions.Update(IsDayWiseAction);
                            dbContext.SaveChanges();
                        }
                    }


                    message = "Updated Successfully";
                    respModel.Status = true;
                    respModel.SetResult(message);

                }
                else
                {
                    message = "Job is not available";
                    respModel.Status = false;
                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
                }

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(DeAssignJobViewmodel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return Tuple.Create(notificationPushedViewModel, respModel);
        }

        //[Obsolete]
        //public async Task<GetResponseViewModel<string>> SlefAssignJob(int jobId)
        //{
        //    logger.SetMethodName(MethodBase.GetCurrentMethod());
        //    int loginUserId = Usr.Id;
        //    var respModel = new GetResponseViewModel<string>();
        //    string message = string.Empty;
        //    List<CreateActivityViewModel> activityList = new List<CreateActivityViewModel>();
        //    List<CreateAuditViewModel> audList = new List<CreateAuditViewModel>();
        //    try
        //    {
        //        
        //        if (Usr.UserTypeId == (byte)UserType.Recruiter)
        //        {
        //            if (jobId != 0)
        //            {
        //                var jobDtls = await dbContext.PhJobOpenings.Where(da => da.Id == jobId).FirstOrDefaultAsync();
        //                if (jobDtls != null)
        //                {
        //                    var jobStatus = dbContext.PhJobStatusSes.ToList();
        //                    var NewStatusId = jobStatus.Where(da => da.Jscode == "NEW").Select(da => da.Id).FirstOrDefault();

        //                    var jobAsgmnt = await dbContext.PhJobAssignments.Where(da => da.AssignedTo == loginUserId && da.Joid == jobId).FirstOrDefaultAsync();
        //                    var assign = dbContext.PiHireUsers.Where(da => da.Id == loginUserId).Select(da => da.FirstName + " " + da.LastName).FirstOrDefault();
        //                    if (jobAsgmnt == null)
        //                    {
        //                        var asstn = new PhJobAssignment
        //                        {
        //                            Joid = jobId,
        //                            Status = (byte)RecordStatus.Active,
        //                            AssignedTo = loginUserId,
        //                            CreatedBy = loginUserId,
        //                            ProfilesRejected = 0,
        //                            ProfilesUploaded = 0,
        //                            NoCvsrequired = 1,
        //                            CreatedDate = CurrentTime,
        //                            CvTargetDate = CurrentTime,
        //                            AssignBy = 1,
        //                            CvTarget = 1
        //                        };
        //                        dbContext.PhJobAssignmentsDayWises.Add(new PhJobAssignmentsDayWise
        //                        {
        //                            Status = (byte)RecordStatus.Active,
        //                            Joid = asstn.Joid,
        //                            AssignedTo = asstn.AssignedTo,
        //                            CreatedBy = asstn.CreatedBy,
        //                            NoCvsuploadded = asstn.ProfilesUploaded,
        //                            AssignBy = asstn.AssignBy,
        //                            NoCvsrequired = asstn.NoCvsrequired,
        //                            AssignmentDate = asstn.CreatedDate
        //                        });
        //                        dbContext.PhJobAssignments.Add(asstn);
        //                        await dbContext.SaveChangesAsync();
        //                        dbContext.PhJobAssignmentHistories.Add(new PhJobAssignmentHistory
        //                                          {
        //                                              Joid = asstn.Joid,
        //                                              AssignedTo = asstn.AssignedTo,
        //                                              CreatedBy = asstn.CreatedBy,
        //                                              CreatedDate = asstn.CreatedDate,
        //                                              Status = (byte) RecordStatus.Active
        //                        });

        //                        // activity
        //                        var activityLog = new CreateActivityViewModel
        //                        {
        //                            JobId = jobId,
        //                            ActivityMode = (byte)WorkflowActionMode.Opening,
        //                            ActivityOn = loginUserId,
        //                            ActivityType = (byte)LogActivityType.RecordUpdates,
        //                            ActivityDesc = " has self Assigned " + jobDtls.JobTitle + " job successfully",
        //                            UserId = loginUserId
        //                        };
        //                        activityList.Add(activityLog);

        //                        if (jobDtls.JobOpeningStatus == NewStatusId) // NEW 
        //                        {
        //                            var WipStatusId = jobStatus.Where(da => da.Jscode == "WIP").Select(da => da.Id).FirstOrDefault();
        //                            jobDtls.JobOpeningStatus = WipStatusId;
        //                            jobDtls.UpdatedDate = CurrentTime;

        //                            dbContext.PhJobOpenings.Update(jobDtls);
        //                            await dbContext.SaveChangesAsync();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        jobAsgmnt.NoCvsrequired += 1;
        //                        jobAsgmnt.CvTargetDate = CurrentTime;
        //                        jobAsgmnt.AssignBy = 1;
        //                        jobAsgmnt.CvTarget = 1;
        //                        jobAsgmnt.ReassignDate = CurrentTime;
        //                        jobAsgmnt.ReassignBy = loginUserId;
        //                        jobAsgmnt.DeassignBy = null;
        //                        jobAsgmnt.DeassignDate = null;
        //                        jobAsgmnt.UpdatedBy = loginUserId;
        //                        jobAsgmnt.UpdatedDate = CurrentTime;
        //                        PhJobAssignmentsDayWise_records(ref jobAsgmnt, CurrentTime, incrementCvsrequired: 1);
        //                        dbContext.PhJobAssignments.Update(jobAsgmnt);
        //                        await dbContext.SaveChangesAsync();

        //                        // activity
        //                        var activityLog = new CreateActivityViewModel
        //                        {
        //                            ActivityMode = (byte)WorkflowActionMode.Opening,
        //                            ActivityOn = loginUserId,
        //                            JobId = jobId,
        //                            ActivityType = (byte)LogActivityType.RecordUpdates,
        //                            ActivityDesc = " has self deAssigned " + jobDtls.JobTitle + " jobInfoModel successfully",
        //                            UserId = loginUserId
        //                        };
        //                        activityList.Add(activityLog);
        //                    }


        //                    var jobAdlDtls = await dbContext.PhJobOpeningsAddlDetails.Where(da => da.Joid == jobId).FirstOrDefaultAsync();
        //                    if (jobAdlDtls != null)
        //                    {
        //                        jobAdlDtls.NoOfCvsRequired += 1;
        //                        dbContext.PhJobOpeningsAddlDetails.Update(jobAdlDtls);
        //                        await dbContext.SaveChangesAsync();
        //                    }


        //                    // Audit 
        //                    var auditLog = new CreateAuditViewModel
        //                    {
        //                        ActivitySubject = " " + assign + " has self Assigned " + jobDtls.JobTitle + " jobInfoModel successfully",
        //                        ActivityDesc = " " + assign + " has self Assigned " + jobDtls.JobTitle + " jobInfoModel successfully",
        //                        ActivityType = (byte)AuditActivityType.RecordUpdates,
        //                        TaskID = 0,
        //                        UserId = loginUserId
        //                    };
        //                    audList.Add(auditLog);

        //                    if (audList.Count > 0)
        //                    {
        //                        SaveAuditLog(audList);
        //                    }
        //                    if (activityList.Count > 0)
        //                    {
        //                        SaveActivity(activityList);
        //                    }

        //                    message = "Assinged Successfully";
        //                    respModel.Status = true;
        //                    respModel.SetResult(message);
        //                }
        //                else
        //                {
        //                    message = " jobInfoModel Id is not available in system";
        //                    respModel.Status = false;
        //                    respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
        //                }
        //            }
        //            else
        //            {
        //                message = "jobInfoModel Id is not available";
        //                respModel.Status = false;
        //                respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
        //            }
        //        }
        //        else
        //        {
        //            message = "Self Assignment should work only recruiters";
        //            respModel.Status = false;
        //            respModel.Meta.SetError(ApiResponseErrorCodes.ResourceDoesNotExist, message, true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log(LogLevel.Error, LoggingEvents.InsertItem, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

        //        respModel.Status = false;
        //        respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
        //        respModel.Result = null;
        //    }
        //    return respModel;
        //}

        #endregion


        #region Pipeline 

        public async Task<GetResponseViewModel<GetJobPiplineViewModel>> GetJobPipeline(int JobId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());


            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<GetJobPiplineViewModel>();
            try
            {


                var pipeline = new GetJobPiplineViewModel();
                var jobOpenings = dbContext.PhJobOpenings.Where(x => x.Id == JobId).FirstOrDefault();
                if (jobOpenings != null)
                {
                    var stages = await dbContext.PhCandStagesSes.Where(x => x.Status == (byte)RecordStatus.Active).ToListAsync();
                    if (stages.Count > 0)
                    {
                        pipeline.PiplineViewModel = new List<PiplineViewModel>();
                        pipeline.AssociatedCandidateCount = 0;
                        foreach (var item in stages)
                        {
                            var piplineViewModel1 = new PiplineViewModel
                            {
                                StageId = item.Id,
                                StageName = item.Title,
                                StageColor = item.ColorCode,
                            };
                            piplineViewModel1.PiplineCandidatesViewModel = new List<PiplineCandidatesViewModel>();
                            var piplineCandidates = (from canProfile in dbContext.PhCandidateProfiles
                                                     join jobProfile in dbContext.PhJobCandidates on canProfile.Id equals jobProfile.CandProfId
                                                     join canStatus in dbContext.PhCandStatusSes on jobProfile.CandProfStatus equals canStatus.Id
                                                     join user in dbContext.PiHireUsers on canProfile.EmailId equals user.UserName
                                                     where jobProfile.StageId == piplineViewModel1.StageId && jobProfile.Joid == JobId
                                                     select new
                                                     {
                                                         jobProfile.CandProfStatus,
                                                         canStatus.Title,
                                                         canProfile.FullNameInPp,
                                                         canProfile.CandName,
                                                         jobProfile.CandProfId,
                                                         jobProfile.CreatedDate,
                                                         user.UserType,
                                                         canStatus.Cscode,
                                                         user.Id,
                                                         user.ProfilePhoto
                                                     }).OrderByDescending(x => x.CreatedDate).ToList();
                            pipeline.AssociatedCandidateCount += piplineCandidates.Count;
                            foreach (var profile in piplineCandidates)
                            {
                                var piplineCandidatesViewModel1 = new PiplineCandidatesViewModel
                                {
                                    UserId = profile.Id,
                                    CandidateStatuName = profile.Title,
                                    CandidateStatusId = profile.CandProfStatus,
                                    CandidateId = profile.CandProfId,
                                    Name = profile.CandName == string.Empty ? profile.CandName : profile.CandName,
                                    ProfileAge = GetTimeDiff(profile.CreatedDate),
                                    StageId = piplineViewModel1.StageId,
                                    isBlockListed = profile.Cscode == "BLT" ? true : false
                                };
                                if (profile.UserType == (byte)UserType.Candidate)
                                {
                                    piplineCandidatesViewModel1.ProfilePhoto = profile.ProfilePhoto;
                                }
                                else
                                {
                                    piplineCandidatesViewModel1.ProfilePhoto = profile.ProfilePhoto != null ? appSettings.AppSettingsProperties.HireApiUrl + "/Employee/" + profile.Id + "/ProfilePhoto/" + profile.ProfilePhoto : string.Empty;
                                }
                                piplineViewModel1.PiplineCandidatesViewModel.Add(piplineCandidatesViewModel1);
                            }
                            pipeline.PiplineViewModel.Add(piplineViewModel1);
                        }
                    }

                    respModel.Status = true;
                    respModel.SetResult(pipeline);
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
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(respModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;
            }
            return respModel;
        }

        #endregion


        #region Activities 


        public async Task<GetResponseViewModel<JobActivities>> GetJobActivities(int JobId)
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
                    var groupByActionTypes = response.GroupBy(x => x.ActivityType).Select(x => x.Key).ToList();

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

                    var groupByUsers = response.GroupBy(x => x.CreatedBy).Select(grp => grp.First()).ToList();
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

        #endregion


        #region Website 

        public async Task<GetResponseViewModel<PortalJobModel>> GetPortalJobsList(PortalJobSearchViewModel portalJobSearchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<PortalJobModel>();

            try
            {
                portalJobSearchViewModel.CountryId = portalJobSearchViewModel.CountryId == 0 ? null : portalJobSearchViewModel.CountryId;
                portalJobSearchViewModel.CurrentPage = (portalJobSearchViewModel.CurrentPage - 1) * portalJobSearchViewModel.PerPage;
                var dtls = await dbContext.GetPortalJobsList(portalJobSearchViewModel.PerPage, portalJobSearchViewModel.CurrentPage, portalJobSearchViewModel.CountryId, portalJobSearchViewModel.LocationId, portalJobSearchViewModel.SearchKeyModel);
                respModel.Status = true;
                respModel.SetResult(dtls);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(portalJobSearchViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }


        public async Task<GetResponseViewModel<List<CountryWiseJobCountModel>>> GetCountryWiseJobCounts()
        {

            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<CountryWiseJobCountModel>>();

            try
            {
                var dtls = await dbContext.GetCountryWiseJobCounts();
                respModel.Status = true;
                respModel.SetResult(dtls);
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


        public async Task<GetResponseViewModel<List<LocationWiseJobCountModel>>> GetLocationWiseJobCounts(int CountryId)
        {

            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<LocationWiseJobCountModel>>();

            try
            {
                var dtls = await dbContext.GetLocationWiseJobCounts(CountryId);
                respModel.Status = true;
                respModel.SetResult(dtls);
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


        public async Task<GetResponseViewModel<CandidateActiveArchivedJobViewModel>> GetCandidateActiveArchivedJobs(int CanPrfId, int FilterType)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<CandidateActiveArchivedJobViewModel>();
            try
            {
                var candidateActiveArchivedJobs = new List<CandidateActiveArchivedJobs>();
                var response = new CandidateActiveArchivedJobViewModel();

                var dtls = await dbContext.GetCandidateActiveArchivedJobs(CanPrfId, FilterType);
                var CandStatus = dbContext.PhCandStatusSes.ToList();
                var CandDocs = dbContext.PhCandidateDocs.Where(x => x.CandProfId == CanPrfId).ToList();
                foreach (var item in dtls)
                {
                    var candidateActiveArchivedJob = new CandidateActiveArchivedJobs
                    {
                        ClientId = item.ClientId,
                        JobId = item.JobId.Value,
                        ClientName = item.ClientName,
                        JobTitle = item.JobTitle,
                        JobDescription = item.JobDescription,
                        JobRole = item.JobRole,
                        PostedDate = item.PostedDate,
                        ClosedDate = item.ClosedDate,
                        JobCountryId = item.JobCountryId,
                        JobCountryName = item.JobCountryName,
                        CandidateCity = item.CandidateCity,
                        CandidateCountry = item.CandidateCountry,
                        JobCityName = item.JobCityName,
                        MinExp = item.MinExp == null ? 0 : item.MinExp.Value,
                        MaxExp = item.MaxExp == null ? 0 : item.MaxExp.Value,
                        EPTakeHomePerMonth = item.EPTakeHomePerMonth,
                        EPCurrency = item.EPCurrency,
                        JobCurrency = item.JobCurrency,
                        MinSalary = item.MinSalary,
                        MaxSalary = item.MaxSalary,
                        OPCurrency = item.OPCurrency,
                        OPGrossPayPerAnnum = item.OPGrossPayPerAnnum,
                        OPConfirmFlag = item.OPConfirmFlag,
                        OPDeductionsPerAnnum = item.OPDeductionsPerAnnum,
                        OPNetPayPerAnnum = item.OPNetPayPerAnnum,
                        OPVarPayPerAnnum = item.OPVarPayPerAnnum,
                        OPGrossPayPerMonth = item.OPGrossPayPerMonth,
                        CPCurrency = item.CPCurrency,
                        CPTakeHomeSalPerMonth = item.CPTakeHomeSalPerMonth == null ? 0 : item.CPTakeHomeSalPerMonth.Value,
                        CandProfStatusCode = CandStatus.Where(x => x.Id == item.CandProfStatus).Select(x => x.Cscode).FirstOrDefault(),
                        ShortJobDesc = item.ShortJobDesc,
                        AppliedDate = item.AppliedDate,
                        RecruiterEmail = item.RecruiterEmail,
                        RecruiterId = item.RecruiterId,
                        RecruiterMobile = item.RecruiterMobile,
                        RecuiterRole = item.RecuiterRole,
                        RecruiterName = item.RecruiterName,
                        RecuiterPhoto = item.RecuiterPhoto
                    };

                    if (!string.IsNullOrEmpty(candidateActiveArchivedJob.RecuiterPhoto) && candidateActiveArchivedJob.RecruiterId != null)
                    {
                        candidateActiveArchivedJob.RecuiterPhoto = getEmployeePhotoUrl(candidateActiveArchivedJob.RecuiterPhoto, candidateActiveArchivedJob.RecruiterId.Value);
                    }

                    var submittedDocuments = (from docs in CandDocs
                                              where docs.Joid == candidateActiveArchivedJob.JobId
                                              && docs.CandProfId == CanPrfId && docs.Status != (byte)RecordStatus.Delete
                                              && (docs.DocStatus == (byte)DocStatus.Notreviewd || docs.DocStatus == (byte)DocStatus.Accepted)
                                              select new CandidateDocumentsModel
                                              {
                                                  Id = docs.Id,
                                                  CandProfId = docs.CandProfId,
                                                  DocStatus = docs.DocStatus,
                                                  FileName = docs.FileName,
                                                  DocTypeName = docs.DocType,
                                                  FileGroup = docs.FileGroup,
                                                  Joid = docs.Joid,
                                                  DocType = string.IsNullOrEmpty(docs.DocType) ? 0 : dbContext.PhRefMasters.Where(x => x.Rmvalue == docs.DocType && x.GroupId == 15).Select(x => x.Id).FirstOrDefault(),
                                                  FileGroupName = string.Empty,
                                                  DocStatusName = string.Empty,
                                                  UploadedFromDrive = false
                                              }).ToList();
                    foreach (var subItem in submittedDocuments)
                    {
                        if (subItem.FileGroup != 0)
                        {
                            subItem.FileGroupName = Enum.GetName(typeof(FileGroup), subItem.FileGroup);
                        }
                        if (subItem.DocStatus != 0)
                        {
                            subItem.DocStatusName = Enum.GetName(typeof(DocStatus), subItem.DocStatus);
                        }
                        if (!string.IsNullOrEmpty(subItem.FileName))
                        {
                            if (ValidHttpURL(subItem.FileName))
                            {
                                subItem.FilePath = subItem.FileName;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(subItem.FileName))
                                {
                                    subItem.FileName = subItem.FileName.Replace("#", "%23");
                                    subItem.FilePath = appSettings.AppSettingsProperties.HireApiUrl + "/Candidate/" + subItem.CandProfId + "/" + subItem.FileName;
                                }
                            }
                        }
                    }

                    candidateActiveArchivedJob.CandidateSubmittedDocuments = submittedDocuments;
                    var pendingDocuments = (from docs in CandDocs
                                            where docs.Joid == candidateActiveArchivedJob.JobId && docs.CandProfId == CanPrfId && docs.Status != (byte)RecordStatus.Delete
                                            && docs.DocStatus == (byte)DocStatus.Requested
                                            select new CandidateDocumentsModel
                                            {
                                                Id = docs.Id,
                                                CandProfId = docs.CandProfId,
                                                DocStatus = docs.DocStatus,
                                                FileName = docs.FileName,
                                                DocTypeName = docs.DocType,
                                                FileGroup = docs.FileGroup,
                                                Joid = docs.Joid,
                                                DocType = string.IsNullOrEmpty(docs.DocType) ? 0 : dbContext.PhRefMasters.Where(x => x.Rmvalue == docs.DocType && x.GroupId == 15).Select(x => x.Id).FirstOrDefault(),
                                                FileGroupName = string.Empty,
                                                DocStatusName = string.Empty
                                            }).ToList();
                    foreach (var pendItem in pendingDocuments)
                    {
                        if (pendItem.FileGroup != 0)
                        {
                            pendItem.FileGroupName = Enum.GetName(typeof(FileGroup), pendItem.FileGroup);
                        }
                        if (pendItem.DocStatus != 0)
                        {
                            pendItem.DocStatusName = Enum.GetName(typeof(DocStatus), pendItem.DocStatus);
                        }
                    }

                    candidateActiveArchivedJob.JobSkillViewModel = new List<JobSkillViewModel>();
                    candidateActiveArchivedJob.JobSkillViewModel = (from tech in dbContext.PhJobOpeningSkills
                                                                    where tech.Joid == item.JobId && tech.Status == (byte)RecordStatus.Active
                                                                    select new JobSkillViewModel
                                                                    {
                                                                        ExpInMonths = tech.ExpMonth,
                                                                        ExpInYears = tech.ExpYears,
                                                                        TechnologyName = tech.Technology
                                                                    }).ToList();

                    candidateActiveArchivedJob.CandidatePeningDocuments = pendingDocuments;
                    candidateActiveArchivedJobs.Add(candidateActiveArchivedJob);
                }



                response.ActiveCandidateJobs = candidateActiveArchivedJobs;
                respModel.Status = true;

                respModel.SetResult(response);
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


        public async Task<GetResponseViewModel<List<CandidateArchivedJobModel>>> GetCandidateArchivedJobs(int CanPrfId)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<List<CandidateArchivedJobModel>>();
            try
            {

                var dtls = await dbContext.GetCandidateArchivedJobs(CanPrfId);

                respModel.Status = true;
                respModel.SetResult(dtls);
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

        public async Task<GetResponseViewModel<CandidateSimilarJobModel>> GetCandidateSimilarJobs(CandidateSimilarJobSearchViewModel candidateSimilarJobSearchViewModel)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            int UserId = Usr.Id;
            var respModel = new GetResponseViewModel<CandidateSimilarJobModel>();

            try
            {
                string JobIds = string.Empty;
                var CanJobIds = dbContext.PhJobCandidates.Where(x => x.CandProfId == candidateSimilarJobSearchViewModel.CanPrfId).Select(x => x.Joid).ToList();
                if (CanJobIds.Count > 0)
                {
                    foreach (var item in CanJobIds)
                    {
                        if (string.IsNullOrEmpty(JobIds))
                        {
                            JobIds = "" + item + "";
                        }
                        else
                        {
                            JobIds += "," + item + "";
                        }
                    }
                }

                candidateSimilarJobSearchViewModel.CountryId = candidateSimilarJobSearchViewModel.CountryId == 0 ? null : candidateSimilarJobSearchViewModel.CountryId;

                candidateSimilarJobSearchViewModel.CurrentPage = (candidateSimilarJobSearchViewModel.CurrentPage.Value - 1) * candidateSimilarJobSearchViewModel.PerPage.Value;

                var dtls = await dbContext.GetCandidateSimilarJobs(JobIds, candidateSimilarJobSearchViewModel.PerPage, candidateSimilarJobSearchViewModel.CurrentPage, candidateSimilarJobSearchViewModel.SearchKey, candidateSimilarJobSearchViewModel.CountryId, candidateSimilarJobSearchViewModel.CanPrfId);

                respModel.Status = true;

                foreach (var item in dtls.CandidateSimilarJobs)
                {
                    if (item.MinExpeInMonths != 0)
                    {
                        item.MinExp = ConvertYears(item.MinExpeInMonths);
                    }
                    if (item.MaxExpeInMonths != 0)
                    {
                        item.MaxExp = ConvertYears(item.MaxExpeInMonths);
                    }
                }

                respModel.SetResult(dtls);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, LoggingEvents.ListItems, ",respModel:" + Newtonsoft.Json.JsonConvert.SerializeObject(candidateSimilarJobSearchViewModel), respModel.Meta.RequestID, ex);

                respModel.Status = false;
                respModel.Meta.SetError(ApiResponseErrorCodes.Exception, string.Empty);
                respModel.Result = null;

            }
            return respModel;
        }

        #endregion


        #region  Client view 


        public async Task<GetResponseViewModel<List<ClientSharedCandidatesModel>>> GetCandidatesSharedToClient(int JobId, int type)
        {

            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<List<ClientSharedCandidatesModel>>();

            try
            {
                var dtls = await dbContext.GetCandidatesSharedToClient(JobId, type);

                respModel.Status = true;
                respModel.SetResult(dtls);
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




        public async Task<GetResponseViewModel<JobAssignmentViewModel>> GetTodayJobAssignments()
        {

            logger.SetMethodName(MethodBase.GetCurrentMethod());
            var respModel = new GetResponseViewModel<JobAssignmentViewModel>();

            try
            {
                var dtls = await dbContext.GetTodayJobAssignments();
                var jobAssignmentViewModel = new JobAssignmentViewModel
                {
                    BDMAssignViewModel = new List<BDMAssignViewModel>(),
                    RecAssignViewModel = new List<RecAssignViewModel>()
                };

                var bdmDtls = dtls.GroupBy(x => x.BdmName);
                foreach (var item in bdmDtls)
                {
                    var bDMAssignViewModel = new BDMAssignViewModel
                    {
                        BdmName = item.Key,
                        BdmId = item.FirstOrDefault().BdmId,
                        BdmJobAssignmentViewModel = new List<BdmJobAssignmentViewModel>(),
                        CvTarget = 0,
                        CvTargetFilled = 0,
                        NoCVSRequired = 0,
                        NoOfFinalCVsFilled = 0
                    };
                    foreach (var item1 in item)
                    {
                        var bdmJobAssignmentViewModel = new BdmJobAssignmentViewModel
                        {
                            BdmId = item1.BdmId,
                            BdmName = item1.BdmName,
                            ClosedDate = item1.ClosedDate,
                            CvTargetDate = item1.CvTargetDate,
                            JobTitle = item1.JobTitle,
                            JoId = item1.JoId,
                            NoCVSRequired = item1.NoCVSRequired,
                            NoOfFinalCVsFilled = item1.NoOfFinalCVsFilled,
                            ProfilesUploaded = item1.ProfilesUploaded,
                            PostedDate = item1.PostedDate,
                            RecId = item1.RecId,
                            RecName = item1.RecName,
                            CvTarget = item1.CvTarget,
                            AssignBy = item1.AssignBy,
                            CvTargetFilled = item1.CvTargetFilled
                        };
                        bDMAssignViewModel.CvTargetFilled += item1.CvTargetFilled == null ? 0 : item1.CvTargetFilled;
                        bDMAssignViewModel.CvTarget += item1.CvTarget == null ? 0 : item1.CvTarget;
                        bDMAssignViewModel.NoCVSRequired += item1.NoCVSRequired == null ? 0 : item1.NoCVSRequired;
                        bDMAssignViewModel.NoOfFinalCVsFilled += item1.NoOfFinalCVsFilled == null ? 0 : item1.NoOfFinalCVsFilled;
                        bDMAssignViewModel.BdmJobAssignmentViewModel.Add(bdmJobAssignmentViewModel);
                    }
                    jobAssignmentViewModel.BDMAssignViewModel.Add(bDMAssignViewModel);
                }

                var recDtls = dtls.GroupBy(x => x.RecName);
                foreach (var item in recDtls)
                {
                    var recAssignViewModel = new RecAssignViewModel
                    {
                        RecName = item.Key,
                        RecId = item.FirstOrDefault().RecId,
                        RecJobAssignmentViewModel = new List<RecJobAssignmentViewModel>(),
                        NoOfFinalCVsFilled = 0,
                        NoCVSRequired = 0,
                        CvTarget = 0,
                        CvTargetFilled = 0
                    };
                    foreach (var item1 in item)
                    {
                        var recJobAssignmentViewModel = new RecJobAssignmentViewModel
                        {
                            BdmId = item1.BdmId,
                            BdmName = item1.BdmName,
                            ClosedDate = item1.ClosedDate,
                            CvTargetDate = item1.CvTargetDate,
                            JobTitle = item1.JobTitle,
                            JoId = item1.JoId,
                            NoCVSRequired = item1.NoCVSRequired,
                            NoOfFinalCVsFilled = item1.NoOfFinalCVsFilled,
                            ProfilesUploaded = item1.ProfilesUploaded,
                            PostedDate = item1.PostedDate,
                            RecId = item1.RecId,
                            RecName = item1.RecName,
                            CvTarget = item1.CvTarget,
                            AssignBy = item1.AssignBy,
                            CvTargetFilled = item1.CvTargetFilled
                        };
                        recAssignViewModel.CvTargetFilled += item1.CvTargetFilled == null ? 0 : item1.CvTargetFilled;
                        recAssignViewModel.CvTarget += item1.CvTarget == null ? 0 : item1.CvTarget;
                        recAssignViewModel.NoCVSRequired += item1.NoCVSRequired == null ? 0 : item1.NoCVSRequired;
                        recAssignViewModel.NoOfFinalCVsFilled += item1.NoOfFinalCVsFilled == null ? 0 : item1.NoOfFinalCVsFilled;
                        recAssignViewModel.RecJobAssignmentViewModel.Add(recJobAssignmentViewModel);
                    }
                    jobAssignmentViewModel.RecAssignViewModel.Add(recAssignViewModel);
                }

                respModel.Status = true;
                respModel.SetResult(jobAssignmentViewModel);
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
