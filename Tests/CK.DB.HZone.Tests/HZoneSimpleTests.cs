using CK.Core;
using CK.DB.Actor;
using CK.DB.HZone;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.HZone.Tests
{
    [TestFixture]
    public class HZoneSimpleTests
    {

        [Test]
        public void adding_a_user_in_a_child_zone_support_AutoAddUserInParentZone()
        {
            var map = TestHelper.StObjMap;
            var zone = map.Default.Obtain<ZoneTable>();
            var group = map.Default.Obtain<Zone.GroupTable>();
            var user = map.Default.Obtain<UserTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                int idUser1 = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                int idUser2 = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                var allZones = new List<int>();
                allZones.Add( zone.CreateZone( ctx, 1 ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[0] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[1] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[2] ) );
                int idGroup = group.CreateGroup( ctx, 1, allZones[3] );

                Assert.Throws<SqlDetailedException>( () => zone.AddUser( ctx, 1, allZones[3], idUser1, autoAddUserInParentZone: false ) );
                zone.AddUser( ctx, 1, allZones[3], idUser1, autoAddUserInParentZone: true );

                Assert.Throws<SqlDetailedException>( () => group.AddUser( ctx, 1, idGroup, idUser2, autoAddUserInZone: false ) );
                group.AddUser( ctx, 1, idGroup, idUser2, autoAddUserInZone: true );

                user.Database.AssertScalarEquals( 4, "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser1 );
                user.Database.AssertScalarEquals( 5, "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 );

                zone.DestroyZone( ctx, 1, allZones[0], forceDestroy: true );
            }
        }

        [Test]
        public void removing_a_user_from_a_zone_removes_it_from_all_child_zones()
        {
            var map = TestHelper.StObjMap;
            var zone = map.Default.Obtain<ZoneTable>();
            var group = map.Default.Obtain<Zone.GroupTable>();
            var user = map.Default.Obtain<UserTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                int idUser1 = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                int idUser2 = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                var allZones = new List<int>();
                allZones.Add( zone.CreateZone( ctx, 1 ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[0] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[1] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[2] ) );
                int idGroup = group.CreateGroup( ctx, 1, allZones[3] );

                zone.AddUser( ctx, 1, allZones[3], idUser1, autoAddUserInParentZone: true );
                group.AddUser( ctx, 1, idGroup, idUser2, autoAddUserInZone: true );

                user.Database.AssertScalarEquals( 4, "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser1 );
                user.Database.AssertScalarEquals( 5, "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 );

                zone.RemoveUser( ctx, 1, allZones[2], idUser2 );
                user.Database.AssertScalarEquals( 2, "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 );
                group.RemoveUser( ctx, 1, allZones[1], idUser2 );
                user.Database.AssertScalarEquals( 1, "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 );
                group.RemoveUser( ctx, 1, allZones[0], idUser2 );
                user.Database.AssertScalarEquals( 0, "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 );

                zone.RemoveUser( ctx, 1, allZones[0], idUser1 );
                user.Database.AssertScalarEquals( 0, "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 );

                zone.DestroyZone( ctx, 1, allZones[0], forceDestroy: true );
            }
        }

        [Test]
        public void creating_and_destroying_zone_with_sub_zones_and_groups_when_ForceDestroy_is_true()
        {
            var map = TestHelper.StObjMap;
            var zone = map.Default.Obtain<ZoneTable>();
            var group = map.Default.Obtain<Zone.GroupTable>();
            var user = map.Default.Obtain<UserTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                var allZones = new List<int>();
                var allGroups = new List<int>();
                allZones.Add( zone.CreateZone( ctx, 1 ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[0] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[0] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[1] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[1] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[3] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[3] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[4] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[4] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[4] ) );
                allZones.Add( zone.CreateZone( ctx, 1, allZones[4] ) );
                foreach( var idZone in allZones )
                {
                    allGroups.Add( group.CreateGroup( ctx, 1, idZone ) );
                }
                Assert.Throws<SqlDetailedException>( () => zone.DestroyZone( ctx, 1, allZones[0], forceDestroy: false ) );
                zone.DestroyZone( ctx, 1, allZones[0], forceDestroy: true );
            }
        }

        [Test]
        public void moving_a_zone_in_the_tree()
        {
            var map = TestHelper.StObjMap;
            var zone = map.Default.Obtain<ZoneTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                var allZones = new List<int>();
                for( int i = 0; i < 10; ++i ) allZones.Add( zone.CreateZone( ctx, 1 ) );
                // 0 
                // | 1
                // | 2
                // | 3
                // | 4
                zone.MoveZone( ctx, 1, allZones[1], allZones[0] );
                zone.MoveZone( ctx, 1, allZones[2], allZones[0] );
                zone.MoveZone( ctx, 1, allZones[3], allZones[0] );
                zone.MoveZone( ctx, 1, allZones[4], allZones[0] );
                // 5 
                // | 7
                // | 6
                // | 9
                // | 8
                zone.MoveZone( ctx, 1, allZones[6], allZones[5] );
                zone.MoveZone( ctx, 1, allZones[7], allZones[5], allZones[6] );
                zone.MoveZone( ctx, 1, allZones[8], allZones[5] );
                zone.MoveZone( ctx, 1, allZones[9], allZones[5], allZones[8] );
            }
        }
    }
}