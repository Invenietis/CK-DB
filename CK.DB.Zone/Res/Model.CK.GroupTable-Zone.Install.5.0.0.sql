--[beginscript]

alter table CK.tGroup add 
	ZoneId int not null constraint DF_TEMP0 default(0);

alter table CK.tGroup drop constraint DF_TEMP0;

--[endscript]

--[beginscript]

alter table CK.tGroup add 
	constraint FK_CK_tGroup_ZoneId foreign key (ZoneId) references CK.tZone( ZoneId );

exec CKCore.sInvariantRegister 'Group.UserNotInZone', N'
	from CK.tUser u
	inner join CK.tActorProfile p on p.ActorId = u.UserId
	inner join CK.tGroup g on g.GroupId = p.GroupId
	left outer join CK.tActorProfile pG on pG.ActorId = u.UserId and pG.GroupId = g.ZoneId
	where g.ZoneId <> 0 and p.ActorId is null
';

--[endscript]
