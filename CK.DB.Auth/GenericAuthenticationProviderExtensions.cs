using System;
using System.Linq;
using System.Reflection;

namespace CK.DB.Auth
{
    /// <summary>
    /// Decorates <see cref="IGenericAuthenticationProvider"/>.
    /// </summary>
    static public class GenericAuthenticationProviderExtensions
    {
        /// <summary>
        /// Gets whether this provider has a standard payload: <see cref="CreatePayload"/> can be called.
        /// </summary>
        /// <param name="this">This provider.</param>
        /// <returns>True if this provider has a payload.</returns>
        public static bool HasPayload( this IGenericAuthenticationProvider @this )
        {
            var t = @this.GetType().GetTypeInfo();
            var eI = t.ImplementedInterfaces
                        .FirstOrDefault( iT => iT.GetTypeInfo().IsGenericType && iT.GetGenericTypeDefinition() == typeof( IGenericAuthenticationProvider<> ) );
            return eI != null;
        }

        /// <summary>
        /// Creates an empty payload object. <see cref="HasPayload"/> must be true otherwise 
        /// an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="this">This provider.</param>
        /// <returns>An empty payload object.</returns>
        public static object CreatePayload( this IGenericAuthenticationProvider @this )
        {
            var t = @this.GetType().GetTypeInfo();
            var eI = t.ImplementedInterfaces
                        .FirstOrDefault( iT => iT.GetTypeInfo().IsGenericType && iT.GetGenericTypeDefinition() == typeof( IGenericAuthenticationProvider<> ) );
            if( eI == null )
            {
                throw new InvalidOperationException();
            }
            return eI.GetMethod( nameof( IGenericAuthenticationProvider<object>.CreatePayload ) )
                        .Invoke( @this, Array.Empty<object>() );
        }
    }
}
