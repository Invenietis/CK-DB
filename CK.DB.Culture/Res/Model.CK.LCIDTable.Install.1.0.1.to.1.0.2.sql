--[beginscript]

alter table CK.tLCID alter column Name varchar(20) collate LATIN1_General_100_BIN2 not null;
alter table CK.tLCID alter column EnglishName varchar(50) collate LATIN1_General_100_BIN2 not null;

--[endscript]
