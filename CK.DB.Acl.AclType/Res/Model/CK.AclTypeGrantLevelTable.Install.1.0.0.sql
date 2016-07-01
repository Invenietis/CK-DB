--[beginscript]

create table CK.tAclTypeGrantLevel
(
	AclTypeId int not null,
	GrantLevel tinyint not null constraint CK_CK_tAclTypeGrantLevel_GrantLevel check( GrantLevel <= 127 ),

	constraint PK_CK_tAclTypeGrantLevel primary key clustered ( AclTypeId, GrantLevel ),
	constraint FK_CK_tAclTypeGrantLevel_AclTypeId foreign key ( AclTypeId ) references CK.tAclType ( AclTypeId )
);

insert into CK.tAclTypeGrantLevel( AclTypeId, GrantLevel ) 
	 values (0,0),	-- Blind
			(0,8),	-- User
			(0,16), -- Viewer
			(0,32), -- Contributor
			(0,64), -- Editor
			(0,80), -- SuperEditor
			(0,92), -- SafeAdministrator
			(0,127); -- Administrator

--[endscript]

