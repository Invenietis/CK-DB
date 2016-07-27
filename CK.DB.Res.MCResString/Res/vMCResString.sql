-- SetupConfig: {}
create view CK.vMCResString
as
	select  r.ResId, 
			c.XLCID,
			v.LCID,
			v.Value
		from CK.tRes r
		cross join CK.tXLCID c
		outer apply (select top(1) s.LCID, s.Value 
						from CK.tMCResString s
						inner join CK.tXLCIDMap m on m.LCID = s.LCID
						where s.ResId = r.ResId and m.XLCID = c.XLCID
						order by m.Idx) v
		where c.XLCID <> 0;