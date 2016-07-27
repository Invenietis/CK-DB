--[beginscript]

create table CK.tMCResString 
(
	ResId int not null,
	LCID int not null,
	Value nvarchar(400) not null,
	constraint PK_CK_MCResString primary key (ResId,LCID),
	constraint FK_CK_MCResString_ResId foreign key (ResId) references CK.tRes(ResId),
	constraint FK_CK_MCResString_LCID foreign key (LCID) references CK.tLCID(LCID)
);

insert into CK.tMCResString( ResId, LCID, Value ) values( 0, 0, N'' );
alter table CK.tLCID with nocheck add constraint CK_CK_tMCResString_LCID check (LCID > 0);

insert into CK.tMCResString( ResId, LCID, Value ) values( 0, 12, N'' );
insert into CK.tMCResString( ResId, LCID, Value ) values( 0, 9, N'' );
insert into CK.tMCResString( ResId, LCID, Value ) values( 1, 12, N'Système' );
insert into CK.tMCResString( ResId, LCID, Value ) values( 1, 9, N'System' );

--[endscript]