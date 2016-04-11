using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Setup;
using CK.SqlServer.Setup;

namespace CK.DB.Acl
{
    [SqlTable( "tAclConfig", Package = typeof( Package ) ), Versions( "1.0.0" )]
    public abstract class AclConfigTable : SqlTable
    {
        void Construct( AclTable acl, Actor.ActorTable actor )
        {
        }
    }
}
