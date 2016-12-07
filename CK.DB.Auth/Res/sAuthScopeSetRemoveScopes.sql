-- SetupConfig: {}
--
-- Removes scopes from a set of scopes.
-- - When @ScopeFilter is not null, it is the whitespace separated list of scopes to remove optionally prefixed by their WAR status.
-- - When @WARStatusFilter is not null, only the scopes with the corresponding status will be removed. 
--
create procedure CK.sAuthScopeSetRemoveScopes
(
	@ActorId int,
	@ScopeSetId int,
	@Scopes nvarchar(max) = null,
	@ScopesHaveStatus bit = 0,
	@DefaultWARStatus char = 'W',
	@WARStatusFilter char = null
)
as 
begin
	--[beginsp]

	if @Scopes is not null
	begin
		declare @xmlScopes xml = replace( N'<s>' + replace( rtrim(ltrim(@Scopes)), N' ', N'</s><s>') + N'</s>', N'<s></s>', N'' );
		declare @Names table( 
			ScopeName nvarchar(255) collate Latin1_General_100_BIN2 not null,
			WARStatus char not null
		);
		insert into @Names select distinct r.value('.','nvarchar(255)'), @DefaultWARStatus from @xmlScopes.nodes('/s') as records(r);
		if @ScopesHaveStatus = 1
		begin
			update @Names set 
				ScopeName = substring(ScopeName,4,len(ScopeName)-3), 
				WARStatus = substring(ScopeName,2,1)
				where ScopeName like N'[[][WAR]]%';
		end

		delete s from CK.tAuthScopeSetContent s
			   inner join CK.tAuthScope scope on scope.ScopeId = s.ScopeId
			   inner join @Names n on n.ScopeName = scope.ScopeName and n.WARStatus = s.WARStatus
			where s.ScopeSetId = @ScopeSetId and (@WARStatusFilter is null or @WARStatusFilter = s.WARStatus)
	end
	else
	begin
		delete s from CK.tAuthScopeSetContent s
			where s.ScopeSetId = @ScopeSetId and (@WARStatusFilter is null or @WARStatusFilter = s.WARStatus)
	end

	--[endsp]
end 
