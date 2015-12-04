-- Version = 1.1.1, Requires = { CK.sZoneUserAdd }
--
-- Add a User to a Group.
--

alter procedure CK.sGroupUserAdd 
(
	@ActorId int,
	@GroupId int,
	@UserId int
)
as begin
    if @ActorId <= 0 raiserror( 'Security.AnonymousNotAllowed', 16, 1 );
	if @GroupId <= 0 raiserror( 'Group.InvalidId', 16, 1 );

	-- System is not added to any group.
    if @UserId = 1 return 0;

	--[beginsp]

	if not exists (select * from CK.tActorProfile where GroupId = @GroupId and ActorId = @UserId)
	begin
		-- if the group is actually a Zone, calls sZoneUserAdd
		if exists( select * from CK.tZone where ZoneId = @GroupId )
		begin
			exec CK.sZoneUserAdd @ActorId, @GroupId, @UserId;
		end
		else
		begin 
			--<Extension Name="Group.PreUserAdd" >

			--From: Zone

			declare @ZoneId int;		
			-- The user must be registered in the Zone or the Group.ZoneId is 0 (this supports "unzoned" Groups).
			select @ZoneId = g.ZoneId
					from CK.tActorProfile a
					inner join CK.tGroup g on g.ZoneId = a.GroupId 
					where g.GroupId = @GroupId and (g.ZoneId = 0 or a.ActorId = @UserId);
		
			if @ZoneId is null 
			begin 
				raiserror('Group.UserNotInZone', 16, 1 );
			end
			--/From: Zone
					
			--</Extension>

			insert into CK.tActorProfile( ActorId, GroupId )  values( @UserId, @GroupId );

			--<Extension Name="Group.PostUserAdd" />
		end
	end
	--[endsp]
end
