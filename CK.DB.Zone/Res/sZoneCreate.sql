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

	-- A Zone is Group.  
	exec CK.sGroupCreate @ActorId, 0, @ZoneIdResult output;
		
	-- Do create the Zone
	insert into CK.tZone( ZoneId ) values ( @ZoneIdResult );

	-- The Zone of this group is the Zone itself.
	update CK.tGroup set ZoneId = @ZoneIdResult where GroupId = @ZoneIdResult;
		
	--<PostZoneCreate />

	--[endsp]
end

