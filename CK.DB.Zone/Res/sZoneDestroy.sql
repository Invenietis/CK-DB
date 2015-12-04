-- Version = 1.0.4, Requires = { CK.sGroupRemoveAllUsers, CK.sGroupDestroy }
create procedure CK.sZoneDestroy
(
	@ActorId int,
	@ZoneId int
)
as
begin
    if @ActorId <= 0 raiserror( 'Security.AnonymousNotAllowed', 16, 1 );
	if @ZoneId <= 1 raiserror( 'Zone.UndestroyableZone', 16, 1 );

	--[beginsp]

	declare @AdministratorsGroupId int;
	select @AdministratorsGroupId = AdministratorsGroupId 
		from CK.tZone 
		where ZoneId = @ZoneId;
	
	if @AdministratorsGroupId is not null 
	begin

		--<Extension Name="Zone.PreZoneDestroy" />

		exec CK.sGroupRemoveAllUsers @ActorId, @AdministratorsGroupId;
		update CK.tZone set AdministratorsGroupId = 0 where ZoneId = @ZoneId;
		exec CK.sGroupDestroy @ActorId, @AdministratorsGroupId;

		-- Removes all groups owned by the zone
		declare @GroupId int;
		declare @CUser cursor;
		set @CUser = cursor local fast_forward for select GroupId from CK.tGroup p where p.ZoneId = @ZoneId and p.GroupId <> @ZoneId;
		open @CUser;
		fetch from @CUser into @GroupId;
		while @@FETCH_STATUS = 0
		begin
			exec CK.sGroupRemoveAllUsers @ActorId, @GroupId;
			exec CK.sGroupDestroy @ActorId, @GroupId;
			fetch next from @CUser into @GroupId;
		end
		deallocate @CUser;

		update CK.tGroup set ZoneId = 0 where GroupId = @ZoneId;
		delete from CK.tZone where ZoneId = @ZoneId;

		exec CK.sGroupRemoveAllUsers @ActorId, @ZoneId;
		exec CK.sGroupDestroy @ActorId, @ZoneId;

		--<Extension Name="Zone.PostZoneDestroy" />
	
	end

	--[endsp]
end

