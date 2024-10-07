using System;
using System.Linq;
using System.Reflection;
using CK.Core;

namespace CK.DB.Auth;

/// <summary>
/// Decorates <see cref="IAuthenticationDatabaseService"/>.
/// </summary>
static public class AuthenticationDatabaseServiceExtensions
{
    /// <summary>
    /// Finds a <see cref="IGenericAuthenticationProvider"/> by its name (using <see cref="StringComparer.OrdinalIgnoreCase"/>).
    /// This methods accepts a scheme: it is the provider name followed by a, optional dotted suffix and in such case only the
    /// provider name part is used.
    /// An <see cref="ArgumentException"/> is thrown if the provider cannot be found.
    /// </summary>
    /// <param name="this">This database service implementation.</param>
    /// <param name="scheme">The scheme to find.</param>
    /// <param name="mustHavePayload">
    /// By default, the provider must be able to create an instance of its payload (<see cref="IGenericAuthenticationProvider.CanCreatePayload"/> must be true).
    /// </param>
    /// <returns>The provider.</returns>
    public static IGenericAuthenticationProvider FindRequiredProvider( this IAuthenticationDatabaseService @this, string scheme, bool mustHavePayload = true )
    {
        var dbProvider = @this.FindProvider( scheme );
        if( dbProvider == null ) throw new ArgumentException( $"Unable to find a database provider for scheme '{scheme}'. Available: {@this.AllProviders.Select( p => p.ProviderName ).Concatenate()}.", nameof( scheme ) );
        if( mustHavePayload && !dbProvider.CanCreatePayload )
        {
            throw new ArgumentException( $"Database provider '{dbProvider.GetType().FullName}' does not handle generic payload." );
        }
        return dbProvider;
    }

}
