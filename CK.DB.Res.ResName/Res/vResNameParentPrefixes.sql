-- SetupConfig: { "Requires": [ "CK.fResNamePrefixes" ] }
create view CK.vResNameParentPrefixes 
as 
	select  r.ResId,
			r.ResName,
			-- Null if the ParentPrefix is not an existing resource.
			ParentResId = pExist.ResId,
			-- Parent prefix that may exist as an actual resource or not.
			p.ParentPrefix,
			-- Level of the ParentPrefix. First parent has level 1.
			p.ParentLevel
		from CK.tResName r
		cross apply CK.fResNamePrefixes( r.ResName ) p
		left outer join CK.tResName pExist on pExist.ResName = p.ParentPrefix;
