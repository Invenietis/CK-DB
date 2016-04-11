using CK.SqlServer.Setup;

namespace CK.DB.Acl
{
    [SqlView( "vAclActorReason", Package = typeof( Package ) )]
    public abstract class AclActorReasonView : SqlView
    {
        void Construct( AclTable acl, CK.DB.Actor.ActorTable actor, CK.DB.Actor.ActorProfileTable actorProfile, AclConfigTable aclConfig )
        {
        }
    }
}
