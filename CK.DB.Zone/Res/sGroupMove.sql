-- SetupConfig: {}
create procedure CK.sGroupMove
(
	@ActorId int,        
    @GroupId int,  
    @NewZoneId int
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	declare @CurrentZoneId int;
	select @CurrentZoneId = ZoneId from CK.tGroup where GroupId = @GroupId;
	if @CurrentZoneId is not null and @CurrentZoneId <> @NewZoneId
	begin

		--<PreGroupMove revert />

		update CK.tGroup set ZoneId = @NewZoneId where GroupId = @GroupId;
	
		--<PostGroupMove />
	
	end

	--[endsp]
end

