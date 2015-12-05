-- Version = 15.12.5
--
-- Destroys a Group: can work only if there is no Users inside the Group.
--
create procedure CK.sGroupDestroy
(
	@ActorId int,
	@GroupId int,
	@ForceDestroy bit = 0
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @GroupId <= 1 throw 50000, 'Group.Undestroyable', 1;

	--[beginsp]
	
	--<Extension Name="Group.PreDestroy" />

	if @ForceDestroy = 1
	begin
		exec CK.sGroupRemoveAllUsers @ActorId, @GroupId;
	end
	else
	begin
		if exists( select * from CK.tActorProfile where GroupId = @GroupId and ActorId <> @GroupId ) throw 50000, 'Group.NotEmptyGroup', 1;
	end
	delete from CK.tActorProfile where GroupId = @GroupId;
	delete from CK.tGroup where GroupId = @GroupId;
	delete from CK.tActor where ActorId = @GroupId;

	--<Extension Name="Group.PostDestroy" />
		
	--[endsp]
end