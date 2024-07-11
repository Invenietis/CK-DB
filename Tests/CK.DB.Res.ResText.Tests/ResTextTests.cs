using CK.Core;
using CK.SqlServer;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Res.ResText.Tests
{
    [TestFixture]
    public class ResTextTests
    {
        [Test]
        public void setting_and_clearing_resource_string()
        {
            var t = SharedEngine.Map.StObjs.Obtain<ResTextTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int resId = t.ResTable.Create( ctx );
                t.Database.ExecuteReader( "select * from CK.tResText where ResId = @0", resId )
                    .Rows.Should().BeEmpty();
                t.SetText( ctx, resId, "Hello World!" );
                t.Database.ExecuteScalar( "select Value from CK.tResText where ResId = @0", resId )
                    .Should().Be( "Hello World!" );
                t.SetText( ctx, resId, null );
                t.Database.ExecuteReader( "select * from CK.tResText where ResId = @0", resId )
                    .Rows.Should().BeEmpty();
                t.SetText( ctx, resId, "Hello World!" );
                t.ResTable.Destroy( ctx, resId );
            }
        }

        [Test]
        public void negative_resource_0_and_1_can_not_be_changed()
        {
            var t = SharedEngine.Map.StObjs.Obtain<ResTextTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                t.Invoking( sut => sut.SetText( ctx, -1, "No way" ) ).Should().Throw<SqlDetailedException>();
                t.Invoking( sut => sut.SetText( ctx, 0, "No way" ) ).Should().Throw<SqlDetailedException>();
                t.Invoking( sut => sut.SetText( ctx, 1, "No way" ) ).Should().Throw<SqlDetailedException>();
            }
        }
    }
}
