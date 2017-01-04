using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System.Linq;

namespace CK.DB.User.UserGoogle.Tests
{
    [TestFixture]
    public class UserGoogleTests
    {
        [Test]
        public void create_Google_user_and_check_read_info_object_method()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserGoogleTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, userName );
                var googleAccountId = Guid.NewGuid().ToString( "N" );

                bool created = u.CreateOrUpdateGoogleUser( ctx, 1, new UserGoogleInfo() { UserId = userId, GoogleAccountId = googleAccountId }, true );
                Assert.That( created, Is.True );
                var info2 = u.FindUserInfo( ctx, googleAccountId );

                Assert.That( info2.UserId, Is.EqualTo( userId ) );
                Assert.That( info2.GoogleAccountId, Is.EqualTo( googleAccountId ) );

                Assert.That( u.FindUserInfo( ctx, Guid.NewGuid().ToString() ), Is.Null );
                user.DestroyUser( ctx, 1, userId );
                Assert.That( u.FindUserInfo( ctx, googleAccountId ), Is.Null );
            }
        }

        [Test]
        public async Task create_Google_user_and_check_read_info_object_method_async()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserGoogleTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = await user.CreateUserAsync( ctx, 1, userName );
                var googleAccountId = Guid.NewGuid().ToString( "N" );

                Assert.That( await u.CreateOrUpdateGoogleUserAsync( ctx, 1, new UserGoogleInfo() { UserId = userId, GoogleAccountId = googleAccountId }, false ) );
                var info2 = await u.FindUserInfoAsync( ctx, googleAccountId );

                Assert.That( info2.UserId, Is.EqualTo( userId ) );
                Assert.That( info2.GoogleAccountId, Is.EqualTo( googleAccountId ) );

                Assert.That( await u.FindUserInfoAsync( ctx, Guid.NewGuid().ToString() ), Is.Null );
                await user.DestroyUserAsync( ctx, 1, userId );
                Assert.That( await u.FindUserInfoAsync( ctx, googleAccountId ), Is.Null );
            }
        }

        [Test]
        public void Google_AuthProvider_is_registered()
        {
            Auth.Tests.AuthTests.CheckProviderRegistration( "Google" );
        }

        [Test]
        public void vUserAuthProvider_reflects_the_user_Google_authentication()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserGoogleTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "Basic auth - " + Guid.NewGuid().ToString();
                var googleAccountId = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );
                u.Database.AssertEmptyReader( $"select * from CK.vUserAuthProvider where UserId={idU} and ProviderName='Google'" );
                u.CreateOrUpdateGoogleUser( ctx, 1, new UserGoogleInfo() { UserId = idU, GoogleAccountId = googleAccountId }, true );
                u.Database.AssertScalarEquals( 1, $"select count(*) from CK.vUserAuthProvider where UserId={idU} and ProviderName='Google'" );
                u.DestroyGoogleUser( ctx, 1, idU );
                u.Database.AssertEmptyReader( $"select * from CK.vUserAuthProvider where UserId={idU} and ProviderName='Google'" );
                // To let the use in the database with a Google authentication.
                u.CreateOrUpdateGoogleUser( ctx, 1, new UserGoogleInfo() { UserId = idU, GoogleAccountId = googleAccountId }, true );
            }
        }


        [Test]
        [Explicit]
        public async Task explicit_refresh_token()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            var userG = TestHelper.StObjMap.Default.Obtain<UserGoogleTable>();
            // This is the PrimarySchool Google application.
            p.ClientId = "368841447214-b0hhtth684efi54lfjhs03uk4an28dd9.apps.googleusercontent.com";
            p.ClientSecret = "GiApMZBp3RTxdNzsHbhAQKSG";
            string googleAccountId = "112981383157638924429";
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
            {
                var info = await userG.FindUserInfoAsync( ctx, googleAccountId );
                if( info == null )
                {
                    var userName = Guid.NewGuid().ToString();
                    int userId = await user.CreateUserAsync( ctx, 1, userName );
                    info = new UserGoogleInfo()
                    {
                        UserId = userId,
                        GoogleAccountId = googleAccountId,
                        RefreshToken = "1/t63rMARi7a9qQWIYEcKPVIrfnNJU51K2TpNB3hjrEjI",
                    };
                }
                info.AccessToken = null;
                Assert.That( await p.RefreshAccessTokenAsync( ctx, info ) );
                Assert.That( info.AccessToken, Is.Not.Null );
                Assert.That( info.AccessTokenExpirationTime, Is.GreaterThan( DateTime.UtcNow ) );
                Assert.That( info.AccessTokenExpirationTime, Is.LessThan( DateTime.UtcNow.AddDays( 1 ) ) );
                Assert.That( await p.RefreshAccessTokenAsync( ctx, info ) );
            }
        }

    }

}

