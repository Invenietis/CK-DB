-- Version = 1.0.2, Requires={ CK.sGroupUserRemove }
--
-- Clears a Group.
--
create procedure CK.sGroupRemoveAllUsers
(
	@ActorId int,
	@GroupId int
)
as begin
    if @ActorId <= 0 raiserror( 'Security.AnonymousNotAllowed', 16, 1 );
    if @GroupId <= 0 raiserror( 'Group.InvalidGroup', 16, 1 );

	--[beginsp]

	declare @UserId int;
	declare @CUser cursor;
	set @CUser = cursor local fast_forward for 
		select ActorId from CK.tActorProfile p 
						where p.GroupId = @GroupId and p.ActorId <> @GroupId and p.ActorId <> @GroupId;
	open @CUser;
	fetch from @CUser into @UserId;
	while @@FETCH_STATUS = 0
	begin
		exec CK.sGroupUserRemove @ActorId, @GroupId, @UserId;
		fetch next from @CUser into @UserId;
	end
	deallocate @CUser;

	--[endsp]
end