using CK.SqlServer;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using CK.Core;

namespace CK.DB.Actor
{
    /// <summary>
    /// The UserTable kernel.
    /// </summary>
    [SqlTable( "tUser", Package = typeof( Package ) )]
    [Versions( "5.0.0, 5.0.1, 5.0.2, 5.0.3" )]
    [SqlObjectItem( "vUser" )]
    public abstract partial class UserTable : SqlTable
    {
        void StObjConstruct( ActorTable actor )
        {
        }

        /// <summary>
        /// Tries to create a new user. If the user name is not unique, returns -1.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userName">The user name (must be unique otherwise -1 is returned).</param>
        /// <returns>A new user identifier or -1 if the user name is not unique.</returns>
        [SqlProcedure( "sUserCreate" )]
        public abstract Task<int> CreateUserAsync( ISqlCallContext ctx, int actorId, string userName );

        /// <summary>
        /// Tries to set a new user name.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier to update.</param>
        /// <param name="userName">The user name (must be unique otherwise false is returned).</param>
        /// <returns>True on success, false if the new name already exists.</returns>
        [SqlProcedure( "sUserUserNameSet" )]
        public abstract Task<bool> UserNameSetAsync( ISqlCallContext ctx, int actorId, int userId, string userName );

        /// <summary>
        /// Destroys a user.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier to destroy.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserDestroy" )]
        public abstract Task DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId );

        /// <summary>
        /// Removes a user from all the Groups it belongs to.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must be removed from all its groups.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserRemoveFromAllGroups" )]
        public abstract Task RemoveFromAllGroupsAsync( ISqlCallContext ctx, int actorId, int userId );

        /// <summary>
        /// Finds the user identifier given its user name.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="userName">The user name to lookup.</param>
        /// <returns>The user identifier or 0 if not found.</returns>
        public async Task<int> FindByNameAsync( ISqlCallContext ctx, string userName )
        {
            using( var cmd = new SqlCommand( "select UserId from CK.tUser where UserName=@Key" ) )
            {
                cmd.Parameters.AddWithValue( "@Key", userName );
                return (await ctx[Database].ExecuteScalarAsync( cmd ).ConfigureAwait( false )) is int id ? id : 0;
            }
        }
    }
}
