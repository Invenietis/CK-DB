using CK.Core;
using CK.SqlServer;
using CK.Testing;
using Shouldly;
using NUnit.Framework;

namespace CK.DB.Res.ResString.Tests;

[TestFixture]
public class ResStringTests
{
    [Test]
    public void setting_and_clearing_resource_string()
    {
        var t = SharedEngine.Map.StObjs.Obtain<ResStringTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int resId = t.ResTable.Create( ctx );
            t.Database.ExecuteReader( "select * from CK.tResString where ResId = @0", resId )
                .Rows.ShouldBeEmpty();
            t.SetString( ctx, resId, "Hello World!" );
            t.Database.ExecuteScalar( "select Value from CK.tResString where ResId = @0", resId )
                .ShouldBe( "Hello World!" );
            t.SetString( ctx, resId, null );
            t.Database.ExecuteReader( "select * from CK.tResString where ResId = @0", resId )
                .Rows.ShouldBeEmpty();
            t.SetString( ctx, resId, "Hello World!" );
            t.ResTable.Destroy( ctx, resId );
        }
    }

    [Test]
    public void negative_resource_and_0_can_not_be_changed()
    {
        var t = SharedEngine.Map.StObjs.Obtain<ResStringTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            Util.Invokable( () => t.SetString( ctx, -1, "No way" ) ).ShouldThrow<SqlDetailedException>();
            Util.Invokable( () => t.SetString( ctx, 0, "No way" ) ).ShouldThrow<SqlDetailedException>();
            Util.Invokable( () => t.SetString( ctx, 1, "Le Système" ) ).ShouldNotThrow();
            t.SetString( ctx, 1, "System" );
        }
    }
}
