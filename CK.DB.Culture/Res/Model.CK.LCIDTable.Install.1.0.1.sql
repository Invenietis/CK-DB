--[beginscript]

create table CK.tLCID
(
	LCID int not null,
	Name varchar(20) collate LATIN1_General_BIN2 not null,
	EnglishName varchar(50) collate LATIN1_General_BIN2 not null,
	NativeName nvarchar(50) not null,
	ParentLCID int not null,
	constraint PK_CK_LCID primary key( LCID ),
	constraint FK_CK_LCID_XLCID foreign key( LCID ) references CK.tXLCID( XLCID ),
	constraint FK_CK_LCID_ParentLCID foreign key( ParentLCID ) references CK.tLCID( LCID )
);

insert into CK.tLCID( LCID, Name, EnglishName, NativeName, ParentLCID ) values( 0, '', 'Invariant Language (Invariant Country)', N'Invariant Language (Invariant Country)', 0 );
insert into CK.tLCID( LCID, Name, EnglishName, NativeName, ParentLCID ) values( 9, 'en', 'English', N'English', 0 );
insert into CK.tLCID( LCID, Name, EnglishName, NativeName, ParentLCID ) values( 12, 'fr', 'French', N'Français', 0 );

--[endscript]
