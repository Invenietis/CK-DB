using CK.Core;
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
        public void calling_vUserAuthProvider_always_work_but_returns_data_only_when_actual_auth_providers_are_installed()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                p.Database.RawExecute( "select * from CK.vUserAuthProvider" );
            }
        }

        [Test]
        public async Task creating_a_fake_provider_and_enable_or_disable_it()
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
