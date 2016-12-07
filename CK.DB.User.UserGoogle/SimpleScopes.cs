using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB
{
    /// <summary>
    /// Simple mutable encapsulation of scopes string with helpers to manipulate them.
    /// </summary>
    public struct SimpleScopes
    {
        /// <summary>
        /// Initializes a new <see cref="SimpleScopes"/> with an initial <see cref="Scopes"/>.
        /// </summary>
        /// <param name="s">The initial scopes.</param>
        public SimpleScopes( string s )
        {
            Scopes = s;
        }

        /// <summary>
        /// Gets or sets the raw scopes: a space-separated list of scopes as specified 
        /// in http://tools.ietf.org/html/rfc6749#section-3.3.
        /// See https://developers.google.com/identity/protocols/googlescopes for available google scopes.
        /// To be valid, this must must not be null, empty or whitespaces.
        /// </summary>
        public string Scopes { get; set; }

        /// <summary>
        /// Gets whether <see cref="Scopes"/> is valid: it must not be null, empty or whitespaces.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace( Scopes );

        /// <summary>
        /// Tests whether the given scopes (that can be multiple scopes separated by white spaces)
        /// are already in <see cref="Scopes"/>.
        /// </summary>
        /// <param name="scope">The scope or scopes to challenge.</param>
        /// <returns>True if all <paramref name="scope"/> are in <see cref="Scopes"/>.</returns>
        public bool HasAllScopes( params string[] scope )
        {
            if( string.IsNullOrWhiteSpace( Scopes ) ) return false;
            var t = new HashSet<string>( Scopes.Split( ' ' ) );
            var h = new HashSet<string>( scope.SelectMany( sc => sc.Split( ' ' ) ) );
            return t.IsSupersetOf( h );
        }

        /// <summary>
        /// Combines the given scopes (that can be multiple scopes separated by white spaces)
        /// with any existing <see cref="Scopes"/>.
        /// </summary>
        /// <param name="scope">The scope or scopes to add.</param>
        public void AddScopes( params string[] scope )
        {
            var h = new HashSet<string>( scope.SelectMany( sc => sc.Split( ' ' ) ).Where( s => !string.IsNullOrWhiteSpace( s ) ) );
            if( !string.IsNullOrWhiteSpace( Scopes ) )
            {
                var t = new HashSet<string>( Scopes.Split( ' ' ).Where( s => !string.IsNullOrWhiteSpace( s ) ) );
                h.AddRange( t );
            }
            Scopes = string.Join( " ", h );
        }


        /// <summary>
        /// Overridden to return <see cref="Scopes"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Scopes;

        /// <summary>
        /// Relay to <see cref="Scopes"/>.
        /// </summary>
        /// <returns>The Scopes's hash code.</returns>
        public override int GetHashCode() => Scopes != null ? Scopes.GetHashCode() : 0;

        /// <summary>
        /// Overrides equality operator: the <see cref="Scopes"/> string must be exactly the same.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True when equal, false otherwise.</returns>
        public override bool Equals( object obj ) => obj is SimpleScopes && ((SimpleScopes)obj).Scopes == Scopes;

        public static bool operator == ( SimpleScopes s1, SimpleScopes s2 ) => s1.Scopes == s2.Scopes; 
        public static bool operator != ( SimpleScopes s1, SimpleScopes s2 ) => s1.Scopes != s2.Scopes; 

        /// <summary>
        /// Allows conversion from string.
        /// </summary>
        /// <param name="s">The scopes string.</param>
        public static implicit operator SimpleScopes( string s ) => new SimpleScopes( s );

        /// <summary>
        /// Allows conversion from <see cref="SimpleScopes"/> to string.
        /// </summary>
        /// <param name="scopes">The scopes.</param>
        static public implicit operator string( SimpleScopes scopes ) => scopes.Scopes;

    }
}
