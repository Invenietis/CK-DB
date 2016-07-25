--[beginscript]

create table CK.tXLCID 
(
	XLCID int not null,
	constraint PK_CK_XLCID primary key (XLCID)
);

insert into CK.tXLCID( XLCID ) values( 0 );
insert into CK.tXLCID( XLCID ) values( 9 );
insert into CK.tXLCID( XLCID ) values( 12 );

--[endscript]