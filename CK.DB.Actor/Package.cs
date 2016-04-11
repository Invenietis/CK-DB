using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.SqlServer.Setup;
using CK.Setup;

namespace CK.DB.Actor
{

    [SqlPackage( Schema = "CK", Database = typeof( SqlDefaultDatabase ), ResourcePath = "Res" ), Versions( "5.0.0" )]
    public abstract class Package : SqlPackage
    {
        [InjectContract]
        public GroupTable GroupTable { get; protected set; }

        [InjectContract]
        public UserTable UserTable { get; protected set; }

        [InjectContract]
        public ActorProfileTable ActorProfileTable { get; protected set; }
    }
}
