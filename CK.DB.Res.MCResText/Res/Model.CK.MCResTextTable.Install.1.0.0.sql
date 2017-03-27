--[beginscript]

create table CK.tMCResText 
(
	ResId int not null,
	LCID int not null,
	Value nvarchar(max) not null,
	constraint PK_CK_MCResText primary key (ResId,LCID),
	constraint FK_CK_MCResText_ResId foreign key (ResId) references CK.tRes(ResId),
	constraint FK_CK_MCResText_LCID foreign key (LCID) references CK.tLCID(LCID)
);

insert into CK.tMCResText( ResId, LCID, Value ) values( 0, 0, N'' );
alter table CK.tLCID with nocheck add constraint CK_CK_tMCResText_LCID check (LCID > 0);

insert into CK.tMCResText( ResId, LCID, Value ) values( 0, 12, N'' );
insert into CK.tMCResText( ResId, LCID, Value ) values( 0, 9, N'' );
insert into CK.tMCResText( ResId, LCID, Value ) values( 1, 12, N'Système' );
insert into CK.tMCResText( ResId, LCID, Value ) values( 1, 9, N'System' );

--[endscript]