-- Version = 15.12.5
--
-- Creates a Group.
--
create procedure CK.sGroupCreate 
(
	@ActorId int,
	@GroupIdResult int output
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]
	
	--<Extension Name="Group.PreCreate" />

	exec CK.sActorCreate @ActorId, @GroupIdResult output;
	insert into CK.tGroup( GroupId ) values( @GroupIdResult );

	--<Extension Name="Group.PostCreate" />

	--[endsp]
end