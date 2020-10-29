--[beginscript]

create table CK.tAcl
(
	AclId int not null identity (0, 1),
	constraint PK_CK_tAcl primary key clustered (AclId)
);

-- Acl from 0 to 8 are preallocated for the defaults:
-- None of them can be destroyed (sAclDestroy), only the 1 can be edited (sAclGrantSet).
insert into CK.[tAcl] default values; -- 0 - Administrator level to anybody.
insert into CK.[tAcl] default values; -- 1 - This is the "System Acl": it is the only one that can be configured.  
insert into CK.[tAcl] default values; -- 2 - User level to anybody
insert into CK.[tAcl] default values; -- 3 - Viewer level to anybody.
insert into CK.[tAcl] default values; -- 4 - Contributor level to anybody.
insert into CK.[tAcl] default values; -- 5 - Editor level to anybody.
insert into CK.[tAcl] default values; -- 6 - Super editor to anybody.
insert into CK.[tAcl] default values; -- 7 - SafeAdministrator level to anybody.
insert into CK.[tAcl] default values; -- 8 - Blind to anybody (no rights at all).

--[endscript]
