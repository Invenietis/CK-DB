-- SetupConfig: {}
-- 
-- Sets or adds whitespace separated scopes. 
-- When @ScopesHaveStatus is 1, scopes in @Scopes can be prefixed by status (like N'[A]openid [W]user:email').
-- When @ScopesHaveStatus is 0 or when no [W], [A] or [R] prefix appears, @DefaultWARStatus is used.
-- When @ResetScopes is 1, existing scopes are cleared.
--
create procedure CK.sAuthScopeSetAddScopes
(
	@ActorId int,
	@ScopeSetId int,
	@Scopes nvarchar(max),
	@ScopesHaveStatus bit,
	@DefaultWARStatus char = 'W',
	@ResetScopes bit = 0
)
as 
begin
	if @Scopes = null throw 50000, 'Argument.NullScopes', 1;
	if @ScopesHaveStatus = null throw 50000, 'Argument.NullScopesHaveStatus', 1;
	if @DefaultWARStatus = null throw 50000, 'Argument.NullDefaultWARStatus', 1;
	if @ResetScopes = null throw 50000, 'Argument.NullResetScopes', 1;

	--[beginsp]
	if exists( select 1 from CK.tAuthScopeSet where ScopeSetId = @ScopeSetId )
	begin
		-- Transforms the white space separated string of scopes into a table of names.
		-- The outer replace removes empty elements.
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

		-- Creates the scope names that do not exist yet.
		merge CK.tAuthScope as target
			using( select ScopeName from @Names ) as source 
			on target.ScopeName = source.ScopeName
			when not matched by target then insert( ScopeName ) values( source.ScopeName );

		-- Updates the scope set content.
		merge CK.tAuthScopeSetContent as target
			using( select s.ScopeId, n.WARStatus from CK.tAuthScope s inner join @Names n on n.ScopeName = s.ScopeName ) as source
			on target.ScopeSetId = @ScopeSetId and target.ScopeId = source.ScopeId
			when not matched by source and @ResetScopes = 1 
					then delete
			when matched and target.WARStatus <> source.WARStatus 
					then update set WARStatus = source.WARStatus, WARStatusLastWrite = sysutcdatetime()
			when not matched 
					then insert( ScopeSetId, ScopeId, WARStatus, WARStatusLastWrite )
							values( @ScopeSetId, source.ScopeId, source.WARStatus, sysutcdatetime() );
	end
	--[endsp]
end 
