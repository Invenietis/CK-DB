using System;
using System.Collections.Generic;
using CK.SqlServer;
using Microsoft.Data.SqlClient;
using CK.Core;
using CK.Auth;

namespace CK.DB.Auth;

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

    record AuthInfo( int UserId, string UserName, IReadOnlyList<StdUserSchemeInfo> Schemes ) : IUserAuthInfo;

    /// <summary>
    /// Reads a <see cref="IUserAuthInfo"/> for a user.
    /// Null for unexisting user or for the anonymous (<paramref name="userId"/> = 0).
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The user information or null if the user identifier does not exist.</returns>
    public IUserAuthInfo? ReadUserAuthInfo( ISqlCallContext ctx, int actorId, int userId )
    {
        static AuthInfo? DoRead( SqlCommand c )
        {
            using( var r = c.ExecuteReader() )
            {
                if( !r.Read() ) return null;
                var userId = r.GetInt32( 0 );
                var userName = r.GetString( 1 );
                List<StdUserSchemeInfo>? schemes = null;
                if( r.NextResult() && r.Read() )
                {
                    schemes = new List<StdUserSchemeInfo>();
                    do
                    {
                        schemes.Add( new StdUserSchemeInfo( r.GetString( 0 ), r.GetDateTime( 1 ) ) );
                    }
                    while( r.Read() );
                }
                return new AuthInfo( userId, userName, (IReadOnlyList<StdUserSchemeInfo>?)schemes ?? Array.Empty<StdUserSchemeInfo>() );
            }
        }
        using( var cmd = CmdReadUserAuthInfo( actorId, userId ) )
        {
            return ctx[Database].ExecuteQuery( cmd, DoRead );
        }
    }

}
