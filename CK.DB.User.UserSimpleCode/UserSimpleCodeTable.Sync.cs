using CK.Core;
using CK.DB.Auth;
using CK.SqlServer;

namespace CK.DB.User.UserSimpleCode
{
    public abstract partial class UserSimpleCodeTable
    {
        /// <summary>
        /// Creates or updates a user entry for this provider. 
        /// This is the "binding account" feature since it binds an external identity to 
        /// an already existing user that may already be registered into other authencation providers.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must be registered.</param>
        /// <param name="info">Provider specific data: the <see cref="IUserSimpleCodeInfo"/> poco.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <returns>The operation result.</returns>
        public UCLResult CreateOrUpdateSimpleCodeUser( ISqlCallContext ctx, int actorId, int userId, IUserSimpleCodeInfo info, UCLMode mode = UCLMode.CreateOrUpdate )
        {
            return UserSimpleCodeUCL( ctx, actorId, userId, info, mode );
        }

        /// <summary>
        /// Challenges <see cref="IUserSimpleCodeInfo"/> data to identify a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="info">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <returns>The login result.</returns>
        public LoginResult LoginUser( ISqlCallContext ctx, IUserSimpleCodeInfo info, bool actualLogin = true )
        {
            var mode = actualLogin
                        ? UCLMode.UpdateOnly | UCLMode.WithActualLogin
                        : UCLMode.UpdateOnly | UCLMode.WithCheckLogin;
            var r = UserSimpleCodeUCL( ctx, 1, 0, info, mode );
            return r.LoginResult;
        }

        /// <summary>
        /// Destroys a SimpleCode entry for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which simple code information must be destroyed.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sUserSimpleCodeDestroy" )]
        public abstract void DestroySimpleCodeUser( ISqlCallContext ctx, int actorId, int userId );

        /// <summary>
        /// Finds a user by its SimpleCode.
        /// Returns null if no such user exists.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="simpleCode">The simple code.</param>
        /// <returns>A <see cref="IdentifiedUserInfo{T}"/> or null if not found.</returns>
        public IdentifiedUserInfo<IUserSimpleCodeInfo> FindKnownUserInfo( ISqlCallContext ctx, string simpleCode )
        {
            using( var c = CreateReaderCommand( simpleCode ) )
            {
                return ctx[Database].ExecuteSingleRow( c, r => r == null
                                                            ? null
                                                            : DoCreateUserUnfo( simpleCode, r ) );
            }
        }

        /// <summary>
        /// Raw call to manage SimpleCode user. Since this should not be used directly, it is protected.
        /// Actual implementation of the centralized update, create or login procedure.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which a SimpleCode account must be created or updated.</param>
        /// <param name="info">User information to create or update.</param>
        /// <param name="mode">Configures Create, Update only or WithCheck/ActualLogin behavior.</param>
        /// <returns>The user identifier (when <paramref name="userId"/> is 0, this is a login) and the operation result.</returns>
        [SqlProcedure( "sUserSimpleCodeUCL" )]
        protected abstract UCLResult UserSimpleCodeUCL(
            ISqlCallContext ctx,
            int actorId,
            int userId,
            [ParameterSource]IUserSimpleCodeInfo info,
            UCLMode mode );


    }
}
