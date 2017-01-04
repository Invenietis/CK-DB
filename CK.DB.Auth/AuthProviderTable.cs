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
    /// Defines authentication providers that are registered and offers a data-driven way 
    /// to disable them.
    /// </summary>
    [SqlTable( "tAuthProvider", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract partial class AuthProviderTable : SqlTable
    {
        /// <summary>
        /// Enables or disables a provider.
        /// Disabled provider must be handled by the application: since implementation can heavily differ
        /// between them, that some of their capabilities may continue to be operational, and because of
        /// race conditions from the user perspective, provider implementations MUST ignore this flag.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="providerName">The provider name (case insensitive).</param>
        /// <param name="isEnabled">True to enable, false to disable.</param>
        [SqlProcedure( "sAuthProviderIsEnableSet" )]
        public abstract Task EnableProviderAsync( ISqlCallContext ctx, int actorId, string providerName, bool isEnabled = true );

        /// <summary>
        /// Registers a new provider with a (must be) unique name, enabled by default.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="providerName">The provider name to set. Must be unique.</param>
        /// <param name="userProviderSchemaTableName">The schema.[table name] that holds at least UserId and LastLoginTime columns.</param>
        /// <returns>The authentication provider identifier.</returns>
        [SqlProcedure( "sAuthProviderRegister" )]
        public abstract Task<int> RegisterProviderAsync( ISqlCallContext ctx, int actorId, string providerName, string userProviderSchemaTableName );
    }
}
