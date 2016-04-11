using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.SqlServer.Setup;

namespace CK.DB.Acl
{
    [SqlView( "vAclConfigMemory", Package = typeof( Package ) )]
    public abstract class AclConfigMemoryView : SqlView
    {
        void Construct( AclConfigMemoryTable aclConfigMemory, AclActorView aclActorView, Actor.GroupTable group, Actor.UserTable user )
        {
        }
    }
}
