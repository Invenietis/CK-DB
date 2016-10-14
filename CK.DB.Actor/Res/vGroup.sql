-- SetupConfig: {}
create view CK.vGroup
as 
	select  GroupId,
			GroupName = N'#Group-' +  cast( GroupId as varchar),
			UserCount = (select count(*) 
							from CK.tUser u with(nolock) 
							inner join CK.tActorProfile p with(nolock) on p.ActorId = u.UserId
							where p.GroupId = g.GroupId)
		from CK.tGroup g;
