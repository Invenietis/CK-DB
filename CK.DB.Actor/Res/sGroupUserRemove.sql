-- Version = *
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
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @GroupId <= 0 throw 50000, 'Group.InvalidId', 1;

	--[beginsp]

	if @GroupId <> @UserId and exists (select * from CK.tActorProfile where GroupId = @GroupId and ActorId = @UserId)
	begin
		-- If this is the System Group, only members of it can remove Users.
		if @GroupId = 1 
		begin
			if not exists( select 1 from CK.tActorProfile p where p.GroupId = 1 and p.ActorId = @ActorId ) 
			begin
				;throw 50000, 'Security.ActorMustBeSytem', 1;
			end
		end
		--<PreUserRemove />

		delete from CK.tActorProfile where GroupId = @GroupId and ActorId = @UserId;
		
		--<PostUserRemove revert />
	end

	--[endsp]
end