-- SetupConfig: {}
-- The view does not consider that a Zone is its own child: relations where ZoneId = ChildId 
-- do not appear (as opposed to the vZoneAllChildren view).
-- Goal of this wiew is focused on getting children of a zone. 
create view CK.vZoneDirectChildren
as
	select	z.ZoneId, 
			ChildId = c.ZoneId,
			ChildOrderByKey = c.HierarchicalId
	from CK.tZone z
	inner join CK.tZone c on c.HierarchicalId.GetAncestor(1) = z.HierarchicalId;

