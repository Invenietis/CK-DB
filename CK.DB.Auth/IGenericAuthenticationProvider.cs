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
    /// <para>
    /// Direct registration (ie. without explicit intermediate steps as when you directly register on a site 
    /// via Google or Facebook), when functionnally needed, must be done in, at least, two steps:
    /// <list type="number">
    /// <item><description>
    /// Call LoginUser to try to login the already registered user.
    /// </description></item>
    /// <item><description>
    /// If it fails, try to exploit the user data (typically the claims) to find an existing user registered
    /// with any other provider.
    /// If a match is found, either bind the existing account (if you trust the match) or confirm the match
    /// by using a confirmation email, sms, or any other means that can prove the identity match before registering
    /// it in its new provider.
    /// </description></item>
    /// <item><description>
    /// If no match can be found, create the user and register it.
    /// </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The untyped object payload is provider specific. <see cref="IBasicAuthenticationProvider"/> generic wrapper for 
    /// instance accepts a Tuple&lt;int,string&gt; (for userId, password), a Tuple&lt;string,string&gt;; (for userName, password) or any 
    /// IEnumerable&lt;KeyValuePair&lt;string,object&gt;&gt; into which it will lookup for a "Password" key with a string 
    /// value and a "UserId" key with an int value or a "UserName" key with a string value.
    /// </para>
    /// <para>
    /// Just like the basic one, providers SHOULD handle IDictionary&lt;string,object&gt; (or the more abstract IEnumerable&lt;KeyValuePair&lt;string,object&gt;&gt;)
    /// where the names of the keys match the names of their database columns.
    /// The helper <see cref="PocoFactotyExtensions.ExtractPayload{T}(IPocoFactory{T}, object)"/> does just that (but does not check required
    /// property, this is up to provider implementations).
    /// </para>
    /// <para>
    /// Payloads MUST be correct: a provider MUST throw an ArgumentException whenever the payload parameter is not 
    /// of the expected type. Any other kind of exception MUST be thrown if the payload does not carry the required 
    /// information to be able to <see cref="CreateOrUpdateUser"/> or <see cref="LoginUser"/>.
    /// </para>
    /// </summary>
    public interface IGenericAuthenticationProvider
    {
        /// <summary>
        /// Gets the name of the provider.
        /// This name is unique, must not contain any dot ('.'), is case insensitive (must use <see cref="StringComparison.OrdinalIgnoreCase"/>) 
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
        /// <returns>The result.</returns>
        CreateOrUpdateResult CreateOrUpdateUser( ISqlCallContext ctx, int actorId, int userId, object payload, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate );

        /// <summary>
        /// Destroys a registered user entry for this provider. 
        /// This is the "unregister from this provider" feature.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which provider information must be destroyed.</param>
        /// <param name="schemeSuffix">
        /// Optional scheme suffix for multi scheme providers.
        /// When null, all registrations for this provider regardless of the scheme suffix are deleted.
        /// </param>
        void DestroyUser( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix = null );

        /// <summary>
        /// Challenges provider dependent information to identify a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="payload">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <returns>The login result.</returns>
        LoginResult LoginUser( ISqlCallContext ctx, object payload, bool actualLogin = true );

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
        /// <returns>The result.</returns>
        Task<CreateOrUpdateResult> CreateOrUpdateUserAsync( ISqlCallContext ctx, int actorId, int userId, object payload, CreateOrUpdateMode mode = CreateOrUpdateMode.CreateOrUpdate, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Destroys a registered user entry for this provider. 
        /// This is the "unregister from this provider" feature.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier for which provider information must be destroyed.</param>
        /// <param name="schemeSuffix">
        /// Optional scheme suffix for multi scheme providers.
        /// When null, all registrations for this provider regardless of the scheme suffix are deleted.
        /// </param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix = null, CancellationToken cancellationToken = default( CancellationToken ) );

        /// <summary>
        /// Challenges provider dependent information to locate a user.
        /// Note that a successful challenge may have side effects such as updating claims, access tokens or other data
        /// related to the user and this provider.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="payload">The payload to challenge.</param>
        /// <param name="actualLogin">Set it to false to avoid login side-effect (such as updating the LastLoginTime) on success.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The login result.</returns>
        Task<LoginResult> LoginUserAsync( ISqlCallContext ctx, object payload, bool actualLogin = true, CancellationToken cancellationToken = default( CancellationToken ) );

    }
}
