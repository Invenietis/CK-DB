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
    public abstract partial class GroupTable 
    {
        [SqlProcedure( "sGroupMove" )]
        public abstract void MoveGroup( ISqlCallContext ctx, int actorId, int groupId, int newZoneId );

        [SqlProcedure( "transform:sGroupCreate" )]
        public abstract int CreateGroup( ISqlCallContext ctx, int actorId, int zoneId );

        [SqlProcedure( "transform:sGroupUserAdd" )]
        public abstract void AddUser( ISqlCallContext ctx, int actorId, int groupId, int userId, bool autoAddUserInZone = false );
    }
}