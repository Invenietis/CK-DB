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

        /// <summary>
        /// Creates a new zone.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <returns>A new zone identifier.</returns>
        [SqlProcedure( "CK.sZoneCreate" )]
        public abstract int CreateZone( ISqlCallContext ctx, int actorId );

        /// <summary>
        /// Destroys a Zone, optionally destroying its groups.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="zoneId">The Zone identifier to destroy.</param>
        /// <param name="forceDestroy">True to destroy the Zone even it is contains User or Groups (its Groups are destroyed).</param>
        [SqlProcedure( "CK.sZoneDestroy" )]
        public abstract void DestroyZone( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false );

        /// <summary>
        /// Registers a user in a Zone: the user can then be added to groups of the zone.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="zoneId">The Zone identifier into which the user must be added.</param>
        /// <param name="userId">The user identifier to add.</param>
        [SqlProcedure( "sZoneUserAdd" )]
        public abstract void AddUser( ISqlCallContext ctx, int actorId, int zoneId, int userId );

        /// <summary>
        /// Removes a user from a Zone.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="zoneId">The Zone identifier from which the user must be removed.</param>
        /// <param name="userId">The user identifier to remove.</param>
        [SqlProcedure( "sZoneUserRemove" )]
        public abstract void RemoveUser( ISqlCallContext ctx, int actorId, int zoneId, int userId );

    }
}
