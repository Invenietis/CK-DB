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

namespace CK.DB.User.UserPassword
{

    /// <summary>
    /// Holds password hashes for users and offer standard strong hash implementation:
    /// PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, with a default to 10000 iterations.
    /// Static <see cref="HashIterationCount"/> may be changed (typically at starting time).
    /// </summary>
    [SqlTable("tUserPassword", Package = typeof(Package))]
    [Versions("1.0.0")]
    [SqlObjectItem("transform:sUserDestroy")]
    public abstract partial class UserPasswordTable : SqlTable
    {
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

        /// <summary>
        /// Associates a PasswordUser to an existing user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a password.</param>
        /// <param name="password">The initial password. Can not be null nor empty.</param>
        /// <returns>The awaitable.</returns>
        public Task CreatePasswordUserAsync(ISqlCallContext ctx, int actorId, int userId, string password)
        {
            if( string.IsNullOrEmpty( password ) ) throw new ArgumentNullException( nameof( password ) );
            PasswordHasher p = new PasswordHasher(HashIterationCount);
            return CreatePasswordUserWithRawPwdHashAsync( ctx, actorId, userId, p.HashPassword( password ) ); 
        }

        /// <summary>
        /// Changes the password of a PasswordUser.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a password.</param>
        /// <param name="password">The new password to set. Can not be null nor empty.</param>
        /// <returns>The awaitable.</returns>
        public Task SetPasswordAsync( ISqlCallContext ctx, int actorId, int userId, string password )
        {
            if( string.IsNullOrEmpty( password ) ) throw new ArgumentNullException( nameof( password ) );
            PasswordHasher p = new PasswordHasher( HashIterationCount );
            return SetPwdRawHashAsync( ctx, actorId, userId, p.HashPassword( password ) );
        }

        /// <summary>
        /// Verifies a password for a user identifier.
        /// This automatically updates the hash if the <see cref="HashIterationCount"/> changed
        /// or if the internal algorithm is upgraded.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The password to challenge.</param>
        /// <returns>True on success, false if the password does not match.</returns>
        public Task<bool> VerifyAsync(ISqlCallContext ctx, int userId, string password)
        {
            using (var c = new SqlCommand($"select PwdHash, @UserId from CK.tUserPassword where UserId=@UserId"))
            {
                c.Parameters.AddWithValue("@UserId", userId);
                return DoVerifyAsync(ctx, c, password);
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
        /// <returns>True on success, false if the password does not match.</returns>
        public Task<bool> VerifyAsync(ISqlCallContext ctx, string userName, string password)
        {
            using (var c = new SqlCommand($"select p.PwdHash, p.UserId from CK.tUserPassword p inner join CK.tUser u on u.UserId = p.UserId where u.UserName=@UserName"))
            {
                c.Parameters.AddWithValue("@UserName", userName);
                return DoVerifyAsync(ctx, c, password);
            }
        }

        async Task<bool> DoVerifyAsync(ISqlCallContext ctx, SqlCommand hashReader, string password )
        {
            if( string.IsNullOrEmpty( password ) ) return false;

            // 1 - Get the PwdHash and UserId.
            byte[] hash;
            int userId;
            using( await (hashReader.Connection = ctx[Database]).EnsureOpenAsync() )
            using( var r = await hashReader.ExecuteReaderAsync( System.Data.CommandBehavior.SingleRow ) )
            {
                if( !await r.ReadAsync() ) return false;
                hash = r.GetSqlBytes( 0 ).Buffer;
                userId = r.GetInt32( 1 );
            }
            // 2 - Check it.
            PasswordHasher p = new PasswordHasher(HashIterationCount);
            var result = p.VerifyHashedPassword(hash, password);
            switch (result)
            {
                case PasswordVerificationResult.Failed: return false;
                case PasswordVerificationResult.SuccessRehashNeeded:
                    {
                        // 3 - Rehash the password and update the database.
                        await SetPwdRawHashAsync(ctx, 1, userId, p.HashPassword(password)); 
                        return true;
                    }
                default:
                    {
                        Debug.Assert(result == PasswordVerificationResult.Success);
                        return true;
                    }
            }
        }

        /// <summary>
        /// Destroys a PasswordUser for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier to destroy.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure("sUserPasswordDestroy")]
        public abstract Task DestroyPasswordUserAsync(ISqlCallContext ctx, int actorId, int userId);

        /// <summary>
        /// Creates a PasswordUser with an initial raw hash for an existing user.
        /// This method should be used only if the standard password hasher and verfication 
        /// mechanism is not used.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for wich a PassworUser must be created.</param>
        /// <param name="pwdHash">The initial raw hash (no more than 64 bytes).</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure("sUserPasswordCreate")]
        public abstract Task CreatePasswordUserWithRawPwdHashAsync( ISqlCallContext ctx, int actorId, int userId, byte[] pwdHash);

        /// <summary>
        /// Sets a raw hash to a PasswordUser.
        /// This method should be used only if the standard password hasher and verfication 
        /// mechanism is not used.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for wich a raw hash must be set.</param>
        /// <param name="pwdHash">The raw hash to set (no more than 64 bytes).</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure("sUserPasswordPwdHashSet")]
        public abstract Task SetPwdRawHashAsync( ISqlCallContext ctx, int actorId, int userId, byte[] pwdHash);
    }
}
