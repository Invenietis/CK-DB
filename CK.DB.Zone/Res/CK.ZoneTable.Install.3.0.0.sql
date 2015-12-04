--[beginscript]

create table CK.tZone 
(
	ZoneId int not null,
	AdministratorsGroupId int not null,

	constraint PK_CK_tZone primary key clustered ( ZoneId ),
	constraint FK_CK_tZone_tGroup_ZoneId foreign key( ZoneId ) references CK.tGroup( GroupId ),
	constraint FK_CK_tZone_tGroup_AdministratorsGroupId foreign key( AdministratorsGroupId ) references CK.tGroup( GroupId )
);
insert into CK.tZone( ZoneId, AdministratorsGroupId ) values ( 1, 1 );
-- The System Zone is administrator of the Public Zone.
insert into CK.tZone( ZoneId, AdministratorsGroupId ) values ( 0, 1 );

--[endscript]
