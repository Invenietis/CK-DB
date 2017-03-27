--[beginscript]

alter table CK.tGroup drop UK_CK_tGroup_GroupName;
alter table CK.tGroup add constraint UK_CK_tGroup_GroupName unique( ZoneId, GroupName );

--[endscript]