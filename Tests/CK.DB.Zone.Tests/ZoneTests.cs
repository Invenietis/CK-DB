using System;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using Shouldly;
using CK.Testing;

namespace CK.DB.Zone.Tests;

[TestFixture]
public class ZoneTests
{
    [TearDown]
    public void CheckInvariants()
    {
        SharedEngine.Map.StObjs.Obtain<SqlDefaultDatabase>().GetCKCoreInvariantsViolations()
            .Rows.ShouldBeEmpty();
    }

    [Test]
    public void zone_0_and_1_can_not_be_destroyed()
    {
        var t = SharedEngine.Map.StObjs.Obtain<ZoneTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            Util.Invokable( () => t.DestroyZone( ctx, 1, 0 ) ).ShouldThrow<SqlDetailedException>();
            Util.Invokable( () => t.DestroyZone( ctx, 1, 1 ) ).ShouldThrow<SqlDetailedException>();
        }
    }

    [Test]
    public void zone_can_be_created_and_destroyed_by_System()
    {
        var t = SharedEngine.Map.StObjs.Obtain<ZoneTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int zoneId = t.CreateZone( ctx, 1 );
            Assert.That( zoneId, Is.GreaterThan( 1 ) );
            t.Database.ExecuteScalar( "select IsZone from CK.vGroup where GroupId=@0", zoneId )
                .ShouldBe( true );
            t.DestroyZone( ctx, 1, zoneId );
            t.Database.ExecuteReader( "select * from CK.tZone where ZoneId=@0", zoneId )
                .Rows.ShouldBeEmpty();
        }
    }

    [Test]
    public void zone_with_existing_groups_can_be_destroyed_when_ForceDestroy_is_true()
    {
        var t = SharedEngine.Map.StObjs.Obtain<ZoneTable>();
        var g = SharedEngine.Map.StObjs.Obtain<GroupTable>();
        var u = SharedEngine.Map.StObjs.Obtain<UserTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int zoneId = t.CreateZone( ctx, 1 );
            Assert.That( zoneId, Is.GreaterThan( 1 ) );

            int groupId1 = g.CreateGroup( ctx, zoneId );
            int groupId2 = g.CreateGroup( ctx, zoneId );

            int userId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString( "N" ) );
            t.AddUser( ctx, 1, zoneId, userId );
            g.AddUser( ctx, 1, groupId1, userId );
            g.AddUser( ctx, 1, groupId2, userId );

            t.DestroyZone( ctx, 1, zoneId, true );

            t.Database.ExecuteReader( "select * from CK.tGroup where ZoneId=@0", zoneId )
                .Rows.ShouldBeEmpty();
            t.Database.ExecuteReader( "select * from CK.tZone where ZoneId=@0", zoneId )
                .Rows.ShouldBeEmpty();
        }
    }

    [Test]
    public void Anonymous_cant_not_create_or_destroy_a_zone()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Zone.Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            Util.Invokable( () => p.ZoneTable.CreateZone( ctx, 0 ) ).ShouldThrow<SqlDetailedException>();

            int zoneId = p.ZoneTable.CreateZone( ctx, 1 );
            Util.Invokable( () => p.ZoneTable.DestroyZone( ctx, 0, zoneId ) ).ShouldThrow<SqlDetailedException>();
            p.ZoneTable.DestroyZone( ctx, 1, zoneId );
        }
    }

    [Test]
    public void adding_a_user_to_a_group_when_he_is_not_registered_in_the_zone_is_an_error()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Zone.Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int zoneId = p.ZoneTable.CreateZone( ctx, 1 );
            int userId = p.UserTable.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
            int groupId = p.GroupTable.CreateGroup( ctx, 1, zoneId );

            Util.Invokable( () => p.GroupTable.AddUser( ctx, 1, groupId, userId ) ).ShouldThrow<SqlDetailedException>();

            Util.Invokable( () => p.ZoneTable.AddUser( ctx, 1, zoneId, userId ) ).ShouldNotThrow( "Adding the user to the zone." );
            Util.Invokable( () => p.GroupTable.AddUser( ctx, 1, groupId, userId ) ).ShouldNotThrow( "Adding the user to group: now it works." );

            Util.Invokable( () => p.GroupTable.AddUser( ctx, 1, groupId, userId ) ).ShouldNotThrow( "If the user already exists in the zone, it is okay." );
            Util.Invokable( () => p.ZoneTable.AddUser( ctx, 1, zoneId, userId ) ).ShouldNotThrow( "Just like Groups: adding an already existing user to a Zone is okay." );

            p.ZoneTable.DestroyZone( ctx, 1, zoneId, true );
        }
    }

    [Test]
    public void by_default_destroying_a_zone_that_has_a_group_is_an_error_ie_when_ForceDestroy_is_false()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Zone.Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int zoneId = p.ZoneTable.CreateZone( ctx, 1 );
            int groupId = p.GroupTable.CreateGroup( ctx, 1, zoneId );

            Util.Invokable( () => p.ZoneTable.DestroyZone( ctx, 1, zoneId ) ).ShouldThrow<SqlDetailedException>();

            p.GroupTable.DestroyGroup( ctx, 1, groupId );
            p.ZoneTable.DestroyZone( ctx, 1, zoneId );

            p.Database.ExecuteReader( "select * from CK.tZone where ZoneId=@0", zoneId )
                .Rows.ShouldBeEmpty();
        }
    }

    [Test]
    public void removing_a_user_from_a_Zone_removes_him_from_all_groups()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Zone.Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int zoneId = p.ZoneTable.CreateZone( ctx, 1 );
            int userId = p.UserTable.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
            int groupId1 = p.GroupTable.CreateGroup( ctx, 1, zoneId );
            int groupId2 = p.GroupTable.CreateGroup( ctx, 1, zoneId );

            p.ZoneTable.AddUser( ctx, 1, zoneId, userId );
            p.GroupTable.AddUser( ctx, 1, groupId1, userId );
            p.GroupTable.AddUser( ctx, 1, groupId2, userId );

            p.Database.ExecuteScalar( "select GroupCount = count(*)-1 from CK.tActorProfile where ActorId = @0", userId )
                .ShouldBe( 3 );

            p.ZoneTable.RemoveUser( ctx, 1, zoneId, userId );

            p.Database.ExecuteScalar( "select GroupCount = count(*)-1 from CK.tActorProfile where ActorId = @0", userId )
                .ShouldBe( 0 );

            p.GroupTable.DestroyGroup( ctx, 1, groupId1 );
            p.GroupTable.DestroyGroup( ctx, 1, groupId2 );
            p.ZoneTable.DestroyZone( ctx, 1, zoneId );
            p.UserTable.DestroyUser( ctx, 1, userId );
        }
    }

    [Test]
    public void can_not_create_a_group_in_System_group()
    {
        var map = SharedEngine.Map;
        var g = map.StObjs.Obtain<GroupTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            Util.Invokable( () => g.CreateGroup( ctx, 1, 1 ) ).ShouldThrow<SqlDetailedException>();
        }
    }

    [Test]
    public void groups_can_be_moved_from_its_zone_to_another_one()
    {
        var map = SharedEngine.Map;
        var g = map.StObjs.Obtain<GroupTable>();
        var z = map.StObjs.Obtain<ZoneTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int idGroup = g.CreateGroup( ctx, 1 );
            int idZone1 = z.CreateZone( ctx, 1 );
            int idZone2 = z.CreateZone( ctx, 1 );

            g.Database.ExecuteScalar( "select ZoneId from CK.vGroup where GroupId=@0", idGroup )
                .ShouldBe( 0 );

            g.MoveGroup( ctx, 1, idGroup, idZone1 );
            g.Database.ExecuteScalar( "select ZoneId from CK.vGroup where GroupId=@0", idGroup )
                .ShouldBe( idZone1 );

            g.MoveGroup( ctx, 1, idGroup, idZone2 );
            g.Database.ExecuteScalar( "select ZoneId from CK.vGroup where GroupId=@0", idGroup )
                .ShouldBe( idZone2 );

            g.MoveGroup( ctx, 1, idGroup, 0 );
            g.Database.ExecuteScalar( "select ZoneId from CK.vGroup where GroupId=@0", idGroup )
                .ShouldBe( 0 );
            g.DestroyGroup( ctx, 1, idGroup );
            z.DestroyZone( ctx, 1, idZone1 );
            z.DestroyZone( ctx, 1, idZone2 );
        }
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
            int idGroup = g.CreateGroup( ctx, 1 );
            int idZoneEmpty = z.CreateZone( ctx, 1 );
            int idZoneOK = z.CreateZone( ctx, 1 );

            g.AddUser( ctx, 1, idGroup, idUser );
            z.AddUser( ctx, 1, idZoneOK, idUser );
            // This works since the user is in the zoneOK.
            g.MoveGroup( ctx, 1, idGroup, idZoneOK );
            // User is in the Group and in the ZoneOK.
            u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idGroup} and ActorId = {idUser}" )
                 .ShouldBe( idUser );
            u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idZoneOK} and ActorId = {idUser}" )
                .ShouldBe( idUser );

            // This does not: ZoneEmpty does not contain the user.
            // This uses the default option: GroupMoveOption.None.
            Util.Invokable( () => g.MoveGroup( ctx, 1, idGroup, idZoneEmpty ) ).ShouldThrow<SqlDetailedException>();
            // User is still in the Group.
            u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idGroup} and ActorId = {idUser}" )
                .ShouldBe( idUser );
            // ...and still not in the ZoneEmpty.
            u.Database.ExecuteReader( $"select ActorId from CK.tActorProfile where GroupId = {idZoneEmpty} and ActorId = {idUser}" )
                .Rows.ShouldBeEmpty();
        }
    }

    [Test]
    public void with_option_Intersect_when_a_group_is_moved_its_users_not_already_registered_in_the_target_zone_are_removed()
    {
        var map = SharedEngine.Map;
        var g = map.StObjs.Obtain<GroupTable>();
        var z = map.StObjs.Obtain<ZoneTable>();
        var u = map.StObjs.Obtain<UserTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int idUser = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
            int idGroup = g.CreateGroup( ctx, 1 );
            int idZoneEmpty = z.CreateZone( ctx, 1 );
            int idZoneOK = z.CreateZone( ctx, 1 );

            g.AddUser( ctx, 1, idGroup, idUser );
            z.AddUser( ctx, 1, idZoneOK, idUser );
            // This works since the user is in the zoneOK (Intersect does nothing).
            g.MoveGroup( ctx, 1, idGroup, idZoneOK, GroupMoveOption.Intersect );
            // User is in the Group and in the ZoneOK.
            u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idGroup} and ActorId = {idUser}" )
                .ShouldBe( idUser );
            u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idZoneOK} and ActorId = {idUser}" )
                .ShouldBe( idUser );

            // This does work... But the user is removed from the group
            // to preserve the 'Group.UserNotInZone' invariant.
            g.MoveGroup( ctx, 1, idGroup, idZoneEmpty, GroupMoveOption.Intersect );
            // User is no more in the Group: it has been removed.
            u.Database.ExecuteReader( $"select ActorId from CK.tActorProfile where GroupId = {idGroup} and ActorId = {idUser}" )
                .Rows.ShouldBeEmpty();
            // ...and still not in the ZoneEmpty.
            u.Database.ExecuteReader( $"select ActorId from CK.tActorProfile where GroupId = {idZoneEmpty} and ActorId = {idUser}" )
                .Rows.ShouldBeEmpty();
        }
    }

    [Test]
    public void with_option_AutoUserRegistration_when_a_group_is_moved_its_users_not_already_registered_in_the_target_zone_are_automatically_registered()
    {
        var map = SharedEngine.Map;
        var g = map.StObjs.Obtain<GroupTable>();
        var z = map.StObjs.Obtain<ZoneTable>();
        var u = map.StObjs.Obtain<UserTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int idUser = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
            int idGroup = g.CreateGroup( ctx, 1 );
            int idZoneEmpty = z.CreateZone( ctx, 1 );
            int idZoneOK = z.CreateZone( ctx, 1 );

            g.AddUser( ctx, 1, idGroup, idUser );
            z.AddUser( ctx, 1, idZoneOK, idUser );
            // This works since the user is in the zoneOK (Intersect does nothing).
            g.MoveGroup( ctx, 1, idGroup, idZoneOK, GroupMoveOption.AutoUserRegistration );
            // User is in the Group and in the ZoneOK.
            u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idGroup} and ActorId = {idUser}" )
                .ShouldBe( idUser );
            u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idZoneOK} and ActorId = {idUser}" )
                .ShouldBe( idUser );

            // This does work: and the user is automatically added to the target Zone!
            // (the 'Group.UserNotInZone' invariant is preserved).
            g.MoveGroup( ctx, 1, idGroup, idZoneEmpty, GroupMoveOption.AutoUserRegistration );
            // User is no more in the Group: it has been removed.
            u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idGroup} and ActorId = {idUser}" )
                .ShouldBe( idUser );
            // ...and still not in the ZoneEmpty.
            u.Database.ExecuteScalar( $"select ActorId from CK.tActorProfile where GroupId = {idZoneEmpty} and ActorId = {idUser}" )
                .ShouldBe( idUser );
        }
    }
}
