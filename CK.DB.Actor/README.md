# CK.DB.Actor

This is the ultimate dependency of numerous other packages. It implements a minimalist model that
handles Users and Groups that some packages extend:
- [CK.DB.Zone](../CK.DB.Zone): Adds the support of Zones that are Groups and contains a set of Groups. Zones implement a 
 one-level only group hierarchies/
- [CK.DB.HZone](../CK.DB.HZone): Extends the CK.DB.Zone to be hierarchical. Thanks to this package, Zones (that are Groups) can be 
subordinated to a parent Zone.

- [CK.DB.SimpleGroupNaming](../CK.DB.SimpleGroupNaming): Adds a simple `nvarchar(128) GroupName` column to the tGroup table.

- [CK.DB.Acl](../CK.DB.Acl): Introduces Access Control Lists.

CK.DB.Workspace (in its own repository), introduces the notion of Workspace. a Workspace is a Zone that has an Acl identifier 
and and an Administrator group.
