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
            var infoFactory = TestHelper.StObjMap.Default.Obtain<IPocoFactory<IUserGoogleInfo>>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, userName );
                var googleAccountId = Guid.NewGuid().ToString( "N" );

                var info = infoFactory.Create();
                info.GoogleAccountId = googleAccountId;
                var created = u.CreateOrUpdateGoogleUser( ctx, 1, userId, info );
                Assert.That( created, Is.EqualTo( CreateOrUpdateResult.Created ) );
                var info2 = u.FindKnownUserInfo( ctx, googleAccountId );

                Assert.That( info2.UserId, Is.EqualTo( userId ) );
                Assert.That( info2.Info.GoogleAccountId, Is.EqualTo( googleAccountId ) );

                Assert.That( u.FindKnownUserInfo( ctx, Guid.NewGuid().ToString() ), Is.Null );
                user.DestroyUser( ctx, 1, userId );
                Assert.That( u.FindKnownUserInfo( ctx, googleAccountId ), Is.Null );
            }
        }

        [Test]
        public async Task create_Google_user_and_check_read_info_object_method_async()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserGoogleTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            var infoFactory = TestHelper.StObjMap.Default.Obtain<IPocoFactory<IUserGoogleInfo>>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = await user.CreateUserAsync( ctx, 1, userName );
                var googleAccountId = Guid.NewGuid().ToString( "N" );

                var info = infoFactory.Create();
                info.GoogleAccountId = googleAccountId;
                var created = await u.CreateOrUpdateGoogleUserAsync( ctx, 1, userId, info );
                Assert.That( created, Is.EqualTo( CreateOrUpdateResult.Created ) );
                var info2 = await u.FindUserInfoAsync( ctx, googleAccountId );

                Assert.That( info2.UserId, Is.EqualTo( userId ) );
                Assert.That( info2.Info.GoogleAccountId, Is.EqualTo( googleAccountId ) );

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
                string userName = "Google auth - " + Guid.NewGuid().ToString();
                var googleAccountId = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );
                u.Database.AssertEmptyReader( $"select * from CK.vUserAuthProvider where UserId={idU} and ProviderName='Google'" );
                var info = u.CreateUserInfo<IUserGoogleInfo>();
                info.GoogleAccountId = googleAccountId;
                u.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                u.Database.AssertScalarEquals( 1, $"select count(*) from CK.vUserAuthProvider where UserId={idU} and ProviderName='Google'" );
                u.DestroyGoogleUser( ctx, 1, idU );
                u.Database.AssertEmptyReader( $"select * from CK.vUserAuthProvider where UserId={idU} and ProviderName='Google'" );
            }
        }

        [Test]
        public void Google_provider_ignores_AuthProvider_IsEnabled_flag_as_required()
        {
            var provider = TestHelper.StObjMap.Default.Obtain<AuthProviderTable>();
            var u = TestHelper.StObjMap.Default.Obtain<UserGoogleTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "Google auth - " + Guid.NewGuid().ToString();
                var googleAccountId = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );

                provider.EnableProvider( ctx, 1, "Google", false );
                var info = u.CreateUserInfo<IUserGoogleInfo>();
                info.GoogleAccountId = googleAccountId;
                var created = u.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                Assert.That( created, Is.EqualTo( CreateOrUpdateResult.Created ) );
                var loggedId = u.LoginUser( ctx, info, true );
                Assert.That( loggedId, Is.EqualTo( idU ) );

                provider.EnableProvider( ctx, 1, "Google" );
            }
        }

        [Test]
        public void standard_generic_tests_for_Google_provider()
        {
            var auth = TestHelper.StObjMap.Default.Obtain<Auth.Package>();
            var f = TestHelper.StObjMap.Default.Obtain<IPocoFactory<IUserGoogleInfo>>();
            CK.DB.Auth.Tests.AuthTests.StandardTestGorGenericAuthenticationProvider(
                auth,
                "Google",
                payloadForCreateOrUpdate: (userId, userName) => f.Create( i => i.GoogleAccountId = "GoogleAccountIdFor:"+userName ),
                payloadForLogin: (userId, userName) => f.Create(i => i.GoogleAccountId = "GoogleAccountIdFor:" + userName),
                payloadForLoginFail: (userId, userName) => f.Create(i => i.GoogleAccountId = "NO!" + userName)
                );
        }

    }

}

