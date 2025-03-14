CREATE OR ALTER PROCEDURE [dbo].[Sp_Assignment_Jobs_List_Count]
	@FilterKey nvarchar(max),
	@puId int,
	@bdmId int,
	@JobPriority int,
	@FromDate Datetime,
	@ToDate Datetime,
	@clientId int,
	@jobStatusId int,
	@assignmentStatus bit,

	--Authorization
	@loginUserType int,
	@loginUserId int
AS
BEGIN   
	SET NOCOUNT ON 

	SELECT Job.ID,Job.ClosedDate,Job.CreatedDate from PH_JOB_OPENINGS as Job  
			JOIN  dbo.PH_JOB_OPENINGS_ADDL_DETAILS as jobAddl  on Job.Id = jobAddl.JOID 
			LEFT JOIN dbo.PH_COUNTRY as Cuntry  on Job.CountryID = Cuntry.Id 
			LEFT JOIN dbo.PH_CITY as city  on Job.JobLocationID = city.Id 
			LEFT JOIN dbo.PH_JOB_STATUS_S as JobStatus  on Job.JobOpeningStatus = JobStatus.Id 
			LEFT JOIN dbo.PI_HIRE_USERS as HireUser on Job.CreatedBy = HireUser.UserId 
			LEFT OUTER JOIN dbo.Ph_Job_Opening_Actv_Counter as Counter on Job.Id = Counter.JOID and Counter.Status !=5
		
	Where 
		--Authorization
		( 
			(@loginUserType = 1) or --SuperAdmin
			(@loginUserType = 2 and Job.ID in (select JOID from [dbo].[PH_JOB_OPENINGS_ADDL_DETAILS] jbDtl with(nolock) inner join [dbo].[vwUserPuBu] vw with(nolock) on /*jbDtl.BUID=vw.[BusinessUnit] and*/ jbDtl.PUID=vw.[ProcessUnit] and vw.UserId=@loginUserId)) or --Admin
			(@loginUserType = 3 and @loginUserId = coalesce(Job.BroughtBy,Job.CreatedBy)) or --BDM
			(@loginUserType = 4 and Job.ID in (select asgn.JOID from dbo.PH_JOB_ASSIGNMENTS asgn where asgn.AssignedTo = @loginUserId and asgn.DeassignDate is null)) or --Recruiter
			--Candidate 5
			(@loginUserType > 4 and 1 = 0)
			--Hire manager [dbo].[PH_JOB_OPENINGS_ADDL_DETAILS]
		)	AND
		JobStatus.JSCode !='CLS' and 

		(@FilterKey is null or (Job.Id like '%'+@FilterKey+'%' or Job.JobTitle like '%'+@FilterKey+'%' or Job.JobRole like '%'+@FilterKey+'%')) and  

		(@puId is null or (jobAddl.PUID = @puId)) and 
		(@bdmId is null or (coalesce(Job.BroughtBy, Job.CreatedBy) = @bdmId))  and
		(@JobPriority is null or Job.[Priority] = @JobPriority) and 
		(@FromDate is null or (Job.CreatedDate >= @FromDate and Job.CreatedDate <= @ToDate)) and
		(@clientId is null or Job.[ClientID] = @clientId) and 
		(@jobStatusId is null or Job.[JobOpeningStatus] = @jobStatusId) and 
		(@assignmentStatus is null or 
			(@assignmentStatus = 0 and not exists(select 1 from dbo.PH_JOB_ASSIGNMENTS asg where asg.Status =1 and asg.DeassignDate is null and asg.JOID=Job.ID)) or
			(@assignmentStatus = 1 and exists(select 1 from dbo.PH_JOB_ASSIGNMENTS asg where asg.Status =1 and asg.DeassignDate is null and asg.JOID=Job.ID))
		)

END
Go
CREATE OR ALTER PROCEDURE [dbo].[Sp_Assignment_Jobs_List]

	@FilterKey nvarchar(max),
	@puId int,
	@bdmId int,
	@JobPriority int,
	@FromDate Datetime,
	@ToDate Datetime,
	@PerPage int,
	@CurrentPage int,
	@clientId int,
	@jobStatusId int,
	@assignmentStatus bit,

	--Authorization
	@loginUserType int,
	@loginUserId int
AS
BEGIN   

	DECLARE @false bit = 0;

	SET NOCOUNT ON 

	SELECT
		Job.JobLocationID,
		Job.Id AS Id,
		city.id AS CityId,
		dbo.fn_titlecase(city.name) AS CityName,
		Cuntry.nicename AS CountryName,
		CountryID,
		Job.ClientId,
		Job.ClientName,
		Job.ClosedDate,
		Job.JobRole,
		Job.JobTitle,
		Job.JobDescription,
		Job.PostedDate AS StartDate,
		Job.JobOpeningStatus,
		JobStatus.Title AS JobOpeningStatusName,
		JobStatus.JSCode,
		Job.status,
		Job.MaxExpeInMonths / 12 AS MaxExp,
		Job.MinExpeInMonths / 12 AS MinExp,
		Job.CreatedDate,
		Job.CreatedBy,
		HireUser.FirstName AS CreatedByName,
		Job.ShortJobDesc,		
		Counter.AsmtCounter,
		Counter.JobPostingCounter,
		Counter.ClientViewsCounter,
		Counter.EmailsCounter,
		(SELECT COUNT(id) FROM dbo.Ph_Candidate_Profiles_Shared cps WHERE cps.Joid = Job.id) AS ProfilesSharedToClientCounter,
		ACTIVITY_LOG.ModificationOn,
		ACTIVITY_LOG.ModificationBy,
		(SELECT COUNT(id) FROM dbo.PH_JOB_ASSIGNMENTS JA with (nolock) WHERE JA.Joid = Job.id and JA.DeassignDate is null) AS Assinged,
		Job.PRIORITY AS [Priority],
		Ref.RMValue As PriorityName,

		CAST(DATEDIFF(DAY, CASE
         WHEN Job.ReopenedDate is null THEN Job.CreatedDate
         ELSE Job.ReopenedDate END, GETDATE()) AS VARCHAR(10)) AS Age,
		jobAddl.NoOfCvsRequired,
		(select SUM(coalesce(NoOfFinalCVsFilled,0)) from [dbo].[PH_JOB_ASSIGNMENTS] jobRecr with (nolock)
		where Status!=5 and jobRecr.JOID=job.ID) AS NoOfCvsFullfilled,

		@false AS Assign,
		@false AS PriorityUpdate,
		@false AS Note,
		@false AS Interviews,
		@false AS JobStatus,
		@false AS CandStatus


	FROM
		dbo.PH_JOB_OPENINGS AS Job  with (nolock)
		JOIN
			dbo.PH_JOB_OPENINGS_ADDL_DETAILS AS jobAddl  with (nolock) ON Job.Id = jobAddl.JOID
		LEFT JOIN
			dbo.PH_REF_MASTER_S AS Ref  with (nolock) ON Job.[Priority] = Ref.Id
		LEFT JOIN
			dbo.PH_COUNTRY AS Cuntry  with (nolock) ON Job.CountryID = Cuntry.Id
		LEFT JOIN
			dbo.PH_CITY AS city  with (nolock) ON Job.JobLocationID = city.Id
		LEFT JOIN
			dbo.PH_JOB_STATUS_S AS JobStatus  with (nolock) ON Job.JobOpeningStatus = JobStatus.Id
		LEFT JOIN
			dbo.PI_HIRE_USERS AS HireUser  with (nolock) ON Job.CreatedBy = HireUser.id and HireUser.UserType !=5 -- candidate
		LEFT OUTER JOIN
			dbo.Ph_Job_Opening_Actv_Counter AS [Counter]  with (nolock) ON Job.Id = [Counter].JOID AND [Counter].Status != 5
		CROSS APPLY (
			SELECT TOP 1
				Logs.CreatedDate AS ModificationOn,CONCAT(HireUser.FirstName , HireUser.LastName) AS ModificationBy
			FROM
				dbo.PH_ACTIVITY_LOG AS Logs with (nolock)
			JOIN
				dbo.PI_HIRE_USERS AS HireUser with (nolock) ON Logs.Createdby = HireUser.Id and UserType in (1,2,3)
			WHERE
				ActivityMode = 2
				AND ActivityType IN (7, 2)
				AND Logs.JoId = Job.Id
			ORDER BY
				Logs.id DESC
		) ACTIVITY_LOG
	Where 
		--Authorization
		( 
			(@loginUserType = 1) or --SuperAdmin
			(@loginUserType = 2 and Job.ID in (select JOID from [dbo].[PH_JOB_OPENINGS_ADDL_DETAILS] jbDtl with(nolock) inner join [dbo].[vwUserPuBu] vw with(nolock) on /*jbDtl.BUID=vw.[BusinessUnit] and*/ jbDtl.PUID=vw.[ProcessUnit] and vw.UserId=@loginUserId)) or --Admin
			(@loginUserType = 3 and @loginUserId = coalesce(Job.BroughtBy,Job.CreatedBy)) or --BDM
			(@loginUserType = 4 and Job.ID in (select asgn.JOID from dbo.PH_JOB_ASSIGNMENTS asgn with(nolock) where asgn.AssignedTo = @loginUserId and asgn.DeassignDate is null)) or --Recruiter
			--Candidate 5
			(@loginUserType > 4 and 1 = 0)
			--Hire manager [dbo].[PH_JOB_OPENINGS_ADDL_DETAILS]
		)	AND
		JobStatus.JSCode !='CLS'  AND

			(@FilterKey is null or (Job.Id like '%'+@FilterKey+'%' or Job.JobTitle like '%'+@FilterKey+'%' or Job.JobRole like '%'+@FilterKey+'%')) and  
		
			(@puId is null or (jobAddl.PUID = @puId)) and 
			(@bdmId is null or (coalesce(Job.BroughtBy, Job.CreatedBy) = @bdmId)) and 
			(@JobPriority is null or Job.[Priority] = @JobPriority) and 
			(@FromDate is null or (Job.CreatedDate >= @FromDate and Job.CreatedDate <= @ToDate)) and
			(@clientId is null or Job.[ClientID] = @clientId) and 
			(@jobStatusId is null or Job.[JobOpeningStatus] = @jobStatusId) and 
			(@assignmentStatus is null or 
				(@assignmentStatus = 0 and not exists(select 1 from dbo.PH_JOB_ASSIGNMENTS asg where asg.Status =1 and asg.DeassignDate is null and asg.JOID=Job.ID)) or
				(@assignmentStatus = 1 and exists(select 1 from dbo.PH_JOB_ASSIGNMENTS asg where asg.Status =1 and asg.DeassignDate is null and asg.JOID=Job.ID))
			)

			ORDER by Job.CreatedDate desc offset @CurrentPage rows fetch next @PerPage rows only


END



