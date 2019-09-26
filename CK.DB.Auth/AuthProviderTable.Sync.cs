using CK.Core;
using CK.SqlServer;

namespace CK.DB.Auth
{
    public abstract partial class AuthProviderTable : SqlTable
    {
        /// <summary>
        /// Registers a new provider with a (must be) unique name, enabled by default.
        /// This should be called directly by install or setup package scripts.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="providerName">The provider name to set. Must be unique.</param>
        /// <param name="userProviderSchemaTableName">The "schema.[table name]" that holds at least UserId and LastLoginTime columns.</param>
        /// <param name="isMultiScheme">True for multi scheme provider.</param>
        /// <returns>The authentication provider identifier.</returns>
        [SqlProcedure( "sAuthProviderRegister" )]
        public abstract int RegisterProvider( ISqlCallContext ctx, int actorId, string providerName, string userProviderSchemaTableName, bool isMultiScheme );
    }
}
