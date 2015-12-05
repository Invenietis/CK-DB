-- Version = 15.12.5
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
		if not exists (select * from CK.tZone where ZoneId = @ZoneId) throw 50000, 'Zone.InvalidId', 1;

		--<Extension Name="Zone.PreUserRemove" />

		-- Removes the user from all the groups of the security Zone.
		-- Testing @IsZone here avoids one relay per Zone (preserving the 32 maximum levels of calls).
		declare @GroupId int;
		declare @IsZone bit;
		declare @CGroup cursor;
		set @CGroup = cursor local fast_forward for 
			select a.GroupId, case when z.ZoneId is null then 0 else 1 end
				from CK.tActorProfile a
				inner join CK.tGroup g on g.GroupId = a.GroupId
				left outer join CK.tZone z on z.ZoneId = g.GroupId
				where g.ZoneId = @ZoneId and a.ActorId = @UserId and a.GroupId <> @ZoneId;
		open @CGroup;
		fetch from @CGroup into @GroupId, @IsZone;
		while @@FETCH_STATUS = 0
		begin
			if @IsZone = 1
			begin
				exec CK.sZoneUserRemove @ActorId, @GroupId, @UserId;
			end
			else
			begin
				exec CK.sGroupUserRemove @ActorId, @GroupId, @UserId;
			end
			fetch next from @CGroup into @GroupId, @IsZone;
		end
		deallocate @CGroup;

		delete from CK.tActorProfile where GroupId = @ZoneId and ActorId = @UserId;

		--<Extension Name="Zone.PostUserRemove" />
	end

	--[endsp]
end