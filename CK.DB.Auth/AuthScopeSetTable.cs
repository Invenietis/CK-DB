using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using CK.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Auth
{
    /// <summary>
    /// Defines Scope Set entities.
    /// </summary>
    [SqlTable( "tAuthScopeSet", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "vAuthScopeSet, vAuthScopeSetContent" )]
    public abstract partial class AuthScopeSetTable : SqlTable
    {
        void Construct( AuthScopeTable scopes )
        {
        }

        /// <summary>
        /// Reads a <see cref="AuthScopeSet"/> content.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="scopeSetId">The scope set identifier to read.</param>
        /// <returns>The set of scopes.</returns>
        public async Task<AuthScopeSet> ReadAuthScopeSetAsync( ISqlCallContext ctx, int scopeSetId )
        {
            using( var c = CreateReadCommand( scopeSetId ) )
            using( await (c.Connection = ctx[Database.ConnectionString]).EnsureOpenAsync().ConfigureAwait( false ) )
            using( var r = await c.ExecuteReaderAsync().ConfigureAwait( false ) )
            {
                var result = new AuthScopeSet();
                while( await r.ReadAsync().ConfigureAwait( false ) ) result.Add( CreateAuthScope( r ) );
                return result;
            }
        }

        /// <summary>
        /// Factory method for <see cref="AuthScope"/> (or specialized one).
        /// </summary>
        /// <param name="r">The data reader.</param>
        /// <returns>A new scope.</returns>
        protected virtual AuthScope CreateAuthScope( IDataReader r )
        {
            string n = r.GetString( 0 );
            ScopeWARStatus s;
            switch( r.GetString( 1 ) )
            {
                case "A": s = ScopeWARStatus.Accepted; break;
                case "R": s = ScopeWARStatus.Rejected; break;
                default: s = ScopeWARStatus.Waiting; break;
            }
            return new AuthScope( n, s, r.GetDateTime( 2 ) );
        }

        /// <summary>
        /// Creates the command to read the name, status and last write time of scopes from a scope set.
        /// This can be overridden to read more specialized data.
        /// </summary>
        /// <param name="authScopeId">The scope set identifier.</param>
        /// <returns>The command.</returns>
        protected virtual SqlCommand CreateReadCommand( int authScopeId )
        {
            return new SqlCommand( $"select ScopeName, WARStatus, WARStatusLastWrite from CK.vAuthScopeSetContent where ScopeSetId = {authScopeId}" );
        }

        /// <summary>
        /// Creates a scope set with an optional initial set of scopes.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopes">Optional initial scopes.</param>
        /// <param name="scopesHaveStatus">True to handle [W], [A] or [R] prefix from scopes.</param>
        /// <param name="defaultStatus">Initial status of of initial scopes.</param>
        /// <returns>The scope set identifier.</returns>
        public virtual Task<int> CreateScopeSetAsync( ISqlCallContext ctx, int actorId, string scopes = null, bool scopesHaveStatus = false, ScopeWARStatus defaultStatus = ScopeWARStatus.Waiting )
        {
            return DoCreateScopeSetAsync( ctx, actorId, scopes, scopesHaveStatus, defaultStatus.ToString()[0] );
        }

        /// <summary>
        /// Creates a scope set with an initial set of scopes.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopes">Initial scopes with their status.</param>
        /// <returns>The scope set identifier.</returns>
        public virtual Task<int> CreateScopeSetAsync( ISqlCallContext ctx, int actorId, IEnumerable<AuthScope> scopes )
        {
            return DoCreateScopeSetAsync( ctx, actorId, ToString( scopes ), true, 'W' );
        }

        static string ToString( IEnumerable<AuthScope> scopes )
        {
            return scopes.OrderBy( s => s.ScopeName ).Select( s => s.ToString() ).Concatenate( " " );
        }

        /// <summary>
        /// Creates a scope set with an optional initial set of scopes.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="initScopes">Optional initial scopes.</param>
        /// <param name="initScopesHaveStatus">True to handle [W], [A] or [R] prefixes from <paramref name="initScopes"/>.</param>
        /// <param name="initDefaultWARStatus">Default status of initial scopes.</param>
        /// <returns>The scope set identifier.</returns>
        [SqlProcedure( "sAuthScopeSetCreate" )]
        protected abstract Task<int> DoCreateScopeSetAsync( 
            ISqlCallContext ctx, 
            int actorId, 
            string initScopes, 
            bool initScopesHaveStatus, 
            char initDefaultWARStatus );

        /// <summary>
        /// Destroys a set of scopes.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The scope set identifier to destroy.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sAuthScopeSetDestroy" )]
        public abstract Task DestroyScopeSetAsync( ISqlCallContext ctx, int actorId, int scopeSetId );

        /// <summary>
        /// Adds new scopes or sets <see cref="ScopeWARStatus"/> of existing ones.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="scopes">Scopes to add with their status.</param>
        /// <returns>The awaitable.</returns>
        public virtual Task AddScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, IEnumerable<AuthScope> scopes )
        {
            return DoAddScopesAsync( ctx, actorId, scopeSetId, ToString( scopes ), true, 'W', false );
        }

        /// <summary>
        /// Adds new scopes or sets <see cref="ScopeWARStatus"/> of existing ones.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="scopes">Whitespace separated list of scopes.</param>
        /// <param name="scopesHaveStatus">True to handle [W], [A] or [R] prefixes from <paramref name="scopes"/>.</param>
        /// <param name="defaultWARstatus">The status ('W', 'A' or 'R') to use when no explicit prefix are handled.</param>
        /// <returns>The awaitable.</returns>
        public virtual Task AddScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, string scopes, bool scopesHaveStatus, ScopeWARStatus defaultWARstatus )
        {
            return DoAddScopesAsync( ctx, actorId, scopeSetId, scopes, scopesHaveStatus, defaultWARstatus.ToString()[0], false );
        }

        /// <summary>
        /// Sets the scopes of a scope set: existing scopes that do not appear in <paramref name="scopes"/>
        /// are removed.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="scopes">Whitespace separated list of scopes.</param>
        /// <param name="scopesHaveStatus">True to handle [W], [A] or [R] prefixes from <paramref name="scopes"/>.</param>
        /// <param name="defaultWARStatus">The status ('W', 'A' or 'R') to use when no explicit prefix are handled.</param>
        /// <returns>The awaitable.</returns>
        public virtual Task SetScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, string scopes, bool scopesHaveStatus, ScopeWARStatus defaultWARStatus = ScopeWARStatus.Waiting )
        {
            return DoAddScopesAsync( ctx, actorId, scopeSetId, scopes, scopesHaveStatus, defaultWARStatus.ToString()[0], true );
        }

        /// <summary>
        /// Sets the scopes of a scope set: existing scopes that do not appear in <paramref name="scopes"/>
        /// are removed.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="scopes">Scopes to set.</param>
        /// <returns>The awaitable.</returns>
        public virtual Task SetScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, AuthScopeSet scopes )
        {
            return DoAddScopesAsync( ctx, actorId, scopeSetId, scopes.ToString(), true, 'W', true );
        }

        /// <summary>
        /// Adds new scopes or sets <see cref="ScopeWARStatus"/> of existing ones, optionally 
        /// removing any other scopes.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="scopes">Whitespace separated list of scopes.</param>
        /// <param name="scopesHaveStatus">True to handle [W], [A] or [R] prefixes from <paramref name="scopes"/>.</param>
        /// <param name="defaultWARStatus">The status ('W', 'A' or 'R') to use when no explicit prefix are handled.</param>
        /// <param name="resetScopes">True to remove exisiting scopes that do not appear in <paramref name="scopes"/>.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sAuthScopeSetAddScopes" )]
        protected abstract Task DoAddScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, string scopes, bool scopesHaveStatus, char defaultWARStatus, bool resetScopes );

        /// <summary>
        /// Removes the given scopes from a scope set.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="scopes">Whitespace separated list of scopes to remove.</param>
        /// <param name="scopesHaveStatus">True to handle [W], [A] or [R] prefixes from <paramref name="scopes"/>.</param>
        /// <param name="defaultWARStatus">The status ('W', 'A' or 'R') to use when no explicit prefix are handled.</param>
        /// <param name="statusFilter">Optional filter status: only scopes with this status will be removed.</param>
        /// <returns>The awaitable.</returns>
        public Task RemoveScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, string scopes, bool scopesHaveStatus, ScopeWARStatus defaultWARStatus = ScopeWARStatus.Waiting, ScopeWARStatus? statusFilter = null )
        {
            return DoRemoveScopesAsync( ctx, actorId, scopeSetId, scopes, scopesHaveStatus, defaultWARStatus.ToString()[0], statusFilter?.ToString()[0] );
        }

        /// <summary>
        /// Removes the given scopes from a scope set.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="scopes">Scopes to remove.</param>
        /// <returns>The awaitable.</returns>
        public Task RemoveScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, IEnumerable<AuthScope> scopes )
        {
            return DoRemoveScopesAsync( ctx, actorId, scopeSetId, ToString( scopes ), true, 'W', null );
        }

        /// <summary>
        /// Removes all scopes from a scope set that have a specific status.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="status">The status that must match for the scope to be removed.</param>
        /// <returns>The awaitable.</returns>
        public Task RemoveScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, ScopeWARStatus status )
        {
            return DoRemoveScopesAsync( ctx, actorId, scopeSetId, null, false, 'W', status.ToString()[0] );
        }

        /// <summary>
        /// Removes scopes (low level).
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="scopes">Whitespace separated list of scopes to remove.</param>
        /// <param name="scopesHaveStatus">True to handle [W], [A] or [R] prefixes from <paramref name="scopes"/>.</param>
        /// <param name="defaultWARStatus">The status ('W', 'A' or 'R') to use when no explicit prefix are handled.</param>
        /// <param name="warStatusFilter">Optional filter status: only scopes with this status will be removed.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sAuthScopeSetRemoveScopes" )]
        protected abstract Task DoRemoveScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, string scopes, bool scopesHaveStatus, char defaultWARStatus, char? warStatusFilter );
    }
}
