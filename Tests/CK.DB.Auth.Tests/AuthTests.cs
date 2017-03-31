﻿using CK.Core;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Auth.Tests
{
    [TestFixture]
    public class AuthTests
    {
        [Test]
        public void existing_providers_are_registered_in_tAuthProvider_and_available_as_IGenericAuthenticationProvider()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using (var ctx = new SqlStandardCallContext())
            {
            }
        }

        [Test]
        public void when_basic_provider_exists_it_is_registered_in_tAuthProvider_and_available_as_IGenericAuthenticationProvider()
        {
            var auth = TestHelper.StObjMap.Default.Obtain<Package>();
            Assume.That(auth.BasicProvider != null);

            Assert.That(auth.AllProviders.Single(provider => provider.ProviderName == "Basic"), Is.Not.Null);
            Assert.That(auth.FindProvider("Basic"), Is.Not.Null);
            Assert.That(auth.FindProvider("bASIC"), Is.Not.Null);
        }

        static public void StandardTestGorGenericAuthenticationProvider( 
            Package auth,
            string providerName,
            Func<int, string, object> payloadForCreateOrUpdate,
            Func<int, string, object> payloadForLogin,
            Func<int, string, object> payloadForLoginFail
            )
        {
            var user = TestHelper.StObjMap.Default.Obtain<Actor.UserTable>();
            IGenericAuthenticationProvider g = auth.FindProvider(providerName);
            using (var ctx = new SqlStandardCallContext())
            {
                string userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser(ctx, 1, userName);
                IUserAuthInfo info = auth.ReadUserAuthInfo(ctx, 1, userId);

                Assert.That(info.UserId, Is.EqualTo(userId));
                Assert.That(info.UserName, Is.EqualTo(userName));
                Assert.That(info.Providers.Count, Is.EqualTo(0));

                {
                    #region CreateOrUpdateUser without WithLogin

                    Assert.That(g.CreateOrUpdateUser(ctx, 1, userId, payloadForCreateOrUpdate(userId,userName)), Is.EqualTo(CreateOrUpdateResult.Created));
                    info = auth.ReadUserAuthInfo(ctx, 1, userId);
                    Assert.That(info.Providers.Count, Is.EqualTo(0), "Still no provider since we did not use WithLogin.");

                    Assert.That(g.LoginUser(ctx, payloadForLogin(userId, userName), actualLogin:false), Is.EqualTo(userId));
                    info = auth.ReadUserAuthInfo(ctx, 1, userId);
                    Assert.That(info.Providers.Count, Is.EqualTo(0), "Still no provider since we challenge login but not use WithLogin.");

                    Assert.That(g.LoginUser(ctx, payloadForLogin(userId, userName)), Is.EqualTo(userId));
                    info = auth.ReadUserAuthInfo(ctx, 1, userId);
                    Assert.That(info.Providers.Count, Is.EqualTo(1));
                    Assert.That(info.Providers[0].Name, Is.EqualTo(g.ProviderName));
                    Assert.That(info.Providers[0].LastUsed, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-1)));

                    g.DestroyUser(ctx, 1, userId);
                    info = auth.ReadUserAuthInfo(ctx, 1, userId);
                    Assert.That(info.Providers.Count, Is.EqualTo(0));

                    #endregion 
                }
                {
                    #region CreateOrUpdateUser WithLogin
                    Assert.That(info.UserId, Is.EqualTo(userId));
                    Assert.That(info.UserName, Is.EqualTo(userName));
                    Assert.That(info.Providers.Count, Is.EqualTo(0));

                    Assert.That(g.CreateOrUpdateUser(ctx, 1, userId, payloadForCreateOrUpdate(userId,userName), CreateOrUpdateMode.CreateOnly | CreateOrUpdateMode.WithLogin), Is.EqualTo(CreateOrUpdateResult.Created));
                    info = auth.ReadUserAuthInfo(ctx, 1, userId);
                    Assert.That(info.Providers.Count, Is.EqualTo(1));
                    Assert.That(info.Providers[0].Name, Is.EqualTo(g.ProviderName));
                    Assert.That(info.Providers[0].LastUsed, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-1)));

                    Assert.That(g.LoginUser(ctx, payloadForLoginFail(userId,userName)), Is.EqualTo(0));

                    g.DestroyUser(ctx, 1, userId);
                    info = auth.ReadUserAuthInfo(ctx, 1, userId);
                    Assert.That(info.Providers.Count, Is.EqualTo(0));

                    user.DestroyUser(ctx, 1, userId);
                    #endregion
                }
                user.DestroyUser(ctx, 1, userId);
            }
        }

        [Test]
        public void when_a_basic_provider_exists_its_IGenericAuthenticationProvider_adpater_accepts_UserId_or_UserName_based_login_payloads()
        {
            var auth = TestHelper.StObjMap.Default.Obtain<Package>();
            Assume.That( auth.BasicProvider != null );

            StandardTestGorGenericAuthenticationProvider(
                auth,
                "Basic",
                payloadForCreateOrUpdate: (userId, userName) => "password",
                payloadForLogin: (userId, userName) => Tuple.Create(userId, "password"),
                payloadForLoginFail: (userId, userName) => Tuple.Create(userId, "wrong password"));

            StandardTestGorGenericAuthenticationProvider(
                auth,
                "Basic",
                payloadForCreateOrUpdate: (userId, userName) => "password",
                payloadForLogin: (userId, userName) => Tuple.Create(userName, "password"),
                payloadForLoginFail: (userId, userName) => Tuple.Create(userName, "wrong password"));
        }

        [Test]
        public async Task creating_a_fake_provider_in_the_database_and_enable_or_disable_it()
        {
            var provider = TestHelper.StObjMap.Default.Obtain<AuthProviderTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string name = "Absolutely Nimp name";
                var id = await provider.RegisterProviderAsync( ctx, 1, "Absolutely Nimp name", "Schema.[a table name]" );
                Assert.That( id, Is.GreaterThan( 0 ) );
                provider.Database.AssertScalarEquals( true, $"select IsEnabled from CK.tAuthProvider where AuthProviderId = {id}" );
                await provider.EnableProviderAsync( ctx, 1, name, false );
                provider.Database.AssertScalarEquals( false, $"select IsEnabled from CK.tAuthProvider where AuthProviderId = {id}" );
                await provider.EnableProviderAsync( ctx, 1, name, true );
                provider.Database.AssertScalarEquals( true, $"select IsEnabled from CK.tAuthProvider where AuthProviderId = {id}" );
                provider.Database.ExecuteNonQuery( $"delete CK.tAuthProvider where AuthProviderId = {id}" );
            }
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

    }
}
