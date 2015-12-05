-- Version = 15.12.5, Requires = { CK.sActorCreate }
create procedure CK.sUserCreate 
(
	@ActorId int,
	@UserName nvarchar( 127 ),
	@UserIdResult int output
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]
	set @UserIdResult = 0;
	if exists( select UserId from CK.tUser where UserName = @UserName )
	begin
		set @UserIdResult = -1;
	end
	if @UserIdResult = 0
	begin
		--<Extension Name="User.PreCreate" />

		exec CK.sActorCreate @ActorId, @UserIdResult output;
		insert into CK.tUser( UserId, UserName ) values ( @UserIdResult, @UserName );

		--<Extension Name="User.PostCreate" />
	end
	--[endsp]
end

