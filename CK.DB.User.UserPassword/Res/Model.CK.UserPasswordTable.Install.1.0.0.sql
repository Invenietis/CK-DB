--[beginscript]

create table CK.tUserPassword
(
	UserId int not null,
	PwdHash varbinary(64) not null,
	LastWriteTime datetime2(2) not null,
	constraint PK_CK_UserPassword primary key (UserId),
	constraint FK_CK_UserPassword_UserId foreign key (UserId) references CK.tUser(UserId)
);

insert into CK.tUserPassword( UserId, PwdHash, LastWriteTime ) values(0, 0x0, sysutcdatetime() );

--[endscript]