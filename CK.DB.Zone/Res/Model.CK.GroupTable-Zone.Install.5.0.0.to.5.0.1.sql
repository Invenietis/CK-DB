
exec CKCore.sInvariantRegister 'Group.UserNotInZone', N'
	from CK.tUser u
	inner join CK.tActorProfile p on p.ActorId = u.UserId
	inner join CK.tGroup g on g.GroupId = p.GroupId
	left outer join CK.tActorProfile pZ on pZ.ActorId = u.UserId and pZ.GroupId = g.ZoneId
	where g.ZoneId <> 0 and pZ.ActorId is null
';

