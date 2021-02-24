# CK.DB.Zone

## Well-known Actor identifiers

Actor identifiers from 0 to 3 have predefined semantics. These well-known values are "const" and it is perfectly valid to use
this ActorId = 0, 1, 2 or 3 wherever the related entity must be referenced.

Please see [CK.DB.Acl](../CK.DB.Acl) for well-known Acl identifiers (the "Standard Acls") and the
relationship between these Actor identifiers and the standard Acls.

### ActorId 0: The Anonymous

The 0 is the lack of any authentication, it conveys the "Anonymous"/"All Users"/"Unknown User" semantics.
For Groups (and Zones), it is the default group to which logically every actor belongs (this "inclusion" doesn't appear
in the CK.tActorProfile table).

### ActorId 1: The System, the Gods

This actor represents the System itself (you can imagine that 0 - the Anonymous - is the "outside", 1 is the "inside").
The System User (that should not be used as a login) should have absolutely no limitation of any kind on any
part of the System, that's why this System user may sometimes be named "God". 
The ActorId 1 is also a Group, the System group: members of this group are like the user System, they should not be
limited in any way (members of the System group are sometimes refereed as "Gods").

### ActorId 2: The Platform Administrators

The [CK.DB.Actor](../CK.DB.Actor) package creates an "Administrators" group which is group #2
This group is the group of the global Administrators.
Members of this group should have a lot of power on the platform, but are not necessarily as powerful as member
of the System group (of course, if a user belongs to both groups, it is a God).

### ActorId 3: The Platform Zone

*Reminder:* A Zone is a Group (which is an Actor) that contains other Groups.

When the **CK.DB.Zone** package is installed, the Zone table [Settle script](Res/CK.ZoneTable.Settle.sql)
creates the "Platform Zone". This Zone must be used as generic Zone that defines/contains groups
that make sense for the whole system.

