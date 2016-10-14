using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;

namespace CK.DB.HZone
{
    public abstract partial class ZoneTable
    {
        [SqlProcedure( "transform:CK.sZoneCreate" )]
        public abstract int CreateZone( ISqlCallContext ctx, int actorId, int parentZoneId );

        [SqlProcedure( "transform:CK.sZoneDestroy" )]
        public abstract void DestroyZone( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false, bool? destroySubZone = null );

        [SqlProcedure( "transform:CK.sZoneUserAdd" )]
        public abstract void AddUser( ISqlCallContext ctx, int actorId, int zoneId, int userId, bool autoAddUserInParentZone = false );

        [SqlProcedure( "transform:sZoneUserRemove" )]
        public abstract void RemoveUser( ISqlCallContext ctx, int actorId, int zoneId, int userId, bool autoRemoveUserFromChildZone = true );

        [SqlProcedure( "sZoneMove" )]
        public abstract void MoveZone( ISqlCallContext ctx, int actorId, int zoneId, int newParentZoneId, int nextSiblingId = 0 );

    }
}
