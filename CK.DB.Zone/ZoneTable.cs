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
    [SqlTable( "tZone", Package = typeof( Package ) )]
    [Versions( "5.0.0" )]
    [SqlObjectItem( "vZone" )]
    public abstract partial class ZoneTable : SqlTable
    {
        void Construct( Actor.GroupTable group )
        {
        }

        [SqlProcedure( "CK.sZoneCreate" )]
        public abstract Task<int> CreateZoneAsync( ISqlCallContext ctx, int actorId );

        [SqlProcedure( "CK.sZoneDestroy" )]
        public abstract Task DestroyZoneAsync( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false );

        [SqlProcedure( "sZoneUserAdd" )]
        public abstract Task AddUserAsync( ISqlCallContext ctx, int actorId, int zoneId, int userId );

        [SqlProcedure( "sZoneUserRemove" )]
        public abstract Task RemoveUserAsync( ISqlCallContext ctx, int actorId, int zoneId, int userId );
    }
}
