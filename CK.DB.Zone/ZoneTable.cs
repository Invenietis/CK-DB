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
    [SqlTable( "tZone", Package = typeof( Package ) ), Versions( "3.0.0" )]
    [SqlObjectItem( "vZone" )]
    public abstract class ZoneTable : SqlTable
    {
        void Construct( Actor.GroupTable group )
        {
        }

        [SqlProcedureNonQuery( "CK.sZoneCreate" )]
        public abstract int CreateZone( ISqlCallContext ctx, int actorId );

        [SqlProcedureNonQuery( "CK.sZoneCreate" )]
        public abstract Task<int> CreateZoneAsync( ISqlCallContext ctx, int actorId );

        [SqlProcedureNonQuery( "CK.sZoneDestroy" )]
        public abstract void DestroyZone( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false );

        [SqlProcedureNonQuery( "CK.sZoneDestroy" )]
        public abstract Task DestroyZoneAsync( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false );

        [SqlProcedureNonQuery( "sZoneUserAdd" )]
        public abstract void AddUser( ISqlCallContext ctx, int actorId, int zoneId, int userId );

        [SqlProcedureNonQuery( "sZoneUserAdd" )]
        public abstract Task AddUserAsync( ISqlCallContext ctx, int actorId, int zoneId, int userId );

        [SqlProcedureNonQuery( "sZoneUserRemove" )]
        public abstract void RemoveUser( ISqlCallContext ctx, int actorId, int zoneId, int userId );

        [SqlProcedureNonQuery( "sZoneUserRemove" )]
        public abstract Task RemoveUserAsync( ISqlCallContext ctx, int actorId, int zoneId, int userId );
    }
}
