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

                var info = u.CreateOrUpdateGoogleUser( ctx, 1, new UserGoogleInfo() { UserId = userId, GoogleAccountId = googleAccountId, Scopes = "openid" } );
                var info2 = u.FindUserInfo( ctx, googleAccountId );

                Assert.That( info2.UserId, Is.EqualTo( userId ) );
                Assert.That( info2.GoogleAccountId, Is.EqualTo( googleAccountId ) );
                Assert.That( info2.Scopes.ToString(), Is.EqualTo( "openid" ) );

                Assert.That( u.FindUserInfo( ctx, Guid.NewGuid().ToString() ), Is.Null );
                user.DestroyUser( ctx, 1, userId );
                Assert.That( u.FindUserInfo( ctx, googleAccountId ), Is.Null );
            }
        }

        [Test]
        public async void create_Google_user_and_check_read_info_object_method_async()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserGoogleTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = await user.CreateUserAsync( ctx, 1, userName );
                var googleAccountId = Guid.NewGuid().ToString( "N" );

                Assert.That( await u.CreateOrUpdateGoogleUserAsync( ctx, 1, new UserGoogleInfo() { UserId = userId, GoogleAccountId = googleAccountId, Scopes = "openid" } ) );
                var info2 = await u.FindUserInfoAsync( ctx, googleAccountId );

                Assert.That( info2.UserId, Is.EqualTo( userId ) );
                Assert.That( info2.GoogleAccountId, Is.EqualTo( googleAccountId ) );
                Assert.That( info2.Scopes.ToString(), Is.EqualTo( "openid" ) );

                Assert.That( await u.FindUserInfoAsync( ctx, Guid.NewGuid().ToString() ), Is.Null );
                await user.DestroyUserAsync( ctx, 1, userId );
                Assert.That( await u.FindUserInfoAsync( ctx, googleAccountId ), Is.Null );
            }
        }

        [Test]
        public async void gets_and_sets_default_scopes()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var original = await p.GetDefaultScopesAsync( ctx );
                Assert.That( !original.HasAllScopes( "nimp" ) && !original.HasAllScopes( "thing" ) && !original.HasAllScopes( "other" ) );
                var replaced = original;
                replaced.AddScopes( "nimp thing", "other" );
                await p.SetDefaultScopesAsync( ctx, replaced );
                var readback = await p.GetDefaultScopesAsync( ctx );
                Assert.That( readback.HasAllScopes( "nimp", "other thing" ) );
                await p.SetDefaultScopesAsync( ctx, original );
            }
        }

        [Test]
        [Explicit]
        public async void explicit_refresh_token()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            var userG = TestHelper.StObjMap.Default.Obtain<UserGoogleTable>();
            // This is the PrimarySchool Google application.
            p.ClientId = "368841447214-b0hhtth684efi54lfjhs03uk4an28dd9.apps.googleusercontent.com";
            p.ClientSecret = "GiApMZBp3RTxdNzsHbhAQKSG";
            string googleAccountId = "112981383157638924429";
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
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
                        Scopes = "openid email profile"
                    };
                }
                info.AccessToken = null;
                Assert.That( await p.RefreshAccessTokenAsync( ctx, TestHelper.Monitor, info ) );
                Assert.That( info.AccessToken, Is.Not.Null );
                Assert.That( info.AccessTokenExpirationTime, Is.GreaterThan( DateTime.UtcNow ) );
                Assert.That( info.AccessTokenExpirationTime, Is.LessThan( DateTime.UtcNow.AddDays( 1 ) ) );
                Assert.That( await p.RefreshAccessTokenAsync( ctx, TestHelper.Monitor, info ) );
            }
        }

    }

}

