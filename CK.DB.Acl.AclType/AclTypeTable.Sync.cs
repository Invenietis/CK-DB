using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System.Threading.Tasks;

namespace CK.DB.Acl.AclType
{
    public abstract partial class AclTypeTable : SqlTable
    {
        [SqlProcedure( "sAclTypeCreate" )]
        public abstract int CreateAclType( ISqlCallContext ctx, int actorId );

        [SqlProcedure( "sAclTypeDestroy" )]
        public abstract void DestroyAclType( ISqlCallContext ctx, int actorId, int aclTypeId );

        [SqlProcedure( "sAclTypeConstrainedGrantLevelSet" )]
        public abstract void SetConstrainedGrantLevel( ISqlCallContext ctx, int actorId, int aclTypeId, bool set );

        [SqlProcedure( "sAclTypeGrantLevelSet" )]
        public abstract void SetGrantLevel( ISqlCallContext ctx, int actorId, int aclTypeId, byte grantLevel, bool set );

        /// <summary>
        /// Creates a typed acl.
        /// </summary>
        /// <param name="ctx">Call context to use.</param>
        /// <param name="actorId">Calling actor identifier.</param>
        /// <param name="aclTypeId">The acl type identifier.</param>
        /// <returns>A new Acl identifier that is bound to the Acl type.</returns>
        [SqlProcedure( "transform:sAclCreate" )]
        public abstract int CreateAcl( ISqlCallContext ctx, int actorId, int aclTypeId );

    }
}
