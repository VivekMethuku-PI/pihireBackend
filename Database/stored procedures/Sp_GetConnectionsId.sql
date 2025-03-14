USE [piHIRE1.0_DEV]
GO


ALTER PROCEDURE [dbo].[Sp_GetConnectionsId]
(
	@UserId nvarchar(500)
)

AS
BEGIN

	WITH temp_cte AS (
		SELECT m.DeviceUID, ROW_NUMBER() OVER (PARTITION BY UserID ORDER BY id DESC) AS rn
		FROM PI_USER_TXN_LOG AS m
		WHERE (m.UserId in (
			SELECT l.UserId 
			FROM PI_USER_LOG l
			WHERE (@UserId is null or LEN(@UserId)=0 or l.UserId in (SELECT CAST(value as int) as val FROM dbo.SplitString(@UserId, ','))) and l.LoginStatus=1
		)) and m.TxnOutDate is null
	)
	SELECT DeviceUID FROM temp_cte --WHERE rn = 1;
	

END