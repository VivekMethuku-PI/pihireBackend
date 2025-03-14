
CREATE OR ALTER PROCEDURE [dbo].[Sp_Dashboard_CandidateInterviewsCount]
	@searchKey nvarchar(200), 
	@jobid int,
	@puId int, 	

	@bdmId int,
	@recId int,
	@FromDate datetime,
	@ToDate datetime,
	@clientId int,

	--Authorization
	@loginUserType int,
	@loginUserId int

AS
BEGIN
	SELECT 
		count(1) TotCnt,tabNo
	FROM 
		vwDashboardCandidateInterview AS vw WITH (nolock)
	WHERE 
		--Authorization
		( 
			(@loginUserType = 1) or --SuperAdmin
			(@loginUserType = 2 and vw.JobId in (select JOID from [dbo].[PH_JOB_OPENINGS_ADDL_DETAILS] jbDtl with(nolock) inner join [dbo].[vwUserPuBu] vw with(nolock) on /*jbDtl.BUID=vw.[BusinessUnit] and*/ jbDtl.PUID=vw.[ProcessUnit] and vw.UserId=@loginUserId)) or --Admin
			(@loginUserType = 3 and @loginUserId = vw.bdmId) or --BDM
			(@loginUserType = 4 and vw.JobId in (select asgn.JOID from dbo.PH_JOB_ASSIGNMENTS asgn where asgn.AssignedTo = @loginUserId and asgn.DeassignDate is null)) or --Recruiter
			--Candidate 5
			(@loginUserType > 4 and 1 = 0)
			--Hire manager [dbo].[PH_JOB_OPENINGS_ADDL_DETAILS]
		)		
		AND (@searchKey IS NULL OR (
                CandProfID LIKE '%' + @SearchKey + '%' 
                OR ContactNo LIKE '%' + @SearchKey + '%' 
                OR EmailID LIKE '%' + @SearchKey  + '%' 
                OR LOWER(CandName) LIKE '%' + LOWER(@SearchKey) + '%' 
            ))
		AND (@jobid IS NULL OR JobId = @jobid) 
		AND (@bdmId IS NULL OR bdmId = @bdmId) 
		AND (@recId IS NULL OR recruiterID = @recId) AND
		(@FromDate is null or (CreatedDate >= @FromDate and CreatedDate <= @ToDate))
		AND (@PuId IS NULL OR jobId in (SELECT JOID FROM dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl WITH(nolock) WHERE PUID =@PuId))
		AND (@clientId is null or vw.ClientID=@clientId)
	Group By 
		tabNo 
END
Go

CREATE OR ALTER PROCEDURE [dbo].[Sp_Dashboard_CandidateInterviews]
	@searchKey nvarchar(200), 
	@jobid int,
	@puId int, 	

	@bdmId int,
	@recId int,
	@tabId int,
	@FromDate datetime,
	@ToDate datetime,
	@clientId int,

	--Authorization
	@loginUserType int,
	@loginUserId int,

	--pagination
	@fetchCount int,--0 if no pagination
	@offsetCount int
AS
BEGIN

	SELECT 
		vw.*,DC.[FileName] AS finalCVUrl,CAST(DATEDIFF(DAY, vw.CreatedDate, GETDATE()) AS VARCHAR(10)) AS TimeLine,CS.title AS CandProfStatusName
	FROM 
		vwDashboardCandidateInterview as vw with (nolock)
		CROSS APPLY (
			select top 1 docs.[FileName] from dbo.PH_CANDIDATE_DOCS docs where docs.DocType = 'Final CV' AND docs.JOID = JOID
			and docs.CandProfID = CandProfID ORDER BY ID DESC
		) DC
		JOIN [dbo].PH_CAND_STATUS_S AS CS WITH (NOLOCK) ON vw.CandProfStatus = CS.Id
		
	WHERE 
		--Authorization
		( 
			(@loginUserType = 1) or --SuperAdmin
			(@loginUserType = 2 and vw.JobId in (select JOID from [dbo].[PH_JOB_OPENINGS_ADDL_DETAILS] jbDtl with(nolock) inner join [dbo].[vwUserPuBu] vw with(nolock) on /*jbDtl.BUID=vw.[BusinessUnit] and*/ jbDtl.PUID=vw.[ProcessUnit] and vw.UserId=@loginUserId)) or --Admin
			(@loginUserType = 3 and @loginUserId = vw.bdmId) or --BDM
			(@loginUserType = 4 and vw.JobId in (select asgn.JOID from dbo.PH_JOB_ASSIGNMENTS asgn where asgn.AssignedTo = @loginUserId and asgn.DeassignDate is null)) or --Recruiter
			--Candidate 5
			(@loginUserType > 4 and 1 = 0)
			--Hire manager [dbo].[PH_JOB_OPENINGS_ADDL_DETAILS]
		)
		
		 AND (@searchKey IS NULL OR (
                CandProfID LIKE '%' + @SearchKey + '%' 
                OR ContactNo LIKE '%' + @SearchKey + '%' 
                OR EmailID LIKE '%' + @SearchKey  + '%' 
                OR LOWER(CandName) LIKE '%' + LOWER(@SearchKey) + '%' 
            ))
	    AND (@jobid IS NULL OR JobId = @jobid) 
		AND (@bdmId IS NULL OR bdmId = @bdmId) 
		AND (@recId IS NULL OR recruiterID = @recId)
		AND	(@FromDate is null or (vw.CreatedDate >= @FromDate and vw.CreatedDate <= @ToDate))
		AND (@PuId IS NULL OR jobId in (select JOID from dbo.PH_JOB_OPENINGS_ADDL_DETAILS jbDtl with(nolock) where PUID =@PuId))
		AND (@tabId is null or tabNo=@tabId)
		AND (@clientId is null or vw.ClientID=@clientId)
		
	ORDER BY InterviewDate DESC offset @offsetCount ROWS FETCH NEXT @fetchCount ROWS only;

END

