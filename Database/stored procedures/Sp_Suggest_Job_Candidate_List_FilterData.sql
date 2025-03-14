USE [piHIRE1.0_QA]
GO

--Alter PROCEDURE [dbo].[Sp_Suggest_Job_Candidate_List]
CREATE OR ALTER procedure [dbo].[Sp_Suggest_Job_Candidate_List_FilterData]
	@JobId int,
	@UserType int,
	@UserId int
AS
begin

	declare @tbl table(id int)
	insert @tbl
	select distinct TechnologyID from [dbo].[PH_JOB_OPENING_SKILLS] where [Status] = 1 and JOID = @jobId;

	--declare @CanIds nvarchar(max);
	declare @skillCount int;
	select @skillCount=count(1) from @tbl

	--select @JobSKills= STRING_AGG(TechnologyID, ', ')  FROM [dbo].[PH_JOB_OPENING_SKILLS] where JoId = @JobId;
	--select @skillCount= Count(TechnologyID) FROM [dbo].[PH_JOB_OPENING_SKILLS] where JoId = @JobId;
	declare @tblJobCands table(id int)
	insert @tblJobCands
	--select distinct TechnologyID from [dbo].[PH_JOB_OPENING_SKILLS] where [Status] = 1 and JOID = @jobId;
	select CandProfID  FROM [dbo].[PH_JOB_CANDIDATES] where [Status] != 5 and JoId = @JobId;

	select 
		Distinct
		--RecruiterID, 		
		--SelfRating,
		--Gender,
		--SourceID,
		CANDIDATE_PROFILE.CountryID,
		CANDIDATE_PROFILE.Nationality,
		CandProfStatus,
		OpCurrency		
		--NoticePeriod,
		--OpTakeHomePerMonth,
		--datediff(year, DOB, getdate()) age,
		--MaritalStatus
	from 
		dbo.PH_CANDIDATE_PROFILES as CANDIDATE_PROFILE 
		join (
			select * from (
				select *, row_number() over (
					partition by CandProfID
					order by CreatedDate desc
				) as row_num
				from dbo.PH_JOB_CANDIDATES where CandProfID not in (SELECT id from @tblJobCands)
			) as ordered_widgets
			where ordered_widgets.row_num = 1
		) as JOB_CANDIDATES on CANDIDATE_PROFILE.ID = JOB_CANDIDATES.CandProfID
		--join PH_CAND_STATUS_S as CAND_STATUS_S  WITH(NOLOCK) on JOB_CANDIDATES.CandProfStatus  = CAND_STATUS_S.Id

	where  1 = 1
	 --CANDIDATE_PROFILE.ID in (
		--Select machSk.CandProfID
		--from
		--	(
		--			select Count(Skill.TechnologyId) as counts, skill.CandProfID 
		--			from [dbo].[PH_CANDIDATE_SKILLSET] as Skill  WITH(NOLOCK) 
		--			where skill.TechnologyId in (SELECT value from string_split(@JobSKills, ',')) 
		--			GROUP BY skill.CandProfID
		--	) machSk
		--where  
		--	machSk.counts = @skillCount
		--)
		and
		(case when @skillCount = 0 then 0 else 
				(select count(1)/@skillCount from [dbo].[PH_CANDIDATE_SKILLSET] as candSkill  WITH(NOLOCK) where candSkill.[Status] = 1 and candSkill.TechnologyID in (select id from @tbl))
			end) > 0.7
end
