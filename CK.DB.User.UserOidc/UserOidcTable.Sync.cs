using CK.DB.Auth;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.User.UserOidc
{
    public abstract partial class UserOidcTable
    {
        /// <summary>
        /// Creates or updates a user entry for this provider. 
        /// This is the "binding account" feature since it binds an external identity to 
        /// an already existing user that may already be registered into other authencation providers.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must be registered.</param>
        /// <param name="info">Provider specific data: the <see cref="IUserOidcInfo"/> poco.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <returns>The operation result.</returns>
        public CreateOrUpdateResult CreateOrUpdateOidcUser( ISqlCallContext ctx, int actorId, int userId, IUserOidcInfo info, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate )
        {
            var r = RawCreateOrUpdateOidcUser( ctx, actorId, userId, info, mode );
            return r.Result;
        }

        /// <summary>
        /// Challenges <see cref="IUserOidcInfo"/> data to identify a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="info">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <returns>The positive identifier of the user on success or 0 if the Oidc user does not exist.</returns>
        public int LoginUser( ISqlCallContext ctx, IUserOidcInfo info, bool actualLogin = true )
        {
            var mode = actualLogin
                        ? CreateOrUpdateMode.UpdateOnly | CreateOrUpdateMode.WithLogin
                        : CreateOrUpdateMode.UpdateOnly; 
            var r = RawCreateOrUpdateOidcUser( ctx, 1, 0, info, mode );
            return r.Result == CreateOrUpdateResult.Updated ? r.UserId : 0;
        }

        /// <summary>
        /// Destroys a OidcUser for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which Oidc account information must be destroyed.</param>
        /// <param name="schemeSuffix">
        /// Scheme suffix to delete.
        /// When null, all registrations for this provider regardless of the scheme suffix are deleted.
        /// </param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserOidcDestroy" )]
        public abstract void DestroyOidcUser( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix );

        /// <summary>
        /// Finds a user by its Oidc account identifier.
        /// Returns null if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="schemeSuffix">The scheme suffix.</param>
        /// <param name="sub">The sub that identifies the user in the <paramref name="schemeSuffix"/>.</param>
        /// <returns>A <see cref="KnownUserOidcInfo"/> or null if not found.</returns>
        public KnownUserOidcInfo FindKnownUserInfo( ISqlCallContext ctx, string schemeSuffix, string sub )
        {
            using( var c = CreateReaderCommand( schemeSuffix, sub ) )
            {
                return c.ExecuteRow( ctx[Database], r => r == null
                                                            ? null
                                                            : DoCreateUserUnfo( schemeSuffix, sub, r ) );
            }
        }

        /// <summary>
        /// Raw call to manage OidcUser. Since this should not be used directly, it is protected.
        /// Actual implementation of the centralized create, update or login procedure.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which a Oidc account must be created or updated.</param>
        /// <param name="info">User information to create or update.</param>
        /// <param name="mode">Configures Create, Update only or WithLogin behavior.</param>
        /// <returns>The user identifier (when <paramref name="userId"/> is 0, this is a login) and the operation result.</returns>
        [SqlProcedure( "sUserOidcCreateOrUpdate" )]
        protected abstract RawResult RawCreateOrUpdateOidcUser( 
            ISqlCallContext ctx, 
            int actorId, 
            int userId,
            [ParameterSource]IUserOidcInfo info,
            CreateOrUpdateMode mode );


    }
}
