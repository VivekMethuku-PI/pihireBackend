using PiHire.BAL.Common.Types;
using PiHire.BAL.ViewModels;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.Repositories
{
    public class CreateOpeningViewModel
    {
        [Required]
        public int? Puid { get; set; }
        [Required]
        public int? Buid { get; set; }

        [Required]
        public int ClientId { get; set; }
        [Required]
        [MaxLength(100)]
        public string ClientName { get; set; }
        public int? Spocid { get; set; }

        [Required]
        public bool ClientReviewFlag { get; set; }
        public int? BroughtBy { get; set; }


        [Required]
        public int? Priority { get; set; }  // Priority
        //[MaxLength(500)]
        //public string JobTitle { get; set; }
        [MaxLength(100)]
        public string JobRole { get; set; }

        //Templates
        [MaxLength(1000)]
        public string ShortJobDesc { get; set; }
        [Required]
        public string JobDescription { get; set; }

        [Required]
        public DateTime StartDate { get; set; } //PostedDate   
        [Required]
        public DateTime ClosedDate { get; set; } // EndDate
        public DateTime? ApprJoinDate { get; set; } // JoiningDate 

        [Required]
        public int? NoOfPositions { get; set; }
        [Required]
        public int? NoOfCvsRequired { get; set; }

        [Required]
        public int? CurrencyId { get; set; }
        //[MaxLength(15)]
        //public string JobCurrency { get; set; }
        [Required]
        public int? ClientBilling { get; set; }
        [Required]
        public decimal? MinSalary { get; set; }
        [Required]
        public decimal? MaxSalary { get; set; }


        //Job candidate desirable
        public CreateOpeningPrefTypeViewModel<int> JobDesirableDomain { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableSpecializations { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableImplementations { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableDesigns { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableDevelopments { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableSupports { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableQualities { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableDocumentations { get; set; }

        public CreateOpeningPrefTypeViewModel<int> JobDesirableTeamRole { get; set; }


        [Required]
        public int CountryId { get; set; }
        //[MaxLength(200)]
        //public string JobCountryName { get; set; }
        [Required]
        public int CityId { get; set; } //JobLocationId
        //[MaxLength(200)]
        //public string JobCityName { get; set; }
        public bool? LocalAnyWhere { get; set; }

        [Required]
        public int? JobCategoryId { get; set; }
        [MaxLength(100)]
        public string JobCategory { get; set; } // OpeningType

        public int? JobTenure { get; set; }

        public CreateOpeningPrefTypeViewModel<int> JobWorkPattern { get; set; }


        //Candidate Preference
        public CreateOpeningPrefTypeViewModel<bool> CandidatePrefValidPassport { get; set; }
        public CreateOpeningPrefTypeViewModel<bool> CandidatePrefDOB { get; set; }
        public CreateOpeningPrefTypeViewModel<int> CandidatePrefGender { get; set; }
        public CreateOpeningPrefTypeViewModel<int> CandidatePrefMaritalStatus { get; set; }
        public CreateOpeningPrefTypeViewModel<int> CandidatePrefLanguage { get; set; }
        public CreateOpeningPrefTypeViewModel<int> CandidatePrefVisaPreference { get; set; }
        public CreateOpeningPrefTypeViewModel<int> CandidatePrefRegion { get; set; }
        public CreateOpeningPrefTypeViewModel<bool> CandidatePrefNationality { get; set; }
        public CreateOpeningPrefTypeViewModel<bool> CandidatePrefResidingCountry { get; set; }
        public CreateOpeningPrefTypeViewModel<bool> CandidatePrefResidingCity { get; set; }
        public CreateOpeningPrefTypeViewModel<int> CandidatePrefDrivingLicence { get; set; }
        public CreateOpeningPrefTypeViewModel<bool> CandidatePrefEmployeeStatus { get; set; }
        public CreateOpeningPrefTypeViewModel<bool> CandidatePrefResume { get; set; }
        public CreateOpeningPrefTypeViewModel<bool> CandidatePrefVidPrfl { get; set; }
        public CreateOpeningPrefTypeViewModel<bool> CandidatePrefPaySlp { get; set; }
        public CreateOpeningPrefTypeViewModel<bool> CandidatePrefNoticePeriod { get; set; }

        //Job Skill template
        public List<CreateOpeningSkillSetViewModel> OpeningSkillSet { get; set; }
        public List<CreateOpeningCandidateEducationQualificationViewModel> CandidateEducationQualifications { get; set; }
        public List<CreateOpeningPrefTypeViewModel<int>> CandidateEducationCertifications { get; set; }
        public List<CreateOpeningPrefTypeViewModel<int>> OpeningQualifications { get; set; }
        public List<CreateOpeningPrefTypeViewModel<int>> OpeningCertifications { get; set; }

        [Required]
        public CreateOpeningPrefTypeViewModel<int> MinYears { get; set; }
        [Required]
        public CreateOpeningPrefTypeViewModel<int> MaxYears { get; set; }

        [Required]
        public CreateOpeningPrefTypeViewModel<int> ReleventExpMinYears { get; set; }
        [Required]
        public CreateOpeningPrefTypeViewModel<int> ReleventExpMaxYears { get; set; }


        [Required]
        public CreateOpeningPrefTypeViewModel<int> NoticePeriod { get; set; }

        public List<CreateOpeningAssessmentViewModel> OpeningAssessments { get; set; }

        //public int OfficeId { get; set; }

        //public List<CreateOpeningQtns> OpeningQtns { get; set; }
    }
    public class CreateOpeningSkillSetViewModel
    {
        [Required]
        public int? TechnologyId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Technology { get; set; }
        public int? ExpMonth { get; set; }
        public int? ExpYears { get; set; }

        [Required]
        public JobOpeningPreferenceTypes PreferenceType { get; set; }
    }
    public class CreateOpeningCandidateEducationQualificationViewModel
    {
        [Required]
        public int Qualification { get; set; }
        [Required]
        public int Course { get; set; }
        [Required]
        public JobOpeningPreferenceTypes PreferenceType { get; set; }
    }
    public class CreateOpeningKnowledgePrefViewModel
    {
        //[Required]
        //[MaxLength(5)]
        //public string FieldCode { get; set; }
        [Required]
        public int TechnologyId { get; set; }
        [Required]
        public int ExpYears { get; set; }
        [Required]
        public JobOpeningPreferenceTypes PreferenceType { get; set; }
    }
    public class CreateOpeningPrefTypeViewModel<T>
    {
        [Required]
        public T Value { get; set; }
        [Required]
        public JobOpeningPreferenceTypes PreferenceType { get; set; }
    }


    public class CRM_createJobOpeningViewModel
    {
        public string opportunity_name { get; set; }

        public int? account_id { get; set; }
        public int spocId { get; set; }

        public int? process_Unit { get; set; }
        public int? business_Unit { get; set; }

        public decimal? negotiated_price { get; set; }
        public DateTime opp_start_date { get; set; }
        public DateTime? opp_end_date { get; set; }
        public decimal? quantity { get; set; }
    }

    public class DeAssignJobViewmodel
    {
        public int JobId { get; set; }
        public int RecId { get; set; }
    }

    public class MoreCVPerJobViewModel
    {
        [Required]
        [Range(1, 100)]
        public int CvRequired { get; set; }
        public int JobId { get; set; }

    }


    public class MoreCVPerJobRecruiterViewModel
    {

        [Required]
        public int JobId { get; set; }
        public List<CVAssigntoTeamMembers> CVAssigntoTeamMembers { get; set; }

    }


    public class CVAssigntoTeamMembers
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public short NoOfCvs { get; set; }
    }



    public class UpdateOpeningViewModel
    {
        [Required]
        public int JobId { get; set; }
        [Required]
        public int JobStatusId { get; set; }
    }


    public class UpdateOpeningCertification
    {
        [Required]
        public int Certification { get; set; }
    }

    public class GetOpeningCertification
    {
        [Required]
        public int Certification { get; set; }
        public string CertificationName { get; set; }
        public int JoId { get; set; }
        public int Status { get; set; }
    }

    public class UpdateOpeningQualification
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int Qualification { get; set; }
        [Required]
        public int Course { get; set; }
    }

    public class GetOpeningQualification
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int Qualification { get; set; }
        public string QualificationName { get; set; }
        [Required]
        public int Course { get; set; }
        public string CourseName { get; set; }
        public int JoId { get; set; }
        public int Status { get; set; }
    }

    public class UpdateOpeningQtns
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; }
        [Required]
        public byte QuestionType { get; set; }
        [Required]
        public byte Slno { get; set; }
    }

    public class GetOpeningQtns
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; }
        [Required]
        public byte QuestionType { get; set; }
        [Required]
        public byte Slno { get; set; }
        public int JoId { get; set; }
        public byte Status { get; set; }
    }

    public class CreateOpeningCertification
    {
        [Required]
        public int Certification { get; set; }
    }


    public class CreateOpeningQtns
    {
        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; }
        [Required]
        public byte QuestionType { get; set; }
        [Required]
        public byte Slno { get; set; }
    }


    public class CreateOpeningAssessmentViewModel
    {
        public int? CandStatusId { get; set; }
        public string AssessmentId { get; set; }
        public int? StageId { get; set; }
        public string AssessmentName { get; set; }
        public string StageName { get; set; }
    }

    public class EditOpeningViewModel : CreateOpeningViewModel
    {
        [Required]
        public int Id { get; set; }
        //[Required]
        //public int? Puid { get; set; }
        //[Required]
        //public int? Buid { get; set; }
        //[MaxLength(100)]
        //public string JobCategory { get; set; } // Opening Type
        //[Required]
        //public int? Priority { get; set; }  // Priority
        //[Required]
        //public int ClientId { get; set; }
        //[Required]
        //[MaxLength(100)]
        //public string ClientName { get; set; }
        //public int? Spocid { get; set; }
        //[MaxLength(500)]
        //public string JobTitle { get; set; }
        //[MaxLength(100)]
        //public string JobRole { get; set; }
        //[Required]
        //public DateTime StartDate { get; set; } //ReceivedDate,PostedDate   
        //[Required]
        //public DateTime ClosedDate { get; set; }
        //public DateTime? ApprJoinDate { get; set; }
        //public int CountryId { get; set; }
        //[Required]
        //public int CityId { get; set; } //JobLocationId
        //[Required]
        //public int? MaxYears { get; set; }
        //[Required]
        //public int? MinYears { get; set; }
        //[Required]
        //public int? NoOfPositions { get; set; }
        //[Required]
        //public int? NoOfCvsRequired { get; set; }
        //[Required]
        //public decimal? MinSalary { get; set; }
        //[Required]
        //public decimal? MaxSalary { get; set; }
        //[Required]
        //public int? CurrencyId { get; set; }
        //[Required]
        //public int? NoticePeriod { get; set; }
        //[Required]
        //public string JobTenure { get; set; }
        //[Required]
        //public string JobDescription { get; set; }
        //public bool ClientReviewFlag { get; set; }
        //public int? BroughtBy { get; set; }

        //[Required]
        //public int ClientBilling { get; set; }
        //[MaxLength(1000)]
        //public string ShortJobDesc { get; set; }

        [Required]
        public string CurrentSpocName { get; set; }
        [Required]
        public string NewSpocName { get; set; }
        //public List<EditOpeningSkillSetViewModel> OpeningSkillSetViewModel { get; set; }
        //public List<EditOpeningPREFViewModel> OpeningPREFViewModel { get; set; }
        //public List<EditOpeningAssessmentViewModel> OpeningAssessmentViewModel { get; set; }
        //public List<UpdateOpeningCertification> OpeningCertification { get; set; }
        //public List<UpdateOpeningQualification> OpeningQualification { get; set; }
        //public List<UpdateOpeningQtns> OpeningQtns { get; set; }
    }

    //public class EditOpeningSkillSetViewModel
    //{
    //    [Required]
    //    public int Id { get; set; }
    //    [Required]
    //    public int? TechnologyId { get; set; }
    //    [Required]
    //    [MaxLength(200)]
    //    public string Technology { get; set; }
    //    public int? ExpMonth { get; set; }
    //    public int? ExpYears { get; set; }
    //}

    //public class EditOpeningPREFViewModel
    //{
    //    [Required]
    //    public int Id { get; set; }
    //    [Required]
    //    [MaxLength(5)]
    //    public string FieldCode { get; set; }
    //    [Required]
    //    public byte DisplayFlag { get; set; }
    //}

    //public class EditOpeningAssessmentViewModel
    //{
    //    [Required]
    //    public int Id { get; set; }
    //    public int? CandStatusId { get; set; }
    //    public string AssessmentId { get; set; }
    //    public int? StageId { get; set; }
    //    public string StageName { get; set; }
    //    public string AssessmentName { get; set; }
    //}

    public class GetOpeningViewModel
    {
        public int Id { get; set; }        
        public int JobOpeningStatus { get; set; }
        public string JobOpeningStatusName { get; set; }




        public int? Puid { get; set; }
        public int? Buid { get; set; }

        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public int? Spocid { get; set; }

        public bool ClientReviewFlag { get; set; }
        public int? BroughtBy { get; set; }


        public int? Priority { get; set; } 
        [MaxLength(100)]
        public string JobRole { get; set; }

        public string ShortJobDesc { get; set; }
        public string JobDescription { get; set; }

        public DateTime StartDate { get; set; } //PostedDate   
        public DateTime ClosedDate { get; set; } // EndDate
        public DateTime? ApprJoinDate { get; set; } // JoiningDate 

        public int? NoOfPositions { get; set; }
        public int? NoOfCvsRequired { get; set; }

        public int? CurrencyId { get; set; }
        public int? ClientBilling { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }


        //Job candidate desirable
        public GetOpeningViewModel_PrefType<int> JobDesirableDomain { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableSpecializations { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableImplementations { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableDesigns { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableDevelopments { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableSupports { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableQualities { get; set; }
        public List<CreateOpeningKnowledgePrefViewModel> JobDesirableDocumentations { get; set; }

        public GetOpeningViewModel_PrefType<int> JobDesirableTeamRole { get; set; }


        public int CountryId { get; set; }
        public int CityId { get; set; } //JobLocationId
        public bool? LocalAnyWhere { get; set; }

        public int? JobCategoryId { get; set; }
        public string JobCategory { get; set; } // OpeningType

        public int? JobTenure { get; set; }

        public GetOpeningViewModel_PrefType<int> JobWorkPattern { get; set; }


        //Candidate Preference
        public GetOpeningViewModel_PrefType<bool> CandidatePrefValidPassport { get; set; }
        public GetOpeningViewModel_PrefType<bool> CandidatePrefDOB { get; set; }
        public GetOpeningViewModel_PrefType<int> CandidatePrefGender { get; set; }
        public GetOpeningViewModel_PrefType<int> CandidatePrefMaritalStatus { get; set; }
        public GetOpeningViewModel_PrefType<int> CandidatePrefLanguage { get; set; }
        public GetOpeningViewModel_PrefType<int> CandidatePrefVisaPreference { get; set; }
        public GetOpeningViewModel_PrefType<int> CandidatePrefRegion { get; set; }
        public GetOpeningViewModel_PrefType<bool> CandidatePrefNationality { get; set; }
        public GetOpeningViewModel_PrefType<bool> CandidatePrefResidingCountry { get; set; }
        public GetOpeningViewModel_PrefType<bool> CandidatePrefResidingCity { get; set; }
        public GetOpeningViewModel_PrefType<int> CandidatePrefDrivingLicence { get; set; }
        public GetOpeningViewModel_PrefType<bool> CandidatePrefEmployeeStatus { get; set; }
        public GetOpeningViewModel_PrefType<bool> CandidatePrefResume { get; set; }
        public GetOpeningViewModel_PrefType<bool> CandidatePrefVidPrfl { get; set; }
        public GetOpeningViewModel_PrefType<bool> CandidatePrefPaySlp { get; set; }
        public GetOpeningViewModel_PrefType<bool> CandidatePrefNoticePeriod { get; set; }

        //Job Skill template
        public List<CreateOpeningSkillSetViewModel> OpeningSkillSet { get; set; }
        public List<CreateOpeningCandidateEducationQualificationViewModel> CandidateEducationQualifications { get; set; }
        public List<GetOpeningViewModel_PrefType<int>> CandidateEducationCertifications { get; set; }
        public List<GetOpeningViewModel_PrefType<int>> OpeningQualifications { get; set; }
        public List<GetOpeningViewModel_PrefType<int>> OpeningCertifications { get; set; }

        public GetOpeningViewModel_PrefType<int> MinYears { get; set; }
        public GetOpeningViewModel_PrefType<int> MaxYears { get; set; }

        public GetOpeningViewModel_PrefType<int> ReleventExpMinYears { get; set; }
        public GetOpeningViewModel_PrefType<int> ReleventExpMaxYears { get; set; }


        public GetOpeningViewModel_PrefType<int> NoticePeriod { get; set; }

        public List<CreateOpeningAssessmentViewModel> OpeningAssessments { get; set; }
    }
    public class GetOpeningViewModel_PrefType<T>
    {
        public T Value { get; set; }
        public JobOpeningPreferenceTypes? PreferenceType { get; set; }
    }

    public class GetOpeningSkillSetViewModel
    {
        public int Id { get; set; }
        public int? SkillLevelId { get; set; }
        public string SkillName { get; set; }
        public int? TechnologyId { get; set; }
        public string Technology { get; set; }
        public int? ExpMonth { get; set; }
        public int? ExpYears { get; set; }
        public int JobId { get; set; }
    }
    public class GetpeningPREFViewModel
    {
        public int Id { get; set; }
        public string FieldCode { get; set; }
        public string FieldCodeName { get; set; }
        public byte DisplayFlag { get; set; }
    }
    public class JobAssessmentViewModel
    {
        public int Id { get; set; }
        public int? CandStatusId { get; set; }
        public string CandStatusName { get; set; }
        public string AssessmentId { get; set; }
        public string AssessmentName { get; set; }
        public int? StageId { get; set; }
        public string StageName { get; set; }
        public string StageColor { get; set; }
        public int JobId { get; set; }
        public byte Status { get; set; }
        public string PreviewUrl { get; set; }
    }

    public class JobListViewModel
    {
        public int OpeningCount { get; set; }
        public List<JobsList> OpeningList { get; set; }
    }
    public class RecruitersJobListToAssignViewModel
    {
        public int OpeningsCount { get; set; }
        public List<RecruitersJobToAssignViewModel> OpeningList { get; set; }
    }
    public class RecruitersJobToAssignViewModel : JobsList
    {
        public DateTime? cvTargetDate { get; set; }
        public short? todayCvsRequired { get; set; }

        internal static List<RecruitersJobToAssignViewModel> ToModel(List<JobsList> jobList, Dictionary<int, DateTime?> cvTargetDates, Dictionary<int, short?> todayCvsRequired)
        {
            List<RecruitersJobToAssignViewModel> models = new List<RecruitersJobToAssignViewModel>();
            foreach (var job in jobList)
            {
                models.Add(ToModel(job, cvTargetDates, todayCvsRequired));
            }
            return models;
        }

        private static RecruitersJobToAssignViewModel ToModel(JobsList job, Dictionary<int, DateTime?> cvTargetDates, Dictionary<int, short?> todayCvsRequired)
        {
            return new RecruitersJobToAssignViewModel
            {
                AsmtCounter = job.AsmtCounter,
                CityId = job.CityId,
                CityName = job.CityName,
                ClientId = job.ClientId,
                ClientName = job.ClientName,
                ClientViewsCounter = job.ClientViewsCounter,
                ClosedDate = job.ClosedDate,
                CountryId = job.CountryId,
                CountryName = job.CountryName,
                CreatedBy = job.CreatedBy,
                CreatedByName = job.CreatedByName,
                CreatedDate = job.CreatedDate,
                EmailsCounter = job.EmailsCounter,
                Id = job.Id,
                JobDescription = job.JobDescription,
                JobOpeningStatus = job.JobOpeningStatus,
                JobOpeningStatusCounter = job.JobOpeningStatusCounter,
                JobOpeningStatusName = job.JobOpeningStatusName,
                JobPostingCounter = job.JobPostingCounter,
                JobRole = job.JobRole,
                JobTitle = job.JobTitle,
                Jscode = job.Jscode,
                MaxExp = job.MaxExp,
                MinExp = job.MinExp,
                ModificationBy = job.ModificationBy,
                ModificationOn = job.ModificationOn,
                ProfilesSharedToClientCounter = job.ProfilesSharedToClientCounter,
                ShortJobDesc = job.ShortJobDesc,
                StartDate = job.StartDate,
                Status = job.Status,


                isAssigned = job.isAssigned,
                cvTargetDate = cvTargetDates.ContainsKey(job.Id) ? cvTargetDates[job.Id] : null,
                todayCvsRequired = todayCvsRequired.ContainsKey(job.Id) ? todayCvsRequired[job.Id] : null,
            };
        }
    }

    public class JobAssignmentViewModel
    {
        public List<BDMAssignViewModel> BDMAssignViewModel { get; set; }
        public List<RecAssignViewModel> RecAssignViewModel { get; set; }
    }

    public class BDMAssignViewModel
    {
        public int BdmId { get; set; }
        public string BdmName { get; set; }
        public short? NoCVSRequired { get; set; }
        public int? NoOfFinalCVsFilled { get; set; }
        public int? CvTarget { get; set; }
        public int? CvTargetFilled { get; set; }
        public List<BdmJobAssignmentViewModel> BdmJobAssignmentViewModel { get; set; }
    }

    public class BdmJobAssignmentViewModel
    {
        public int RecId { get; set; }
        public string RecName { get; set; }
        public short? NoCVSRequired { get; set; }
        public int? NoOfFinalCVsFilled { get; set; }
        public short? ProfilesUploaded { get; set; }
        public int? CvTarget { get; set; }
        public byte? AssignBy { get; set; }
        public int JoId { get; set; }
        public string JobTitle { get; set; }
        public int BdmId { get; set; }
        public string BdmName { get; set; }
        public DateTime? CvTargetDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public DateTime PostedDate { get; set; }
        public int? CvTargetFilled { get; set; }
    }

    public class RecAssignViewModel
    {
        public int RecId { get; set; }
        public string RecName { get; set; }
        public short? NoCVSRequired { get; set; }
        public int? NoOfFinalCVsFilled { get; set; }
        public int? CvTarget { get; set; }
        public int? CvTargetFilled { get; set; }
        public List<RecJobAssignmentViewModel> RecJobAssignmentViewModel { get; set; }
    }

    public class RecJobAssignmentViewModel
    {
        public int RecId { get; set; }
        public string RecName { get; set; }
        public short? NoCVSRequired { get; set; }
        public int? NoOfFinalCVsFilled { get; set; }
        public short? ProfilesUploaded { get; set; }
        public int? CvTarget { get; set; }
        public byte? AssignBy { get; set; }
        public int JoId { get; set; }
        public string JobTitle { get; set; }
        public int BdmId { get; set; }
        public string BdmName { get; set; }
        public DateTime? CvTargetDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public DateTime PostedDate { get; set; }
        public int? CvTargetFilled { get; set; }
    }

    public class JobOpeningACTVCounterViewModel
    {
        public int? AsmtCounter { get; set; }
        public int? EmailsCounter { get; set; }
        public int? JobPostingCounter { get; set; }
        public int? ClientViewsCounter { get; set; }
    }

    public class OpeningListSearchViewModel
    {
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        [Required]
        public int? FilterType { get; set; }
        public string Sort { get; set; }
        [MaxLength(256)]
        public string SearchKey { get; set; }
    }

    public class JobsListToAssignRecruitersSearchViewModel
    {
        [Required]
        public int UserId { get; set; }
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        [MaxLength(256)]
        public string SearchKey { get; set; }
    }

    public class TagJobListSearchViewModel
    {
        [MaxLength(256)]
        public string SearchKey { get; set; }
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        [Required]
        public int? CandidateId { get; set; }
    }

    public class CloneOpeningViewModel
    {
        public int Id { get; set; }
    }

    public class JobNotesViewModel
    {
        public List<NotesViewModel> NotesViewModel { get; set; }
        public List<NoteCreatedBy> NoteCreatedBy { get; set; }

    }

    public class NotesViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string NotesDesc { get; set; }
        public int? NoteId { get; set; }
        public int? JobId { get; set; }
        public byte Status { get; set; }
        public string ProfilePhoto { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TimeDiff { get; set; }
        public string CreatedByName { get; set; }
        public int TagId { get; set; }
        public string TagName { get; set; }
        public List<NotesReplyViewModel> NotesReplyViewModel { get; set; }
    }

    public class NotesReplyViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string NotesDesc { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public string ProfilePhoto { get; set; }
        public string TimeDiff { get; set; }
        public int TagId { get; set; }
        public string TagName { get; set; }
        public byte Status { get; set; }
    }

    public class CreateJobNotesViewModel
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        [MaxLength(1000)]
        public string NotesDesc { get; set; }
        public int NoteId { get; set; }
        [Required]
        public int JobId { get; set; }
        public int CandId { get; set; }
        public List<int> TagId { get; set; }
    }
    public class MapAssessmentToJobViewModel
    {
        [Required]
        public int JobId { get; set; }
        public List<AssessmentIdViewModel> AssessmentIdViewModel { get; set; }
    }
    public class AssessmentIdViewModel
    {
        public int Id { get; set; }

        [Required]
        public int? CandStatusId { get; set; }
        [Required]
        public int? StageId { get; set; }
        [Required]
        public string AssessmentId { get; set; }
    }

    public class JobDescriptionViewModel
    {
        public int CreatedBy { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string JobRole { get; set; }
        public DateTime TargetDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? StartDate { get; set; }

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string JobTenure { get; set; }
        public int MinYear { get; set; }
        public int MaxYear { get; set; }
        public int? MaxExpeInMonths { get; set; }
        public int? MinExpeInMonths { get; set; }

        public string JobDescription { get; set; }
        public string ShortJobDesc { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public string EmploymentType { get; set; }
        public string BroughtByName { get; set; }
        public int? BroughtBy { get; set; }
        public string BroughtByRole { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedByRole { get; set; }

        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }

        public int JobStatus { get; set; }
        public string JobStatusCode { get; set; }
        public string JobStatusName { get; set; }
        public int? NoticePeriod { get; set; }

        public int? NoOfPositions { get; set; }
        public int? NoOfCvsRequired { get; set; }
        public int? NoOfFinalCVsFilled { get; set; }

        public string BroughtByProfilePhoto { get; set; }
        public string CreatedByProfilePhoto { get; set; }

        public string ClientName { get; set; }
        public int ClientId { get; set; }


        public int? PuId { get; set; }
        public string PuShortName { get; set; }
        public string PuName { get; set; }

        public int? BuId { get; set; }
        public string BuName { get; set; }

        public int? SpocId { get; set; }
        public string SpocName { get; set; }
        public string AccountManagerName { get; set; }


        public DateTime? ReopenedDate { get; set; }
        public string JobType { get; set; }
        public int CompletedAge { get; set; }
        public int JobAge { get; set; }

        public int? Priority { get; set; }
        public string PriorityName { get; set; }

        public bool AddCadidate { get; set; }
        public string LastModified { get; set; }
        public DateTime LastModifiedOn { get; set; }


        public List<JobSkillViewModel> JobDescSkillViewModel { get; set; }
        public List<JobWorkingTeamViewModl> JobTeam { get; set; }
        public List<JobOpeningStatusCounterViewModel> JobOpeningStatusCounter { get; set; }
    }

    public class JobInfoViewModel
    {
        public int JobId { get; set; }
        //public string JobTitle { get; set; }
        public string JobRole { get; set; }
        public string JobCategory { get; set; }
        public int? JobCategoryId { get; set; }
        //public string ClientName { get; set; }
        //public DateTime PostedDate { get; set; }
        //public DateTime? JoiningDate { get; set; }
        //public decimal? MinSalary { get; set; }
        //public decimal? MaxSalary { get; set; }
        //public int MinYear { get; set; }
        //public int MaxYear { get; set; }
        //public int? MaxExpeInMonths { get; set; }
        //public int? MinExpeInMonths { get; set; }
        public string JobDescription { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        //public string EmploymentType { get; set; }
        //public int? CurrencyId { get; set; }
        //public string CurrencyName { get; set; }
        //public int? NoticePeriod { get; set; }
        //public List<JobSkillViewModel> JobDescSkillViewModel { get; set; }
        //public List<GetpeningPREFViewModel> OpeningPREFViewModel { get; set; }
        //public List<GetOpeningCertification> OpeningCertification { get; set; }
        //public List<GetOpeningQualification> OpeningQualification { get; set; }
        //public List<GetOpeningQtns> OpeningQtns { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefValidPassport { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefDOB { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefGender { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefMaritalStatus { get; set; }
        public JobInfoViewModel_PrefType<int> CandidatePrefRegion { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefNationality { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefResidingCountry { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefResidingCity { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefDrivingLicence { get; set; }
        public JobInfoViewModel_PrefType<int> JobDesirableDomain { get; set; }
        public JobInfoViewModel_PrefType<int> CandidatePrefLanguage { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefVisaPreference { get; set; }
        public int? JobTenure { get; set; }
        public JobInfoViewModel_PrefType<int> JobWorkPattern { get; set; }
        public JobInfoViewModel_PrefType<int> JobDesirableTeamRole { get; set; }
        public JobOpeningPreferenceTypes? ExpeInMonthsPrefTyp { get; set; }
        public JobOpeningPreferenceTypes? ReleventExpInMonthsPrefTyp { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefPaySlp { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefEmployeeStatus { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefNoticePeriod { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefResume { get; set; }
        public JobOpeningPreferenceTypes? CandidatePrefVidPrfl { get; set; }
        public List<JobInfoViewModel_KnowledgePref> JobDesirableSpecializations { get; internal set; }
        public List<JobInfoViewModel_KnowledgePref> JobDesirableImplementations { get; internal set; }
        public List<JobInfoViewModel_KnowledgePref> JobDesirableDesigns { get; internal set; }
        public List<JobInfoViewModel_KnowledgePref> JobDesirableDevelopments { get; internal set; }
        public List<JobInfoViewModel_KnowledgePref> JobDesirableSupports { get; internal set; }
        public List<JobInfoViewModel_KnowledgePref> JobDesirableQualities { get; internal set; }
        public List<JobInfoViewModel_KnowledgePref> JobDesirableDocumentations { get; internal set; }
        public List<JobInfoViewModel_SkillSet> OpeningSkillSet { get; internal set; }
        public List<JobInfoViewModel_CandidateEducationQualification> CandidateEducationQualifications { get; internal set; }
        public List<JobInfoViewModel_PrefTypeWithName<int>> CandidateEducationCertifications { get; internal set; }
        public List<JobInfoViewModel_PrefTypeWithName<int>> OpeningCertifications { get; internal set; }
        public List<JobInfoViewModel_PrefTypeWithName<int>> OpeningQualifications { get; internal set; }        
    }
    public class JobInfoViewModel_KnowledgePref
    {
        public int TechnologyId { get; set; }
        public string Technology { get; set; }
        public JobOpeningPreferenceTypes PreferenceType { get; set; }
    }
    public class JobInfoViewModel_SkillSet
    {
        public int? TechnologyId { get; set; }        
        public string Technology { get; set; }

        public JobOpeningPreferenceTypes PreferenceType { get; set; }
    }
    public class JobInfoViewModel_CandidateEducationQualification
    {
        public int Qualification { get; set; }
        public string QualificationName { get; set; }
        public int Course { get; set; }
        public string CourseName { get; set; }
        public JobOpeningPreferenceTypes PreferenceType { get; set; }
    }
    public class JobInfoViewModel_PrefType<T>
    {
        public T Value { get; set; }
        public JobOpeningPreferenceTypes PreferenceType { get; set; }
    }
    public class JobInfoViewModel_PrefTypeWithName<T>
    {
        public T Value { get; set; }
        public JobOpeningPreferenceTypes PreferenceType { get; set; }
        public string ValueName { get; set; }
    }

    public class JobWorkingTeamViewModl
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Location { get; set; }
        public string ProfilePhoto { get; set; }
        public short? NoOfCv { get; set; }
        public int? NoOfFinalCvsFilled { get; set; }
        public DateTime? DeAssignDate { get; set; }
    }

    public class JobSkillViewModel
    {
        public int? SkillLevelId { get; set; }
        public string SkillLevel { get; set; }
        public int? TechnologyId { get; set; }
        public string TechnologyName { get; set; }
        public int? ExpInMonths { get; set; }
        public int? ExpInYears { get; set; }
    }

    public class GetJobAssignedTeamMembersViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public int UserId { get; set; }
        public int ActiveJobs { get; set; }
        public bool JobAssinged { get; set; }
        public bool CopyJobAssinged { get; set; }
        public string Location { get; set; }
        public int? LocationId { get; set; }
        public string RoleName { get; set; }
        public string Signature { get; set; }
        public string ProfilePhoto { get; set; }
    }

    public class JobAssignedMembersViewModel
    {
        [Required]
        public int JobId { get; set; }
        public List<AssignMembers> assignMembers { get; set; }
    }

    public class MultipleJobAssignmentMembersViewModel
    {
        public List<MultipleJobAssignmentMembers> MultipleJobAssignmentMembers { get; set; }
    }

    public class MultipleJobAssignmentMembers
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public short NoOfCvs { get; set; }
        [Required]
        public int JoId { get; set; }
        public DateTime? cvTargetDate { get; set; }
    }

    public class AssignMembers
    {
        [Required]
        public int UserId { get; set; }
        public bool Assign { get; set; }
        [Required]
        public short NoOfCvs { get; set; }
    }

    public class GetJobPiplineViewModel
    {
        public int AssociatedCandidateCount { get; set; }
        public List<PiplineViewModel> PiplineViewModel { get; set; }
    }

    public class PiplineViewModel
    {
        public int StageId { get; set; }
        public string StageName { get; set; }
        public string StageColor { get; set; }

        public List<PiplineCandidatesViewModel> PiplineCandidatesViewModel { get; set; }
    }

    public class PiplineCandidatesViewModel
    {
        public int UserId { get; set; }
        public int CandidateId { get; set; }
        public string Name { get; set; }
        public string ProfileAge { get; set; }
        public DateTime ProfileDate { get; set; }
        public int? CandidateStatusId { get; set; }
        public string CandidateStatuName { get; set; }
        public int StageId { get; set; }
        public string Cscode { get; set; }
        public string ProfilePhoto { get; set; }
        public bool isBlockListed { get; set; }

        public int JobId { get; set; }
        public string JobName { get; set; }
        public DateTime? stageUpdatedDate { get; set; }
        public string stageUpdatedAge { get; set; }
        public string stageUpdatedBy { get; set; }
        public string stageUpdatedByPhoto { get; set; }
        public int? RecruiterId { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterPhoto { get; set; }
    }

    public class JobActivities
    {
        public List<ActionTypesViewModel> ActionTypesViewModel { get; set; }
        public List<ActivityCreatedBy> ActivityCreatedBy { get; set; }
        public List<ActivitiesViewModel> ActivitiesViewModel { get; set; }
    }

    public class ActionTypesViewModel
    {
        public int Id { get; set; }
        public string ActionTypeName { get; set; }
    }

    public class ActivityCreatedBy
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string ProfilePhoto { get; set; }

    }

    public class NoteCreatedBy
    {
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string ProfilePhoto { get; set; }
    }

    public class ActivitiesViewModel
    {
        public int Id { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ActivityDesc { get; set; }
        public string GroupDate { get; set; }
        public byte ActivityType { get; set; }
        public string ProfilePhoto { get; set; }
    }


    public class CandidateSimilarJobSearchViewModel
    {
        [Required]
        public int CanPrfId { get; set; }
        public int? PerPage { get; set; }
        public int? CurrentPage { get; set; }
        [MaxLength(256)]
        public string SearchKey { get; set; }
        public int? CountryId { get; set; }
    }

    public class PortalJobSearchViewModel
    {
        [Required]
        public int PerPage { get; set; }
        [Required]
        public int CurrentPage { get; set; }
        public int? CountryId { get; set; }
        public int? LocationId { get; set; }
        public List<SearchKeyModel> SearchKeyModel { get; set; }
    }




    public class QryJobViewModel
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public string JobRole { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public int? JobLocationId { get; set; }
        public int CountryId { get; set; }
        public int JobOpeningStatus { get; set; }
        public int Status { get; set; }
        public string CountryName { get; set; }
        public string Jscode { get; set; }
        public string JobDescription { get; set; }
        public int MinExp { get; set; }
        public int MaxExp { get; set; }
        public int? MinExpeInMonths { get; set; }
        public int? MaxExpeInMonths { get; set; }
    }

    public class CandidateActiveArchivedJobs
    {
        public int JobId { get; set; }
        public int? ClientId { get; set; }
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public string JobRole { get; set; }
        public DateTime? PostedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public int? MinExp { get; set; }
        public int? MaxExp { get; set; }
        public bool? OPConfirmFlag { get; set; }
        public int? EPTakeHomePerMonth { get; set; }
        public string EPCurrency { get; set; }
        public string OPCurrency { get; set; }
        public int? OPGrossPayPerMonth { get; set; }

        public string JobCurrency { get; set; }
        public int? OPGrossPayPerAnnum { get; set; }
        public int? OPDeductionsPerAnnum { get; set; }
        public int? OPVarPayPerAnnum { get; set; }
        public int? OPNetPayPerAnnum { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }

        public string JobCityName { get; set; }
        public string CandidateCity { get; set; }
        public string CandidateCountry { get; set; }
        public string JobCountryName { get; set; }
        public int? JobCountryId { get; set; }
        public string CandProfStatusCode { get; set; }
        public string CPCurrency { get; set; }
        public int? CPTakeHomeSalPerMonth { get; set; }
        public string ShortJobDesc { get; set; }


        public int? RecruiterId { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterMobile { get; set; }
        public string RecuiterPhoto { get; set; }
        public string RecruiterEmail { get; set; }
        public string RecuiterRole { get; set; }
        public DateTime? AppliedDate { get; set; }

        public List<CandidateDocumentsModel> CandidatePeningDocuments { get; set; }

        public List<CandidateDocumentsModel> CandidateSubmittedDocuments { get; set; }

        public List<JobSkillViewModel> JobSkillViewModel { get; set; }
    }

    public class AutoAssignmentSearchViewModel
    {
        public int Joid { get; set; }
        public int Puid { get; set; }
        public int NoOfCvsRequired { get; set; }
        public int OfficeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Enddate { get; set; }
        public int CreatedBy { get; set; }
    }

    public class CandidateDocumentsModel
    {
        public int Id { get; set; }
        public int CandProfId { get; set; }
        public byte DocStatus { get; set; }
        public string DocStatusName { get; set; }
        public int? DocType { get; set; }
        public string DocTypeName { get; set; }
        public byte FileGroup { get; set; }
        public string FileGroupName { get; set; }
        public int Joid { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public bool UploadedFromDrive { get; set; }
    }

    public class SendingEmailsPerSuitableCandidateRequestViewModel
    {
        public int JoId { get; set; }
        public string JobTitle { get; set; }
        public string JobLocation { get; set; }
        public string JobPostedDate { get; set; }
        public string JobExp { get; set; }
        public string JobShortDesc { get; set; }
        public string JobDesc { get; set; }
        public string JobCurrency { get; set; }
        public int JobGrossPackage { get; set; }
        public int JobNetSalPerMonth { get; set; }
        public string JobApplyLink { get; set; }
        public string JobCloseDate { get; set; }
        public string ClinetName { get; set; }
        public string JobCountry { get; set; }
        public int UserId { get; set; }
        public List<GetOpeningSkillSetViewModel> JobSkills { get; set; }
    }

    public class CandidateActiveArchivedJobViewModel
    {
        public List<CandidateActiveArchivedJobs> ActiveCandidateJobs { get; set; }
    }
}
