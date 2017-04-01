--[beginscript]

create table CK.tUserGoogle
(
	UserId int not null,
	-- The Google account identifier is the key to identify a Google user.
	-- See http://stackoverflow.com/questions/27330348/what-is-the-data-type-of-a-google-accounts-unique-user-identifier
	GoogleAccountId varchar(36) collate Latin1_General_100_BIN2 not null,
	LastLoginTime datetime2(2) not null,
	constraint PK_CK_UserGoogle primary key (UserId),
	constraint FK_CK_UserGoogle_UserId foreign key (UserId) references CK.tUser(UserId),
	constraint UK_CK_UserGoogle_GoogleAccountId unique( GoogleAccountId )
);

insert into CK.tUserGoogle( UserId, GoogleAccountId, LastLoginTime ) 
	values( 0, '', sysutcdatetime() );

--[endscript]