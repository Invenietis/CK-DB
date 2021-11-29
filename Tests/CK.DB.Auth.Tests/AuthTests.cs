using CK.Core;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.Auth.Tests
{
    [TestFixture]
    public class AuthTests
    {

        [Test]
        public void calling_OnUserLogin_directlty_works_but_is_reserved_for_very_rare_scenarii()
        {
            var auth = TestHelper.StObjMap.StObjs.Obtain<Package>();
            var user = TestHelper.StObjMap.StObjs.Obtain<Actor.UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            using( auth.Database.TemporaryTransform( @"
                    create transformer on CK.sAuthUserOnLogin
                    as
                    begin
                        inject
                        "" if @Scheme = 'test-fail'
                           begin
                                set @FailureReason = N'The failure';
                                set @FailureCode = 3712;
                           end
                           if @Scheme = 'test-fail-reason-only'
                           begin
                                set @FailureReason = N'The failure text';
                           end
                           if @Scheme = 'test-fail-reason-only-empty'
                           begin
                                set @FailureReason = N'  ';
                           end
                           if @Scheme = 'test-fail-code-only'
                           begin
                                set @FailureCode = 42;
                           end
                        "" into ""CheckLoginFailure"";
                    end
                    " ) )
            {
                int idUser = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                var result = auth.OnUserLogin( ctx, "test-success", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
                result.FailureCode.Should().Be( 0 );
                result.FailureReason.Should().BeNull();
                result.IsSuccess.Should().BeTrue();
                result.UserId.Should().Be( idUser );

                result = auth.OnUserLogin( ctx, "test-fail", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
                result.FailureCode.Should().Be( 3712 );
                result.FailureReason.Should().Be( "The failure" );
                result.IsSuccess.Should().BeFalse();
                result.UserId.Should().Be( 0 );
                
                result = auth.OnUserLogin( ctx, "test-fail-reason-only", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
                result.FailureCode.Should().Be( (int)KnownLoginFailureCode.Unspecified );
                result.FailureReason.Should().Be( "The failure text" );
                result.IsSuccess.Should().BeFalse();
                result.UserId.Should().Be( 0 );

                result = auth.OnUserLogin( ctx, "test-fail-reason-only-empty", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
                result.FailureCode.Should().Be( (int)KnownLoginFailureCode.Unspecified );
                result.FailureReason.Should().Be( "Unspecified reason." );
                result.IsSuccess.Should().BeFalse();
                result.UserId.Should().Be( 0 );

                result = auth.OnUserLogin( ctx, "test-fail-code-only", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
                result.FailureCode.Should().Be( 42 );
                result.FailureReason.Should().Be( "Unspecified reason." );
                result.IsSuccess.Should().BeFalse();
                result.UserId.Should().Be( 0 );
            }
        }

        [Test]
        public void when_basic_provider_exists_it_is_registered_in_tAuthProvider_and_available_as_IGenericAuthenticationProvider()
        {
            var auth = TestHelper.StObjMap.StObjs.Obtain<Package>();
            Assume.That( auth.BasicProvider != null );

            auth.AllProviders.Single( provider => provider.ProviderName == "Basic" ).Should().NotBeNull();
            auth.FindProvider( "Basic" ).Should().NotBeNull();
            auth.FindProvider( "bASIC" ).Should().NotBeNull();
            auth.FindRequiredProvider( "Basic", mustHavePayload: false ).Should().NotBeNull();
        }

        static public void StandardTestForGenericAuthenticationProvider(
            Package auth,
            string schemeOrProviderName,
            Func<int, string, object> payloadForCreateOrUpdate,
            Func<int, string, object> payloadForLogin,
            Func<int, string, object> payloadForLoginFail
            )
        {
            var user = TestHelper.StObjMap.StObjs.Obtain<Actor.UserTable>();
            IGenericAuthenticationProvider g = auth.FindProvider( schemeOrProviderName );
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, userName );
                using( TestHelper.Monitor.OpenInfo( $"StandardTest for generic {schemeOrProviderName} with userId:{userId} and userName:{userName}." ) )
                {

                    IUserAuthInfo info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    info.UserId.Should().Be( userId );
                    info.UserName.Should().Be( userName );
                    info.Schemes.Should().BeEmpty();

                    using( TestHelper.Monitor.OpenInfo( "CreateOrUpdateUser without login" ) )
                    {
                        g.CreateOrUpdateUser( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ) ).OperationResult.Should().Be( UCResult.Created );
                        info = auth.ReadUserAuthInfo( ctx, 1, userId );
                        info.Schemes.Count.Should().Be( 0, "Still no scheme since we did not use WithActualLogin." );

                        g.LoginUser( ctx, payloadForLogin( userId, userName ), actualLogin: false ).UserId.Should().Be( userId );
                        info = auth.ReadUserAuthInfo( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty( "Still no scheme since we challenge login but not use WithActualLogin." );

                        g.LoginUser( ctx, payloadForLogin( userId, userName ) ).UserId.Should().Be( userId );
                        info = auth.ReadUserAuthInfo( ctx, 1, userId );
                        info.Schemes.Count.Should().Be( 1 );
                        info.Schemes[0].Name.Should().StartWith( g.ProviderName );
                        info.Schemes[0].Name.Should().BeEquivalentTo( schemeOrProviderName );
                        info.Schemes[0].LastUsed.Should().BeCloseTo( DateTime.UtcNow, TimeSpan.FromSeconds( 1 ) );

                        g.DestroyUser( ctx, 1, userId );
                        info = auth.ReadUserAuthInfo( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty();
                    }
                    using( TestHelper.Monitor.OpenInfo( "CreateOrUpdateUser WithActualLogin" ) )
                    {
                        info.UserId.Should().Be( userId );
                        info.UserName.Should().Be( userName );
                        info.Schemes.Should().BeEmpty();

                        var result = g.CreateOrUpdateUser( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ), UCLMode.CreateOnly | UCLMode.WithActualLogin );
                        result.OperationResult.Should().Be( UCResult.Created );
                        result.LoginResult.UserId.Should().Be( userId );
                        info = auth.ReadUserAuthInfo( ctx, 1, userId );
                        info.Schemes.Should().HaveCount( 1 );
                        info.Schemes[0].Name.Should().StartWith( g.ProviderName );
                        info.Schemes[0].Name.Should().BeEquivalentTo( schemeOrProviderName );
                        info.Schemes[0].LastUsed.Should().BeCloseTo( DateTime.UtcNow, TimeSpan.FromSeconds( 1 ) );

                        g.LoginUser( ctx, payloadForLoginFail( userId, userName ) ).UserId.Should().Be( 0 );

                        g.DestroyUser( ctx, 1, userId );
                        info = auth.ReadUserAuthInfo( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty();
                    }
                    using( TestHelper.Monitor.OpenInfo( "Login for an unregistered user." ) )
                    {
                        info.UserId.Should().Be( userId );
                        info.UserName.Should().Be( userName );
                        info.Schemes.Should().BeEmpty();

                        var result = g.LoginUser( ctx, payloadForLogin( userId, userName ) );
                        result.IsSuccess.Should().BeFalse();
                        result.UserId.Should().Be( 0 );
                        result.FailureCode.Should().Be( (int)KnownLoginFailureCode.UnregisteredUser );
                        result.FailureReason.Should().Be( "Unregistered user." );
                        info = auth.ReadUserAuthInfo( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty();

                        g.DestroyUser( ctx, 1, userId );
                        info = auth.ReadUserAuthInfo( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty();
                    }
                    using( TestHelper.Monitor.OpenInfo( "Invalid payload MUST throw an ArgumentException." ) )
                    {
                        g.Invoking( sut => sut.CreateOrUpdateUser( ctx, 1, userId, DBNull.Value ) ).Should().Throw<ArgumentException>();
                        g.Invoking( sut => sut.LoginUser( ctx, DBNull.Value ) ).Should().Throw<ArgumentException>();
                    }
                }
                user.DestroyUser( ctx, 1, userId );
            }
        }

        static public async Task StandardTestForGenericAuthenticationProviderAsync(
            Package auth,
            string schemeOrProviderName,
            Func<int, string, object> payloadForCreateOrUpdate,
            Func<int, string, object> payloadForLogin,
            Func<int, string, object> payloadForLoginFail
            )
        {
            var user = TestHelper.StObjMap.StObjs.Obtain<Actor.UserTable>();
            IGenericAuthenticationProvider g = auth.FindProvider( schemeOrProviderName );
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = Guid.NewGuid().ToString();
                int userId = await user.CreateUserAsync( ctx, 1, userName );
                using( TestHelper.Monitor.OpenInfo( $"StandardTestAsync for generic {schemeOrProviderName} with userId:{userId} and userName:{userName}." ) )
                {
                    IUserAuthInfo info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );

                    info.UserId.Should().Be( userId );
                    info.UserName.Should().Be( userName );
                    info.Schemes.Should().BeEmpty();

                    using( TestHelper.Monitor.OpenInfo( "CreateOrUpdateUser without login." ) )
                    {
                        (await g.CreateOrUpdateUserAsync( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ) )).OperationResult.Should().Be( UCResult.Created );
                        info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty( "Still no scheme since we did not use WithLogin." );

                        (await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ), actualLogin: false )).UserId.Should().Be( userId );
                        info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty( "Still no scheme since we challenge login but not use WithLogin." );

                        (await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ) )).UserId.Should().Be( userId );
                        info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                        info.Schemes.Should().HaveCount( 1 );
                        info.Schemes[0].Name.Should().StartWith( g.ProviderName );
                        info.Schemes[0].Name.Should().BeEquivalentTo( schemeOrProviderName );
                        info.Schemes[0].LastUsed.Should().BeCloseTo( DateTime.UtcNow, TimeSpan.FromSeconds( 1 ) );

                        await g.DestroyUserAsync( ctx, 1, userId );
                        info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty();
                    }
                    using( TestHelper.Monitor.OpenInfo( "CreateOrUpdateUser WithActualLogin." ) )
                    {
                        info.UserId.Should().Be( userId );
                        info.UserName.Should().Be( userName );
                        info.Schemes.Should().BeEmpty();

                        var result = await g.CreateOrUpdateUserAsync( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ), UCLMode.CreateOnly | UCLMode.WithActualLogin );
                        result.OperationResult.Should().Be( UCResult.Created );
                        result.LoginResult.UserId.Should().Be( userId );
                        info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                        info.Schemes.Count.Should().Be( 1 );
                        info.Schemes[0].Name.Should().StartWith( g.ProviderName );
                        info.Schemes[0].Name.Should().BeEquivalentTo( schemeOrProviderName );
                        info.Schemes[0].LastUsed.Should().BeCloseTo( DateTime.UtcNow, TimeSpan.FromSeconds( 1 ) );

                        (await g.LoginUserAsync( ctx, payloadForLoginFail( userId, userName ) )).UserId.Should().Be( 0 );

                        await g.DestroyUserAsync( ctx, 1, userId );
                        info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty();
                    }
                    using( TestHelper.Monitor.OpenInfo( "Login for an unregistered user." ) )
                    {
                        info.UserId.Should().Be( userId );
                        info.UserName.Should().Be( userName );
                        info.Schemes.Should().BeEmpty();

                        var result = await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ) );
                        result.IsSuccess.Should().BeFalse();
                        result.UserId.Should().Be( 0 );
                        result.FailureCode.Should().Be( (int)KnownLoginFailureCode.UnregisteredUser );
                        result.FailureReason.Should().Be( "Unregistered user." );
                        info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty();

                        await g.DestroyUserAsync( ctx, 1, userId );
                        info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                        info.Schemes.Should().BeEmpty();
                    }
                    using( TestHelper.Monitor.OpenInfo( "Invalid payload MUST throw an ArgumentException." ) )
                    {
                        await g.Awaiting( sut => sut.CreateOrUpdateUserAsync( ctx, 1, userId, DBNull.Value ) ).Should().ThrowAsync<ArgumentException>();
                        await g.Awaiting( sut => sut.LoginUserAsync( ctx, DBNull.Value ) ).Should().ThrowAsync<ArgumentException>();
                    }
                    using( TestHelper.Monitor.OpenInfo( "Injecting disabled user in sAuthUserOnLogin." ) )
                    using( auth.Database.TemporaryTransform( @"
                            create transformer on CK.sAuthUserOnLogin
                            as
                            begin
                                inject ""set @FailureCode = 6; -- GloballyDisabledUser"" into ""CheckLoginFailure"";
                            end
                        " ) )
                    {
                        UCLResult result = await g.CreateOrUpdateUserAsync( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ), UCLMode.CreateOnly | UCLMode.WithActualLogin );
                        result.OperationResult.Should().Be( UCResult.Created );
                        result.LoginResult.UserId.Should().Be( 0 );
                        result.LoginResult.IsSuccess.Should().BeFalse();
                        result.LoginResult.FailureCode.Should().Be( (int)KnownLoginFailureCode.GloballyDisabledUser );
                        LoginResult login = await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ) );
                        login.IsSuccess.Should().BeFalse();
                        login.UserId.Should().Be( 0 );
                        login.FailureCode.Should().Be( (int)KnownLoginFailureCode.GloballyDisabledUser );
                        login = await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ), actualLogin: false );
                        login.IsSuccess.Should().BeFalse();
                        login.UserId.Should().Be( 0 );
                        login.FailureCode.Should().Be( (int)KnownLoginFailureCode.GloballyDisabledUser );
                    }
                }
                await user.DestroyUserAsync( ctx, 1, userId );
            }
        }

        [Test]
        public void when_a_basic_provider_exists_its_IGenericAuthenticationProvider_adpater_accepts_UserId_or_UserName_based_login_payloads()
        {
            var auth = TestHelper.StObjMap.StObjs.Obtain<Package>();
            Assume.That( auth.BasicProvider != null );

            // With Tuple (UserId, Password) payload.  
            StandardTestForGenericAuthenticationProvider(
                auth,
                "Basic",
                payloadForCreateOrUpdate: ( userId, userName ) => "password",
                payloadForLogin: ( userId, userName ) => Tuple.Create( userId, "password" ),
                payloadForLoginFail: ( userId, userName ) => Tuple.Create( userId, "wrong password" ) );

            // With Tuple (UserName, Password) payload.  
            StandardTestForGenericAuthenticationProvider(
                auth,
                "Basic",
                payloadForCreateOrUpdate: ( userId, userName ) => "password",
                payloadForLogin: ( userId, userName ) => Tuple.Create( userName, "password" ),
                payloadForLoginFail: ( userId, userName ) => Tuple.Create( userName, "wrong password" ) );

            // With KeyValuePairs (UserName, Password) payload.  
            StandardTestForGenericAuthenticationProvider(
                auth,
                "Basic",
                payloadForCreateOrUpdate: ( userId, userName ) => "£$$µ+",
                payloadForLogin: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string, object>("username", userName),
                    new KeyValuePair<string, object>("password", "£$$µ+")
                },
                payloadForLoginFail: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string, object>("username", userName),
                    new KeyValuePair<string, object>("password", "wrong password")
                } );

            // With KeyValuePairs (UserId, Password) payload.  
            StandardTestForGenericAuthenticationProvider(
                auth,
                "Basic",
                payloadForCreateOrUpdate: ( userId, userName ) => "MM£$$µ+",
                payloadForLogin: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string, object>("USERID", userId),
                    new KeyValuePair<string, object>("PASSWORD", "MM£$$µ+")
                },
                payloadForLoginFail: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string, object>("USERID", userId),
                    new KeyValuePair<string, object>("PASSWORD", "wrong password")
                } );
        }

        [Test]
        public async Task when_a_basic_provider_exists_its_IGenericAuthenticationProvider_adpater_accepts_UserId_or_UserName_based_login_payloads_Async()
        {
            var auth = TestHelper.StObjMap.StObjs.Obtain<Package>();
            Assume.That( auth.BasicProvider != null );

            // With (UserId, Password) payload.  
            await StandardTestForGenericAuthenticationProviderAsync(
                auth,
                "Basic",
                payloadForCreateOrUpdate: ( userId, userName ) => "password",
                payloadForLogin: ( userId, userName ) => Tuple.Create( userId, "password" ),
                payloadForLoginFail: ( userId, userName ) => Tuple.Create( userId, "wrong password" ) );

            // With (UserName, Password) payload.  
            await StandardTestForGenericAuthenticationProviderAsync(
                auth,
                "Basic",
                payloadForCreateOrUpdate: ( userId, userName ) => "password",
                payloadForLogin: ( userId, userName ) => Tuple.Create( userName, "password" ),
                payloadForLoginFail: ( userId, userName ) => Tuple.Create( userName, "wrong password" ) );
        }

        /// <summary>
        /// Helper to be used by actual providers to check that they are properly registered.
        /// </summary>
        /// <param name="providerName">provider name that must be registered.</param>
        public static void CheckProviderRegistration( string providerName )
        {
            var provider = TestHelper.StObjMap.StObjs.Obtain<AuthProviderTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                provider.Database.ExecuteScalar( "select count(*) from CK.tAuthProvider where ProviderName = @0", providerName )
                    .Should().Be( 1 );
            }
        }

        [Test]
        public void reading_IUserAuthInfo_for_an_unexisting_user_or_Anonymous_returns_null()
        {
            var p = TestHelper.StObjMap.StObjs.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                IUserAuthInfo info = p.ReadUserAuthInfo( ctx, 1, int.MaxValue );
                info.Should().BeNull();
                info = p.ReadUserAuthInfo( ctx, 1, 0 );
                info.Should().BeNull();
            }
        }
    }
}
