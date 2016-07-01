--[beginscript]

create table CK.tZone 
(
	ZoneId int not null,

	constraint PK_CK_tZone primary key clustered ( ZoneId ),
	constraint FK_CK_tZone_tGroup_ZoneId foreign key( ZoneId ) references CK.tGroup( GroupId )
);
-- Public, default, Zone.
insert into CK.tZone( ZoneId ) values ( 0 );
-- System Zone.
insert into CK.tZone( ZoneId ) values ( 1 );

--[endscript]
