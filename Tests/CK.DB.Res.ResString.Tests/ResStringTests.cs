using CK.Core;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.Res.ResString.Tests
{
    [TestFixture]
    public class ResStringTests
    {
        [Test]
        public void setting_and_clearing_resource_string()
        {
            var t = TestHelper.StObjMap.StObjs.Obtain<ResStringTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int resId = t.ResTable.Create( ctx );
                t.Database.ExecuteReader( "select * from CK.tResString where ResId = @0", resId )
                    .Rows.Should().BeEmpty();
                t.SetString( ctx, resId, "Hello World!" );
                t.Database.ExecuteScalar( "select Value from CK.tResString where ResId = @0", resId )
                    .Should().Be( "Hello World!" );
                t.SetString( ctx, resId, null );
                t.Database.ExecuteReader( "select * from CK.tResString where ResId = @0", resId )
                    .Rows.Should().BeEmpty();
                t.SetString( ctx, resId, "Hello World!" );
                t.ResTable.Destroy( ctx, resId );
            }
        }

        [Test]
        public void negative_resource_and_0_can_not_be_changed()
        {
            var t = TestHelper.StObjMap.StObjs.Obtain<ResStringTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                t.Invoking( sut => sut.SetString( ctx, -1, "No way" ) ).Should().Throw<SqlDetailedException>();
                t.Invoking( sut => sut.SetString( ctx, 0, "No way" ) ).Should().Throw<SqlDetailedException>();
                t.Invoking( sut => sut.SetString( ctx, 1, "Le Système" ) ).Should().NotThrow();
                t.SetString( ctx, 1, "System" );
            }
        }
    }
}
