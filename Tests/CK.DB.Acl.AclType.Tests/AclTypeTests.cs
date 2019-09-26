using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.Acl.AclType.Tests
{
    [TestFixture]
    public class AclTypeTests
    {
        [Test]
        public async Task creating_and_destroying_type()
        {
            var map = TestHelper.StObjMap;
            var aclType = map.StObjs.Obtain<AclTypeTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var db = aclType.Database;
                int id = await aclType.CreateAclTypeAsync( ctx, 1 );

                db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
                                    .Should().Be( 2 );
                db.ExecuteReader( "select * from CK.tAclTypeGrantLevel where AclTypeId = @0 and GrantLevel not in (0, 127)", id )
                                    .Rows.Should().BeEmpty();

                await aclType.DestroyAclTypeAsync( ctx, 1, id );

                db.ExecuteReader( "select * from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
                                    .Rows.Should().BeEmpty();
            }
        }

        [Test]
        public async Task constrained_levels_must_not_be_deny_and_0_and_127_can_not_be_removed()
        {
            var map = TestHelper.StObjMap;
            var aclType = map.StObjs.Obtain<AclTypeTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var db = aclType.Database;
                int id = await aclType.CreateAclTypeAsync( ctx, 1 );
                await aclType.SetGrantLevelAsync( ctx, 1, id, 87, true );
                db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
                  .Should().Be( 3 );

                await aclType.SetGrantLevelAsync( ctx, 1, id, 88, true );
                db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
                  .Should().Be( 4 );

                // Removing an unexisting level is always possible...
                await aclType.SetGrantLevelAsync( ctx, 1, id, 126, false );
                await aclType.SetGrantLevelAsync( ctx, 1, id, 1, false );
                db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
                  .Should().Be( 4 );

                // ...except if it is 0 or 127.
                aclType.Awaiting( sut => sut.SetGrantLevelAsync( ctx, 1, id, 0, false ) ).Should().Throw<SqlDetailedException>();
                aclType.Awaiting( sut => sut.SetGrantLevelAsync( ctx, 1, id, 127, false ) ).Should().Throw<SqlDetailedException>();

                // Configured GrantLevel must not be deny level:
                aclType.Awaiting( sut => sut.SetGrantLevelAsync( ctx, 1, id, 128, true ) ).Should().Throw<SqlDetailedException>();
                aclType.Awaiting( sut => sut.SetGrantLevelAsync( ctx, 1, id, 255, true ) ).Should().Throw<SqlDetailedException>();
                aclType.Awaiting( sut => sut.SetGrantLevelAsync( ctx, 1, id, 255, false ) ).Should().Throw<SqlDetailedException>();

                await aclType.SetGrantLevelAsync( ctx, 1, id, 87, false );
                db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
                  .Should().Be( 3 );

                await aclType.SetGrantLevelAsync( ctx, 1, id, 88, false );
                db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
                  .Should().Be( 2 );

                await aclType.DestroyAclTypeAsync( ctx, 1, id );
            }
        }

        [Test]
        public async Task type_can_not_be_destroyed_when_typed_acl_exist()
        {
            var map = TestHelper.StObjMap;
            var acl = map.StObjs.Obtain<AclTable>();
            var aclType = map.StObjs.Obtain<AclTypeTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var db = aclType.Database;
                int idType = await aclType.CreateAclTypeAsync( ctx, 1 );
                int idAcl = await aclType.CreateAclAsync( ctx, 1, idType );
                db.ExecuteScalar( "select AclTypeId from CK.tAcl where AclId = @0", idAcl )
                  .Should().Be( idType );
                aclType.Awaiting( sut => sut.DestroyAclTypeAsync( ctx, 1, idType ) ).Should().Throw<SqlDetailedException>();
                acl.DestroyAcl( ctx, 1, idAcl );
                await aclType.DestroyAclTypeAsync( ctx, 1, idType );
            }
        }

        [Test]
        public void typed_acl_with_constrained_levels_control_their_grant_levels()
        {
            var map = TestHelper.StObjMap;
            var user = map.StObjs.Obtain<UserTable>();
            var acl = map.StObjs.Obtain<AclTable>();
            var aclType = map.StObjs.Obtain<AclTypeTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int idType = aclType.CreateAclType( ctx, 1 );
                int idAcl = aclType.CreateAcl( ctx, 1, idType );
                int idUser = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );

                // Sets the type as a Constrained one.
                aclType.SetConstrainedGrantLevel( ctx, 1, idType, true );
                // Allowing GrantLevel: 50
                acl.Invoking( sut => sut.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 50 ) ).Should().Throw<SqlDetailedException>();
                acl.Invoking( sut => sut.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 255 - 50 ) ).Should().Throw<SqlDetailedException>();
                aclType.SetGrantLevel( ctx, 1, idType, 50, true );
                acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 50 );
                acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 255 - 50 );

                // Allowing GrantLevel: 75
                acl.Invoking( sut => sut.AclGrantSet( ctx, 1, idAcl, idUser, null, 75 ) ).Should().Throw<SqlDetailedException>();
                acl.Invoking( sut => sut.AclGrantSet( ctx, 1, idAcl, idUser, null, 255 - 75 ) ).Should().Throw<SqlDetailedException>();
                aclType.SetGrantLevel( ctx, 1, idType, 75, true );
                acl.AclGrantSet( ctx, 1, idAcl, idUser, null, 255 - 75 );
                acl.AclGrantSet( ctx, 1, idAcl, idUser, null, 75 );

                // Since 75 and 50 are currently used, one can not remove it.
                aclType.Invoking( sut => sut.SetGrantLevel( ctx, 1, idType, 75, false ) ).Should().Throw<SqlDetailedException>();
                aclType.Invoking( sut => sut.SetGrantLevel( ctx, 1, idType, 50, false ) ).Should().Throw<SqlDetailedException>();

                // Removing the 50 configuration: we can now remove the level 50.
                acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 0 );
                aclType.SetGrantLevel( ctx, 1, idType, 50, false );
                // We can no more use the level 50.
                acl.Invoking( sut => sut.AclGrantSet( ctx, 1, idAcl, idUser, "Won't do it!", 50 ) ).Should().Throw<SqlDetailedException>();

                // Cleaning the Acl and the type.
                user.DestroyUser( ctx, 1, idUser );
                acl.DestroyAcl( ctx, 1, idAcl );
                aclType.DestroyAclType( ctx, 1, idType );
            }
        }

        [Test]
        public void existing_level_prevents_set_constrained()
        {
            var map = TestHelper.StObjMap;
            var aclType = map.StObjs.Obtain<AclTypeTable>();
            var acl = map.StObjs.Obtain<AclTable>();
            var user = map.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int idType = aclType.CreateAclType( ctx, 1 );
                int idAcl = aclType.CreateAcl( ctx, 1, idType );
                int idUser = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );

                acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 50 );
                // Sets the type as a Constrained one.
                aclType.Invoking( sut => sut.SetConstrainedGrantLevel( ctx, 1, idType, true ) ).Should().Throw<SqlDetailedException>();

                acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 0 );
                aclType.SetConstrainedGrantLevel( ctx, 1, idType, true );

                // Cleaning the Acl and the type.
                user.DestroyUser( ctx, 1, idUser );
                acl.DestroyAcl( ctx, 1, idAcl );
                aclType.DestroyAclType( ctx, 1, idType );
            }
        }
    }

}
