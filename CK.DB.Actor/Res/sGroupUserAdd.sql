-- SetupConfig: {}
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
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @GroupId <= 0 throw 50000, 'Group.InvalidId', 1;

    if not exists(select 1 from CK.tUser where UserId = @UserId)
    begin
        ;throw 50000, 'User.NotAUser', 1;
    end

	-- System is, somehow, already in all groups.
    if @UserId = 1 return 0;

	--[beginsp]

	if @GroupId <> @UserId and not exists (select * from CK.tActorProfile where GroupId = @GroupId and ActorId = @UserId)
	begin
		-- If this is the System Group, only members of it can add new Users.
		if @GroupId = 1
		begin
			if not exists( select 1 from CK.tActorProfile p where p.GroupId = 1 and p.ActorId = @ActorId )
			begin
				;throw 50000, 'Security.ActorMustBeSytem', 1;
			end
		end

		--<PreUserAdd revert />

		insert into CK.tActorProfile( ActorId, GroupId ) values( @UserId, @GroupId );

		--<PostUserAdd />

	end
	--[endsp]
end
