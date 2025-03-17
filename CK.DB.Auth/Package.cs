using CK.Core;
using CK.SqlServer;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CK.Auth;
using System.Diagnostics.CodeAnalysis;

namespace CK.DB.Auth;

/// <summary>
/// This package defines common abstractions and data to authentication providers.
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.0" )]
[SqlObjectItem( "vUserAuthProvider" )]
public abstract partial class Package : SqlPackage, IAuthenticationDatabaseService
{
    [AllowNull]
    Dictionary<string, IGenericAuthenticationProvider> _allProviders;
    [AllowNull]
    IReadOnlyCollection<IGenericAuthenticationProvider> _allProvidersValues;

    void StObjConstruct( Actor.Package actor )
    {
    }

    void StObjInitialize( IActivityMonitor m, IStObjObjectMap map )
    {
        using( m.OpenInfo( $"Initializing CK.DB.Auth.Package : IAuthenticationDatabaseService" ) )
        {
            _allProviders = map.FinalImplementations
                                .Select( f => f.Implementation )
                                .OfType<IGenericAuthenticationProvider>()
                                .ToDictionary( p => p.ProviderName, StringComparer.OrdinalIgnoreCase );
            if( BasicProvider != null ) _allProviders.Add( BasicToGenericProviderAdapter.Name, new BasicToGenericProviderAdapter( BasicProvider ) );
            _allProvidersValues = _allProviders.Values.AsIReadOnlyCollection();
            m.CloseGroup( $"{_allProviders.Count} providers: " + _allProviders.Keys.Concatenate() );
        }
    }

    /// <summary>
    /// Gets the only <see cref="IBasicAuthenticationProvider"/> if it exists or null.
    /// </summary>
    [InjectObject( IsOptional = true )]
    public IBasicAuthenticationProvider? BasicProvider { get; protected set; }

    /// <summary>
    /// Gets the collection of existing providers, including an adapter of <see cref="IBasicAuthenticationProvider"/> 
    /// if it exists.
    /// </summary>
    public IReadOnlyCollection<IGenericAuthenticationProvider> AllProviders => _allProvidersValues;

    /// <summary>
    /// Finds a <see cref="IGenericAuthenticationProvider"/> by its name (using <see cref="StringComparer.OrdinalIgnoreCase"/>).
    /// This methods accepts a scheme: it is the provider name followed by a, optional dotted suffix and
    /// in such case only the provider name part is used.
    /// Returns null if it does not exist.
    /// </summary>
    /// <param name="schemeOrProviderName">
    /// The provider name (or ProviderName.SchemeSuffix) to find (lookup is case insensitive).
    /// </param>
    /// <returns>The provider or null.</returns>
    public IGenericAuthenticationProvider? FindProvider( string schemeOrProviderName )
    {
        if( string.IsNullOrEmpty( schemeOrProviderName ) ) return null;
        int idx = schemeOrProviderName.IndexOf( '.' );
        if( idx >= 0 ) schemeOrProviderName = schemeOrProviderName.Substring( 0, idx );
        return _allProviders.GetValueOrDefault( schemeOrProviderName );
    }

    /// <summary>
    /// Obtains the command object to read auth info.
    /// This is protected since there is no need to call it externally.
    /// </summary>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The configured command.</returns>
    [SqlProcedureNoExecute( "sAuthUserInfoRead" )]
    protected abstract SqlCommand CmdReadUserAuthInfo( int actorId, int userId );


    /// <summary>
    /// Calls the OnUserLogin hook.
    /// This is not intended to be called by code: this is public to allow edge case scenarii.
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="scheme">The scheme used.</param>
    /// <param name="lastLoginTime">Last login time (<see cref="Util.UtcMinValue"/> for first login).</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="actualLogin">True for an actual login, false otherwise (only checks must be done).</param>
    /// <param name="loginTimeNow">Current login time.</param>
    /// <returns>The login result.</returns>
    [SqlProcedure( "sAuthUserOnLogin" )]
    public abstract Task<LoginResult> OnUserLoginAsync( ISqlCallContext ctx, string scheme, DateTime lastLoginTime, int userId, bool actualLogin, DateTime loginTimeNow );

    /// <summary>
    /// Reads a <see cref="IUserAuthInfo"/> for a user.
    /// Null for unexisting user or for the anonymous (<paramref name="userId"/> = 0).
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The user information or null if the user identifier does not exist.</returns>
    public async Task<IUserAuthInfo?> ReadUserAuthInfoAsync( ISqlCallContext ctx, int actorId, int userId )
    {
        static async Task<IUserAuthInfo?> ReadAsync( SqlCommand c, CancellationToken t )
        {
            using( var r = await c.ExecuteReaderAsync( t ).ConfigureAwait( false ) )
            {
                if( !await r.ReadAsync( t ).ConfigureAwait( false ) ) return null;
                var userId = r.GetInt32( 0 );
                var userName = r.GetString( 1 );
                List<UserSchemeInfo>? schemes = null;
                if( await r.NextResultAsync( t ).ConfigureAwait( false )
                    && await r.ReadAsync( t ).ConfigureAwait( false ) )
                {
                    schemes = new List<UserSchemeInfo>();
                    do
                    {
                        schemes.Add( new UserSchemeInfo( r.GetString( 0 ), r.GetDateTime( 1 ) ) );
                    }
                    while( await r.ReadAsync( t ).ConfigureAwait( false ) );
                }
                return new AuthInfo( userId, userName, (IReadOnlyList<UserSchemeInfo>?)schemes ?? Array.Empty<UserSchemeInfo>() );
            }
        }
        using( var cmd = CmdReadUserAuthInfo( actorId, userId ) )
        {
            return await ctx[Database].ExecuteQueryAsync( cmd, ReadAsync );
        }
    }
}
