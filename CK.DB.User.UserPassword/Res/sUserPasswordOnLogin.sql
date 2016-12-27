-- SetupConfig: {}
--
create procedure CK.sUserPasswordOnLogin
(
	@UserId int /*input*/output
)
as
begin
	--[beginsp]

	--<PreOnLogin revert /> 

	update CK.tUserPassword set LastLoginTime = sysutcdatetime() where UserId = @UserId;

	--<PostOnLogin /> 

	--[endsp]
end