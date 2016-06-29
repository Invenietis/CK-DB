-- SetupConfig: { "Requires": [ "CK.vAclActor" ] }
alter view CK.vAclConfigMemory --with SCHEMABINDING
as 
select 	ac.AclId, 
		ac.ActorId, 
		ac.GrantLevel,
		ResultingLevel = acl.GrantLevel,
		KeyReason = ac.KeyReason
	from CK.tAclConfigMemory ac
	inner join CK.vAclActor acl on acl.AclId = ac.AclId and acl.ActorId = ac.ActorId



