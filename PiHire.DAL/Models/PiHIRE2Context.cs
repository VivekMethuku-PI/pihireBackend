using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PiHire.DAL.Models;

namespace PiHire.DAL
{
    public partial class PiHIRE2Context
    {
        bool IsConnectionString { get; set; }
        string ConnectionString { get; set; }

        public PiHIRE2Context(string ConnectionString)
        {
            if (string.IsNullOrEmpty(ConnectionString) == false)
            {
                IsConnectionString = true;
                this.ConnectionString = ConnectionString;
            }
        }


        protected void OnModelCreatingCustom(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CandidateActiveJobModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<CandidateArchivedJobModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<JobsList>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<JobCountModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<JobsListCount>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<AuditList>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<AuditCounts>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<SuitableCandidatesViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<ClientViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<TasksViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<CustomSchedulerViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<CustomSchedulerJobViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetPUViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetBUViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<RecsViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<UsersViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<PuLocationsModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetDeviceConnectionsIdViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetModuleModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<ActivitesList>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<NotesList>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<CountryModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<CityModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<ClientSpocsModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<DashboardCandiateInterviewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<DashboardJobStageModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<DashboardJobRecruiterStageModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<DashboardRecruiterStatusModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<DashboardCountModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetDashboardHireAdminModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetDashboardBdmModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetDashboardRecruiterModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetDashboardRecruiterJobCategoryModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetDashboardRecruiterCandidateModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetDashboardRecruiterAnalyticModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetDashboardRecruiterAnalyticGraphModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<Sp_Similar_JobsModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetChatRoomModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetChatRoomCountModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<CountryWiseJobCountModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<RecruitersOverviewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<BDMsOverviewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<RecruiterOverviewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<BDMOverviewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<BDMOpeningOverviewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<RecruiterOpeningOverviewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<JobDtlsWorkflowModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<JobSkillsWorkflowModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<CandDtlsWorkflowModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<JobCandsWorkflowModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<ClientSharedCandidatesModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<CandidatesViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<CandidateCountModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<JobCandidatesViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<LocationWiseJobCountModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<TechnologiesModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<TechnologiesModelCount>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<JobCandidateListFilterDataModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<CandidatesSourceOverviewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<PuLocationViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<SourcedCandidatesModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<WebSourceRecruitersOverviewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<TodayJobAssignment>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<Sp_UserType_LocationsModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<JobCandidatesBasedOnProfileStatusViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<Sp_Dashboard_Daywise_FilterModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<GetUserPUViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<InterviewStageStatus>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<DashboardJobsList>(entity => { entity.HasNoKey(); });

            modelBuilder.Entity<CandidateStageWiseViewModel>(entity => { entity.HasNoKey(); });
            modelBuilder.Entity<Sp_BroughtBy_Job_ClientNamesModel>(entity => { entity.HasNoKey(); });
        }


        public async Task<CandidateListModel> GetCandidateList(string SearchKey,
            string Recruiter, string Rating, string ApplicationStatus, string Gender, string Nationality, string CurrentLocation,
            string Source, string Currency, string MaritalStatus, byte UserType, int UserId, int? PuId = null, int? Availability = null,
            int? SalaryMinRange = null, int? SalaryMaxRange = null,
            int? MinAge = null, int? MaxAge = null, int? JobId = null, DateTime? fromDate = null, DateTime? toDate = null, int? PerPage = null, int? CurrentPage = null)
        {
            var model = new CandidateListModel();
            _ = new SqlParameter();
            SqlParameter param1;
            if (string.IsNullOrEmpty(SearchKey))
                param1 = new SqlParameter("@SearchKey", DBNull.Value);
            else
                param1 = new SqlParameter("@SearchKey", SearchKey);

            SqlParameter param2;
            if (string.IsNullOrEmpty(Rating))
                param2 = new SqlParameter("@Rating", DBNull.Value);
            else
                param2 = new SqlParameter("@Rating", Rating);

            SqlParameter param3;
            if (string.IsNullOrEmpty(ApplicationStatus))
                param3 = new SqlParameter("@ApplicationStatus", DBNull.Value);
            else
                param3 = new SqlParameter("@ApplicationStatus", ApplicationStatus);

            SqlParameter param4;
            if (string.IsNullOrEmpty(Gender))
                param4 = new SqlParameter("@Gender", DBNull.Value);
            else
                param4 = new SqlParameter("@Gender", Gender);

            SqlParameter param5;
            if (string.IsNullOrEmpty(Nationality))
                param5 = new SqlParameter("@Nationality", DBNull.Value);
            else
                param5 = new SqlParameter("@Nationality", Nationality);

            SqlParameter param6;
            if (string.IsNullOrEmpty(CurrentLocation))
                param6 = new SqlParameter("@CurrentLocation", DBNull.Value);
            else
                param6 = new SqlParameter("@CurrentLocation", CurrentLocation);

            SqlParameter param7;
            if (string.IsNullOrEmpty(Recruiter))
                param7 = new SqlParameter("@Recruiter", DBNull.Value);
            else
                param7 = new SqlParameter("@Recruiter", Recruiter);


            SqlParameter param8;
            if (string.IsNullOrEmpty(Source))
                param8 = new SqlParameter("@Source", DBNull.Value);
            else
                param8 = new SqlParameter("@Source", Source);


            SqlParameter param9;
            if (string.IsNullOrEmpty(MaritalStatus))
                param9 = new SqlParameter("@MaritalStatus", DBNull.Value);
            else
                param9 = new SqlParameter("@MaritalStatus", MaritalStatus);

            SqlParameter param10;
            if (string.IsNullOrEmpty(Currency))
                param10 = new SqlParameter("@Currency", DBNull.Value);
            else
                param10 = new SqlParameter("@Currency", Currency);

            var param11 = new SqlParameter("@Availability", (object)Availability ?? DBNull.Value);
            var param12 = new SqlParameter("@MinAge", (object)MinAge ?? DBNull.Value);
            var param13 = new SqlParameter("@MaxAge", (object)MaxAge ?? DBNull.Value);

            var param14 = new SqlParameter("@SalaryMinRange", (object)SalaryMinRange ?? DBNull.Value);
            var param15 = new SqlParameter("@SalaryMaxRange", (object)SalaryMaxRange ?? DBNull.Value);


            var param16 = new SqlParameter("@PuId", (object)PuId ?? DBNull.Value);
            var param19 = new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value);
            var param20 = new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value);

            var param17 = new SqlParameter("@fromDate", (object)fromDate ?? DBNull.Value);
            var param18 = new SqlParameter("@toDate", (object)toDate ?? DBNull.Value);

            model.CandidatesViewModel = await this.Set<CandidatesViewModel>().FromSqlRaw("EXEC [dbo].[Sp_Candidate_List] @SearchKey,@Rating,@ApplicationStatus,@Gender,@Nationality,@CurrentLocation,@Recruiter,@Source,@MaritalStatus,@PuId,@Currency,@Availability,@MinAge,@MaxAge,@SalaryMinRange,@SalaryMaxRange,@fromDate,@toDate,@PerPage,@CurrentPage", param1, param2, param3, param4, param5, param6, param7, param8, param9, param16, param10, param11, param12, param13, param14, param15, param17, param18, param19, param20).ToListAsync();

            var dtls = await this.Set<CandidateCountModel>().FromSqlRaw("EXEC [dbo].[Sp_Candidate_List_Count] @SearchKey,@Rating,@ApplicationStatus,@Gender,@Nationality,@CurrentLocation,@Recruiter,@Source,@MaritalStatus,@PuId,@Currency,@Availability,@MinAge,@MaxAge,@SalaryMinRange,@SalaryMaxRange,@fromDate,@toDate", param1, param2, param3, param4, param5, param6, param7, param8, param9, param16, param10, param11, param12, param13, param14, param15, param17, param18).ToListAsync();
            if (dtls.Count > 0)
            {
                model.CandidateCount = dtls[0].CandidateCount;
            }

            return model;
        }


        public async Task<JobCandidateListModel> GetJobCandidateList(string SearchKey,
           string Recruiter, string Rating, string ApplicationStatus, string Gender, string Nationality, string CurrentLocation,
           string Source, string Currency, string MaritalStatus, byte UserType, int UserId, int? Availability = null, int? SalaryMinRange = null, int? SalaryMaxRange = null, int? MinAge = null, int? MaxAge = null, int? JobId = null, bool? MyCandidates = null, bool? tlReview = null, bool? dmReview = null, bool? l1Review = null, DateTime? fromDate = null, DateTime? toDate = null, int? PerPage = null, int? CurrentPage = null)
        {
            var model = new JobCandidateListModel();
            _ = new SqlParameter();
            SqlParameter param1;
            if (string.IsNullOrEmpty(SearchKey))
                param1 = new SqlParameter("@SearchKey", DBNull.Value);
            else
                param1 = new SqlParameter("@SearchKey", SearchKey);

            SqlParameter param2;
            if (string.IsNullOrEmpty(Rating))
                param2 = new SqlParameter("@Rating", DBNull.Value);
            else
                param2 = new SqlParameter("@Rating", Rating);

            SqlParameter param3;
            if (string.IsNullOrEmpty(ApplicationStatus))
                param3 = new SqlParameter("@ApplicationStatus", DBNull.Value);
            else
                param3 = new SqlParameter("@ApplicationStatus", ApplicationStatus);

            SqlParameter param4;
            if (string.IsNullOrEmpty(Gender))
                param4 = new SqlParameter("@Gender", DBNull.Value);
            else
                param4 = new SqlParameter("@Gender", Gender);

            SqlParameter param5;
            if (string.IsNullOrEmpty(Nationality))
                param5 = new SqlParameter("@Nationality", DBNull.Value);
            else
                param5 = new SqlParameter("@Nationality", Nationality);

            SqlParameter param6;
            if (string.IsNullOrEmpty(CurrentLocation))
                param6 = new SqlParameter("@CurrentLocation", DBNull.Value);
            else
                param6 = new SqlParameter("@CurrentLocation", CurrentLocation);

            SqlParameter param7;
            if (string.IsNullOrEmpty(Recruiter))
                param7 = new SqlParameter("@Recruiter", DBNull.Value);
            else
                param7 = new SqlParameter("@Recruiter", Recruiter);

            SqlParameter param8;
            if (string.IsNullOrEmpty(Source))
                param8 = new SqlParameter("@Source", DBNull.Value);
            else
                param8 = new SqlParameter("@Source", Source);

            SqlParameter param9;
            if (string.IsNullOrEmpty(MaritalStatus))
                param9 = new SqlParameter("@MaritalStatus", DBNull.Value);
            else
                param9 = new SqlParameter("@MaritalStatus", MaritalStatus);

            SqlParameter param10;
            if (string.IsNullOrEmpty(Currency))
                param10 = new SqlParameter("@Currency", DBNull.Value);
            else
                param10 = new SqlParameter("@Currency", Currency);

            var param11 = new SqlParameter("@Availability", (object)Availability ?? DBNull.Value);
            var param12 = new SqlParameter("@MinAge", (object)MinAge ?? DBNull.Value);
            var param13 = new SqlParameter("@MaxAge", (object)MaxAge ?? DBNull.Value);

            var param14 = new SqlParameter("@SalaryMinRange", (object)SalaryMinRange ?? DBNull.Value);
            var param15 = new SqlParameter("@SalaryMaxRange", (object)SalaryMaxRange ?? DBNull.Value);

            var param16 = new SqlParameter("@JobId", (object)JobId ?? DBNull.Value);
            var param17 = new SqlParameter("@UserType", (object)UserType ?? DBNull.Value);
            var param18 = new SqlParameter("@UserId", (object)UserId ?? DBNull.Value);

            var param19 = new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value);
            var param20 = new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value);
            var param21 = new SqlParameter("@MyCandidates", (object)MyCandidates ?? DBNull.Value);

            var param22 = new SqlParameter("@dmReview", (object)dmReview ?? DBNull.Value);
            var param23 = new SqlParameter("@tlReview", (object)tlReview ?? DBNull.Value);
            var param24 = new SqlParameter("@l1Review", (object)l1Review ?? DBNull.Value);

            var param25 = new SqlParameter("@fromDate", (object)fromDate ?? DBNull.Value);
            var param26 = new SqlParameter("@toDate", (object)toDate ?? DBNull.Value);

            model.CandidatesViewModel = await this.Set<JobCandidatesViewModel>().FromSqlRaw("EXEC [dbo].[Sp_Job_Candidate_List] @SearchKey,@Rating,@ApplicationStatus,@Gender,@Nationality,@CurrentLocation,@Recruiter,@Source,@MaritalStatus,@Currency,@Availability,@MinAge,@MaxAge,@SalaryMinRange,@SalaryMaxRange,@JobId,@UserType,@UserId,@MyCandidates,@dmReview,@tlReview,@l1Review,@fromDate,@toDate,@PerPage,@CurrentPage", param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param21, param22, param23, param24, param25, param26, param19, param20).ToListAsync();

            var dtls = await this.Set<CandidateCountModel>().FromSqlRaw("EXEC [dbo].[Sp_Job_Candidate_List_Count] @SearchKey,@Rating,@ApplicationStatus,@Gender,@Nationality,@CurrentLocation,@Recruiter,@Source,@MaritalStatus,@Currency,@Availability,@MinAge,@MaxAge,@SalaryMinRange,@SalaryMaxRange,@JobId,@UserType,@UserId,@MyCandidates,@dmReview,@tlReview,@l1Review,@fromDate,@toDate", param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param21, param22, param23, param24, param25, param26).ToListAsync();
            if (dtls.Count > 0)
            {
                model.CandidateCount = dtls[0].CandidateCount;
            }

            return model;
        }


        public async Task<List<JobCandidateListFilterDataModel>> GetJobCandidateListFilterData(byte UserType, int UserId, int? JobId = null)
        {
            var param16 = new SqlParameter("@JobId", (object)JobId ?? DBNull.Value);
            var param17 = new SqlParameter("@UserType", (object)UserType ?? DBNull.Value);
            var param18 = new SqlParameter("@UserId", (object)UserId ?? DBNull.Value);

            var model = await this.Set<JobCandidateListFilterDataModel>().FromSqlRaw("EXEC [dbo].[Sp_Job_Candidate_List_FilterData] @JobId,@UserType,@UserId", param16, param17, param18).ToListAsync();

            return model;
        }


        public async Task<List<JobCandidateListFilterDataModel>> GetJobSuggestListFilterData(byte UserType, int UserId, int JobId)
        {
            var param16 = new SqlParameter("@JobId", (object)JobId ?? DBNull.Value);
            var param17 = new SqlParameter("@UserType", (object)UserType ?? DBNull.Value);
            var param18 = new SqlParameter("@UserId", (object)UserId ?? DBNull.Value);

            var model = await this.Set<JobCandidateListFilterDataModel>().FromSqlRaw("EXEC [dbo].[Sp_Suggest_Job_Candidate_List_FilterData] @JobId,@UserType,@UserId", param16, param17, param18).ToListAsync();

            return model;
        }


        public async Task<JobCandidateListModel> GetSuggestCandidateList(string SearchKey,
           string Recruiter, string Rating, string ApplicationStatus, string Gender, string Nationality, string CurrentLocaiton,
           string Source, string Currency, string MaritalStatus, int JobId, int? Availability = null, int? SalaryMinRange = null, int? SalaryMaxRange = null,
           int? MinAge = null, int? MaxAge = null, int? PerPage = null, int? CurrentPage = null)
        {

            var model = new JobCandidateListModel();
            _ = new SqlParameter();
            SqlParameter param1;
            if (string.IsNullOrEmpty(SearchKey))
                param1 = new SqlParameter("@SearchKey", DBNull.Value);
            else
                param1 = new SqlParameter("@SearchKey", SearchKey);

            SqlParameter param2;
            if (string.IsNullOrEmpty(Recruiter))
                param2 = new SqlParameter("@Recruiter", DBNull.Value);
            else
                param2 = new SqlParameter("@Recruiter", Recruiter);

            SqlParameter param3;
            if (string.IsNullOrEmpty(Rating))
                param3 = new SqlParameter("@Rating", DBNull.Value);
            else
                param3 = new SqlParameter("@Rating", Rating);

            SqlParameter param4;
            if (string.IsNullOrEmpty(ApplicationStatus))
                param4 = new SqlParameter("@ApplicationStatus", DBNull.Value);
            else
                param4 = new SqlParameter("@ApplicationStatus", ApplicationStatus);

            SqlParameter param5;
            if (string.IsNullOrEmpty(Gender))
                param5 = new SqlParameter("@Gender", DBNull.Value);
            else
                param5 = new SqlParameter("@Gender", Gender);

            SqlParameter param6;
            if (string.IsNullOrEmpty(Nationality))
                param6 = new SqlParameter("@Nationality", DBNull.Value);
            else
                param6 = new SqlParameter("@Nationality", Nationality);

            SqlParameter param7;
            if (string.IsNullOrEmpty(CurrentLocaiton))
                param7 = new SqlParameter("@CurrentLocation", DBNull.Value);
            else
                param7 = new SqlParameter("@CurrentLocation", CurrentLocaiton);

            SqlParameter param8;
            if (string.IsNullOrEmpty(Source))
                param8 = new SqlParameter("@Source", DBNull.Value);
            else
                param8 = new SqlParameter("@Source", Source);


            SqlParameter param9;
            if (string.IsNullOrEmpty(MaritalStatus))
                param9 = new SqlParameter("@MaritalStatus", DBNull.Value);
            else
                param9 = new SqlParameter("@MaritalStatus", MaritalStatus);

            SqlParameter param10;
            if (string.IsNullOrEmpty(Currency))
                param10 = new SqlParameter("@Currency", DBNull.Value);
            else
                param10 = new SqlParameter("@Currency", Currency);


            var param11 = new SqlParameter("@MinAge", (object)MinAge ?? DBNull.Value);
            var param12 = new SqlParameter("@MaxAge", (object)MaxAge ?? DBNull.Value);

            var param13 = new SqlParameter("@Availability", (object)Availability ?? DBNull.Value);

            var param14 = new SqlParameter("@SalaryMinRange", (object)SalaryMinRange ?? DBNull.Value);
            var param15 = new SqlParameter("@SalaryMaxRange", (object)SalaryMaxRange ?? DBNull.Value);

            var param16 = new SqlParameter("@JobId", (object)JobId ?? DBNull.Value);

            var param17 = new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value);
            var param18 = new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value);

            model.CandidatesViewModel = await this.Set<JobCandidatesViewModel>().FromSqlRaw("EXEC [dbo].[Sp_Suggest_Job_Candidate_List] @SearchKey,@Recruiter,@Rating,@ApplicationStatus,@Gender,@Nationality,@CurrentLocation,@Source,@MaritalStatus,@Currency,@MinAge,@MaxAge,@Availability,@SalaryMinRange,@SalaryMaxRange,@JobId,@PerPage,@CurrentPage", param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18).ToListAsync();

            var dtls = await this.Set<CandidateCountModel>().FromSqlRaw("EXEC [dbo].[Sp_Suggest_Job_Candidate_List_Count] @SearchKey,@Recruiter,@Rating,@ApplicationStatus,@Gender,@Nationality,@CurrentLocation,@Source,@MaritalStatus,@Currency,@MinAge,@MaxAge,@Availability,@SalaryMinRange,@SalaryMaxRange,@JobId", param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16).ToListAsync();
            if (dtls.Count > 0)
            {
                model.CandidateCount = dtls[0].CandidateCount;
            }

            return model;
        }


        public async Task<List<PuLocationsModel>> GetCompanyLocations(int[] cId)
        {
            List<PuLocationsModel> puLocationsModels;
            var puIds = string.Join(",", cId);
            using (var connection = this.Database.GetDbConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[Sp_Pu_Locations] @PuId";

                command.Parameters.Add(new SqlParameter("@PuId", (object)puIds ?? DBNull.Value));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    puLocationsModels = DataTableToList<PuLocationsModel>(dataTable);
                }
            }
            return puLocationsModels;
        }


        public async Task<List<Sp_UserType_LocationsModel>> GetUserTypeLocations(short? UserType)
        {
            return await this.Set<Sp_UserType_LocationsModel>().FromSqlRaw("EXEC [dbo].[Sp_UserType_Locations] @UserType", new SqlParameter("@UserType", (object)UserType ?? DBNull.Value)).ToListAsync();
        }

        public async Task<List<PuLocationViewModel>> GetCompanyLocations(int cId)
        {
            return await this.Set<PuLocationViewModel>().FromSqlRaw("EXEC [dbo].[Sp_Company_Locations] @cId", new SqlParameter("@cId", (object)cId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<PuLocationViewModel>> GetCompanyLocation(int locId)
        {
            return await this.Set<PuLocationViewModel>().FromSqlRaw("EXEC [dbo].[Sp_Company_Location] @locId", new SqlParameter("@locId", (object)locId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<UsersViewModel>> GetUsers()
        {
            return await this.Set<UsersViewModel>().FromSqlRaw("EXEC [dbo].[Sp_GetUsers]").ToListAsync();
        }


        public async Task<List<CountryWiseJobCountModel>> GetCountryWiseJobCounts()
        {
            return await this.Set<CountryWiseJobCountModel>().FromSqlRaw("EXEC [dbo].[Sp_Country_Wise_Job_Count]").ToListAsync();
        }


        public async Task<List<LocationWiseJobCountModel>> GetLocationWiseJobCounts(int CountryId)
        {
            return await this.Set<LocationWiseJobCountModel>().FromSqlRaw("EXEC [dbo].[Sp_Location_Wise_Job_Count] @CountryId", new SqlParameter("@CountryId", (object)CountryId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<JobDtlsWorkflowModel> GetJobDtlsPerWorkflowRules(int? JoId)
        {
            var resp = await this.Set<JobDtlsWorkflowModel>().FromSqlRaw("EXEC	[dbo].[Sp_JobDtls_Per_Workflow_Rules]		@JobId", new SqlParameter("@JobId", (object)JoId ?? DBNull.Value)).ToListAsync();
            return resp.FirstOrDefault();
        }


        public async Task<List<JobSkillsWorkflowModel>> GetJobSkillsPerWorkflowRules(int? JoId)
        {
            return await this.Set<JobSkillsWorkflowModel>().FromSqlRaw("EXEC	[dbo].[Sp_JobSkills_Per_Workflow_Rules]		@JobId ", new SqlParameter("@JobId", (object)JoId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<CandDtlsWorkflowModel> GetCandDtlsPerWorkflowRules(int? JoId, int? CandId)
        {
            var resp = await this.Set<CandDtlsWorkflowModel>().FromSqlRaw("EXEC [dbo].[Sp_CandDtls_Per_Workflow_Rules]  @JobId, @CandId", new SqlParameter("@JobId", (object)JoId ?? DBNull.Value), new SqlParameter("@CandId", (object)CandId ?? DBNull.Value)).ToListAsync();
            return resp.FirstOrDefault();
        }


        public async Task<List<JobCandsWorkflowModel>> GetJobCandDtlsPerWorkflowRules(int? JoId)
        {
            return await this.Set<JobCandsWorkflowModel>().FromSqlRaw("EXEC [dbo].[Sp_Job_CandDtls_Per_Workflow_Rule]  @JobId", new SqlParameter("@JobId", (object)JoId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<ClientViewModel>> GetClients(int? EmpId, int Usertype)
        {
            return await this.Set<ClientViewModel>().FromSqlRaw("EXEC [dbo].[Sp_Clients] @EmpId,@Usertype", new SqlParameter("@EmpId", (object)EmpId ?? DBNull.Value), new SqlParameter("@Usertype", (object)Usertype ?? DBNull.Value)).ToListAsync();
        }


        public async Task<TechnologyModel> GetTechnologies(string SearchKey, int? PerPage, int? CurrentPage)
        {
            var model = new TechnologyModel();
            var param1 = new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value);
            var param2 = new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value);
            var param3 = new SqlParameter("@SearchKey", (object)SearchKey ?? DBNull.Value);

            model.TechnologiesModel = await this.Set<TechnologiesModel>().FromSqlRaw("EXEC [dbo].[Sp_Technologies] @PerPage, @CurrentPage, @SearchKey", param1, param2, param3).ToListAsync();

            var dtls = await this.Set<TechnologiesModelCount>().FromSqlRaw("EXEC [dbo].[Sp_Technologies_Count] @SearchKey", param3).ToListAsync();
            if (dtls.Count > 0)
            {
                model.TechCount = dtls[0].TechCount;
            }
            return model;
        }


        public async Task<List<ClientViewModel>> GetClient(int ClientId)
        {
            return await this.Set<ClientViewModel>().FromSqlRaw("EXEC [dbo].[Sp_Client] @ClientId", new SqlParameter("@ClientId", (object)ClientId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<ClientSpocsModel>> GetClientSpocs(int? ClientId)
        {
            return await this.Set<ClientSpocsModel>().FromSqlRaw("EXEC [dbo].[Sp_Client_Spocs] @ClientId",
                new SqlParameter("@ClientId", (object)ClientId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<RecruiterOverviewModel>> GetRecruiterOverview(DateTime Fromdate,
            DateTime Todate, int? PuId, string StatusCode, byte UserType, int? UserId, int? RecId)
        {
            return await this.Set<RecruiterOverviewModel>().FromSqlRaw("EXEC [dbo].[Sp_Report_Recruiter_overview] @FromDate,@Todate,@PuId,@StatusCode,@UserType,@UserId,@RecId",
                new SqlParameter("@FromDate", Fromdate),
                new SqlParameter("@Todate", Todate),
                new SqlParameter("@PuId", (object)PuId ?? DBNull.Value),
                new SqlParameter("@StatusCode", StatusCode),
                new SqlParameter("@UserType", UserType),
                new SqlParameter("@UserId", UserId),
                new SqlParameter("@RecId", (object)RecId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<RecruitersOverviewModel>> GetRecruitersOverview(DateTime Fromdate, DateTime Todate,
            byte UserType, int UsersId, int? PuId)
        {
            return await this.Set<RecruitersOverviewModel>().FromSqlRaw("EXEC [dbo].[Sp_Report_Recruiters_overview] @FromDate,@Todate,@UserType,@UsersId,@PuId",
                new SqlParameter("@FromDate", Fromdate),
                new SqlParameter("@Todate", Todate),
                new SqlParameter("@UserType", UserType),
                new SqlParameter("@UsersId", UsersId),
                new SqlParameter("@PuId", (object)PuId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<WebSourceRecruitersOverviewModel>> GetSourcedWebsiteCandidates(DateTime Fromdate, DateTime Todate, int? RecId, int? PuId)
        {
            return await this.Set<WebSourceRecruitersOverviewModel>().FromSqlRaw("EXEC [dbo].[Sp_Report_Web_Source_Recruiters_overview] @FromDate,@Todate,@RecId,@PuId",
                new SqlParameter("@FromDate", Fromdate),
                new SqlParameter("@Todate", Todate),
                new SqlParameter("@RecId", (object)RecId ?? DBNull.Value),
                new SqlParameter("@PuId", (object)PuId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<RecruiterOpeningOverviewModel>> GetRecruiterOpeningOverview(DateTime Fromdate, DateTime Todate, int UserId, byte UserTypeId, int? PuId, int? RecId)
        {
            return await this.Set<RecruiterOpeningOverviewModel>().FromSqlRaw("EXEC [dbo].[Sp_Report_Recruiters_Opening_overview] @FromDate,@Todate,@UserId,@UserType,@PuId,@RecId",
                new SqlParameter("@FromDate", Fromdate),
                new SqlParameter("@Todate", Todate),
                new SqlParameter("@UserId", UserId),
                new SqlParameter("@UserType", UserTypeId),
                new SqlParameter("@PuId", (object)PuId ?? DBNull.Value),
                new SqlParameter("@RecId", (object)RecId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<CandidatesSourceOverviewModel>> GetCandidatesSourceOverview(DateTime Fromdate, DateTime Todate, int? PuId, byte UserType, int UserId)
        {
            return await this.Set<CandidatesSourceOverviewModel>().FromSqlRaw("EXEC [dbo].[Sp_Report_Candidates_Source_overview] @FromDate,@Todate,@UserType,@UsersId,@PuId",
                new SqlParameter("@FromDate", Fromdate),
                new SqlParameter("@Todate", Todate),
                new SqlParameter("@UserType", UserType),
                new SqlParameter("@UsersId", UserId),
                new SqlParameter("@PuId", (object)PuId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<SourcedCandidatesModel>> GetSourcedCandidates(DateTime Fromdate, DateTime Todate, int? PuId, int UserType, int UserId, int? RecId, string SourcedFrom)
        {
            return await this.Set<SourcedCandidatesModel>().FromSqlRaw("EXEC [dbo].[Sp_Report_Sourced_Candidates] @FromDate,@Todate,@puId,@recId,@userType,@userId,@sourcedFrom",
                new SqlParameter("@FromDate", Fromdate),
                new SqlParameter("@Todate", Todate),
                new SqlParameter("@puId", (object)PuId ?? DBNull.Value),
                new SqlParameter("@recId", (object)RecId ?? DBNull.Value),
                new SqlParameter("@userType", UserType),
                new SqlParameter("@userId", UserId),
                new SqlParameter("@sourcedFrom", SourcedFrom)).ToListAsync();
        }



        public async Task<List<BDMsOverviewModel>> GetBDMsOverview(DateTime Fromdate, DateTime Todate, int? PuId, byte UserType, int UserId)
        {
            return await this.Set<BDMsOverviewModel>().FromSqlRaw("EXEC [dbo].[Sp_Report_BDMs_overview] @FromDate,@Todate,@userType,@usersId,@puId",
                new SqlParameter("@FromDate", Fromdate),
                new SqlParameter("@Todate", Todate),
                new SqlParameter("@userType", UserType),
                new SqlParameter("@usersId", UserId),
                new SqlParameter("@puId", (object)PuId ?? DBNull.Value)).ToListAsync();
        }



        public async Task<List<BDMOpeningOverviewModel>> GetBDMOpeningOverview(DateTime Fromdate, DateTime Todate, int? PuId, byte UserType, int UserId, int? bdmId)
        {
            return await this.Set<BDMOpeningOverviewModel>().FromSqlRaw("EXEC [dbo].[Sp_Report_BDMs_Opening_overview] @FromDate,@Todate,@puId,@userId,@userType,@bdmId",
                new SqlParameter("@FromDate", Fromdate), new SqlParameter("@Todate", Todate),
                new SqlParameter("@puId", (object)PuId ?? DBNull.Value),
                new SqlParameter("@userId", UserId),
                new SqlParameter("@userType", UserType),
                new SqlParameter("@bdmId", (object)bdmId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<BDMOverviewModel>> GetBDMOverview(DateTime Fromdate, DateTime Todate, int? PuId, string StatusCode, byte UserType, int UserId, int? bdmId)
        {
            return await this.Set<BDMOverviewModel>().FromSqlRaw("EXEC [dbo].[Sp_Report_BDM_overview] @FromDate,@Todate,@puId,@StatusCode,@userId,@userType,@bdmId",
                new SqlParameter("@FromDate", Fromdate),
                new SqlParameter("@Todate", Todate),
                new SqlParameter("@puId", (object)PuId ?? DBNull.Value),
                new SqlParameter("@StatusCode", StatusCode),
                new SqlParameter("@userId", UserId),
                new SqlParameter("@userType", UserType),
                new SqlParameter("@bdmId", (object)bdmId ?? DBNull.Value)).ToListAsync();
        }



        public async Task<List<GetModuleModel>> GetModules(int ApplicationID)
        {
            return await this.Set<GetModuleModel>().FromSqlRaw("EXEC [dbo].[Sp_Modules] @ApplicationID", new SqlParameter("@ApplicationID", ApplicationID)).ToListAsync();
        }


        public async Task<List<TasksViewModel>> GetTasks(int ApplicationID, int? ModuleId)
        {
            return await this.Set<TasksViewModel>().FromSqlRaw("EXEC [dbo].[Sp_Tasks] @ApplicationID, @ModuleId", new SqlParameter("@ApplicationID", ApplicationID), new SqlParameter("@ModuleId", (object)ModuleId ?? DBNull.Value)).ToListAsync();
        }


        public async Task<List<GetPUViewModel>> GetPUs()
        {
            return await this.Set<GetPUViewModel>().FromSqlRaw("EXEC [dbo].[Sp_ProcessUnits]").ToListAsync();
        }


        public async Task<List<GetBUViewModel>> GetUserBuListAsync(string puId, int type, int id)
        {
            SqlParameter userType;
            userType = new SqlParameter("@userType", (object)type ?? DBNull.Value);
            SqlParameter userId;
            userId = new SqlParameter("@userId", (object)id ?? DBNull.Value);
            SqlParameter puIds;
            if (string.IsNullOrEmpty(puId))
                puIds = new SqlParameter("@puID", DBNull.Value);
            else
                puIds = new SqlParameter("@puID", puId);

            var response = await this.Set<GetBUViewModel>().FromSqlRaw("EXEC [dbo].[Sp_User_BusinessUnits] @puID,@userType,@userId", puIds, userType, userId).ToListAsync();
            return response;
        }


        public async Task<List<GetUserPUViewModel>> GetUserPuListAsync(int type, int id)
        {
            SqlParameter userType;
            userType = new SqlParameter("@userType", (object)type ?? DBNull.Value);
            SqlParameter userId;
            userId = new SqlParameter("@userId", (object)id ?? DBNull.Value);
            var response = await this.Set<GetUserPUViewModel>().FromSqlRaw("EXEC [dbo].[Sp_User_ProcessUnits] @userType,@userId", userType, userId).ToListAsync();
            return response;
        }


        public async Task<List<GetBUViewModel>> GetBUs(string puId)
        {
            SqlParameter puIds;
            if (string.IsNullOrEmpty(puId))
                puIds = new SqlParameter("@puID", DBNull.Value);
            else
                puIds = new SqlParameter("@puID", puId);

            return await this.Set<GetBUViewModel>().FromSqlRaw("EXEC [dbo].[Sp_BusinessUnits] @puID", puIds).ToListAsync();
        }

        public async Task<TagJobModel> GetJobsListToTagCandidate(string SearchKey, int CandidateId, int? PerPage = null, int? CurrentPage = null)
        {
            var model = new TagJobModel();

            using (var connection = this.Database.GetDbConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[Sp_JobsTo_Tag] @SearchKey,@CandidateId,@PerPage,@CurrentPage";

                if (string.IsNullOrEmpty(SearchKey))
                    command.Parameters.Add(new SqlParameter("@SearchKey", DBNull.Value));
                else
                    command.Parameters.Add(new SqlParameter("@SearchKey", SearchKey));

                command.Parameters.Add(new SqlParameter("@CandidateId", (object)CandidateId ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    var canList = DataTableToList<ToTagJobsList>(dataTable);

                    dataTable = new DataTable();
                    dataTable.Load(reader);
                    var tmp = DataTableToList<JobCountModel>(dataTable);
                    if (tmp.Count > 0)
                    {
                        var cntCount = tmp[0];
                        model.JobCount = cntCount.JobsCount;
                        model.JobList = canList;
                    }
                    else
                    {
                        model.JobCount = 0;
                        model.JobList = new List<ToTagJobsList>();
                    }

                }
            }
            return model;
        }



        public async Task<JobsModel> GetJobsList(string SearchKey, string Sort, string SortDirection, int UserId, int UserType, int? FilterType = null, int? PerPage = null, int? CurrentPage = null)
        {
            var model = new JobsModel();
            var param1 = new SqlParameter("@FilterKey", (object)SearchKey ?? DBNull.Value);
            var param2 = new SqlParameter("@Sort", (object)Sort ?? DBNull.Value);
            var param3 = new SqlParameter("@SortDirection", (object)SortDirection ?? DBNull.Value);
            var param4 = new SqlParameter("@UserId", (object)UserId ?? DBNull.Value);
            var param5 = new SqlParameter("@UserType", (object)UserType ?? DBNull.Value);
            var param6 = new SqlParameter("@StatusFilter", (object)FilterType ?? DBNull.Value);
            var param7 = new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value);
            var param8 = new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value);

            model.JobList = await this.Set<JobsList>().FromSqlRaw("EXEC [dbo].[Sp_Jobs_List] @FilterKey,@Sort,@SortDirection,@UserId,@UserType,@StatusFilter,@PerPage,@CurrentPage", param1, param2, param3, param4, param5, param6, param7, param8).ToListAsync();

            var dtls = await this.Set<JobsListCount>().FromSqlRaw("EXEC [dbo].[Sp_Jobs_List_Count] @FilterKey,@Sort,@SortDirection,@UserId,@UserType,@StatusFilter", param1, param2, param3, param4, param5, param6).ToListAsync();
            if (dtls.Count > 0)
            {
                model.JobCount = dtls.Count();
            }

            return model;
        }


        public async Task<JobsModel> GetJobsToAssignRecruiters(string SearchKey, int UserId, int? PerPage = null, int? CurrentPage = null)
        {
            var model = new JobsModel();
            using (var connection = this.Database.GetDbConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[Sp_Jobs_To_Assign_Recruiters] @SearchKey,@PerPage,@CurrentPage,@UserId";

                command.Parameters.Add(new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@UserId", (object)UserId ?? DBNull.Value));

                if (string.IsNullOrEmpty(SearchKey))
                {
                    command.Parameters.Add(new SqlParameter("@SearchKey", DBNull.Value));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@SearchKey", SearchKey));
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    var jobList = DataTableToList<JobsList>(dataTable);

                    dataTable = new DataTable();
                    dataTable.Load(reader);
                    var tmp = DataTableToList<JobCountModel>(dataTable);
                    if (tmp.Count > 0)
                    {
                        var cntCount = tmp[0];
                        model.JobCount = cntCount.JobsCount;
                        model.JobList = jobList;
                    }
                    else
                    {
                        model.JobCount = 0;
                        model.JobList = new List<JobsList>();
                    }
                }
            }
            return model;
        }


        public async Task<List<CandidateActiveJobModel>> GetCandidateActiveArchivedJobs(int CandPrfId, int FilterType)
        {
            return await this.Set<CandidateActiveJobModel>().FromSqlRaw("EXEC [dbo].[Sp_Candidate_Active_Archived_Jobs] @CandidateId,@FilterType", new SqlParameter("@CandidateId", CandPrfId), new SqlParameter("@FilterType", FilterType)).ToListAsync();
        }


        public async Task<List<RecsViewModel>> GetRecuiters()
        {
            return await this.Set<RecsViewModel>().FromSqlRaw("EXEC [dbo].[Sp_GetRecuiters]").ToListAsync();
        }


        public async Task<List<TodayJobAssignment>> GetTodayJobAssignments()
        {
            return await this.Set<TodayJobAssignment>().FromSqlRaw("EXEC [dbo].[Sp_Today_Job_Assignments]").ToListAsync();
        }


        public async Task<List<ClientSharedCandidatesModel>> GetCandidatesSharedToClient(int JobId, int Type)
        {
            return await this.Set<ClientSharedCandidatesModel>().FromSqlRaw("EXEC [dbo].[Sp_CandidateSharedTo_Client] @Type,@JobId", new SqlParameter("@Type", Type), new SqlParameter("@JobId", JobId)).ToListAsync();
        }


        public async Task<List<CandidateArchivedJobModel>> GetCandidateArchivedJobs(int CandPrfId)
        {
            return await this.Set<CandidateArchivedJobModel>().FromSqlRaw("EXEC [dbo].[Sp_Candidate_Archived_Jobs] @CandidateId", new SqlParameter("@CandidateId", CandPrfId)).ToListAsync();
        }


        public async Task<List<string>> GetDeviceConnectionsId(int[] userIds)
        {
            List<GetDeviceConnectionsIdViewModel> connections;
            var userIdStr = userIds == null ? "" : string.Join(",", userIds);
            connections = await this.Set<GetDeviceConnectionsIdViewModel>().FromSqlRaw("EXEC [dbo].[Sp_GetConnectionsId] @UserId", new SqlParameter("@UserId", (object)userIdStr ?? DBNull.Value)).ToListAsync();
            return connections.Select(s => s.DeviceUID).ToList();
        }


        public async Task<int> TmpAllCandidatesUpdate()
        {
            var data = await this.Database.ExecuteSqlRawAsync("EXEC [dbo].[pi_Sp_tmpAllCandidates_Update_trigger]");
            return data;
        }

        public async Task<int> RecruiterJobAssignmentsCarryForwardAsync()
        {
            var data = await this.Database.ExecuteSqlRawAsync("EXEC [dbo].[Sp_Recruiter_JobAssignment_CarryForward]");
            return data;
        }

        public async Task<CandidateSimilarJobModel> GetCandidateSimilarJobs(string JobIds, int? PerPage, int? CurrentPage, string SearchKey, int? CountryId, int CandPrfId)
        {
            var model = new CandidateSimilarJobModel();

            using (var connection = this.Database.GetDbConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[Sp_Candidate_Similar_Jobs] @CandPrfId,@PerPage,@CurrentPage,@CountryId,@SearchKey,@JobIds";

                command.Parameters.Add(new SqlParameter("@CandPrfId", (object)CandPrfId ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@CountryId", (object)CountryId ?? DBNull.Value));

                if (string.IsNullOrEmpty(SearchKey))
                {
                    command.Parameters.Add(new SqlParameter("@SearchKey", DBNull.Value));
                }
                else
                {
                    SearchKey = SearchKey.Replace("%", " ");
                    command.Parameters.Add(new SqlParameter("@SearchKey", SearchKey));
                }

                if (string.IsNullOrEmpty(JobIds))
                {
                    command.Parameters.Add(new SqlParameter("@JobIds", DBNull.Value));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@JobIds", JobIds));
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    var jobList = DataTableToList<CandidateSimilarJobs>(dataTable);

                    dataTable = new DataTable();
                    dataTable.Load(reader);
                    var tmp = DataTableToList<CandidateSimilarJobCount>(dataTable);
                    if (tmp.Count > 0)
                    {
                        var cntCount = tmp[0];
                        model.SimilarJobCount = cntCount.SimilarJobCount;
                        model.CandidateSimilarJobs = jobList;
                    }
                    else
                    {
                        model.SimilarJobCount = 0;
                        model.CandidateSimilarJobs = new List<CandidateSimilarJobs>();
                    }
                }
            }
            return model;
        }

        public async Task<OfferedCandidatesModel> GetOfferdCandidatesList(string SearchKey, int? PerPage, int? CurrentPage)
        {
            var model = new OfferedCandidatesModel();

            using (var connection = this.Database.GetDbConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[Sp_Offered_Candidates] @SearchKey,@PerPage,@CurrentPage";

                command.Parameters.Add(new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value));

                if (string.IsNullOrEmpty(SearchKey))
                {
                    command.Parameters.Add(new SqlParameter("@SearchKey", DBNull.Value));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@SearchKey", SearchKey));
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    var jobList = DataTableToList<OfferdCandidateList>(dataTable);

                    dataTable = new DataTable();
                    dataTable.Load(reader);
                    var tmp = DataTableToList<OfferdCandidateCount>(dataTable);
                    if (tmp.Count > 0)
                    {
                        var cntCount = tmp[0];
                        model.CandCount = cntCount.CandCount;
                        model.OfferdCandidateList = jobList;
                    }
                    else
                    {
                        model.CandCount = 0;
                        model.OfferdCandidateList = new List<OfferdCandidateList>();
                    }
                }
            }
            return model;
        }

        public async Task<PortalJobModel> GetPortalJobsList(int? PerPage, int? CurrentPage, int? CountryId, int? LocationId, List<SearchKeyModel> searchKeyModel)
        {
            var model = new PortalJobModel();
            using (var connection = this.Database.GetDbConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[Sp_Portal_Jobs] @PerPage,@CurrentPage,@CountryId,@LocationId,@TechIds,@JobRole";

                string TechIds = string.Join(",", searchKeyModel.Where(p => p.Category == "TE").Select(p => p.Display));
                if (string.IsNullOrEmpty(TechIds))
                {
                    command.Parameters.Add(new SqlParameter("@TechIds", DBNull.Value));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@TechIds", TechIds));
                }

                string JobRole = string.Join(",", searchKeyModel.Where(p => p.Category == "JR").Select(p => p.Display));
                if (string.IsNullOrEmpty(JobRole))
                {
                    command.Parameters.Add(new SqlParameter("@JobRole", DBNull.Value));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@JobRole", JobRole));
                }

                if (CountryId == null || CountryId == 0)
                {
                    command.Parameters.Add(new SqlParameter("@CountryId", DBNull.Value));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@CountryId", CountryId));
                }

                if (LocationId == null || LocationId == 0)
                {
                    command.Parameters.Add(new SqlParameter("@LocationId", DBNull.Value));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@LocationId", LocationId));
                }

                command.Parameters.Add(new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    var jobList = DataTableToList<PortalJobs>(dataTable);

                    dataTable = new DataTable();
                    dataTable.Load(reader);
                    var tmp = DataTableToList<PortalJobCounts>(dataTable);
                    if (tmp.Count > 0)
                    {
                        var cntCount = tmp[0];
                        model.PortalJobCount = cntCount.PortalJobCount;
                        model.PortalJobs = jobList;
                    }
                    else
                    {
                        model.PortalJobCount = 0;
                        model.PortalJobs = new List<PortalJobs>();
                    }
                }
            }
            return model;
        }



        public async Task<List<CityModel>> GeCityList()
        {
            _ = new List<CityModel>();
            List<CityModel> model = await Set<CityModel>().FromSqlRaw("EXEC [dbo].[Sp_Cities]").ToListAsync();
            return model;
        }


        public async Task<List<CountryModel>> GeCountryList()
        {
            _ = new List<CountryModel>();
            List<CountryModel> model = await Set<CountryModel>().FromSqlRaw("EXEC [dbo].[Sp_Countries]").ToListAsync();
            return model;
        }


        public async Task<List<NotesList>> GeJobNoteList(int jobId)
        {
            _ = new List<NotesList>();
            List<NotesList> model = await Set<NotesList>().FromSqlRaw("EXEC [dbo].[Sp_Job_Notes] @JobId", new SqlParameter("@JobId", jobId)).ToListAsync();
            return model;
        }


        public async Task<List<ActivitesList>> GeJobActiviesList(int jobId)
        {
            _ = new List<ActivitesList>();
            List<ActivitesList> model = await Set<ActivitesList>().FromSqlRaw("EXEC [dbo].[Sp_Job_Activities] @JobId", new SqlParameter("@JobId", jobId)).ToListAsync();
            return model;
        }


        public async Task<AuditModel> GetAuditList(int? PerPage, int? CurrentPage, int? UserType, int? UserId, int? AuditType, int? SUserId, DateTime? FromDate, DateTime? ToDate)
        {
            var model = new AuditModel();
            using (var connection = this.Database.GetDbConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[Sp_User_Audits] @PerPage,@CurrentPage,@UserType,@UserId,@AuditType,@SUserId,@FromDate,@ToDate";
                if (AuditType == 0)
                {
                    AuditType = null;
                }
                if (SUserId == 0)
                {
                    SUserId = null;
                }
                command.Parameters.Add(new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@UserType", (object)UserType ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@UserId", (object)UserId ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@AuditType", (object)AuditType ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@SUserId", (object)SUserId ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@FromDate", (object)FromDate ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@ToDate", (object)ToDate ?? DBNull.Value));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    var jobList = DataTableToList<AuditList>(dataTable);

                    dataTable = new DataTable();
                    dataTable.Load(reader);
                    var tmp = DataTableToList<AuditCounts>(dataTable);
                    if (tmp.Count > 0)
                    {
                        var cntCount = tmp[0];
                        model.AuditCount = cntCount.AuditCount;
                        model.Audits = jobList;
                    }
                    else
                    {
                        model.AuditCount = 0;
                        model.Audits = new List<AuditList>();
                    }
                }
            }
            return model;
        }

        public async Task<ActivitesModel> GetActivitiesList(byte UserType, int? PerPage, int? CurrentPage, int? UserId, int? ActivityType, int? SUserId, DateTime? FromDate, DateTime? ToDate)
        {
            var model = new ActivitesModel();
            using (var connection = this.Database.GetDbConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[Sp_User_Activities] @PerPage,@CurrentPage,@UserId,@ActivityType,@UserType,@SUserId,@FromDate,@ToDate";
                if (ActivityType == 0)
                {
                    ActivityType = null;
                }
                if (SUserId == 0)
                {
                    SUserId = null;
                }
                command.Parameters.Add(new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@UserId", (object)UserId ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@ActivityType", (object)ActivityType ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@UserType", (object)UserType ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@SUserId", (object)SUserId ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@FromDate", (object)FromDate ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@ToDate", (object)ToDate ?? DBNull.Value));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    var activityList = DataTableToList<ActivitesList>(dataTable);

                    dataTable = new DataTable();
                    dataTable.Load(reader);
                    var tmp = DataTableToList<ActiviteCounts>(dataTable);
                    if (tmp.Count > 0)
                    {
                        var cntCount = tmp[0];
                        model.ActiviteCount = cntCount.ActiviteCount;
                        model.Activites = activityList;
                    }
                    else
                    {
                        model.ActiviteCount = 0;
                        model.Activites = new List<ActivitesList>();
                    }
                }
            }
            return model;
        }

        List<T> DataTableToList<T>(DataTable dt) where T : class, new()
        {
            List<T> lstItems = new List<T>();
            if (dt != null && dt.Rows.Count > 0)
                foreach (DataRow row in dt.Rows)
                    lstItems.Add(ConvertDataRowToGenericType<T>(row));
            return lstItems;
        }

        T ConvertDataRowToGenericType<T>(DataRow row) where T : class, new()
        {
            Type entityType = typeof(T);
            T objEntity = new T();
            foreach (DataColumn column in row.Table.Columns)
            {
                object value = row[column.ColumnName];
                if (value == DBNull.Value) value = null;
                PropertyInfo property = entityType.GetProperty(column.ColumnName, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
                try
                {
                    if (property != null && property.CanWrite)
                        property.SetValue(objEntity, value, null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return objEntity;
        }


        public async Task<IEnumerable<CustomSchedulerViewModel>> GetCustomSchedulerHireUser(string puIds, string buIds, string gndr, string cntrIds)
        {
            return
                await this.Set<CustomSchedulerViewModel>().FromSqlRaw("EXEC [dbo].[sp_customScheduler_hireUsr] @puIds, @buIds, @gndr, @cntrIds",
                    new SqlParameter("@puIds", (string.IsNullOrEmpty(puIds) ? "" : puIds)),
                    new SqlParameter("@buIds", (string.IsNullOrEmpty(buIds) ? "" : buIds)),
                    new SqlParameter("@gndr", (string.IsNullOrEmpty(gndr) ? "" : gndr)),
                    new SqlParameter("@cntrIds", (string.IsNullOrEmpty(cntrIds) ? "" : cntrIds))
                    ).ToListAsync();
        }


        public async Task<IEnumerable<CustomSchedulerViewModel>> GetCustomSchedulerHireUserBirthday(DateTime currentDate, string puIds, string buIds, string gndr, string cntrIds)
        {
            return
                await this.Set<CustomSchedulerViewModel>().FromSqlRaw("EXEC [dbo].[sp_customScheduler_hireUsr_birthday] @currentDate, @puIds, @buIds, @gndr, @cntrIds",
                    new SqlParameter("@currentDate", currentDate),
                    new SqlParameter("@puIds", (string.IsNullOrEmpty(puIds) ? "" : puIds)),
                    new SqlParameter("@buIds", (string.IsNullOrEmpty(buIds) ? "" : buIds)),
                    new SqlParameter("@gndr", (string.IsNullOrEmpty(gndr) ? "" : gndr)),
                    new SqlParameter("@cntrIds", (string.IsNullOrEmpty(cntrIds) ? "" : cntrIds))
                    ).ToListAsync();
        }


        public async Task<IEnumerable<CustomSchedulerViewModel>> GetCustomSchedulerCandidate(string puIds, string buIds, string gndr, string cntrIds, string candStatus)
        {
            return
                await this.Set<CustomSchedulerViewModel>().FromSqlRaw("EXEC [dbo].[sp_customScheduler_candidate] @puIds, @buIds, @gndr, @cntrIds, @candStatus ",
                    new SqlParameter("@puIds", (string.IsNullOrEmpty(puIds) ? "" : puIds)),
                    new SqlParameter("@buIds", (string.IsNullOrEmpty(buIds) ? "" : buIds)),
                    new SqlParameter("@gndr", (string.IsNullOrEmpty(gndr) ? "" : gndr)),
                    new SqlParameter("@cntrIds", (string.IsNullOrEmpty(cntrIds) ? "" : cntrIds)),
                    new SqlParameter("@candStatus", (string.IsNullOrEmpty(candStatus) ? "" : candStatus))
                    ).ToListAsync();
        }


        public async Task<IEnumerable<CustomSchedulerViewModel>> GetCustomSchedulerCandidateBirthday(DateTime currentDate, string puIds, string buIds, string gndr, string cntrIds, string candStatus)
        {
            return
                await this.Set<CustomSchedulerViewModel>().FromSqlRaw("EXEC [dbo].[sp_customScheduler_candidate_birthday] @currentDate, @puIds, @buIds, @gndr, @cntrIds, @candStatus",
                    new SqlParameter("@currentDate", currentDate),
                    new SqlParameter("@puIds", (string.IsNullOrEmpty(puIds) ? "" : puIds)),
                    new SqlParameter("@buIds", (string.IsNullOrEmpty(buIds) ? "" : buIds)),
                    new SqlParameter("@gndr", (string.IsNullOrEmpty(gndr) ? "" : gndr)),
                    new SqlParameter("@cntrIds", (string.IsNullOrEmpty(cntrIds) ? "" : cntrIds)),
                    new SqlParameter("@candStatus", (string.IsNullOrEmpty(candStatus) ? "" : candStatus))
                    ).ToListAsync();
        }


        public async Task<IEnumerable<CustomSchedulerViewModel>> GetCustomSchedulerCandidateJobOpenings(int jobId, string puIds, string buIds, string gndr, string cntrIds, string candStatus)
        {
            return
                await this.Set<CustomSchedulerViewModel>().FromSqlRaw("EXEC [dbo].[sp_customScheduler_candidate_jobOpenings] @puIds, @buIds, @gndr, @cntrIds, @candStatus, @jobId",
                    new SqlParameter("@puIds", (string.IsNullOrEmpty(puIds) ? "" : puIds)),
                    new SqlParameter("@buIds", (string.IsNullOrEmpty(buIds) ? "" : buIds)),
                    new SqlParameter("@gndr", (string.IsNullOrEmpty(gndr) ? "" : gndr)),
                    new SqlParameter("@cntrIds", (string.IsNullOrEmpty(cntrIds) ? "" : cntrIds)),
                    new SqlParameter("@candStatus", (string.IsNullOrEmpty(candStatus) ? "" : candStatus)),
                    new SqlParameter("@jobId", jobId)
                    ).ToListAsync();
        }


        public async Task<IEnumerable<CustomSchedulerJobViewModel>> GetCustomSchedulerJobDtls(int JobId)
        {
            return
                await this.Set<CustomSchedulerJobViewModel>().FromSqlRaw("EXEC [dbo].[sp_customScheduler_jobDtls] @jobId",
                    new SqlParameter("@jobId", JobId)
                    ).ToListAsync();
        }

        public async Task<List<DashboardJobStageModel>> GetDashboardJobStageAsync(int[] puIds, int[] buIds, int pageCount, int skipCount, int userType, int userId)
        {
            return await this.Set<DashboardJobStageModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_JobStage] @puIds,	@buIds,	@fetchCount, @offsetCount, @userType, @userId",
                    new SqlParameter("@puIds", puIds != null && puIds.Length > 0 ? string.Join(',', puIds) : ""),
                    new SqlParameter("@buIds", buIds != null && buIds.Length > 0 ? string.Join(',', buIds) : ""),
                    new SqlParameter("@fetchCount", pageCount), new SqlParameter("@offsetCount", skipCount),
                    new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
        }


        public async Task<DashboardCountModel> GetDashboardJobStageCountAsync(int[] puIds, int[] buIds, int userType, int userId)
        {
            var rslt = await this.Set<DashboardCountModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_JobStageCount] @puIds,	@buIds,	@userType, @userId",
                    new SqlParameter("@puIds", puIds != null && puIds.Length > 0 ? string.Join(',', puIds) : ""),
                    new SqlParameter("@buIds", buIds != null && buIds.Length > 0 ? string.Join(',', buIds) : ""),
                    new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
            return rslt.FirstOrDefault();
        }


        public async Task<List<Sp_Similar_JobsModel>> GetSimilarJobsAsync(int jobId)
        {
            var rslt = await this.Set<Sp_Similar_JobsModel>().FromSqlRaw("EXEC [dbo].[Sp_Similar_Jobs]	@jobId",
                new SqlParameter("@jobId", jobId)).ToListAsync();
            return rslt;
        }

        public async Task<List<DashboardJobRecruiterStageModel>> GetDashboardJobRecruiterStageAsync(int jobId, int userType, int userId)
        {
            return await this.Set<DashboardJobRecruiterStageModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_JobStage_recruiter] @jobId, @userType, @userId",
                    new SqlParameter("@jobId", jobId),
                    new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
        }


        public async Task<List<DashboardRecruiterStatusModel>> GetDashboardRecruiterStatusAsync(DateTime currentDt, DateTime? fromDate, DateTime? toDate, bool? onLeave, int userType, int userId)
        {
            return await this.Set<DashboardRecruiterStatusModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_RecruiterStatus]	@currentDt,	@fmDt,	@toDt,	@onLeave,	@userType,	@userId",
                new SqlParameter("@currentDt", (object)currentDt), new SqlParameter("@fmDt", (object)fromDate ?? DBNull.Value), new SqlParameter("@toDt", (object)toDate ?? DBNull.Value),
                new SqlParameter("@onLeave", (object)onLeave ?? DBNull.Value),
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
        }

        public async Task<List<Sp_Dashboard_Daywise_FilterModel>> GetDashboardDaywiseFilterAsync(DateTime currentDt, int filterUserType, bool? onLeave, int? locationId, int? jobId, int[] filterUserIds, int loginUserType, int loginUserId)
        {
            this.Database.SetCommandTimeout(Timeout.InfiniteTimeSpan);
            if (filterUserIds.Length == 0)
                filterUserIds = new int[] { 0 };
            return await this.Set<Sp_Dashboard_Daywise_FilterModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_Daywise_Filter]	@currentDt,	@filterUserType, @onLeave, @locationId, @jobId, @filterUserIds,	@loginUserType,	@loginUserId",
                new SqlParameter("@currentDt", (object)currentDt),
                new SqlParameter("@filterUserType", filterUserType),

                new SqlParameter("@onLeave", (object)onLeave ?? DBNull.Value),
                new SqlParameter("@locationId", (object)locationId ?? DBNull.Value),
                new SqlParameter("@jobId", (object)jobId ?? DBNull.Value),
                new SqlParameter("@filterUserIds", string.Join(',', filterUserIds)),

                new SqlParameter("@loginUserType", loginUserType),
                new SqlParameter("@loginUserId", loginUserId)).ToListAsync();
        }


        public async Task<GetDashboardHireAdminModel> GetDashboardHireAdminAsync(DateTime? fromDate, DateTime? toDate, int[] puIds, int[] buIds, DateTime currentDate, int userType, int userId)
        {
            var rslt = await this.Set<GetDashboardHireAdminModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_HireAdmin]	@fmDt,	@toDt, @puIds,	@buIds,	@currentDt,	@userType,	@userId",
                new SqlParameter("@fmDt", (object)fromDate ?? DBNull.Value), new SqlParameter("@toDt", (object)toDate ?? DBNull.Value),
                new SqlParameter("@puIds", puIds != null && puIds.Length > 0 ? string.Join(',', puIds) : ""),
                new SqlParameter("@buIds", buIds != null && buIds.Length > 0 ? string.Join(',', buIds) : ""),
                new SqlParameter("@currentDt", (object)currentDate ?? DBNull.Value),
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
            return rslt.FirstOrDefault();
        }


        public async Task<GetDashboardBdmModel> GetDashboardBdmAsync(DateTime? fromDate, DateTime? toDate, string JobCategory, int userType, int userId)
        {
            var rslt = await this.Set<GetDashboardBdmModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_BDM]	@fmDt,	@toDt, @JobCategory,	@userType,	@userId",
                new SqlParameter("@fmDt", (object)fromDate ?? DBNull.Value), new SqlParameter("@toDt", (object)toDate ?? DBNull.Value),
                new SqlParameter("@JobCategory", (object)JobCategory ?? DBNull.Value),
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
            return rslt.FirstOrDefault();
        }


        public async Task<GetDashboardRecruiterModel> GetDashboardRecruiterAsync(DateTime? fromDate, DateTime? toDate, int userType, int userId)
        {
            var rslt = await this.Set<GetDashboardRecruiterModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_Recruiter]	@fmDt,	@toDt, @userType,	@userId",
                new SqlParameter("@fmDt", (object)fromDate ?? DBNull.Value), new SqlParameter("@toDt", (object)toDate ?? DBNull.Value),
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
            return rslt.FirstOrDefault();
        }


        public async Task<List<GetDashboardRecruiterJobCategoryModel>> GetDashboardRecruiterJobCategoryAsync(DateTime? fromDate, DateTime? toDate, int userType, int userId, bool typeCheck = true)
        {
            var rslt = await this.Set<GetDashboardRecruiterJobCategoryModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_Recruiter_JobCategory]	@fmDt,	@toDt, @typeId, @userType,	@userId",
                new SqlParameter("@fmDt", (object)fromDate ?? DBNull.Value), new SqlParameter("@toDt", (object)toDate ?? DBNull.Value),
                new SqlParameter("@typeId", typeCheck ? 1 : 2),
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
            return rslt;
        }


        public async Task<List<GetDashboardRecruiterCandidateModel>> GetDashboardRecruiterCandidatesAsync(DateTime? fromDate, DateTime? toDate, int userType, int userId, bool typeCheck = true)
        {
            var rslt = await this.Set<GetDashboardRecruiterCandidateModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_Recruiter_Candidates]	@fmDt,	@toDt, @typeId, @userType,	@userId",
                new SqlParameter("@fmDt", (object)fromDate ?? DBNull.Value), new SqlParameter("@toDt", (object)toDate ?? DBNull.Value),
                new SqlParameter("@typeId", typeCheck ? 1 : 2),
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
            return rslt;
        }


        public async Task<GetDashboardRecruiterAnalyticModel> GetDashboardRecruiterAnalyticAsync(DateTime? fromDate, DateTime? toDate, int userType, int userId)
        {
            var rslt = await this.Set<GetDashboardRecruiterAnalyticModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_Recruiter_Analytics]	@fmDt,	@toDt, @userType,	@userId",
                new SqlParameter("@fmDt", (object)fromDate ?? DBNull.Value), new SqlParameter("@toDt", (object)toDate ?? DBNull.Value),
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
            return rslt.FirstOrDefault();
        }


        public async Task<List<GetDashboardRecruiterAnalyticGraphModel>> GetDashboardRecruiterAnalyticGrphAsync(DateTime? fromDate, DateTime? toDate, int userType, int userId)
        {
            var rslt = await this.Set<GetDashboardRecruiterAnalyticGraphModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_Recruiter_Analytics_grph]	@fmDt,	@toDt, @userType,	@userId",
                new SqlParameter("@fmDt", (object)fromDate ?? DBNull.Value), new SqlParameter("@toDt", (object)toDate ?? DBNull.Value),
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
            return rslt;
        }


        public async Task<GetChatRoomCountModel> GetChatRoomsCountAsync(int userId)
        {
            var rslt = await this.Set<GetChatRoomCountModel>().FromSqlRaw("EXEC [dbo].[Sp_GetChatRoomCount] @userId",
                new SqlParameter("@userId", userId)).ToListAsync();
            return rslt.FirstOrDefault();
        }


        public async Task<List<GetChatRoomModel>> GetChatRoomsAsync(int pageCount, int skipCount, int userId)
        {
            var rslt = await this.Set<GetChatRoomModel>().FromSqlRaw("EXEC [dbo].[Sp_GetChatRoom] @fetchCount, @offsetCount, @userId",
                new SqlParameter("@fetchCount", pageCount), new SqlParameter("@offsetCount", skipCount),
                new SqlParameter("@userId", userId)).ToListAsync();
            return rslt;
        }



        #region New Dashboard 



        public async Task<DashboardJobsModel> GetDayWiseAssignmentJobsList(int userType, int userId, string SearchKey, int? bdmId, int? puId, int? JobPriority,
            bool? Assign, bool? PriorityUpdate, bool? Note, bool? Interviews, bool? JobStatus,
            DateTime? FromDate = null, DateTime? ToDate = null, int? PerPage = null, int? CurrentPage = null, int? clientId = null)
        {
            var model = new DashboardJobsModel();
            var param1 = new SqlParameter("@FilterKey", (object)SearchKey ?? DBNull.Value);
            var param2 = new SqlParameter("@bdmId", (object)bdmId ?? DBNull.Value);
            var param4 = new SqlParameter("@puId", (object)puId ?? DBNull.Value);
            var param9 = new SqlParameter("@JobPriority", (object)JobPriority ?? DBNull.Value);
            var param5 = new SqlParameter("@FromDate", (object)FromDate ?? DBNull.Value);
            var param6 = new SqlParameter("@ToDate", (object)ToDate ?? DBNull.Value);
            var param7 = new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value);
            var param8 = new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value);


            var param10 = new SqlParameter("@Assign", (object)Assign ?? DBNull.Value);
            var param11 = new SqlParameter("@PriorityUpdate", (object)PriorityUpdate ?? DBNull.Value);
            var param12 = new SqlParameter("@Note", (object)Note ?? DBNull.Value);
            var param13 = new SqlParameter("@Interviews", (object)Interviews ?? DBNull.Value);
            var param14 = new SqlParameter("@JobStatus", (object)JobStatus ?? DBNull.Value);

            var paramclientId = new SqlParameter("@clientId", (object)clientId ?? DBNull.Value);

            model.JobList = await this.Set<DashboardJobsList>().FromSqlRaw("EXEC [dbo].[Sp_Day_Wise_Assignment_Jobs_List] @FilterKey,@puId,@bdmId,@JobPriority,@FromDate,@ToDate,@PerPage,@CurrentPage,@Assign,@PriorityUpdate,@Note,@Interviews,@JobStatus, @clientId, @loginUserType, @loginUserId", param1, param4, param2, param9, param5, param6, param7, param8,
                param10, param11, param12, param13, param14, paramclientId,
                new SqlParameter("@loginUserType", userType),
                new SqlParameter("@loginUserId", userId)).ToListAsync();

            var dtls = await this.Set<JobsListCount>().FromSqlRaw("EXEC [dbo].[Sp_Day_Wise_Assignment_Jobs_List_Count] @FilterKey,@puId,@bdmId,@JobPriority,@FromDate,@ToDate,@Assign,@PriorityUpdate,@Note,@Interviews,@JobStatus, @clientId, @loginUserType, @loginUserId", param1, param4, param2, param9, param5, param6,
                 param10, param11, param12, param13, param14, paramclientId,
                new SqlParameter("@loginUserType", userType),
                new SqlParameter("@loginUserId", userId)).ToListAsync();
            if (dtls.Count > 0)
            {
                model.JobCount = dtls.Count();
            }


            return model;
        }



        public async Task<DashboardJobsModel> GetAssignmentJobsList(int userType, int userId, string SearchKey, int? bdmId, int? puId, int? JobPriority, DateTime? FromDate = null, DateTime? ToDate = null, int? PerPage = null, int? CurrentPage = null, int? clientId = null, int? jobStatusId = null, bool? assignmentStatus = null)
        {
            var model = new DashboardJobsModel();
            var param1 = new SqlParameter("@FilterKey", (object)SearchKey ?? DBNull.Value);
            var param2 = new SqlParameter("@bdmId", (object)bdmId ?? DBNull.Value);
            var param4 = new SqlParameter("@puId", (object)puId ?? DBNull.Value);
            var param9 = new SqlParameter("@JobPriority", (object)JobPriority ?? DBNull.Value);
            var param5 = new SqlParameter("@FromDate", (object)FromDate ?? DBNull.Value);
            var param6 = new SqlParameter("@ToDate", (object)ToDate ?? DBNull.Value);
            var param7 = new SqlParameter("@PerPage", (object)PerPage ?? DBNull.Value);
            var param8 = new SqlParameter("@CurrentPage", (object)CurrentPage ?? DBNull.Value);

            var paramclientId = new SqlParameter("@clientId", (object)clientId ?? DBNull.Value);
            var paramjobStatusId = new SqlParameter("@jobStatusId", (object)jobStatusId ?? DBNull.Value);
            var paramassignmentStatus = new SqlParameter("@assignmentStatus", (object)assignmentStatus ?? DBNull.Value);

            model.JobList = await this.Set<DashboardJobsList>().FromSqlRaw("EXEC [dbo].[Sp_Assignment_Jobs_List] @FilterKey,@puId,@bdmId,@JobPriority,@FromDate,@ToDate,@PerPage,@CurrentPage,@clientId,@jobStatusId,@assignmentStatus, @userType, @userId", param1, param4, param2, param9, param5, param6, param7, param8,
                paramclientId, paramjobStatusId, paramassignmentStatus,
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();

            var dtls = await this.Set<JobsListCount>().FromSqlRaw("EXEC [dbo].[Sp_Assignment_Jobs_List_Count] @FilterKey,@puId,@bdmId,@JobPriority,@FromDate,@ToDate,@clientId,@jobStatusId,@assignmentStatus, @userType, @userId", param1, param4, param2, param9, param5, param6,
                paramclientId, paramjobStatusId, paramassignmentStatus,
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
            if (dtls.Count > 0)
            {
                model.JobCount = dtls.Count();
            }


            return model;
        }
        public async Task<List<Sp_BroughtBy_Job_ClientNamesModel>> GetBroughtByJobClientNamesAsync(int boughtBy, int? puId, int loginUserType, int loginUserId)
        {
            var data = await this.Set<Sp_BroughtBy_Job_ClientNamesModel>().FromSqlRaw(
                "EXEC [dbo].[Sp_BroughtBy_Job_ClientNames] @boughtBy,@puId,@loginUserType,@loginUserId",

                new SqlParameter("@boughtBy", (object)boughtBy ?? DBNull.Value),
                new SqlParameter("@puId", (object)puId ?? DBNull.Value),
                new SqlParameter("@loginUserType", (object)loginUserType ?? DBNull.Value),
                new SqlParameter("@loginUserId", (object)loginUserId ?? DBNull.Value)
                ).ToListAsync();

            return data;
        }
        public async Task<List<Sp_BroughtBy_Job_ClientNamesModel>> GetBroughtByDayWiseJobClientNamesAsync(int boughtBy, int? puId, DateTime? FromDate, DateTime? ToDate, int loginUserType, int loginUserId)
        {
            var data = await this.Set<Sp_BroughtBy_Job_ClientNamesModel>().FromSqlRaw(
                "EXEC [dbo].[Sp_BroughtBy_daywiseJob_ClientNames] @boughtBy,@puId,@FromDate,@ToDate,@loginUserType,@loginUserId",

                new SqlParameter("@boughtBy", (object)boughtBy ?? DBNull.Value),
                new SqlParameter("@puId", (object)puId ?? DBNull.Value),
                new SqlParameter("@FromDate", (object)FromDate ?? DBNull.Value),
                new SqlParameter("@ToDate", (object)ToDate ?? DBNull.Value),
                new SqlParameter("@loginUserType", (object)loginUserType ?? DBNull.Value),
                new SqlParameter("@loginUserId", (object)loginUserId ?? DBNull.Value)
                ).ToListAsync();

            return data;
        }
        public async Task<List<Sp_BroughtBy_Job_ClientNamesModel>> GetBroughtByInterviewClientNamesAsync(int boughtBy, int loginUserType, int loginUserId)
        {
            var data = await this.Set<Sp_BroughtBy_Job_ClientNamesModel>().FromSqlRaw(
                "EXEC [dbo].[Sp_BroughtBy_Interview_ClientNames] @boughtBy,@loginUserType,@loginUserId",

                new SqlParameter("@boughtBy", (object)boughtBy ?? DBNull.Value),
                new SqlParameter("@loginUserType", (object)loginUserType ?? DBNull.Value),
                new SqlParameter("@loginUserId", (object)loginUserId ?? DBNull.Value)
                ).ToListAsync();

            return data;
        }



        public async Task<List<DashboardCandiateInterviewModel>> GetDashboardCandidateInterviewAsync(DateTime? fromDate, DateTime? toDate, int[] puIds, int[] buIds, int? tabId, int pageCount, int skipCount, int userType, int userId)
        {
            return await this.Set<DashboardCandiateInterviewModel>().FromSqlRaw("EXEC [dbo].[Sp_CandidateInterview] @fmDt, @toDt, @puIds, @buIds, @tabId, @fetchCount, @offsetCount, @userType, @userId",
                new SqlParameter("@fmDt", (object)fromDate ?? DBNull.Value),
                new SqlParameter("@toDt", (object)toDate ?? DBNull.Value),
                new SqlParameter("@puIds", puIds != null && puIds.Length > 0 ? string.Join(',', puIds) : ""),
                new SqlParameter("@buIds", buIds != null && buIds.Length > 0 ? string.Join(',', buIds) : ""),
                new SqlParameter("@tabId", (object)tabId ?? DBNull.Value),
                new SqlParameter("@fetchCount", pageCount), new SqlParameter("@offsetCount", skipCount),
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
        }


        public async Task<DashboardCountModel> GetDashboardCandidateInterviewCountAsync(DateTime? fromDate, DateTime? toDate, int[] puIds, int[] buIds, int? tabId, int userType, int userId)
        {
            var rslt = await this.Set<DashboardCountModel>().FromSqlRaw("EXEC [dbo].[Sp_CandidateInterviewCount] @fmDt, @toDt,@puIds,	@buIds,	@tabId,	@userType,	@userId",
                new SqlParameter("@fmDt", (object)fromDate ?? DBNull.Value),
                new SqlParameter("@toDt", (object)toDate ?? DBNull.Value),
                new SqlParameter("@puIds", puIds != null && puIds.Length > 0 ? string.Join(',', puIds) : ""),
                new SqlParameter("@buIds", buIds != null && buIds.Length > 0 ? string.Join(',', buIds) : ""),
                new SqlParameter("@tabId", (object)tabId ?? DBNull.Value),
                new SqlParameter("@userType", userType), new SqlParameter("@userId", userId)).ToListAsync();
            return rslt.FirstOrDefault();
        }



        public async Task<List<DashboardCandiateInterviewModel>> GetDashboardCandidateInterviewsAsync(string? searchKey, int? jobId, int? puId, int? bdmId, int? recId, int? tabId, DateTime? FromDate, DateTime? ToDate, int? clientId, int loginUserType, int loginUserId, int? pageCount, int? skipCount)
        {
            return await this.Set<DashboardCandiateInterviewModel>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_CandidateInterviews] @searchKey,@jobId,@puId,@bdmId,@recId,@tabId,@FromDate,@ToDate,@clientId,@loginUserType,@loginUserId,@fetchCount,@offsetCount",
                new SqlParameter("@searchKey", (object)searchKey ?? DBNull.Value),
                new SqlParameter("@jobId", (object)jobId ?? DBNull.Value),
                new SqlParameter("@puId", (object)puId ?? DBNull.Value),
                new SqlParameter("@bdmId", (object)bdmId ?? DBNull.Value),
                new SqlParameter("@recId", (object)recId ?? DBNull.Value),
                new SqlParameter("@tabId", (object)tabId ?? DBNull.Value),
                new SqlParameter("@FromDate", (object)FromDate ?? DBNull.Value),
                new SqlParameter("@ToDate", (object)ToDate ?? DBNull.Value),
                new SqlParameter("@clientId", (object)clientId ?? DBNull.Value),
                new SqlParameter("@loginUserType", (object)loginUserType ?? DBNull.Value),
                new SqlParameter("@loginUserId", (object)loginUserId ?? DBNull.Value),
                new SqlParameter("@fetchCount", pageCount),
                new SqlParameter("@offsetCount", skipCount)).ToListAsync();
        }


        public async Task<List<InterviewStageStatus>> GetDashboardCandidateInterviewsCountAsync(string? searchKey, int? jobId, int? puId, int? bdmId, int? recId, DateTime? FromDate, DateTime? ToDate, int? clientId, int loginUserType, int loginUserId)
        {
            return await this.Set<InterviewStageStatus>().FromSqlRaw("EXEC [dbo].[Sp_Dashboard_CandidateInterviewsCount] @searchKey,@jobId,@puId,@bdmId,@recId,@FromDate,@ToDate,@clientId,@loginUserType,@loginUserId",
                new SqlParameter("@searchKey", (object)searchKey ?? DBNull.Value),
                new SqlParameter("@jobId", (object)jobId ?? DBNull.Value),
                new SqlParameter("@puId", (object)puId ?? DBNull.Value),
                new SqlParameter("@bdmId", (object)bdmId ?? DBNull.Value),
                new SqlParameter("@recId", (object)recId ?? DBNull.Value),
                new SqlParameter("@FromDate", (object)FromDate ?? DBNull.Value),
                new SqlParameter("@ToDate", (object)ToDate ?? DBNull.Value),
                new SqlParameter("@clientId", (object)clientId ?? DBNull.Value),
                new SqlParameter("@loginUserType", (object)loginUserType ?? DBNull.Value),
                new SqlParameter("@loginUserId", (object)loginUserId ?? DBNull.Value)).ToListAsync();
        }



        public async Task<List<JobCandidatesBasedOnProfileStatusViewModel>> GetJobCandidatesBasedStatusAsync(string? searchKey, int jobId, string profileStatus, int? pageCount, int? skipCount)
        {
            return await this.Set<JobCandidatesBasedOnProfileStatusViewModel>().FromSqlRaw("EXEC [dbo].[Sp_Job_Candidates_Based_On_ProfileStatus] @searchKey,@jobId,@profileStatus,@fetchCount,@offsetCount",
                new SqlParameter("@searchKey", (object)searchKey ?? DBNull.Value),
                new SqlParameter("@jobId", (object)jobId ?? DBNull.Value),
                new SqlParameter("@profileStatus", (object)profileStatus ?? DBNull.Value),
                new SqlParameter("@fetchCount", pageCount),
                new SqlParameter("@offsetCount", skipCount)).ToListAsync();
        }


        public async Task<DashboardCountModel> GetJobCandidatesBasedStatusCountAsync(string? searchKey, int jobId, string profileStatus)
        {
            var rslt = await this.Set<DashboardCountModel>().FromSqlRaw("EXEC [dbo].[Sp_Job_Candidates_Based_On_ProfileStatusCount] @searchKey,@jobId,@profileStatus",
                new SqlParameter("@searchKey", (object)searchKey ?? DBNull.Value),
                new SqlParameter("@jobId", (object)jobId ?? DBNull.Value),
                new SqlParameter("@profileStatus", (object)profileStatus ?? DBNull.Value)).ToListAsync();
            return rslt?.FirstOrDefault();
        }



        public async Task<List<CandidateStageWiseViewModel>> GetCandidateStageWiseInfoAsync(int jobId, int candProfId)
        {
            var rslt = await this.Set<CandidateStageWiseViewModel>().FromSqlRaw("EXEC [dbo].[Sp_Candidate_StageWise_Info] @JobId,@CandProfId",
                new SqlParameter("@JobId", (object)jobId ?? DBNull.Value),
                new SqlParameter("@CandProfId", (object)candProfId ?? DBNull.Value)).ToListAsync();
            return rslt;
        }

        #endregion 


    }
}
