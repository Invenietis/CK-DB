using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CK.SqlServer;
using CK.Core;
using System.Data.SqlClient;

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
                g.Database.AssertScalarEquals( groupId, "select GroupId from CK.tGroup where GroupId = @0", groupId );

                g.DestroyGroup( ctx, 1, groupId );

                g.Database.AssertEmptyReader( "select * from CK.tGroup where GroupId = @0", groupId );
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
        public void groups_can_not_be_destroyed_when_users_exist_by_default()
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

                g.Database.AssertEmptyReader( "select * from CK.tUser where UserId = @0", userId )
                          .AssertEmptyReader( "select * from CK.tGroup where GroupId = @0", groupId );
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
                g.Database.AssertEmptyReader( "select * from CK.tUser where UserId = @0", userId )
                          .AssertEmptyReader( "select * from CK.tGroup where GroupId = @0", groupId );
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
                g.Database.AssertEmptyReader( "select * from CK.tActorProfile where ActorId = @0", userId )
                          .AssertEmptyReader( "select * from CK.tUser where UserId = @0", userId );

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
                g.Database.AssertScalarEquals( 1, "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId );
                Assert.DoesNotThrow( () => g.RemoveUser( ctx, 1, 1, userId ) );
                g.Database.AssertScalarEquals( 0, "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId );
                Assert.DoesNotThrow( () => g.AddUser( ctx, 1, 1, userId ) );
                g.Database.AssertScalarEquals( 1, "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId );
            }
            int userId2;
            int anotherUserId;
            using( var ctx = new SqlStandardCallContext() )
            {
                userId2 = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                Assert.DoesNotThrow( () => g.AddUser( ctx, 1, 1, userId2 ) );
                g.Database.AssertScalarEquals( 1, "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId2 );
                Assert.DoesNotThrow( () => g.RemoveUser( ctx, 1, 1, userId2 ) );
                g.Database.AssertScalarEquals( 0, "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId2 );
                anotherUserId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
            }
            // Using ActorId = userId2.
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => g.RemoveUser( ctx, userId2, 1, userId ) );
                g.Database.AssertScalarEquals( 1, "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", userId );

                Assert.Throws<SqlDetailedException>( () => g.AddUser( ctx, userId2, 1, anotherUserId ) );
                g.Database.AssertScalarEquals( 0, "select count(*) from CK.tActorProfile where ActorId = @0 and GroupId <> @0", anotherUserId );

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
