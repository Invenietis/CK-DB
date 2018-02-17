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
using System.Threading;
using System.Threading.Tasks;

namespace CK.DB.Auth.AuthScope
{
    /// <summary>
    /// Defines Scope Set entities.
    /// </summary>
    [SqlTable( "tAuthScopeSet", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "vAuthScopeSet, vAuthScopeSetContent, sAuthScopeSetCopy" )]
    public abstract partial class AuthScopeSetTable : SqlTable
    {
        void StObjConstruct( AuthScopeTable scopes )
        {
        }

        /// <summary>
        /// Reads a <see cref="AuthScopeSet"/> content.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="scopeSetId">The scope set identifier to read.</param>
        /// <returns>The set of scopes.</returns>
        public Task<AuthScopeSet> ReadAuthScopeSetAsync( ISqlCallContext ctx, int scopeSetId )
        {
            return RawReadAuthScopeSetAsync( ctx, CreateReadCommand( $"select {scopeSetId}" ) );
        }

        /// <summary>
        /// Reads a <see cref="AuthScopeSet"/> content from a configured 
        /// command (created by <see cref="CreateReadCommand"/>).
        /// Null is returned for empty returned set.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="cmd">The reader command.</param>
        /// <returns>The set of scopes or null.</returns>
        public Task<AuthScopeSet> RawReadAuthScopeSetAsync(ISqlCallContext ctx, SqlCommand cmd)
        {
            async Task<AuthScopeSet> ReadAsync( SqlCommand c, CancellationToken t )
            {
                using( var r = await cmd.ExecuteReaderAsync().ConfigureAwait( false ) )
                {
                    if( !await r.ReadAsync().ConfigureAwait( false ) ) return null;
                    var result = new AuthScopeSet() { ScopeSetId = r.GetInt32( 0 ) };
                    if( await r.NextResultAsync().ConfigureAwait( false ) )
                    {
                        while( await r.ReadAsync().ConfigureAwait( false ) ) result.Add( CreateAuthScope( r ) );
                    }
                    return result;
                }
            }
            return ctx[Database].ExecuteQueryAsync( cmd, ReadAsync );
        }

        /// <summary>
        /// Creates a scope set with an initial set of scopes.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopes">Initial scopes with their status.</param>
        /// <returns>The scope set identifier.</returns>
        public virtual Task<int> CreateScopeSetAsync( ISqlCallContext ctx, int actorId, IEnumerable<AuthScopeItem> scopes )
        {
            return DoCreateScopeSetAsync( ctx, actorId, ToString( scopes ), true, 'W' );
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
        /// Destroys a scope set.
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
        /// <param name="set">The scope set to add or update.</param>
        /// <returns>The awaitable.</returns>
        public virtual Task AddOrUpdateScopesAsync( ISqlCallContext ctx, int actorId, AuthScopeSet set )
        {
            return DoAddOrUpdateScopesAsync( ctx, actorId, set.ScopeSetId, set.ToString(), true, 'W', false );
        }

        /// <summary>
        /// Adds new scopes or sets <see cref="ScopeWARStatus"/> of existing ones.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="scopes">Whitespace separated list of scopes to add or update.</param>
        /// <param name="scopesHaveStatus">True to handle [W], [A] or [R] prefixes from <paramref name="scopes"/>.</param>
        /// <param name="defaultWARstatus">The status ('W', 'A' or 'R') to use when no explicit prefix are handled.</param>
        /// <returns>The awaitable.</returns>
        public virtual Task AddOrUpdateScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, string scopes, bool scopesHaveStatus, ScopeWARStatus defaultWARstatus )
        {
            return DoAddOrUpdateScopesAsync( ctx, actorId, scopeSetId, scopes, scopesHaveStatus, defaultWARstatus.ToString()[0], false );
        }

        /// <summary>
        /// Sets the scopes of a scope set: existing scopes that do not appear in <paramref name="set"/>
        /// are removed.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="set">The new scope set value.</param>
        /// <returns>The awaitable.</returns>
        public virtual Task SetScopesAsync( ISqlCallContext ctx, int actorId, AuthScopeSet set )
        {
            return DoAddOrUpdateScopesAsync( ctx, actorId, set.ScopeSetId, set.ToString(), true, 'W', true );
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
            return DoAddOrUpdateScopesAsync( ctx, actorId, scopeSetId, scopes, scopesHaveStatus, defaultWARStatus.ToString()[0], true );
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
        /// <param name="resetScopes">True to remove existing scopes that do not appear in <paramref name="scopes"/>.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sAuthScopeSetAddScopes" )]
        protected abstract Task DoAddOrUpdateScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, string scopes, bool scopesHaveStatus, char defaultWARStatus, bool resetScopes );

        /// <summary>
        /// Removes the given scopes from a scope set.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="scopeSetId">The target scope set identifier.</param>
        /// <param name="scopes">Scopes to remove.</param>
        /// <returns>The awaitable.</returns>
        public Task RemoveScopesAsync( ISqlCallContext ctx, int actorId, int scopeSetId, IEnumerable<AuthScopeItem> scopes )
        {
            return DoRemoveScopesAsync( ctx, actorId, scopeSetId, ToString( scopes ), true, 'W', null );
        }

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
