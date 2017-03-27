-- SetupConfig: {}
-- The view consider that a Zone is its own child: relations where ZoneId = ChildId appear.
-- This is both faster (this avoids a filter) and easier to use for dumping sub trees.
create view CK.vZoneAllChildren
as
	select	z.ZoneId, 
			ChildId = c.ZoneId,
			ChildDepth = cast( c.HierarchicalId.GetLevel() as int ),
			ChildOrderByKey = c.HierarchicalId
	from CK.tZone z
	inner join CK.tZone c on c.HierarchicalId.IsDescendantOf(z.HierarchicalId) = 1;

