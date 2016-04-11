-- Version = *
create view CK.vGroup
as 
	select  GroupId,
			CreationDate,
			GroupName = N'#Group-' + GroupId,
			UserCount = (select count(*) from CK.tActorProfile with(nolock) where GroupId = g.GroupId)
		from CK.tGroup g;