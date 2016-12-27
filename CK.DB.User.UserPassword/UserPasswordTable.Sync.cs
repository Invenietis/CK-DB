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

    public abstract partial class UserPasswordTable : SqlTable
    {
        /// <summary>
        /// Associates a PasswordUser to an existing user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a password.</param>
        /// <param name="password">The initial password. Can not be null nor empty.</param>
        public void CreatePasswordUser( ISqlCallContext ctx, int actorId, int userId, string password )
        {
            if( string.IsNullOrEmpty( password ) ) throw new ArgumentNullException( nameof( password ) );
            PasswordHasher p = new PasswordHasher( HashIterationCount );
            CreatePasswordUserWithPwdRawHash( ctx, actorId, userId, p.HashPassword( password ) );
        }

        /// <summary>
        /// Changes the password of a PasswordUser.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a password.</param>
        /// <param name="password">The new password to set. Can not be null nor empty.</param>
        /// <returns>The awaitable.</returns>
        public void SetPassword( ISqlCallContext ctx, int actorId, int userId, string password )
        {
            if( string.IsNullOrEmpty( password ) ) throw new ArgumentNullException( nameof( password ) );
            PasswordHasher p = new PasswordHasher( HashIterationCount );
            SetPwdRawHash( ctx, actorId, userId, p.HashPassword( password ) );
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
        /// <returns>Non zero identifier of the user on success, 0 if the password does not match.</returns>
        public int Verify( ISqlCallContext ctx, int userId, string password, bool actualLogin = true )
        {
            using( var c = new SqlCommand( $"select PwdHash, @UserId from CK.tUserPassword where UserId=@UserId" ) )
            {
                c.Parameters.AddWithValue( "@UserId", userId );
                return DoVerify( ctx, c, password, userId, actualLogin );
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
        /// <returns>Non zero identifier of the user on success, 0 if the password does not match.</returns>
        public int Verify( ISqlCallContext ctx, string userName, string password, bool actualLogin = true )
        {
            using( var c = new SqlCommand( $"select p.PwdHash, p.UserId from CK.tUserPassword p inner join CK.tUser u on u.UserId = p.UserId where u.UserName=@UserName" ) )
            {
                c.Parameters.AddWithValue( "@UserName", userName );
                return DoVerify( ctx, c, password, userName, actualLogin );
            }
        }

        int DoVerify( ISqlCallContext ctx, SqlCommand hashReader, string password, object objectKey, bool actualLogin )
        {
            if( string.IsNullOrEmpty( password ) ) return 0;
            // 1 - Get the PwdHash and UserId.
            //     hash is null if the user is not a UserPassword: we'll try to migrate it.
            byte[] hash = null;
            int userId = 0;
            using( (hashReader.Connection = ctx[Database] ).EnsureOpen() )
            using( var r = hashReader.ExecuteReader( System.Data.CommandBehavior.SingleRow ) )
            {
                if( r.Read() )
                {
                    hash = r.GetSqlBytes( 0 ).Buffer;
                    userId = r.GetInt32( 1 );
                    if( userId == 0 ) return 0;
                }
            }
            PasswordVerificationResult result = PasswordVerificationResult.Failed;
            PasswordHasher p = null;
            IUserPasswordMigrator migrator = null;
            // 2 - Handle external password migration or check the hash.
            if( hash == null )
            {
                migrator = _package.PasswordMigrator;
                if( migrator != null )
                {
                    if( objectKey is int )
                    {
                        userId = (int)objectKey;
                    }
                    else
                    {
                        Debug.Assert( objectKey is string );
                        userId = _userTable.FindByName( ctx, (string)objectKey );
                        if( userId == 0 ) return 0;
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
            if( result == PasswordVerificationResult.Failed ) return 0;
            if( result == PasswordVerificationResult.SuccessRehashNeeded )
            {
                // 3.1 - If migration occurred, create the user with its password.
                //       Else rehash the password and update the database.
                if( migrator != null )
                {
                    CreatePasswordUser( ctx, 1, userId, password );
                    migrator.MigrationDone( ctx, userId );
                }
                else SetPwdRawHash( ctx, 1, userId, p.HashPassword( password ) );
            }
            // 4 - Side-effect of successful login.
            if( actualLogin )
            {
                OnLogin( ctx, ref userId );
            }
            return userId;
       }

        /// <summary>
        /// Destroys a PasswordUser for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which Password information must be destroyed.</param>
        [SqlProcedure( "sUserPasswordDestroy" )]
        public abstract void DestroyPasswordUser( ISqlCallContext ctx, int actorId, int userId );

        /// <summary>
        /// Creates a PasswordUser with an initial raw hash for an existing user.
        /// This method should be used only if the standard password hasher and verfication 
        /// mechanism is not used.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for wich a PassworUser must be created.</param>
        /// <param name="pwdHash">The initial raw hash (no more than 64 bytes).</param>
        [SqlProcedure( "sUserPasswordCreate" )]
        public abstract void CreatePasswordUserWithPwdRawHash( ISqlCallContext ctx, int actorId, int userId, byte[] pwdHash );

        /// <summary>
        /// Sets a raw hash to a PasswordUser.
        /// This method should be used only if the standard password hasher and verfication 
        /// mechanism is not used.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for wich a raw hash must be set.</param>
        /// <param name="pwdHash">The raw hash to set (no more than 64 bytes).</param>
        [SqlProcedure( "sUserPasswordPwdHashSet" )]
        public abstract void SetPwdRawHash( ISqlCallContext ctx, int actorId, int userId, byte[] pwdHash );

        /// <summary>
        /// Called once a login succeed (password hash verification done) and actualLogin parameter is true.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userId">
        /// The user identifier that logged in: this extension point may change it (setting
        /// it to 0 de facto forbids login).
        /// </param>
        [SqlProcedure( "sUserPasswordOnLogin" )]
        protected abstract void OnLogin( ISqlCallContext ctx, ref int userId );
    }
}
