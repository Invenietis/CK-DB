using CK.SqlServer;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using System.Threading;
using CK.DB.Auth;

namespace CK.DB.User.UserGoogle
{
    /// <summary>
    /// Google authentication provider.
    /// </summary>
    [SqlTable( "tUserGoogle", Package = typeof( Package ), Schema = "CK" )]
    [Versions( "1.0.0,1.0.1,1.0.2,2.0.0,2.0.1" )]
    [SqlObjectItem( "transform:sUserDestroy" )]
    public abstract partial class UserGoogleTable : SqlTable, IGenericAuthenticationProvider<IUserGoogleInfo>
    {
        IPocoFactory<IUserGoogleInfo> _infoFactory;

        /// <summary>
        /// Gets "Google" that is the name of the Google provider.
        /// </summary>
        public string ProviderName => "Google";

        void StObjConstruct( IPocoFactory<IUserGoogleInfo> infoFactory )
        {
            _infoFactory = infoFactory;
        }

        IUserGoogleInfo IGenericAuthenticationProvider<IUserGoogleInfo>.CreatePayload() => _infoFactory.Create();

        /// <summary>
        /// Creates a <see cref="IUserGoogleInfo"/> poco.
        /// </summary>
        /// <returns>A new instance.</returns>
        public T CreateUserInfo<T>() where T : IUserGoogleInfo => (T)_infoFactory.Create();

        /// <summary>
        /// Creates or updates a user entry for this provider. 
        /// This is the "binding account" feature since it binds an external identity to 
        /// an already existing user that may already be registered into other authencation providers.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must be registered.</param>
        /// <param name="info">Provider specific data: the <see cref="IUserGoogleInfo"/> poco.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result.</returns>
        public async Task<UCLResult> CreateOrUpdateGoogleUserAsync( ISqlCallContext ctx, int actorId, int userId, IUserGoogleInfo info, UCLMode mode = UCLMode.CreateOrUpdate, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            var r = await GoogleUserUCLAsync( ctx, actorId, userId, info, mode, cancellationToken ).ConfigureAwait( false );
            return r;
        }

        /// <summary>
        /// Challenges <see cref="IUserGoogleInfo"/> data to identify a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="info">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The <see cref="LoginResult"/>.</returns>
        public async Task<LoginResult> LoginUserAsync( ISqlCallContext ctx, IUserGoogleInfo info, bool actualLogin = true, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            var mode = actualLogin
                        ? UCLMode.UpdateOnly | UCLMode.WithActualLogin
                        : UCLMode.UpdateOnly | UCLMode.WithCheckLogin;
            var r = await GoogleUserUCLAsync( ctx, 1, 0, info, mode, cancellationToken ).ConfigureAwait( false );
            return r.LoginResult;
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
        /// Raw call to manage GoogleUser. Since this should not be used directly, it is protected.
        /// Actual implementation of the centralized update, create or login procedure.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which a Google account must be created or updated.</param>
        /// <param name="info">User information to create or update.</param>
        /// <param name="mode">Configures Create, Update only or WithCheck/ActualLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result.</returns>
        [SqlProcedure( "sUserGoogleUCL" )]
        protected abstract Task<UCLResult> GoogleUserUCLAsync(
            ISqlCallContext ctx,
            int actorId,
            int userId,
            [ParameterSource]IUserGoogleInfo info,
            UCLMode mode,
            CancellationToken cancellationToken );

        /// <summary>
        /// Finds a user by its Google account identifier.
        /// Returns null if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="googleAccountId">The google account identifier.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A <see cref="IdentifiedUserInfo{T}"/> object or null if not found.</returns>
        public Task<IdentifiedUserInfo<IUserGoogleInfo>> FindKnownUserInfoAsync( ISqlCallContext ctx, string googleAccountId, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            using( var c = CreateReaderCommand( googleAccountId ) )
            {
                return ctx[Database].ExecuteSingleRowAsync( c, r => r == null
                                                                    ? null
                                                                    : DoCreateUserUnfo( googleAccountId, r ) );
            }
        }

        /// <summary>
        /// Creates a the reader command parametrized with the Google account identifier.
        /// Single-row returned columns are defined by <see cref="AppendUserInfoColumns(StringBuilder)"/>.
        /// </summary>
        /// <param name="googleAccountId">Google account identifier to look for.</param>
        /// <returns>A ready to use reader command.</returns>
        SqlCommand CreateReaderCommand( string googleAccountId )
        {
            StringBuilder b = new StringBuilder( "select " );
            AppendUserInfoColumns( b ).Append( " from CK.tUserGoogle where GoogleAccountId=@A" );
            var c = new SqlCommand( b.ToString() );
            c.Parameters.Add( new SqlParameter( "@A", googleAccountId ) );
            return c;
        }

        IdentifiedUserInfo<IUserGoogleInfo> DoCreateUserUnfo( string googleAccountId, SqlDataRow r )
        {
            var info = _infoFactory.Create();
            info.GoogleAccountId = googleAccountId;
            FillUserGoogleInfo( info, r, 1 );
            return new IdentifiedUserInfo<IUserGoogleInfo>( r.GetInt32( 0 ), info );
        }

        /// <summary>
        /// Adds the columns name to read.
        /// </summary>
        /// <param name="b">The string builder.</param>
        /// <returns>The string builder.</returns>
        protected virtual StringBuilder AppendUserInfoColumns( StringBuilder b )
        {
            var props = _infoFactory.PocoClassType.GetProperties().Where( p => p.Name != nameof( IUserGoogleInfo.GoogleAccountId ) );
            return props.Any() ? b.Append( "UserId, " ).AppendStrings( props.Select( p => p.Name ) ) : b.Append( "UserId " );
        }

        /// <summary>
        /// Fill UserInfo properties from reader.
        /// </summary>
        /// <param name="info">The info to fill.</param>
        /// <param name="r">The record.</param>
        /// <param name="idx">The index of the first column.</param>
        /// <returns>The updated index.</returns>
        protected virtual int FillUserGoogleInfo( IUserGoogleInfo info, SqlDataRow r, int idx )
        {
            var props = _infoFactory.PocoClassType.GetProperties().Where( p => p.Name != nameof( IUserGoogleInfo.GoogleAccountId ) );
            foreach( var p in props )
            {
                p.SetValue( info, r.GetValue( idx++ ) );
            }
            return idx;
        }

        #region IGenericAuthenticationProvider explicit implementation.

        UCLResult IGenericAuthenticationProvider.CreateOrUpdateUser( ISqlCallContext ctx, int actorId, int userId, object payload, UCLMode mode )
        {
            IUserGoogleInfo info = _infoFactory.ExtractPayload( payload );
            return CreateOrUpdateGoogleUser( ctx, actorId, userId, info, mode );
        }

        LoginResult IGenericAuthenticationProvider.LoginUser( ISqlCallContext ctx, object payload, bool actualLogin )
        {
            IUserGoogleInfo info = _infoFactory.ExtractPayload( payload );
            return LoginUser( ctx, info, actualLogin );
        }

        Task<UCLResult> IGenericAuthenticationProvider.CreateOrUpdateUserAsync( ISqlCallContext ctx, int actorId, int userId, object payload, UCLMode mode, CancellationToken cancellationToken )
        {
            IUserGoogleInfo info = _infoFactory.ExtractPayload( payload );
            return CreateOrUpdateGoogleUserAsync( ctx, actorId, userId, info, mode, cancellationToken );
        }

        Task<LoginResult> IGenericAuthenticationProvider.LoginUserAsync( ISqlCallContext ctx, object payload, bool actualLogin, CancellationToken cancellationToken )
        {
            IUserGoogleInfo info = _infoFactory.ExtractPayload( payload );
            return LoginUserAsync( ctx, info, actualLogin, cancellationToken );
        }

        void IGenericAuthenticationProvider.DestroyUser( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix )
        {
            DestroyGoogleUser( ctx, actorId, userId );
        }

        Task IGenericAuthenticationProvider.DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix, CancellationToken cancellationToken )
        {
            return DestroyGoogleUserAsync( ctx, actorId, userId, cancellationToken );
        }

        #endregion
    }
}
