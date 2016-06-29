-- SetupConfig: { "Requires": [ "CK.sGroupUserRemove" ] }
--
-- Removes a User from all the Groups it belongs to.
--
create procedure CK.sUserRemoveFromAllGroups
(
	@ActorId int,
	@UserId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	declare @GroupId int;
	declare @CGroup cursor;
	set @CGroup = cursor local fast_forward for 
		select GroupId from CK.tActorProfile where ActorId = @UserId and GroupId <> @UserId;
	open @CGroup
	fetch from @CGroup into @GroupId
	while @@FETCH_STATUS = 0
	begin
		exec CK.sGroupUserRemove @ActorId, @GroupId, @UserId;
		fetch next from @CGroup into @GroupId;
	end
	deallocate @CGroup;

	--[endsp]
end