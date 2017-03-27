-- SetupConfig: {}
--
create procedure CK.sUserPasswordOnLogin
(
	@UserId int
)
as
begin
	--[beginsp]

	declare @Now datetime2(2) = sysutcdatetime(); 
	
	--<PreOnLogin revert /> 
	update CK.tUserPassword set LastLoginTime = @Now where UserId = @UserId;
	exec CK.sAuthUserOnLogin 'Basic', @Now, @UserId;
	--<PostOnLogin /> 

	--[endsp]
end