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
        /// <returns>True on success, false if the password does not match.</returns>
        public bool Verify( ISqlCallContext ctx, int userId, string password )
        {
            using( var c = new SqlCommand( $"select PwdHash, @UserId from CK.tUserPassword where UserId=@UserId" ) )
            {
                c.Parameters.AddWithValue( "@UserId", userId );
                return DoVerify( ctx, c, password );
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
        public bool Verify( ISqlCallContext ctx, string userName, string password )
        {
            using( var c = new SqlCommand( $"select p.PwdHash, p.UserId from CK.tUserPassword p inner join CK.tUser u on u.UserId = p.UserId where u.UserName=@UserName" ) )
            {
                c.Parameters.AddWithValue( "@UserName", userName );
                return DoVerify( ctx, c, password );
            }
        }

        bool DoVerify( ISqlCallContext ctx, SqlCommand hashReader, string password )
        {
            if( string.IsNullOrEmpty( password ) ) return false;

            // 1 - Get the PwdHash.
            byte[] hash;
            int userId;
            using( ( hashReader.Connection = ctx[Database] ).EnsureOpen() )
            using( var r = hashReader.ExecuteReader( System.Data.CommandBehavior.SingleRow ) )
            {
                if( !r.Read() ) return false;
                hash = r.GetSqlBytes( 0 ).Buffer;
                userId = r.GetInt32( 1 );
            }

            // 2 - Check it.
            PasswordHasher p = new PasswordHasher( HashIterationCount );
            var result = p.VerifyHashedPassword( hash, password );
            switch( result )
            {
                case PasswordVerificationResult.Failed: return false;
                case PasswordVerificationResult.SuccessRehashNeeded:
                    {
                        // 3 - Rehash the password and update the database.
                        SetPwdRawHash( ctx, 1, userId, p.HashPassword( password ) );
                        return true;
                    }
                default:
                    {
                        Debug.Assert( result == PasswordVerificationResult.Success );
                        return true;
                    }
            }
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
    }
}
