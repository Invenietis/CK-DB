-- Version = 1.0.0, Requires={ CK.sUserRemoveFromAllGroups }
--
-- Destroys a User: automatically removes it from any Groups it may belong to.
--
create procedure CK.sUserDestroy
(
	@ActorId int,
	@UserId int
)
as begin
    if @ActorId <= 0 raiserror( 'Security.AnonymousNotAllowed', 16, 1 );
    if @UserId <= 1 raiserror( 'User.Undestroyable', 16, 1 );

	--[beginsp]

	if exists( select * from CK.tUser where UserId = @UserId )
	begin
		--<Extension Name="User.PreDestroy" />

		exec CK.sUserRemoveFromAllGroups @ActorId, @UserId;

		delete from CK.tActorProfile where ActorId = @UserId;
		delete from CK.tUser where UserId = @UserId;
		delete from CK.tActor where ActorId = @UserId;

		--<Extension Name="User.PostDestroy" />

	end
	--[endsp]
end