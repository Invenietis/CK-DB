--[beginscript]

drop index IK_CK_tAclConfigMemory on CK.tAclConfigMemory;

alter table CK.tAclConfigMemory add
	constraint PK_CK_tAclConfigMemory primary key clustered( AclId, ActorId, KeyReason );

alter table CK.tAclConfigMemory drop constraint FK_CK_tAclConfigMemory_AclId;

alter table CK.tAclConfigMemory add
	constraint FK_CK_AclConfigMemory_AclId foreign key ( AclId ) references CK.tAcl ( AclId );


--[endscript]
