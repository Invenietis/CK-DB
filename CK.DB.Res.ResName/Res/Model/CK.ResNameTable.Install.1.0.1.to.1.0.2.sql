--[beginscript]

alter table CK.tResName drop constraint CK_FK_ResName_ResId;

alter table CK.tResName add constraint FK_CK_ResName_ResId foreign key (ResId) references CK.tRes( ResId );

--[endscript]