-- Version = *
--
-- Adds a User to a Zone.
--
alter procedure CK.sZoneUserAdd 
(
	@ActorId int,
	@ZoneId int,
	@UserId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @ZoneId <= 0 throw 50000, 'Zone.InvalidId', 1;
	-- ..and if the ZoneId is actually a Group, this is an error.
	if not exists (select * from CK.tZone where ZoneId = @ZoneId) throw 50000, 'Zone.InvalidId', 1;

	-- System is, somehow, already in all groups.
    if @UserId = 1 return 0;

	--[beginsp]


	-- The user must not be already in the zone...
	if @ZoneId <> @UserId and not exists (select * from CK.tActorProfile where GroupId = @ZoneId and ActorId = @UserId)
	begin
		-- ...and if this is the System Zone, only members of it can add Users.
		if @ZoneId = 1 
		begin
			if not exists( select 1 from CK.tActorProfile p where p.GroupId = 1 and p.ActorId = @ActorId ) 
			begin
				;throw 50000, 'Security.ActorMustBeSytem', 1;
			end
		end

		--<Extension Name="Zone.PreUserAdd" />

		insert into CK.tActorProfile( ActorId, GroupId ) values( @UserId, @ZoneId );

		--<Extension Name="Zone.PostUserAdd" />
	end

	--[endsp]
end