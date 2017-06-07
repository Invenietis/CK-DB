--[beginscript]

alter table CK.tAuthProvider add 
    	IsMultiScheme bit not null constraint DF_TEMP default(0);
alter table CK.tAuthProvider drop constraint DF_TEMP;

alter table CK.tAuthProvider add 
	constraint CK_CK_AuthProvider_ProviderName check (CharIndex( '.', ProviderName ) = 0);

--[endscript]