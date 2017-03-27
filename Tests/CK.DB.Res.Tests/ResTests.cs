using CK.Core;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.Tests
{
    [TestFixture]
    public class ResTests
    {
        [Test]
        public async Task creating_and_destroying_raw_resource()
        {
            var r = TestHelper.StObjMap.Default.Obtain<ResTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int id = await r.CreateAsync( ctx );
                r.Database.AssertScalarEquals( 1, "select count(*) from CK.tRes where ResId = @0", id );
                await r.DestroyAsync( ctx, id );
                r.Database.AssertScalarEquals( 0, "select count(*) from CK.tRes where ResId = @0", id );
            }
        }
    }
}
