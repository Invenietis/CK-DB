-- SetupConfig: {}
create procedure CK.sGroupMove
(
	@ActorId int,        
    @GroupId int,  
    @NewZoneId int,
	@Option int -- not null enum { "None": 0, "Intersect": 1, "AutoUserRegistration": 2 }
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	declare @CurrentZoneId int;
	select @CurrentZoneId = ZoneId from CK.tGroup where GroupId = @GroupId;
	if @CurrentZoneId is not null and @CurrentZoneId <> @NewZoneId
	begin

		--<PreGroupMoveCheck revert />
		if @Option = 0
		begin
			if exists( select p.ActorId from CK.tActorProfile p where p.GroupId = @GroupId and p.ActorId <> p.GroupId
							except 
					   select p.ActorId from CK.tActorProfile p where p.GroupId = @NewZoneId and p.ActorId <> p.GroupId )
			begin
				;throw 50000, 'Group.UserNotInZone', 1;
			end
		end
		else if @Option = 1 or @Option = 2 
		begin
			declare @ExtraUserIdInZone int;
			declare @CUserToRemove cursor;
			set @CUserToRemove = cursor local fast_forward for 
					   select p.ActorId from CK.tActorProfile p where p.GroupId = @GroupId and p.ActorId <> p.GroupId
							except 
					   select p.ActorId from CK.tActorProfile p where p.GroupId = @NewZoneId and p.ActorId <> p.GroupId;
			open @CUserToRemove;
			fetch from @CUserToRemove into @ExtraUserIdInZone;
			while @@FETCH_STATUS = 0
			begin
				if @Option = 1 -- Intersect
				begin
					exec CK.sGroupUserRemove @ActorId, @GroupId, @ExtraUserIdInZone;
				end
				else
				begin -- 2 - AutoUserRegistration
					exec CK.sZoneUserAdd @ActorId, @NewZoneId, @ExtraUserIdInZone;
				end
				fetch next from @CUserToRemove into @ExtraUserIdInZone;
			end
			deallocate @CUserToRemove;
		end
		else throw 50000, 'ArgumentNotSupported', 1;
		--<PostGroupMoveCheck />

		--<PreGroupMove revert />

		update CK.tGroup set ZoneId = @NewZoneId where GroupId = @GroupId;
	
		--<PostGroupMove />
	
	end

	--[endsp]
end

