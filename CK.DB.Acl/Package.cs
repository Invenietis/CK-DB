using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;

namespace CK.DB.Acl
{
    [SqlPackage( ResourcePath = "Res", Schema = "CK" )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "sUserDestroy, sGroupDestroy" )]
    public abstract class Package : SqlPackage
    {
        void Construct( Actor.Package actorPackage )
        {
        }

        [InjectContract]
        public AclTable AclTable { get; protected set; }

        [InjectContract] 
        public AclConfigTable AclConfigTable { get; protected set; }

        [InjectContract]
        public AclConfigMemoryTable AclConfigMemoryTable { get; protected set; }

    }
}
