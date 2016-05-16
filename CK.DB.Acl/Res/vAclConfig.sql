-- Version = *, Requires = { CK.vAclActor }
create view CK.vAclConfig --with SCHEMABINDING
as 
select 	ac.AclId, 
		ac.ActorId, 
		ac.GrantLevel,
		ResultingLevel = acl.GrantLevel
	from CK.tAclConfig ac
	inner join CK.vAclActor acl on acl.AclId = ac.AclId and acl.ActorId = ac.ActorId;
