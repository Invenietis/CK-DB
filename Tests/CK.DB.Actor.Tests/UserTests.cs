using System;
using NUnit.Framework;
using CK.SqlServer;
using CK.Core;
using FluentAssertions;
using CK.Testing;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Actor.Tests;

[TestFixture]
public class UserTests
{

    [Test]
    public void Anonymous_can_not_create_a_user()
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            u.Invoking( sut => sut.CreateUser( ctx, 0, Guid.NewGuid().ToString() ) ).Should().Throw<SqlDetailedException>();
        }
    }

    [Test]
    public void user_FindByName_returns_0_when_not_found()
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            var exist = Guid.NewGuid().ToString();
            var notExist = Guid.NewGuid().ToString();

            Assert.That( u.FindByName( ctx, notExist ) == 0 );
            int userId = u.CreateUser( ctx, 1, exist );
            Assert.That( userId > 1 );
            Assert.That( u.FindByName( ctx, exist ) == userId );
        }
    }

    [Test]
    public void user_can_not_be_created_with_an_already_existing_UserName()
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserTable>();

        string testName = "user_can_not_be_created_with_an_already_existing_UserName" + Guid.NewGuid().ToString();

        using( var ctx = new SqlStandardCallContext() )
        {
            int id = u.CreateUser( ctx, 1, testName );
            Assert.That( id, Is.GreaterThan( 1 ) );

            int idRejected = u.CreateUser( ctx, 1, testName );
            Assert.That( idRejected, Is.EqualTo( -1 ) );

            u.DestroyUser( ctx, 1, id );

            u.Database.ExecuteReader( "select * from CK.tUser where UserName = @0", testName )
                .Rows.Should().BeEmpty();
        }
    }

    [Test]
    public void UserName_is_not_set_if_another_user_exists_with_the_same_UserName()
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserTable>();

        string existingName = Guid.NewGuid().ToString();
        string userName = Guid.NewGuid().ToString();

        using( var ctx = new SqlStandardCallContext() )
        {
            int idExist = u.CreateUser( ctx, 1, existingName );
            int idUser = u.CreateUser( ctx, 1, userName );

            u.UserNameSet( ctx, 1, idUser, existingName ).Should().BeFalse( "No rename on clash." );
            u.UserNameSet( ctx, 1, idExist, userName ).Should().BeFalse( "No rename on clash." );

            u.UserNameSet( ctx, 1, idUser, userName ).Should().BeTrue( "One can always rename to the current name." );
            u.UserNameSet( ctx, 1, idExist, existingName ).Should().BeTrue( "One can always rename to the current name." );

            u.DestroyUser( ctx, 1, idExist );
            u.DestroyUser( ctx, 1, idUser );

            u.Database.ExecuteReader( "select * from CK.tUser where UserName = @0 or UserName = @1", existingName, userName )
                .Rows.Should().BeEmpty();
        }
    }

}
