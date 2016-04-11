--[beginscript]

create table CK.tAcl
(
	AclId int not null identity (0, 1),
	constraint PK_CK_tAcl primary key clustered (AclId)
);

-- Acl from 0 to 7 are reserved for the defaults
insert into CK.[tAcl] default values; -- 0
insert into CK.[tAcl] default values; -- 1
insert into CK.[tAcl] default values; -- 2
insert into CK.[tAcl] default values; -- 3
insert into CK.[tAcl] default values; -- 4
insert into CK.[tAcl] default values; -- 5
insert into CK.[tAcl] default values; -- 6
insert into CK.[tAcl] default values; -- 7

--[endscript]
