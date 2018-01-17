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

namespace CK.DB.Res.ResText.Tests
{
    [TestFixture]
    public class ResTextTests
    {
        [Test]
        public void setting_and_clearing_resource_string()
        {
            var t = TestHelper.StObjMap.Default.Obtain<ResTextTable>();
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
            var t = TestHelper.StObjMap.Default.Obtain<ResTextTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => t.SetText( ctx, -1, "No way" ) );
                Assert.Throws<SqlDetailedException>( () => t.SetText( ctx, 0, "No way" ) );
                Assert.Throws<SqlDetailedException>( () => t.SetText( ctx, 1, "No way" ) );
            }
        }
    }
}
