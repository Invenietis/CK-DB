--[beginscript]

-- Creates the default template scope set for new users.
declare @DefaultScopeSetId int;
exec CK.sAuthScopeSetCreate 1, N'openid', @ScopeSetIdResult = @DefaultScopeSetId output;
update CK.tUserGoogle set ScopeSetId = @DefaultScopeSetId where UserId = 0;

-- Replicates the default template on all existing UserGoogle.
declare @UserId int;
declare @CUser cursor;
set @CUser = cursor local fast_forward for 
	select UserId from CK.tUserGoogle u where u.ScopeSetId = 0;
open @CUser;
fetch from @CUser into @UserId;
while @@FETCH_STATUS = 0
begin
	declare @NewScopeId int;
	exec CK.sAuthScopeSetCopy @ActorId = 1, @ScopeSetId = @DefaultScopeSetId, @ForceWARStatus = 'W', @ScopeSetIdResult = @NewScopeId output
	update CK.tUserGoogle set ScopeSetId = @NewScopeId where UserId = @UserId;
	fetch next from @CUser into @UserId;
end
deallocate @CUser;

-- Now that each Google user has a dedicated ScopeSet, we can ensure its unicity
-- so that no two users can share the same ScopeSet.
alter table CK.tUserGoogle add
	constraint UK_CK_UserGoogle_ScopeSetId unique( ScopeSetId )

--[endscript]