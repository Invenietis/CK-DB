using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;

namespace CK.DB.HZone
{
    [SqlPackage( ResourcePath = "Res", ResourceType = typeof( Package ) ) ]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sGroupUserAdd, transform:vZone" )]
    [SqlObjectItem( "vZoneDirectChildren, vZoneAllChildren" )]
    public abstract class Package : Zone.Package
    {
        void Construct( Zone.Package zone )
        {
        }

        public new ZoneTable ZoneTable => (ZoneTable)base.ZoneTable;
    }
}
