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
    public class AuthScopeSet 
    {
        class Wrapper : IReadOnlyCollection<AuthScope>
        {
            public readonly Dictionary<string,AuthScope> Scopes;

            public Wrapper( IEnumerable<AuthScope> scopes )
            {
                Scopes = new Dictionary<string, AuthScope>();
                if( scopes != null )
                {
                    foreach( var s in scopes ) Scopes[s.ScopeName] = s;
                }
            }


            public int Count => Scopes.Count;

            public IEnumerator<AuthScope> GetEnumerator() => Scopes.Values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public override string ToString() => Scopes.Values.OrderBy( s => s.ScopeName ).Select( s => s.ToString() ).Concatenate( " " );

        }

        readonly Wrapper _wrapper;

        /// <summary>
        /// Initializes a new empty <see cref="AuthScopeSet"/>.
        /// </summary>
        public AuthScopeSet()
        {
            _wrapper = new Wrapper( null );
        }

        /// <summary>
        /// Initializes a new <see cref="AuthScopeSet"/>.
        /// </summary>
        /// <param name="scopes">Initial scopes.</param>
        public AuthScopeSet( IEnumerable<AuthScope> scopes )
            : this()
        {
            if( scopes == null ) throw new ArgumentNullException( nameof( scopes ) );
            _wrapper = new Wrapper( scopes );
        }

        /// <summary>
        /// Gets the scopes.
        /// </summary>
        public IReadOnlyCollection<AuthScope> Scopes => _wrapper;

        /// <summary>
        /// Clones a new <see cref="AuthScopeSet"/> with the same scopes as this one.
        /// </summary>
        /// <returns>A clone of this set.</returns>
        public AuthScopeSet Clone() => new AuthScopeSet( Scopes );

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
        public int Count => _wrapper.Count;

        /// <summary>
        /// Adds a <see cref="AuthScope"/>. Existing scope with the same <see cref="AuthScope.ScopeName"/> is replaced.
        /// </summary>
        /// <param name="scope">The scope to add.</param>
        public void Add( AuthScope scope )
        {
            if( scope == null ) throw new ArgumentNullException( nameof(scope) );
            _wrapper.Scopes[ scope.ScopeName ] = scope;
        }

        /// <summary>
        /// Clears this set.
        /// </summary>
        public void Clear() => _wrapper.Scopes.Clear();

        /// <summary>
        /// Gets whether this set contains a scope with the given name.
        /// Use the indexer <see cref="this[string]"/> to retrieve it.
        /// </summary>
        /// <param name="name">Scope name.</param>
        /// <returns>True if found, false otherwise.</returns>
        public bool Contains( string name ) => _wrapper.Scopes.ContainsKey( name );

        /// <summary>
        /// Gets the named <see cref="AuthScope"/> if it exists.
        /// </summary>
        /// <param name="name">Scope name.</param>
        /// <returns>The scope or null.</returns>
        public AuthScope this[ string name ] => _wrapper.Scopes.GetValueWithDefault( name, null );

        /// <summary>
        /// Removes the scope with the same <see cref="AuthScope.ScopeName"/> and <see cref="AuthScope.Status"/>.
        /// </summary>
        /// <param name="scope">The scope to remove.</param>
        /// <returns>True on success, false if not found.</returns>
        public bool Remove( AuthScope scope )
        {
            if( scope == null ) throw new ArgumentNullException( nameof( scope ) );
            AuthScope s;
            return _wrapper.Scopes.TryGetValue( scope.ScopeName, out s ) && s.Status == scope.Status && _wrapper.Scopes.Remove( scope.ScopeName );
        }

        /// <summary>
        /// Removes the scope with the given name.
        /// </summary>
        /// <param name="name">The scope name to remove.</param>
        /// <returns>True on success, false if not found.</returns>
        public bool Remove( string name ) => _wrapper.Scopes.Remove( name );

        /// <summary>
        /// Overridden to return a whitespace separated string of scopes prefixed with their status.
        /// Scopes are ordered by their <see cref="AuthScope.ScopeName"/>.
        /// </summary>
        /// <returns>The scopes with their status.</returns>
        public override string ToString() => _wrapper.ToString();

    }
}
