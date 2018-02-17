using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.SqlServer.Setup;
using CK.Setup;
using CK.SqlServer;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace CK.DB.Actor
{
    public abstract partial class UserTable : SqlTable
    {
        /// <summary>
        /// Tries to create a new user. If the user name is not unique, returns -1.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userName">The user name (must be unique otherwise -1 is returned).</param>
        /// <returns>A new user identifier or -1 if the user name is not unique.</returns>
        [SqlProcedure( "sUserCreate" )]
        public abstract int CreateUser( ISqlCallContext ctx, int actorId, string userName );

        /// <summary>
        /// Tries to set a new user name.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier to update.</param>
        /// <param name="userName">The user name (must be unique otherwise false is returned).</param>
        /// <returns>True on success, false if the new name already exists.</returns>
        [SqlProcedure( "sUserUserNameSet" )]
        public abstract bool UserNameSet( ISqlCallContext ctx, int actorId, int userId, string userName );

        /// <summary>
        /// Destroys a user.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier to destroy.</param>
        [SqlProcedure( "sUserDestroy" )]
        public abstract void DestroyUser( ISqlCallContext ctx, int actorId, int userId );

        /// <summary>
        /// Removes a user from all the Groups it belongs to.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must be removed from all its groups.</param>
        [SqlProcedure( "sUserRemoveFromAllGroups" )]
        public abstract void RemoveFromAllGroups( ISqlCallContext ctx, int actorId, int userId );

        /// <summary>
        /// Finds the user identifier given its user name.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="userName">The user name to lookup.</param>
        /// <returns>The user identifier or 0 if not found.</returns>
        public int FindByName( ISqlCallContext ctx, string userName )
        {
            using( var cmd = new SqlCommand( "select UserId from CK.tUser where UserName=@Key" ) )
            {
                cmd.Parameters.AddWithValue( "@Key", userName );
                return ctx[Database].ExecuteScalar( cmd ) is int id ? id : 0;
            }
        }

    }
}
