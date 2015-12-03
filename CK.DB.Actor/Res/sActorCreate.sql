-- Version = 1.0.0
create procedure CK.sActorCreate 
(
	@ActorId int,
	@ActorIdResult int output
)
as
begin
	--[beginsp]

	insert into CK.tActor default values;
	set @ActorIdResult = SCOPE_IDENTITY();

	-- The actor is in its own group.
	insert into CK.tActorProfile( ActorId, GroupId ) values( @ActorIdResult, @ActorIdResult );

	--[endsp]
end

