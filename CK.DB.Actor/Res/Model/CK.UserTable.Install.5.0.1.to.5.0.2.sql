--[beginscript]

alter table CK.tUser drop constraint UK_CK_tUser_UserName;
alter table CK.tUser alter column UserName nvarchar( 255 ) collate Latin1_General_CI_AS not null;
alter table CK.tUser add constraint UK_CK_tUser_UserName unique (UserName);

--[endscript]

