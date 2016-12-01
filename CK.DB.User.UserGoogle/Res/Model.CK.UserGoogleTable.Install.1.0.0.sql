--[beginscript]

create table CK.tUserGoogle
(
	UserId int not null,
	-- The Google account identifier is the key to identify a Google user.
	-- See http://stackoverflow.com/questions/27330348/what-is-the-data-type-of-a-google-accounts-unique-user-identifier
	GoogleAccountId varchar(36) not null,
	Scopes varchar(max) not null, 
	AccessToken varchar(max) not null, 
	AccessTokenExpirationTime datetime2(2) not null, 
	RefreshToken varchar(max) not null,
	LastRefreshTokenTime datetime2(2) not null,
	constraint PK_CK_UserGoogle primary key (UserId),
	constraint FK_CK_UserGoogle_UserId foreign key (UserId) references CK.tUser(UserId),
	constraint UK_CK_UserGoogle_GoogleAccountId unique( GoogleAccountId )
);
-- The Anonymous holds the default scopes
insert into CK.tUserGoogle( UserId, GoogleAccountId, Scopes, AccessToken, AccessTokenExpirationTime, RefreshToken, LastRefreshTokenTime ) 
	values( 0, '', 'openid', '', sysutcdatetime(), '', sysutcdatetime() );

--[endscript]