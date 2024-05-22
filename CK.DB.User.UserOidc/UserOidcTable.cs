using CK.SqlServer;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using System.Threading;
using CK.DB.Auth;
using System.Diagnostics.CodeAnalysis;

namespace CK.DB.User.UserOidc
{
    /// <summary>
    /// Oidc authentication provider.
    /// </summary>
    [SqlTable( "tUserOidc", Package = typeof( Package ), Schema = "CK" )]
    [Versions( "2.0.0, 2.0.1" )]
    [SqlObjectItem( "transform:sUserDestroy" )]
    public abstract partial class UserOidcTable : SqlTable, IGenericAuthenticationProvider<IUserOidcInfo>
    {
        [AllowNull]
        IPocoFactory<IUserOidcInfo> _infoFactory;

        /// <summary>
        /// Gets "Oidc" that is the name of the Oidc provider.
        /// </summary>
        public string ProviderName => "Oidc";

        public bool CanCreatePayload => true;

        object IGenericAuthenticationProvider.CreatePayload() => _infoFactory.Create();

        IUserOidcInfo IGenericAuthenticationProvider<IUserOidcInfo>.CreatePayload() => _infoFactory.Create();

        void StObjConstruct( IPocoFactory<IUserOidcInfo> infoFactory )
        {
            _infoFactory = infoFactory;
        }

        /// <summary>
        /// Creates a <see cref="IUserOidcInfo"/> poco.
        /// </summary>
        /// <returns>A new instance.</returns>
        public T CreateUserInfo<T>() where T : IUserOidcInfo => (T)_infoFactory.Create();

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
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result.</returns>
        public async Task<UCLResult> CreateOrUpdateOidcUserAsync( ISqlCallContext ctx, int actorId, int userId, IUserOidcInfo info, UCLMode mode = UCLMode.CreateOrUpdate, CancellationToken cancellationToken = default )
        {
            var r = await UserOidcULCAsync( ctx, actorId, userId, info, mode, cancellationToken ).ConfigureAwait( false );
            return r;
        }

        /// <summary>
        /// Challenges <see cref="IUserOidcInfo"/> data to identify a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="info">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The login result.</returns>
        public async Task<LoginResult> LoginUserAsync( ISqlCallContext ctx, IUserOidcInfo info, bool actualLogin = true, CancellationToken cancellationToken = default )
        {
            var mode = actualLogin
                        ? UCLMode.UpdateOnly | UCLMode.WithActualLogin
                        : UCLMode.UpdateOnly | UCLMode.WithCheckLogin;
            var r = await UserOidcULCAsync( ctx, 1, 0, info, mode, cancellationToken ).ConfigureAwait( false );
            return r.LoginResult;
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
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserOidcDestroy" )]
        public abstract Task DestroyOidcUserAsync( ISqlCallContext ctx, int actorId, int userId, string? schemeSuffix, CancellationToken cancellationToken = default );

        /// <summary>
        /// Raw call to manage OidcUser. Since this should not be used directly, it is protected.
        /// Actual implementation of the centralized create, update or login procedure.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which a Oidc account must be created or updated.</param>
        /// <param name="info">User information to create or update.</param>
        /// <param name="mode">Configures Create, Update only or WithLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result.</returns>
        [SqlProcedure( "sUserOidcUCL" )]
        protected abstract Task<UCLResult> UserOidcULCAsync(
            ISqlCallContext ctx,
            int actorId,
            int userId,
            [ParameterSource]IUserOidcInfo info,
            UCLMode mode,
            CancellationToken cancellationToken );

        /// <summary>
        /// Finds a user by its Oidc scheme suffix and sub.
        /// Returns null if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="schemeSuffix">The scheme suffix.</param>
        /// <param name="sub">The sub that identifies the user in the <paramref name="schemeSuffix"/>.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A <see cref="IdentifiedUserInfo{T}"/> object or null if not found.</returns>
        public async Task<IdentifiedUserInfo<IUserOidcInfo>?> FindKnownUserInfoAsync( ISqlCallContext ctx, string schemeSuffix, string sub, CancellationToken cancellationToken = default )
        {
            using( var c = CreateReaderCommand( schemeSuffix, sub ) )
            {
                return await ctx[Database].ExecuteSingleRowAsync( c, r => r == null 
                                                                            ? null 
                                                                            : DoCreateUserUnfo( schemeSuffix, sub, r ), cancellationToken )
                                           .ConfigureAwait( false );
            }
        }


        /// <summary>
        /// Creates a the reader command parametrized with the Oidc account identifier.
        /// Single-row returned columns are defined by <see cref="AppendUserInfoColumns(StringBuilder)"/>.
        /// </summary>
        /// <param name="schemeSuffix">The scheme suffix.</param>
        /// <param name="sub">The sub that identifies the user in the <paramref name="schemeSuffix"/>.</param>
        /// <returns>A ready to use reader command.</returns>
        SqlCommand CreateReaderCommand( string? schemeSuffix, string sub )
        {
            StringBuilder b = new StringBuilder( "select " );
            AppendUserInfoColumns( b ).Append( " from CK.tUserOidc where SchemeSuffix=@S and Sub=@U" );
            var c = new SqlCommand( b.ToString() );
            c.Parameters.Add( new SqlParameter( "@S", schemeSuffix ) );
            c.Parameters.Add( new SqlParameter( "@U", sub ) );
            return c;
        }

        IdentifiedUserInfo<IUserOidcInfo> DoCreateUserUnfo( string? schemeSuffix, string sub, SqlDataRow r )
        {
            var info = _infoFactory.Create();
            info.SchemeSuffix = schemeSuffix ?? string.Empty;
            info.Sub = sub;
            FillUserOidcInfo( info, r, 1 );
            var result = new IdentifiedUserInfo<IUserOidcInfo>( r.GetInt32( 0 ), info );
            return result;
        }

        /// <summary>
        /// Adds the columns name to read.
        /// </summary>
        /// <param name="b">The string builder.</param>
        /// <returns>The string builder.</returns>
        protected virtual StringBuilder AppendUserInfoColumns( StringBuilder b )
        {
            var props = _infoFactory.PocoClassType.GetProperties().Where( p => p.Name != nameof( IUserOidcInfo.SchemeSuffix ) && p.Name != nameof( IUserOidcInfo.Sub ) );
            return props.Any() ? b.Append( "UserId, " ).AppendStrings( props.Select( p => p.Name ) ) : b.Append( "UserId " );
        }

        /// <summary>
        /// Fill UserInfo properties from reader.
        /// </summary>
        /// <param name="info">The info to fill.</param>
        /// <param name="r">The record.</param>
        /// <param name="idx">The index of the first column.</param>
        /// <returns>The updated index.</returns>
        protected virtual int FillUserOidcInfo( IUserOidcInfo info, SqlDataRow r, int idx )
        {
            var props = _infoFactory.PocoClassType.GetProperties().Where( p => p.Name != nameof( IUserOidcInfo.SchemeSuffix ) && p.Name != nameof( IUserOidcInfo.Sub ) );
            foreach( var p in props )
            {
                p.SetValue( info, r.GetValue( idx++ ) );
            }
            return idx;
        }

        #region IGenericAuthenticationProvider explicit implementation.

        UCLResult IGenericAuthenticationProvider.CreateOrUpdateUser( ISqlCallContext ctx, int actorId, int userId, object payload, UCLMode mode )
        {
            IUserOidcInfo info = _infoFactory.ExtractPayload( payload );
            return CreateOrUpdateOidcUser( ctx, actorId, userId, info, mode );
        }

        LoginResult IGenericAuthenticationProvider.LoginUser( ISqlCallContext ctx, object payload, bool actualLogin )
        {
            IUserOidcInfo info = _infoFactory.ExtractPayload( payload );
            return LoginUser( ctx, info, actualLogin );
        }

        Task<UCLResult> IGenericAuthenticationProvider.CreateOrUpdateUserAsync( ISqlCallContext ctx, int actorId, int userId, object payload, UCLMode mode, CancellationToken cancellationToken )
        {
            IUserOidcInfo info = _infoFactory.ExtractPayload( payload );
            return CreateOrUpdateOidcUserAsync( ctx, actorId, userId, info, mode, cancellationToken );
        }

        Task<LoginResult> IGenericAuthenticationProvider.LoginUserAsync( ISqlCallContext ctx, object payload, bool actualLogin, CancellationToken cancellationToken )
        {
            IUserOidcInfo info = _infoFactory.ExtractPayload( payload );
            return LoginUserAsync( ctx, info, actualLogin, cancellationToken );
        }

        void IGenericAuthenticationProvider.DestroyUser( ISqlCallContext ctx, int actorId, int userId, string? schemeSuffix )
        {
            DestroyOidcUser( ctx, actorId, userId, schemeSuffix );
        }

        Task IGenericAuthenticationProvider.DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId, string? schemeSuffix, CancellationToken cancellationToken )
        {
            return DestroyOidcUserAsync( ctx, actorId, userId, schemeSuffix, cancellationToken );
        }

        #endregion
    }
}
