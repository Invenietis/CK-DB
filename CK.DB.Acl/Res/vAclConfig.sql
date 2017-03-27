-- SetupConfig: { "Requires": [ "CK.vAclActor" ] }
-- This view gives, along with the resulting level, the configuration for each Actor and each Acl.
-- This view is based on the tAclConfig table: each (ActorId,AclId) appear multiple times, once by configuration.
-- The vAclConfigMemory view gives more information. 
create view CK.vAclConfig --with SCHEMABINDING
as 
select 	ac.AclId, 
		ac.ActorId, 
		ac.GrantLevel,
		ResultingLevel = acl.GrantLevel
	from CK.tAclConfig ac
	inner join CK.vAclActor acl on acl.AclId = ac.AclId and acl.ActorId = ac.ActorId;
