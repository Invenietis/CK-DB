using System.Threading.Tasks;
using CK.SqlServer;
using CK.Core;
using CK.Auth;

namespace CK.DB.Auth;

/// <summary>
/// Implements the <see cref="IUserInfoProvider"/> defined in CK.Auth.Abstractions.
/// </summary>
public sealed class UserInfoProvider : IUserInfoProvider
{
    readonly IAuthenticationTypeSystem _typeSystem;
    readonly IAuthenticationDatabaseService _dbAuth;

    /// <summary>
    /// Initializes a new <see cref="UserInfoProvider"/>.
    /// </summary>
    /// <param name="typeSystem">The type system.</param>
    /// <param name="dbAuth">The authentication database service.</param>
    public UserInfoProvider( IAuthenticationTypeSystem typeSystem, IAuthenticationDatabaseService dbAuth )
    {
        _typeSystem = typeSystem;
        _dbAuth = dbAuth;
    }

    /// <inheritdoc />
    public async ValueTask<IUserInfo> GetUserInfoAsync( IActivityMonitor monitor, int userId )
    {
        if( userId != 0 )
        {
            using( var ctx = new SqlStandardCallContext( monitor ) )
            {
                var dbInfo = await _dbAuth.ReadUserAuthInfoAsync( ctx, 1, userId );
                if( dbInfo != null )
                {
                    return _typeSystem.UserInfo.Create( dbInfo.UserId, dbInfo.UserName, dbInfo.Schemes );
                }
                monitor.Warn( $"Unable to read UserInfo for UserId={userId}. Returning the Anonymous." );
            }
        }
        return _typeSystem.UserInfo.Anonymous;
    }
}
