--[beginscript]

create table CK.tXLCIDMap 
(
	XLCID int not null,
	Idx smallint not null, 
	LCID int not null,
	constraint PK_CK_XLCIDMap primary key (XLCID,Idx),
	constraint FK_CK_XLCIDMap_XLCID foreign key( XLCID ) references CK.tXLCID( XLCID ),
	constraint FK_CK_XLCIDMap_LCID foreign key( LCID ) references CK.tLCID( LCID )
);

insert into CK.tXLCIDMap( XLCID, Idx, LCID ) values( 9, 0, 9 );
insert into CK.tXLCIDMap( XLCID, Idx, LCID ) values( 9, 1, 12 );
insert into CK.tXLCIDMap( XLCID, Idx, LCID ) values( 12, 0, 12 );
insert into CK.tXLCIDMap( XLCID, Idx, LCID ) values( 12, 1, 9 );

--[endscript]