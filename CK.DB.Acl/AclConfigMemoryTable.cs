using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Setup;
using CK.SqlServer.Setup;

namespace CK.DB.Acl
{
    [SqlTable( "tAclConfigMemory", Package = typeof( Package ) ), Versions( "1.0.0" )]
    [SqlObjectItem( "vAclConfigMemory" )]
    public abstract class AclConfigMemoryTable : SqlTable
    {
        void Construct( AclTable acl, Actor.ActorTable actor )
        {
        }
    }
}
