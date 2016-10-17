--[beginscript]

create table CK.tUser 
(
	UserId int not null,
	-- Note: _BIN2 collations match the behavior of the Ordinal .Net StringComparison. 
	--       This is NOT an invariant: this can be altered if needed.
	UserName nvarchar( 127 ) collate Latin1_General_100_BIN2 not null,
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

