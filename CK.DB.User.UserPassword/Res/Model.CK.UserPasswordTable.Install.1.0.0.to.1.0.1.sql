--[beginscript]

alter table CK.tUserPassword add LastLoginTime datetime2(2) not null constraint DF_TEMP default( sysutcdatetime() );
alter table CK.tUserPassword drop constraint DF_TEMP;

--[endscript]