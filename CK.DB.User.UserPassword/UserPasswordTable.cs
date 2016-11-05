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
    /// Contains the password hash for users.
    /// </summary>
    [SqlTable("tUserPassword", Package = typeof(Package))]
    [Versions("1.0.0")]
    public abstract partial class UserPasswordTable : SqlTable
    {
        static public int HashIterationCount = 10000;
        /// <summary>
        /// initializes a password for a user.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="actorId"></param>
        /// <param name="userId"></param>
        /// <param name="pwd"></param>
        /// <returns>The awaitable.</returns>
        public Task CreateAsync(ISqlCallContext ctx, int actorId, int userId, string pwd)
        {
            PasswordHasher p = new PasswordHasher(HashIterationCount);
            return CreateWithRawHashAsync(ctx, actorId, userId, p.HashPassword(pwd)); 
        }

        /// <summary>
        /// Sets a password for a user
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="actorId"></param>
        /// <param name="userId"></param>
        /// <param name="pwd"></param>
        /// <returns>The awaitable.</returns>
        public Task SetPasswordAsync(ISqlCallContext ctx, int actorId, int userId, string pwd)
        {
            PasswordHasher p = new PasswordHasher(HashIterationCount);
            return SetRawHashAsync(ctx, actorId, userId, p.HashPassword(pwd));
        }

        /// <summary>
        /// Verifies a password for a user Identifier.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns>The awaitable.</returns>
        public Task<bool> Verify(ISqlCallContext ctx, int userId, string password)
        {
            using (var c = new SqlCommand($"select PwdHash, @UserId from CK.tUserPassword where UserId=@UserId"))
            {
                c.Parameters.AddWithValue("@UserId", userId);
                return DoVerifyAsync(ctx, c, password);
            }
        }

        /// <summary>
        /// Verifies a password for a user name.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>True on success, false otherwise.</returns>
        public Task<bool> Verify(ISqlCallContext ctx, string userName, string password)
        {
            using (var c = new SqlCommand($"select p.PwdHash, p.UserId from CK.tUserPassword p inner join CK.tUser u on u.UserId = p.UserId where u.UserName=@UserName"))
            {
                c.Parameters.AddWithValue("@UserName", userName);
                return DoVerifyAsync(ctx, c, password);
            }
        }

        async Task<bool> DoVerifyAsync(ISqlCallContext ctx, SqlCommand hashReader, string password )
        { 
            // 1 - Get the PwdHash.
            object[] hashAndUserId = await ctx.Executor.GetProvider( Database.ConnectionString ).ReadFirstRowAsync( hashReader );
            byte[] hash = (byte[])hashAndUserId[0];

            // 2 - Check it.
            PasswordHasher p = new PasswordHasher(HashIterationCount);
            var result = p.VerifyHashedPassword(hash, password);
            switch (result)
            {
                case PasswordVerificationResult.Failed: return false;
                case PasswordVerificationResult.SuccessRehashNeeded:
                    {
                        // 3 - Rehash the password and update the database.
                        int userId = (int)hashAndUserId[1];
                        await SetRawHashAsync(ctx, 1, userId, p.HashPassword(password)); 
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
        /// Destroys a password for an user;
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="actorId"></param>
        /// <param name="userId"></param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure("sUserPasswordDestroy")]
        public abstract Task DestroyAsync(ISqlCallContext ctx, int actorId, int userId);

        /// <summary>
        /// Creates a PasswordUser with an initial raw hash for an existing user.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="actorId"></param>
        /// <param name="userId"></param>
        /// <param name="pwdHash"></param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure("sUserPasswordCreate")]
        public abstract Task CreateWithRawHashAsync(ISqlCallContext ctx, int actorId, int userId, byte[] pwdHash);

        /// <summary>
        /// Sets a raw hash to a PasswordUser.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="actorId"></param>
        /// <param name="userId"></param>
        /// <param name="pwdHash"></param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure("sUserPasswordPwdHashSet")]
        public abstract Task SetRawHashAsync(ISqlCallContext ctx, int actorId, int userId, byte[] pwdHash);
    }
}
