-- Every member of this Administrator group (Id=2) are "Administrator" on the System acl.
exec CK.sAclGrantSet 1, @AclId = 1, @ActorIdToGrant = 2, @KeyReason = 'AdministratorsGroup', @GrantLevel = 127;


