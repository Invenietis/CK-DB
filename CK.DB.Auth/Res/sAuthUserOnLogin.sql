-- SetupConfig: { } 
-- 
create procedure CK.sAuthUserOnLogin
( 
	@Scheme varchar(64),
	@LoginTime datetime2(2),
	@UserId int
)
as 
begin
	return 0;
end
