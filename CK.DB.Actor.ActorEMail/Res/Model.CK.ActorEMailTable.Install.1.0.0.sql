--[beginscript]

create table CK.tActorEMail
(
	ActorId int not null,
	EMail nvarchar( 255 ) collate Latin1_General_100_CI_AS not null,
	IsPrimary bit not null,
	ValTime datetime2(2) not null,
	constraint PK_CK_ActorEMail primary key (ActorId,EMail),
	constraint FK_CK_ActorEMail_ActorId foreign key (ActorId) references CK.tActor(ActorId)
);

--[endscript]