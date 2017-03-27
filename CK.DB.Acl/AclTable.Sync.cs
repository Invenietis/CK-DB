using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Setup;
using CK.SqlServer.Setup;
using CK.SqlServer;
using CK.Core;

namespace CK.DB.Acl
{
    public abstract partial class AclTable : SqlTable
    {
        /// <summary>
        /// Creates a new Acl.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <returns>A new Acl identifier.</returns>
        [SqlProcedure( "sAclCreate" )]
        public abstract int CreateAcl( ISqlCallContext ctx, int actorId );

        /// <summary>
        /// Destroys an Acl.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="aclId">The Acl to destroy.</param>
        [SqlProcedure( "sAclDestroy" )]
        public abstract void DestroyAcl( ISqlCallContext ctx, int actorId, int aclId );

        /// <summary>
        /// Configures an Acl.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="aclId">The Acl identifier to configure.</param>
        /// <param name="actorIdToGrant">The actor identifier to grant or deny.</param>
        /// <param name="keyReason">The reason. Use null or empty string when no specific, applicative reason exists.</param>
        /// <param name="grantLevel">The grant level to set. Greater than 127 is a deny (value is 255-GrantLevel).</param>
        [SqlProcedure( "sAclGrantSet" )]
        public abstract void AclGrantSet( ISqlCallContext ctx, int actorId, int aclId, int actorIdToGrant, string keyReason, byte grantLevel );

        /// <summary>
        /// Reads the final GrantLevel for an Actor on an Acl.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The actor identifier.</param>
        /// <param name="aclId">The acl identifier.</param>
        /// <returns>The GrantLevel between 0 (Blind) and 127 (Administrator)</returns>
        [SqlScalarFunction( "fAclGrantLevel" )]
        public abstract byte GetGrantLevel( ISqlCallContext ctx, int actorId, int aclId );

    }
}
