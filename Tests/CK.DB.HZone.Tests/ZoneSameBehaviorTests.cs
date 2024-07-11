using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System;
using FluentAssertions;
using static CK.Testing.MonitorTestHelper;
using CK.Testing;

namespace CK.DB.HZone.Tests
{
    [TestFixture]
    public class ZoneSameBehaviorTests
    {
        [TearDown]
        public void CheckCKCoreInvariant()
        {
            SharedEngine.Map.StObjs.Obtain<SqlDefaultDatabase>().GetCKCoreInvariantsViolations()
                .Rows.Should().BeEmpty();
        }

        [Test]
        public void by_default_when_a_group_is_moved_all_of_its_users_must_be_already_registered_in_the_target_zone()
        {
            var map = SharedEngine.Map;
            var g = map.StObjs.Obtain<GroupTable>();
            var z = map.StObjs.Obtain<ZoneTable>();
            var u = map.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int idUser = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                int idSubZone = z.CreateZone( ctx, 1 );
                int idZoneEmpty = z.CreateZone( ctx, 1 );
                int idZoneOK = z.CreateZone( ctx, 1 );

                z.AddUser( ctx, 1, idSubZone, idUser );
                z.AddUser( ctx, 1, idZoneOK, idUser );
                // This works since the user is in the zoneOK.
                z.MoveZone( ctx, 1, idSubZone, idZoneOK );
                // User is in the Group and in the ZoneOK.
                u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idSubZone} and ActorId = {idUser}" )
                    .Should().Be( idUser );
                u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idZoneOK} and ActorId = {idUser}" )
                    .Should().Be( idUser );

                // This does not: ZoneEmpty does not contain the user.
                // This uses the default option: GroupMoveOption.None.
                z.Invoking( sut => sut.MoveZone( ctx, 1, idSubZone, idZoneEmpty ) ).Should().Throw<SqlDetailedException>();
                // User is still in the Group.
                u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idSubZone} and ActorId = {idUser}" )
                    .Should().Be( idUser );
                // ...and still not in the ZoneEmpty.
                u.Database.ExecuteReader( $"select ActorId from CK.tActorProfile where GroupId = {idZoneEmpty} and ActorId = {idUser}" )
                    .Rows.Should().BeEmpty();
            }
        }

        [Test]
        public void with_option_Intersect_when_a_group_is_moved_its_users_not_already_registered_in_the_target_zone_are_removed()
        {
            var map = SharedEngine.Map;
            var z = map.StObjs.Obtain<ZoneTable>();
            var u = map.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int idUser = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                int idSubZone = z.CreateZone( ctx, 1 );
                int idZoneEmpty = z.CreateZone( ctx, 1 );
                int idZoneOK = z.CreateZone( ctx, 1 );

                z.AddUser( ctx, 1, idSubZone, idUser );
                z.AddUser( ctx, 1, idZoneOK, idUser );
                // This works since the user is in the zoneOK (Intersect does nothing).
                z.MoveZone( ctx, 1, idSubZone, idZoneOK, Zone.GroupMoveOption.Intersect );
                // User is in the Group and in the ZoneOK.
                u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idSubZone} and ActorId = {idUser}" )
                    .Should().Be( idUser );
                u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idZoneOK} and ActorId = {idUser}" )
                    .Should().Be( idUser );

                // This does work... But the user is removed from the group
                // to preserve the 'Group.UserNotInZone' invariant.
                z.MoveZone( ctx, 1, idSubZone, idZoneEmpty, Zone.GroupMoveOption.Intersect );
                // User is no more in the Group: it has been removed.
                u.Database.ExecuteReader( $"select ActorId from CK.tActorProfile where GroupId = {idSubZone} and ActorId = {idUser}" )
                    .Rows.Should().BeEmpty();
                // ...and still not in the ZoneEmpty.
                u.Database.ExecuteReader( $"select ActorId from CK.tActorProfile where GroupId = {idZoneEmpty} and ActorId = {idUser}" )
                    .Rows.Should().BeEmpty();
            }
        }

        [Test]
        public void with_option_AutoUserRegistration_when_a_group_is_moved_its_users_not_already_registered_in_the_target_zone_are_automatically_registered()
        {
            var map = SharedEngine.Map;
            var z = map.StObjs.Obtain<ZoneTable>();
            var u = map.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int idUser = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                int idSubZone = z.CreateZone( ctx, 1 );
                int idZoneEmpty = z.CreateZone( ctx, 1 );
                int idZoneOK = z.CreateZone( ctx, 1 );

                z.AddUser( ctx, 1, idSubZone, idUser );
                z.AddUser( ctx, 1, idZoneOK, idUser );
                // This works since the user is in the zoneOK (Intersect does nothing).
                z.MoveZone( ctx, 1, idSubZone, idZoneOK, Zone.GroupMoveOption.AutoUserRegistration );
                // User is in the Group and in the ZoneOK.
                u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idSubZone} and ActorId = {idUser}" )
                    .Should().Be( idUser );
                u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idZoneOK} and ActorId = {idUser}" )
                    .Should().Be( idUser );

                // This does work: and the user is automatically added to the target Zone!
                // (the 'Group.UserNotInZone' invariant is preserved).
                z.MoveZone( ctx, 1, idSubZone, idZoneEmpty, Zone.GroupMoveOption.AutoUserRegistration );
                // User is no more in the Group: it has been removed.
                u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idSubZone} and ActorId = {idUser}" )
                    .Should().Be( idUser );
                // ...and still not in the ZoneEmpty.
                u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idZoneEmpty} and ActorId = {idUser}" )
                     .Should().Be( idUser );
            }
        }

    }
}
