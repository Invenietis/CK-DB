-- Version = 1.0.1
--
-- Removes a User from a Group.
--
create procedure CK.sGroupUserRemove
(
	@ActorId int,
	@GroupId int,
	@UserId int
)
as begin
    if @ActorId <= 0 raiserror( 'Security.AnonymousNotAllowed', 16, 1 );
    if @GroupId <= 0 raiserror( 'Group.InvalidId', 16, 1 );

	--[beginsp]

	if @GroupId <> @UserId and exists (select * from CK.tActorProfile where GroupId = @GroupId and ActorId = @UserId)
	begin
		-- If this is the System Group, only members of it can remove Users.
		if @GroupId = 1 
		begin
			if not exists( select 1 from CK.tActorProfile p where p.GroupId = 1 and p.ActorId = @ActorId ) 
			begin
				raiserror( 'Security.ActorMustBeSytem', 16, 1 );
			end
		end
		--<Extension Name="Group.PreUserRemove" />

		delete from CK.tActorProfile where GroupId = @GroupId and ActorId = @UserId;
		
		--<Extension Name="Group.PostUserRemove" />
	end

	--[endsp]
end