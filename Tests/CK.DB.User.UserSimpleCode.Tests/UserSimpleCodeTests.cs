using System;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using CK.DB.Auth;
using System.Collections.Generic;
using FluentAssertions;
using static CK.Testing.MonitorTestHelper;
using CK.Testing;

namespace CK.DB.User.UserSimpleCode.Tests
{
    [TestFixture]
    public class UserSimpleCodeTests
    {
        [Test]
        public void create_SimpleCode_user_and_check_read_info_object_method()
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserSimpleCodeTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserSimpleCodeInfo>>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, userName );
                var googleAccountId = Guid.NewGuid().ToString( "N" );

                var info = infoFactory.Create();
                info.SimpleCode = googleAccountId;
                var created = u.CreateOrUpdateSimpleCodeUser( ctx, 1, userId, info );
                created.OperationResult.Should().Be( UCResult.Created );
                var info2 = u.FindKnownUserInfo( ctx, googleAccountId );

                info2.UserId.Should().Be( userId );
                info2.Info.SimpleCode.Should().Be( googleAccountId );

                u.FindKnownUserInfo( ctx, Guid.NewGuid().ToString() ).Should().BeNull();
                user.DestroyUser( ctx, 1, userId );
                u.FindKnownUserInfo( ctx, googleAccountId ).Should().BeNull();
            }
        }

        [Test]
        public async Task create_SimpleCode_user_and_check_read_info_object_method_Async()
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserSimpleCodeTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserSimpleCodeInfo>>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = await user.CreateUserAsync( ctx, 1, userName );
                var googleAccountId = Guid.NewGuid().ToString( "N" );

                var info = infoFactory.Create();
                info.SimpleCode = googleAccountId;
                var created = await u.CreateOrUpdateSimpleCodeUserAsync( ctx, 1, userId, info );
                created.OperationResult.Should().Be( UCResult.Created );
                var info2 = await u.FindKnownUserInfoAsync( ctx, googleAccountId );

                info2.UserId.Should().Be( userId );
                info2.Info.SimpleCode.Should().Be( googleAccountId );

                (await u.FindKnownUserInfoAsync( ctx, Guid.NewGuid().ToString() )).Should().BeNull();
                await user.DestroyUserAsync( ctx, 1, userId );
                (await u.FindKnownUserInfoAsync( ctx, googleAccountId )).Should().BeNull();
            }
        }

        [Test]
        public void SimpleCode_AuthProvider_is_registered()
        {
            Auth.Tests.AuthTests.CheckProviderRegistration( "SimpleCode" );
        }

        [Test]
        public void vUserAuthProvider_reflects_the_user_SimpleCode_authentication()
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserSimpleCodeTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "SimpleCode auth - " + Guid.NewGuid().ToString();
                var googleAccountId = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );
                u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='SimpleCode'" )
                    .Rows.Should().BeEmpty();
                var info = u.CreateUserInfo<IUserSimpleCodeInfo>();
                info.SimpleCode = googleAccountId;
                u.CreateOrUpdateSimpleCodeUser( ctx, 1, idU, info );
                u.Database.ExecuteScalar( $"select count(*) from CK.vUserAuthProvider where UserId={idU} and Scheme='SimpleCode'" )
                    .Should().Be( 1 );
                u.DestroySimpleCodeUser( ctx, 1, idU );
                u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='SimpleCode'" )
                    .Rows.Should().BeEmpty();
            }
        }

        [Test]
        public void standard_generic_tests_for_SimpleCode_provider()
        {
            var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>();
            // With IUserSimpleCodeInfo POCO.
            var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserSimpleCodeInfo>>();
            CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
                auth,
                "SimpleCode",
                payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => i.SimpleCode = "SimpleCodeFor:" + userName ),
                payloadForLogin: ( userId, userName ) => f.Create( i => i.SimpleCode = "SimpleCodeFor:" + userName ),
                payloadForLoginFail: ( userId, userName ) => f.Create( i => i.SimpleCode = "NO!" + userName )
                );
            // With a KeyValuePair.
            CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
                auth,
                "SimpleCode",
                payloadForCreateOrUpdate: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "SimpleCode", "IdFor:" + userName)
                },
                payloadForLogin: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "SimpleCode", "IdFor:" + userName)
                },
                payloadForLoginFail: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "SimpleCode", ("IdFor:" + userName).ToUpperInvariant())
                }
                );
        }

        [Test]
        public async Task standard_generic_tests_for_SimpleCode_provider_Async()
        {
            var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>();
            var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserSimpleCodeInfo>>();
            await Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProviderAsync(
                auth,
                "SimpleCode",
                payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => i.SimpleCode = "SimpleCodeFor:" + userName ),
                payloadForLogin: ( userId, userName ) => f.Create( i => i.SimpleCode = "SimpleCodeFor:" + userName ),
                payloadForLoginFail: ( userId, userName ) => f.Create( i => i.SimpleCode = "NO!" + userName )
                );
        }

    }

}

