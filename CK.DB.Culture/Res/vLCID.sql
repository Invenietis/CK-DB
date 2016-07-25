-- SetupConfig:{ "Requires": [ "CK.vXLCID" ] }
create view CK.vLCID
as
	select  c.LCID, 
			c.Name,
			c.EnglishName,
			c.NativeName,
			x.Fallbacks,
			x.FallbacksLCID,
			x.FallbacksNames,
			x.FallbacksEnglishNames,
			x.FallbacksNativeNames
		from CK.tLCID c
		inner join CK.vXLCID x on x.XLCID = c.LCID;
