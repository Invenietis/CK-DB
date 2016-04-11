using CK.SqlServer.Setup;

namespace CK.DB.Acl
{
    [SqlView( "vAclActor", Package = typeof( Package ) )]
    public abstract class AclActorView : SqlView
    {
        void Construct( AclTable acl, Actor.ActorTable actor, Actor.ActorProfileTable actorProfile, AclConfigTable aclConfig )
        {
        }
    }
}
