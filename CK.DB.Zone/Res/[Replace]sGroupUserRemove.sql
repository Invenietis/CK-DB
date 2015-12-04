-- Version = 1.0.2, Requires = { CK.sZoneUserRemove }
--
-- Remove a User from a Group.
--
create procedure CK.sGroupUserRemove
(
	@ActorId int,
	@GroupId int,
	@UserId int
)
as begin
    if @ActorId <= 0 raiserror( 'Security.AnonymousNotAllowed', 16, 1 );
	if @GroupId <= 0 raiserror( 'Group.InvalidId', 16, 1 );

	--[beginsp]

	-- if the group is actually a Zone, calls sZoneUserRemove
	if exists( select * from CK.tZone where ZoneId = @GroupId )
	begin
		exec CK.sZoneUserRemove @ActorId, @GroupId, @UserId;
	end
	else
	begin
		if @GroupId <> @UserId and exists (select * from CK.tActorProfile where GroupId = @GroupId and ActorId = @UserId)
		begin

			--<Extension Name="Group.PreUserRemove">

			delete from CK.tActorProfile where GroupId = @GroupId and ActorId = @UserId;
		
			--<Extension Name="Group.PostUserRemove" />
		end
	end
	--[endsp]
end
