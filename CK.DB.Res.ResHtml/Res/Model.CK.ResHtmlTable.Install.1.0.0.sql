--[beginscript]

create table CK.tResHtml
(
	ResId int not null,
	Value nvarchar(max) not null,
	constraint PK_CK_ResHtml primary key (ResId),
	constraint FK_CK_ResHtml_ResId foreign key (ResId) references CK.tRes(ResId)
);

insert into CK.tResHtml( ResId, Value ) values( 0, N'' );
insert into CK.tResHtml( ResId, Value ) values( 1, N'<strong>System</strong>' );

--[endscript]