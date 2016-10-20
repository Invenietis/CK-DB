using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Setup;
using System.Data.SqlClient;
using CK.SqlServer.Setup;

namespace CK.DB.Actor
{
    /// <summary>
    /// Internal Actor x Group relation.
    /// </summary>
    [SqlTable( "tActorProfile", Package = typeof( Package ) )]
    [Versions( "5.0.0" )]
    public abstract class ActorProfileTable : SqlTable
    {
        void Construct( ActorTable actor, GroupTable group )
        {
        }
    }
}
