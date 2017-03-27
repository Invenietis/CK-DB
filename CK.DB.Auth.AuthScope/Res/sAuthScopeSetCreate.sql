-- SetupConfig: { "Requires" : "CK.sAuthScopeSetAddScopes" }
-- 
create procedure CK.sAuthScopeSetCreate
(
	@ActorId int,
	@InitScopes nvarchar(max) = null,
	@InitScopesHaveStatus bit = 0,
	@InitDefaultWARStatus char = 'W',
	@ScopeSetIdResult int output
)
as 
begin
	--[beginsp]
	insert into CK.tAuthScopeSet default values;
	set @ScopeSetIdResult = Scope_Identity();
	if @InitScopes is not null
	begin
		exec CK.sAuthScopeSetAddScopes @ActorId, @ScopeSetIdResult, @InitScopes, @InitScopesHaveStatus, @InitDefaultWARStatus;
	end
	--[endsp]
end 
