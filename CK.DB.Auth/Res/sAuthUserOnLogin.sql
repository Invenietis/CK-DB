-- SetupConfig: { } 
-- 
create procedure CK.sAuthUserOnLogin
( 
	@Scheme varchar(64),
	@LoginTime datetime2(2),
	@UserId int,
    @FailureCode int output,
    @FailureReason varchar(128) output
)
as 
begin
	return 0;
end
