using System;
using NUnit.Framework;
using CK.SqlServer;
using CK.Core;
using FluentAssertions;
using static CK.Testing.MonitorTestHelper;
using System.Threading.Tasks;

namespace CK.DB.User.NamedUser.Tests
{
    [TestFixture]
    public class NamedUserTests
    {
        [Test]
        public async Task can_create_named_user_Async()
        {
            var u = SharedEngine.Map.StObjs.Obtain<NamedUserTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                if( u is not null )
                {
                    var userName = Guid.NewGuid().ToString();
                    var firstName = Guid.NewGuid().ToString();
                    var lastName = Guid.NewGuid().ToString();
                    int userId = await u.CreateUserAsync( ctx, 1, userName, lastName, firstName );
                    Assert.That( userId, Is.GreaterThan( 1 ) );

                    u.Database.ExecuteScalar( "select FirstName from CK.vUser where UserId = @0", userId ).Should().Be( firstName );
                    u.Database.ExecuteScalar( "select LastName from CK.vUser where UserId = @0", userId ).Should().Be( lastName );
                }
            }
        }

        [Test]
        public async Task can_set_names_Async()
        {
            var u = SharedEngine.Map.StObjs.Obtain<NamedUserTable>();

            using( var ctx = new SqlStandardCallContext() )
            {
                if( u is not null )
                {
                    var userName = Guid.NewGuid().ToString();
                    var firstName = Guid.NewGuid().ToString();
                    var lastName = Guid.NewGuid().ToString();
                    int userId = await u.CreateUserAsync( ctx, 1, userName, lastName, firstName );

                    u.Database.ExecuteScalar( "select FirstName from CK.vUser where UserId = @0", userId ).Should().Be( firstName );
                    u.Database.ExecuteScalar( "select LastName from CK.vUser where UserId = @0", userId ).Should().Be( lastName );

                    await u.SetNamesAsync( ctx, 1, userId, "Mikado", "Domika" );

                    u.Database.ExecuteScalar( "select FirstName from CK.vUser where UserId = @0", userId ).Should().Be( "Mikado" );
                    u.Database.ExecuteScalar( "select LastName from CK.vUser where UserId = @0", userId ).Should().Be( "Domika" );

                    await u.SetNamesAsync( ctx, 1, userId, null, "Einstein" );

                    u.Database.ExecuteScalar( "select FirstName from CK.vUser where UserId = @0", userId ).Should().Be( "Mikado" );
                    u.Database.ExecuteScalar( "select LastName from CK.vUser where UserId = @0", userId ).Should().Be( "Einstein" );

                    await u.SetNamesAsync( ctx, 1, userId, "Albert", null );

                    u.Database.ExecuteScalar( "select FirstName from CK.vUser where UserId = @0", userId ).Should().Be( "Albert" );
                    u.Database.ExecuteScalar( "select LastName from CK.vUser where UserId = @0", userId ).Should().Be( "Einstein" );

                    await u.SetNamesAsync( ctx, 1, userId, null, null );

                    u.Database.ExecuteScalar( "select FirstName from CK.vUser where UserId = @0", userId ).Should().Be( "Albert" );
                    u.Database.ExecuteScalar( "select LastName from CK.vUser where UserId = @0", userId ).Should().Be( "Einstein" );

                }
            }
        }
    }
}
