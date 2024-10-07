using System.Threading.Tasks;
using CK.Core;
using CK.SqlServer;

namespace CK.DB.HZone;

/// <summary>
/// Specializes the <see cref="Zone.ZoneTable"/> to support parent zones whenever possible.
/// </summary>
[SqlTable( "tZone", Package = typeof( Package ) ), Versions( "1.0.0" )]
public abstract partial class ZoneTable : CK.DB.Zone.ZoneTable
{
    /// <summary>
    /// Creates a new Zone under an another one (or 0 for a root zone).
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="parentZoneId">The parent zone.</param>
    /// <returns>A new Zone identifier.</returns>
    [SqlProcedure( "transform:CK.sZoneCreate" )]
    public abstract Task<int> CreateZoneAsync( ISqlCallContext ctx, int actorId, int parentZoneId );

    /// <summary>
    /// Destroys a Zone, optionally destroying its groups and child zones.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier to destroy.</param>
    /// <param name="forceDestroy">True to destroy the Zone even it is contains User or Groups (its Groups are destroyed).</param>
    /// <param name="destroySubZone">
    /// True to destroy any existing child zones (recursively), false to raise an error if a child zone exists.
    /// Letting it to null uses the <paramref name="forceDestroy"/> parameter value.
    /// </param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "transform:CK.sZoneDestroy" )]
    public abstract Task DestroyZoneAsync( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false, bool? destroySubZone = null );

    /// <summary>
    /// Registers a user in a Zone: the user can then be added to groups of the zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier into which the user must be added.</param>
    /// <param name="userId">The user identifier to add.</param>
    /// <param name="autoAddUserInParentZone">True to automatically add the user to its parent zones.</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "transform:CK.sZoneUserAdd" )]
    public abstract Task AddUserAsync( ISqlCallContext ctx, int actorId, int zoneId, int userId, bool autoAddUserInParentZone = false );

    /// <summary>
    /// Removes a user from a Zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier from which the user must be removed.</param>
    /// <param name="userId">The user identifier to remove.</param>
    /// <param name="autoRemoveUserFromChildZone">
    /// True to automatically remove the user from any child zones.
    /// False will raise an error if the user is registered in child zones.
    /// </param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "transform:sZoneUserRemove" )]
    public abstract Task RemoveUserAsync( ISqlCallContext ctx, int actorId, int zoneId, int userId, bool autoRemoveUserFromChildZone = true );

    /// <summary>
    /// Moves a Zone to another parent zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier to move.</param>
    /// <param name="newParentZoneId">The target Zone identifier.</param>
    /// <param name="option">Options that control the move. See <see cref="Zone.GroupMoveOption"/>.</param>
    /// <param name="nextSiblingId">
    /// Optional identifier of a child zone in the target zone.
    /// When not specified, the zone is moved at the end of the existing child ot the target zone.
    /// </param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "sZoneMove" )]
    public abstract Task MoveZoneAsync( ISqlCallContext ctx, int actorId, int zoneId, int newParentZoneId, Zone.GroupMoveOption option = Zone.GroupMoveOption.None, int nextSiblingId = 0 );

}
