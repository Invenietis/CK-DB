-- SetupConfig: { }
-- 
create procedure CK.sAuthScopeSetCopy
(
	@ActorId int,
	@ScopeSetId int,
	@ForceWARStatus char = null,
	@ScopeSetIdResult int output
)
as 
begin
	--[beginsp]
	insert into CK.tAuthScopeSet default values;
	set @ScopeSetIdResult = Scope_Identity();
	insert into CK.tAuthScopeSetContent( ScopeSetId, ScopeId, WARStatus, WARStatusLastWrite )
		select  @ScopeSetIdResult, 
				ScopeId, 
				case when @ForceWARStatus is null then WARStatus else @ForceWARStatus end, 
				sysutcdatetime()
			from CK.tAuthScopeSetContent
			where ScopeSetId = @ScopeSetId;
	--[endsp]
end 
