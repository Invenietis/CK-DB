-- SetupConfig: { } 
-- 
create view CK.vUserAuthProvider( UserId, ProviderName, LastLoginTime )
as 
	select 0, 'Fake', sysutcdatetime() from CKCore.tSystem where 1=0
