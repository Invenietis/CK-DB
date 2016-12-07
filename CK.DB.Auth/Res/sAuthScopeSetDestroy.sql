-- SetupConfig: {}
-- 
create procedure CK.sAuthScopeSetDestroy
(
	@ActorId int,
	@ScopeSetId int
)
as 
begin
	--[beginsp]
	delete from CK.tAuthScopeSetContent where ScopeSetId = @ScopeSetId;
	delete from CK.tAuthScopeSet where ScopeSetId = @ScopeSetId;
	--[endsp]
end 
