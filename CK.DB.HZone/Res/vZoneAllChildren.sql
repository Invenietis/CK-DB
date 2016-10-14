-- SetupConfig: {}
create view CK.vZoneAllChildren
as
	select	z.ZoneId, 
			Depth = z.HierarchicalId.GetLevel(),
			ChildId = c.ZoneId,
			ChildDepth = c.HierarchicalId.GetLevel(),
			ChildOrderByKey = c.HierarchicalId
	from CK.tZone z
	inner join CK.tZone c on c.ZoneId <> z.ZoneId and c.HierarchicalId.IsDescendantOf(z.HierarchicalId) = 1;

