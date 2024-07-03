--[beginscript]

-- Introduction of the "AdminZone": the n°3.
-- CK.DB.Actor reserved the space.
if not exists( select 1 from CK.tActor where ActorId = 3 ) 
begin    
    declare @MovedId int;
    exec CK.sZoneCreate 1, @MovedId output;
    declare @StrValue nvarchar(max);
    set IDENTITY_INSERT CK.tActor on;
    insert into CK.tActor( ActorId ) values( 3 );
    set IDENTITY_INSERT CK.tActor off;
    set @StrValue = cast( @MovedId as nvarchar(30) );
    exec CKCore.sRefBazookation @SchemaName = 'CK',      
                                @TableName = 'tActor',   
                                @ColumnName = 'ActorId', 
                                @ExistingValue = @StrValue,    
                                @NewValue = '3';
    -- We move the 'Administrators' (Id=2) group into the PlatformZone, auto registering its potential
    -- users in the AdminZone.
    exec CK.sGroupMove 1, @GroupId = 2, @NewZoneId = 3, @Option = 2 /*AutoUserRegistration*/;

    -- Requires at least CK.DB.Group.SimpleNaming.
    -- We set the 'AdminZone' name. This is a default name that can be changed.
    -- Note that this code is also in the CK.DB.Group.SimpleNaming.Package.Settle script: if the naming
    -- is installed after the Zone support, the 'AdminZone' name will be correctly initialized.
    if object_id('CK.sGroupGroupNameSet') is not null
    begin
        exec CK.sGroupGroupNameSet 1, @GroupId = 3, @GroupName = N'AdminZone';
    end

    -- Requires CK.DB.Acl
    if object_id('CK.sAclGrantSet') is not null
    begin
        -- Every member of the PlatformZone (Id=3) are "Viewer" on the System Acl (n°1).
        -- Note that this code is also in CK.DB.Acl.Package.Settle script: if the Acl support
        -- is installed after the Zone support, the Acl n°1 will also be configured like here.
        exec CK.sAclGrantSet 1, @AclId = 1, @ActorIdToGrant = 3, @KeyReason = 'AdminZone', @GrantLevel = 16;
    end
end


--[endscript]
