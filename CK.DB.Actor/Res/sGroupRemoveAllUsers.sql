-- SetupConfig: { "Requires": [ "CK.sGroupUserRemove" ] }
--
-- Clears a Group.
--
create procedure CK.sGroupRemoveAllUsers
(
	@ActorId int,
	@GroupId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @GroupId <= 0 throw 50000, 'Group.InvalidGroup', 1;

	--[beginsp]

	declare @UserId int;
	declare @CUser cursor;
	set @CUser = cursor local fast_forward for 
		select ActorId from CK.tActorProfile p 
						inner join CK.tUser u on u.UserId = p.ActorId
						where p.GroupId = @GroupId and p.ActorId <> @GroupId;
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