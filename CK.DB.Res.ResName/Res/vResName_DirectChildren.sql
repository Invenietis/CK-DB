-- SetupConfig: {}
create view CK.vResName_DirectChildren
as
select  ResId = r.ResId,
		ResName = r.ResName,
		c.ChildId,
		c.ChildName
	from CK.tResName r
	cross apply (select ChildId = ResId, 
						ChildName = ResName
					from CK.tResName 
					where ResName like r.ResName + '.%' and ResName not like r.ResName + '.%.%' ) c;

