
-- The AdminZone, if defined must be the ActorId = 3.
-- This special zone is created by CK.DB.Zone.
-- 
if exists( select 1 from CK.tActor where ActorId = 3 )
begin
    -- If the ActorId = 3 is defined then it is the Zone, it must be a Group.
    if not exists( select 1 from CK.tGroup where GroupId = 3 ) throw 50000, 'Invalid ActorId = 3: Must be the AdminZone, the group 3 must exist.', 1;

    -- If the name is (still) the default GUID, we set the 'AdminZone' name.
    -- This is a default name that can be changed.
    if TRY_CONVERT(UNIQUEIDENTIFIER, (select GroupName from CK.tGroup where GroupId = 3) ) is not null
    begin
        exec CK.sGroupGroupNameSet 1, @GroupId = 3, @GroupName = N'AdminZone';
    end
end
