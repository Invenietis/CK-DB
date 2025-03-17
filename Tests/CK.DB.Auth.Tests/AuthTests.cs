using CK.Core;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using CK.Testing;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Auth.Tests;

[TestFixture]
public class AuthTests
{

    [Test]
    public void calling_OnUserLogin_directlty_works_but_is_reserved_for_very_rare_scenarii()
    {
        var auth = SharedEngine.Map.StObjs.Obtain<Package>();
        var user = SharedEngine.Map.StObjs.Obtain<Actor.UserTable>();
        Throw.DebugAssert( auth != null && user != null );
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
            result.FailureCode.ShouldBe( 0 );
            result.FailureReason.ShouldBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.UserId.ShouldBe( idUser );

            result = auth.OnUserLogin( ctx, "test-fail", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
            result.FailureCode.ShouldBe( 3712 );
            result.FailureReason.ShouldBe( "The failure" );
            result.IsSuccess.ShouldBeFalse();
            result.UserId.ShouldBe( 0 );

            result = auth.OnUserLogin( ctx, "test-fail-reason-only", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
            result.FailureCode.ShouldBe( (int)KnownLoginFailureCode.Unspecified );
            result.FailureReason.ShouldBe( "The failure text" );
            result.IsSuccess.ShouldBeFalse();
            result.UserId.ShouldBe( 0 );

            result = auth.OnUserLogin( ctx, "test-fail-reason-only-empty", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
            result.FailureCode.ShouldBe( (int)KnownLoginFailureCode.Unspecified );
            result.FailureReason.ShouldBe( "Unspecified reason." );
            result.IsSuccess.ShouldBeFalse();
            result.UserId.ShouldBe( 0 );

            result = auth.OnUserLogin( ctx, "test-fail-code-only", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
            result.FailureCode.ShouldBe( 42 );
            result.FailureReason.ShouldBe( "Unspecified reason." );
            result.IsSuccess.ShouldBeFalse();
            result.UserId.ShouldBe( 0 );
        }
    }

    [Test]
    public void when_basic_provider_exists_it_is_registered_in_tAuthProvider_and_available_as_IGenericAuthenticationProvider()
    {
        var auth = SharedEngine.Map.StObjs.Obtain<Package>();
        Throw.DebugAssert( auth != null );
        Assume.That( auth.BasicProvider != null );

        auth.AllProviders.Single( provider => provider.ProviderName == "Basic" ).ShouldNotBeNull();
        auth.FindProvider( "Basic" ).ShouldNotBeNull();
        auth.FindProvider( "bASIC" ).ShouldNotBeNull();
        auth.FindRequiredProvider( "Basic", mustHavePayload: false ).ShouldNotBeNull();
    }

    static public void StandardTestForGenericAuthenticationProvider( Package auth,
                                                                     string schemeOrProviderName,
                                                                     Func<int, string, object> payloadForCreateOrUpdate,
                                                                     Func<int, string, object> payloadForLogin,
                                                                     Func<int, string, object> payloadForLoginFail )
    {
        var user = SharedEngine.Map.StObjs.Obtain<Actor.UserTable>();
        IGenericAuthenticationProvider? g = auth.FindProvider( schemeOrProviderName );
        Throw.DebugAssert( user != null && g != null );
        using( var ctx = new SqlStandardCallContext() )
        {
            string userName = Guid.NewGuid().ToString();
            int userId = user.CreateUser( ctx, 1, userName );
            using( TestHelper.Monitor.OpenInfo( $"StandardTest for generic {schemeOrProviderName} with userId:{userId} and userName:{userName}." ) )
            {

                IUserAuthInfo? info = auth.ReadUserAuthInfo( ctx, 1, userId );
                Throw.DebugAssert( info != null );
                info.UserId.ShouldBe( userId );
                info.UserName.ShouldBe( userName );
                info.Schemes.ShouldBeEmpty();

                using( TestHelper.Monitor.OpenInfo( "CreateOrUpdateUser without login" ) )
                {
                    g.CreateOrUpdateUser( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ) ).OperationResult.ShouldBe( UCResult.Created );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.Count.ShouldBe( 0, "Still no scheme since we did not use WithActualLogin." );

                    g.LoginUser( ctx, payloadForLogin( userId, userName ), actualLogin: false ).UserId.ShouldBe( userId );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty( "Still no scheme since we challenge login but not use WithActualLogin." );

                    g.LoginUser( ctx, payloadForLogin( userId, userName ) ).UserId.ShouldBe( userId );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.Count.ShouldBe( 1 );
                    info.Schemes[0].Name.ShouldStartWith( g.ProviderName );
                    info.Schemes[0].Name.ShouldBe( schemeOrProviderName );
                    info.Schemes[0].LastUsed.ShouldBe( DateTime.UtcNow, tolerance: TimeSpan.FromSeconds( 1 ) );

                    g.DestroyUser( ctx, 1, userId );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty();
                }
                using( TestHelper.Monitor.OpenInfo( "CreateOrUpdateUser WithActualLogin" ) )
                {
                    info.UserId.ShouldBe( userId );
                    info.UserName.ShouldBe( userName );
                    info.Schemes.ShouldBeEmpty();

                    var result = g.CreateOrUpdateUser( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ), UCLMode.CreateOnly | UCLMode.WithActualLogin );
                    result.OperationResult.ShouldBe( UCResult.Created );
                    result.LoginResult.UserId.ShouldBe( userId );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.Count.ShouldBe( 1 );
                    info.Schemes[0].Name.ShouldStartWith( g.ProviderName );
                    info.Schemes[0].Name.ShouldBe( schemeOrProviderName );
                    info.Schemes[0].LastUsed.ShouldBe( DateTime.UtcNow, tolerance: TimeSpan.FromSeconds( 1 ) );

                    g.LoginUser( ctx, payloadForLoginFail( userId, userName ) ).UserId.ShouldBe( 0 );

                    g.DestroyUser( ctx, 1, userId );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty();
                }
                using( TestHelper.Monitor.OpenInfo( "Login for an unregistered user." ) )
                {
                    info.UserId.ShouldBe( userId );
                    info.UserName.ShouldBe( userName );
                    info.Schemes.ShouldBeEmpty();

                    var result = g.LoginUser( ctx, payloadForLogin( userId, userName ) );
                    result.IsSuccess.ShouldBeFalse();
                    result.UserId.ShouldBe( 0 );
                    result.FailureCode.ShouldBe( (int)KnownLoginFailureCode.UnregisteredUser );
                    result.FailureReason.ShouldBe( "Unregistered user." );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty();

                    g.DestroyUser( ctx, 1, userId );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty();
                }
                using( TestHelper.Monitor.OpenInfo( "Invalid payload MUST throw an ArgumentException." ) )
                {
                    Util.Invokable( () => g.CreateOrUpdateUser( ctx, 1, userId, DBNull.Value ) ).ShouldThrow<ArgumentException>();
                    Util.Invokable( () => g.LoginUser( ctx, DBNull.Value ) ).ShouldThrow<ArgumentException>();
                }
            }
            user.DestroyUser( ctx, 1, userId );
        }
    }

    static public async Task StandardTestForGenericAuthenticationProviderAsync( Package auth,
                                                                                string schemeOrProviderName,
                                                                                Func<int, string, object> payloadForCreateOrUpdate,
                                                                                Func<int, string, object> payloadForLogin,
                                                                                Func<int, string, object> payloadForLoginFail )
    {
        var user = SharedEngine.Map.StObjs.Obtain<Actor.UserTable>();
        IGenericAuthenticationProvider? g = auth.FindProvider( schemeOrProviderName );
        Throw.DebugAssert( user != null && g != null );
        using( var ctx = new SqlStandardCallContext() )
        {
            string userName = Guid.NewGuid().ToString();
            int userId = await user.CreateUserAsync( ctx, 1, userName );
            using( TestHelper.Monitor.OpenInfo( $"StandardTestAsync for generic {schemeOrProviderName} with userId:{userId} and userName:{userName}." ) )
            {
                IUserAuthInfo? info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                Throw.DebugAssert( info != null );
                info.UserId.ShouldBe( userId );
                info.UserName.ShouldBe( userName );
                info.Schemes.ShouldBeEmpty();

                using( TestHelper.Monitor.OpenInfo( "CreateOrUpdateUser without login." ) )
                {
                    (await g.CreateOrUpdateUserAsync( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ) )).OperationResult.ShouldBe( UCResult.Created );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty( "Still no scheme since we did not use WithLogin." );

                    (await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ), actualLogin: false )).UserId.ShouldBe( userId );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty( "Still no scheme since we challenge login but not use WithLogin." );

                    (await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ) )).UserId.ShouldBe( userId );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.Count.ShouldBe( 1 );
                    info.Schemes[0].Name.ShouldStartWith( g.ProviderName );
                    info.Schemes[0].Name.ShouldBe( schemeOrProviderName );
                    info.Schemes[0].LastUsed.ShouldBe( DateTime.UtcNow, tolerance: TimeSpan.FromSeconds( 1 ) );

                    await g.DestroyUserAsync( ctx, 1, userId );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty();
                }
                using( TestHelper.Monitor.OpenInfo( "CreateOrUpdateUser WithActualLogin." ) )
                {
                    info.UserId.ShouldBe( userId );
                    info.UserName.ShouldBe( userName );
                    info.Schemes.ShouldBeEmpty();

                    var result = await g.CreateOrUpdateUserAsync( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ), UCLMode.CreateOnly | UCLMode.WithActualLogin );
                    result.OperationResult.ShouldBe( UCResult.Created );
                    result.LoginResult.UserId.ShouldBe( userId );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.Count.ShouldBe( 1 );
                    info.Schemes[0].Name.ShouldStartWith( g.ProviderName );
                    info.Schemes[0].Name.ShouldBe( schemeOrProviderName );
                    info.Schemes[0].LastUsed.ShouldBe( DateTime.UtcNow, tolerance: TimeSpan.FromSeconds( 1 ) );

                    (await g.LoginUserAsync( ctx, payloadForLoginFail( userId, userName ) )).UserId.ShouldBe( 0 );

                    await g.DestroyUserAsync( ctx, 1, userId );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty();
                }
                using( TestHelper.Monitor.OpenInfo( "Login for an unregistered user." ) )
                {
                    info.UserId.ShouldBe( userId );
                    info.UserName.ShouldBe( userName );
                    info.Schemes.ShouldBeEmpty();

                    var result = await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ) );
                    result.IsSuccess.ShouldBeFalse();
                    result.UserId.ShouldBe( 0 );
                    result.FailureCode.ShouldBe( (int)KnownLoginFailureCode.UnregisteredUser );
                    result.FailureReason.ShouldBe( "Unregistered user." );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty();

                    await g.DestroyUserAsync( ctx, 1, userId );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Throw.DebugAssert( info != null );
                    info.Schemes.ShouldBeEmpty();
                }
                using( TestHelper.Monitor.OpenInfo( "Invalid payload MUST throw an ArgumentException." ) )
                {
                    await Util.Awaitable( () => g.CreateOrUpdateUserAsync( ctx, 1, userId, DBNull.Value ) ).ShouldThrowAsync<ArgumentException>();
                    await Util.Invokable( () => g.LoginUserAsync( ctx, DBNull.Value ) ).ShouldThrowAsync<ArgumentException>();
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
                    result.OperationResult.ShouldBe( UCResult.Created );
                    result.LoginResult.UserId.ShouldBe( 0 );
                    result.LoginResult.IsSuccess.ShouldBeFalse();
                    result.LoginResult.FailureCode.ShouldBe( (int)KnownLoginFailureCode.GloballyDisabledUser );
                    LoginResult login = await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ) );
                    login.IsSuccess.ShouldBeFalse();
                    login.UserId.ShouldBe( 0 );
                    login.FailureCode.ShouldBe( (int)KnownLoginFailureCode.GloballyDisabledUser );
                    login = await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ), actualLogin: false );
                    login.IsSuccess.ShouldBeFalse();
                    login.UserId.ShouldBe( 0 );
                    login.FailureCode.ShouldBe( (int)KnownLoginFailureCode.GloballyDisabledUser );
                }
            }
            await user.DestroyUserAsync( ctx, 1, userId );
        }
    }

    [Test]
    public void when_a_basic_provider_exists_its_IGenericAuthenticationProvider_adpater_accepts_UserId_or_UserName_based_login_payloads()
    {
        var auth = SharedEngine.Map.StObjs.Obtain<Package>();
        Throw.DebugAssert( auth != null );
        Assume.That( auth.BasicProvider != null );

        // With Tuple<UserId, Password> payload.  
        StandardTestForGenericAuthenticationProvider( auth,
                                                     "Basic",
                                                      payloadForCreateOrUpdate: ( userId, userName ) => "password",
                                                      payloadForLogin: ( userId, userName ) => Tuple.Create( userId, "password" ),
                                                      payloadForLoginFail: ( userId, userName ) => Tuple.Create( userId, "wrong password" ) );

        // With ValueTuple (UserId, Password) payload.  
        StandardTestForGenericAuthenticationProvider( auth,
                                                     "Basic",
                                                      payloadForCreateOrUpdate: ( userId, userName ) => "password",
                                                      payloadForLogin: ( userId, userName ) => (userId, "password"),
                                                      payloadForLoginFail: ( userId, userName ) => (userId, "wrong password") );

        // With Tuple<UserName, Password> payload.  
        StandardTestForGenericAuthenticationProvider( auth,
                                                      "Basic",
                                                      payloadForCreateOrUpdate: ( userId, userName ) => "password",
                                                      payloadForLogin: ( userId, userName ) => Tuple.Create( userName, "password" ),
                                                      payloadForLoginFail: ( userId, userName ) => Tuple.Create( userName, "wrong password" ) );

        // With ValueTuple (UserName, Password) payload.  
        StandardTestForGenericAuthenticationProvider( auth,
                                                      "Basic",
                                                      payloadForCreateOrUpdate: ( userId, userName ) => "password",
                                                      payloadForLogin: ( userId, userName ) => (userName, "password"),
                                                      payloadForLoginFail: ( userId, userName ) => (userName, "wrong password") );

        // With KeyValuePairs payload.  
        StandardTestForGenericAuthenticationProvider( auth,
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

        // With ValueTuples payload.  
        StandardTestForGenericAuthenticationProvider( auth,
                                                      "Basic",
                                                      payloadForCreateOrUpdate: ( userId, userName ) => "£$$µ+",
                                                      payloadForLogin: ( userId, userName ) => new[]
                                                      {
                                                          ("username", userName),
                                                          ("password", "£$$µ+")
                                                      },
                                                      payloadForLoginFail: ( userId, userName ) => new[]
                                                      {
                                                          ("username", userName),
                                                          ("password", "wrong password")
                                                      } );

        // With KeyValuePairs payload.  
        StandardTestForGenericAuthenticationProvider( auth,
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
        // With ValueTuples payload.  
        StandardTestForGenericAuthenticationProvider( auth,
                                                      "Basic",
                                                      payloadForCreateOrUpdate: ( userId, userName ) => "MM£$$µ+",
                                                      payloadForLogin: ( userId, userName ) => new ValueTuple<string, object?>[]
                                                      {
                                                          ("USERID", userId),
                                                          ("PASSWORD", "MM£$$µ+")
                                                      },
                                                      payloadForLoginFail: ( userId, userName ) => new ValueTuple<string, object?>[]
                                                      {
                                                          ("USERID", userId),
                                                          ("PASSWORD", "wrong password")
                                                      } );
    }

    [Test]
    public async Task when_a_basic_provider_exists_its_IGenericAuthenticationProvider_adpater_accepts_UserId_or_UserName_based_login_payloads_Async()
    {
        var auth = SharedEngine.Map.StObjs.Obtain<Package>();
        Throw.DebugAssert( auth != null );
        Assume.That( auth.BasicProvider != null );

        // With Tuple (UserId, Password) payload.  
        await StandardTestForGenericAuthenticationProviderAsync( auth,
                                                                 "Basic",
                                                                 payloadForCreateOrUpdate: ( userId, userName ) => "password",
                                                                 payloadForLogin: ( userId, userName ) => Tuple.Create( userId, "password" ),
                                                                 payloadForLoginFail: ( userId, userName ) => Tuple.Create( userId, "wrong password" ) );

        // With ValueTuple (UserId, Password) payload.  
        await StandardTestForGenericAuthenticationProviderAsync( auth,
                                                                 "Basic",
                                                                 payloadForCreateOrUpdate: ( userId, userName ) => "password",
                                                                 payloadForLogin: ( userId, userName ) => (userId, "password"),
                                                                 payloadForLoginFail: ( userId, userName ) => (userId, "wrong password") );

        // With Tuple (UserName, Password) payload.  
        await StandardTestForGenericAuthenticationProviderAsync( auth,
                                                                "Basic",
                                                                 payloadForCreateOrUpdate: ( userId, userName ) => "password",
                                                                 payloadForLogin: ( userId, userName ) => Tuple.Create( userName, "password" ),
                                                                 payloadForLoginFail: ( userId, userName ) => Tuple.Create( userName, "wrong password" ) );

        // With ValueTuple (UserName, Password) payload.  
        await StandardTestForGenericAuthenticationProviderAsync( auth,
                                                                "Basic",
                                                                 payloadForCreateOrUpdate: ( userId, userName ) => "password",
                                                                 payloadForLogin: ( userId, userName ) => (userName, "password"),
                                                                 payloadForLoginFail: ( userId, userName ) => (userName, "wrong password") );
    }

    /// <summary>
    /// Helper to be used by actual providers to check that they are properly registered.
    /// </summary>
    /// <param name="providerName">provider name that must be registered.</param>
    public static void CheckProviderRegistration( string providerName )
    {
        var provider = SharedEngine.Map.StObjs.Obtain<AuthProviderTable>();
        Throw.DebugAssert( provider != null );
        using( var ctx = new SqlStandardCallContext() )
        {
            provider.Database.ExecuteScalar( "select count(*) from CK.tAuthProvider where ProviderName = @0", providerName )
                .ShouldBe( 1 );
        }
    }

    [Test]
    public void reading_IUserAuthInfo_for_an_unexisting_user_or_Anonymous_returns_null()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        Throw.DebugAssert( p != null );
        using( var ctx = new SqlStandardCallContext() )
        {
            IUserAuthInfo? info = p.ReadUserAuthInfo( ctx, 1, int.MaxValue );
            info.ShouldBeNull();
            info = p.ReadUserAuthInfo( ctx, 1, 0 );
            info.ShouldBeNull();
        }
    }
}
