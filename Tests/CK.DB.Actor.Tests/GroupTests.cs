using System;
using System.Data;
using System.Diagnostics;
using NUnit.Framework;
using CK.SqlServer;
using CK.Core;
using Shouldly;
using Microsoft.Data.SqlClient;
using CK.Testing;

namespace CK.DB.Actor.Tests;

[TestFixture]
public class GroupTests
{
    [Test]
    public void groups_can_be_created_and_destroyed_when_empty()
    {
        var g = SharedEngine.Map.StObjs.Obtain<GroupTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int groupId = g.CreateGroup( ctx, 1 );
            Assert.That( groupId, Is.GreaterThan( 1 ) );
            g.Database.ExecuteScalar( "select GroupId from CK.tGroup where GroupId = @0", groupId )
                .ShouldBe( groupId );

            g.DestroyGroup( ctx, 1, groupId );

            g.Database.ExecuteReader( "select * from CK.tGroup where GroupId = @0", groupId )
                .Rows.ShouldBeEmpty();
        }
    }

    [Test]
    public void Anonymous_can_not_create_a_group()
    {
        var g = SharedEngine.Map.StObjs.Obtain<GroupTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            Util.Invokable( () => g.CreateGroup( ctx, 0 ) ).ShouldThrow<SqlDetailedException>();
        }
    }

    [Test]
    public void Anonymous_can_not_destroy_a_group()
    {
        var g = SharedEngine.Map.StObjs.Obtain<GroupTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int groupId = g.CreateGroup( ctx, 1 );
            Util.Invokable(() => g.DestroyGroup(ctx, 0, groupId)).ShouldThrow<SqlDetailedException>();
            g.DestroyGroup( ctx, 1, groupId );
        }
    }

    [Test]
    public void by_default_groups_can_not_be_destroyed_when_users_exist()
    {
        var map = SharedEngine.Map;
        var g = map.StObjs.Obtain<GroupTable>();
        var u = map.StObjs.Obtain<UserTable>();

        using( var ctx = new SqlStandardCallContext() )
        {
            int groupId = g.CreateGroup( ctx, 1 );
            int userId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );

            g.AddUser( ctx, 1, groupId, userId );

            Util.Invokable(() => g.DestroyGroup(ctx, 1, groupId)).ShouldThrow<SqlDetailedException>();

            u.DestroyUser( ctx, 1, userId );
            g.DestroyGroup( ctx, 1, groupId );

            g.Database.ExecuteReader( "select * from CK.tUser where UserId = @0", userId )
                .Rows.ShouldBeEmpty();
            g.Database.ExecuteReader( "select * from CK.tGroup where GroupId = @0", groupId )
                .Rows.ShouldBeEmpty();

        }
    }

    [Test]
    public void groups_are_destroyed_even_when_users_exist_when_ForceDestroy_is_true()
    {
        var map = SharedEngine.Map;
        var g = map.StObjs.Obtain<GroupTable>();
        var u = map.StObjs.Obtain<UserTable>();

        using( var ctx = new SqlStandardCallContext() )
        {
            int groupId = g.CreateGroup( ctx, 1 );
            int userId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );

            g.AddUser( ctx, 1, groupId, userId );

            Assert.DoesNotThrow( () => g.DestroyGroup( ctx, 1, groupId, true ) );

            u.DestroyUser( ctx, 1, userId );
            g.Database.ExecuteReader( "select * from CK.tUser where UserId = @0", userId )
                .Rows.ShouldBeEmpty();
            g.Database.ExecuteReader( "select * from CK.tGroup where GroupId = @0", groupId )
                .Rows.ShouldBeEmpty();
        }
    }

    [Test]
    public void destroying_a_user_removes_it_from_all_the_groups_it_belongs_to()
    {
        var map = SharedEngine.Map;
        var g = map.StObjs.Obtain<GroupTable>();
        var u = map.StObjs.Obtain<UserTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int groupId1 = g.CreateGroup( ctx, 1 );
            int groupId2 = g.CreateGroup( ctx, 1 );
            int userId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );

            g.AddUser( ctx, 1, groupId1, userId );
            g.AddUser( ctx, 1, groupId2, userId );

            u.DestroyUser( ctx, 1, userId );
            g.Database.ExecuteReader( "select * from CK.tActorProfile where ActorId = @0", userId )
                .Rows.ShouldBeEmpty();
            g.Database.ExecuteReader( "select * from CK.tUser where UserId = @0", userId )
                .Rows.ShouldBeEmpty();


            g.DestroyGroup( ctx, 1, groupId1 );
            g.DestroyGroup( ctx, 1, groupId2 );
        }
    }

    [Test]
    public void only_system_users_can_add_or_remove_users_from_group_System()
    {
        var map = SharedEngine.Map;
        var g = map.StObjs.Obtain<GroupTable>();
        var u = map.StObjs.Obtain<UserTable>();
        int userId;
        using( var ctx = new SqlStandardCallContext() )
        {
            userId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
            Assert.DoesNotThrow( () => g.AddUser( ctx, 1, 1, userId ) );
            g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId )
                .ShouldBe( 1 );
            g.RemoveUser( ctx, 1, 1, userId );
            g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId )
                .ShouldBe( 0 );
            g.AddUser( ctx, 1, 1, userId );
            g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId )
                .ShouldBe( 1 );
        }
        int userId2;
        int anotherUserId;
        using( var ctx = new SqlStandardCallContext() )
        {
            userId2 = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
            g.AddUser( ctx, 1, 1, userId2 );
            g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId2 )
                .ShouldBe( 1 );
            g.RemoveUser( ctx, 1, 1, userId2 );
            g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId2 )
                .ShouldBe( 0 );
            anotherUserId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
        }
        // Using ActorId = userId2.
        using( var ctx = new SqlStandardCallContext() )
        {
            Util.Invokable(() => g.RemoveUser(ctx, userId2, 1, userId)).ShouldThrow<SqlDetailedException>();
            g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId )
                .ShouldBe( 1 );

            Util.Invokable(() => g.AddUser(ctx, userId2, 1, anotherUserId)).ShouldThrow<SqlDetailedException>();
            g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", anotherUserId )
                .ShouldBe( 0 );

            Util.Invokable(() => g.RemoveAllUsers(ctx, userId2, 1)).ShouldThrow<SqlDetailedException>();
        }

        using( var ctx = new SqlStandardCallContext() )
        {
            u.DestroyUser( ctx, 1, userId );
            u.DestroyUser( ctx, 1, userId2 );
            u.DestroyUser( ctx, 1, anotherUserId );
        }
    }

    [Test]
    public void sGroupUserAdd_should_throw_when_adding_an_actor_that_is_not_a_user()
    {
        var groupTable = SharedEngine.Map.StObjs.Obtain<GroupTable>();
        var actorTable = SharedEngine.Map.StObjs.Obtain<ActorTable>();
        Debug.Assert( groupTable != null, nameof( groupTable ) + " != null" );
        Debug.Assert( actorTable != null, nameof( actorTable ) + " != null" );

        using( var context = new SqlStandardCallContext() )
        {
            var groupId = groupTable.CreateGroup( context, 1 );

            // Directly call the sActorCreate procedure: it is not exposed on the C# side
            // since there is no point to call it... except from tests.
            var cmd = new SqlCommand( "CK.sActorCreate" );
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add( "@ActorId", SqlDbType.Int ).Value = 1;
            cmd.Parameters.Add( "@ActorIdResult", SqlDbType.Int ).Direction = ParameterDirection.Output;
            actorTable.Database.ExecuteNonQuery( cmd );
            var actorIdResult = Convert.ToInt32( cmd.Parameters["@ActorIdResult"].Value );

            Util.Invokable( () => groupTable.AddUser( context, 1, groupId, actorIdResult ) )
                      .ShouldThrow<SqlDetailedException>()
                      .InnerException.ShouldBeOfType<SqlException>()
                      .Message.ShouldBe( "User.NotAUser" );
        }
    }

}
