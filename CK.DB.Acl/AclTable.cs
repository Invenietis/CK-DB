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
    [SqlTable( "tAcl", Package = typeof( Package ) ), Versions( "3.0.0" )]
    [SqlObjectItem( "vAclActor, vAclActorReason" )]
    public abstract class AclTable : SqlTable
    {
        void Construct()
        {
        }

        [SqlProcedureNonQuery( "sAclCreate" )]
        public abstract int CreateAcl( ISqlCallContext ctx, int actorId );

        [SqlProcedureNonQuery( "sAclDestroy" )]
        public abstract void DestroyAcl( ISqlCallContext ctx, int actorId, int aclId );

        [SqlProcedureNonQuery( "sAclGrantSet" )]
        public abstract void AclGrantSet( ISqlCallContext ctx, int actorId, int aclId, int actorIdToGrant, string keyReason, byte grantLevel );

        [SqlProcedureNonQuery( "sAclGrantSet" )]
        public abstract Task AclGrantSetAsync( ISqlCallContext ctx, int actorId, int aclId, int actorIdToGrant, string keyReason, byte grantLevel );

        [SqlScalarFunction( "fAclGrantLevel" )]
        public abstract byte GetGrantLevel( ISqlCallContext ctx, int actorId, int aclId );

    }
}
