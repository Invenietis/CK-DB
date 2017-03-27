--[beginscript]

alter table CK.tUserGoogle add LastLoginTime datetime2(2) not null constraint DF_TEMP default( sysutcdatetime() );
alter table CK.tUserGoogle drop constraint DF_TEMP;

--[endscript]