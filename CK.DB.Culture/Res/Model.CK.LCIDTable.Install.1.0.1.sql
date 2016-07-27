--[beginscript]

create table CK.tLCID
(
	LCID int not null,
	Name varchar(20) collate LATIN1_General_BIN2 not null,
	EnglishName varchar(50) collate LATIN1_General_BIN2 not null,
	NativeName nvarchar(50) not null,
	-- Total count of pure XLCID created so far with this LCID as the primary LCID.
	XLCIDCount smallint not null,
	constraint PK_CK_LCID primary key( LCID ),
	constraint FK_CK_LCID_XLCID foreign key( LCID ) references CK.tXLCID( XLCID )
);

insert into CK.tLCID( LCID, Name, EnglishName, NativeName, XLCIDCount ) values( 0, '', 'Invalid Language', N'Invalid Language', 0 );

alter table CK.tLCID with nocheck add 
	-- Only XLCID that are only fallbacks are greater than 0xFFFF.
	constraint CK_CK_tLCID_LCID check (LCID > 0 and LCID < 0xFFFF);

insert into CK.tLCID( LCID, Name, EnglishName, NativeName, XLCIDCount ) values( 9, 'en', 'English', N'English', 0 );
insert into CK.tLCID( LCID, Name, EnglishName, NativeName, XLCIDCount ) values( 12, 'fr', 'French', N'Français', 0 );

--[endscript]
