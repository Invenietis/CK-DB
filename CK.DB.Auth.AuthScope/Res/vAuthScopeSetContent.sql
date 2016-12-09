-- SetupConfig: { }
-- 
create view CK.vAuthScopeSetContent
as
	select	c.ScopeSetId, 
			n.ScopeName,
			c.WARStatus,
			c.WARStatusLastWrite,
			c.ScopeId
	from CK.tAuthScopeSetContent c
	inner join CK.tAuthScope n on n.ScopeId = c.ScopeId