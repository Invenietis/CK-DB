using CK.Core;
using CK.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.DB.Auth
{
    /// <summary>
    /// Defines the ambient contract of the basic authentication provider (there 
    /// can be at most one such provider) that handles "PasswordUsers".
    /// </summary>
    public interface IBasicAuthenticationProvider : IAmbientContract
    {
        /// <summary>
        /// Associates a PasswordUser to an existing user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a password.</param>
        /// <param name="password">The initial password. Can not be null nor empty.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <returns>The operation result.</returns>
        CreateOrUpdateResult CreateOrUpdatePasswordUser( ISqlCallContext ctx, int actorId, int userId, string password, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate );

        /// <summary>
        /// Associates a PasswordUser to an existing user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a password.</param>
        /// <param name="password">The initial password. Can not be null nor empty.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The operation result.</returns>
        Task<CreateOrUpdateResult> CreateOrUpdatePasswordUserAsync( ISqlCallContext ctx, int actorId, int userId, string password, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Destroys a PasswordUser for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which Password information must be destroyed.</param>
        void DestroyPasswordUser( ISqlCallContext ctx, int actorId, int userId );

        /// <summary>
        /// Destroys a PasswordUser for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which Password information must be destroyed.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The awaitable.</returns>
        Task DestroyPasswordUserAsync( ISqlCallContext ctx, int actorId, int userId, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Changes the password of a PasswordUser.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a password.</param>
        /// <param name="password">The new password to set. Can not be null nor empty.</param>
        void SetPassword( ISqlCallContext ctx, int actorId, int userId, string password );

        /// <summary>
        /// Changes the password of a PasswordUser.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must have a new password.</param>
        /// <param name="password">The new password to set. Can not be null nor empty.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The awaitable.</returns>
        Task SetPasswordAsync( ISqlCallContext ctx, int actorId, int userId, string password, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Verifies a password for a user name.
        /// This must automatically updates the hash if the internal algorithm or one of its parameter is upgraded.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password to challenge.</param>
        /// <param name="actualLogin">Sets to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <returns>Non zero identifier of the user on success, 0 if the password does not match.</returns>
        int LoginUser( ISqlCallContext ctx, string userName, string password, bool actualLogin = true );

        /// <summary>
        /// Verifies a password for a user identifier.
        /// This must automatically updates the hash if the internal algorithm or one of its parameter is upgraded.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The password to challenge.</param>
        /// <param name="actualLogin">Sets to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <returns>Non zero identifier of the user on success, 0 if the password does not match.</returns>
        int LoginUser( ISqlCallContext ctx, int userId, string password, bool actualLogin = true );

        /// <summary>
        /// Verifies a password for a user name.
        /// This must automatically updates the hash if the internal algorithm or one of its parameter is upgraded.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password to challenge.</param>
        /// <param name="actualLogin">Sets to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Non zero identifier of the user on success, 0 if the password does not match.</returns>
        Task<int> LoginUserAsync( ISqlCallContext ctx, string userName, string password, bool actualLogin = true, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Verifies a password for a user identifier.
        /// This must automatically updates the hash if the internal algorithm or one of its parameter is upgraded.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The password to challenge.</param>
        /// <param name="actualLogin">Sets to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Non zero identifier of the user on success, 0 if the password does not match.</returns>
        Task<int> LoginUserAsync( ISqlCallContext ctx, int userId, string password, bool actualLogin = true, CancellationToken cancellationToken = default( CancellationToken ) );
    }
}
