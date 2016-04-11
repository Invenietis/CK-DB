-- Version = *
--
-- Creates a Group.
--
create procedure CK.sGroupCreate 
(
	@ActorId int,
	@ZoneId int = 0,
	@GroupIdResult int output
)
as 
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @ZoneId = 1 throw 50000, 'Zone.SystemZoneHasNoGroup', 1;

	--[beginsp]
	
	--<Extension Name="Group.PreCreate" />

	exec CK.sActorCreate @ActorId, @GroupIdResult output;
	insert into CK.tGroup( GroupId, ZoneId ) values( @GroupIdResult, @ZoneId );

	--<Extension Name="Group.PostCreate" />

	--[endsp]
end