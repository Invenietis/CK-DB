--[beginscript]

create table CK.tUserSimpleCode
(
	UserId int not null,
	SimpleCode nvarchar(128) collate Latin1_General_100_BIN2 not null,
	LastLoginTime datetime2(2) not null,
	constraint PK_CK_UserSimpleCode primary key (UserId),
	constraint FK_CK_UserSimpleCode_UserId foreign key (UserId) references CK.tUser(UserId),
	constraint UK_CK_UserSimpleCode_SimpleCode unique( SimpleCode )
);

insert into CK.tUserSimpleCode( UserId, SimpleCode, LastLoginTime ) 
	values( 0, '', sysutcdatetime() );

--[endscript]
