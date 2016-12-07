using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System.Linq;
using CK.DB.Auth;

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

                var info = u.CreateOrUpdateGoogleUser( ctx, 1, new UserGoogleInfo() { UserId = userId, GoogleAccountId = googleAccountId } );
                var info2 = u.FindUserInfo( ctx, googleAccountId );

                Assert.That( info2.UserId, Is.EqualTo( userId ) );
                Assert.That( info2.GoogleAccountId, Is.EqualTo( googleAccountId ) );
                Assert.That( info2.ScopeSetId, Is.GreaterThan( 1 ) );

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

                Assert.That( await u.CreateOrUpdateGoogleUserAsync( ctx, 1, new UserGoogleInfo() { UserId = userId, GoogleAccountId = googleAccountId } ) );
                var info2 = await u.FindUserInfoAsync( ctx, googleAccountId );

                Assert.That( info2.UserId, Is.EqualTo( userId ) );
                Assert.That( info2.GoogleAccountId, Is.EqualTo( googleAccountId ) );
                Assert.That( info2.ScopeSetId, Is.GreaterThan( 1 ) );

                Assert.That( await u.FindUserInfoAsync( ctx, Guid.NewGuid().ToString() ), Is.Null );
                await user.DestroyUserAsync( ctx, 1, userId );
                Assert.That( await u.FindUserInfoAsync( ctx, googleAccountId ), Is.Null );
            }
        }

        [Test]
        public async Task setting_default_scopes_impact_new_users()
        {
            var user = TestHelper.StObjMap.Default.Obtain<Actor.UserTable>();
            var auth = TestHelper.StObjMap.Default.Obtain<Auth.AuthScopeSetTable>();
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var defaultId = await p.GetDefaultScopeSetIdAsync( ctx );
                AuthScopeSet original = await auth.ReadAuthScopeSetAsync( ctx, defaultId );
                Assert.That( !original.Contains( "nimp" ) && !original.Contains( "thing" ) && !original.Contains( "other" ) );

                {
                    int id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                    UserGoogleInfo userInfo = new UserGoogleInfo() { UserId = id, GoogleAccountId = Guid.NewGuid().ToString() };
                    await p.UserGoogleTable.CreateOrUpdateGoogleUserAsync( ctx, 1, userInfo );
                    userInfo = await p.UserGoogleTable.FindUserInfoAsync( ctx, userInfo.GoogleAccountId );
                    AuthScopeSet userSet = await auth.ReadAuthScopeSetAsync( ctx, userInfo.ScopeSetId );
                    Assert.That( userSet.ToString(), Is.EqualTo( original.ToString() ) );
                }
                AuthScopeSet replaced = original.Clone();
                replaced.Add( new AuthScope( "nimp" ) );
                replaced.Add( new AuthScope( "thing", ScopeWARStatus.Rejected ) );
                replaced.Add( new AuthScope( "other", ScopeWARStatus.Accepted ) );
                await auth.SetScopesAsync( ctx, 1, defaultId, replaced.Scopes );
                var readback = await auth.ReadAuthScopeSetAsync( ctx, defaultId );
                Assert.That( readback.ToString(), Is.EqualTo( replaced.ToString() ) );
                // Default scopes have non W status!
                // This must not impact new users: their satus will be W.
                Assert.That( readback.ToString(), Does.Contain( "[R]thing" ) );
                Assert.That( readback.ToString(), Does.Contain( "[A]other" ) );

                {
                    int id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                    UserGoogleInfo userInfo = new UserGoogleInfo() { UserId = id, GoogleAccountId = Guid.NewGuid().ToString() };
                    await p.UserGoogleTable.CreateOrUpdateGoogleUserAsync( ctx, 1, userInfo );
                    userInfo = await p.UserGoogleTable.FindUserInfoAsync( ctx, userInfo.GoogleAccountId );
                    AuthScopeSet userSet = await auth.ReadAuthScopeSetAsync( ctx, userInfo.ScopeSetId );
                    Assert.That( userSet.ToString(), Does.Contain( "[W]thing" ) );
                    Assert.That( userSet.ToString(), Does.Contain( "[W]other" ) );
                    Assert.That( userSet.ToString(), Does.Contain( "[W]nimp" ) );
                }

                await auth.SetScopesAsync( ctx, 1, defaultId, original.Scopes );
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

