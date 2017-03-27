-- SetupConfig: {"Requires": ["CK.vGroup"] }
create view CK.vZone
as
	select	z.ZoneId, 
			ZoneName = g.GroupName,
			GroupCount = (select count(*) from CK.tGroup g with(nolock) where g.ZoneId = z.ZoneId),
			UserCount = g.UserCount
	from CK.tZone z
	inner join CK.vGroup g on g.GroupId = z.ZoneId
