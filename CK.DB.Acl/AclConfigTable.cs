using CK.Core;

namespace CK.DB.Acl
{
    /// <summary>
    /// Base table that holds for each Acl and each Actor the configured GrantLevel.
    /// </summary>
    [SqlTable( "tAclConfig", Package = typeof( Package ) ), Versions( "1.0.0" )]
    [SqlObjectItem( "vAclConfig" )]
    public abstract class AclConfigTable : SqlTable
    {
        void StObjConstruct( AclConfigMemoryTable acl, Actor.ActorTable actor )
        {
        }
    }
}
