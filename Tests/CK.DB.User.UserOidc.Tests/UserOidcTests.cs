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

namespace CK.DB.User.UserOidc.Tests
{
    [TestFixture]
    public class UserOidcTests
    {
        [TestCase( "" )]
        [TestCase( "IdSrv" )]
        public void create_Oidc_user_and_check_read_info_object_method( string schemeSuffix )
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserOidcTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, userName );
                var sub = Guid.NewGuid().ToString( "N" );

                var info = infoFactory.Create();
                info.SchemeSuffix = schemeSuffix;
                info.Sub = sub;
                var created = u.CreateOrUpdateOidcUser( ctx, 1, userId, info );
                created.OperationResult.Should().Be( UCResult.Created );
                var info2 = u.FindKnownUserInfo( ctx, schemeSuffix, sub );

                info2.UserId.Should().Be( userId );
                info2.Info.SchemeSuffix.Should().Be( schemeSuffix );
                info2.Info.Sub.Should().Be( sub );

                u.FindKnownUserInfo( ctx, schemeSuffix, Guid.NewGuid().ToString() ).Should().BeNull();
                user.DestroyUser( ctx, 1, userId );
                u.FindKnownUserInfo( ctx, schemeSuffix, sub ).Should().BeNull();
            }
        }

        [TestCase( "" )]
        [TestCase( "IdSrv" )]
        public async Task create_Oidc_user_and_check_read_info_object_method_Async( string schemeSuffix )
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserOidcTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = await user.CreateUserAsync( ctx, 1, userName );
                var sub = Guid.NewGuid().ToString( "N" );

                var info = infoFactory.Create();
                info.SchemeSuffix = schemeSuffix;
                info.Sub = sub;
                var created = await u.CreateOrUpdateOidcUserAsync( ctx, 1, userId, info );
                created.OperationResult.Should().Be( UCResult.Created );
                var info2 = await u.FindKnownUserInfoAsync( ctx, schemeSuffix, sub );

                info2.UserId.Should().Be( userId );
                info2.Info.SchemeSuffix.Should().Be( schemeSuffix );
                info2.Info.Sub.Should().Be( sub );

                (await u.FindKnownUserInfoAsync( ctx, schemeSuffix, Guid.NewGuid().ToString() )).Should().BeNull();
                await user.DestroyUserAsync( ctx, 1, userId );
                (await u.FindKnownUserInfoAsync( ctx, schemeSuffix, sub )).Should().BeNull();
            }
        }

        [Test]
        public void Oidc_AuthProvider_is_registered()
        {
            Auth.Tests.AuthTests.CheckProviderRegistration( "Oidc" );
        }

        [TestCase( "" )]
        [TestCase( "IdSrv" )]
        public void vUserAuthProvider_reflects_the_user_Oidc_authentication( string schemeSuffix )
        {
            string scheme = schemeSuffix.Length > 0 ? "Oidc." + schemeSuffix : "Oidc";
            var u = SharedEngine.Map.StObjs.Obtain<UserOidcTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "Oidc auth - " + Guid.NewGuid().ToString();
                var sub = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );
                u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='{scheme}'" )
                    .Rows.Should().BeEmpty();
                var info = u.CreateUserInfo<IUserOidcInfo>();
                info.SchemeSuffix = schemeSuffix;
                info.Sub = sub;
                u.CreateOrUpdateOidcUser( ctx, 1, idU, info );
                u.Database.ExecuteScalar( $"select count(*) from CK.vUserAuthProvider where UserId={idU} and Scheme='{scheme}'" )
                    .Should().Be( 1 );
                u.DestroyOidcUser( ctx, 1, idU, schemeSuffix );
                u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='{scheme}'" )
                      .Rows.Should().BeEmpty();
            }
        }

        [TestCase( "" )]
        [TestCase( "IdSrv" )]
        public void standard_generic_tests_for_Oidc_provider( string schemeSuffix )
        {
            string scheme = schemeSuffix.Length > 0 ? "Oidc." + schemeSuffix : "Oidc";

            var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>();
            // With IUserOidcInfo POCO.
            var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>();
            CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
                auth,
                scheme,
                payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "OidcAccountIdFor:" + userName; } ),
                payloadForLogin: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "OidcAccountIdFor:" + userName; } ),
                payloadForLoginFail: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "NO!" + userName; } )
                );

            // With KeyValuePairs.
            CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
                auth,
                scheme,
                payloadForCreateOrUpdate: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "SchemeSuffix", schemeSuffix ),
                    new KeyValuePair<string,object>( "Sub", "IdFor:" + userName)
                },
                payloadForLogin: ( userId, userName ) => new[]
                {
                   new KeyValuePair<string,object>( "Sub", "IdFor:" + userName),
                     new KeyValuePair<string,object>( "SchemeSuffix", schemeSuffix ),
                },
                payloadForLoginFail: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "Sub", ("IdFor:" + userName).ToUpperInvariant()),
                     new KeyValuePair<string,object>( "SchemeSuffix", schemeSuffix ),
                } );

            // With ValueTuples.
            CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
                auth,
                scheme,
                payloadForCreateOrUpdate: ( userId, userName ) => new[]
                {
                    ( "SchemeSuffix", schemeSuffix ),
                    ( "Sub", "IdFor:" + userName)
                },
                payloadForLogin: ( userId, userName ) => new[]
                {
                   ( "Sub", "IdFor:" + userName),
                   ( "SchemeSuffix", schemeSuffix ),
                },
                payloadForLoginFail: ( userId, userName ) => new[]
                {
                    ( "Sub", ("IdFor:" + userName).ToUpperInvariant()),
                    ( "SchemeSuffix", schemeSuffix ),
                } );
        }

        [TestCase( "" )]
        [TestCase( "IdSrv" )]
        public async Task standard_generic_tests_for_Oidc_provider_Async( string schemeSuffix )
        {
            string scheme = schemeSuffix.Length > 0 ? "Oidc." + schemeSuffix : "Oidc";
            var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>();
            var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>();
            await Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProviderAsync(
                auth,
                scheme,
                payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "OidcAccountIdFor:" + userName; } ),
                payloadForLogin: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "OidcAccountIdFor:" + userName; } ),
                payloadForLoginFail: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "NO!" + userName; } )
                );
        }

    }

}

