-- Version = *, Requires={ CK.sUserRemoveFromAllGroups }
--
-- Destroys a User: automatically removes it from any Groups it may belong to.
--
create procedure CK.sUserDestroy
(
	@ActorId int,
	@UserId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @UserId <= 1 throw 50000, 'User.Undestroyable', 1;

	--[beginsp]

	if exists( select * from CK.tUser where UserId = @UserId )
	begin
		--<PreDestroy/>

		exec CK.sUserRemoveFromAllGroups @ActorId, @UserId;

		delete from CK.tActorProfile where ActorId = @UserId;
		delete from CK.tUser where UserId = @UserId;
		delete from CK.tActor where ActorId = @UserId;

		--<PostDestroy revert />

	end

	--[endsp]
end