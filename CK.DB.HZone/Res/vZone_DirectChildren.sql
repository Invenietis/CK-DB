-- Version = *
create view CK.vZone_DirectChildren
as
	select	z.ZoneId, 
			Depth = z.Depth,
			ChildId = c.ChildId,
			ChildOrderByKey = c.ChildOrderByKey
	from CK.tZone z
	inner join CK.vZone_AllChildren c on c.ZoneId = z.ZoneId and c.ChildDepth = z.Depth+1;

