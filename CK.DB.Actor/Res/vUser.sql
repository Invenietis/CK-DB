-- SetupConfig: {}
create view CK.vUser
as 
	select  u.UserId,
			u.CreationDate,
			u.UserName
		from CK.tUser u;