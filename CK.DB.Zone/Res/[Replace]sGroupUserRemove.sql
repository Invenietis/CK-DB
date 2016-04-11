-- Version = *, Requires = { CK.sZoneUserRemove }
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
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
	if @GroupId <= 0 throw 50000, 'Group.InvalidId', 1;

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
