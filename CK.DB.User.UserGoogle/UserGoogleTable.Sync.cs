using CK.Core;
using CK.DB.Auth;
using CK.SqlServer;

namespace CK.DB.User.UserGoogle;

public abstract partial class UserGoogleTable
{
    /// <summary>
    /// Creates or updates a user entry for this provider. 
    /// This is the "binding account" feature since it binds an external identity to 
    /// an already existing user that may already be registered into other authencation providers.
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The user identifier that must be registered.</param>
    /// <param name="info">Provider specific data: the <see cref="IUserGoogleInfo"/> poco.</param>
    /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
    /// <returns>The result.</returns>
    public UCLResult CreateOrUpdateGoogleUser( ISqlCallContext ctx, int actorId, int userId, IUserGoogleInfo info, UCLMode mode = UCLMode.CreateOrUpdate )
    {
        return UserGoogleUCL( ctx, actorId, userId, info, mode );
    }

    /// <summary>
    /// Challenges <see cref="IUserGoogleInfo"/> data to identify a user.
    /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
    /// related to the user and this provider.
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="info">The payload to challenge.</param>
    /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
    /// <returns>The login result.</returns>
    public LoginResult LoginUser( ISqlCallContext ctx, IUserGoogleInfo info, bool actualLogin = true )
    {
        var mode = actualLogin
                    ? UCLMode.UpdateOnly | UCLMode.WithActualLogin
                    : UCLMode.UpdateOnly | UCLMode.WithCheckLogin;
        var r = UserGoogleUCL( ctx, 1, 0, info, mode );
        return r.LoginResult;
    }

    /// <summary>
    /// Destroys a GoogleUser for a user.
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The user identifier for which Google account information must be destroyed.</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "sUserGoogleDestroy" )]
    public abstract void DestroyGoogleUser( ISqlCallContext ctx, int actorId, int userId );

    /// <summary>
    /// Finds a user by its Google account identifier.
    /// Returns null if no such user exists.
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="googleAccountId">The google account identifier.</param>
    /// <returns>A <see cref="IdentifiedUserInfo{T}"/> or null if not found.</returns>
    public IdentifiedUserInfo<IUserGoogleInfo>? FindKnownUserInfo( ISqlCallContext ctx, string googleAccountId )
    {
        using( var c = CreateReaderCommand( googleAccountId ) )
        {
            return ctx[Database].ExecuteSingleRow( c, r => r == null
                                                        ? null
                                                        : DoCreateUserUnfo( googleAccountId, r ) );
        }
    }

    /// <summary>
    /// Raw call to manage GoogleUser. Since this should not be used directly, it is protected.
    /// Actual implementation of the centralized update, create or login procedure.
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The user identifier for which a Google account must be created or updated.</param>
    /// <param name="info">User information to create or update.</param>
    /// <param name="mode">Configures Create, Update only or WithCheck/ActualLogin behavior.</param>
    /// <returns>The result.</returns>
    [SqlProcedure( "sUserGoogleUCL" )]
    protected abstract UCLResult UserGoogleUCL(
        ISqlCallContext ctx,
        int actorId,
        int userId,
        [ParameterSource] IUserGoogleInfo info,
        UCLMode mode );


}
