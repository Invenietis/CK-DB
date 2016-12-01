using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using System.Threading;

namespace CK.DB.User.UserGoogle
{
    /// <summary>
    /// Holds Google account identifier and refresh token for users.
    /// </summary>
    [SqlTable( "tUserGoogle", Package = typeof(Package), Schema = "CK" )]
    [Versions("1.0.0")]
    [SqlObjectItem("transform:sUserDestroy")]
    public abstract partial class UserGoogleTable : SqlTable
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
        /// <param name="scopes">Validated scopes for the Google account.</param>
        /// <param name="accessToken">The access token. Can be null: an empty string is stored.</param>
        /// <param name="accessTokenExpirationTime">Access token expiration time. Can be null (the largest datetime2(2) = '9999-12-31T23:59:59.99' is used).</param>
        /// <param name="refreshToken">The obtained refresh token. Can be null: an empty string is stored on creation and current refresh token is not touched on update.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if the Google user has been created, false if it has been updated.</returns>
        [SqlProcedure( "sUserGoogleCreateOrUpdate" )]
        public abstract Task<bool> CreateOrUpdateGoogleUserAsync( ISqlCallContext ctx, int actorId, int userId, string googleAccountId, string scopes, string accessToken, DateTime? accessTokenExpirationTime, string refreshToken, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Associates a GoogleUser to an existing user.
        /// The <see cref="UserGoogleInfo.GoogleAccountId"/> must not already be associated to another 
        /// user than <see cref="UserGoogleInfo.UserId"/>.
        /// Other fields can be let to null or their default values.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userInfo">The <see cref="UserGoogleInfo"/> for which a Google user must be created or updated.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if the Google user has been created, false if it has been updated.</returns>
        public Task<bool> CreateOrUpdateGoogleUserAsync( ISqlCallContext ctx, int actorId, [ParameterSource]UserGoogleInfo info, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            return CreateOrUpdateGoogleUserAsync( ctx, actorId, info.UserId, info.GoogleAccountId, info.Scopes, info.AccessToken, info.AccessTokenExpirationTime, info.RefreshToken, cancellationToken );
        }

        /// <summary>
        /// Destroys a GoogleUser for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which Google account information must be destroyed.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserGoogleDestroy" )]
        public abstract Task DestroyGoogleUserAsync( ISqlCallContext ctx, int actorId, int userId, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Finds a user by its Google account identifier.
        /// Returns 0 (Anonymous) if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="googleAccountId">The google account identifier.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The positive user identifier or 0 if not found.</returns>
        public Task<int> FindUserAsync( ISqlCallContext ctx, string googleAccountId, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            return ScalarByGoogleAccountIdAsync<int>( ctx, "UserId", googleAccountId, cancellationToken );
        }

        internal async Task<T> ScalarByGoogleAccountIdAsync<T>( ISqlCallContext ctx, string fieldName, string googleAccountId, CancellationToken cancellationToken )
        {
            using( var c = new SqlCommand( $"select {fieldName} from CK.tUserGoogle where GoogleAccountId=@A" ) )
            using( await (c.Connection = ctx[Database]).EnsureOpenAsync().ConfigureAwait( false ) )
            {
                c.Parameters.AddWithValue( "@A", googleAccountId );
                var o = await c.ExecuteScalarAsync().ConfigureAwait( false );
                return o != DBNull.Value ? (T)o : default(T);
            }
        }

        /// <summary>
        /// Finds a user by its Google account identifier.
        /// Returns null if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="googleAccountId">The google account identifier.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A <see cref="UserGoogleInfo"/> object or null if not found.</returns>
        public async Task<UserGoogleInfo> FindUserInfoAsync( ISqlCallContext ctx, string googleAccountId, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            using( var c = CreateReaderCommand( googleAccountId ) )
            using( await (c.Connection = ctx[Database]).EnsureOpenAsync().ConfigureAwait( false ) )
            using( var r = await c.ExecuteReaderAsync( System.Data.CommandBehavior.SingleRow ).ConfigureAwait( false ) )
            {
                if( !await r.ReadAsync().ConfigureAwait( false ) ) return null;
                return CreateUserUnfo( googleAccountId, r );
            }
        }

        /// <summary>
        /// Creates the <see cref="UserGoogleInfo"/> object from the reader returned by <see cref="CreateReaderCommand(string)"/>.
        /// </summary>
        /// <param name="googleAccountId">The google account identifier.</param>
        /// <param name="r">The reader.</param>
        /// <returns>The user info object.</returns>
        protected virtual UserGoogleInfo CreateUserUnfo( string googleAccountId, SqlDataReader r )
        {
            return new UserGoogleInfo()
            {
                UserId = r.GetInt32( 0 ),
                GoogleAccountId = googleAccountId,
                Scopes = r.GetString( 1 ),
                AccessToken = r.GetString( 2 ),
                AccessTokenExpirationTime = r.GetDateTime( 3 ),
                RefreshToken = r.GetString( 4 ),
                LastRefreshTokenTime = r.GetDateTime( 5 )
            };
        }

        /// <summary>
        /// Creates a reader command parametized with the Google account identifier.
        /// Sinlge-row returned columns must be in this order: 
        /// UserId, RefreshToken, LastRefreshTokenTime, AccessToken, AccessTokenExpirationTime
        /// </summary>
        /// <param name="googleAccountId">Google account identifier to look for.</param>
        /// <returns>A ready to use reader command.</returns>
        protected virtual SqlCommand CreateReaderCommand( string googleAccountId )
        {
            var c = new SqlCommand( $"select UserId, Scopes, AccessToken, AccessTokenExpirationTime, RefreshToken, LastRefreshTokenTime from CK.tUserGoogle where GoogleAccountId=@A" );
            c.Parameters.Add( new SqlParameter( "@A", googleAccountId ) );
            return c;
        }
    }
}
