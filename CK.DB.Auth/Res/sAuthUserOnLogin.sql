-- SetupConfig: { } 
-- 
create procedure CK.sAuthUserOnLogin
( 
	@ProviderName varchar(64),
	@LoginTime datetime2(2),
	@UserId int
)
as 
begin
	return 0;
end
