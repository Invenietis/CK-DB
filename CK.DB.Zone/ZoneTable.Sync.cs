using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.SqlServer.Setup;
using CK.Setup;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CK.SqlServer;

namespace CK.DB.Zone
{
    public abstract partial class ZoneTable
    {

        [SqlProcedure( "CK.sZoneCreate" )]
        public abstract int CreateZone( ISqlCallContext ctx, int actorId );

        [SqlProcedure( "CK.sZoneDestroy" )]
        public abstract void DestroyZone( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false );

        [SqlProcedure( "sZoneUserAdd" )]
        public abstract void AddUser( ISqlCallContext ctx, int actorId, int zoneId, int userId );

        [SqlProcedure( "sZoneUserRemove" )]
        public abstract void RemoveUser( ISqlCallContext ctx, int actorId, int zoneId, int userId );

    }
}
