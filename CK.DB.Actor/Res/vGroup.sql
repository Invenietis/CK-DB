-- Version = *
create view CK.vGroup
as 
	select  GroupId,
			CreationDate,
			GroupName = N'#Group-' + GroupId
		from CK.tGroup g;