declare @ActorId int;
declare @AclId int;
declare @KeyReason varchar(128) = '';

declare InvalidConfigsCursor cursor local static read_only forward_only
    for select ActorId, AclId, KeyReason from CK.tAclConfigMemory where GrantLevel = 0;

open InvalidConfigsCursor;

fetch next from InvalidConfigsCursor into @ActorId, @AclId, @KeyReason;
while @@FETCH_STATUS = 0
begin
    exec CK.sAclGrantSet 1, @AclId, @ActorId, @KeyReason, 0;

    fetch next from InvalidConfigsCursor into @ActorId, @AclId, @KeyReason;
end

close InvalidConfigsCursor;
deallocate InvalidConfigsCursor;
