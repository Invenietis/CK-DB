using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using CK.DB.Auth;
using System.Threading;

namespace CK.DB.User.UserPassword
{

    /// <summary>
    /// Holds password hashes for users and offer standard strong hash implementation:
    /// PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, with a default to 10000 iterations.
    /// Static <see cref="HashIterationCount"/> may be changed (typically at starting time).
    /// </summary>
    [SqlTable("tUserPassword", Package = typeof(Package))]
    [Versions("1.0.0,1.0.1")]
    [SqlObjectItem("transform:sUserDestroy")]
    public abstract partial class UserPasswordTable : SqlTable, IBasicAuthenticationProvider
    {
        Package _package;
        Actor.UserTable _userTable;

        static int _iterationCount;

        /// <summary>
        /// Current iteration count.
        /// Should be changed only at start and only if you know what you are doing.
        /// It can not be less than 1000 and defaults to <see cref="DefaultHashIterationCount"/> = 10000.
        /// </summary>
        static public int HashIterationCount
        {
            get { return _iterationCount; }
            set
            {
                if( value < 1000 ) throw new ArgumentException( "HashIterationCount must be at the very least 1000." );
                _iterationCount = value;
            }
        }

        /// <summary>
        /// The default <see cref="HashIterationCount"/> is 10000.
        /// </summary>
        public static readonly int DefaultHashIterationCount;

        static UserPasswordTable()
        {
            DefaultHashIterationCount = 10000;
            _iterationCount = DefaultHashIterationCount;
        }

        void StObjConstruct( Actor.UserTable userTable )
        {
            _userTable = userTable;
        }

        /// <summary>
        /// Gets the User password package.
        /// </summary>
        [InjectContract]
        public Package UserPasswordPackage
        {
            get { return _package; }
            protected set { _package = value; }
        }

        /// <summary>
        /// Associates a PasswordUser to an existing user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a password.</param>
        /// <param name="password">The initial password. Can not be null nor empty.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The result.</returns>
        public Task<CreateOrUpdateResult> CreateOrUpdatePasswordUserAsync( ISqlCallContext ctx, int actorId, int userId, string password, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if( string.IsNullOrEmpty( password ) ) throw new ArgumentNullException( nameof( password ) );
            PasswordHasher p = new PasswordHasher( HashIterationCount );
            return CreateOrUpdatePasswordUserWithRawPwdHashAsync( ctx, actorId, userId, p.HashPassword( password ), mode, cancellationToken );
        }

        /// <summary>
        /// Changes the password of a PasswordUser.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a new password.</param>
        /// <param name="password">The new password to set. Can not be null nor empty.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The awaitable.</returns>
        public Task SetPasswordAsync( ISqlCallContext ctx, int actorId, int userId, string password, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if( string.IsNullOrEmpty( password ) ) throw new ArgumentNullException( nameof( password ) );
            PasswordHasher p = new PasswordHasher( HashIterationCount );
            return CreateOrUpdatePasswordUserWithRawPwdHashAsync( ctx, actorId, userId, p.HashPassword( password ), CreateOrUpdateMode.UpdateOnly, cancellationToken );
        }

        /// <summary>
        /// Verifies a password for a user identifier.
        /// This automatically updates the hash if the <see cref="HashIterationCount"/> changed
        /// or if the internal algorithm is upgraded.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The password to challenge.</param>
        /// <param name="actualLogin">Sets to false to avoid any login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The login result.</returns>
        public Task<LoginResult> LoginUserAsync( ISqlCallContext ctx, int userId, string password, bool actualLogin = true, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            using( var c = new SqlCommand( $"select PwdHash, @UserId from CK.tUserPassword where UserId=@UserId" ) )
            {
                c.Parameters.AddWithValue( "@UserId", userId );
                return DoVerifyAsync( ctx, c, password, userId, actualLogin, cancellationToken );
            }
        }

        /// <summary>
        /// Verifies a password for a user name.
        /// This automatically updates the hash if the <see cref="HashIterationCount"/> changed
        /// or if the internal algorithm is upgraded.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password to challenge.</param>
        /// <param name="actualLogin">Sets to false to avoid any login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The login result.</returns>
        public Task<LoginResult> LoginUserAsync( ISqlCallContext ctx, string userName, string password, bool actualLogin = true, CancellationToken cancellationToken = default(CancellationToken) )
        {
            using( var c = CreateReadByNameCommand( userName ) )
            {
                return DoVerifyAsync( ctx, c, password, userName, actualLogin, cancellationToken );
            }
        }

        /// <summary>
        /// Creates the command to read the user hash and identifier fromits name.
        /// Defaults to: "select p.PwdHash, p.UserId from CK.tUserPassword p inner join CK.tUser u on u.UserId = p.UserId where u.UserName=@UserName".
        /// By overriding this, what is considered as the login name (currently the tUser.UserName) can be changed. 
        /// </summary>
        /// <param name="userName">The user name to lookup.</param>
        /// <returns>The command that must select the hash and user identifier.</returns>
        protected virtual SqlCommand CreateReadByNameCommand( string userName )
        {
            var c = new SqlCommand( "select p.PwdHash, p.UserId from CK.tUserPassword p inner join CK.tUser u on u.UserId = p.UserId where u.UserName=@UserName" );
            c.Parameters.AddWithValue( "@UserName", userName );
            return c;
        }

        async Task<LoginResult> DoVerifyAsync( ISqlCallContext ctx, SqlCommand hashReader, string password, object objectKey, bool actualLogin, CancellationToken cancellationToken )
        {
            if( string.IsNullOrEmpty( password ) ) return new LoginResult( "InvalidPassword" );
            // 1 - Get the PwdHash and UserId.
            //     hash is null if the user is not a UserPassword: we'll try to migrate it.
            byte[] hash = null;
            int userId = 0;
            using( await (hashReader.Connection = ctx[Database]).EnsureOpenAsync( cancellationToken ).ConfigureAwait( false ) )
            using( var r = await hashReader.ExecuteReaderAsync( System.Data.CommandBehavior.SingleRow, cancellationToken ).ConfigureAwait( false ) )
            {
                if( await r.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
                {
                    hash = r.GetSqlBytes( 0 ).Buffer;
                    userId = r.GetInt32( 1 );
                    if( userId == 0 ) return new LoginResult("InvalidUserKey");
                }
            }
            PasswordVerificationResult result = PasswordVerificationResult.Failed;
            PasswordHasher p = null;
            IUserPasswordMigrator migrator = null;
            // 2 - Handle external password migration or check the hash.
            if( hash == null )
            {
                migrator = _package.PasswordMigrator;
                if( migrator != null  )
                {
                    if( objectKey is int )
                    {
                        userId = (int)objectKey;
                    }
                    else
                    {
                        Debug.Assert( objectKey is string );
                        userId = await _userTable.FindByNameAsync( ctx, (string)objectKey ).ConfigureAwait( false );
                        if( userId == 0 ) return new LoginResult( "InvalidUserName" );
                    }
                    if( migrator.VerifyPassword( ctx, userId, password ) )
                    {
                        result = PasswordVerificationResult.SuccessRehashNeeded;
                        p = new PasswordHasher( HashIterationCount );
                    }
                }
            }
            else
            {
                p = new PasswordHasher( HashIterationCount );
                result = p.VerifyHashedPassword( hash, password );
            }
            // 3 - Handle result.
            if( result == PasswordVerificationResult.Failed ) return new LoginResult("InvalidPassword");
            if( result == PasswordVerificationResult.SuccessRehashNeeded )
            {
                // 3.1 - If migration occurred, create the user with its password.
                //       Else rehash the password and update the database.
                await CreateOrUpdatePasswordUserWithRawPwdHashAsync( ctx, 1, userId, p.HashPassword( password ), CreateOrUpdateMode.CreateOrUpdate, cancellationToken ).ConfigureAwait( false );
                if( migrator != null )
                {
                    migrator.MigrationDone( ctx, userId );
                }
            }
            if( actualLogin )
            {
                return await OnLoginAsync( ctx, userId ).ConfigureAwait( false );
            }
            return new LoginResult( userId );
        }

        /// <summary>
        /// Destroys a PasswordUser for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which Password information must be destroyed.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserPasswordDestroy" )]
        public abstract Task DestroyPasswordUserAsync( ISqlCallContext ctx, int actorId, int userId, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Creates a PasswordUser with an initial raw hash for an existing user.
        /// This method should be used only if the standard password hasher and verification 
        /// mechanism is not used.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for wich a PassworUser must be created.</param>
        /// <param name="pwdHash">The initial raw hash (no more than 64 bytes).</param>
        /// <param name="mode">Configures Create or Update only behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The operation result.</returns>
        [SqlProcedure("sUserPasswordCreateOrUpdate")]
        public abstract Task<CreateOrUpdateResult> CreateOrUpdatePasswordUserWithRawPwdHashAsync( ISqlCallContext ctx, int actorId, int userId, byte[] pwdHash, CreateOrUpdateMode mode, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Called once a login succeed (password hash verification done) and actualLogin parameter is true 
        /// or <see cref="CreateOrUpdateMode.WithLogin"/> is set.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userId">The user identifier that logged in.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserPasswordOnLogin" )]
        protected abstract Task<LoginResult> OnLoginAsync( ISqlCallContext ctx, int userId, CancellationToken cancellationToken = default( CancellationToken ) );


    }
}
