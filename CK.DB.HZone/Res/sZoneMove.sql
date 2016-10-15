-- SetupConfig: { "Requires": [ "CK.sGroupMove" ] }
create procedure CK.sZoneMove
(
	@ActorId int,        
    @ZoneId int,  
    @NewParentZoneId int,
    @NextSiblingId int = 0 
)
as begin
	if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]
	
	declare @ZoneHId hierarchyid;
	declare @ParentHId hierarchyid;
	declare @NextSiblingHId hierarchyid;
	declare @LastHId hierarchyid;
	declare @NewHId hierarchyId;

	select @ZoneHId = HierarchicalId from CK.tZone with(serializable) where ZoneId = @ZoneId;
	if @ZoneHId is null throw 50000, 'Zone.InvalidZoneId', 1;
	select @ParentHId = HierarchicalId from CK.tZone with(serializable)  where ZoneId = @NewParentZoneId;
	if @ParentHId is null throw 50000, 'Zone.InvalidNewParentZoneId', 1;
	if @NextSiblingId = 0
	begin
		select @LastHId = max(HierarchicalId) 
			from CK.tZone with(serializable) 
			where HierarchicalId.GetAncestor(1) = @ParentHId;
	end
	else
	begin
		select @NextSiblingHId = HierarchicalId from CK.tZone with(serializable) where ZoneId = @NextSiblingId;
		if @NextSiblingHId is null throw 50000, 'Zone.InvalidNextSiblingId', 1;
		if @NextSiblingHId.GetAncestor(1) <> @ParentHId throw 50000, 'Zone.NextSiblingIdIsNotInNewParentZoneId', 1;
		select @LastHId = max(HierarchicalId) 
			from CK.tZone with(serializable) 
			where HierarchicalId.GetAncestor(1) = @ParentHId and HierarchicalId < @NextSiblingHId;
	end
	select @NewHId = @ParentHId.GetDescendant(@LastHId, @NextSiblingHId);

	--<PreZoneMove revert />

	update CK.tZone set HierarchicalId = HierarchicalId.GetReparentedValue(@ZoneHId, @NewHId)
        where HierarchicalId.IsDescendantOf(@ZoneHId) = 1;
	exec CK.sGroupMove @ActorId, @ZoneId, @NewParentZoneId;

	--<PostZoneMove />

	--[endsp]
end