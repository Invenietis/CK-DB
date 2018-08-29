using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.Zone.SimpleNaming.Tests
{
    [TestFixture]
    public class ZoneNameTests
    {
        [Test]
        public void groups_with_the_same_name_can_exist_in_different_zones()
        {
            var map = TestHelper.StObjMap;
            var z = map.StObjs.Obtain<ZoneTable>();
            var g = map.StObjs.Obtain<GroupTable>();
            var gN = map.StObjs.Obtain<SimpleNaming.Package>();
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
                gN.GroupRename( ctx, 1, idGIn0, name ).Should().Be( name );
                gN.GroupRename( ctx, 1, idGIn1, name ).Should().Be( name );
                gN.GroupRename( ctx, 1, idGIn2, name ).Should().Be( name );

                g.DestroyGroup( ctx, 1, idGIn0 );
                z.DestroyZone( ctx, 1, idZone1, forceDestroy: true );
                z.DestroyZone( ctx, 1, idZone2, forceDestroy: true );
            }
        }

        [Test]
        public void when_groups_are_moved_name_clash_are_automatically_handled()
        {
            var map = TestHelper.StObjMap;
            var z = map.StObjs.Obtain<ZoneTable>();
            var g = map.StObjs.Obtain<GroupTable>();
            var gN = map.StObjs.Obtain<SimpleNaming.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int idZone1 = z.CreateZone( ctx, 1 );
                int idZone2 = z.CreateZone( ctx, 1 );
                int idGIn1 = g.CreateGroup( ctx, 1, idZone1 );
                gN.GroupRename( ctx, 1, idGIn1, "Test" );
                int idGIn2 = g.CreateGroup( ctx, 1, idZone2 );
                gN.GroupRename( ctx, 1, idGIn2, "Test" );
                g.MoveGroup( ctx, 1, idGIn1, idZone2 );
                g.Database.ExecuteScalar( "select GroupName from CK.vGroup where GroupId = @0", idGIn1 )
                    .Should().Be( "Test (1)" );
            }
        }
    }
}
