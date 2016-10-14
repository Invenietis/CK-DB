-- SetupConfig: {}
create view CK.vZoneDirectChildren
as
	select	z.ZoneId, 
			Depth = z.HierarchicalId.GetLevel(),
			ChildId = c.ZoneId,
			ChildOrderByKey = c.HierarchicalId
	from CK.tZone z
	inner join CK.tZone c on c.HierarchicalId.GetAncestor(1) = z.HierarchicalId;

