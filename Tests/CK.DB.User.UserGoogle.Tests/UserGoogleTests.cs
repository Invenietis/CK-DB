using System;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using CK.DB.Auth;
using System.Collections.Generic;
using FluentAssertions;
using CK.Testing;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserGoogle.Tests;

[TestFixture]
public class UserGoogleTests
{
    [Test]
    public void create_Google_user_and_check_read_info_object_method()
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserGoogleTable>();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserGoogleInfo>>();
        using( var ctx = new SqlStandardCallContext() )
        {
            var userName = Guid.NewGuid().ToString();
            int userId = user.CreateUser( ctx, 1, userName );
            var googleAccountId = Guid.NewGuid().ToString( "N" );

            var info = infoFactory.Create();
            info.GoogleAccountId = googleAccountId;
            var created = u.CreateOrUpdateGoogleUser( ctx, 1, userId, info );
            created.OperationResult.Should().Be( UCResult.Created );
            var info2 = u.FindKnownUserInfo( ctx, googleAccountId );

            info2.UserId.Should().Be( userId );
            info2.Info.GoogleAccountId.Should().Be( googleAccountId );

            u.FindKnownUserInfo( ctx, Guid.NewGuid().ToString() ).Should().BeNull();
            user.DestroyUser( ctx, 1, userId );
            u.FindKnownUserInfo( ctx, googleAccountId ).Should().BeNull();
        }
    }

    [Test]
    public async Task create_Google_user_and_check_read_info_object_method_Async()
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserGoogleTable>();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserGoogleInfo>>();
        using( var ctx = new SqlStandardCallContext() )
        {
            var userName = Guid.NewGuid().ToString();
            int userId = await user.CreateUserAsync( ctx, 1, userName );
            var googleAccountId = Guid.NewGuid().ToString( "N" );

            var info = infoFactory.Create();
            info.GoogleAccountId = googleAccountId;
            var created = await u.CreateOrUpdateGoogleUserAsync( ctx, 1, userId, info );
            created.OperationResult.Should().Be( UCResult.Created );
            var info2 = await u.FindKnownUserInfoAsync( ctx, googleAccountId );

            info2.UserId.Should().Be( userId );
            info2.Info.GoogleAccountId.Should().Be( googleAccountId );

            (await u.FindKnownUserInfoAsync( ctx, Guid.NewGuid().ToString() )).Should().BeNull();
            await user.DestroyUserAsync( ctx, 1, userId );
            (await u.FindKnownUserInfoAsync( ctx, googleAccountId )).Should().BeNull();
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
        var u = SharedEngine.Map.StObjs.Obtain<UserGoogleTable>();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        using( var ctx = new SqlStandardCallContext() )
        {
            string userName = "Google auth - " + Guid.NewGuid().ToString();
            var googleAccountId = Guid.NewGuid().ToString( "N" );
            var idU = user.CreateUser( ctx, 1, userName );
            u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='Google'" )
                .Rows.Should().BeEmpty();
            var info = u.CreateUserInfo<IUserGoogleInfo>();
            info.GoogleAccountId = googleAccountId;
            u.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
            u.Database.ExecuteScalar( $"select count(*) from CK.vUserAuthProvider where UserId={idU} and Scheme='Google'" )
                .Should().Be( 1 );
            u.DestroyGoogleUser( ctx, 1, idU );
            u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='Google'" )
                .Rows.Should().BeEmpty();
        }
    }

    [Test]
    public void standard_generic_tests_for_Google_provider()
    {
        var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>();
        // With IUserGoogleInfo POCO.
        var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserGoogleInfo>>();
        CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
            auth,
            "Google",
            payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => i.GoogleAccountId = "GoogleAccountIdFor:" + userName ),
            payloadForLogin: ( userId, userName ) => f.Create( i => i.GoogleAccountId = "GoogleAccountIdFor:" + userName ),
            payloadForLoginFail: ( userId, userName ) => f.Create( i => i.GoogleAccountId = "NO!" + userName )
            );
        // With a KeyValuePair.
        CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
            auth,
            "Google",
            payloadForCreateOrUpdate: ( userId, userName ) => new[]
            {
                new KeyValuePair<string,object>( "GoogleAccountId", "IdFor:" + userName)
            },
            payloadForLogin: ( userId, userName ) => new[]
            {
                new KeyValuePair<string,object>( "GoogleAccountId", "IdFor:" + userName)
            },
            payloadForLoginFail: ( userId, userName ) => new[]
            {
                new KeyValuePair<string,object>( "GoogleAccountId", ("IdFor:" + userName).ToUpperInvariant())
            }
            );
    }

    [Test]
    public async Task standard_generic_tests_for_Google_provider_Async()
    {
        var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>();
        var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserGoogleInfo>>();
        f.Should().NotBeNull( "IPocoFactory<IUserGoogleInfo> cannot be obtained." );
        await Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProviderAsync(
            auth,
            "Google",
            payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => i.GoogleAccountId = "GoogleAccountIdFor:" + userName ),
            payloadForLogin: ( userId, userName ) => f.Create( i => i.GoogleAccountId = "GoogleAccountIdFor:" + userName ),
            payloadForLoginFail: ( userId, userName ) => f.Create( i => i.GoogleAccountId = "NO!" + userName )
            );
    }

}

