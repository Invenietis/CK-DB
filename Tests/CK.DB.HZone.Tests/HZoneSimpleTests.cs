using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using FluentAssertions;
using CK.Testing;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.HZone.Tests;

[TestFixture]
public class HZoneSimpleTests
{
    [TearDown]
    public void CheckCKCoreInvariant()
    {
        SharedEngine.Map.StObjs.Obtain<SqlDefaultDatabase>().GetCKCoreInvariantsViolations()
            .Rows.Should().BeEmpty();
    }

    [Test]
    public void adding_a_user_in_a_child_zone_support_AutoAddUserInParentZone()
    {
        var map = SharedEngine.Map;
        var zone = map.StObjs.Obtain<ZoneTable>();
        var group = map.StObjs.Obtain<Zone.GroupTable>();
        var user = map.StObjs.Obtain<UserTable>();

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

            zone.Invoking( sut => sut.AddUser( ctx, 1, allZones[3], idUser1, autoAddUserInParentZone: false ) ).Should().Throw<SqlDetailedException>();
            zone.AddUser( ctx, 1, allZones[3], idUser1, autoAddUserInParentZone: true );

            group.Invoking( sut => sut.AddUser( ctx, 1, idGroup, idUser2, autoAddUserInZone: false ) ).Should().Throw<SqlDetailedException>();
            group.AddUser( ctx, 1, idGroup, idUser2, autoAddUserInZone: true );

            user.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser1 )
                .Should().Be( 4 );
            user.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 )
                .Should().Be( 5 );

            zone.DestroyZone( ctx, 1, allZones[0], forceDestroy: true );
        }
    }

    [Test]
    public void removing_a_user_from_a_zone_removes_it_from_all_child_zones()
    {
        var map = SharedEngine.Map;
        var zone = map.StObjs.Obtain<ZoneTable>();
        var group = map.StObjs.Obtain<Zone.GroupTable>();
        var user = map.StObjs.Obtain<UserTable>();

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

            user.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser1 )
                .Should().Be( 4 );
            user.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 )
                .Should().Be( 5 );

            zone.RemoveUser( ctx, 1, allZones[2], idUser2 );
            user.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 )
                .Should().Be( 2 );
            group.RemoveUser( ctx, 1, allZones[1], idUser2 );
            user.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 )
                .Should().Be( 1 );
            group.RemoveUser( ctx, 1, allZones[0], idUser2 );
            user.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 )
                .Should().Be( 0 );

            zone.RemoveUser( ctx, 1, allZones[0], idUser1 );
            user.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId <> GroupId and ActorId = @0", idUser2 )
                .Should().Be( 0 );

            zone.DestroyZone( ctx, 1, allZones[0], forceDestroy: true );
        }
    }

    [Test]
    public void creating_and_destroying_zone_with_sub_zones_and_groups_when_ForceDestroy_is_true()
    {
        var map = SharedEngine.Map;
        var zone = map.StObjs.Obtain<ZoneTable>();
        var group = map.StObjs.Obtain<Zone.GroupTable>();
        var user = map.StObjs.Obtain<UserTable>();

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
            zone.Invoking( sut => sut.DestroyZone( ctx, 1, allZones[0], forceDestroy: false ) ).Should().Throw<SqlDetailedException>();
            zone.DestroyZone( ctx, 1, allZones[0], forceDestroy: true );
        }
    }

    [Test]
    public void moving_a_zone_in_the_tree_can_specify_the_next_sibling_id()
    {
        var map = SharedEngine.Map;
        var zone = map.StObjs.Obtain<ZoneTable>();

        using( var ctx = new SqlStandardCallContext() )
        {
            var z = new List<int>();
            for( int i = 0; i < 10; ++i ) z.Add( zone.CreateZone( ctx, 1 ) );
            zone.MoveZone( ctx, 1, z[1], z[0] );
            zone.MoveZone( ctx, 1, z[2], z[0] );
            zone.MoveZone( ctx, 1, z[3], z[0] );
            zone.MoveZone( ctx, 1, z[4], z[0] );
            zone.CheckTree( ctx, z[0], $@"
                                {z[0]}
                                +{z[1]}
                                +{z[2]}
                                +{z[3]}
                                +{z[4]}
                                " );
            zone.MoveZone( ctx, 1, z[6], z[5] );
            zone.MoveZone( ctx, 1, z[7], z[5], nextSiblingId: z[6] );
            zone.MoveZone( ctx, 1, z[8], z[5] );
            zone.MoveZone( ctx, 1, z[9], z[5], nextSiblingId: z[8] );
            zone.CheckTree( ctx, z[5], $@"
                                {z[5]}
                                +{z[7]}
                                +{z[6]}
                                +{z[9]}
                                +{z[8]}
                                " );
        }
    }

    [Test]
    public void GroupMove_can_safely_be_called_instead_of_ZoneMove()
    {
        var map = SharedEngine.Map;
        var zone = map.StObjs.Obtain<ZoneTable>();
        var group = map.StObjs.Obtain<Zone.GroupTable>();

        using( var ctx = new SqlStandardCallContext() )
        {
            var z = new List<int>();
            for( int i = 0; i < 5; ++i ) z.Add( zone.CreateZone( ctx, 1 ) );
            group.MoveGroup( ctx, 1, z[1], z[0] );
            group.MoveGroup( ctx, 1, z[4], z[0] );
            group.MoveGroup( ctx, 1, z[2], z[0] );
            group.MoveGroup( ctx, 1, z[3], z[0] );
            zone.CheckTree( ctx, z[0], $@"
                                {z[0]}
                                +{z[1]}
                                +{z[4]}
                                +{z[2]}
                                +{z[3]}
                                " );
            group.MoveGroup( ctx, 1, z[3], z[2] );
            group.MoveGroup( ctx, 1, z[4], z[3] );
            zone.CheckTree( ctx, z[0], $@"
                                {z[0]}
                                +{z[1]}
                                +{z[2]}
                                ++{z[3]}
                                +++{z[4]}
                                " );
        }
    }

    [Test]
    public void moving_a_zone_in_a_child_zone_is_an_error()
    {
        var map = SharedEngine.Map;
        var zone = map.StObjs.Obtain<ZoneTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int idZone1 = zone.CreateZone( ctx, 1 );
            int idZone2 = zone.CreateZone( ctx, 1, idZone1 );

            zone.Invoking( sut => sut.MoveZone( ctx, 1, idZone1, idZone2 ) ).Should().Throw<SqlDetailedException>();
        }
    }
}
