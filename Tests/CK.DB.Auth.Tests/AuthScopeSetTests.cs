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
    public class AuthScopeSetTests
    {
        [Test]
        public async Task creating_simple_scope_set()
        {
            var scopes = TestHelper.StObjMap.Default.Obtain<AuthScopeSetTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var id = await scopes.CreateScopeSetAsync( ctx, 1, "openid profile" );
                Assert.That( id, Is.GreaterThan( 0 ) );
                scopes.Database.AssertScalarEquals( 2, $"select count(*) from CK.tAuthScopeSetContent where ScopeSetId = {id}" );
                await scopes.DestroyScopeSetAsync( ctx, 1, id );
                scopes.Database.AssertEmptyReader( $"select * from CK.tAuthScopeSetContent where ScopeSetId = {id}" );
            }
        }

        [Test]
        public async Task adding_and_removing_scopes_via_raw_strings()
        {
            var scopes = TestHelper.StObjMap.Default.Obtain<AuthScopeSetTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var id = await scopes.CreateScopeSetAsync( ctx, 1, "   profile   openid  " );
                scopes.Database.AssertScalarEquals( "[W]openid [W]profile", $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" );

                await scopes.RemoveScopesAsync( ctx, 1, id, ScopeWARStatus.Waiting );
                scopes.Database.AssertScalarEquals( string.Empty, $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" );

                await scopes.AddScopesAsync( ctx, 1, id, "Z Y X A", true, ScopeWARStatus.Waiting );
                scopes.Database.AssertScalarEquals( "[W]A [W]X [W]Y [W]Z", $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" );

                await scopes.RemoveScopesAsync( ctx, 1, id, "A Y Z", false );
                scopes.Database.AssertScalarEquals( "[W]X", $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" );

                await scopes.SetScopesAsync( ctx, 1, id, "a b c d a b", false, ScopeWARStatus.Accepted );
                scopes.Database.AssertScalarEquals( "[A]a [A]b [A]c [A]d", $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" );

                await scopes.AddScopesAsync( ctx, 1, id, "a d z", false, ScopeWARStatus.Rejected );
                scopes.Database.AssertScalarEquals( "[R]a [A]b [A]c [R]d [R]z", $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" );

                await scopes.RemoveScopesAsync( ctx, 1, id, "a b c z", false, ScopeWARStatus.Rejected );
                scopes.Database.AssertScalarEquals( "[A]b [A]c [R]d", $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" );

                await scopes.DestroyScopeSetAsync( ctx, 1, id );
            }
        }

        [Test]
        public async Task AuthScopeSet_manipulation()
        {
            var scopes = TestHelper.StObjMap.Default.Obtain<AuthScopeSetTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var set = new AuthScopeSet() {
                    new AuthScope( "A", ScopeWARStatus.Accepted ),
                    new AuthScope( "B", ScopeWARStatus.Waiting ),
                    new AuthScope( "C", ScopeWARStatus.Rejected )
                };

                var id = await scopes.CreateScopeSetAsync( ctx, 1, set );
                scopes.Database.AssertScalarEquals( "[A]A [W]B [R]C", $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" );

                var readSet = await scopes.ReadAuthScopeSetAsync( ctx, id );
                Assert.That( readSet.ToString(), Is.EqualTo( "[A]A [W]B [R]C" ) );

                set.Add( new AuthScope( "B", ScopeWARStatus.Accepted ) );
                set.Add( new AuthScope( "D", ScopeWARStatus.Waiting ) );
                await scopes.AddScopesAsync( ctx, 1, id, set );
                readSet = await scopes.ReadAuthScopeSetAsync( ctx, id );
                Assert.That( readSet.ToString(), Is.EqualTo( "[A]A [A]B [R]C [W]D" ) );

                set.Remove( "B" );
                set.Remove( "C" );
                await scopes.AddScopesAsync( ctx, 1, id, set );
                readSet = await scopes.ReadAuthScopeSetAsync( ctx, id );
                Assert.That( readSet.ToString(), Is.EqualTo( "[A]A [A]B [R]C [W]D" ) );

                await scopes.SetScopesAsync( ctx, 1, id, set );
                readSet = await scopes.ReadAuthScopeSetAsync( ctx, id );
                Assert.That( readSet.ToString(), Is.EqualTo( "[A]A [W]D" ) );

                await scopes.DestroyScopeSetAsync( ctx, 1, id );
            }
        }


    }
}
