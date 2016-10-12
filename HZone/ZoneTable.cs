using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;

namespace CK.DB.Zone.HZone
{
    [SqlTable( "tZone", Package=typeof(Package) ), Versions( "1.0.0" )]
    [SqlObjectItem( "sZoneDestroy, vZone, sZoneMove" )]
    public abstract class ZoneTable : CK.DB.Zone.ZoneTable
    {
        [SqlProcedureNonQuery( "CK.sZoneCreate" )]
        public abstract int CreateZone( ISqlCallContext ctx, int actorId, int parentZoneId );
    }
}
