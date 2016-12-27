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
        /// <summary>
        /// Associates a GoogleUser to an existing user.
        /// The <paramref name="googleAccountId"/> must not already be associated to another 
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
        /// <returns>True if the Google user has been created, false if it has been updated.</returns>
        [SqlProcedure( "sUserGoogleCreateOrUpdate" )]
        public abstract bool CreateOrUpdateGoogleUser( ISqlCallContext ctx, int actorId, int userId, string googleAccountId, bool actualLogin, string accessToken, DateTime? accessTokenExpirationTime, string refreshToken );

        /// <summary>
        /// Associates a GoogleUser to an existing user.
        /// The <see cref="UserGoogleInfo.GoogleAccountId"/> must not already be associated to another 
        /// user than <see cref="UserGoogleInfo.UserId"/>.
        /// Other fields can be let to null or their default values.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="info">The <see cref="UserGoogleInfo"/> for which a Google user must be created or updated.</param>
        /// <param name="actualLogin">
        /// True to update the LastLoginTime, false otherwise.
        /// This parameter is ignored when creating: it is always considered as a login since LastLoginTime is
        /// always updated.
        /// </param>
        /// <returns>True if the Google user has been created, false if it has been updated.</returns>
        public bool CreateOrUpdateGoogleUser( ISqlCallContext ctx, int actorId, [ParameterSource]UserGoogleInfo info, bool actualLogin )
        {
            return CreateOrUpdateGoogleUser( ctx, actorId, info.UserId, info.GoogleAccountId, actualLogin, info.AccessToken, info.AccessTokenExpirationTime, info.RefreshToken );
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
        /// Returns 0 (Anonymous) if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="googleAccountId">The google account identifier.</param>
        /// <returns>The positive user identifier or 0 if not found.</returns>
        public int FindUser( ISqlCallContext ctx, string googleAccountId )
        {
            return FindByGoogleAccountId<int>( ctx, "UserId", googleAccountId );
        }

        internal T FindByGoogleAccountId<T>( ISqlCallContext ctx, string fieldName, string googleAccountId )
        {
            using( var c = new SqlCommand( $"select {fieldName} from CK.tUserGoogle where GoogleAccountId=@A" ) )
            {
                return c.ExecuteScalar<T>( ctx[Database] );
            }
        }

        /// <summary>
        /// Finds a user by its Google account identifier.
        /// Returns null if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="googleAccountId">The google account identifier.</param>
        /// <returns>A <see cref="UserGoogleInfo"/> object or null if not found.</returns>
        public UserGoogleInfo FindUserInfo( ISqlCallContext ctx, string googleAccountId )
        {
            using( var c = CreateReaderCommand( googleAccountId ) )
            {
                return c.ExecuteRow( ctx[Database], r => r == null ? null : CreateUserUnfo( googleAccountId, r ) );
            }
        }


    }
}
