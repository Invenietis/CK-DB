-- SetupConfig: {"Requires": ["CK.vGroup"] }
create view CK.vZone
as
	select	z.ZoneId, 
			GroupCount = (select count(*)-1 from CK.tGroup g where g.ZoneId = z.ZoneId)
	from CK.tZone z
	inner join CK.vGroup g on g.GroupId = z.ZoneId
