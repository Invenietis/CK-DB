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
    [SqlTable( "tZone", Package=typeof(Package) ), Versions( "1.0.0" )]
    public abstract partial class ZoneTable : CK.DB.Zone.ZoneTable
    {
        [SqlProcedure( "transform:CK.sZoneCreate" )]
        public abstract Task<int> CreateZoneAsync( ISqlCallContext ctx, int actorId, int parentZoneId );

        [SqlProcedure( "transform:CK.sZoneDestroy" )]
        public abstract Task DestroyZoneAsync( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false, bool? destroySubZone = null );

        [SqlProcedure( "transform:CK.sZoneUserAdd" )]
        public abstract Task AddUserAsync( ISqlCallContext ctx, int actorId, int zoneId, int userId, bool autoAddUserInParentZone = false );

        [SqlProcedure( "transform:sZoneUserRemove" )]
        public abstract Task RemoveUserAsync( ISqlCallContext ctx, int actorId, int zoneId, int userId, bool autoRemoveUserFromChildZone = true );

        [SqlProcedure( "sZoneMove" )]
        public abstract Task MoveZoneAsync( ISqlCallContext ctx, int actorId, int zoneId, int newParentZoneId, Zone.GroupMoveOption option = Zone.GroupMoveOption.None, int nextSiblingId = 0 );

    }
}
