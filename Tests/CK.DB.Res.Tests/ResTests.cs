using CK.Core;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Res.Tests
{
    [TestFixture]
    public class ResTests
    {
        [Test]
        public async Task creating_and_destroying_raw_resource_Async()
        {
            var r = SharedEngine.Map.StObjs.Obtain<ResTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int id = await r.CreateAsync( ctx );
                r.Database.ExecuteScalar( "select count(*) from CK.tRes where ResId = @0", id )
                    .Should().Be( 1 );
                await r.DestroyAsync( ctx, id );
                r.Database.ExecuteScalar( "select count(*) from CK.tRes where ResId = @0", id )
                    .Should().Be( 0 );
            }
        }
    }
}
