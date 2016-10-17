﻿using System;
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
    [SqlTable( "tGroup", Package = typeof( Package ) )]
    [Versions( "5.0.0, 5.0.1" )]
    [SqlObjectItem( "transform:sGroupUserRemove, transform:vGroup" )]
    public abstract partial class GroupTable : Actor.GroupTable
    {
        void Construct( ZoneTable zoneTable )
        {
        }

        [SqlProcedure( "sGroupMove" )]
        public abstract Task MoveGroupAsync( ISqlCallContext ctx, int actorId, int groupId, int newZoneId );

        [SqlProcedure( "transform:sGroupCreate" )]
        public abstract Task<int> CreateGroupAsync( ISqlCallContext ctx, int actorId, int zoneId );

        [SqlProcedure( "transform:sGroupUserAdd" )]
        public abstract Task AddUserAsync( ISqlCallContext ctx, int actorId, int groupId, int userId, bool autoAddUserInZone = false );

    }
}