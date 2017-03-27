--[beginscript]

create table CK.tResString 
(
	ResId int not null,
	Value nvarchar(400) not null,
	constraint PK_CK_ResString primary key (ResId),
	constraint FK_CK_ResString_ResId foreign key (ResId) references CK.tRes(ResId)
);

insert into CK.tResString( ResId, Value ) values( 0, N'' );
insert into CK.tResString( ResId, Value ) values( 1, N'System' );

--[endscript]