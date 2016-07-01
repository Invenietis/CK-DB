using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.SqlServer.Setup;
using CK.Setup;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Actor
{
    public abstract partial class UserTable : SqlTable
    {        
        [SqlProcedure( "sUserCreate" )]
        public abstract int CreateUser( ISqlCallContext ctx, int actorId, string userName );

        [SqlProcedure( "sUserUserNameSet" )]
        public abstract bool UserNameSet( ISqlCallContext ctx, int actorId, int userId, string userName );

        [SqlProcedure( "sUserDestroy" )]
        public abstract void DestroyUser( ISqlCallContext ctx, int actorId, int userId );

        [SqlProcedure( "sUserRemoveFromAllGroups" )]
        public abstract void RemoveFromAllGroups( ISqlCallContext ctx, int actorId, int userId );
    }
}
