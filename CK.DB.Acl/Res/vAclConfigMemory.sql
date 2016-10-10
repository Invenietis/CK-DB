-- SetupConfig: { "Requires": [ "CK.vAclActor" ] }
-- This view gives, along with the resulting level, the configuration reason for each Actor and each Acl.
-- This is the most detailed view, based on the tAclConfigMemory table.
-- Each (ActorId,AclId) appear multiple times, once by actual configuration with its KeyReason string.
alter view CK.vAclConfigMemory --with SCHEMABINDING
as 
select 	ac.AclId, 
		ac.ActorId, 
		ac.GrantLevel,
		ResultingLevel = acl.GrantLevel,
		KeyReason = ac.KeyReason
	from CK.tAclConfigMemory ac
	inner join CK.vAclActor acl on acl.AclId = ac.AclId and acl.ActorId = ac.ActorId;



