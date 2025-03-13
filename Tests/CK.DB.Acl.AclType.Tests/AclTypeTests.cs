using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using CK.Testing;
using Shouldly;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CK.DB.Acl.AclType.Tests;

[TestFixture]
public class AclTypeTests
{
    [Test]
    public async Task creating_and_destroying_type_Async()
    {
        var map = SharedEngine.Map;
        var aclType = map.StObjs.Obtain<AclTypeTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            var db = aclType.Database;
            int id = await aclType.CreateAclTypeAsync( ctx, 1 );

            db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
                                .ShouldBe( 2 );
            db.ExecuteReader( "select * from CK.tAclTypeGrantLevel where AclTypeId = @0 and GrantLevel not in (0, 127)", id )
                                .Rows.ShouldBeEmpty();

            await aclType.DestroyAclTypeAsync( ctx, 1, id );

            db.ExecuteReader( "select * from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
                                .Rows.ShouldBeEmpty();
        }
    }

    [Test]
    public async Task constrained_levels_must_not_be_deny_and_0_and_127_can_not_be_removed_Async()
    {
        var map = SharedEngine.Map;
        var aclType = map.StObjs.Obtain<AclTypeTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            var db = aclType.Database;
            int id = await aclType.CreateAclTypeAsync( ctx, 1 );
            await aclType.SetGrantLevelAsync( ctx, 1, id, 87, true );
            db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
              .ShouldBe( 3 );

            await aclType.SetGrantLevelAsync( ctx, 1, id, 88, true );
            db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
              .ShouldBe( 4 );

            // Removing an unexisting level is always possible...
            await aclType.SetGrantLevelAsync( ctx, 1, id, 126, false );
            await aclType.SetGrantLevelAsync( ctx, 1, id, 1, false );
            db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
              .ShouldBe( 4 );

            // ...except if it is 0 or 127.
            await Util.Invokable(() => aclType.SetGrantLevelAsync(ctx, 1, id, 0, false)).ShouldThrowAsync<SqlDetailedException>();
            await Util.Invokable(() => aclType.SetGrantLevelAsync(ctx, 1, id, 127, false)).ShouldThrowAsync<SqlDetailedException>();

            // Configured GrantLevel must not be deny level:
            await Util.Invokable(() => aclType.SetGrantLevelAsync(ctx, 1, id, 128, true)).ShouldThrowAsync<SqlDetailedException>();
            await Util.Invokable(() => aclType.SetGrantLevelAsync(ctx, 1, id, 255, true)).ShouldThrowAsync<SqlDetailedException>();
            await Util.Invokable(() => aclType.SetGrantLevelAsync(ctx, 1, id, 255, false)).ShouldThrowAsync<SqlDetailedException>();

            await aclType.SetGrantLevelAsync( ctx, 1, id, 87, false );
            db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
              .ShouldBe( 3 );

            await aclType.SetGrantLevelAsync( ctx, 1, id, 88, false );
            db.ExecuteScalar( "select count(*) from CK.tAclTypeGrantLevel where AclTypeId = @0", id )
              .ShouldBe( 2 );

            await aclType.DestroyAclTypeAsync( ctx, 1, id );
        }
    }

    [Test]
    public async Task type_can_not_be_destroyed_when_typed_acl_exist_Async()
    {
        var map = SharedEngine.Map;
        var acl = map.StObjs.Obtain<AclTable>();
        var aclType = map.StObjs.Obtain<AclTypeTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            var db = aclType.Database;
            int idType = await aclType.CreateAclTypeAsync( ctx, 1 );
            int idAcl = await aclType.CreateAclAsync( ctx, 1, idType );
            db.ExecuteScalar( "select AclTypeId from CK.tAcl where AclId = @0", idAcl )
              .ShouldBe( idType );
            await Util.Awaitable(() => aclType.DestroyAclTypeAsync(ctx, 1, idType)).ShouldThrowAsync<SqlDetailedException>();
            await acl.DestroyAclAsync( ctx, 1, idAcl );
            await aclType.DestroyAclTypeAsync( ctx, 1, idType );
        }
    }

    [Test]
    public void typed_acl_with_constrained_levels_control_their_grant_levels()
    {
        var map = SharedEngine.Map;
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
            Util.Invokable( () => acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 50 ) ).ShouldThrow<SqlDetailedException>();
            Util.Invokable( () => acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 255 - 50 ) ).ShouldThrow<SqlDetailedException>();
            aclType.SetGrantLevel( ctx, 1, idType, 50, true );
            acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 50 );
            acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 255 - 50 );

            // Allowing GrantLevel: 75
            Util.Invokable( () => acl.AclGrantSet( ctx, 1, idAcl, idUser, null!, 75 ) ).ShouldThrow<SqlDetailedException>();
            Util.Invokable( () => acl.AclGrantSet( ctx, 1, idAcl, idUser, null!, 255 - 75 ) ).ShouldThrow<SqlDetailedException>();
            aclType.SetGrantLevel( ctx, 1, idType, 75, true );
            acl.AclGrantSet( ctx, 1, idAcl, idUser, null!, 255 - 75 );
            acl.AclGrantSet( ctx, 1, idAcl, idUser, null!, 75 );

            // Since 75 and 50 are currently used, one can not remove it.
            Util.Invokable(() => aclType.SetGrantLevel(ctx, 1, idType, 75, false)).ShouldThrow<SqlDetailedException>();
            Util.Invokable(() => aclType.SetGrantLevel(ctx, 1, idType, 50, false)).ShouldThrow<SqlDetailedException>();

            // Removing the 50 configuration: we can now remove the level 50.
            acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 0 );
            aclType.SetGrantLevel( ctx, 1, idType, 50, false );
            // We can no more use the level 50.
            Util.Invokable(() => acl.AclGrantSet(ctx, 1, idAcl, idUser, "Won't do it!", 50)).ShouldThrow<SqlDetailedException>();

            // Cleaning the Acl and the type.
            user.DestroyUser( ctx, 1, idUser );
            acl.DestroyAcl( ctx, 1, idAcl );
            aclType.DestroyAclType( ctx, 1, idType );
        }
    }

    [Test]
    public void existing_level_prevents_set_constrained()
    {
        var map = SharedEngine.Map;
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
            Util.Invokable(() => aclType.SetConstrainedGrantLevel(ctx, 1, idType, true)).ShouldThrow<SqlDetailedException>();

            acl.AclGrantSet( ctx, 1, idAcl, idUser, "A reason", 0 );
            aclType.SetConstrainedGrantLevel( ctx, 1, idType, true );

            // Cleaning the Acl and the type.
            user.DestroyUser( ctx, 1, idUser );
            acl.DestroyAcl( ctx, 1, idAcl );
            aclType.DestroyAclType( ctx, 1, idType );
        }
    }
}
