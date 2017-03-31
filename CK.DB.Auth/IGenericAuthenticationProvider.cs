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
    /// Generalizes authentication provider.
    /// An authentication provider can not create a new user by itself, it can only 
    /// register/bind to an existing user.
    /// Direct registration (without explicit intermediate steps as when you directly register on a site 
    /// via Google or Facebook), when functionnally needed, must be done in, at least, two steps:
    /// 1) Call LoginUser to try to login the already registered user.
    /// 2) If it fails, tries to exploit the user data (ie. the claims) to find an existing user registered
    /// with any other provider.
    /// If a match is found, either bind the existing account (if you trust the match) or confirm the match
    /// by using a confirmation email, sms, or any other means that can prove the identity match before registering
    /// it in its new provider.
    /// 3) If no match can be found, create the user and register it.
    /// </summary>
    public interface IGenericAuthenticationProvider
    {
        /// <summary>
        /// Gets the name of the provider.
        /// This name is unique, case insensitive (must use <see cref="StringComparison.OrdinalIgnoreCase"/>) 
        /// and must be the one added to the CK.tAuthProvider table.
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Creates or updates a user entry for this provider. 
        /// This is the "binding account" feature since it binds an external identity to 
        /// an already existing user that may already be registered into other authencation providers.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must be registered.</param>
        /// <param name="payload">Provider specific data.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <returns>The operation result.</returns>
        CreateOrUpdateResult CreateOrUpdateUser( ISqlCallContext ctx, int actorId, int userId, object payload, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate );

        /// <summary>
        /// Destroys a registered user entry for this provider. 
        /// This is the "unregister from this provider" feature.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which provider information must be destroyed.</param>
        void DestroyUser( ISqlCallContext ctx, int actorId, int userId );

        /// <summary>
        /// Challenges provider dependent information to identify a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="payload">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <returns>
        /// Null when the payload can not be handled by this provider, the positive identifier of the user on success or 0 if the challenge fails.
        /// </returns>
        int? LoginUser( ISqlCallContext ctx, object payload, bool actualLogin = true );

        /// <summary>
        /// Creates or updates a user entry for this provider. 
        /// This is the "binding account" feature since it binds an external identity to 
        /// an already existing user that may already be registered into other authencation providers.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier that must be registered.</param>
        /// <param name="payload">Provider specific data.</param>
        /// <param name="mode">Optionnaly configures Create, Update only or WithLogin behavior.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The operation result.</returns>
        Task<CreateOrUpdateResult> CreateOrUpdateUserAsync( ISqlCallContext ctx, int actorId, int userId, object payload, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Destroys a registered user entry for this provider. 
        /// This is the "unregister from this provider" feature.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which provider information must be destroyed.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Challenges provider dependent information to locate a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="payload">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>
        /// Null when the payload can not be handled by this provider, the positive identifier of the user on success or 0 if the challenge fails.
        /// </returns>
        Task<int?> LoginUserAsync( ISqlCallContext ctx, object payload, bool actualLogin = true, CancellationToken cancellationToken = default( CancellationToken ) );

    }
}
