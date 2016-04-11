--[beginscript]

create table CK.tAclConfigMemory
(
	AclId int not null,
	ActorId int not null,
	-- Note: _BIN2 collations match the behavior of the Ordinal .Net StringComparison. 
	--       This is an invariant!
	KeyReason varchar(128) collate Latin1_General_BIN2 not null,
	GrantLevel tinyint not null,

	constraint FK_CK_tAclConfigMemory_AclId foreign key ( AclId ) references CK.tAcl ( AclId ),
	constraint FK_CK_AclConfigMemory_ActorId foreign key ( ActorId ) references CK.tActor ( ActorId )
);

create clustered index IK_CK_tAclConfigMemory on CK.tAclConfigMemory( AclId, ActorId, KeyReason );

--[endscript]
