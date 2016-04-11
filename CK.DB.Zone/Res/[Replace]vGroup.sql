-- Version = *
create view CK.vGroup
as 
	select  g.GroupId,
			g.CreationDate,
			GroupName = N'#Group-' + GroupId,
			UserCount = (select count(*) from CK.tActorProfile with(nolock) where GroupId = g.GroupId),
			--From: Zone
			ZoneId = g.ZoneId
		from CK.tGroup g;
