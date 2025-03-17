using CK.SqlServer;
using Shouldly;
using Microsoft.Data.SqlClient;
using System.Text;

namespace CK.DB.HZone.Tests;

static public class DebugExtensions
{
    static public string DumpTree( this ZoneTable @this, ISqlCallContext ctx, int zoneId )
    {
        using( var cmd = new SqlCommand( $"select ChildId, ChildDepth from CK.vZoneAllChildren where ZoneId={zoneId} order by ChildOrderByKey" ) )
        {
            return ctx[@this.Database].ExecuteQuery( cmd, c =>
            {
                using( var r = cmd.ExecuteReader() )
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
            } );
        }
    }

    static public void CheckTree( this ZoneTable @this, ISqlCallContext ctx, int zoneId, string tree )
    {
        string dump = @this.DumpTree( ctx, zoneId ).TrimEnd();
        tree = tree.Trim().ReplaceLineEndings().Replace( " ", string.Empty );
        dump.ShouldBe( tree );
    }

}
