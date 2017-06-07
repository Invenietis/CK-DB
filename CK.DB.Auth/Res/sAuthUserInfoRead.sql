-- SetupConfig: { "Requires": "CK.vUserAuthProvider" } 
-- 
create procedure CK.sAuthUserInfoRead
( 
	@ActorId int,
	@UserId int
)
as 
begin
	select UserId, UserName from CK.tUser with(nolock) where UserId = @UserId and @UserId > 0;
	select Scheme, LastUsed 
		from CK.vUserAuthProvider with(nolock) 
		where UserId = @UserId and LastUsed > '0001-01-01'
		order by LastUsed desc;
	return 0;
end
