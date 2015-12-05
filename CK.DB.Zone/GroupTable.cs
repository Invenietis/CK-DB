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
    [SqlTable( "tGroup", Package = typeof( Package ) ), Versions( "3.0.0" )]
    [SqlObjectItem( "sGroupUserAdd, sGroupUserRemove, vGroup" )]
    public abstract class GroupTable : Actor.GroupTable
    {
        void Construct( ZoneTable Zone )
        {
        }

        [SqlProcedureNonQuery( "sGroupCreate" )]
        public abstract int CreateGroup( ISqlCallContext ctx, int actorId, int zoneId );

        [SqlProcedureNonQuery( "sGroupCreate" )]
        public abstract Task<int> CreateGroupAsync( ISqlCallContext ctx, int actorId, int zoneId );

        //[EditorBrowsable( EditorBrowsableState.Never )]
        //public override abstract int CreateGroup( ISqlCallContext ctx, int actorId );

        //[EditorBrowsable( EditorBrowsableState.Never )]
        //public override abstract Task<int> CreateGroupAsync( ISqlCallContext ctx, int actorId );

    }
}