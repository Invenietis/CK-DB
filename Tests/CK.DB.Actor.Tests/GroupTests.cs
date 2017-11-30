using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CK.SqlServer;
using CK.Core;
using System.Data.SqlClient;
using FluentAssertions;

namespace CK.DB.Actor.Tests
{
    [TestFixture]
    public class GroupTests
    {
        [Test]
        public void groups_can_be_created_and_destroyed_when_empty()
        {
            var map = TestHelper.StObjMap;
            var g = map.Default.Obtain<GroupTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int groupId = g.CreateGroup( ctx, 1 );
                Assert.That( groupId, Is.GreaterThan( 1 ) );
                g.Database.ExecuteScalar( "select GroupId from CK.tGroup where GroupId = @0", groupId )
                    .Should().Be( groupId );

                g.DestroyGroup( ctx, 1, groupId );

                g.Database.ExecuteReader( "select * from CK.tGroup where GroupId = @0", groupId )
                    .Rows.Should().BeEmpty();
            }
        }

        [Test]
        public void Anonymous_can_not_create_a_group()
        {
            var map = TestHelper.StObjMap;
            var g = map.Default.Obtain<GroupTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => g.CreateGroup( ctx, 0 ) );
            }
        }

        [Test]
        public void Anonymous_can_not_destroy_a_group()
        {
            var map = TestHelper.StObjMap;
            var g = map.Default.Obtain<GroupTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int groupId = g.CreateGroup( ctx, 1 );
                Assert.Throws<SqlDetailedException>( () => g.DestroyGroup( ctx, 0, groupId ) );
                g.DestroyGroup( ctx, 1, groupId );
            }
        }

        [Test]
        public void by_default_groups_can_not_be_destroyed_when_users_exist()
        {
            var map = TestHelper.StObjMap;
            var g = map.Default.Obtain<GroupTable>();
            var u = map.Default.Obtain<UserTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                int groupId = g.CreateGroup( ctx, 1 );
                int userId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );

                g.AddUser( ctx, 1, groupId, userId );

                Assert.Throws<SqlDetailedException>( () => g.DestroyGroup( ctx, 1, groupId ) );

                u.DestroyUser( ctx, 1, userId );
                g.DestroyGroup( ctx, 1, groupId );

                g.Database.ExecuteReader( "select * from CK.tUser where UserId = @0", userId )
                    .Rows.Should().BeEmpty();
                g.Database.ExecuteReader( "select * from CK.tGroup where GroupId = @0", groupId )
                    .Rows.Should().BeEmpty();

            }
        }

        [Test]
        public void groups_are_destroyed_even_when_users_exist_when_ForceDestroy_is_true()
        {
            var map = TestHelper.StObjMap;
            var g = map.Default.Obtain<GroupTable>();
            var u = map.Default.Obtain<UserTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                int groupId = g.CreateGroup( ctx, 1 );
                int userId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );

                g.AddUser( ctx, 1, groupId, userId );

                Assert.DoesNotThrow( () => g.DestroyGroup( ctx, 1, groupId, true ) );

                u.DestroyUser( ctx, 1, userId );
                g.Database.ExecuteReader( "select * from CK.tUser where UserId = @0", userId )
                    .Rows.Should().BeEmpty();
                g.Database.ExecuteReader( "select * from CK.tGroup where GroupId = @0", groupId )
                    .Rows.Should().BeEmpty();
            }
        }

        [Test]
        public void destroying_a_user_removes_it_from_all_the_groups_it_belongs_to()
        {
            var map = TestHelper.StObjMap;
            var g = map.Default.Obtain<GroupTable>();
            var u = map.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int groupId1 = g.CreateGroup( ctx, 1 );
                int groupId2 = g.CreateGroup( ctx, 1 );
                int userId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );

                g.AddUser( ctx, 1, groupId1, userId );
                g.AddUser( ctx, 1, groupId2, userId );

                u.DestroyUser( ctx, 1, userId );
                g.Database.ExecuteReader( "select * from CK.tActorProfile where ActorId = @0", userId )
                    .Rows.Should().BeEmpty();
                g.Database.ExecuteReader( "select * from CK.tUser where UserId = @0", userId )
                    .Rows.Should().BeEmpty();


                g.DestroyGroup( ctx, 1, groupId1 );
                g.DestroyGroup( ctx, 1, groupId2 );
            }
        }

        [Test]
        public void only_system_users_can_add_or_remove_users_from_group_System()
        {
            var map = TestHelper.StObjMap;
            var g = map.Default.Obtain<GroupTable>();
            var u = map.Default.Obtain<UserTable>();
            int userId;
            using( var ctx = new SqlStandardCallContext() )
            {
                userId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                Assert.DoesNotThrow( () => g.AddUser( ctx, 1, 1, userId ) );
                g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId )
                    .Should().Be( 1 );
                g.RemoveUser( ctx, 1, 1, userId );
                g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId )
                    .Should().Be( 0 );
                g.AddUser( ctx, 1, 1, userId );
                g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId )
                    .Should().Be( 1 );
            }
            int userId2;
            int anotherUserId;
            using( var ctx = new SqlStandardCallContext() )
            {
                userId2 = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                g.AddUser( ctx, 1, 1, userId2 );
                g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId2 )
                    .Should().Be( 1 );
                g.RemoveUser( ctx, 1, 1, userId2 );
                g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId2 )
                    .Should().Be( 0 );
                anotherUserId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
            }
            // Using ActorId = userId2.
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => g.RemoveUser( ctx, userId2, 1, userId ) );
                g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId )
                    .Should().Be( 1 );

                Assert.Throws<SqlDetailedException>( () => g.AddUser( ctx, userId2, 1, anotherUserId ) );
                g.Database.ExecuteScalar( "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", anotherUserId )
                    .Should().Be( 0 );

                Assert.Throws<SqlDetailedException>( () => g.RemoveAllUsers( ctx, userId2, 1 ) );
            }

            using( var ctx = new SqlStandardCallContext() )
            {
                u.DestroyUser( ctx, 1, userId );
                u.DestroyUser( ctx, 1, userId2 );
                u.DestroyUser( ctx, 1, anotherUserId );
            }
        }

    }
}
