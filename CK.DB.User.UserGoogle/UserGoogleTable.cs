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
using CK.DB.Auth;

namespace CK.DB.User.UserGoogle
{
    /// <summary>
    /// Holds Google account identifier and refresh token for users.
    /// </summary>
    [SqlTable( "tUserGoogle", Package = typeof(Package), Schema = "CK" )]
    [Versions("1.0.0,1.0.1")]
    [SqlObjectItem( "transform:sUserDestroy" )]
    public abstract partial class UserGoogleTable : SqlTable
    {
        public async Task<CreateOrUpdateResult> CreateOrUpdateGoogleUserAsync( ISqlCallContext ctx, int actorId, int userId, UserGoogleInfo info, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate, CancellationToken cancellationToken = default(CancellationToken) )
        {
            var r = await RawCreateOrUpdateGoogleUserAsync( ctx, actorId, userId, info.GoogleAccountId, info.AccessToken, info.AccessTokenExpirationTime, info.RefreshToken, mode, cancellationToken ).ConfigureAwait( false );
            return r.Result;
        }

        public async Task<int> LoginUserAsync( ISqlCallContext ctx, UserGoogleInfo info, bool actualLogin = true, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            var mode = actualLogin
                        ? CreateOrUpdateMode.UpdateOnly | CreateOrUpdateMode.WithLogin
                        : CreateOrUpdateMode.UpdateOnly;
            var r = await RawCreateOrUpdateGoogleUserAsync( ctx, 1, 0, info.GoogleAccountId, info.AccessToken, info.AccessTokenExpirationTime, info.RefreshToken, mode, cancellationToken ).ConfigureAwait( false );
            return r.Result == CreateOrUpdateResult.Updated ? r.UserId : 0;
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

        public struct RawResult
        {
            public readonly int UserId;
            public readonly CreateOrUpdateResult Result;

            public RawResult( int userId, CreateOrUpdateResult result )
            {
                UserId = userId;
                Result = result;
            }
        }

        [SqlProcedure( "sUserGoogleCreateOrUpdate" )]
        public abstract Task<RawResult> RawCreateOrUpdateGoogleUserAsync(
            ISqlCallContext ctx,
            int actorId,
            int userId,
            string googleAccountId,
            string accessToken,
            DateTime? accessTokenExpirationTime,
            string refreshToken,
            CreateOrUpdateMode mode,
            CancellationToken cancellationToken );

        /// <summary>
        /// Finds a user by its Google account identifier.
        /// Returns null if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="googleAccountId">The google account identifier.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A <see cref="KnownUserGoogleInfo"/> object or null if not found.</returns>
        public Task<KnownUserGoogleInfo> FindUserInfoAsync( ISqlCallContext ctx, string googleAccountId, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            using( var c = CreateReaderCommand( googleAccountId ) )
            {
                return c.ExecuteRowAsync( ctx[Database], r => r == null ? null : CreateUserUnfo( googleAccountId, r ) );
            }
        }

        /// <summary>
        /// Creates the <see cref="KnownUserGoogleInfo"/> object from the reader returned by <see cref="CreateReaderCommand(string)"/>.
        /// </summary>
        /// <param name="googleAccountId">The google account identifier.</param>
        /// <param name="r">The reader.</param>
        /// <returns>The user info object.</returns>
        protected virtual KnownUserGoogleInfo CreateUserUnfo( string googleAccountId, SqlDataReader r )
        {
            return new KnownUserGoogleInfo()
            {
                UserId = r.GetInt32( 0 ),
                GoogleAccountId = googleAccountId,
                AccessToken = r.GetString( 1 ),
                AccessTokenExpirationTime = r.GetDateTime( 2 ),
                RefreshToken = r.GetString( 3 ),
                LastRefreshTokenTime = r.GetDateTime( 4 )
            };
        }

        /// <summary>
        /// Creates a reader command parametized with the Google account identifier.
        /// Single-row returned columns must be in this order: 
        /// UserId, RefreshToken, LastRefreshTokenTime, AccessToken, AccessTokenExpirationTime
        /// </summary>
        /// <param name="googleAccountId">Google account identifier to look for.</param>
        /// <returns>A ready to use reader command.</returns>
        protected virtual SqlCommand CreateReaderCommand( string googleAccountId )
        {
            var c = new SqlCommand( $"select UserId, AccessToken, AccessTokenExpirationTime, RefreshToken, LastRefreshTokenTime from CK.tUserGoogle where GoogleAccountId=@A" );
            c.Parameters.Add( new SqlParameter( "@A", googleAccountId ) );
            return c;
        }
    }
}
