-- Version = 1.0.0
--
-- Destroys a Group: can work only if there is no Users inside the Group.
--
create procedure CK.sGroupDestroy
(
	@ActorId int,
	@GroupId int,
	@ForceDelete bit = 0
)
as begin
    if @ActorId <= 0 raiserror( 'Security.AnonymousNotAllowed', 16, 1 );
    if @GroupId <= 1 raiserror( 'Group.Undestroyable', 16, 1 );

	--[beginsp]
	
	if exists( select * from CK.tActorProfile where GroupId = @GroupId and ActorId <> @GroupId ) raiserror( 'Group.NotEmptyGroup', 16, 1 );

	--<Extension Name="Group.PreDestroy" />

	if @ForceDelete = 1
	begin
		exec CK.sGroupRemoveAllUsers @ActorId, @GroupId;
	end

	delete from CK.tActorProfile where GroupId = @GroupId;
	delete from CK.tGroup where GroupId = @GroupId;
	delete from CK.tActor where ActorId = @GroupId;

	--<Extension Name="Group.PostDestroy" />
		
	--[endsp]
end