using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using PiHire.DAL.Entities;

namespace PiHire.DAL
{
    public partial class PiHIRE2Context : DbContext
    {
        public PiHIRE2Context()
        {
        }

        public PiHIRE2Context(DbContextOptions<PiHIRE2Context> options)
            : base(options)
        {
        }


        public virtual DbSet<Cand> Cands { get; set; }

        public virtual DbSet<FileMigratationLog> FileMigratationLogs { get; set; }

        public virtual DbSet<PhActivityLog> PhActivityLogs { get; set; }

        public virtual DbSet<PhAuditLog> PhAuditLogs { get; set; }

        public virtual DbSet<PhBgJob> PhBgJobs { get; set; }

        public virtual DbSet<PhBgJobDetail> PhBgJobDetails { get; set; }

        public virtual DbSet<PhBlog> PhBlogs { get; set; }

        public virtual DbSet<PhCityWiseBenefit> PhCityWiseBenefits { get; set; }

        public virtual DbSet<PhCountryWiseBenefit> PhCountryWiseBenefits { get; set; }

        public virtual DbSet<PhCountryWiseAllowance> PhCountryWiseAllowances { get; set; }        

        public virtual DbSet<PhCandAssignment> PhCandAssignments { get; set; }

        public virtual DbSet<PhCandReofferDetail> PhCandReofferDetails { get; set; }

        public virtual DbSet<PhCandReofferDetailsTemp> PhCandReofferDetailsTemps { get; set; }

        public virtual DbSet<PhCandSocialPref> PhCandSocialPrefs { get; set; }

        public virtual DbSet<PhCandStageMap> PhCandStageMaps { get; set; }

        public virtual DbSet<PhCandStagesS> PhCandStagesSes { get; set; }

        public virtual DbSet<PhCandStatusConfig> PhCandStatusConfigs { get; set; }

        public virtual DbSet<PhCandStatusS> PhCandStatusSes { get; set; }

        public virtual DbSet<PhCandidateBgvDetail> PhCandidateBgvDetails { get; set; }

        public virtual DbSet<PhCandidateCertification> PhCandidateCertifications { get; set; }

        public virtual DbSet<PhCandidateDoc> PhCandidateDocs { get; set; }

        public virtual DbSet<PhCandidateEduDetail> PhCandidateEduDetails { get; set; }

        public virtual DbSet<PhCandidateEmpmtDetail> PhCandidateEmpmtDetails { get; set; }

        public virtual DbSet<PhCandidateProfile> PhCandidateProfiles { get; set; }

        public virtual DbSet<PhCandidateProfilesShared> PhCandidateProfilesShareds { get; set; }

        public virtual DbSet<PhCandidateRecruitersHistory> PhCandidateRecruitersHistories { get; set; }

        public virtual DbSet<PhCandidateSkillset> PhCandidateSkillsets { get; set; }

        public virtual DbSet<PhCandidateStatusLog> PhCandidateStatusLogs { get; set; }

        public virtual DbSet<PhCandidateTag> PhCandidateTags { get; set; }

        public virtual DbSet<PhChatMessage> PhChatMessages { get; set; }

        public virtual DbSet<PhChatRoom> PhChatRooms { get; set; }

        public virtual DbSet<PhCity> PhCities { get; set; }

        public virtual DbSet<PhCountry> PhCountries { get; set; }

        public virtual DbSet<PhCurrencyExchangeRate> PhCurrencyExchangeRates { get; set; }

        public virtual DbSet<PhDayWiseJobAction> PhDayWiseJobActions { get; set; }

        public virtual DbSet<PhEmpLeaveRequest> PhEmpLeaveRequests { get; set; }

        public virtual DbSet<PhIntegrationsS> PhIntegrationsSes { get; set; }

        public virtual DbSet<PhJobAssignment> PhJobAssignments { get; set; }
        public virtual DbSet<PhJobAssignmentHistory> PhJobAssignmentHistories { get; set; }

        public virtual DbSet<PhJobAssignmentsDayWise> PhJobAssignmentsDayWises { get; set; }
        public virtual DbSet<PhJobAssignmentsDayWiseLog> PhJobAssignmentsDayWisesLogs { get; set; }

        public virtual DbSet<PhJobCandidate> PhJobCandidates { get; set; }
        public virtual DbSet<PhJobCandidateSkillset> PhJobCandidateSkillsets { get; set; }
        public virtual DbSet<PhJobCandidateOpeningsQualification> PhJobCandidateOpeningsQualifications { get; set; }
        public virtual DbSet<PhJobCandidateOpeningsCertification> PhJobCandidateOpeningsCertifications { get; set; }        

        public virtual DbSet<PhJobCandidateAssemt> PhJobCandidateAssemts { get; set; }

        public virtual DbSet<PhJobCandidateEvaluation> PhJobCandidateEvaluations { get; set; }

        public virtual DbSet<PhJobCandidateInterview> PhJobCandidateInterviews { get; set; }

        public virtual DbSet<PhJobCandidateStResponse> PhJobCandidateStResponses { get; set; }

        public virtual DbSet<PhJobOfferAllowance> PhJobOfferAllowances { get; set; }

        public virtual DbSet<PhJobOfferLetter> PhJobOfferLetters { get; set; }

        public virtual DbSet<PhJobOfferSlabDetail> PhJobOfferSlabDetails { get; set; }

        public virtual DbSet<PhJobOpening> PhJobOpenings { get; set; }

        public virtual DbSet<PhJobOpeningActvCounter> PhJobOpeningActvCounters { get; set; }

        public virtual DbSet<PhJobOpeningAssmt> PhJobOpeningAssmts { get; set; }

        public virtual DbSet<PhJobOpeningPref> PhJobOpeningPrefs { get; set; }

        public virtual DbSet<PhJobOpeningSkill> PhJobOpeningSkills { get; set; }

        public virtual DbSet<PhJobOpeningStQtn> PhJobOpeningStQtns { get; set; }

        public virtual DbSet<PhJobOpeningStatusCounter> PhJobOpeningStatusCounters { get; set; }

        public virtual DbSet<PhJobOpeningsAddlDetail> PhJobOpeningsAddlDetails { get; set; }

        public virtual DbSet<PhJobOpeningsDesirables> PhJobOpeningsDesirables { get; set; }

        public virtual DbSet<PhJobOpeningsDesirableSkill> PhJobOpeningsDesirableSkills { get; set; }

        public virtual DbSet<PhJobRecruiterPriority> PhJobRecruiterPriorities { get; set; }

        public virtual DbSet<PhJobOpeningsCertification> PhJobOpeningsCertifications { get; set; }
        public virtual DbSet<PhJobOpeningsQualification> PhJobOpeningsQualifications { get; set; }

        public virtual DbSet<PhJobStatusS> PhJobStatusSes { get; set; }

        public virtual DbSet<PhMediaFile> PhMediaFiles { get; set; }

        public virtual DbSet<PhMessageTemplate> PhMessageTemplates { get; set; }

        public virtual DbSet<PhNote> PhNotes { get; set; }

        public virtual DbSet<PhNotesSendList> PhNotesSendLists { get; set; }

        public virtual DbSet<PhNotification> PhNotifications { get; set; }

        public virtual DbSet<PhNotificationsUser> PhNotificationsUsers { get; set; }

        public virtual DbSet<PhRefMaster> PhRefMasters { get; set; }

        public virtual DbSet<PhEducationQualificationMaster> PhEducationQualificationMasters { get; set; }

        public virtual DbSet<PhSalaryComp> PhSalaryComps { get; set; }

        public virtual DbSet<PhSalarySlabsS> PhSalarySlabsSes { get; set; }

        public virtual DbSet<PhSalarySlabsWiseCompsS> PhSalarySlabsWiseCompsSes { get; set; }

        public virtual DbSet<PhShift> PhShifts { get; set; }

        public virtual DbSet<PhShiftDetl> PhShiftDetls { get; set; }

        public virtual DbSet<PhTechnologyGroupsS> PhTechnologyGroupsSes { get; set; }

        public virtual DbSet<PhTechnologysS> PhTechnologysSes { get; set; }

        public virtual DbSet<PhTestimonial> PhTestimonials { get; set; }

        public virtual DbSet<PhUsersConfig> PhUsersConfigs { get; set; }

        public virtual DbSet<PhUsersRemark> PhUsersRemarks { get; set; }

        public virtual DbSet<PhWorkflow> PhWorkflows { get; set; }

        public virtual DbSet<PhWorkflowsDet> PhWorkflowsDets { get; set; }

        public virtual DbSet<PiAppModulesS> PiAppModulesSes { get; set; }

        public virtual DbSet<PiAppTasksS> PiAppTasksSes { get; set; }

        public virtual DbSet<PiAppUserPuBu> PiAppUserPuBus { get; set; }

        public virtual DbSet<PiAppUserResp> PiAppUserResps { get; set; }

        public virtual DbSet<PiAppUserRole> PiAppUserRoles { get; set; }

        public virtual DbSet<PiAppUserRoleMap> PiAppUserRoleMaps { get; set; }

        public virtual DbSet<PiAppUserRoleResp> PiAppUserRoleResps { get; set; }

        public virtual DbSet<PiEmailServiceProvider> PiEmailServiceProviders { get; set; }

        public virtual DbSet<PiHireUser> PiHireUsers { get; set; }

        public virtual DbSet<PiUserLog> PiUserLogs { get; set; }

        public virtual DbSet<PiUserTxnLog> PiUserTxnLogs { get; set; }

        public virtual DbSet<TblParamProcessUnitMaster> TblParamProcessUnitMasters { get; set; }

        public virtual DbSet<TblParamPuBusinessUnit> TblParamPuBusinessUnits { get; set; }

        public virtual DbSet<TblParamPuOfficeLocation> TblParamPuOfficeLocations { get; set; }

        public virtual DbSet<TblParamRoleMaster> TblParamRoleMasters { get; set; }

        public virtual DbSet<TmpAllCandidate> TmpAllCandidates { get; set; }

        public virtual DbSet<VwAllCandidate> VwAllCandidates { get; set; }

        public virtual DbSet<VwAllrecruiter> VwAllrecruiters { get; set; }

        public virtual DbSet<VwCandidateSourceDtl> VwCandidateSourceDtls { get; set; }

        public virtual DbSet<VwDashboardCandidateInterview> VwDashboardCandidateInterviews { get; set; }

        public virtual DbSet<VwDashboardDaywiseFilterDatum> VwDashboardDaywiseFilterData { get; set; }

        public virtual DbSet<VwDashboardJobStage> VwDashboardJobStages { get; set; }

        public virtual DbSet<VwDashboardJobStageRecruiter> VwDashboardJobStageRecruiters { get; set; }

        public virtual DbSet<VwDashboardRecruiterStatus> VwDashboardRecruiterStatuses { get; set; }

        public virtual DbSet<VwJob> VwJobs { get; set; }

        public virtual DbSet<VwJobCandidate> VwJobCandidates { get; set; }

        public virtual DbSet<VwJobCandidateStatusHistory> VwJobCandidateStatusHistories { get; set; }

        public virtual DbSet<VwJobCandidatesByRole> VwJobCandidatesByRoles { get; set; }

        public virtual DbSet<VwJobStatusHistory> VwJobStatusHistories { get; set; }

        public virtual DbSet<VwUserPuBu> VwUserPuBus { get; set; }

        public virtual DbSet<VwUserTargetsByMonthAndTarget> VwUserTargetsByMonthAndTargets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cand>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("Cands");

                entity.Property(e => e.Epcurrency)
                    .HasMaxLength(10)
                    .HasColumnName("EPCurrency");
                entity.Property(e => e.EptakeHomePerMonth).HasColumnName("EPTakeHomePerMonth");
                entity.Property(e => e.JobCategory).HasMaxLength(100);
                entity.Property(e => e.OpCurrency).HasMaxLength(10);
                entity.Property(e => e.OpgrossPayPerMonth).HasColumnName("OPGrossPayPerMonth");
                entity.Property(e => e.RecruiterId).HasColumnName("RecruiterID");
            });

            modelBuilder.Entity<FileMigratationLog>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("fileMigratationLog");

                entity.Property(e => e.FileName).HasColumnName("fileName");
                entity.Property(e => e.IsSuccess).HasColumnName("isSuccess");
                entity.Property(e => e.Msg).HasColumnName("msg");
                entity.Property(e => e.TblName).HasColumnName("tblName");
            });

            modelBuilder.Entity<PhActivityLog>(entity =>
            {
                entity.ToTable("PH_ACTIVITY_LOG");

                entity.HasIndex(e => new { e.ActivityMode, e.ActivityType, e.UpdateStatusId }, "IX_ActivityLog_Filter");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ActivityMode).HasDefaultValueSql("((1))");
                entity.Property(e => e.ActivityType).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.CurrentStatusId).HasColumnName("CurrentStatusID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdateStatusId).HasColumnName("UpdateStatusID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhAuditLog>(entity =>
            {
                entity.ToTable("PH_AUDIT_LOG");

                entity.HasIndex(e => e.ActivityType, "IX_ActivityType");

                entity.HasIndex(e => e.CreatedBy, "IX_CreatedBy");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ActivitySubject)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.ActivityType).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.TaskId).HasColumnName("TaskID");
            });

            modelBuilder.Entity<PhBgJob>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_BG_JOBS_ID");

                entity.ToTable("PH_BG_JOBS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Bus)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasDefaultValueSql("((0))")
                    .HasColumnName("BUs");
                entity.Property(e => e.CandidateStatus)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasDefaultValueSql("((0))");
                entity.Property(e => e.CountryIds)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasDefaultValueSql("((0))")
                    .HasColumnName("CountryIDs");
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.EmailTemplateId).HasColumnName("EmailTemplateID");
                entity.Property(e => e.EventType)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Frequency).HasDefaultValueSql("((1))");
                entity.Property(e => e.Gender)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('A')")
                    .IsFixedLength();
                entity.Property(e => e.JobDesc).HasMaxLength(500);
                entity.Property(e => e.Pus)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasDefaultValueSql("((0))")
                    .HasColumnName("PUs");
                entity.Property(e => e.ScheduleDate).HasColumnType("date");
                entity.Property(e => e.ScheduleTime)
                    .IsRequired()
                    .HasMaxLength(10);
                entity.Property(e => e.SendTo).HasDefaultValueSql("((1))");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhBgJobDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_BG_JOB_DETAILS_ID");

                entity.ToTable("PH_BG_JOB_DETAILS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Bjid).HasColumnName("BJID");
                entity.Property(e => e.BulkReferenceId)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("BulkReferenceID");
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.ExecutedOn).HasColumnType("datetime");
                entity.Property(e => e.JobId).HasColumnName("JobID");
                entity.Property(e => e.Remarks).HasMaxLength(500);
                entity.Property(e => e.ServiceProviderId).HasColumnName("ServiceProviderID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Bj).WithMany(p => p.PhBgJobDetails)
                    .HasForeignKey(d => d.Bjid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PH_BG_JOBS_BJID");
            });

            modelBuilder.Entity<PhBlog>(entity =>
            {
                entity.ToTable("PH_BLOGS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AuthorName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.BlogDesc).IsRequired();
                entity.Property(e => e.BlogPic)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.BlogShortDesc).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Tags).HasMaxLength(1000);
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(1000);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCityWiseBenefit>(entity =>
            {
                entity.ToTable("PH_CITY_WISE_BENEFITS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CityId).HasColumnName("CityId");
                entity.Property(e => e.BenefitTitle)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.BenefitDesc).IsRequired();
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCountryWiseBenefit>(entity =>
            {
                entity.ToTable("PH_COUNTRY_WISE_BENEFITS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CountryId).HasColumnName("CountryId");
                entity.Property(e => e.IsSalaryWise).HasColumnName("IsSalaryWise");
                entity.Property(e => e.BenefitTitle)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.BenefitDesc).IsRequired();
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCountryWiseAllowance>(entity =>
            {
                entity.ToTable("PH_COUNTRY_WISE_ALLOWANCES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CountryId).HasColumnName("CountryId");
                entity.Property(e => e.IsCitizenWise).HasColumnName("IsCitizenWise");
                entity.Property(e => e.AllowanceCode)
                    .IsRequired()
                    .HasMaxLength(10);
                entity.Property(e => e.AllowanceTitle)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.AllowanceDesc);
                entity.Property(e => e.AllowancePrice).HasColumnType("decimal(38, 2)");
                entity.Property(e => e.AllowancePercentage).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandAssignment>(entity =>
            {
                entity.ToTable("PH_CAND_ASSIGNMENTS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DeassignDate).HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.NoCvsrequired).HasColumnName("NoCVSRequired");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandReofferDetail>(entity =>
            {
                entity.ToTable("PH_CAND_REOFFER_DETAILS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Opcurrency)
                    .HasMaxLength(12)
                    .HasColumnName("OPCurrency");
                entity.Property(e => e.OpgrossPayPerMonth).HasColumnName("OPGrossPayPerMonth");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<PhCandReofferDetailsTemp>(entity =>
            {
                entity.ToTable("PH_CAND_REOFFER_DETAILS_temp");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Opcurrency)
                    .HasMaxLength(12)
                    .HasColumnName("OPCurrency");
                entity.Property(e => e.OpgrossPayPerMonth).HasColumnName("OPGrossPayPerMonth");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<PhCandSocialPref>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__PH_CAND___3214EC2778E2E051");

                entity.ToTable("PH_CAND_SOCIAL_PREF");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.ProfileUrl)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnName("ProfileURL");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandStageMap>(entity =>
            {
                entity.ToTable("PH_CAND_STAGE_MAP");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandStatusId).HasColumnName("CandStatusID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.StageId).HasColumnName("StageID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandStagesS>(entity =>
            {
                entity.ToTable("PH_CAND_STAGES_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ColorCode)
                    .IsRequired()
                    .HasMaxLength(10);
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.StageDesc)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandStatusConfig>(entity =>
            {
                entity.ToTable("PH_CAND_STATUS_CONFIG_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DispOrder).HasDefaultValueSql("((1))");
                entity.Property(e => e.NextStatusId).HasColumnName("NextStatusID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.StatusId).HasColumnName("StatusID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandStatusS>(entity =>
            {
                entity.ToTable("PH_CAND_STATUS_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Cscode)
                    .HasMaxLength(5)
                    .HasColumnName("CSCode");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.StatusDesc)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandidateBgvDetail>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_BGV_DETAILS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AnotherName).HasMaxLength(50);
                entity.Property(e => e.BgacceptFlag).HasColumnName("BGAcceptFlag");
                entity.Property(e => e.BgcompStatus).HasColumnName("BGCompStatus");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
                entity.Property(e => e.EmerContactNo).HasMaxLength(50);
                entity.Property(e => e.EmerContactPerson).HasMaxLength(100);
                entity.Property(e => e.EmerContactRelation).HasMaxLength(50);
                entity.Property(e => e.EmiratesId)
                    .HasMaxLength(50)
                    .HasColumnName("EmiratesID");
                entity.Property(e => e.FatherName).HasMaxLength(50);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.HomePhone).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.MiddleName).HasMaxLength(50);
                entity.Property(e => e.MobileNo).HasMaxLength(50);
                entity.Property(e => e.MotherName).HasMaxLength(50);
                entity.Property(e => e.PermAddrCityId).HasColumnName("PermAddrCityID");
                entity.Property(e => e.PermAddrContactNo).HasMaxLength(50);
                entity.Property(e => e.PermAddrContactPerson).HasMaxLength(50);
                entity.Property(e => e.PermAddrCountryId).HasColumnName("PermAddrCountryID");
                entity.Property(e => e.PermAddrLandMark).HasMaxLength(100);
                entity.Property(e => e.PermAddrResiSince).HasColumnType("datetime");
                entity.Property(e => e.PermAddrResiTill).HasColumnType("datetime");
                entity.Property(e => e.PermAddress).HasMaxLength(500);
                entity.Property(e => e.PermContactRelation).HasMaxLength(50);
                entity.Property(e => e.PlaceOfBirth).HasMaxLength(50);
                entity.Property(e => e.PpexpiryDate)
                    .HasColumnType("datetime")
                    .HasColumnName("PPExpiryDate");
                entity.Property(e => e.Ppnumber)
                    .HasMaxLength(50)
                    .HasColumnName("PPNumber");
                entity.Property(e => e.PresAddrCityId).HasColumnName("PresAddrCityID");
                entity.Property(e => e.PresAddrContactNo).HasMaxLength(50);
                entity.Property(e => e.PresAddrContactPerson).HasMaxLength(50);
                entity.Property(e => e.PresAddrCountryId).HasColumnName("PresAddrCountryID");
                entity.Property(e => e.PresAddrLandMark).HasMaxLength(100);
                entity.Property(e => e.PresAddrPrefTimeForVerification).HasMaxLength(50);
                entity.Property(e => e.PresAddrResiSince).HasColumnType("datetime");
                entity.Property(e => e.PresAddress).HasMaxLength(500);
                entity.Property(e => e.PresContactRelation).HasMaxLength(50);
                entity.Property(e => e.SpouseName).HasMaxLength(50);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UgmedicalTreaDetails)
                    .HasMaxLength(500)
                    .HasColumnName("UGMedicalTreaDetails");
                entity.Property(e => e.UgmedicalTreaFlag).HasColumnName("UGMedicalTreaFlag");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandidateCertification>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_CERTIFICATIONS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CertificationId).HasColumnName("CertificationID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandidateDoc>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_DOCS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DocType)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.FileName).HasMaxLength(200);
                entity.Property(e => e.FileType).HasMaxLength(100);
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Remerks).HasMaxLength(200);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.UploadedBy).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<PhCandidateEduDetail>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_EDU_DETAILS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.Course).HasMaxLength(100);
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DurationFrom).HasColumnType("date");
                entity.Property(e => e.DurationTo).HasColumnType("date");
                entity.Property(e => e.Grade).HasMaxLength(50);
                entity.Property(e => e.Qualification).HasMaxLength(50);
                entity.Property(e => e.QualificationId).HasColumnName("QualificationID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UnivOrInstitution).HasMaxLength(100);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandidateEmpmtDetail>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_EMPMT_DETAILS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CityId).HasColumnName("CityID");
                entity.Property(e => e.CountryId).HasColumnName("CountryID");
                entity.Property(e => e.Cpdesignation)
                    .HasMaxLength(50)
                    .HasColumnName("CPDesignation");
                entity.Property(e => e.CpemailId)
                    .HasMaxLength(100)
                    .HasColumnName("CPEmailID");
                entity.Property(e => e.Cpname)
                    .HasMaxLength(50)
                    .HasColumnName("CPName");
                entity.Property(e => e.Cpnumber)
                    .HasMaxLength(50)
                    .HasColumnName("CPNumber");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Designation).HasMaxLength(100);
                entity.Property(e => e.DesignationId).HasColumnName("DesignationID");
                entity.Property(e => e.EmployId)
                    .HasMaxLength(50)
                    .HasColumnName("EmployID");
                entity.Property(e => e.EmployerName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.EmptFromDate).HasColumnType("datetime");
                entity.Property(e => e.EmptToDate).HasColumnType("datetime");
                entity.Property(e => e.HrcontactNo)
                    .HasMaxLength(50)
                    .HasColumnName("HRContactNo");
                entity.Property(e => e.HremailId)
                    .HasMaxLength(100)
                    .HasColumnName("HREmailID");
                entity.Property(e => e.OfficialEmailId)
                    .HasMaxLength(100)
                    .HasColumnName("OfficialEmailID");
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandidateProfile>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_PROFILES");

                entity.HasIndex(e => e.EmailId, "NonClusteredIndex-EmailId");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AlteContactNo).HasMaxLength(20);
                entity.Property(e => e.CandName).HasMaxLength(150);
                entity.Property(e => e.CandOverallStatus).HasDefaultValueSql("((1))");
                entity.Property(e => e.ContactNo).HasMaxLength(20);
                entity.Property(e => e.CountryId).HasColumnName("CountryID");
                entity.Property(e => e.Cpcurrency)
                    .HasMaxLength(5)
                    .HasColumnName("CPCurrency");
                entity.Property(e => e.CpdeductionsPerAnnum).HasColumnName("CPDeductionsPerAnnum");
                entity.Property(e => e.CpgrossPayPerAnnum).HasColumnName("CPGrossPayPerAnnum");
                entity.Property(e => e.CptakeHomeSalPerMonth).HasColumnName("CPTakeHomeSalPerMonth");
                entity.Property(e => e.CpvariablePayPerAnnum).HasColumnName("CPVariablePayPerAnnum");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.CurrEmplFlag).HasDefaultValueSql("((0))");
                entity.Property(e => e.CurrLocation).HasMaxLength(100);
                entity.Property(e => e.CurrLocationId).HasColumnName("CurrLocationID");
                entity.Property(e => e.CurrOrganization).HasMaxLength(100);
                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");
                entity.Property(e => e.EmailId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("EmailID");
                entity.Property(e => e.Epcurrency)
                    .HasMaxLength(5)
                    .HasColumnName("EPCurrency");
                entity.Property(e => e.EptakeHomeSalPerMonth).HasColumnName("EPTakeHomeSalPerMonth");
                entity.Property(e => e.Experience).HasMaxLength(100);
                entity.Property(e => e.FullNameInPp)
                    .HasMaxLength(150)
                    .HasColumnName("FullNameInPP");
                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.MaritalStatus)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Nationality)
                    .HasMaxLength(150)
                    .IsUnicode(false);
                entity.Property(e => e.RelevantExperience).HasMaxLength(20);
                entity.Property(e => e.Remarks).HasMaxLength(500);
                entity.Property(e => e.Roles).HasMaxLength(100);
                entity.Property(e => e.SourceId).HasColumnName("SourceID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.ValidPpflag)
                    .HasDefaultValueSql("((0))")
                    .HasColumnName("ValidPPFlag");
            });

            modelBuilder.Entity<PhCandidateProfilesShared>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_PROFILES_SHARED");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.BatchNo).HasMaxLength(100);
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CcemailIds)
                    .HasMaxLength(1000)
                    .HasColumnName("CCEmailIDs");
                entity.Property(e => e.ClemailId)
                    .HasMaxLength(100)
                    .HasColumnName("CLEmailID");
                entity.Property(e => e.ClientId).HasColumnName("ClientID");
                entity.Property(e => e.Clname)
                    .HasMaxLength(100)
                    .HasColumnName("CLName");
                entity.Property(e => e.ClreviewStatus).HasColumnName("CLReviewStatus");
                entity.Property(e => e.ConfView).HasMaxLength(200);
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.EmailFields).HasMaxLength(200);
                entity.Property(e => e.EmailSubject).HasMaxLength(1000);
                entity.Property(e => e.EndTime).HasMaxLength(15);
                entity.Property(e => e.InterviewDate).HasColumnType("date");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.ModeOfInterview).HasDefaultValueSql("((1))");
                entity.Property(e => e.Reasons).HasMaxLength(500);
                entity.Property(e => e.Remarks)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.ReviewedOn).HasColumnType("datetime");
                entity.Property(e => e.StartTime).HasMaxLength(15);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandidateRecruitersHistory>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_RECRUITERS_HISTORY");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Remarks)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandidateSkillset>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_SKILLSET");

                entity.HasIndex(e => e.CandProfId, "NonClusteredIndex-CandProfID");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.ExpInYears).HasMaxLength(5);
                entity.Property(e => e.SkillLevel).HasMaxLength(300);
                entity.Property(e => e.SkillLevelId).HasColumnName("SkillLevelID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.TechnologyId).HasColumnName("TechnologyID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandidateStatusLog>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_STATUS_LOG");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ActivityDesc).IsRequired();
                entity.Property(e => e.CandProfStatus)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhCandidateTag>(entity =>
            {
                entity.ToTable("PH_CANDIDATE_TAGS");

                entity.HasIndex(e => new { e.CandProfId, e.Status }, "NonClusteredIndex-CandProfID");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.TaggingWord)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhChatMessage>(entity =>
            {
                entity.ToTable("PH_CHAT_MESSAGES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ChatRoomId).HasColumnName("ChatRoomID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.Message).HasMaxLength(1000);
                entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
                entity.Property(e => e.SenderId).HasColumnName("SenderID");
            });

            modelBuilder.Entity<PhChatRoom>(entity =>
            {
                entity.ToTable("PH_CHAT_ROOMS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Crdesc)
                    .HasMaxLength(1000)
                    .HasColumnName("CRDesc");
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<PhCity>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_tbl_city");

                entity.ToTable("PH_CITY");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.Country)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("country");
                entity.Property(e => e.CountryId).HasColumnName("country_id");
                entity.Property(e => e.Iso2)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("iso2");
                entity.Property(e => e.Iso3)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .HasColumnName("iso3");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<PhCountry>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__tbl_coun__3213E83F0C1314C4");

                entity.ToTable("PH_COUNTRY");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CurrSymbol)
                    .HasMaxLength(50)
                    .HasColumnName("curr_symbol");
                entity.Property(e => e.Currency)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("currency");
                entity.Property(e => e.Iso)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("iso");
                entity.Property(e => e.Iso3)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .HasColumnName("iso3");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .IsUnicode(false)
                    .HasColumnName("name");
                entity.Property(e => e.Nicename)
                    .IsRequired()
                    .HasMaxLength(80)
                    .IsUnicode(false)
                    .HasColumnName("nicename");
                entity.Property(e => e.Numcode).HasColumnName("numcode");
                entity.Property(e => e.Phonecode).HasColumnName("phonecode");
            });

            modelBuilder.Entity<PhCurrencyExchangeRate>(entity =>
            {
                entity.ToTable("PH_CURRENCY_EXCHANGE_RATES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.ExchangeRate).HasColumnType("decimal(32,15)");
                entity.Property(e => e.FromCurrency)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(e => e.ToCurrency)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<PhDayWiseJobAction>(entity =>
            {
                entity.ToTable("PH_DAY_WISE_JOB_ACTIONS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Assign).HasDefaultValueSql("((0))");
                entity.Property(e => e.CandStatus).HasDefaultValueSql("((0))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Interviews).HasDefaultValueSql("((0))");
                entity.Property(e => e.JobStatus).HasDefaultValueSql("((0))");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Note).HasDefaultValueSql("((0))");
                entity.Property(e => e.Priority).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<PhEmpLeaveRequest>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__PH_EMP_L__3214EC2738F8269C");

                entity.ToTable("PH_EMP_LEAVE_REQUEST");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ApproveRemarks)
                    .HasMaxLength(500)
                    .IsUnicode(false);
                entity.Property(e => e.ApprovedDate).HasColumnType("datetime");
                entity.Property(e => e.CancelDate).HasColumnType("datetime");
                entity.Property(e => e.CancelRemarks)
                    .HasMaxLength(500)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.EmpId).HasColumnName("EmpID");
                entity.Property(e => e.LeaveEndDate).HasColumnType("datetime");
                entity.Property(e => e.LeaveReason).IsUnicode(false);
                entity.Property(e => e.LeaveStartDate).HasColumnType("datetime");
                entity.Property(e => e.RejectRemarks)
                    .HasMaxLength(500)
                    .IsUnicode(false);
                entity.Property(e => e.RejectedDate).HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<PhIntegrationsS>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_INTEGRATIONS_S_ID");

                entity.ToTable("PH_INTEGRATIONS_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Account)
                    .HasMaxLength(150)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.InteDesc).HasMaxLength(1000);
                entity.Property(e => e.Logo).HasMaxLength(100);
                entity.Property(e => e.ReDirectUrl).HasColumnName("ReDirectURL");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobAssignment>(entity =>
            {
                entity.ToTable("PH_JOB_ASSIGNMENTS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.CvTargetDate).HasColumnType("datetime");
                entity.Property(e => e.DeassignDate).HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.NoCvsrequired).HasColumnName("NoCVSRequired");
                entity.Property(e => e.NoOfFinalCvsFilled)
                    .HasDefaultValueSql("((0))")
                    .HasColumnName("NoOfFinalCVsFilled");
                entity.Property(e => e.ReassignDate).HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobAssignmentHistory>(entity =>
            {
                entity.ToTable("PH_JOB_ASSIGNMENT_HISTORIES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.DeassignDate).HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            });            

            modelBuilder.Entity<PhJobAssignmentsDayWise>(entity =>
            {
                entity.ToTable("PH_JOB_ASSIGNMENTS_DAY_WISE");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AssignmentDate).HasColumnType("datetime");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.NoCvsrequired)
                    .HasDefaultValueSql("((0))")
                    .HasColumnName("NoCVSRequired");
                entity.Property(e => e.NoCvsuploadded)
                    .HasDefaultValueSql("((0))")
                    .HasColumnName("NoCVSUploadded");
                entity.Property(e => e.NoOfFinalCvsFilled)
                    .HasDefaultValueSql("((0))")
                    .HasColumnName("NoOfFinalCVsFilled");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobAssignmentsDayWiseLog>(entity =>
            {
                entity.ToTable("PH_JOB_ASSIGNMENTS_DAY_WISE_LOG");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.NoCvsrequired)
                    .HasDefaultValueSql("((0))")
                    .HasColumnName("NoCVSRequired");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<PhJobCandidate>(entity =>
            {
                entity.ToTable("PH_JOB_CANDIDATES");

                entity.HasIndex(e => e.RecruiterId, "NonClusteredIndex-RecruiterID");

                entity.HasIndex(e => new { e.CandProfId, e.Joid, e.Id, e.Epcurrency, e.OpgrossPayPerMonth, e.CandProfStatus }, "idx_job_candidates_index").IsDescending(false, false, true, false, false, false);

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.BgvacceptedFlag).HasColumnName("BGVAcceptedFlag");
                entity.Property(e => e.Bgvcomments)
                    .HasMaxLength(200)
                    .HasColumnName("BGVComments");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.Cbcurrency)
                    .HasMaxLength(10)
                    .HasColumnName("CBCurrency");
                entity.Property(e => e.CbperMonth).HasColumnName("CBPerMonth");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Epcurrency)
                    .HasMaxLength(10)
                    .HasColumnName("EPCurrency");
                entity.Property(e => e.EpdeductionsPerAnnum).HasColumnName("EPDeductionsPerAnnum");
                entity.Property(e => e.EpgrossPayPerAnnum).HasColumnName("EPGrossPayPerAnnum");
                entity.Property(e => e.EptakeHomePerMonth).HasColumnName("EPTakeHomePerMonth");
                entity.Property(e => e.IsPayslipVerified).HasDefaultValueSql("((0))");
                entity.Property(e => e.IsTagged).HasDefaultValueSql("((0))");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.L1review).HasColumnName("L1Review");
                entity.Property(e => e.Mreview).HasColumnName("MReview");
                entity.Property(e => e.OpconfirmDate)
                    .HasColumnType("datetime")
                    .HasColumnName("OPConfirmDate");
                entity.Property(e => e.OpconfirmFlag).HasColumnName("OPConfirmFlag");
                entity.Property(e => e.Opcurrency)
                    .HasMaxLength(10)
                    .HasColumnName("OPCurrency");
                entity.Property(e => e.OpdeductionsPerAnnum).HasColumnName("OPDeductionsPerAnnum");
                entity.Property(e => e.OpgrossPayPerAnnum).HasColumnName("OPGrossPayPerAnnum");
                entity.Property(e => e.OpgrossPayPerMonth).HasColumnName("OPGrossPayPerMonth");
                entity.Property(e => e.OpnetPayPerAnnum).HasColumnName("OPNetPayPerAnnum");
                entity.Property(e => e.OptakeHomePerMonth).HasColumnName("OPTakeHomePerMonth");
                entity.Property(e => e.OpvarPayPerAnnum).HasColumnName("OPVarPayPerAnnum");
                entity.Property(e => e.PayslipCurrency).HasMaxLength(100);
                entity.Property(e => e.ProfReceDate).HasColumnType("datetime");
                entity.Property(e => e.RecruiterId).HasColumnName("RecruiterID");
                entity.Property(e => e.StageId).HasColumnName("StageID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Tlreview).HasColumnName("TLReview");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.InterviewFaceToFaceReason).HasMaxLength(1000);
            });
            modelBuilder.Entity<PhJobCandidateSkillset>(entity =>
            {
                entity.ToTable("PH_JOB_CANDIDATE_SKILLSET");
                
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });
            modelBuilder.Entity<PhJobCandidateOpeningsQualification>(entity =>
            {
                entity.ToTable("PH_JOB_CANDIDATE_QUALIFICATIONS");
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_CANDIDATE_QUALIFICATIONS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });
            modelBuilder.Entity<PhJobCandidateOpeningsCertification>(entity =>
            {
                entity.ToTable("PH_JOB_CANDIDATE_CERTIFICATIONS");
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_CANDIDATE_CERTIFICATIONS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });


            modelBuilder.Entity<PhJobCandidateAssemt>(entity =>
            {
                entity.ToTable("PH_JOB_CANDIDATE_ASSEMTS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AssmtId)
                    .HasMaxLength(1000)
                    .HasColumnName("AssmtID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.ContactId).HasMaxLength(12);
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DistributionId).HasMaxLength(1024);
                entity.Property(e => e.JoassmtId).HasColumnName("JOAssmtID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.ResponseDate).HasColumnType("datetime");
                entity.Property(e => e.ResponseUrl).HasColumnName("ResponseURL");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobCandidateEvaluation>(entity =>
            {
                entity.ToTable("PH_JOB_CANDIDATE_EVALUATION");

                entity.HasIndex(e => e.CandProfId, "NonClusteredIndex-CandProfID");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.RefId).HasColumnName("RefID");
                entity.Property(e => e.Remakrs).HasMaxLength(200);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobCandidateInterview>(entity =>
            {
                entity.ToTable("PH_JOB_CANDIDATE_INTERVIEWS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.InterviewDate).HasColumnType("date");
                entity.Property(e => e.InterviewDuration).HasDefaultValueSql("((30))");
                entity.Property(e => e.InterviewEndTime).HasMaxLength(15);
                entity.Property(e => e.InterviewStartTime).HasMaxLength(15);
                entity.Property(e => e.InterviewerEmail).HasMaxLength(1000);
                entity.Property(e => e.InterviewerName).HasMaxLength(100);
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.ModeOfInterview).HasDefaultValueSql("((1))");
                entity.Property(e => e.PiTeamEmailIds)
                    .HasMaxLength(1000)
                    .HasColumnName("PiTeamEmailIDs");
                entity.Property(e => e.Remarks).HasMaxLength(1000);
                entity.Property(e => e.ScheduledBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobCandidateStResponse>(entity =>
            {
                entity.ToTable("PH_JOB_CANDIDATE_ST_RESPONSES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Response).HasMaxLength(1000);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.StquestionId).HasColumnName("STQuestionID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobOfferAllowance>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__PH_JOB_O__3214EC27F0A674C8");

                entity.ToTable("PH_JOB_OFFER_ALLOWANCES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AllowanceTitle)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.JobOfferId).HasColumnName("JobOfferID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
            });

            modelBuilder.Entity<PhJobOfferLetter>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__PH_JOB_O__3214EC275F971A07");

                entity.ToTable("PH_JOB_OFFER_LETTERS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.CurrencyId).HasColumnName("CurrencyID");
                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
                entity.Property(e => e.DesignationId).HasColumnName("DesignationID");
                entity.Property(e => e.FileName).HasMaxLength(500);
                entity.Property(e => e.FileType).HasMaxLength(100);
                entity.Property(e => e.FileUrl)
                    .HasMaxLength(500)
                    .HasColumnName("FileURL");
                entity.Property(e => e.Hra).HasColumnName("HRA");
                entity.Property(e => e.Ita).HasColumnName("ITA");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.JoiningDate).HasColumnType("datetime");
                entity.Property(e => e.Otbonus).HasColumnName("OTBonus");
                entity.Property(e => e.ProcessUnitId).HasColumnName("ProcessUnitID");
                entity.Property(e => e.SpecId).HasColumnName("SpecID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobOfferSlabDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__PH_JOB_O__3214EC2703DB9FD4");

                entity.ToTable("PH_JOB_OFFER_SLAB_DETAILS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.ComponentId).HasColumnName("ComponentID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.JobOfferId).HasColumnName("JobOfferID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.SlabId).HasColumnName("SlabID");
            });

            modelBuilder.Entity<PhJobOpening>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_OPENINGS_ID");

                entity.ToTable("PH_JOB_OPENINGS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ClientId).HasColumnName("ClientID");
                entity.Property(e => e.ClientName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.ClosedDate).HasColumnType("datetime");
                entity.Property(e => e.CountryId).HasColumnName("CountryID");
                entity.Property(e => e.CreatedByName).HasMaxLength(100);
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.JobCategory).HasMaxLength(100);
                entity.Property(e => e.JobLocationId).HasColumnName("JobLocationID");
                entity.Property(e => e.JobRole).HasMaxLength(100);
                entity.Property(e => e.JobTitle)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.PostedDate).HasColumnType("datetime");
                entity.Property(e => e.Remarks).HasMaxLength(500);
                entity.Property(e => e.ReopenedDate).HasColumnType("datetime");
                entity.Property(e => e.ShortJobDesc).HasMaxLength(1000);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });            

            modelBuilder.Entity<PhJobOpeningActvCounter>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_OPENING_ACTV_COUNTER_ID");

                entity.ToTable("PH_JOB_OPENING_ACTV_COUNTER");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobOpeningAssmt>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_OPENING_ASSMTS_ID");

                entity.ToTable("PH_JOB_OPENING_ASSMTS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AssessmentId)
                    .HasMaxLength(1000)
                    .HasColumnName("AssessmentID");
                entity.Property(e => e.CandStatusId).HasColumnName("CandStatusID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.StageId).HasColumnName("StageID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobOpeningPref>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_OPENING_PREF_ID");

                entity.ToTable("PH_JOB_OPENING_PREF");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.FieldCode)
                    .IsRequired()
                    .HasMaxLength(5);
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobOpeningSkill>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_OPENING_SKILLS_ID");

                entity.ToTable("PH_JOB_OPENING_SKILLS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.SkillLevelId).HasColumnName("SkillLevelID");
                entity.Property(e => e.SkillName).HasMaxLength(100);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Technology).HasMaxLength(200);
                entity.Property(e => e.TechnologyId).HasColumnName("TechnologyID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobOpeningStQtn>(entity =>
            {
                entity.ToTable("PH_JOB_OPENING_ST_QTNS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.IsMandatory)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.QuestionText)
                    .IsRequired()
                    .HasMaxLength(1000);
                entity.Property(e => e.QuestionType).HasDefaultValueSql("((1))");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobOpeningStatusCounter>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_OPENING_STATUS_COUNTER_ID");

                entity.ToTable("PH_JOB_OPENING_STATUS_COUNTER");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandStatusId).HasColumnName("CandStatusID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.StageId).HasColumnName("StageID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobOpeningsAddlDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_OPENINGS_ADDL_DETAILS_ID");

                entity.ToTable("PH_JOB_OPENINGS_ADDL_DETAILS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AddlComments).HasMaxLength(1000);
                entity.Property(e => e.AnnualSalary).HasColumnType("numeric(9, 0)");
                entity.Property(e => e.ApprJoinDate).HasColumnType("datetime");
                entity.Property(e => e.Buid).HasColumnName("BUID");
                entity.Property(e => e.CrmoppoId).HasColumnName("CRMOppoID");
                entity.Property(e => e.CurrencyId).HasColumnName("CurrencyID");
                entity.Property(e => e.JobTenure).HasMaxLength(10);
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.MaxSalary).HasColumnType("numeric(7, 0)");
                entity.Property(e => e.MinSalary).HasColumnType("numeric(7, 0)");
                entity.Property(e => e.Puid).HasColumnName("PUID");
                entity.Property(e => e.ReceivedDate).HasColumnType("datetime");
                entity.Property(e => e.SalaryPackage).HasMaxLength(50);
                entity.Property(e => e.SalaryRemarks).HasMaxLength(500);
                entity.Property(e => e.Spocid).HasColumnName("SPOCID");
            });

            modelBuilder.Entity<PhJobOpeningsDesirables>(entity =>
            {
                entity.HasKey(e => e.Joid).HasName("PK_PH_JOB_OPENINGS_DESIRABLES");

                entity.ToTable("PH_JOB_OPENINGS_DESIRABLES");

                entity.Property(e => e.Joid).HasColumnName("JOID");
            });

            modelBuilder.Entity<PhJobOpeningsDesirableSkill>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_OPENINGS_DESIRABLE_SKILLS");

                entity.ToTable("PH_JOB_OPENINGS_DESIRABLE_SKILLS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobRecruiterPriority>(entity =>
            {
                entity.ToTable("PH_JOB_RECRUITER_PRIORITIES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobOpeningsCertification>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_OPENINGS_CERTIFICATIONS");

                entity.ToTable("PH_JOB_OPENINGS_CERTIFICATIONS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobOpeningsQualification>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PH_JOB_OPENINGS_QUALIFICATIONS");

                entity.ToTable("PH_JOB_OPENINGS_QUALIFICATIONS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhJobStatusS>(entity =>
            {
                entity.ToTable("PH_JOB_STATUS_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Jscode)
                    .HasMaxLength(5)
                    .HasColumnName("JSCode");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.StatusDesc)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhMediaFile>(entity =>
            {
                entity.ToTable("PH_MEDIA_FILES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.FileType)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhMessageTemplate>(entity =>
            {
                entity.ToTable("PH_MESSAGE_TEMPLATES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DynamicLabels).HasMaxLength(1000);
                entity.Property(e => e.IndustryId).HasColumnName("IndustryID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.TplBody).IsRequired();
                entity.Property(e => e.TplDesc).HasMaxLength(100);
                entity.Property(e => e.TplFullBody)
                    .IsRequired()
                    .HasDefaultValueSql("('')");
                entity.Property(e => e.TplSubject).HasMaxLength(100);
                entity.Property(e => e.TplTitle)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhNote>(entity =>
            {
                entity.ToTable("PH_NOTES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.NoteId).HasColumnName("NoteID");
                entity.Property(e => e.NotesDesc)
                    .IsRequired()
                    .HasMaxLength(1000);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhNotesSendList>(entity =>
            {
                entity.ToTable("PH_NOTES_SEND_LIST");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.NotesId).HasColumnName("NotesID");
            });

            modelBuilder.Entity<PhNotification>(entity =>
            {
                entity.ToTable("PH_NOTIFICATIONS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhNotificationsUser>(entity =>
            {
                entity.ToTable("PH_NOTIFICATIONS_USERS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.NotIid).HasColumnName("NotIID");
                entity.Property(e => e.PushTo).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<PhRefMaster>(entity =>
            {
                entity.ToTable("PH_REF_MASTER_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.GroupId).HasColumnName("GroupID");
                entity.Property(e => e.Rmdesc)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("RMDesc");
                entity.Property(e => e.Rmtype)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("RMType");
                entity.Property(e => e.Rmvalue)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("RMValue");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhEducationQualificationMaster>(entity =>
            {
                entity.ToTable("PH_EDUCATION_QUALIFICATION_MASTER_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.Property(e => e.GroupId).HasColumnName("GroupId");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("Title");
                entity.Property(e => e.Desc)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("Desc");

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<PhSalaryComp>(entity =>
            {
                entity.ToTable("PH_SALARY_COMP_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CompDesc).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhSalarySlabsS>(entity =>
            {
                entity.ToTable("PH_SALARY_SLABS_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Puid).HasColumnName("PUID");
                entity.Property(e => e.SlabDesc).HasMaxLength(500);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhSalarySlabsWiseCompsS>(entity =>
            {
                entity.ToTable("PH_SALARY_SLABS_WISE_COMPS_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.CompId).HasColumnName("CompID");
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.PercentageFlag).HasDefaultValueSql("((0))");
                entity.Property(e => e.Puid).HasColumnName("PUID");
                entity.Property(e => e.SlabId).HasColumnName("SlabID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhShift>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__PH_SHIFT__3214EC27C2C6BB27");

                entity.ToTable("PH_SHIFT");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.ShiftName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhShiftDetl>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__PH_SHIFT__3214EC271DB4E359");

                entity.ToTable("PH_SHIFT_DETL");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AlternativeWeekStartDate).HasColumnType("datetime");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.DayName)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(e => e.FromMeridiem).HasMaxLength(2);
                entity.Property(e => e.ToMeridiem).HasMaxLength(2);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhTechnologyGroupsS>(entity =>
            {
                entity.ToTable("PH_TECHNOLOGY_GROUPS_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.TechnologyGroupId).HasColumnName("TechnologyGroupID");
                entity.Property(e => e.TechnologyId).HasColumnName("TechnologyID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhTechnologysS>(entity =>
            {
                entity.ToTable("PH_TECHNOLOGYS_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhTestimonial>(entity =>
            {
                entity.ToTable("PH_TESTIMONIALS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CandidateId).HasColumnName("CandidateID");
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Designation).HasMaxLength(100);
                entity.Property(e => e.ProfilePic)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Rating).HasDefaultValueSql("((5))");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Tdesc)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnName("TDesc");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhUsersConfig>(entity =>
            {
                entity.ToTable("PH_USERS_CONFIG");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.PasswordHash).HasMaxLength(500);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.UserName).HasMaxLength(100);
                entity.Property(e => e.VerifyToken).HasMaxLength(1000);
            });

            modelBuilder.Entity<PhUsersRemark>(entity =>
            {
                entity.ToTable("PH_USERS_REMARKS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.NotesDesc)
                    .IsRequired()
                    .HasMaxLength(1000);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhWorkflow>(entity =>
            {
                entity.ToTable("PH_WORKFLOWS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.TaskCode)
                    .IsRequired()
                    .HasMaxLength(25);
                entity.Property(e => e.TaskId).HasColumnName("TaskID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PhWorkflowsDet>(entity =>
            {
                entity.ToTable("PH_WORKFLOWS_DET");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ActionType).HasDefaultValueSql("((1))");
                entity.Property(e => e.AsmtOrTplId).HasColumnName("AsmtOrTplID");
                entity.Property(e => e.CurrentStatusId).HasColumnName("CurrentStatusID");
                entity.Property(e => e.DocsReqstdIds)
                    .HasMaxLength(100)
                    .HasColumnName("DocsReqstdIDs");
                entity.Property(e => e.SendMode).HasDefaultValueSql("((1))");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdateStatusId).HasColumnName("UpdateStatusID");
                entity.Property(e => e.WorkflowId).HasColumnName("WorkflowID");
            });

            modelBuilder.Entity<PiAppModulesS>(entity =>
            {
                entity.ToTable("PI_APP_MODULES_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
                entity.Property(e => e.ModuleCode)
                    .IsRequired()
                    .HasMaxLength(5);
                entity.Property(e => e.ModuleDesc).HasMaxLength(200);
                entity.Property(e => e.ModuleName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<PiAppTasksS>(entity =>
            {
                entity.ToTable("PI_APP_TASKS_S");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Activities)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsFixedLength();
                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.TaskCode).HasMaxLength(25);
                entity.Property(e => e.TaskDesc).HasMaxLength(200);
                entity.Property(e => e.TaskName).HasMaxLength(50);
            });

            modelBuilder.Entity<PiAppUserPuBu>(entity =>
            {
                entity.ToTable("PI_APP_USER_PU_BU");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AppUserId).HasColumnName("AppUserID");
                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
                entity.Property(e => e.CreatedBy).HasDefaultValueSql("((1))");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PiAppUserResp>(entity =>
            {
                entity.ToTable("PI_APP_USER_RESP");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AppUserId).HasColumnName("AppUserID");
                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
                entity.Property(e => e.Permissions)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.TaskId).HasColumnName("TaskID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PiAppUserRole>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PI_APP_ROLES");

                entity.ToTable("PI_APP_USER_ROLES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.RoleDesc).HasMaxLength(200);
                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<PiAppUserRoleMap>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PI_APPUSER_ROLE_MAP");

                entity.ToTable("PI_APP_USER_ROLE_MAP");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AppRoleId).HasColumnName("AppRoleID");
                entity.Property(e => e.AppUserId).HasColumnName("AppUserID");
                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PiAppUserRoleResp>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PI_APPUSER_ROLE_RESP");

                entity.ToTable("PI_APP_USER_ROLE_RESP");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
                entity.Property(e => e.Permissions)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.RoleId).HasColumnName("RoleID");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.TaskId).HasColumnName("TaskID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PiEmailServiceProvider>(entity =>
            {
                entity.ToTable("PI_EMAIL_SERVICE_PROVIDERS");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AuthKey)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getutcdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DistType).HasDefaultValueSql("((1))");
                entity.Property(e => e.FromEmailId)
                    .HasMaxLength(200)
                    .HasColumnName("FromEmailID");
                entity.Property(e => e.FromName).HasMaxLength(100);
                entity.Property(e => e.ProviderCode)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.RequestUrl)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnName("RequestURL");
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<PiHireUser>(entity =>
            {
                entity.ToTable("PI_HIRE_USERS");

                entity.HasIndex(e => e.UserType, "NonClusteredIndex-UserType_Name");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Dob)
                    .HasColumnType("date")
                    .HasColumnName("DOB");
                entity.Property(e => e.EmailId)
                    .HasMaxLength(100)
                    .HasColumnName("EmailID");
                entity.Property(e => e.EmployId).HasColumnName("EmployID");
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.LocationId).HasColumnName("LocationID");
                entity.Property(e => e.MobileNumber).HasMaxLength(50);
                entity.Property(e => e.Nationality).HasMaxLength(100);
                entity.Property(e => e.PasswordHash).HasMaxLength(500);
                entity.Property(e => e.ProfilePhoto)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.Status).HasDefaultValueSql("((1))");
                entity.Property(e => e.TokenExpiryDate).HasColumnType("datetime");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.Property(e => e.UserName).HasMaxLength(100);
                entity.Property(e => e.UserRoleName).HasMaxLength(50);
                entity.Property(e => e.VerifyToken).HasMaxLength(1000);
            });

            modelBuilder.Entity<PiUserLog>(entity =>
            {
                entity.ToTable("PI_USER_LOG");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
                entity.Property(e => e.LastTxnId).HasColumnName("LastTxnID");
                entity.Property(e => e.LoginStatus)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");
                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<PiUserTxnLog>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_USER_TXN_LOG");

                entity.ToTable("PI_USER_TXN_LOG");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
                entity.Property(e => e.DeviceType).HasDefaultValueSql("((1))");
                entity.Property(e => e.DeviceUid)
                    .HasMaxLength(100)
                    .HasColumnName("DeviceUID");
                entity.Property(e => e.Ipaddress)
                    .HasMaxLength(50)
                    .HasColumnName("IPAddress");
                entity.Property(e => e.Lat).HasMaxLength(100);
                entity.Property(e => e.Long).HasMaxLength(100);
                entity.Property(e => e.SessionId)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("SessionID");
                entity.Property(e => e.TxnDesc)
                    .IsRequired()
                    .HasMaxLength(1000);
                entity.Property(e => e.TxnOutDate).HasColumnType("datetime");
                entity.Property(e => e.TxnStartDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<TblParamProcessUnitMaster>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_tbl_param_process_units");

                entity.ToTable("tbl_param_process_unit_master");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");
                entity.Property(e => e.City).HasColumnName("city");
                entity.Property(e => e.ColumnFive)
                    .HasColumnType("datetime")
                    .HasColumnName("column_five");
                entity.Property(e => e.ColumnFour).HasColumnName("column_four");
                entity.Property(e => e.ColumnThree).HasColumnName("column_three");
                entity.Property(e => e.ColumnTwo)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("column_two");
                entity.Property(e => e.Country).HasColumnName("country");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("created_date");
                entity.Property(e => e.DateOfEstablisment)
                    .HasColumnType("datetime")
                    .HasColumnName("date_of_establisment");
                entity.Property(e => e.GstNo)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("gst_no");
                entity.Property(e => e.IsoCode)
                    .HasMaxLength(200)
                    .HasColumnName("iso_code");
                entity.Property(e => e.Latitude).HasMaxLength(50);
                entity.Property(e => e.Logo)
                    .HasMaxLength(500)
                    .HasColumnName("logo");
                entity.Property(e => e.Longitude).HasMaxLength(50);
                entity.Property(e => e.MobileNumber)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("mobile_number");
                entity.Property(e => e.PanNo)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("pan_no");
                entity.Property(e => e.PayslipEmail)
                    .HasMaxLength(25)
                    .IsUnicode(false);
                entity.Property(e => e.PuName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("pu_name");
                entity.Property(e => e.ServiceTaxNo)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("service_tax_no");
                entity.Property(e => e.ShortName)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("short_name");
                entity.Property(e => e.State)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("state");
                entity.Property(e => e.TanNo)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("tan_no");
                entity.Property(e => e.TimeZone)
                    .HasMaxLength(200)
                    .HasColumnName("time_zone");
                entity.Property(e => e.TinNo)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("tin_no");
                entity.Property(e => e.VatNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("VAT_Number");
                entity.Property(e => e.Website)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("website");
            });

            modelBuilder.Entity<TblParamPuBusinessUnit>(entity =>
            {
                entity.ToTable("tbl_param_pu_business_units");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");
                entity.Property(e => e.BusUnitCode)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .HasColumnName("bus_unit_code");
                entity.Property(e => e.BusUnitFullName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("bus_unit_full_name");
                entity.Property(e => e.ColumnFive)
                    .HasColumnType("datetime")
                    .HasColumnName("column_five");
                entity.Property(e => e.ColumnFour).HasColumnName("column_four");
                entity.Property(e => e.ColumnOne)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("column_one");
                entity.Property(e => e.ColumnThree).HasColumnName("column_three");
                entity.Property(e => e.ColumnTwo)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("column_two");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("created_date");
                entity.Property(e => e.Description)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("description");
                entity.Property(e => e.PuId).HasColumnName("pu_id");
            });

            modelBuilder.Entity<TblParamPuOfficeLocation>(entity =>
            {
                entity.ToTable("tbl_param_pu_office_locations");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");
                entity.Property(e => e.Address1)
                    .HasMaxLength(100)
                    .HasColumnName("address1");
                entity.Property(e => e.Address2)
                    .HasMaxLength(100)
                    .HasColumnName("address2");
                entity.Property(e => e.Address3)
                    .HasMaxLength(100)
                    .HasColumnName("address3");
                entity.Property(e => e.City).HasColumnName("city");
                entity.Property(e => e.ColumnFive)
                    .HasColumnType("datetime")
                    .HasColumnName("column_five");
                entity.Property(e => e.ColumnFour).HasColumnName("column_four");
                entity.Property(e => e.ColumnThree).HasColumnName("column_three");
                entity.Property(e => e.ContactPerson)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("contact_person");
                entity.Property(e => e.Country).HasColumnName("country");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("created_date");
                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("email_address");
                entity.Property(e => e.FaxNo)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fax_no");
                entity.Property(e => e.IsMainLocation).HasColumnName("isMainLocation");
                entity.Property(e => e.LandMark)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("land_mark");
                entity.Property(e => e.LandNumber)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("land_number");
                entity.Property(e => e.Latitude).HasMaxLength(50);
                entity.Property(e => e.LocationMap)
                    .HasMaxLength(500)
                    .HasColumnName("location_map");
                entity.Property(e => e.LocationMapName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("location_map_name");
                entity.Property(e => e.LocationMapType)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("location_map_type");
                entity.Property(e => e.LocationName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("location_name");
                entity.Property(e => e.Longitude).HasMaxLength(50);
                entity.Property(e => e.MobileNumber)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("mobile_number");
                entity.Property(e => e.Pin)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("pin");
                entity.Property(e => e.PuId).HasColumnName("pu_id");
                entity.Property(e => e.State)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("state");
                entity.Property(e => e.Website)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("website");
            });

            modelBuilder.Entity<TblParamRoleMaster>(entity =>
            {
                entity.ToTable("tbl_param_role_master");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.DepartmentId).HasColumnName("department_id");
                entity.Property(e => e.Description)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.Role)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });          

            modelBuilder.Entity<TmpAllCandidate>(entity =>
            {
                entity.HasKey(e => e.CandProfId).HasName("PK__tmpAllCa__5C7E6E0CFF2E9BFC");

                entity.ToTable("tmpAllCandidates");

                entity.Property(e => e.CandProfId)
                    .ValueGeneratedNever()
                    .HasColumnName("CandProfID");
                entity.Property(e => e.AllEvaluation).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.AllSelfRating).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.AlteContactNo).HasMaxLength(20);
                entity.Property(e => e.CandName).HasMaxLength(150);
                entity.Property(e => e.ContactNo).HasMaxLength(20);
                entity.Property(e => e.CountryId).HasColumnName("CountryID");
                entity.Property(e => e.CountryName)
                    .HasMaxLength(80)
                    .IsUnicode(false);
                entity.Property(e => e.Cpcurrency)
                    .HasMaxLength(5)
                    .HasColumnName("CPCurrency");
                entity.Property(e => e.CpgrossPayPerAnnum).HasColumnName("CPGrossPayPerAnnum");
                entity.Property(e => e.CptakeHomeSalPerMonth).HasColumnName("CPTakeHomeSalPerMonth");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.CurrLocation).HasMaxLength(100);
                entity.Property(e => e.CurrLocationId).HasColumnName("CurrLocationID");
                entity.Property(e => e.CurrOrganization).HasMaxLength(100);
                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");
                entity.Property(e => e.EmailId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("EmailID");
                entity.Property(e => e.Epcurrency)
                    .HasMaxLength(10)
                    .HasColumnName("EPCurrency");
                entity.Property(e => e.EptakeHomePerMonth).HasColumnName("EPTakeHomePerMonth");
                entity.Property(e => e.Evaluation).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Experience).HasMaxLength(100);
                entity.Property(e => e.FullNameInPp)
                    .HasMaxLength(150)
                    .HasColumnName("FullNameInPP");
                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.MaritalStatus)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Nationality)
                    .HasMaxLength(150)
                    .IsUnicode(false);
                entity.Property(e => e.Opcurrency)
                    .HasMaxLength(10)
                    .HasColumnName("OPCurrency");
                entity.Property(e => e.RelevantExperience).HasMaxLength(20);
                entity.Property(e => e.SelfRating).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.SourceId).HasColumnName("SourceID");
                entity.Property(e => e.StageId).HasColumnName("StageID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });          

            modelBuilder.Entity<VwAllCandidate>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwAllCandidates");

                entity.Property(e => e.AlteContactNo).HasMaxLength(20);
                entity.Property(e => e.CandName).HasMaxLength(150);
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CandProfStatusName)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false);
                entity.Property(e => e.ContactNo).HasMaxLength(20);
                entity.Property(e => e.CountryId).HasColumnName("CountryID");
                entity.Property(e => e.CountryName)
                    .HasMaxLength(80)
                    .IsUnicode(false);
                entity.Property(e => e.Cpcurrency)
                    .HasMaxLength(5)
                    .HasColumnName("CPCurrency");
                entity.Property(e => e.CpgrossPayPerAnnum).HasColumnName("CPGrossPayPerAnnum");
                entity.Property(e => e.CptakeHomeSalPerMonth).HasColumnName("CPTakeHomeSalPerMonth");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.CurrLocation).HasMaxLength(100);
                entity.Property(e => e.CurrLocationId).HasColumnName("CurrLocationID");
                entity.Property(e => e.CurrOrganization).HasMaxLength(100);
                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");
                entity.Property(e => e.EmailId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("EmailID");
                entity.Property(e => e.Epcurrency)
                    .HasMaxLength(10)
                    .HasColumnName("EPCurrency");
                entity.Property(e => e.EptakeHomePerMonth).HasColumnName("EPTakeHomePerMonth");
                entity.Property(e => e.Experience).HasMaxLength(100);
                entity.Property(e => e.FullNameInPp)
                    .HasMaxLength(150)
                    .HasColumnName("FullNameInPP");
                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.JobCategory).HasMaxLength(100);
                entity.Property(e => e.MaritalStatus)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Nationality)
                    .HasMaxLength(150)
                    .IsUnicode(false);
                entity.Property(e => e.OpCurrency).HasMaxLength(10);
                entity.Property(e => e.RecName)
                    .IsRequired()
                    .HasMaxLength(101);
                entity.Property(e => e.RelevantExperience).HasMaxLength(20);
                entity.Property(e => e.SourceId).HasColumnName("SourceID");
                entity.Property(e => e.StageId).HasColumnName("StageID");
                entity.Property(e => e.TagWords)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwAllrecruiter>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwALLRecruiters");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.LastName).HasMaxLength(50);
            });

            modelBuilder.Entity<VwCandidateSourceDtl>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwCandidateSourceDtls");

                entity.Property(e => e.BroughtByName)
                    .IsRequired()
                    .HasMaxLength(101);
                entity.Property(e => e.BroughtByPhoto)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.CandName).HasMaxLength(150);
                entity.Property(e => e.CandProfStatus)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.CandProfilePhoto)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.ClientName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.EmailId)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.JobTitle)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.MobileNumber).HasMaxLength(20);
                entity.Property(e => e.RecruiterName)
                    .IsRequired()
                    .HasMaxLength(101);
                entity.Property(e => e.RecruiterProfilePhoto)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.StatusCode).HasMaxLength(5);
            });

            modelBuilder.Entity<VwDashboardCandidateInterview>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwDashboardCandidateInterview");

                entity.Property(e => e.BdmId).HasColumnName("bdmId");
                entity.Property(e => e.BdmName)
                    .HasMaxLength(101)
                    .HasColumnName("bdmName");
                entity.Property(e => e.BdmPhoto)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("bdmPhoto");
                entity.Property(e => e.CandName).HasMaxLength(150);
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CandProfStatus).HasColumnName("candProfStatus");
                entity.Property(e => e.CandProfStatusCode)
                    .HasMaxLength(5)
                    .HasColumnName("candProfStatusCode");
                entity.Property(e => e.CityName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.ClientId).HasColumnName("ClientID");
                entity.Property(e => e.ClientName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.ContactNo).HasMaxLength(20);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.EmailId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("EmailID");
                entity.Property(e => e.Experience).HasMaxLength(100);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.InterviewDate).HasColumnType("date");
                entity.Property(e => e.InterviewEndTime).HasMaxLength(15);
                entity.Property(e => e.InterviewStartTime).HasMaxLength(15);
                entity.Property(e => e.JobTitle)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.RecrName)
                    .HasMaxLength(101)
                    .HasColumnName("recrName");
                entity.Property(e => e.RecrPhoto)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("recrPhoto");
                entity.Property(e => e.RecruiterId).HasColumnName("recruiterID");
                entity.Property(e => e.ReopenedDate).HasColumnType("datetime");
                entity.Property(e => e.TabNo).HasColumnName("tabNo");
            });

            modelBuilder.Entity<VwDashboardDaywiseFilterDatum>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwDashboardDaywiseFilterData");

                entity.Property(e => e.BdmId).HasColumnName("bdmId");
                entity.Property(e => e.Buid).HasColumnName("BUID");
                entity.Property(e => e.ClosedDate).HasColumnType("datetime");
                entity.Property(e => e.JobAssignmentDate)
                    .HasColumnType("datetime")
                    .HasColumnName("jobAssignmentDate");
                entity.Property(e => e.JobId).HasColumnName("jobId");
                entity.Property(e => e.NoCvsrequired).HasColumnName("NoCVSRequired");
                entity.Property(e => e.NoCvsuploadded).HasColumnName("NoCVSUploadded");
                entity.Property(e => e.NoOfFinalCvsFilled).HasColumnName("NoOfFinalCVsFilled");
                entity.Property(e => e.Puid).HasColumnName("PUID");
                entity.Property(e => e.RecruiterId).HasColumnName("recruiterID");
            });

            modelBuilder.Entity<VwDashboardJobStage>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwDashboardJobStage");

                entity.Property(e => e.BdmId).HasColumnName("bdmId");
                entity.Property(e => e.ClientName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("clientName");
                entity.Property(e => e.ClosedDate).HasColumnType("datetime");
                entity.Property(e => e.JobCityId).HasColumnName("jobCityId");
                entity.Property(e => e.JobCountryId).HasColumnName("jobCountryId");
                entity.Property(e => e.JobId).HasColumnName("jobId");
                entity.Property(e => e.JobTitle)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.PostedDate).HasColumnType("datetime");
                entity.Property(e => e.ReopenedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwDashboardJobStageRecruiter>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwDashboardJobStageRecruiter");

                entity.Property(e => e.BdmId).HasColumnName("bdmId");
                entity.Property(e => e.JobId).HasColumnName("jobId");
                entity.Property(e => e.PostedDate).HasColumnType("datetime");
                entity.Property(e => e.RecrName)
                    .HasMaxLength(101)
                    .HasColumnName("recrName");
                entity.Property(e => e.RecruiterId).HasColumnName("recruiterID");
            });

            modelBuilder.Entity<VwDashboardRecruiterStatus>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwDashboardRecruiterStatus");

                entity.Property(e => e.BdmId).HasColumnName("bdmId");
                entity.Property(e => e.ClientName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.ClosedDate).HasColumnType("datetime");
                entity.Property(e => e.JobCityId).HasColumnName("jobCityId");
                entity.Property(e => e.JobCountryId).HasColumnName("jobCountryId");
                entity.Property(e => e.JobId).HasColumnName("jobId");
                entity.Property(e => e.JobTitle)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.NoCvsrequired).HasColumnName("NoCVSRequired");
                entity.Property(e => e.NoOfFinalCvsFilled).HasColumnName("NoOfFinalCVsFilled");
                entity.Property(e => e.PostedDate).HasColumnType("datetime");
                entity.Property(e => e.RecruiterId).HasColumnName("recruiterID");
            });

            modelBuilder.Entity<VwJob>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwJobs");

                entity.Property(e => e.CityName)
                    .HasMaxLength(1000)
                    .IsUnicode(false);
                entity.Property(e => e.ClientName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.ClosedDate).HasColumnType("datetime");
                entity.Property(e => e.CountryId).HasColumnName("CountryID");
                entity.Property(e => e.CountryName)
                    .HasMaxLength(80)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedByName).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.JobLocationId).HasColumnName("JobLocationID");
                entity.Property(e => e.JobOpeningStatusName).HasMaxLength(50);
                entity.Property(e => e.JobRole).HasMaxLength(100);
                entity.Property(e => e.JobTitle)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.Jscode)
                    .HasMaxLength(5)
                    .HasColumnName("JSCode");
                entity.Property(e => e.ShortJobDesc).HasMaxLength(1000);
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.Status).HasColumnName("status");
            });

            modelBuilder.Entity<VwJobCandProfileStatusHistory>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwJobCandProfileStatusHistory");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.CurrentStatusId).HasColumnName("CurrentStatusID");
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ID");
                entity.Property(e => e.Joid).HasColumnName("JOID");
                entity.Property(e => e.UpdateStatusId).HasColumnName("UpdateStatusID");
            });

            modelBuilder.Entity<VwJobCandidate>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwJobCandidates");

                entity.Property(e => e.AlteContactNo).HasMaxLength(20);
                entity.Property(e => e.CandName).HasMaxLength(150);
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CandProfStatusName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.ContactNo).HasMaxLength(20);
                entity.Property(e => e.CountryId).HasColumnName("CountryID");
                entity.Property(e => e.CountryName)
                    .HasMaxLength(80)
                    .IsUnicode(false);
                entity.Property(e => e.Cpcurrency)
                    .HasMaxLength(5)
                    .HasColumnName("CPCurrency");
                entity.Property(e => e.CpgrossPayPerAnnum).HasColumnName("CPGrossPayPerAnnum");
                entity.Property(e => e.CptakeHomeSalPerMonth).HasColumnName("CPTakeHomeSalPerMonth");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.CsCode).HasMaxLength(5);
                entity.Property(e => e.CurrLocation).HasMaxLength(100);
                entity.Property(e => e.CurrLocationId).HasColumnName("CurrLocationID");
                entity.Property(e => e.CurrOrganization).HasMaxLength(100);
                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");
                entity.Property(e => e.EmailId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("EmailID");
                entity.Property(e => e.Epcurrency)
                    .HasMaxLength(10)
                    .HasColumnName("EPCurrency");
                entity.Property(e => e.EptakeHomePerMonth).HasColumnName("EPTakeHomePerMonth");
                entity.Property(e => e.Evaluation).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Experience).HasMaxLength(100);
                entity.Property(e => e.FullNameInPp)
                    .HasMaxLength(150)
                    .HasColumnName("FullNameInPP");
                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.L1review).HasColumnName("L1Review");
                entity.Property(e => e.MaritalStatus)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Mreview).HasColumnName("MReview");
                entity.Property(e => e.Nationality)
                    .HasMaxLength(150)
                    .IsUnicode(false);
                entity.Property(e => e.OpCurrency).HasMaxLength(10);
                entity.Property(e => e.RecName)
                    .IsRequired()
                    .HasMaxLength(101);
                entity.Property(e => e.RelevantExperience).HasMaxLength(20);
                entity.Property(e => e.SourceId).HasColumnName("SourceID");
                entity.Property(e => e.StageId).HasColumnName("StageID");
                entity.Property(e => e.Tlreview).HasColumnName("TLReview");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwJobCandidateStatusHistory>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwJobCandidateStatusHistory");

                entity.Property(e => e.ActivityDate).HasColumnType("date");
                entity.Property(e => e.Buid).HasColumnName("BUID");
                entity.Property(e => e.CandName).HasMaxLength(150);
                entity.Property(e => e.ClientId).HasColumnName("ClientID");
                entity.Property(e => e.ClientName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.JobTitle)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.OpgrossPayPerMonth).HasColumnName("OPGrossPayPerMonth");
                entity.Property(e => e.Puid).HasColumnName("PUID");
                entity.Property(e => e.StatusCode).HasMaxLength(5);
            });

            modelBuilder.Entity<VwJobCandidatesByRole>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwJobCandidates_ByRole");

                entity.Property(e => e.AlteContactNo).HasMaxLength(20);
                entity.Property(e => e.CandName).HasMaxLength(150);
                entity.Property(e => e.CandProfId).HasColumnName("CandProfID");
                entity.Property(e => e.CandProfStatusName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.ContactNo).HasMaxLength(20);
                entity.Property(e => e.CountryId).HasColumnName("CountryID");
                entity.Property(e => e.CountryName)
                    .HasMaxLength(80)
                    .IsUnicode(false);
                entity.Property(e => e.Cpcurrency)
                    .HasMaxLength(5)
                    .HasColumnName("CPCurrency");
                entity.Property(e => e.CpgrossPayPerAnnum).HasColumnName("CPGrossPayPerAnnum");
                entity.Property(e => e.CptakeHomeSalPerMonth).HasColumnName("CPTakeHomeSalPerMonth");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.CsCode).HasMaxLength(5);
                entity.Property(e => e.CurrLocation).HasMaxLength(100);
                entity.Property(e => e.CurrLocationId).HasColumnName("CurrLocationID");
                entity.Property(e => e.CurrOrganization).HasMaxLength(100);
                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");
                entity.Property(e => e.EmailId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("EmailID");
                entity.Property(e => e.Epcurrency)
                    .HasMaxLength(10)
                    .HasColumnName("EPCurrency");
                entity.Property(e => e.EptakeHomePerMonth).HasColumnName("EPTakeHomePerMonth");
                entity.Property(e => e.Evaluation).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Experience).HasMaxLength(100);
                entity.Property(e => e.FullNameInPp)
                    .HasMaxLength(150)
                    .HasColumnName("FullNameInPP");
                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.MaritalStatus)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Nationality)
                    .HasMaxLength(150)
                    .IsUnicode(false);
                entity.Property(e => e.OpCurrency).HasMaxLength(10);
                entity.Property(e => e.RecName)
                    .IsRequired()
                    .HasMaxLength(101);
                entity.Property(e => e.RelevantExperience).HasMaxLength(20);
                entity.Property(e => e.SourceId).HasColumnName("SourceID");
                entity.Property(e => e.StageId).HasColumnName("StageID");
                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwJobStatusHistory>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwJobStatusHistory");

                entity.Property(e => e.ActivityDate).HasColumnType("date");
                entity.Property(e => e.OldStatusCode).HasMaxLength(5);
                entity.Property(e => e.StatusCode).HasMaxLength(5);
            });

            modelBuilder.Entity<VwUserPuBu>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwUserPuBu");
            });

            modelBuilder.Entity<VwUserTargetsByMonthAndTarget>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vwUserTargetsByMonthAndTarget");

                entity.Property(e => e.MonthYear)
                    .HasColumnType("date")
                    .HasColumnName("month_year");
                entity.Property(e => e.TargetDescription)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("targetDescription");
                entity.Property(e => e.TargetQtySet).HasColumnName("Target_qty_set");
                entity.Property(e => e.TargetValue)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("targetValue");
            });

            OnModelCreatingCustom(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (IsConnectionString)
                {
                    optionsBuilder.UseSqlServer(ConnectionString);
                }
                else
                {
                    var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
                    // define the database to use
                    optionsBuilder.UseSqlServer(config.GetConnectionString("dbConnection"));
                }
            }
        }       
    }
}
