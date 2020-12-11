--[beginscript]

create table CK.tActor
(
	ActorId int not null identity (0, 1),
	constraint PK_CK_tActor primary key clustered( ActorId )
);

-- Anonymous n°0.
insert into CK.tActor default values;
-- System n°1.
insert into CK.tActor default values;
-- Administrators group n°2.
insert into CK.tActor default values;

-- Reserved for the "Platform Zone" that must be the n°3.
-- We delete the row immediately (not pretty but who cares).
insert into CK.tActor default values;
delete from CK.tActor where ActorId = 3;

--[endscript]
