--[beginscript]

create table CK.tUser 
(
	UserId int not null,
	-- Collation should be Case insensitive at least (this is the recommended practice for user names).
	-- 255 seems large but this is to support emails as user names: emails can be 254 unicode characters long.
	UserName nvarchar( 255 ) collate Latin1_General_100_CI_AS not null,
	-- Overall storage size for datetime2(0) is the same as for datetime2(2): 7 bytes.
	-- Let's keep the better precision for it.
	CreationDate datetime2(2) not null constraint DF_CK_tUser_CreationDate default( sysutcdatetime() ),

	constraint PK_CK_tUser primary key clustered( UserId ),
	constraint FK_CK_tUser_tActor foreign key ( UserId ) references CK.tActor( ActorId ),
	constraint UK_CK_tUser_UserName unique ( UserName )
);
--
insert into CK.tUser( UserId, UserName ) values( 0, '' );
insert into CK.tUser( UserId, UserName ) values( 1, 'System' );

--[endscript]

