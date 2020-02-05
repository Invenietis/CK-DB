﻿--[beginscript]

create table CK.tResName
(
	ResId int not null,
	ResName varchar(128) collate Latin1_General_100_BIN2 not null,
	constraint PK_CK_ResName primary key nonclustered (ResId),
	constraint FK_CK_ResName_ResId foreign key (ResId) references CK.tRes( ResId )
);

create unique clustered index IX_CK_ResName_ResName on CK.tResName( ResName );

insert into CK.tResName( ResId, ResName ) values( 0, '' );
insert into CK.tResName( ResId, ResName ) values( 1, 'System' );

--[endscript]