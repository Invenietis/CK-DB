using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CK.Text;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.DB.Auth
{
    /// <summary>
    /// Helper class to manipulate <see cref="AuthScope"/>. 
    /// This set guaranties unicity of <see cref="AuthScope.ScopeName"/>.
    /// </summary>
    public class AuthScopeSet : IReadOnlyCollection<AuthScope>
    {
        readonly Dictionary<string,AuthScope> _scopes;

        /// <summary>
        /// Initializes a new empty <see cref="AuthScopeSet"/>.
        /// </summary>
        public AuthScopeSet()
        {
            _scopes = new Dictionary<string, AuthScope>();
        }

        /// <summary>
        /// Initializes a new <see cref="AuthScopeSet"/>.
        /// </summary>
        /// <param name="scopes">Initial scopes.</param>
        public AuthScopeSet( IEnumerable<AuthScope> scopes )
            : this()
        {
            if( scopes == null ) throw new ArgumentNullException( nameof( scopes ) );
            foreach( var s in scopes ) _scopes[ s.ScopeName ] = s;
        }

        /// <summary>
        /// Checks that all scope names exists and have the given status.
        /// </summary>
        /// <param name="status">Status to check.</param>
        /// <param name="scopes">Names to check.</param>
        /// <returns>True if all the scopes have the given status, false otherwise.</returns>
        public bool CheckStatus( ScopeWARStatus status, params string[] scopes ) => CheckStatus( status, (IEnumerable<string>)scopes );

        /// <summary>
        /// Checks that all scope names exists and have the given status.
        /// </summary>
        /// <param name="status">Status to check.</param>
        /// <param name="scopes">Names to check.</param>
        /// <returns>True if all the scopes have the given status, false otherwise.</returns>
        public bool CheckStatus( ScopeWARStatus status, IEnumerable<string> scopes )
        {
            return scopes.All( name => this[name]?.Status == status );
        }

        /// <summary>
        /// Gets the count of <see cref="AuthScope"/> this set contains.
        /// </summary>
        public int Count => _scopes.Count;

        /// <summary>
        /// Adds a <see cref="AuthScope"/>. Existing scope with the same <see cref="AuthScope.ScopeName"/> is replaced.
        /// </summary>
        /// <param name="scope">The scope to add.</param>
        public void Add( AuthScope scope )
        {
            if( scope == null ) throw new ArgumentNullException( nameof(scope) );
            _scopes[ scope.ScopeName ] = scope;
        }

        /// <summary>
        /// Clears this set.
        /// </summary>
        public void Clear() => _scopes.Clear();

        /// <summary>
        /// Gets whether this set contains a scope with the given name.
        /// </summary>
        /// <param name="name">Scope name.</param>
        /// <returns>True if found, false otherwise.</returns>
        public bool Contains( string name ) => _scopes.ContainsKey( name );

        /// <summary>
        /// Gets the named <see cref="AuthScope"/> if it exists.
        /// </summary>
        /// <param name="name">Scope name.</param>
        /// <returns>The scope or null.</returns>
        public AuthScope this[ string name ] => _scopes.GetValueWithDefault( name, null );

        /// <summary>
        /// Creates an enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<AuthScope> GetEnumerator() => _scopes.Values.GetEnumerator();

        /// <summary>
        /// Removes the scope with the same <see cref="AuthScope.ScopeName"/> and <see cref="AuthScope.Status"/>.
        /// </summary>
        /// <param name="scope">The scope to remove.</param>
        /// <returns>True on success, false if not found.</returns>
        public bool Remove( AuthScope scope )
        {
            if( scope == null ) throw new ArgumentNullException( nameof( scope ) );
            AuthScope s;
            return _scopes.TryGetValue( scope.ScopeName, out s ) && s.Status == scope.Status && _scopes.Remove( scope.ScopeName );
        }

        /// <summary>
        /// Removes the scope with the given name.
        /// </summary>
        /// <param name="name">The scope name to remove.</param>
        /// <returns>True on success, false if not found.</returns>
        public bool Remove( string name ) => _scopes.Remove( name );

        /// <summary>
        /// Overridden to return a whitespace separated string of scopes prefixed with their status.
        /// Scopes are ordered by their <see cref="AuthScope.ScopeName"/>.
        /// </summary>
        /// <returns>The scopes with their status.</returns>
        public override string ToString() => _scopes.Values.OrderBy( s => s.ScopeName ).Select( s => s.ToString() ).Concatenate( " " );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
