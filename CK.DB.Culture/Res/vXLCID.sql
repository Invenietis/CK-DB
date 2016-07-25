-- SetupConfig: {}
create view CK.vXLCID
as
	select  c.XLCID, 
			Fallbacks = Stuff((select N'#' + convert(varchar(5),m.LCID)+'|'+n.Name+'|'+n.EnglishName+'|'+n.NativeName  
										from CK.tXLCIDMap m 
										inner join CK.tLCID n on n.LCID = m.LCID 
										where m.XLCID = c.XLCID
										order by m.Idx for xml path(''),TYPE).value('text()[1]','nvarchar(max)'),1,1,N''),
			FallbacksLCID = Stuff((select ',' + convert(varchar(5),LCID) 
										from CK.tXLCIDMap 
										where XLCID = c.XLCID 
										order by Idx for xml path(''),TYPE).value('text()[1]','varchar(max)'),1,1,N''),
			FallbacksNames = Stuff((select ',' + n.Name 
										from CK.tXLCIDMap m 
										inner join CK.tLCID n on n.LCID = m.LCID 
										where m.XLCID = c.XLCID 
										order by m.Idx for xml path(''),TYPE).value('text()[1]','varchar(max)'),1,1,N''),
			FallbacksEnglishNames = Stuff((select '#' + n.EnglishName 
												from CK.tXLCIDMap m 
												inner join CK.tLCID n on n.LCID = m.LCID 
												where m.XLCID = c.XLCID 
												order by m.Idx for xml path(''),TYPE).value('text()[1]','varchar(max)'),1,1,N''),
			FallbacksNativeNames = Stuff((select N'#' + n.NativeName 
												from CK.tXLCIDMap m 
												inner join CK.tLCID n on n.LCID = m.LCID 
												where m.XLCID = c.XLCID 
												order by m.Idx for xml path(''),TYPE).value('text()[1]','nvarchar(max)'),1,1,N'')
		from CK.tXLCID c
		where c.XLCID <> 0;
