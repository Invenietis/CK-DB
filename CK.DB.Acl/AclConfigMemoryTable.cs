using CK.Core;

namespace CK.DB.Acl;

/// <summary>
/// This is an internal table that memorizes for each Acl and each Actor the GrantLevel with a reason.
/// As its name states, this is the memory of the Acl: the <see cref="AclConfigTable"/> is a projection
/// of this information with the Acl, Actor and Max(GrantLevel) whatever the reasons are.
/// </summary>
[SqlTable( "tAclConfigMemory", Package = typeof( Package ) )]
[Versions( "1.0.1, 1.0.2, 1.0.3" )]
[SqlObjectItem( "vAclConfigMemory" )]
public abstract class AclConfigMemoryTable : SqlTable
{
    void StObjConstruct( AclTable acl, Actor.ActorTable actor )
    {
    }
}
