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
    public abstract partial class AuthProviderTable : SqlTable
    {
        /// <summary>
        /// Enables or disables a provider. 
        /// Disabled provider must be handled by the application: since implementation can heavily differ
        /// between them, that some of their capabilities may continue to be operational, and because of
        /// race conditions from the user perspective, provider implementations MUST ignore this flag: 
        /// authentication must always be honored, this MUST be only used by GUI to avoid the actual use of a provider.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="providerName">The provider name (case insensitive).</param>
        /// <param name="isEnabled">True to enable, false to disable.</param>
        [SqlProcedure( "sAuthProviderIsEnableSet" )]
        public abstract void EnableProvider( ISqlCallContext ctx, int actorId, string providerName, bool isEnabled = true );

        /// <summary>
        /// Registers a new provider with a (must be) unique name, enabled by default.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="providerName">The provider name to set. Must be unique.</param>
        /// <param name="userProviderSchemaTableName">The "schema.[table name]" that holds at least UserId and LastLoginTime columns.</param>
        /// <returns>The authentication provider identifier.</returns>
        [SqlProcedure( "sAuthProviderRegister" )]
        public abstract int RegisterProvider( ISqlCallContext ctx, int actorId, string providerName, string userProviderSchemaTableName );
    }
}
