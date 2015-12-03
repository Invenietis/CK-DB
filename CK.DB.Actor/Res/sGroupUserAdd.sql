-- Version = 1.0.3
--
-- Add a User to a Group. Does nothing if the User is already in the Group.
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

	if @GroupId <> @UserId and not exists (select * from CK.tActorProfile where GroupId = @GroupId and ActorId = @UserId)
	begin
		-- If this is the System Group, only members of it can add new Users.
		if @GroupId = 1 
		begin
			if not exists( select 1 from CK.tActorProfile p where p.GroupId = 1 and p.ActorId = @ActorId ) 
			begin
				raiserror( 'Security.ActorMustBeSytem', 16, 1 );
			end
		end
		--<Extension Name="Group.PreUserAdd" />

		insert into CK.tActorProfile( ActorId, GroupId ) values( @UserId, @GroupId );

		--<Extension Name="Group.PostUserAdd" />

	end
	--[endsp]
end