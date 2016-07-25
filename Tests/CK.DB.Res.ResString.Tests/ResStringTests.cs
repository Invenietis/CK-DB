using CK.Core;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.ResString.Tests
{
    [TestFixture]
    public class ResStringTests
    {
        [Test]
        public void setting_and_clearing_resource_string()
        {
            var t = TestHelper.StObjMap.Default.Obtain<ResStringTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int resId = t.ResTable.Create( ctx );
                t.Database.AssertEmptyReader( "select * from CK.tResString where ResId = @0", resId );
                t.SetString( ctx, resId, "Hello World!" );
                t.Database.AssertScalar( Is.EqualTo( "Hello World!" ), "select Value from CK.tResString where ResId = @0", resId );
                t.SetString( ctx, resId, null );
                t.Database.AssertEmptyReader( "select * from CK.tResString where ResId = @0", resId );
                t.SetString( ctx, resId, "Hello World!" );
                t.ResTable.Destroy( ctx, resId );
            }
        }

        [Test]
        public void negative_resource_0_and_1_can_not_be_changed()
        {
            var t = TestHelper.StObjMap.Default.Obtain<ResStringTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => t.SetString( ctx, -1, "No way" ) );
                Assert.Throws<SqlDetailedException>( () => t.SetString( ctx, 0, "No way" ) );
                Assert.Throws<SqlDetailedException>( () => t.SetString( ctx, 1, "No way" ) );
            }
        }
    }
}