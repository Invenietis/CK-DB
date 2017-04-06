using CK.Core;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Auth
{
    /// <summary>
    /// This package defines common abstractions and data to authentication providers.
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "vUserAuthProvider" )]
    public abstract partial class Package : SqlPackage, IAuthenticationDatabaseService
    {
        IDictionary<string,IGenericAuthenticationProvider> _allProviders;
        IReadOnlyCollection<IGenericAuthenticationProvider> _allProvidersValues;

        void StObjConstruct( Actor.Package actor )
        {
        }

        void StObjInitialize(IActivityMonitor m, IContextualStObjMap map)
        {
            _allProviders = map.Implementations.OfType<IGenericAuthenticationProvider>().ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
            if (BasicProvider != null) _allProviders.Add(BasicToGenericProviderAdapter.Name, new BasicToGenericProviderAdapter(BasicProvider));
            _allProvidersValues = new CKReadOnlyCollectionOnICollection<IGenericAuthenticationProvider>(_allProviders.Values);
        }

        /// <summary>
        /// Gets the only <see cref="IBasicAuthenticationProvider"/> if it exists or null.
        /// </summary>
        [InjectContract(IsOptional = true)]
        public IBasicAuthenticationProvider BasicProvider { get; protected set; }

        /// <summary>
        /// Gets the collection of existing providers, including an adapter of <see cref="IBasicAuthenticationProvider"/> 
        /// if it exists.
        /// </summary>
        public IReadOnlyCollection<IGenericAuthenticationProvider> AllProviders => _allProvidersValues;

        /// <summary>
        /// Finds a <see cref="IGenericAuthenticationProvider"/> by its name (using <see cref="StringComparer.OrdinalIgnoreCase"/>).
        /// Null if it does not exist.
        /// </summary>
        /// <param name="providerName">The provider name to find (lookup is case insensitive).</param>
        /// <returns>The provider or null.</returns>
        public IGenericAuthenticationProvider FindProvider(string providerName) => _allProviders.GetValueWithDefault(providerName, null);

        /// <summary>
        /// Obtains the command object to read auth info.
        /// This is protected since there is no need to call it externally.
        /// </summary>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The configured command.</returns>
        [SqlProcedureNoExecute("sAuthUserInfoRead")]
        protected abstract SqlCommand CmdReadUserAuthInfo(int actorId, int userId);


        /// <summary>
        /// Calls the OnUserLogin hook.
        /// This is not intended to be called by code: this is public to allow edge case scenarii.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="providerName">The provider name.</param>
        /// <param name="loginTime">Login time.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sAuthUserOnLogin" )]
        public abstract Task OnUserLoginAsync( ISqlCallContext ctx, string providerName, DateTime loginTime, int userId );

        /// <summary>
        /// Reads a <see cref="IUserAuthInfo"/> for a user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user information or null if the user identifier does not exist.</returns>
        public async Task<IUserAuthInfo> ReadUserAuthInfoAsync(ISqlCallContext ctx, int actorId, int userId)
        {
            using (var cmd = CmdReadUserAuthInfo(actorId, userId))
                try
                {
                    using (await (cmd.Connection = ctx[this]).EnsureOpenAsync().ConfigureAwait(false))
                    using (var r = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                     {
                        if (!await r.ReadAsync().ConfigureAwait(false)) return null;
                        var result = new AuthInfo();
                        result.UserId = r.GetInt32(0);
                        result.UserName = r.GetString(1);
                        if (await r.NextResultAsync().ConfigureAwait(false) 
                            && await r.ReadAsync().ConfigureAwait(false))
                        {
                            var providers = new List<UserAuthProviderInfo>();
                            do
                            {
                                providers.Add(new UserAuthProviderInfo(r.GetString(0), r.GetDateTime(1)));
                            }
                            while (await r.ReadAsync().ConfigureAwait(false));
                            result.Providers = providers;
                        }
                        else result.Providers = Util.Array.Empty<UserAuthProviderInfo>();
                        return result;
                    }
                }
                catch (SqlException ex)
                {
                    throw SqlDetailedException.Create(cmd, ex);
                }
        }

    }
}
