using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Zone.SimpleNaming.Tests
{
    [TestFixture]
    public class ZoneNameTests
    {
        [Test]
        public void groups_with_the_same_name_can_exist_in_different_zones()
        {
            var map = TestHelper.StObjMap;
            var z = map.Default.Obtain<ZoneTable>();
            var g = map.Default.Obtain<GroupTable>();
            var gN = map.Default.Obtain<SimpleNaming.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                // We test the 0 zone, we need a unique name
                // since we do not control the names there...
                string name = Guid.NewGuid().ToString();
                int idZone1 = z.CreateZone( ctx, 1 );
                int idZone2 = z.CreateZone( ctx, 1 );
                int idGIn0 = g.CreateGroup( ctx, 1 );
                int idGIn1 = g.CreateGroup( ctx, 1, idZone1 );
                int idGIn2 = g.CreateGroup( ctx, 1, idZone2 );
                Assert.That( gN.GroupRename( ctx, 1, idGIn0, name ), Is.EqualTo( name ) );
                Assert.That( gN.GroupRename( ctx, 1, idGIn1, name ), Is.EqualTo( name ) );
                Assert.That( gN.GroupRename( ctx, 1, idGIn2, name ), Is.EqualTo( name ) );

                g.DestroyGroup( ctx, 1, idGIn0 );
                z.DestroyZone( ctx, 1, idZone1, forceDestroy: true );
                z.DestroyZone( ctx, 1, idZone2, forceDestroy: true );
            }
        }

    }
}
