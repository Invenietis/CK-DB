-- SetupConfig: {}
--
create procedure CK.sUserPasswordOnLogin
(
	@UserId int,
    @FailureCode int output, -- Optional. Set by CK.sAuthUserOnLogin if login is rejected.
    @FailureReason varchar(128) output -- Optional. Set by CK.sAuthUserOnLogin if login is rejected.
)
as
begin
	--[beginsp]

	declare @Now datetime2(2) = sysutcdatetime(); 
	
	--<PreOnLogin revert /> 
	update CK.tUserPassword set LastLoginTime = @Now where UserId = @UserId;
	exec CK.sAuthUserOnLogin 'Basic', @Now, @UserId, @FailureCode output, @FailureReason output;
	--<PostOnLogin /> 

	--[endsp]
end
