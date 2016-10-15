--[beginscript]

alter table CK.tGroup drop DF_CK_tGroup_GrouName;

alter table CK.tGroup add constraint DF_CK_tGroup_GroupName default( newid() ) for GroupName;

--[endscript]