--[beginscript]

create table CK.tAcl
(
	AclId int not null identity (0, 1),
	constraint PK_CK_tAcl primary key clustered (AclId)
);

-- Acl from 0 to 7 are reserved for the defaults
insert into CK.[tAcl] default values; -- 0 - Administrator level to anybody.
insert into CK.[tAcl] default values; -- 1 - No rights to anybody (except memeber on System Group 1).  
insert into CK.[tAcl] default values; -- 2 - User level to anybody
insert into CK.[tAcl] default values; -- 3 - Viewer level to anybody.
insert into CK.[tAcl] default values; -- 4 - Contributor level to anybody.
insert into CK.[tAcl] default values; -- 5 - Editor level to anybody.
insert into CK.[tAcl] default values; -- 6 - Super editor to anybody.
insert into CK.[tAcl] default values; -- 7 - SafeAdministrator level to anybody.
insert into CK.[tAcl] default values; -- 8 - Reserved (no rights, like 1).

--[endscript]
