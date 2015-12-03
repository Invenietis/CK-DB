--[beginscript]

create table CK.tActorProfile
(
	ActorId int not null,
	GroupId int not null,

	constraint PK_CK_ActorProfile primary key clustered( ActorId, GroupId ),
	constraint FK_CK_ActorProfile_ActorId foreign key(ActorId) references CK.tActor( ActorId ),
	constraint FK_CK_ActorProfile_GroupId foreign key(GroupId) references CK.tActor( ActorId )
);
-- We do not index by GroupId by default. Usage of this kind of lookup (listing Actors of a Group) is 
-- mainly for administrative functionalities. This index may be created if actually needed.

insert into CK.tActorProfile( ActorId, GroupId ) values( 0, 0 );
insert into CK.tActorProfile( ActorId, GroupId ) values( 1, 1 );

--[endscript]
