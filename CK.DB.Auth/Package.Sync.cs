using System;
using System.Collections.Generic;
using CK.SqlServer;
using Microsoft.Data.SqlClient;
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
        /// <param name="lastLoginTime">Last login time (<see cref="Util.UtcMinValue"/> for first login).</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="actualLogin">True for an actual login, false otherwise (only checks must be done).</param>
        /// <param name="loginTimeNow">Current login time.</param>
        /// <returns>The login result.</returns>
        [SqlProcedure( "sAuthUserOnLogin" )]
        public abstract LoginResult OnUserLogin( ISqlCallContext ctx, string scheme, DateTime lastLoginTime, int userId, bool actualLogin, DateTime loginTimeNow );

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
        public IUserAuthInfo ReadUserAuthInfo( ISqlCallContext ctx, int actorId, int userId )
        {
            AuthInfo Read( SqlCommand c )
            {
                using( var r = c.ExecuteReader() )
                {
                    if( !r.Read() ) return null;
                    var info = new AuthInfo();
                    info.UserId = r.GetInt32( 0 );
                    info.UserName = r.GetString( 1 );
                    if( r.NextResult() && r.Read() )
                    {
                        var providers = new List<UserAuthSchemeInfo>();
                        do
                        {
                            providers.Add( new UserAuthSchemeInfo( r.GetString( 0 ), r.GetDateTime( 1 ) ) );
                        }
                        while( r.Read() );
                        info.Schemes = providers;
                    }
                    else info.Schemes = Array.Empty<UserAuthSchemeInfo>();
                    return info;
                }
            }
            using( var cmd = CmdReadUserAuthInfo( actorId, userId ) )
            {
                return ctx[Database].ExecuteQuery( cmd, Read );
            }
        }

    }
}
