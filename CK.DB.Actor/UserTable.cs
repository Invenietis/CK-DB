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
    [SqlTable( "tUser", Package = typeof( Package ) )]
    [Versions( "5.0.0" )]
    [SqlObjectItem( "vUser" )]
    public abstract partial class UserTable : SqlTable
    {
        void Construct( ActorTable actor )
        {
        }
        
        [SqlProcedureNonQuery( "sUserCreate" )]
        public abstract Task<int> CreateUserAsync( ISqlCallContext ctx, int actorId, string userName );

        [SqlProcedureNonQuery( "sUserUserNameSet" )]
        public abstract Task<bool> UserNameSetAsync( ISqlCallContext ctx, int actorId, int userId, string userName );

        [SqlProcedureNonQuery( "sUserDestroy" )]
        public abstract Task DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId );

        [SqlProcedureNonQuery( "sUserRemoveFromAllGroups" )]
        public abstract Task RemoveFromAllGroupsAsync( ISqlCallContext ctx, int actorId, int userId );
    }
}
