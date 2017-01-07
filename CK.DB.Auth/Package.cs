using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
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
    public abstract class Package : SqlPackage
    {
        void Construct( Actor.Package actor )
        {
        }

        /// <summary>
        /// Calls the OnUserLogin hook.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="providerName">The provider name.</param>
        /// <param name="loginTime">Login time.</param>
        /// <param name="userId">The user identifier.</param>
        [SqlProcedure( "sAuthUserOnLogin" )]
        public abstract void OnUserLogin( ISqlCallContext ctx, string providerName, DateTime loginTime, int userId );

        /// <summary>
        /// Calls the OnUserLogin hook.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="providerName">The provider name.</param>
        /// <param name="loginTime">Login time.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sAuthUserOnLogin" )]
        public abstract Task OnUserLoginAsync( ISqlCallContext ctx, string providerName, DateTime loginTime, int userId );
    }
}
