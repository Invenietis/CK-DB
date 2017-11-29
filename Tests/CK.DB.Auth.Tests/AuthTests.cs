using CK.Core;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.DB.Tests;

namespace CK.DB.Auth.Tests
{
    [TestFixture]
    public class AuthTests
    {

        [Test]
        public void calling_OnUserLogin_directlty_works_but_is_reserved_for_very_rare_scenarii()
        {
            var auth = TestHelper.StObjMap.Default.Obtain<Package>();
            var user = TestHelper.StObjMap.Default.Obtain<Actor.UserTable>();
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
                Assert.That( result.FailureCode, Is.EqualTo( 0 ) );
                Assert.That( result.FailureReason, Is.Null );
                Assert.That( result.IsSuccessful );
                Assert.That( result.UserId, Is.EqualTo( idUser ) );

                result = auth.OnUserLogin( ctx, "test-fail", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
                Assert.That( result.FailureCode, Is.EqualTo( 3712 ) );
                Assert.That( result.FailureReason, Is.EqualTo( "The failure" ) );
                Assert.That( result.IsSuccessful, Is.False );
                Assert.That( result.UserId, Is.EqualTo( 0 ) );

                result = auth.OnUserLogin( ctx, "test-fail-reason-only", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
                Assert.That( result.FailureCode, Is.EqualTo( (int)KnownLoginFailureCode.Unspecified ) );
                Assert.That( result.FailureReason, Is.EqualTo( "The failure text" ) );
                Assert.That( result.IsSuccessful, Is.False );
                Assert.That( result.UserId, Is.EqualTo( 0 ) );

                result = auth.OnUserLogin( ctx, "test-fail-reason-only-empty", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
                Assert.That( result.FailureCode, Is.EqualTo( (int)KnownLoginFailureCode.Unspecified ) );
                Assert.That( result.FailureReason, Is.EqualTo( "Unspecified reason." ) );
                Assert.That( result.IsSuccessful, Is.False );
                Assert.That( result.UserId, Is.EqualTo( 0 ) );

                result = auth.OnUserLogin( ctx, "test-fail-code-only", Util.UtcMinValue, idUser, false, DateTime.UtcNow );
                Assert.That( result.FailureCode, Is.EqualTo( 42 ) );
                Assert.That( result.FailureReason, Is.EqualTo( "Unspecified reason." ) );
                Assert.That( result.IsSuccessful, Is.False );
                Assert.That( result.UserId, Is.EqualTo( 0 ) );
            }
        }

        [Test]
        public void when_basic_provider_exists_it_is_registered_in_tAuthProvider_and_available_as_IGenericAuthenticationProvider()
        {
            var auth = TestHelper.StObjMap.Default.Obtain<Package>();
            Assume.That( auth.BasicProvider != null );

            Assert.That( auth.AllProviders.Single( provider => provider.ProviderName == "Basic" ), Is.Not.Null );
            Assert.That( auth.FindProvider( "Basic" ), Is.Not.Null );
            Assert.That( auth.FindProvider( "bASIC" ), Is.Not.Null );
        }

        static public void StandardTestForGenericAuthenticationProvider(
            Package auth,
            string schemeOrProviderName,
            Func<int, string, object> payloadForCreateOrUpdate,
            Func<int, string, object> payloadForLogin,
            Func<int, string, object> payloadForLoginFail
            )
        {
            var user = TestHelper.StObjMap.Default.Obtain<Actor.UserTable>();
            IGenericAuthenticationProvider g = auth.FindProvider( schemeOrProviderName );
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, userName );
                IUserAuthInfo info = auth.ReadUserAuthInfo( ctx, 1, userId );

                Assert.That( info.UserId, Is.EqualTo( userId ) );
                Assert.That( info.UserName, Is.EqualTo( userName ) );
                Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                {
                    #region CreateOrUpdateUser without login

                    Assert.That( g.CreateOrUpdateUser( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ) ).OperationResult, Is.EqualTo( UCResult.Created ) );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ), "Still no scheme since we did not use WithActualLogin." );

                    Assert.That( g.LoginUser( ctx, payloadForLogin( userId, userName ), actualLogin: false ).UserId, Is.EqualTo( userId ) );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ), "Still no scheme since we challenge login but not use WithActualLogin." );

                    Assert.That( g.LoginUser( ctx, payloadForLogin( userId, userName ) ).UserId, Is.EqualTo( userId ) );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 1 ) );
                    Assert.That( info.Schemes[0].Name, Does.StartWith( g.ProviderName ) );
                    Assert.That( info.Schemes[0].Name, Is.EqualTo( schemeOrProviderName ).IgnoreCase );
                    Assert.That( info.Schemes[0].LastUsed, Is.GreaterThan( DateTime.UtcNow.AddSeconds( -1 ) ) );

                    g.DestroyUser( ctx, 1, userId );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    #endregion 
                }
                {
                    #region CreateOrUpdateUser WithActualLogin
                    Assert.That( info.UserId, Is.EqualTo( userId ) );
                    Assert.That( info.UserName, Is.EqualTo( userName ) );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    var result = g.CreateOrUpdateUser( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ), UCLMode.CreateOnly | UCLMode.WithActualLogin );
                    Assert.That( result.OperationResult, Is.EqualTo( UCResult.Created ) );
                    Assert.That( result.LoginResult.UserId, Is.EqualTo( userId ) );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 1 ) );
                    Assert.That( info.Schemes[0].Name, Does.StartWith( g.ProviderName ) );
                    Assert.That( info.Schemes[0].Name, Is.EqualTo( schemeOrProviderName ).IgnoreCase );
                    Assert.That( info.Schemes[0].LastUsed, Is.GreaterThan( DateTime.UtcNow.AddSeconds( -1 ) ) );

                    Assert.That( g.LoginUser( ctx, payloadForLoginFail( userId, userName ) ).UserId, Is.EqualTo( 0 ) );

                    g.DestroyUser( ctx, 1, userId );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    #endregion
                }
                {
                    #region Login for an unregistered user.
                    Assert.That( info.UserId, Is.EqualTo( userId ) );
                    Assert.That( info.UserName, Is.EqualTo( userName ) );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    var result = g.LoginUser( ctx, payloadForLogin( userId, userName ) );
                    Assert.That( result.IsSuccessful, Is.False );
                    Assert.That( result.UserId, Is.EqualTo( 0 ) );
                    Assert.That( result.FailureCode, Is.EqualTo( (int)KnownLoginFailureCode.UnregisteredUser ) );
                    Assert.That( result.FailureReason, Is.EqualTo( "Unregistered user." ) );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    g.DestroyUser( ctx, 1, userId );
                    info = auth.ReadUserAuthInfo( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    #endregion
                }
                {
                    #region Invalid payload MUST throw an ArgumentException

                    Assert.Throws<ArgumentException>( () => g.CreateOrUpdateUser( ctx, 1, userId, DBNull.Value ) );
                    Assert.Throws<ArgumentException>( () => g.LoginUser( ctx, DBNull.Value ) );

                    #endregion
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
            var user = TestHelper.StObjMap.Default.Obtain<Actor.UserTable>();
            IGenericAuthenticationProvider g = auth.FindProvider( schemeOrProviderName );
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = Guid.NewGuid().ToString();
                int userId = await user.CreateUserAsync( ctx, 1, userName );
                IUserAuthInfo info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );

                Assert.That( info.UserId, Is.EqualTo( userId ) );
                Assert.That( info.UserName, Is.EqualTo( userName ) );
                Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                {
                    #region CreateOrUpdateUser without login

                    Assert.That( (await g.CreateOrUpdateUserAsync( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ) )).OperationResult, Is.EqualTo( UCResult.Created ) );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ), "Still no scheme since we did not use WithLogin." );

                    Assert.That( (await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ), actualLogin: false )).UserId, Is.EqualTo( userId ) );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ), "Still no scheme since we challenge login but not use WithLogin." );

                    Assert.That( (await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ) )).UserId, Is.EqualTo( userId ) );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 1 ) );
                    Assert.That( info.Schemes[0].Name, Does.StartWith( g.ProviderName ) );
                    Assert.That( info.Schemes[0].Name, Is.EqualTo( schemeOrProviderName ).IgnoreCase );
                    Assert.That( info.Schemes[0].LastUsed, Is.GreaterThan( DateTime.UtcNow.AddSeconds( -1 ) ) );

                    await g.DestroyUserAsync( ctx, 1, userId );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    #endregion 
                }
                {
                    #region CreateOrUpdateUser WithActualLogin
                    Assert.That( info.UserId, Is.EqualTo( userId ) );
                    Assert.That( info.UserName, Is.EqualTo( userName ) );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    var result = await g.CreateOrUpdateUserAsync( ctx, 1, userId, payloadForCreateOrUpdate( userId, userName ), UCLMode.CreateOnly | UCLMode.WithActualLogin );
                    Assert.That( result.OperationResult, Is.EqualTo( UCResult.Created ) );
                    Assert.That( result.LoginResult.UserId, Is.EqualTo( userId ) );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 1 ) );
                    Assert.That( info.Schemes[0].Name, Does.StartWith( g.ProviderName ) );
                    Assert.That( info.Schemes[0].Name, Is.EqualTo( schemeOrProviderName ).IgnoreCase );
                    Assert.That( info.Schemes[0].LastUsed, Is.GreaterThan( DateTime.UtcNow.AddSeconds( -1 ) ) );

                    Assert.That( (await g.LoginUserAsync( ctx, payloadForLoginFail( userId, userName ) )).UserId, Is.EqualTo( 0 ) );

                    await g.DestroyUserAsync( ctx, 1, userId );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    #endregion
                }
                {
                    #region Login for an unregistered user.
                    Assert.That( info.UserId, Is.EqualTo( userId ) );
                    Assert.That( info.UserName, Is.EqualTo( userName ) );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    var result = await g.LoginUserAsync( ctx, payloadForLogin( userId, userName ) );
                    Assert.That( result.IsSuccessful, Is.False );
                    Assert.That( result.UserId, Is.EqualTo( 0 ) );
                    Assert.That( result.FailureCode, Is.EqualTo( (int)KnownLoginFailureCode.UnregisteredUser ) );
                    Assert.That( result.FailureReason, Is.EqualTo( "Unregistered user." ) );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    await g.DestroyUserAsync( ctx, 1, userId );
                    info = await auth.ReadUserAuthInfoAsync( ctx, 1, userId );
                    Assert.That( info.Schemes.Count, Is.EqualTo( 0 ) );

                    #endregion
                }
                {
                    #region Invalid payload MUST throw an ArgumentException

                    Assert.Throws<ArgumentException>( async () => await g.CreateOrUpdateUserAsync( ctx, 1, userId, DBNull.Value ) );
                    Assert.Throws<ArgumentException>( async () => await g.LoginUserAsync( ctx, DBNull.Value ) );

                    #endregion
                }
                await user.DestroyUserAsync( ctx, 1, userId );
            }
        }

        [Test]
        public void when_a_basic_provider_exists_its_IGenericAuthenticationProvider_adpater_accepts_UserId_or_UserName_based_login_payloads()
        {
            var auth = TestHelper.StObjMap.Default.Obtain<Package>();
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
            var auth = TestHelper.StObjMap.Default.Obtain<Package>();
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
            var provider = TestHelper.StObjMap.Default.Obtain<AuthProviderTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                provider.Database.AssertScalarEquals( 1, "select count(*) from CK.tAuthProvider where ProviderName = @0", providerName );
            }
        }

        [Test]
        public void reading_IUserAuthInfo_for_an_unexisting_user_or_Anonymous_returns_null()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                IUserAuthInfo info = p.ReadUserAuthInfo( ctx, 1, int.MaxValue );
                Assert.That( info, Is.Null );
                info = p.ReadUserAuthInfo( ctx, 1, 0 );
                Assert.That( info, Is.Null );
            }
        }
    }
}
