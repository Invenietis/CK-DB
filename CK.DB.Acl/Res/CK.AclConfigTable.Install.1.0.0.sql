--[beginscript]

create table CK.tAclConfig
(
	AclId int not null,
	ActorId int not null,
	GrantLevel tinyint not null,

	constraint PK_CK_tAclConfig primary key clustered ( AclId, ActorId ),
	constraint FK_CK_tAclConfig_AclId foreign key ( AclId ) references CK.tAcl ( AclId ),
	constraint FK_CK_tAclConfig_ActorId foreign key ( ActorId ) references CK.tActor ( ActorId )
);

--[endscript]

--[beginscript]

-- The 0 Acl gives full rights to anyone
insert into CK.[tAclConfig](AclId,ActorId,GrantLevel) values(0,0,127);
insert into CK.[tAclConfigMemory](AclId,ActorId,KeyReason,GrantLevel) values(0,0,'CK.StdAcl.Public',127);

-- The 1 Acl gives no rights to anybody...except Gods.

-- The 2 Acl can be used by anybody...
insert into CK.[tAclConfig](AclId,ActorId,GrantLevel) values(2,0,8);
insert into CK.[tAclConfigMemory](AclId,ActorId,KeyReason,GrantLevel) values(2,0,'CK.StdAcl.User',8);

-- The 3 Acl can be viewed by anybody...
insert into CK.[tAclConfig](AclId,ActorId,GrantLevel) values(3,0,16)
insert into CK.[tAclConfigMemory](AclId,ActorId,KeyReason,GrantLevel) values(3,0,'CK.StdAcl.Viewer',16);

-- The 4 Acl can be contributed by anybody...
insert into CK.[tAclConfig](AclId,ActorId,GrantLevel) values(4,0,32)
insert into CK.[tAclConfigMemory](AclId,ActorId,KeyReason,GrantLevel) values(4,0,'CK.StdAcl.Contributor',32);

-- The 5 Acl can be edited by anybody...
insert into CK.[tAclConfig](AclId,ActorId,GrantLevel) values(5,0,64)
insert into CK.[tAclConfigMemory](AclId,ActorId,KeyReason,GrantLevel) values(5,0,'CK.StdAcl.Editor',64);

-- The 6 Acl can be super-edited by anybody...
insert into CK.[tAclConfig](AclId,ActorId,GrantLevel) values(6,0,80)
insert into CK.[tAclConfigMemory](AclId,ActorId,KeyReason,GrantLevel) values(6,0,'CK.StdAcl.SuperEditor',80);

-- The 7 Acl can be safe-administrated by anybody...
insert into CK.[tAclConfig](AclId,ActorId,GrantLevel) values(7,0,112)
insert into CK.[tAclConfigMemory](AclId,ActorId,KeyReason,GrantLevel) values(7,0,'CK.StdAcl.SafeAdministrator',112);

-- The 8 Acl is reserved so that normal AclId starts at 9 and not 8.
-- Like 1, it gives no rights to anybody...except Gods.

--[endscript]
