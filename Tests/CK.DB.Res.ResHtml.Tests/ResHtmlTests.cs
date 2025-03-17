using CK.Core;
using CK.SqlServer;
using CK.Testing;
using Shouldly;
using NUnit.Framework;

namespace CK.DB.Res.ResHtml.Tests;

[TestFixture]
public class ResHtmlTests
{
    [Test]
    public void setting_and_clearing_resource_string()
    {
        var t = SharedEngine.Map.StObjs.Obtain<ResHtmlTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int resId = t.ResTable.Create( ctx );
            t.Database.ExecuteReader( "select * from CK.tResHtml where ResId = @0", resId )
                .Rows.ShouldBeEmpty();
            t.SetHtml( ctx, resId, "Hello World!" );
            t.Database.ExecuteScalar( "select Value from CK.tResHtml where ResId = @0", resId )
                .ShouldBe( "Hello World!" );
            t.SetHtml( ctx, resId, null );
            t.Database.ExecuteReader( "select * from CK.tResHtml where ResId = @0", resId )
                .Rows.ShouldBeEmpty();
            t.SetHtml( ctx, resId, "Hello World!" );
            t.ResTable.Destroy( ctx, resId );
        }
    }

    [Test]
    public void negative_resource_0_and_1_can_not_be_changed()
    {
        var t = SharedEngine.Map.StObjs.Obtain<ResHtmlTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            Util.Invokable( () => t.SetHtml( ctx, -1, "No way" ) ).ShouldThrow<SqlDetailedException>();
            Util.Invokable( () => t.SetHtml( ctx, 0, "No way" ) ).ShouldThrow<SqlDetailedException>();
            Util.Invokable( () => t.SetHtml( ctx, 1, "No way" ) ).ShouldThrow<SqlDetailedException>();
        }
    }
}
