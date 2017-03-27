--[beginscript]

create table CK.tRes
(
	ResId int not null identity (0, 1),
	constraint PK_CK_tRes primary key clustered( ResId )
);

insert into CK.tRes default values;
insert into CK.tRes default values;

--[endscript]
