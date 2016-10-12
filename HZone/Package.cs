using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;

namespace CK.DB.Zone.HZone
{
    [SqlPackage( FullName = "HZonePackage", ResourceType = typeof( Package ), ResourcePath = "~CK.DB.Zone.HZone.Res" ), Versions( "1.0.0" )]
    [SqlObjectItem( "vZone" )]
    public abstract class Package : Zone.Package
    {
        void Construct( SecuredZone.Package aclpackage )
        {
        }

        [InjectContract]
        public new ZoneTable ZoneTable { get { return (ZoneTable)base.ZoneTable; } }
    }
}
