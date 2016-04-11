-- Version = *, RequiredBy={CK.vZone}
create view CK.vGroup
as 
	select  g.GroupId,
			g.CreationDate,
			GroupName = N'#Group-' + GroupId,
			--From: Zone
			ZoneId = g.ZoneId
		from CK.tGroup g;
