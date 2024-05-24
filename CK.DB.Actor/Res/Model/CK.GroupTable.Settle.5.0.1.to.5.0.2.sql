--[beginscript]

-- Introduction of the Administrators group: the n°2.
declare @MovedId int;
declare @StrValue nvarchar(max);

-- If an ActorId n°2 exists, we move it (whatever it is).
if exists( select * from CK.tActor where ActorId = 2 )
begin

    --
    insert into CK.tActor default values;
	set @MovedId = SCOPE_IDENTITY();
    set @StrValue = cast( @MovedId as nvarchar(30) );

    exec CKCore.sRefBazookation @SchemaName = 'CK',      
                                @TableName = 'tActor',   
                                @ColumnName = 'ActorId', 
                                @ExistingValue = '2',    
                                @NewValue = @StrValue;
    -- We let the ActorId = 2 in the table.
end
else
begin
    set IDENTITY_INSERT CK.tActor on;
    insert into CK.tActor( ActorId ) values( 2 );
    set IDENTITY_INSERT CK.tActor off;
end
-- Now creates a "normal" group.
exec CK.sGroupCreate 1, @GroupIdResult = @MovedId output;
-- And let this group be the "Administrators" group n°2.
set @StrValue = cast( @MovedId as nvarchar(30) );

exec CKCore.sRefBazookation @SchemaName = 'CK',      
                            @TableName = 'tActor',   
                            @ColumnName = 'ActorId', 
                            @ExistingValue = @StrValue,    
                            @NewValue = '2';

-- If an ActorId n°3 exists, we move it (whatever it is).
-- The n°3 is deleted: it is up to the CK.DB.Zone to set the "AdminZone" here.
if exists( select * from CK.tActor where ActorId = 3 )
begin
    --
    insert into CK.tActor default values;
	set @MovedId = SCOPE_IDENTITY();
    set @StrValue = cast( @MovedId as nvarchar(30) );

    exec CKCore.sRefBazookation @SchemaName = 'CK',      
                                @TableName = 'tActor',   
                                @ColumnName = 'ActorId', 
                                @ExistingValue = '3',    
                                @NewValue = @StrValue;
    delete CK.tActor where ActorId = 3;
end

--[endscript]
     



