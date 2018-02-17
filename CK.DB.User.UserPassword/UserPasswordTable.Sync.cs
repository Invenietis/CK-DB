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
        /// <param name="mode">Optionnaly configures Create or Update only behavior.</param>
        /// <returns>The operation result.</returns>
        public UCLResult CreateOrUpdatePasswordUser( ISqlCallContext ctx, int actorId, int userId, string password, UCLMode mode = UCLMode.CreateOrUpdate )
        {
            if( string.IsNullOrEmpty( password ) ) throw new ArgumentNullException( nameof( password ) );
            PasswordHasher p = new PasswordHasher( HashIterationCount );
            return PasswordUserUCL( ctx, actorId, userId, p.HashPassword( password ), mode, null );
        }

        /// <summary>
        /// Changes the password of a PasswordUser.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a password.</param>
        /// <param name="password">The new password to set. Can not be null nor empty.</param>
        public void SetPassword( ISqlCallContext ctx, int actorId, int userId, string password )
        {
            if( string.IsNullOrEmpty( password ) ) throw new ArgumentNullException( nameof( password ) );
            PasswordHasher p = new PasswordHasher( HashIterationCount );
            PasswordUserUCL( ctx, actorId, userId, p.HashPassword( password ), UCLMode.UpdateOnly, null );
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
        /// <returns>The login result.</returns>
        public LoginResult LoginUser( ISqlCallContext ctx, int userId, string password, bool actualLogin = true )
        {
            using( var c = new SqlCommand( _commandReadByUserId ) )
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
        public LoginResult LoginUser( ISqlCallContext ctx, string userName, string password, bool actualLogin = true )
        {
            using( var c = CreateReadByNameCommand( userName ) )
            {
                return DoVerify( ctx, c, password, userName, actualLogin );
            }
        }

        LoginResult DoVerify( ISqlCallContext ctx, SqlCommand hashReader, string password, object objectKey, bool actualLogin )
        {
            if( string.IsNullOrEmpty( password ) ) return new LoginResult( KnownLoginFailureCode.InvalidCredentials );
            // 1 - Get the PwdHash and UserId.
            //     hash is null if the user is not a UserPassword: we'll try to migrate it.
            var read = ctx[Database].ExecuteSingleRow( hashReader,
                            row => row == null
                                    ? ( 0, null )
                                    : ( UserId : row.GetInt32( 1 ),
                                        PwdHash : row.IsDBNull( 0 )
                                                    ? null
                                                    : row.GetBytes( 0 ) ) );
            if( read.UserId == 0 ) return new LoginResult( KnownLoginFailureCode.InvalidUserKey );

            // If hash is null here, it means that the user is not registered.
            PasswordVerificationResult result = PasswordVerificationResult.Failed;
            PasswordHasher p = null;
            IUserPasswordMigrator migrator = null;
            // 2 - Handle external password migration or check the hash.
            if( read.PwdHash == null )
            {
                migrator = _package.PasswordMigrator;
                if( migrator != null && migrator.VerifyPassword( ctx, read.UserId, password ) )
                {
                    result = PasswordVerificationResult.SuccessRehashNeeded;
                    p = new PasswordHasher( HashIterationCount );
                }
            }
            else
            {
                p = new PasswordHasher( HashIterationCount );
                result = p.VerifyHashedPassword( read.PwdHash, password );
            }
            // 3 - Handle result.
            var mode = actualLogin ? UCLMode.WithActualLogin : UCLMode.WithCheckLogin;
            if( result == PasswordVerificationResult.SuccessRehashNeeded )
            {
                // 3.1 - If migration occurred, create the user with its password.
                //       Else rehash the password and update the database.
                mode |= UCLMode.CreateOrUpdate;
                UCLResult r = PasswordUserUCL( ctx, 1, read.UserId, p.HashPassword( password ), mode, null );
                if( r.OperationResult != UCResult.None && migrator != null )
                {
                    migrator.MigrationDone( ctx, read.UserId );
                }
                return r.LoginResult;
            }
            // 4 - Challenges the database login checks.
            int? failureCode = null;
            if( result == PasswordVerificationResult.Failed ) failureCode = (int)KnownLoginFailureCode.InvalidCredentials;
            return PasswordUserUCL( ctx, 1, read.UserId, null, mode, failureCode ).LoginResult;
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
        /// Low level stored procedure.
        /// This method should be used only if the standard password hasher and verification 
        /// mechanism is not used.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for wich a PassworUser must be created.</param>
        /// <param name="pwdHash">The initial raw hash (no more than 64 bytes).</param>
        /// <param name="mode">Configures Create, Update and/or WithLogin behaviors.</param>
        /// <param name="loginFailureCode">
        /// Login failure code (it is the <see cref="KnownLoginFailureCode.InvalidCredentials"/> when
        /// password match has failed).
        /// </param>
        /// <returns>The result.</returns>
        [SqlProcedure( "sUserPasswordUCL" )]
        protected abstract UCLResult PasswordUserUCL( ISqlCallContext ctx, int actorId, int userId, byte[] pwdHash, UCLMode mode, int? loginFailureCode );

    }
}
