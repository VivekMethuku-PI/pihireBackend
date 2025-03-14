
CREATE OR ALTER PROCEDURE [dbo].[Sp_CandidateInterviewCount]
	@fmDt datetime,
	@toDt datetime,
	@puIds nvarchar(max), 
	@buIds nvarchar(max), 
	@tabId int,
	--Authorization
	@userType int,
	@userId int
AS
begin
	select 
		count(1) TotCnt
	from 
		vwDashboardCandidateInterview with (nolock)
	where 
		( 
			(@userType = 1) or --SuperAdmin
			(@userType = 2 and jobId in (select JOID from dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl with(nolock) inner join [dbo].[vwUserPuBu] vw on /*jbDtl.BUID=vw.[BusinessUnit] and*/ jbDtl.PUID=vw.[ProcessUnit] and vw.UserId=@userId)) or --Admin
			(@userType = 3 and @userId = bdmId) or --BDM
			(@userType = 4 and @userId = recruiterID) or --Recruiter
			(@userType = 5 and @userId = CandProfID) --Candidate
			--Hire manager PH_JOB_OPENINGS_ADDL_DETAILS
		)
		and (@fmDt is null or @toDt is null or InterviewDate between @fmDt and @toDt)
		and (LEN(coalesce(@puIds,''))=0 or (jobId in (select JOID from dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl with(nolock) where PUID in (select cast(value as int) from string_split(@puIds,',')))))
		and (LEN(coalesce(@buIds,''))=0 or (jobId in (select JOID from dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl with(nolock) where BUID in (select cast(value as int) from string_split(@buIds,',')))))
		and (@tabId is null or tabNo=@tabId)
end
GO

CREATE OR ALTER PROCEDURE [dbo].[Sp_CandidateInterview]
	@fmDt datetime,
	@toDt datetime,
	@puIds nvarchar(max), 
	@buIds nvarchar(max), 
	@tabId int,
	--pagination
	@fetchCount int,--0 if no pagination
	@offsetCount int,
	--Authorization
	@userType int,
	@userId int
AS
begin
if(@fetchCount != 0)
	select 
		vw.*,DC.[FileName] AS finalCVUrl,CAST(DATEDIFF(DAY, vw.CreatedDate, GETDATE()) AS VARCHAR(10)) AS TimeLine,CS.title AS CandProfStatusName

	from 
		vwDashboardCandidateInterview as vw with (nolock)
		CROSS APPLY (
		select top 1 docs.[FileName] from dbo.PH_CANDIDATE_DOCS docs where docs.DocType = 'Final CV' AND docs.JOID = JOID
		and docs.CandProfID = CandProfID ORDER BY ID DESC
		) DC
		JOIN [dbo].PH_CAND_STATUS_S AS CS WITH (NOLOCK) ON vw.CandProfStatus = CS.Id
	where 
		( 
			(@userType = 1) or --SuperAdmin
			(@userType = 2 and jobId in (select JOID from dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl with(nolock) inner join [dbo].[vwUserPuBu] vw on /*jbDtl.BUID=vw.[BusinessUnit] and*/ jbDtl.PUID=vw.[ProcessUnit] and vw.UserId=@userId)) or --Admin
			(@userType = 3 and @userId = bdmId) or --BDM
			(@userType = 4 and @userId = recruiterID) or --Recruiter
			(@userType = 5 and @userId = CandProfID) --Candidate
			--Hire manager PH_JOB_OPENINGS_ADDL_DETAILS
		)
		and (@fmDt is null or @toDt is null or InterviewDate between @fmDt and @toDt)
		and (LEN(coalesce(@puIds,''))=0 or (jobId in (select JOID from dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl with(nolock) where PUID in (select cast(value as int) from string_split(@puIds,',')))))
		and (LEN(coalesce(@buIds,''))=0 or (jobId in (select JOID from dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl with(nolock) where BUID in (select cast(value as int) from string_split(@buIds,',')))))
		and (@tabId is null or tabNo=@tabId)
	order by InterviewDate desc offset @offsetCount rows fetch next @fetchCount rows only;

else
	select 
		vw.*,DC.FileName AS finalCVUrl,CAST(DATEDIFF(DAY, vw.CreatedDate, GETDATE()) AS VARCHAR(10)) AS TimeLine,CS.title AS CandProfStatusName
	from 
		vwDashboardCandidateInterview as vw with (nolock)
		CROSS APPLY (
		select top 1 docs.FileName from dbo.PH_CANDIDATE_DOCS docs where docs.DocType = 'Final CV' AND docs.JOID = JOID
		and docs.CandProfID = CandProfID ORDER BY ID DESC
		) DC
		JOIN [dbo].PH_CAND_STATUS_S AS CS WITH (NOLOCK) ON vw.CandProfStatus = CS.Id
	where 
		( 
			(@userType = 1) or --SuperAdmin
			(@userType = 2 and jobId in (select JOID from dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl with(nolock) inner join [dbo].[vwUserPuBu] vw on /*jbDtl.BUID=vw.[BusinessUnit] and*/ jbDtl.PUID=vw.[ProcessUnit] and vw.UserId=@userId)) or --Admin
			(@userType = 3 and @userId = bdmId) or --BDM
			(@userType = 4 and @userId = recruiterID) or --Recruiter
			(@userType = 5 and @userId = CandProfID) --Candidate
			--Hire manager PH_JOB_OPENINGS_ADDL_DETAILS
		)
		and (@fmDt is null or @toDt is null or InterviewDate between @fmDt and @toDt)
		and (LEN(coalesce(@puIds,''))=0 or (jobId in (select JOID from dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl with(nolock) where PUID in (select cast(value as int) from string_split(@puIds,',')))))
		and (LEN(coalesce(@buIds,''))=0 or (jobId in (select JOID from dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl with(nolock) where BUID in (select cast(value as int) from string_split(@buIds,',')))))
		and (@tabId is null or tabNo=@tabId)
	order by InterviewDate desc
end

