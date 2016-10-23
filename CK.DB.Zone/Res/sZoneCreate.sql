-- SetupConfig: { "Requires": [ "CK.sGroupCreate" ] }
create procedure CK.sZoneCreate 
(
	@ActorId int,
	@ZoneIdResult int output
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	--<PreZoneCreate revert />

	-- A Zone is a Group whose ZoneId is 0. Whenever Zone become hierarchical, the Group.ZoneId 
	-- of a Zone must be the parent zone.
	exec CK.sGroupCreate @ActorId, @ZoneIdResult output, @ZoneId = 0;
		
	-- Do create the Zone
	insert into CK.tZone( ZoneId ) values ( @ZoneIdResult );
			
	--<PostZoneCreate />

	--[endsp]
end

