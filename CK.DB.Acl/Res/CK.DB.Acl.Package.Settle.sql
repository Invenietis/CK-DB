
-- This ensures that a 'PlatformZone' configuration exists if the PlatformZone is defined (ActorId = 3).
if exists( select 1 from CK.tActor where ActorId = 3 )
begin
    -- Every member of the PlatformZone (Id=3) are "Viewer" on the System acl (n°1).
    exec CK.sAclGrantSet 1, @AclId = 1, @ActorIdToGrant = 3, @KeyReason = 'PlatformZone', @GrantLevel = 16;
end
