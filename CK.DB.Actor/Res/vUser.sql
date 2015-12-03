-- Version = *
create view CK.vUser
as 
	select  UserId,
			CreationDate,
			UserName
		from CK.tUser;