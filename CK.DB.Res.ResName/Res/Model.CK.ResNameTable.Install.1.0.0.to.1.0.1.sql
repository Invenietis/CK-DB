--[beginscript]

drop index IX_CK_ResName_ResName on CK.tResName;

alter table CK.tResName alter column ResName varchar(128) collate Latin1_General_100_BIN2 not null;

create unique clustered index IX_CK_ResName_ResName on CK.tResName( ResName );

--[endscript]