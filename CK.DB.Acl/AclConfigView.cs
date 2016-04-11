using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.SqlServer.Setup;

namespace CK.DB.Acl
{
    [SqlView( "vAclConfig", Package = typeof( Package ) )]
    public abstract class AclConfigView : SqlView
    {
        void Construct( AclConfigTable aclConfig, AclActorView aclActorView, Actor.GroupTable group, Actor.UserTable user )
        {
        }
    }
}
