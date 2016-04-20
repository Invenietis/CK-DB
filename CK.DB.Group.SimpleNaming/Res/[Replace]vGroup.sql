-- Version = *
create view CK.vGroup
as 
	select  g.GroupId,
			g.CreationDate,
			GroupName = g.GroupName
		from CK.tGroup g;
