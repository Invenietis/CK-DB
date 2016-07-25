--[beginscript]

create table CK.tResText
(
	ResId int not null,
	Value nvarchar(max) not null,
	constraint PK_CK_ResText primary key (ResId),
	constraint FK_CK_ResText_ResId foreign key (ResId) references CK.tRes(ResId)
);

insert into CK.tResText( ResId, Value ) values( 0, N'' );
insert into CK.tResText( ResId, Value ) values( 1, N'System' );

--[endscript]