using System;
using System.Collections.Generic;
using System.Text;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System.Data.SqlClient;
using CK.Core;

namespace CK.DB.Auth
{
    public abstract partial class Package
    {
        /// <summary>
        /// Calls the OnUserLogin hook.
        /// This is not intended to be called by code: this is public to allow edge case scenarii.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="scheme">The scheme used.</param>
        /// <param name="loginTime">Login time.</param>
        /// <param name="userId">The user identifier.</param>
        [SqlProcedure("sAuthUserOnLogin")]
        public abstract LoginResult OnUserLogin(ISqlCallContext ctx, string scheme, DateTime loginTime, int userId);

        class AuthInfo : IUserAuthInfo
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public IReadOnlyList<UserAuthSchemeInfo> Schemes { get; set; }
        }

        /// <summary>
        /// Reads a <see cref="IUserAuthInfo"/> for a user.
        /// Null for unexisting user or for the anonymous (<paramref name="userId"/> = 0).
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user information or null if the user identifier does not exist.</returns>
        public IUserAuthInfo ReadUserAuthInfo(ISqlCallContext ctx, int actorId, int userId)
        {
            using (var cmd = CmdReadUserAuthInfo(actorId, userId))
                try
                {
                    using ((cmd.Connection = ctx.GetConnection(this)).EnsureOpen())
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        var info = new AuthInfo();
                        info.UserId = reader.GetInt32(0);
                        info.UserName = reader.GetString(1);
                        if (reader.NextResult() && reader.Read())
                        {
                            var providers = new List<UserAuthSchemeInfo>();
                            do
                            {
                                providers.Add(new UserAuthSchemeInfo(reader.GetString(0), reader.GetDateTime(1)));
                            }
                            while (reader.Read());
                            info.Schemes = providers;
                        }
                        else info.Schemes = Util.Array.Empty<UserAuthSchemeInfo>();
                        return info;
                    }
                }
                catch (SqlException ex)
                {
                    throw SqlDetailedException.Create(cmd, ex);
                }
        }

    }
}
