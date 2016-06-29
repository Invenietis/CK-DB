-- SetupConfig: {}
create procedure CK.sActorCreate 
(
	@ActorId int,
	@ActorIdResult int output
)
as
begin
	--[beginsp]

	--<PreCreate revert />

	insert into CK.tActor default values;
	set @ActorIdResult = SCOPE_IDENTITY();

	-- The actor is in its own group.
	insert into CK.tActorProfile( ActorId, GroupId ) values( @ActorIdResult, @ActorIdResult );

	--<PostCreate />

	--[endsp]
end

