-- SetupConfig: {}
create view CK.vGroup
as 
	select  g.GroupId,
			GroupName = N'#Group-' +  cast(g.GroupId as varchar) collate Latin1_General_100_CI_AI,
			UserCount = (select count(*) 
							from CK.tUser u with(nolock) 
							inner join CK.tActorProfile p with(nolock) on p.ActorId = u.UserId
							where p.GroupId = g.GroupId)
		from CK.tGroup g;
