-- SetupConfig: {}
create view CK.vGroup
as 
	select  GroupId,
			CreationDate,
			GroupName = N'#Group-' +  cast( GroupId as varchar)
		from CK.tGroup g;