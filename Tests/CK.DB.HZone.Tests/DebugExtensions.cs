using CK.Core;
using CK.SqlServer;
using CK.SqlServer.Setup;
using CK.Text;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.HZone.Tests
{
    static public class DebugExtensions
    {
        static public string DumpTree( this ZoneTable @this, ISqlCallContext ctx, int zoneId )
        {
            using( var c = new SqlConnectionProvider( @this.Database.ConnectionString ) )
            using( var cmd = new SqlCommand( $"select ChildId, ChildDepth from CK.vZoneAllChildren where ZoneId={zoneId} order by ChildOrderByKey" ) )
            using( var r = c.AcquireReader( cmd ) )
            {
                StringBuilder b = new StringBuilder();
                int firstDepth = -1;
                while( r.Read() )
                {
                    int depth = r.GetInt32( 1 );
                    if( firstDepth < 0 ) firstDepth = depth;
                    depth -= firstDepth;
                    b.Append( '+', depth ).Append( r.GetInt32( 0 ) ).AppendLine();
                }
                return b.ToString();
            }
        }

        static public bool CheckTree( this ZoneTable @this, ISqlCallContext ctx, int zoneId, string tree )
        {
            string dump = @this.DumpTree( ctx, zoneId );
            tree = tree.NormalizeEOL().Replace( " ", string.Empty );
            return dump == tree;
        }

    }
}
