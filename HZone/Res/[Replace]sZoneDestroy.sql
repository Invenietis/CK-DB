-- Version = 1.1.0, Requires = { CK.sGroupRemoveAllUsers, CK.sGroupDestroy }
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
	declare @BackOfficeGroupId int;
	declare @ProspectionGroupId int;
	declare @BackOfficeAclId int;
	declare @ProspectionAclId int;
	declare @LeftNumber int;
	declare @RightNumber int;

	select	@AdministratorsGroupId = AdministratorsGroupId,
			@BackOfficeGroupId = BackOfficeGroupId,
			@ProspectionGroupId = ProspectionGroupId,
			@BackOfficeAclId = BackOfficeAclId,
			@ProspectionAclId = ProspectionAclId,
			@LeftNumber = LeftNumber,
			@RightNumber = RightNumber
	from CK.tZone 
	where ZoneId = @ZoneId;
	
	if @AdministratorsGroupId is not null 
	begin

		--<Extension Name="Zone.PreZoneDestroy">
		if @RightNumber - @LeftNumber > 1 raiserror( 'Zone.UnableToDestroyZoneWithChildren', 16, 1 );
		--</Extension>

		-- Remove administrators group
		exec CK.sGroupRemoveAllUsers @ActorId, @AdministratorsGroupId;
		update CK.tZone set AdministratorsGroupId = 0 where ZoneId = @ZoneId;
		exec CK.sGroupDestroy @ActorId, @AdministratorsGroupId;

		-- Remove BackOfficeGroup
		exec CK.sGroupRemoveAllUsers @ActorId, @BackOfficeGroupId;
		update CK.tZone set BackOfficeGroupId = 0 where ZoneId = @ZoneId;
		exec CK.sGroupDestroy @ActorId, @BackOfficeGroupId;

		-- Remove ProspectionGroup
		exec CK.sGroupRemoveAllUsers @ActorId, @ProspectionGroupId;
		update CK.tZone set ProspectionGroupId = 0 where ZoneId = @ZoneId;
		exec CK.sGroupDestroy @ActorId, @ProspectionGroupId;

		-- Remove BackOfficeAcl
		update CK.tZone set BackOfficeAclId = 0 where ZoneId = @ZoneId;
		exec CK.sAclDestroy @ActorId, @BackOfficeAclId;

		-- Remove ProspectionAcl
		update CK.tZone set ProspectionAclId = 0 where ZoneId = @ZoneId;
		exec CK.sAclDestroy @ActorId, @ProspectionAclId;

		-- Zone delete
		delete from CK.tZone where ZoneId = @ZoneId;
		exec CK.sGroupRemoveAllUsers @ActorId, @ZoneId;
		exec CK.sGroupDestroy @ActorId, @ZoneId;

		--<Extension Name="Zone.PostZoneDestroy">
		update CK.tZone
           set LeftNumber = LeftNumber - 2
           where LeftNumber > @RightNumber;
		update CK.tZone
           set RightNumber = RightNumber - 2
           where RightNumber > @RightNumber;
		--</Extension>
	end

	--[endsp]
end

