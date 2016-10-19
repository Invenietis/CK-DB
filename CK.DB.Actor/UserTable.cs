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
    [Versions( "5.0.0, 5.0.1, 5.0.2" )]
    [SqlObjectItem( "vUser" )]
    public abstract partial class UserTable : SqlTable
    {
        void Construct( ActorTable actor )
        {
        }
        
        [SqlProcedure( "sUserCreate" )]
        public abstract Task<int> CreateUserAsync( ISqlCallContext ctx, int actorId, string userName );

        [SqlProcedure( "sUserUserNameSet" )]
        public abstract Task<bool> UserNameSetAsync( ISqlCallContext ctx, int actorId, int userId, string userName );

        [SqlProcedure( "sUserDestroy" )]
        public abstract Task DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId );

        [SqlProcedure( "sUserRemoveFromAllGroups" )]
        public abstract Task RemoveFromAllGroupsAsync( ISqlCallContext ctx, int actorId, int userId );
    }
}
