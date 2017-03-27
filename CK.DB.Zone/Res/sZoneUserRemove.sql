-- SetupConfig: {}
--
-- Removes a User from a Zone.
--
alter procedure CK.sZoneUserRemove
(
	@ActorId int,
	@ZoneId int,
	@UserId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	-- The user must be in the Zone...
	if @ZoneId <> @UserId and exists (select * from CK.tActorProfile where GroupId = @ZoneId and ActorId = @UserId)
	begin
		-- ...and if this is the System Zone, only members of it can remove Users.
		if @ZoneId = 1 
		begin
			if not exists( select 1 from CK.tActorProfile p where p.GroupId = 1 and p.ActorId = @ActorId ) 
			begin
				;throw 50000, 'Security.ActorMustBeSytem', 1;
			end
		end
		-- ..and if the ZoneId is actually a Group, this is an error.
		if not exists (select * from CK.tZone with(serializable) where ZoneId = @ZoneId) throw 50000, 'Zone.InvalidId', 1;

		--<PreZoneUserRemove revert />

		-- Removes the user from all the groups of the security Zone.
		declare @GroupId int;
		declare @CGroup cursor;
		set @CGroup = cursor local fast_forward for 
			select a.GroupId
				from CK.tActorProfile a
				inner join CK.tGroup g on g.GroupId = a.GroupId
				where g.ZoneId = @ZoneId and a.ActorId = @UserId and a.GroupId <> @ZoneId;
		open @CGroup;
		fetch from @CGroup into @GroupId;
		while @@FETCH_STATUS = 0
		begin
			exec CK.sGroupUserRemove @ActorId, @GroupId, @UserId;
			fetch next from @CGroup into @GroupId;
		end
		deallocate @CGroup;

		delete from CK.tActorProfile where GroupId = @ZoneId and ActorId = @UserId;

		--<PostZoneUserRemove />
	end

	--[endsp]
end