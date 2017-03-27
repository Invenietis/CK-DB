-- SetupConfig:{}
create view CK.vRes
as
	select ResId = r.ResId,
		   ResName = 'Auto.' + cast( r.ResId as varchar)
	from CK.tRes r