using System.Collections.Generic;
using System.Threading.Tasks;
using CK.SqlServer;
using CK.Core;
using System;

namespace CK.DB.Auth
{
    /// <summary>
    /// Authentication service that centralizes the different authentication providers
    /// installed in this application context and exposes a way to read <see cref="IUserAuthInfo"/>
    /// information.
    /// </summary>
    public interface IAuthenticationDatabaseService : IAmbientService
    {
        /// <summary>
        /// Gets the only <see cref="IBasicAuthenticationProvider"/> if it exists or null.
        /// </summary>
        IBasicAuthenticationProvider BasicProvider { get; }

        /// <summary>
        /// Gets the collection of existing providers, including an adapter of <see cref="IBasicAuthenticationProvider"/> 
        /// if it exists.
        /// </summary>
        IReadOnlyCollection<IGenericAuthenticationProvider> AllProviders { get; }

        /// <summary>
        /// Finds a <see cref="IGenericAuthenticationProvider"/> by its name (using <see cref="StringComparer.OrdinalIgnoreCase"/>).
        /// This methods accepts a scheme: it is the provider name followed by a, optional dotted suffix and
        /// in such case only the provider name part is used.
        /// Null if it does not exist.
        /// </summary>
        /// <param name="schemeOrProviderName">
        /// The provider name (or ProviderName.SchemeSuffix) to find (lookup is case insensitive).
        /// </param>
        /// <returns>The provider or null.</returns>
        IGenericAuthenticationProvider FindProvider( string schemeOrProviderName );

        /// <summary>
        /// Reads a <see cref="IUserAuthInfo"/> for a user.
        /// Null for unexisting user or for the anonymous (<paramref name="userId"/> = 0).
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user information or null if the user identifier does not exist.</returns>
        IUserAuthInfo ReadUserAuthInfo( ISqlCallContext ctx, int actorId, int userId );

        /// <summary>
        /// Reads a <see cref="IUserAuthInfo"/> for a user.
        /// Null for unexisting user or for the anonymous (<paramref name="userId"/> = 0).
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user information or null if the user identifier does not exist.</returns>
        Task<IUserAuthInfo> ReadUserAuthInfoAsync( ISqlCallContext ctx, int actorId, int userId );
    }
}
