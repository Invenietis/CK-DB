using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Acl.Tests
{
    [TestFixture]
    public class AclSimpleTests
    {

        [Test]
        public void god_user_can_create_and_destroy_acls()
        {
            var map = SharedEngine.Map;
            var acl = map.StObjs.Obtain<AclTable>();
            var user = map.StObjs.Obtain<UserTable>();
            var group = map.StObjs.Obtain<GroupTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                var db = acl.Database;
                int idGod = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                group.AddUser( ctx, 1, 1, idGod );
                int idAcl = acl.CreateAcl( ctx, idGod );

                Assert.That( idAcl >= 8, "Acl 0 to 7 are system-defined acls." );
                db.ExecuteScalar( "select AclId from CK.tAcl where AclId = @0", idAcl )
                  .Should().Be( idAcl );

                acl.GetGrantLevel( ctx, 1, idAcl ).Should().Be( 127, "System user is administrator on any Acls." );
                acl.GetGrantLevel( ctx, idGod, idAcl ).Should().Be( 127, "Members of System Group are administrators on any Acls." );
                acl.GetGrantLevel( ctx, 0, idAcl ).Should().Be( 0, "Anonymous are Blind by default." );

                acl.DestroyAcl( ctx, idGod, idAcl );
                db.ExecuteReader( "select AclId from CK.tAcl where AclId = @0", idAcl )
                  .Rows.Should().BeEmpty();
                user.DestroyUser( ctx, 1, idGod );
            }
        }

        [Test]
        public void system_default_acls_from_0_to_8_cannot_be_destroyed()
        {
            var acl = SharedEngine.Map.StObjs.Obtain<AclTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                for( int idAcl = 0; idAcl <= 8; ++idAcl )
                {
                    acl.Invoking( _ => _.DestroyAcl( ctx, 1, idAcl ) )
                        .Should().Throw<Exception>()
                        .WithInnerException<Exception>()
                        .WithMessage( "Security.ReservedAclId" );
                }
            }
        }

        [TestCase( 0, 127, "CK.StdAcl.Public" )]
        [TestCase( 1, 0, null )]
        [TestCase( 2, 8, "CK.StdAcl.User" )]
        [TestCase( 3, 16, "CK.StdAcl.Viewer" )]
        [TestCase( 4, 32, "CK.StdAcl.Contributor" )]
        [TestCase( 5, 64, "CK.StdAcl.Editor" )]
        [TestCase( 6, 80, "CK.StdAcl.SuperEditor" )]
        [TestCase( 7, 112, "CK.StdAcl.SafeAdministrator" )]
        [TestCase( 8, 0, null )]
        public void challenging_system_default_acls_except_the_1_by_a_random_user( int idAcl, byte grantLevel, string keyReasonForAnonymous )
        {
            var map = SharedEngine.Map;
            var acl = map.StObjs.Obtain<AclTable>();
            var user = map.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int idUser = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                acl.GetGrantLevel( ctx, 0, idAcl ).Should().Be( grantLevel, "Reason: " + keyReasonForAnonymous );
                acl.GetGrantLevel( ctx, idUser, idAcl ).Should().Be( grantLevel, "Reason: " + keyReasonForAnonymous );
                acl.Database.ExecuteScalar( "select KeyReason from CK.tAclConfigMemory where ActorId = 0 and AclId=@0", idAcl )
                    .Should().Be( keyReasonForAnonymous );
            }
        }

        [Test]
        public void the_System_Acl_1_is_the_only_one_that_can_be_configured()
        {
            var map = SharedEngine.Map;
            var acl = map.StObjs.Obtain<AclTable>();
            var user = map.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int idUser = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                acl.AclGrantSet( ctx, 1, aclId: 1, actorIdToGrant: 0, "For Test", 42 ); ;
                acl.AclGrantSet( ctx, 1, aclId: 1, actorIdToGrant: idUser, "For Test", 2*42 );

                // Just challenge the registration here: the actual configuration may differ on a specialized deployment (if Anonymous is configured).
                acl.Database.ExecuteScalar( "select KeyReason from CK.tAclConfigMemory where ActorId = 0 and AclId=1" )
                    .Should().Be( "For Test" );
                acl.Database.ExecuteScalar( "select KeyReason from CK.tAclConfigMemory where ActorId = @0 and AclId=1", idUser )
                    .Should().Be( "For Test" );

                // Cleanup.
                acl.AclGrantSet( ctx, 1, aclId: 1, actorIdToGrant: 0, "For Test", 0 );
                acl.AclGrantSet( ctx, 1, aclId: 1, actorIdToGrant: idUser, "For Test", 0 );

                acl.Invoking( _ => _.AclGrantSet( ctx, 1, aclId: 0, idUser, "For Test", 42 ) ).Should().Throw<Exception>().WithInnerException<Exception>().WithMessage( "Security.ImmutableAclId" );
                acl.Invoking( _ => _.AclGrantSet( ctx, 1, aclId: 2, idUser, "For Test", 42 ) ).Should().Throw<Exception>().WithInnerException<Exception>().WithMessage( "Security.ImmutableAclId" );
                acl.Invoking( _ => _.AclGrantSet( ctx, 1, aclId: 3, idUser, "For Test", 42 ) ).Should().Throw<Exception>().WithInnerException<Exception>().WithMessage( "Security.ImmutableAclId" );
                acl.Invoking( _ => _.AclGrantSet( ctx, 1, aclId: 4, idUser, "For Test", 42 ) ).Should().Throw<Exception>().WithInnerException<Exception>().WithMessage( "Security.ImmutableAclId" );
                acl.Invoking( _ => _.AclGrantSet( ctx, 1, aclId: 5, idUser, "For Test", 42 ) ).Should().Throw<Exception>().WithInnerException<Exception>().WithMessage( "Security.ImmutableAclId" );
                acl.Invoking( _ => _.AclGrantSet( ctx, 1, aclId: 6, idUser, "For Test", 42 ) ).Should().Throw<Exception>().WithInnerException<Exception>().WithMessage( "Security.ImmutableAclId" );
                acl.Invoking( _ => _.AclGrantSet( ctx, 1, aclId: 7, idUser, "For Test", 42 ) ).Should().Throw<Exception>().WithInnerException<Exception>().WithMessage( "Security.ImmutableAclId" );
                acl.Invoking( _ => _.AclGrantSet( ctx, 1, aclId: 8, idUser, "For Test", 42 ) ).Should().Throw<Exception>().WithInnerException<Exception>().WithMessage( "Security.ImmutableAclId" );
            }
        }

        [Test]
        public void playing_with_a_user_in_two_groups()
        {
            var map = SharedEngine.Map;
            var acl = map.StObjs.Obtain<AclTable>();
            var user = map.StObjs.Obtain<UserTable>();
            var group = map.StObjs.Obtain<GroupTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                int idAcl = acl.CreateAcl( ctx, 1 );
                int idUser = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                int idGroupRight = group.CreateGroup( ctx, 1 );
                int idGroupEditor = group.CreateGroup( ctx, 1 );

                group.AddUser( ctx, 1, idGroupRight, idUser );
                group.AddUser( ctx, 1, idGroupEditor, idUser );

                Assert.That( acl.GetGrantLevel( ctx, idGroupRight, idAcl ), Is.EqualTo( 0 ), "Acl is not configured: Blind for anyone..." );
                Assert.That( acl.GetGrantLevel( ctx, idGroupEditor, idAcl ), Is.EqualTo( 0 ), "Acl is not configured: Blind for anyone..." );
                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 0 ), "Acl is not configured: Blind for anyone..." );

                acl.AclGrantSet( ctx, 1, idAcl, idGroupRight, null, 16 );
                Assert.That( acl.GetGrantLevel( ctx, idGroupRight, idAcl ), Is.EqualTo( 16 ), "GrantLevel can be retrieved for a group." );
                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 16 ), "User is now Viewer since Right is granted Viewer." );

                acl.AclGrantSet( ctx, 1, idAcl, idGroupEditor, null, 64 );
                Assert.That( acl.GetGrantLevel( ctx, idGroupEditor, idAcl ), Is.EqualTo( 64 ), "GroupEditor has its GrantLevel." );
                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 64 ), "User is now Editor." );

                acl.AclGrantSet( ctx, 1, idAcl, idGroupRight, null, 80 );
                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 80 ), "User is now SuperEditor since Right has been boosted." );

                acl.AclGrantSet( ctx, 1, idAcl, idGroupRight, null, 0 );
                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 64 ), "Right has been removed: the user is now back to Editor." );

                acl.AclGrantSet( ctx, 1, idAcl, idGroupRight, null, 255 - 16 );
                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 16 ), "Right now gives Viewer level and NO MORE!" );

                acl.AclGrantSet( ctx, 1, idAcl, idGroupRight, null, 255 - 0 );
                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 0 ), "Right is Blind!" );

                acl.AclGrantSet( ctx, 1, idAcl, idGroupRight, null, 255 - 112 );
                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 112 ), "Right is SafeAdministrator, but not more." );

                acl.AclGrantSet( ctx, 1, idAcl, idGroupEditor, null, 127 );
                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 112 ), "Right is SafeAdministrator, but not more." );

                acl.DestroyAcl( ctx, 1, idAcl );
                acl.Database.ExecuteReader( "select AclId from CK.tAcl where AclId = @0", idAcl )
                    .Rows.Should().BeEmpty();
            }
        }


        [Test]
        public void destroying_actors_suppress_any_related_configurations()
        {
            var map = SharedEngine.Map;
            var acl = map.StObjs.Obtain<AclTable>();
            var user = map.StObjs.Obtain<UserTable>();
            var group = map.StObjs.Obtain<GroupTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                var db = acl.Database;
                int idAcl = acl.CreateAcl( ctx, 1 );
                int idUser = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                int idGroup = group.CreateGroup( ctx, 1 );

                group.AddUser( ctx, 1, idGroup, idUser );
                acl.AclGrantSet( ctx, 1, idAcl, idUser, null, 92 );
                acl.AclGrantSet( ctx, 1, idAcl, idGroup, null, 64 );

                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 92 ) );
                Assert.That( acl.GetGrantLevel( ctx, idGroup, idAcl ), Is.EqualTo( 64 ) );

                acl.AclGrantSet( ctx, 1, idAcl, idGroup, null, 127 );
                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 127 ) );

                db.ExecuteScalar( "select count(*) from CK.tAclConfig where AclId = @0 and ActorId=@1", idAcl, idGroup )
                  .Should().Be( 1 );
                group.DestroyGroup( ctx, 1, idGroup, true );
                db.ExecuteReader( "select * from CK.tAclConfig where AclId = @0 and ActorId=@1", idAcl, idGroup )
                  .Rows.Should().BeEmpty();

                Assert.That( acl.GetGrantLevel( ctx, idUser, idAcl ), Is.EqualTo( 92 ) );

                db.ExecuteScalar( "select count(*) from CK.tAclConfig where AclId = @0 and ActorId=@1", idAcl, idUser )
                  .Should().Be( 1 );
                user.DestroyUser( ctx, 1, idUser );
                db.ExecuteReader( "select * from CK.tAclConfig where AclId = @0 and ActorId=@1", idAcl, idUser )
                  .Rows.Should().BeEmpty();

                acl.DestroyAcl( ctx, 1, idAcl );
                db.ExecuteReader( "select AclId from CK.tAcl where AclId = @0", idAcl )
                  .Rows.Should().BeEmpty();

            }
        }

    }
}
