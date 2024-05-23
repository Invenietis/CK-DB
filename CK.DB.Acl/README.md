# CK.DB.Acl

This package introduces Access Control List, abbreviated in Acl that is an **Authorization layer** implementation.

Acls are simple entities identified their **`AclId`**. They can be referenced and used by any other
participant that aims to "protect" something: an action ("*Does the caller can destroy this object?*") as well as a datum
("*Can the caller see the Salary column of this Employee?*").

Before detailing the implementation, let's take a look at the two majors way to implement an Authorization
layer under a relational database (but not only: these are very general approaches).

## Security By Roles and Row Level Security

Any Authorization Layer belong to one these 2 approaches:

- **Security By Roles** : Roles are Groups of Users that define the authorization itself. 
Roles are for instance: "Administrators", "ContentMaster", "Editor", "NewsLetterValidator", etc. 
This is simple end effective for small systems (and you will find this Roles in numerous applications), but there
is one major drawback: a Role protects a "feature" of the System, not individual items.

Sql Server security itself works this way: one can define new [User defined Database Roles](https://docs.microsoft.com/en-us/sql/relational-databases/security/authentication-access/getting-started-with-database-engine-permissions?#user-defined-database-roles):
and adds users to them. 

And these roles are then granted or denied specific authorizations:




- **Row Level Security** : In this approach, a mechanism protects . 


## The GrantLevel ladder



## The well-known Acl identifiers

The [tAcl table install script](Res/Model/CK.AclTable.Install.3.0.0.sql) creates 9 standard acls:
```sql
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
```

The comments above are self-explanatory (They are configured in the [tAclConfig table install script](Res/Model/CK.AclConfigTable.Install.1.0.0.sql).)

Among them, the System AclId = 1 is the most interesting (since it is the only one that can be modified).
- The "Administrators" group (ActorId = 2) is granted access level 127 (Administrator) on it.
- If CK.DB.Zone is installed, then the "AdminZone" (ActorId = 3) is has a "Viewer" access level on it.

See here for a presentation of the well-known Actor identifiers: [CK.DB.Zone](../CK.DB.Zone).


