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
    public abstract partial class UserGoogleTable : SqlTable, IGenericAuthenticationTableProvider
    {
        /// <summary>
        /// Gets "Google" that is the name of the Google provider.
        /// </summary>
        public string ProviderName => "Google";

        /// <summary>
        /// Creates or updates a user entry for this provider. 
        /// This is the "binding account" feature since it binds an external identity to 
        /// an already existing user that may already be registered into other authencation providers.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must be registered.</param>
        /// <param name="payload">Provider specific data.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <returns>The operation result.</returns>
        public async Task<CreateOrUpdateResult> CreateOrUpdateGoogleUserAsync( ISqlCallContext ctx, int actorId, int userId, UserGoogleInfo info, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate, CancellationToken cancellationToken = default(CancellationToken) )
        {
            var r = await RawCreateOrUpdateGoogleUserAsync( ctx, actorId, userId, info, mode, cancellationToken ).ConfigureAwait( false );
            return r.Result;
        }

        /// <summary>
        /// Challenges <see cref="UserGoogleInfo"/> data to identify a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="payload">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <returns>The positive identifier of the user on success or 0 if the Google user does not exist.</returns>
        public async Task<int> LoginUserAsync( ISqlCallContext ctx, UserGoogleInfo info, bool actualLogin = true, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            var mode = actualLogin
                        ? CreateOrUpdateMode.UpdateOnly | CreateOrUpdateMode.WithLogin
                        : CreateOrUpdateMode.UpdateOnly;
            var r = await RawCreateOrUpdateGoogleUserAsync( ctx, 1, 0, info, mode, cancellationToken ).ConfigureAwait( false );
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

        /// <summary>
        /// Captures the result of <see cref="RawCreateOrUpdateGoogleUser"/> and <see cref="RawCreateOrUpdateGoogleUserAsync"/>.
        /// </summary>
        public struct RawResult
        {
            /// <summary>
            /// The user identifier.
            /// </summary>
            public readonly int UserId;

            /// <summary>
            /// The operation result.
            /// </summary>
            public readonly CreateOrUpdateResult Result;

            /// <summary>
            /// Initializes a new <see cref="RawResult"/>.
            /// </summary>
            /// <param name="userId">User identifier.</param>
            /// <param name="result">Operation result.</param>
            public RawResult( int userId, CreateOrUpdateResult result )
            {
                UserId = userId;
                Result = result;
            }
        }

        /// <summary>
        /// Raw call to manage GoogleUser. Since this should not be used directly, it is protected.
        /// Actual implementation of the centralized create, update or login procedure.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which a Google account must be created or updated.</param>
        /// <param name="googleAccountId">The Google account identifier.</param>
        /// <param name="accessToken">The access token. Can be null: an empty string is stored.</param>
        /// <param name="accessTokenExpirationTime">Access token expiration time. Can be null (the largest datetime2(2) = '9999-12-31T23:59:59.99' is used).</param>
        /// <param name="refreshToken">The obtained refresh token. Can be null: an empty string is stored on creation and current refresh token is not touched on update.</param>
        /// <param name="mode">Configures Create, Update only and/or WithLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The user identifier (when <paramref name="userId"/> is 0, this is a login) and the operation result.</returns>
        [SqlProcedure( "sUserGoogleCreateOrUpdate" )]
        protected abstract Task<RawResult> RawCreateOrUpdateGoogleUserAsync(
            ISqlCallContext ctx,
            int actorId,
            int userId,
            [ParameterSource]UserGoogleInfo info,
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
                return c.ExecuteRowAsync( ctx[Database], r => r == null ? null : DoCreateUserUnfo( googleAccountId, r ) );
            }
        }


        /// <summary>
        /// Creates a the reader command parametrized with the Google account identifier.
        /// Single-row returned columns are defined by <see cref="AppendColumns(StringBuilder)"/>.
        /// </summary>
        /// <param name="googleAccountId">Google account identifier to look for.</param>
        /// <returns>A ready to use reader command.</returns>
        SqlCommand CreateReaderCommand( string googleAccountId )
        {
            StringBuilder b = new StringBuilder( "select " );
            AppendColumns( b ).Append( " from CK.tUserGoogle where GoogleAccountId=@A" );
            var c = new SqlCommand( b.ToString() );
            c.Parameters.Add( new SqlParameter( "@A", googleAccountId ) );
            return c;
        }

        KnownUserGoogleInfo DoCreateUserUnfo( string googleAccountId, SqlDataReader r )
        {
            KnownUserGoogleInfo info = new KnownUserGoogleInfo()
            {
                UserId = r.GetInt32( 0 ),
                Info = CreateUserUnfo( googleAccountId )
            };
            FillUserGoogleInfo( info.Info, r, 1 );
            return info;
        }


        /// <summary>
        /// Adds the columns name to read.
        /// </summary>
        /// <param name="b">The string builder.</param>
        /// <returns>The string builder.</returns>
        protected virtual StringBuilder AppendColumns( StringBuilder b )
        {
            return b.Append( "UserId, AccessToken, AccessTokenExpirationTime, RefreshToken, LastRefreshTokenTime" );
        }

        /// <summary>
        /// Leaf creation of the mixin type...
        /// </summary>
        /// <returns></returns>
        protected virtual UserGoogleInfo CreateUserUnfo( string googleAccountId ) => new UserGoogleInfo() { GoogleAccountId = googleAccountId };

        /// <summary>
        /// Fill data from reader from top to bottom.
        /// </summary>
        /// <param name="info">The ifnfo to fill.</param>
        /// <param name="r">The data reader.</param>
        /// <param name="idx">The index of the first column.</param>
        /// <returns>The updated index.</returns>
        protected virtual int FillUserGoogleInfo( UserGoogleInfo info, SqlDataReader r, int idx )
        {
            info.AccessToken = r.GetString( idx++ );
            info.AccessTokenExpirationTime = r.GetDateTime( idx++ );
            info.RefreshToken = r.GetString( idx++ );
            info.LastRefreshTokenTime = r.GetDateTime( idx++ );
            return idx;
        }


        #region IGenericAuthenticationProvider explicit implementation.

        CreateOrUpdateResult IGenericAuthenticationProvider.CreateOrUpdateUser( ISqlCallContext ctx, int actorId, int userId, object payload, CreateOrUpdateMode mode )
        {
            UserGoogleInfo info = payload as UserGoogleInfo;
            if( info == null ) throw new ArgumentException( nameof( payload ) );
            return CreateOrUpdateGoogleUser( ctx, actorId, userId, info, mode );
        }

        int? IGenericAuthenticationProvider.LoginUser( ISqlCallContext ctx, object payload, bool actualLogin )
        {
            UserGoogleInfo info = payload as UserGoogleInfo;
            if( info == null ) return null;
            return LoginUser( ctx, info, actualLogin );
        }

        Task<CreateOrUpdateResult> IGenericAuthenticationProvider.CreateOrUpdateUserAsync( ISqlCallContext ctx, int actorId, int userId, object payload, CreateOrUpdateMode mode, CancellationToken cancellationToken )
        {
            UserGoogleInfo info = payload as UserGoogleInfo;
            if( info == null ) throw new ArgumentException( nameof( payload ) );
            return CreateOrUpdateGoogleUserAsync( ctx, actorId, userId, info, mode, cancellationToken );
        }

        async Task<int?> IGenericAuthenticationProvider.LoginUserAsync( ISqlCallContext ctx, object payload, bool actualLogin, CancellationToken cancellationToken )
        {
            UserGoogleInfo info = payload as UserGoogleInfo;
            if( info == null ) return null;
            return await LoginUserAsync( ctx, info, actualLogin, cancellationToken );
        }

        void IGenericAuthenticationProvider.DestroyUser( ISqlCallContext ctx, int actorId, int userId )
        {
            DestroyGoogleUser( ctx, actorId, userId );
        }

        Task IGenericAuthenticationProvider.DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId, CancellationToken cancellationToken )
        {
            return DestroyGoogleUserAsync( ctx, actorId, userId, cancellationToken );
        }

        #endregion
    }
}
