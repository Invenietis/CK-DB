using CK.DB.Auth;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.User.UserGoogle
{
    public abstract partial class UserGoogleTable
    {
        public CreateOrUpdateResult CreateOrUpdateGoogleUser( ISqlCallContext ctx, int actorId, int userId, UserGoogleInfo info, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate )
        {
            var r = RawCreateOrUpdateGoogleUser( ctx, actorId, userId, info.GoogleAccountId, info.AccessToken, info.AccessTokenExpirationTime, info.RefreshToken, mode );
            return r.Result;
        }

        public int LoginUser( ISqlCallContext ctx, UserGoogleInfo info, bool actualLogin = true )
        {
            var mode = actualLogin
                        ? CreateOrUpdateMode.UpdateOnly | CreateOrUpdateMode.WithLogin
                        : CreateOrUpdateMode.UpdateOnly; 
            var r = RawCreateOrUpdateGoogleUser( ctx, 1, 0, info.GoogleAccountId, info.AccessToken, info.AccessTokenExpirationTime, info.RefreshToken, mode );
            return r.Result == CreateOrUpdateResult.Updated ? r.UserId : 0;
        }

        /// <summary>
        /// Destroys a GoogleUser for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which Google account information must be destroyed.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserGoogleDestroy" )]
        public abstract void DestroyGoogleUser( ISqlCallContext ctx, int actorId, int userId );

        /// <summary>
        /// Finds a user by its Google account identifier.
        /// Returns null if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="googleAccountId">The google account identifier.</param>
        /// <returns>A <see cref="UserGoogleInfo"/> object or null if not found.</returns>
        public KnownUserGoogleInfo FindUserInfo( ISqlCallContext ctx, string googleAccountId )
        {
            using( var c = CreateReaderCommand( googleAccountId ) )
            {
                return c.ExecuteRow( ctx[Database], r => r == null ? null : CreateUserUnfo( googleAccountId, r ) );
            }
        }

        /// <summary>
        /// Raw call to manage GoogleUser. This should not be used directly.
        /// user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which a Google account must be created or updated.</param>
        /// <param name="googleAccountId">The Google account identifier.</param>
        /// <param name="actualLogin">True to update the LastLoginTime, false otherwise.</param>
        /// <param name="accessToken">The access token. Can be null: an empty string is stored.</param>
        /// <param name="accessTokenExpirationTime">Access token expiration time. Can be null (the largest datetime2(2) = '9999-12-31T23:59:59.99' is used).</param>
        /// <param name="refreshToken">The obtained refresh token. Can be null: an empty string is stored on creation and current refresh token is not touched on update.</param>
        /// <param name="mode">Configures Create or Update only behavior.</param>
        /// <returns>The user identifier (when <paramref name="userId"/> is 0, this is a login) and the operation result.</returns>
        [SqlProcedure( "sUserGoogleCreateOrUpdate" )]
        public abstract RawResult RawCreateOrUpdateGoogleUser( 
            ISqlCallContext ctx, 
            int actorId, 
            int userId, 
            string googleAccountId, 
            string accessToken, 
            DateTime? accessTokenExpirationTime, 
            string refreshToken, 
            CreateOrUpdateMode mode );


    }
}
