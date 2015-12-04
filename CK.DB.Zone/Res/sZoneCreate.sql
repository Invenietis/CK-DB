-- Version = 1.0.0, Requires = { CK.sGroupUserAdd, CK.sZoneUserAdd }
create procedure CK.sZoneCreate 
(
	@ActorId int,
	@ZoneIdResult int output
)
as
begin
    if @ActorId <= 0 raiserror( 'Security.AnonymousNotAllowed', 16, 1 );

	--[beginsp]

	--<Extension Name="Zone.PreZoneCreate" />

	-- A Zone is Group.  
	exec CK.sGroupCreate @ActorId, 0, @ZoneIdResult output;
		
	-- Do create the Zone
	insert into CK.tZone( 
			ZoneId, AdministratorsGroupId 
		) values ( 
			@ZoneIdResult, 1 
		);

	-- The Zone of this group is the Zone itself.
	update CK.tGroup set ZoneId = @ZoneIdResult where GroupId = @ZoneIdResult;

	-- Creates the first group of the Zone. This group is the Administrators Group of the Zone.
	declare @AdministratorsGroupId int;
	exec CK.sGroupCreate @ActorId, @ZoneIdResult, @AdministratorsGroupId output;
		
	update CK.tZone set AdministratorsGroupId = @AdministratorsGroupId where ZoneId = @ZoneIdResult;

	if @ActorId > 1
	begin
		declare @Done bit;
		-- The current actor becomes a member of the newly created Zone.
		exec CK.sZoneUserAdd @ActorId, @ZoneIdResult, @ActorId, @Done output;
		if @Done = 0 raiserror( 'Zone.UnableToAddActorIdInTheCreatedZone', 16, 1 );
		
		-- The current actor becomes an administrator of the newly created Zone.
		exec CK.sGroupUserAdd @ActorId, @AdministratorsGroupId, @ActorId, @Done output;
		if @Done = 0 raiserror( 'Zone.UnableToAddActorIdInAdministratorsGroup', 16, 1 );
	end

	--<Extension Name="Zone.PostZoneCreate" />

	--[endsp]
end

