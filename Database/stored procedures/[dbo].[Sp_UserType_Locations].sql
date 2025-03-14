CREATE OR ALTER PROCEDURE [dbo].[Sp_UserType_Locations]
	@UserType tinyint
AS
BEGIN
	SELECT 
		loc.id,
		--loc.pu_id,
		--cm.id as company_id,
		--cm.pu_name as company_name,		
		cty.name as city_name,
		cntry.name as country_name,
		loc.[State],
		loc.location_name
		--cm.logo AS PuLogo
	FROM 
		[dbo].[tbl_param_pu_office_locations] as loc 
		--JOIN [dbo].[tbl_param_process_unit_master] as cm on loc.pu_id = cm.Id
		LEFT JOIN [dbo].[ph_country] as cntry on loc.country = cntry.Id
		LEFT JOIN [dbo].[ph_city] as cty on loc.city = cty.Id
	WHERE 
		(@UserType is null or loc.ID in (select LocationID from dbo.PI_HIRE_USERS where UserType = @UserType and LocationID > 0))
END


