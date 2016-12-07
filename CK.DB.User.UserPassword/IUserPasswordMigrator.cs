using CK.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.User.UserPassword
{
    /// <summary>
    /// Implementation of this interface can be set on <see cref="Package.PasswordMigrator"/>.
    /// Implmentation must be thread-safe. No asynchronous operations are supported in order
    /// to ease implementation and because this will be sollicitated only once per user: once a user 
    /// has a UserPassword facet, this migrator is ignored.
    /// </summary>
    public interface IUserPasswordMigrator
    {
        /// <summary>
        /// Must validate the password against any external password management that should be migrated.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The password to challenge.</param>
        /// <returns>True if the pasword matches, false otherwise.</returns>
        bool VerifyPassword( ISqlCallContext ctx, int userId, string password );

        /// <summary>
        /// Called when a migration succeeded. 
        /// This can be used to cleanup the previous data and/or transfer information.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="userId">The user identifier.</param>
        void MigrationDone( ISqlCallContext ctx, int userId );
    }
}
