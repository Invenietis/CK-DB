using CK.Core;
using CK.SqlServer;

namespace CK.DB.Zone
{
    public abstract partial class GroupTable 
    {
        /// <summary>
        /// Moves a group to another Zone.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="groupId">The group identifier to move.</param>
        /// <param name="newZoneId">The target zone identifier.</param>
        /// <param name="option">Options that control the move. See <see cref="GroupMoveOption"/>.</param>
        [SqlProcedure( "sGroupMove" )]
        public abstract void MoveGroup( ISqlCallContext ctx, int actorId, int groupId, int newZoneId, GroupMoveOption option = GroupMoveOption.None );

        /// <summary>
        /// Creates a group in a Zone.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="zoneId">The zone that must contain the new group.</param>
        /// <returns>A new group identifier.</returns>
        [SqlProcedure( "transform:sGroupCreate" )]
        public abstract int CreateGroup( ISqlCallContext ctx, int actorId, int zoneId );

        /// <summary>
        /// Adds a user to a group: this user must have been registered in the zone
        /// unless <paramref name="autoAddUserInZone"/> is true.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="userId">The user identifier to add.</param>
        /// <param name="autoAddUserInZone">
        /// True to automatically register the user in the group's zone.</param>
        [SqlProcedure( "transform:sGroupUserAdd" )]
        public abstract void AddUser( ISqlCallContext ctx, int actorId, int groupId, int userId, bool autoAddUserInZone = false );
    }
}
