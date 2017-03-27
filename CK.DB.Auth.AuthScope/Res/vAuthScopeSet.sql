-- SetupConfig: {}
-- 
create view CK.vAuthScopeSet
as
	select	s.ScopeSetId, 
			Scopes = isnull(Stuff((select ' ' + n.ScopeName 
										from CK.tAuthScope n
										inner join CK.tAuthScopeSetContent c on c.ScopeId = n.ScopeId
										where c.ScopeSetId = s.ScopeSetId 
										order by n.ScopeName for xml path(''),TYPE).value('text()[1]','nvarchar(max)'),1,1,N''), N''),
			ScopesWithStatus = isnull(Stuff((select ' ' + N'[' + cast(c.WARStatus as nvarchar) + N']' + n.ScopeName 
												from CK.tAuthScope n
												inner join CK.tAuthScopeSetContent c on c.ScopeId = n.ScopeId
												where c.ScopeSetId = s.ScopeSetId 
												order by n.ScopeName for xml path(''),TYPE).value('text()[1]','nvarchar(max)'),1,1,N''), N''),
			MaxWARStatusWrite = (select max(c.WARStatusLastWrite) from CK.tAuthScopeSetContent c where c.ScopeSetId = s.ScopeSetId)
	from CK.tAuthScopeSet s