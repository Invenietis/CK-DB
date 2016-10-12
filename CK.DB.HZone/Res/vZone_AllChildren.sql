-- Version = *
create view CK.vZone_AllChildren
as
	select	z.ZoneId, 
			Depth = z.Depth,
			ChildId = c.ZoneId,
			ChildDepth = c.Depth,
			ChildOrderByKey = c.LeftNumber
	from CK.tZone c
	inner join CK.tZone z on z.LeftNumber < c.RightNumber and z.RightNumber > c.RightNumber;
