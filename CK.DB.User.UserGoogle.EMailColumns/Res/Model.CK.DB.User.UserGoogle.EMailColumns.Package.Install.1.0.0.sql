--[beginscript]

-- Since we now uses Poco feature, this test skips the fields creation
-- for databases that previously used the specialized UserGoogleWithEMailTable.
if not exists( select * from CKCore.tItemVersionStore where FullName = '[]db^CK.UserGoogleWithEMailTable' )
begin
	alter table CK.tUserGoogle add
		EMail nvarchar( 255 ) collate Latin1_General_100_CI_AS not null constraint DF_TEMP1 default(N''),
		EMailVerified bit not null constraint DF_TEMP2 default(0);

	alter table CK.tUserGoogle drop constraint DF_TEMP1;
	alter table CK.tUserGoogle drop constraint DF_TEMP2;
end

--[endscript]