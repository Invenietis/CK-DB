-- SetupConfig: { "Requires": "CK.vUserAuthProvider" } 
-- 
create procedure CK.sAuthUserInfoRead
( 
	@ActorId int,
	@UserId int
)
as 
begin
	select UserId, UserName from CK.tUser with(nolock) where UserId = @UserId;
	select ProviderName, LastUsed 
		from CK.vUserAuthProvider with(nolock) 
		where UserId = @UserId and LastUsed > '0001-01-01 00:00:00'
		order by LastUsed desc;
	return 0;
end
