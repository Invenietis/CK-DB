
-- Fix spelling.
alter table CK.tAuthScope drop constraint UK_CK_AUthScope_ScopeName;
alter table CK.tAuthScope drop constraint CK_CK_AUthScope_ScopeName;

alter table CK.tAuthScope add constraint UK_CK_AuthScope_ScopeName unique (ScopeName);
alter table CK.tAuthScope add constraint CK_CK_AuthScope_ScopeName check (CHARINDEX(N' ',ScopeName) = 0)
