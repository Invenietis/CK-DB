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
using CK.Text;
using System.Reflection;
using System.Data;

namespace CK.DB.User.UserSimpleCode
{
    /// <summary>
    /// SimpleCode authentication provider.
    /// </summary>
    [SqlTable( "tUserSimpleCode", Package = typeof( Package ), Schema = "CK" )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sUserDestroy" )]
    public abstract partial class UserSimpleCodeTable : SqlTable, IGenericAuthenticationProvider<IUserSimpleCodeInfo>
    {
        IPocoFactory<IUserSimpleCodeInfo> _infoFactory;

        /// <summary>
        /// Gets "SimpleCode" that is the name of the SimpleCode provider.
        /// </summary>
        public string ProviderName => "SimpleCode";

        void StObjConstruct( IPocoFactory<IUserSimpleCodeInfo> infoFactory )
        {
            _infoFactory = infoFactory;
        }

        IUserSimpleCodeInfo IGenericAuthenticationProvider<IUserSimpleCodeInfo>.CreatePayload() => _infoFactory.Create();

        /// <summary>
        /// Creates a <see cref="IUserSimpleCodeInfo"/> poco.
        /// </summary>
        /// <returns>A new instance.</returns>
        public T CreateUserInfo<T>() where T : IUserSimpleCodeInfo => (T)_infoFactory.Create();

        /// <summary>
        /// Creates or updates a user entry for this provider. 
        /// This is the "binding account" feature since it binds an external identity to 
        /// an already existing user that may already be registered into other authencation providers.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must be registered.</param>
        /// <param name="info">Provider specific data: the <see cref="IUserSimpleCodeInfo"/> poco.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result.</returns>
        public async Task<UCLResult> CreateOrUpdateSimpleCodeUserAsync( ISqlCallContext ctx, int actorId, int userId, IUserSimpleCodeInfo info, UCLMode mode = UCLMode.CreateOrUpdate, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            var r = await SimpleCodeUserUCLAsync( ctx, actorId, userId, info, mode, cancellationToken ).ConfigureAwait( false );
            return r;
        }

        /// <summary>
        /// Challenges <see cref="IUserSimpleCodeInfo"/> data to identify a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="info">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The login result.</returns>
        public async Task<LoginResult> LoginUserAsync( ISqlCallContext ctx, IUserSimpleCodeInfo info, bool actualLogin = true, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            var mode = actualLogin
                        ? UCLMode.UpdateOnly | UCLMode.WithActualLogin
                        : UCLMode.UpdateOnly | UCLMode.WithCheckLogin;
            var r = await SimpleCodeUserUCLAsync( ctx, 1, 0, info, mode, cancellationToken ).ConfigureAwait( false );
            return r.LoginResult;
        }

        /// <summary>
        /// Destroys a SimpleCode entry for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which SimpleCode information must be destroyed.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserSimpleCodeDestroy" )]
        public abstract Task DestroySimpleCodeUserAsync( ISqlCallContext ctx, int actorId, int userId, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Raw call to manage SimpleCode user. Since this should not be used directly, it is protected.
        /// Actual implementation of the centralized update, create or login procedure.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which a simple code must be created or its information must be updated.</param>
        /// <param name="info">User information to create or update.</param>
        /// <param name="mode">Configures Create, Update only or WithCheck/ActualLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result.</returns>
        [SqlProcedure( "sUserSimpleCodeUCL" )]
        protected abstract Task<UCLResult> SimpleCodeUserUCLAsync(
            ISqlCallContext ctx,
            int actorId,
            int userId,
            [ParameterSource]IUserSimpleCodeInfo info,
            UCLMode mode,
            CancellationToken cancellationToken );

        /// <summary>
        /// Finds a user by its SimpleCode identifier.
        /// Returns null if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="simpleCode">The user code.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A <see cref="IdentifiedUserInfo{T}"/> object or null if not found.</returns>
        public Task<IdentifiedUserInfo<IUserSimpleCodeInfo>> FindKnownUserInfoAsync( ISqlCallContext ctx, string simpleCode, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            using( var c = CreateReaderCommand( simpleCode ) )
            {
                return ctx[Database].ExecuteSingleRowAsync( c, r => r == null
                                                                ? null
                                                                : DoCreateUserUnfo( simpleCode, r ) );
            }
        }


        /// <summary>
        /// Creates a the reader command parametrized with the simple code identifier.
        /// Single-row returned columns are defined by <see cref="AppendUserInfoColumns(StringBuilder)"/>.
        /// </summary>
        /// <param name="simpleCode">Simple code to look for.</param>
        /// <returns>A ready to use reader command.</returns>
        SqlCommand CreateReaderCommand( string simpleCode )
        {
            StringBuilder b = new StringBuilder( "select " );
            AppendUserInfoColumns( b ).Append( " from CK.tUserSimpleCode where SimpleCode=@A" );
            var c = new SqlCommand( b.ToString() );
            c.Parameters.Add( new SqlParameter( "@A", simpleCode ) );
            return c;
        }

        IdentifiedUserInfo<IUserSimpleCodeInfo> DoCreateUserUnfo( string simpleCode, SqlDataRow r )
        {
            var info = _infoFactory.Create();
            info.SimpleCode = simpleCode;
            FillUserSimpleCodeInfo( info, r, 1 );
            return new IdentifiedUserInfo<IUserSimpleCodeInfo>( r.GetInt32( 0 ), info );
        }

        /// <summary>
        /// Adds the columns name to read.
        /// </summary>
        /// <param name="b">The string builder.</param>
        /// <returns>The string builder.</returns>
        protected virtual StringBuilder AppendUserInfoColumns( StringBuilder b )
        {
            var props = _infoFactory.PocoClassType.GetProperties().Where( p => p.Name != nameof( IUserSimpleCodeInfo.SimpleCode ) );
            return props.Any() ? b.Append( "UserId, " ).AppendStrings( props.Select( p => p.Name ) ) : b.Append( "UserId " );
        }

        /// <summary>
        /// Fill UserInfo properties from reader.
        /// </summary>
        /// <param name="info">The info to fill.</param>
        /// <param name="r">The record.</param>
        /// <param name="idx">The index of the first column.</param>
        /// <returns>The updated index.</returns>
        protected virtual int FillUserSimpleCodeInfo( IUserSimpleCodeInfo info, SqlDataRow r, int idx )
        {
            var props = _infoFactory.PocoClassType.GetProperties().Where( p => p.Name != nameof( IUserSimpleCodeInfo.SimpleCode ) );
            foreach( var p in props )
            {
                p.SetValue( info, r.GetValue( idx++ ) );
            }
            return idx;
        }

        #region IGenericAuthenticationProvider explicit implementation.

        UCLResult IGenericAuthenticationProvider.CreateOrUpdateUser( ISqlCallContext ctx, int actorId, int userId, object payload, UCLMode mode )
        {
            IUserSimpleCodeInfo info = _infoFactory.ExtractPayload( payload );
            return CreateOrUpdateSimpleCodeUser( ctx, actorId, userId, info, mode );
        }

        LoginResult IGenericAuthenticationProvider.LoginUser( ISqlCallContext ctx, object payload, bool actualLogin )
        {
            IUserSimpleCodeInfo info = _infoFactory.ExtractPayload( payload );
            return LoginUser( ctx, info, actualLogin );
        }

        Task<UCLResult> IGenericAuthenticationProvider.CreateOrUpdateUserAsync( ISqlCallContext ctx, int actorId, int userId, object payload, UCLMode mode, CancellationToken cancellationToken )
        {
            IUserSimpleCodeInfo info = _infoFactory.ExtractPayload( payload );
            return CreateOrUpdateSimpleCodeUserAsync( ctx, actorId, userId, info, mode, cancellationToken );
        }

        Task<LoginResult> IGenericAuthenticationProvider.LoginUserAsync( ISqlCallContext ctx, object payload, bool actualLogin, CancellationToken cancellationToken )
        {
            IUserSimpleCodeInfo info = _infoFactory.ExtractPayload( payload );
            return LoginUserAsync( ctx, info, actualLogin, cancellationToken );
        }

        void IGenericAuthenticationProvider.DestroyUser( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix )
        {
            DestroySimpleCodeUser( ctx, actorId, userId );
        }

        Task IGenericAuthenticationProvider.DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix, CancellationToken cancellationToken )
        {
            return DestroySimpleCodeUserAsync( ctx, actorId, userId, cancellationToken );
        }

        #endregion
    }
}
