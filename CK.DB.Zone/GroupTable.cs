using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.SqlServer.Setup;
using CK.Setup;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CK.SqlServer;
using System.ComponentModel;

namespace CK.DB.Zone
{
    [SqlTable( "tGroup", Package = typeof( Package ) ), Versions( "5.0.0" )]
    [SqlObjectItem( "transform:sGroupUserAdd, transform:sGroupUserRemove, transform:vGroup" )]
    public abstract class GroupTable : Actor.GroupTable
    {
        void Construct( ZoneTable Zone )
        {
        }

        [SqlProcedureNonQuery( "transform:sGroupCreate" )]
        public abstract int CreateGroup( ISqlCallContext ctx, int actorId, int zoneId );

        [SqlProcedureNonQuery( "transform:sGroupCreate" )]
        public abstract Task<int> CreateGroupAsync( ISqlCallContext ctx, int actorId, int zoneId );

    }
}