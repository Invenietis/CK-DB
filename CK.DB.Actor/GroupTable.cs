using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.SqlServer.Setup;
using CK.Setup;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CK.SqlServer;

namespace CK.DB.Actor
{
    [SqlTable( "tGroup", Package = typeof( Package ) )]
    [Versions( "5.0.0, 5.0.1" )]
    [SqlObjectItem( "vGroup" )]
    public abstract partial class GroupTable : SqlTable
    {
        void Construct( ActorTable actor )
        {
        }

        /// <summary>
        /// Creates a new Group.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <returns>The new group identifier.</returns>
        [SqlProcedure( "sGroupCreate" )]
        public abstract Task<int> CreateGroupAsync( ISqlCallContext ctx, int actorId );

        /// <summary>
        /// Destroys a Group if and only if there is no more users inside.
        /// Idempotent.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The actor identifier.</param>
        /// <param name="groupId">
        /// The group identifier to destroy. 
        /// If <paramref name="forceDestroy"/> if false, it must be empty otherwise an exception is thrown.
        /// </param>
        /// <param name="forceDestroy">True to remove all users before destroying the group.</param>
        [SqlProcedure( "sGroupDestroy" )]
        public abstract Task DestroyGroupAsync( ISqlCallContext ctx, int actorId, int groupId, bool forceDestroy = false );

        /// <summary>
        /// Adds a user into a group.
        /// Idempotent.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="userId">The user identifier to add.</param>
        [SqlProcedure( "sGroupUserAdd" )]
        public abstract Task AddUserAsync( ISqlCallContext ctx, int actorId, int groupId, int userId );

        /// <summary>
        /// Removes a user from a group.
        /// Idempotent.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="userId">The user identifier to remove.</param>
        [SqlProcedure( "sGroupUserRemove" )]
        public abstract Task RemoveUserAsync( ISqlCallContext ctx, int actorId, int groupId, int userId );

        /// <summary>
        /// Removes all users from a group.
        /// Idempotent.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="groupId">The group identifier.</param>
        [SqlProcedure( "sGroupRemoveAllUsers" )]
        public abstract Task RemoveAllUsersAsync( ISqlCallContext ctx, int actorId, int groupId );
    }
}
