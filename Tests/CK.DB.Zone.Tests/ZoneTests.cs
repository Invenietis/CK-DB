using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;

namespace CK.DB.Zone.Tests
{
    [TestFixture]
    public class ZoneTests
    {
        [Test]
        public void zone_0_and_1_can_not_be_destroyed()
        {
            var t = TestHelper.StObjMap.Default.Obtain<ZoneTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => t.DestroyZone( ctx, 1, 0 ) );
                Assert.Throws<SqlDetailedException>( () => t.DestroyZone( ctx, 1, 1 ) );
            }
        }

        [Test]
        public void zone_can_be_created_and_destroyed_by_System()
        {
            var t = TestHelper.StObjMap.Default.Obtain<ZoneTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int zoneId = t.CreateZone( ctx, 1 );
                Assert.That( zoneId, Is.GreaterThan( 1 ) );
                t.DestroyZone( ctx, 1, zoneId );
                t.Database.AssertEmptyReader( "select * from CK.tZone where ZoneId=@0", zoneId );
            }
        }

        [Test]
        public void zone_with_existing_groups_can_be_destroyed_when_ForceDestroy_is_true()
        {
            var t = TestHelper.StObjMap.Default.Obtain<ZoneTable>();
            var g = TestHelper.StObjMap.Default.Obtain<GroupTable>();
            var u = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int zoneId = t.CreateZone( ctx, 1 );
                Assert.That( zoneId, Is.GreaterThan( 1 ) );

                int groupId1 = g.CreateGroup( ctx, zoneId );
                int groupId2 = g.CreateGroup( ctx, zoneId );

                int userId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString("N") );
                t.AddUser( ctx, 1, zoneId, userId );
                g.AddUser( ctx, 1, groupId1, userId );
                g.AddUser( ctx, 1, groupId2, userId );

                t.DestroyZone( ctx, 1, zoneId, true );

                t.Database.AssertEmptyReader( "select * from CK.tGroup where ZoneId=@0", zoneId );
                t.Database.AssertEmptyReader( "select * from CK.tZone where ZoneId=@0", zoneId );
            }
        }

        [Test]
        public void Anonymous_cant_not_create_or_destroy_a_zone()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Zone.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => p.ZoneTable.CreateZone( ctx, 0 ) );

                int zoneId = p.ZoneTable.CreateZone( ctx, 1 );
                Assert.Throws<SqlDetailedException>( () => p.ZoneTable.DestroyZone( ctx, 0, zoneId ) );
                p.ZoneTable.DestroyZone( ctx, 1, zoneId );
            }
        }

        [Test]
        public void adding_a_user_to_a_group_when_he_is_not_registered_in_the_zone_is_an_error()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Zone.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int zoneId = p.ZoneTable.CreateZone( ctx, 1 );
                int userId = p.UserTable.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                int groupId = p.GroupTable.CreateGroup( ctx, 1, zoneId );

                Assert.Throws<SqlDetailedException>( () => p.GroupTable.AddUser( ctx, 1, groupId, userId ) );

                Assert.DoesNotThrow( () => p.ZoneTable.AddUser( ctx, 1, zoneId, userId ), "Adding the user to the zone." );
                Assert.DoesNotThrow( () => p.GroupTable.AddUser( ctx, 1, groupId, userId ), "Adding the user to group: now it works." );

                Assert.DoesNotThrow( () => p.GroupTable.AddUser( ctx, 1, groupId, userId ), "If the user already exists in the zone, it is okay." );
                Assert.DoesNotThrow( () => p.ZoneTable.AddUser( ctx, 1, zoneId, userId ), "Just like Groups: adding an already existing user to a Zone is okay." );

                p.ZoneTable.DestroyZone( ctx, 1, zoneId, true );
            }
        }

        [Test]
        public void destroying_a_zone_with_any_other_group_than_AdministratorsGroup_is_an_error_by_default()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Zone.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int zoneId = p.ZoneTable.CreateZone( ctx, 1 );
                int groupId = p.GroupTable.CreateGroup( ctx, 1, zoneId );

                Assert.Throws<SqlDetailedException>( () => p.ZoneTable.DestroyZone( ctx, 1, zoneId ) );

                p.GroupTable.DestroyGroup( ctx, 1, groupId );
                p.ZoneTable.DestroyZone( ctx, 1, zoneId );

                p.Database.AssertEmptyReader( "select * from CK.tZone where ZoneId=@0", zoneId );
            }
        }

        [Test]
        public void removing_a_user_from_a_Zone_removes_him_from_all_groups()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Zone.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int zoneId = p.ZoneTable.CreateZone( ctx, 1 );
                int userId = p.UserTable.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                int groupId1 = p.GroupTable.CreateGroup( ctx, 1, zoneId );
                int groupId2 = p.GroupTable.CreateGroup( ctx, 1, zoneId );

                p.ZoneTable.AddUser( ctx, 1, zoneId, userId );
                p.GroupTable.AddUser( ctx, 1, groupId1, userId );
                p.GroupTable.AddUser( ctx, 1, groupId2, userId );

                p.Database.AssertScalarEquals( 3, "select GroupCount = count(*)-1 from CK.tActorProfile where ActorId = @0", userId );

                p.ZoneTable.RemoveUser( ctx, 1, zoneId, userId );

                p.Database.AssertScalarEquals( 0, "select GroupCount = count(*)-1 from CK.tActorProfile where ActorId = @0", userId );

                p.GroupTable.DestroyGroup( ctx, 1, groupId1 );
                p.GroupTable.DestroyGroup( ctx, 1, groupId2 );
                p.ZoneTable.DestroyZone( ctx, 1, zoneId );
                p.UserTable.DestroyUser( ctx, 1, userId );
            }
        }

        [Test]
        public void can_not_create_a_group_in_System_group()
        {
            var map = TestHelper.StObjMap;
            var g = map.Default.Obtain<GroupTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => g.CreateGroup( ctx, 1, 1 ) );
            }
        }

    }
}
